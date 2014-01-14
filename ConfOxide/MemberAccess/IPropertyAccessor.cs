using System.Reflection;
using Newtonsoft.Json.Linq;

namespace ConfOxide.MemberAccess {
	///<summary>Describes and manipulates a specific property in a settings class.</summary>
	///<typeparam name="T">The type that owns the property.</typeparam>
	public interface IPropertyAccessor<in T> {
		///<summary>Gets the property being accessed.</summary>
		PropertyInfo Property { get; }
		///<summary>Gets the name to use when serializing this property to JSON.</summary>
		string JsonName { get; }

		///<summary>Initializes the property, preparing it to hold actual data.  This is only called when the instance is first constructed.</summary>
		void InitializeValue(T instance);

		///<summary>Compares the values of the property from two owning objects.</summary>
		bool CompareValues(T x, T y);

		///<summary>Copies the value of this property from one owning object to another.</summary>
		///<param name="from">The instance to read the value from.</param>
		///<param name="to">The instance to write the value to.</param>
		void Copy(T from, T to);

		///<summary>Reads this property's value from a JSON token into an instance.</summary>
		void FromJson(T instance, JToken token);

		///<summary>Resets the value of this property on an owning object to its default value.</summary>
		void ResetValue(T instance);

		///<summary>Updates the value of a JProperty to reflect this property's value.</summary>
		void UpdateJsonProperty(T instance, JProperty jsonProperty);
	}
	///<summary>A strongly-typed <see cref="IPropertyAccessor{TOwner}"/> that exposes the property's value.</summary>
	public interface ITypedPropertyAccessor<in TOwner, out TProperty> : IPropertyAccessor<TOwner> {
		///<summary>Gets the value of this property from the specified instance.</summary>
		TProperty GetValue(TOwner instance);
	}
}