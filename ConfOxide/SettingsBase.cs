using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConfOxide.MemberAccess;

namespace ConfOxide {
	///<summary>A base class for a strongly-typed settings class.</summary>
	///<typeparam name="T">The concrete type of the derived class (using the CRTP).  This allows fast and type-safe lookup of compiled property accessors.</typeparam>
	///<remarks>
	/// Concrete settings classes must be sealed.
	/// To share logic between different settings
	/// classes, create an abstract class.  These
	/// classes must maintain the type parameter,
	/// so that the concrete class will still use
	/// the CRTP.
	/// 
	/// This is necessary to allow member lookups
	/// using the type system.
	///</remarks>
	public abstract class SettingsBase<T> where T : SettingsBase<T> {
		///<summary>Initializes a new SettingsBase instance.  If the derived class has invalid properties, this will throw an exception.</summary>
		protected SettingsBase() {
			// Avoid GetType() calls.  The Sealed check in TypeAccessor takes care of this.
			//if (GetType() != typeof(T))
			//    throw new InvalidOperationException("Settings class " + GetType() + " must inherit SettingsBase<" + GetType().Name + ">.");
			if (!String.IsNullOrEmpty(TypeAccessor<T>.Error))
				throw new InvalidOperationException("Settings class " + typeof(T) + " has invalid properties:\n" + TypeAccessor<T>.Error);

			// The compiler cannot know that this class is T, so
			// I need to cast it.
			var t = (T)this;
			foreach (var prop in TypeAccessor<T>.Properties) {
				prop.InitializeValue(t);
			}
			t.ResetValues();
		}
	}
}
