using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ConfOxide.MemberAccess {
	///<summary>Holds statically cached metadata and compiled property accessors for any type.</summary>
	///<typeparam name="T">The type that owns the properties.</typeparam>
	public static class TypeAccessor<T> {
		///<summary>Gets all properties on this type that are supported settings properties.</summary>
		public static ReadOnlyCollection<IPropertyAccessor<T>> Properties { get; private set; }
		private static readonly Dictionary<string, IPropertyAccessor<T>> jsonKeyMap;

		///<summary>Gets the property with the specified JSON serialization name, if any.</summary>
		public static bool TryGetJsonProperty(string jsonName, out IPropertyAccessor<T> value) {
			return jsonKeyMap.TryGetValue(jsonName, out value);
		}

		///<summary>Creates a new instance of <typeparamref name="T"/>.</summary>
		public static readonly Func<T> CreateInstance = Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();

		///<summary>Gets an error message describing properties of this type that are not recognized as settings properties, or null if the type has no errors.</summary>
		public static string Error { get; private set; }

		static TypeAccessor() {
			var errors = new List<string>();
			if (!typeof(T).IsSealed)
				errors.Add("Concrete settings class must be sealed.");

			var propInfos = typeof(T).GetProperties();
			var props = new List<IPropertyAccessor<T>>(propInfos.Length);
			foreach (var prop in propInfos) {
				var elementType = prop.PropertyType.GetListElementType();
				if (elementType != null)
					props.Add((IPropertyAccessor<T>)Activator.CreateInstance(
						typeof(CollectionPropertyAccessor<,,>).MakeGenericType(typeof(T), prop.PropertyType, elementType),
						prop
					));
				else if (prop.PropertyType.IsSettingsType())
					props.Add((IPropertyAccessor<T>)Activator.CreateInstance(
						typeof(NestedPropertyAccessor<,>).MakeGenericType(typeof(T), prop.PropertyType),
						prop
					));
				else if (prop.PropertyType.IsScalarType())
					props.Add((IPropertyAccessor<T>)Activator.CreateInstance(
						typeof(ValuePropertyAccessor<,>).MakeGenericType(typeof(T), prop.PropertyType),
						prop
					));
				else
					errors.Add("Property " + prop.Name + " is of unrecognized type " + prop.PropertyType);
			}
			Properties = new ReadOnlyCollection<IPropertyAccessor<T>>(props);
			jsonKeyMap = Properties.ToDictionary(p => p.JsonName);
			if (errors.Any())
				Error = String.Join("\n", errors);
		}
	}
	///<summary>A collection of strongly-typed property accessors, indexed by property name.</summary>
	///<typeparam name="T">The type that owns the properties.</typeparam>
	public class PropertyCollection<T> : KeyedCollection<string, IPropertyAccessor<T>> {
		///<summary>Gets the dictionary key for the specified item.</summary>
		protected override string GetKeyForItem(IPropertyAccessor<T> item) { return item.Property.Name; }

		///<summary>Gets the value of the specified key, if present.</summary>
		public bool TryGetValue(string key, out IPropertyAccessor<T> value) {
			if (Dictionary != null)
				return Dictionary.TryGetValue(key, out value);
			value = this.FirstOrDefault(v => v.Property.Name == key);
			return value != null;
		}
	}
	///<summary>A collection of strongly-typed property accessors, indexed by property name.</summary>
	///<typeparam name="T">The type that owns the properties.</typeparam>
	public class ReadOnlyPropertyCollection<T> : ReadOnlyCollection<IPropertyAccessor<T>> {
		private readonly PropertyCollection<T> inner;
		///<summary>Creates a <see cref="ReadOnlyPropertyCollection{T}"/> wrapping a <see cref="PropertyCollection{T}"/>.</summary>
		public ReadOnlyPropertyCollection(PropertyCollection<T> inner) : base(inner) { this.inner = inner; }

		///<summary>Gets the value of the specified key, if present.</summary>
		public bool TryGetValue(string key, out IPropertyAccessor<T> value) {
			return inner.TryGetValue(key, out value);
		}

		///<summary>Gets the property with the specified key.</summary>
		public IPropertyAccessor<T> this[string key] { get { return inner[key]; } }
	}
}
