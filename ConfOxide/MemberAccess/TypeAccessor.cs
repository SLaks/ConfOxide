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
						typeof(NestedPropertyAccessor<,>).MakeGenericType(typeof(T), prop.PropertyType, elementType),
						prop
					));
				else if (prop.PropertyType.IsSubclassOf(typeof(SettingsBase<>).MakeGenericType(prop.PropertyType)))
					props.Add((IPropertyAccessor<T>)Activator.CreateInstance(
						typeof(NestedPropertyAccessor<,>).MakeGenericType(typeof(T), prop.PropertyType),
						prop
					));
				else if (prop.PropertyType.IsScalarType())
					props.Add(new ValuePropertyAccessor<T>(prop));
				else
					errors.Add("Property " + prop.Name + " is of unrecognized type " + prop.PropertyType);
			}
			Properties = new ReadOnlyCollection<IPropertyAccessor<T>>(props);
			if (errors.Any())
				Error = String.Join("\n", errors);
		}
	}
	///<summary>A collection of strongly-typed property accessors, indexed by property name.</summary>
	///<typeparam name="T">The type that owns the properties.</typeparam>
	public class PropertyCollection<T> : KeyedCollection<string, ValuePropertyAccessor<T>> {
		///<summary>Gets the dictionary key for the specified item.</summary>
		protected override string GetKeyForItem(ValuePropertyAccessor<T> item) { return item.Property.Name; }
	}
}
