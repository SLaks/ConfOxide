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
	public class ValuePropertyAccessor<TOwner, TProperty> : TypedPropertyAccessor<TOwner, TProperty>, IPropertyAccessor<TOwner> {
		private readonly Action<TOwner, TProperty> setter;

		private readonly TProperty defaultValue;

		private readonly Func<TOwner, TOwner, bool> valueComparer;
		internal ValuePropertyAccessor(PropertyInfo property) : base(property) {
			setter = (Action<TOwner, TProperty>)Delegate.CreateDelegate(typeof(Action<TOwner, TProperty>), property.GetSetMethod());

			var param = Expression.Parameter(typeof(TOwner));
			var paramValue = Expression.Property(param, property);

			var param2 = Expression.Parameter(typeof(TOwner));
			valueComparer = Expression.Lambda<Func<TOwner, TOwner, bool>>(
				Expression.Equal(paramValue, Expression.Property(param2, property)),
				param, param2
			).Compile();

			var defaultAttr = property.GetCustomAttributes(typeof(DefaultValueAttribute), true).FirstOrDefault() as DefaultValueAttribute;
			if (defaultAttr != null)
				defaultValue = (TProperty)Convert.ChangeType(defaultAttr.Value, typeof(TProperty), CultureInfo.InvariantCulture);
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
		public bool CompareValues(TOwner x, TOwner y) { return valueComparer(x, y); }

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
