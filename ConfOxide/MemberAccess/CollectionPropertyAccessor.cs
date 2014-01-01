using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ConfOxide.MemberAccess {
	///<summary>A PropertyAccessor for a property that holds a generic collection of a supported type.</summary>
	public class CollectionPropertyAccessor<TOwner, TValue, TCollection> : IPropertyAccessor<TOwner> where TCollection : IList<TValue> {
		private readonly Func<TOwner, TCollection> valueAccessor;

		private readonly Action<TOwner> initializer;

		internal CollectionPropertyAccessor(PropertyInfo property) {
			Property = property;

			var param = Expression.Parameter(typeof(TOwner));

			valueAccessor = (Func<TOwner, TCollection>)Delegate.CreateDelegate(typeof(Func<TOwner, TCollection>), property.GetGetMethod());

			Expression creator;
			if (typeof(TCollection).IsInterface)
				creator = Expression.New(typeof(List<>).MakeGenericType(typeof(TValue)));
			else
				creator = Expression.New(typeof(TCollection));

			initializer = Expression.Lambda<Action<TOwner>>(
				Expression.Call(param, property.GetSetMethod(), creator),
				param
			).Compile();
		}

		///<summary>Gets the property being accessed.</summary>
		public PropertyInfo Property { get; private set; }

		///<summary>Initializes this property to a new collection.  This is only called when the instance is first constructed.</summary>
		public void InitializeValue(TOwner instance) { initializer(instance); }

		///<summary>Copies the value of this property from one owning object to another.</summary>
		///<param name="from">The instance to read the value from.</param>
		///<param name="to">The instance to write the value to.</param>
		public void Copy(TOwner from, TOwner to) {
			var dest = valueAccessor(to);
			dest.Clear();
			foreach (var item in valueAccessor(from))
				dest.Add(item);
		}

		///<summary>Resets the value of this property on an owning object to its default value.</summary>
		public void ResetValue(TOwner instance) {
			valueAccessor(instance).Clear();
		}

		///<summary>Compares the values of the property from two owning objects.</summary>
		public bool CompareValues(TOwner x, TOwner y) {
			return valueAccessor(x).SequenceEqual(valueAccessor(y));
		}

		///<summary>Reads this property's value from a JSON token into an instance.</summary>
		public void FromJson(TOwner instance, JToken token) {
			var dest = valueAccessor(instance);
			dest.Clear();
			var jsonArray = (JArray)token;
			foreach (var item in jsonArray) {
				//TODO: Shared code to cast each token
			}
		}

		///<summary>Updates the value of a JProperty to reflect this property's value.</summary>
		public void UpdateJsonProperty(TOwner instance, JProperty jsonProperty) {
			var jsonArray = jsonProperty.Value as JArray;
			if (jsonArray == null)
				jsonProperty.Value = jsonArray = new JArray();
			jsonArray.Clear();
			foreach (var item in valueAccessor(instance)) {
				//TODO: Convert scalar values to tokens
			}
		}
	}
}
