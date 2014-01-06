using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ConfOxide.MemberAccess {
	///<summary>A PropertyAccessor for properties that hold nested <see cref="SettingsBase{T}"/> classes.</summary>
	public class NestedPropertyAccessor<TOwner, TProperty> :
		TypedPropertyAccessor<TOwner, TProperty>,
		IPropertyAccessor<TOwner>
		where TProperty : SettingsBase<TProperty> {
		private readonly Action<TOwner> initializer;

		///<summary>Creates a <see cref="NestedPropertyAccessor{TOwner, TProperty}"/> for the specified property.</summary>
		public NestedPropertyAccessor(PropertyInfo property) : base(property) {
			var param = Expression.Parameter(typeof(TOwner));
			initializer = Expression.Lambda<Action<TOwner>>(
				Expression.Call(param, property.GetSetMethod(), Expression.New(typeof(TProperty))),
				param
			).Compile();
		}

		///<summary>Initializes this property to a new collection.  This is only called when the instance is first constructed.</summary>
		public void InitializeValue(TOwner instance) { initializer(instance); }

		///<summary>Compares the values of the property from two owning objects.</summary>
		public bool CompareValues(TOwner x, TOwner y) { return GetValue(x).EquivalentTo(GetValue(y)); }

		///<summary>Copies the value of this property from one owning object to another.</summary>
		///<param name="from">The instance to read the value from.</param>
		///<param name="to">The instance to write the value to.</param>
		public void Copy(TOwner from, TOwner to) { GetValue(to).AssignFrom(GetValue(to)); }

		///<summary>Resets the value of this property on an owning object to its default value.</summary>
		public void ResetValue(TOwner instance) { GetValue(instance).ResetValues(); }

		///<summary>Reads this property's value from a JSON token into an instance.</summary>
		public void FromJson(TOwner instance, JToken token) {
			GetValue(instance).ReadJson((JObject)token);
		}

		///<summary>Updates the value of a JProperty to reflect this property's value.</summary>
		public void UpdateJsonProperty(TOwner instance, JProperty jsonProperty) {
			var jobj = jsonProperty.Value as JObject;
			if (jobj == null)
				jsonProperty.Value = GetValue(instance).ToJson();
			else
				GetValue(instance).UpdateJson(jobj);
		}
	}
}
