using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ConfOxide.MemberAccess {

	///<summary>A PropertyAccessor for a scalar property.</summary>
	public class ValuePropertyAccessor<TOwner, TProperty> : TypedPropertyAccessor<TOwner, TProperty>, ITypedPropertyAccessor<TOwner, TProperty> {
		private readonly Action<TOwner, TProperty> setter;
		private readonly TProperty defaultValue;

		///<summary>Creates a <see cref="ValuePropertyAccessor{TOwner, TProperty}"/> for the specified property.</summary>
		public ValuePropertyAccessor(PropertyInfo property) : base(property) {
			setter = (Action<TOwner, TProperty>)Delegate.CreateDelegate(typeof(Action<TOwner, TProperty>), property.GetSetMethod());

			var defaultAttr = property.GetCustomAttribute<DefaultValueAttribute>();
			if (defaultAttr != null) {
				if (defaultAttr.Value != null)
					defaultValue = ScalarType<TProperty>.FromObject(defaultAttr.Value);
				else if (typeof(TProperty).IsValueType && Nullable.GetUnderlyingType(typeof(TProperty)) == null)
					throw new InvalidOperationException("Property " + property.DeclaringType.Name + "." + property.Name + " is not nullable and cannot default to null");
				// If the default value is null, we don't need to do anything; default(TProperty) is already null.
			}
		}

		///<summary>Copies the value of this property from one owning object to another.</summary>
		///<param name="from">The instance to read the value from.</param>
		///<param name="to">The instance to write the value to.</param>
		public void Copy(TOwner from, TOwner to) {
			setter(to, GetValue(from));
		}

		///<summary>Resets the value of this property on an owning object to its default value.</summary>
		public void ResetValue(TOwner instance) { setter(instance, defaultValue); }

		///<summary>Compares the values of the property from two owning objects.</summary>
		public bool CompareValues(TOwner x, TOwner y) {
			return EqualityComparer<TProperty>.Default.Equals(GetValue(x), GetValue(y));
		}

		///<summary>Reads this property's value from a JSON token into an instance.</summary>
		public void FromJson(TOwner instance, JToken token) {
			setter(instance, ScalarType<TProperty>.FromJson((JValue)token));
		}

		///<summary>Updates the value of a JProperty to reflect this property's value.</summary>
		public void UpdateJsonProperty(TOwner instance, JProperty jsonProperty) {
			jsonProperty.Value = ScalarType<TProperty>.ToJson(GetValue(instance));
		}

		///<summary>Does nothing; value properties do not need initialization.</summary>
		public void InitializeValue(TOwner instance) { }
	}
}
