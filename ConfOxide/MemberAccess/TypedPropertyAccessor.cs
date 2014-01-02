using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ConfOxide.MemberAccess {
	///<summary>A base class for a strongly-typed property accessor.</summary>
	public abstract class TypedPropertyAccessor<TOwner, TProperty> {
		private readonly Func<TOwner, TProperty> getter;

		///<summary>Creates a TypedPropertyAccessor for the specified property.</summary>
		protected TypedPropertyAccessor(PropertyInfo property) {
			Property = property;
			getter = (Func<TOwner, TProperty>)Delegate.CreateDelegate(typeof(Func<TOwner, TProperty>), property.GetGetMethod());
		}

		///<summary>Gets the property being accessed.</summary>
		public PropertyInfo Property { get; private set; }

		///<summary>Gets the value of this property from the specified instance.</summary>
		public TProperty GetValue(TOwner instance) { return getter(instance); }
	}
}
