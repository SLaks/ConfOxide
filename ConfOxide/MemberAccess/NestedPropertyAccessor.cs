using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ConfOxide.MemberAccess {
	///<summary>A PropertyAccessor for properties that hold nested <see cref="SettingsBase{T}"/> classes.</summary>
	public class NestedPropertyAccessor<TOwner, TProperty> : IPropertyAccessor<TOwner> where TProperty : SettingsBase<TProperty> {
		private readonly Action<TOwner> initializer;
		private readonly Func<TOwner, TProperty> valueAccessor;

		internal NestedPropertyAccessor(PropertyInfo property) {
			Property = property;

			valueAccessor = (Func<TOwner, TProperty>)Delegate.CreateDelegate(typeof(Func<TOwner, TProperty>), property.GetGetMethod());

			var param = Expression.Parameter(typeof(TOwner));
			initializer = Expression.Lambda<Action<TOwner>>(
				Expression.Call(param, property.GetSetMethod(), Expression.New(typeof(TProperty))),
				param
			).Compile();

		}

		///<summary>Gets the property being accessed.</summary>
		public PropertyInfo Property { get; private set; }

		///<summary>Initializes this property to a new collection.  This is only called when the instance is first constructed.</summary>
		public void InitializeValue(TOwner instance) { initializer(instance); }

		///<summary>Compares the values of the property from two owning objects.</summary>
		public bool CompareValues(TOwner x, TOwner y) { return valueAccessor(x).EquivalentTo(valueAccessor(y)); }

		///<summary>Copies the value of this property from one owning object to another.</summary>
		///<param name="from">The instance to read the value from.</param>
		///<param name="to">The instance to write the value to.</param>
		public void Copy(TOwner from, TOwner to) { valueAccessor(to).AssignFrom(valueAccessor(to)); }

		///<summary>Resets the value of this property on an owning object to its default value.</summary>
		public void ResetValue(TOwner instance) { valueAccessor(instance).ResetValues(); }

		///<summary>Reads this property's value from a JSON token into an instance.</summary>
		public void FromJson(TOwner instance, JToken token) { }

		///<summary>Updates the value of a JProperty to reflect this property's value.</summary>
		public void UpdateJsonProperty(TOwner instance, JProperty jsonProperty) { }
	}
}
