using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace ConfOxide.MemberAccess {
	///<summary>A base class for a strongly-typed property accessor.</summary>
	public abstract class TypedPropertyAccessor<TOwner, TProperty> {
		private readonly Func<TOwner, TProperty> getter;

		///<summary>Creates a TypedPropertyAccessor for the specified property.</summary>
		protected TypedPropertyAccessor(PropertyInfo property) {
			Property = property;
			getter = (Func<TOwner, TProperty>)Delegate.CreateDelegate(typeof(Func<TOwner, TProperty>), property.GetGetMethod());

			JsonName = GetJsonName(property) ?? property.Name;
		}

		static string GetJsonName(PropertyInfo property) {
			var jsonProperty = property.GetCustomAttribute<JsonPropertyAttribute>();
			if (jsonProperty != null && !string.IsNullOrEmpty(jsonProperty.PropertyName))
				return jsonProperty.PropertyName;

			var dataMember = property.GetCustomAttribute<DataMemberAttribute>();
			if (dataMember != null && !string.IsNullOrEmpty(dataMember.Name))
				return dataMember.Name;

			return null;
		}

		///<summary>Gets the property being accessed.</summary>
		public PropertyInfo Property { get; private set; }

		///<summary>Gets the name to use when serializing this property to JSON.</summary>
		public string JsonName { get; private set; }

		///<summary>Gets the value of this property from the specified instance.</summary>
		public TProperty GetValue(TOwner instance) { return getter(instance); }
	}
}
