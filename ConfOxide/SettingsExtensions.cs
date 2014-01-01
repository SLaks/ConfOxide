using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConfOxide.MemberAccess;

namespace ConfOxide {
	///<summary>Contains basic pre-compiled operations for settings classes.</summary>
	///<remarks>
	/// These are implemented as extension methods so that
	/// they get a reference of type T rather than <see cref="SettingsBase{T}"/>.
	///</remarks>
	public static class SettingsExtensions {
		///<summary>Checks whether two <see cref="SettingsBase{T}"/> instances hold the same values.</summary>
		public static bool EquivalentTo<T>(this T first, T second) where T : SettingsBase<T> {
			return TypeAccessor<T>.Properties.All(p => p.CompareValues(first, second));
		}
		///<summary>Resets all settings on a <see cref="SettingsBase{T}"/> instance to their default values.</summary>
		public static void ResetValues<T>(this T instance) where T : SettingsBase<T> {
			foreach (var property in TypeAccessor<T>.Properties)
				property.ResetValue(instance);
		}
		///<summary>Copies all settings from one <see cref="SettingsBase{T}"/> instance to another.</summary>
		///<param name="target">The instance to write the values to.</param>
		///<param name="source">The instance to read the values from.</param>
		public static void AssignFrom<T>(this T target, T source) where T : SettingsBase<T> {
			foreach (var property in TypeAccessor<T>.Properties)
				property.Copy(source, target);
		}
	}
}
