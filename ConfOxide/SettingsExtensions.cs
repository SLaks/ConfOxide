using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConfOxide.MemberAccess;
using Newtonsoft.Json.Linq;

namespace ConfOxide {
	///<summary>Contains basic pre-compiled operations for settings classes.</summary>
	///<remarks>
	/// These are implemented as extension methods so that
	/// they get a reference of type T rather than <see cref="SettingsBase{T}"/>.
	///</remarks>
	public static class SettingsExtensions {
		///<summary>Checks whether two <see cref="SettingsBase{T}"/> instances hold the same values.</summary>
		public static bool IsEquivalentTo<T>(this T first, T second) where T : SettingsBase<T> {
			return TypeAccessor<T>.Properties.All(p => p.CompareValues(first, second));
		}
		///<summary>Resets all settings on a <see cref="SettingsBase{T}"/> instance to their default values.</summary>
		///<returns>The <paramref name="instance"/>, for chaining.</returns>
		public static T ResetValues<T>(this T instance) where T : SettingsBase<T> {
			foreach (var property in TypeAccessor<T>.Properties)
				property.ResetValue(instance);
			instance.ResetCustom();
			return instance;
		}
		///<summary>Copies all settings from one <see cref="SettingsBase{T}"/> instance to another.</summary>
		///<param name="target">The instance to write the values to.</param>
		///<param name="source">The instance to read the values from.</param>
		///<returns>The <paramref name="target"/> instance, for chaining.</returns>
		public static T AssignFrom<T>(this T target, T source) where T : SettingsBase<T> {
			foreach (var property in TypeAccessor<T>.Properties)
				property.Copy(source, target);
			return target;
		}

		///<summary>Creates a deep copy of a <see cref="SettingsBase{T}"/> object, holding the same values as the original.</summary>
		public static T CreateCopy<T>(this T source) where T : SettingsBase<T> {
			return TypeAccessor<T>.CreateInstance().AssignFrom(source);
		}

		///<summary>Updates a <see cref="SettingsBase{T}"/> instance from a JSON file.</summary>
		///<returns>The <paramref name="target"/> instance, for chaining.</returns>
		public static T ReadJson<T>(this T target, JObject json) where T : SettingsBase<T> {
			if (target == null) throw new ArgumentNullException("target");
			if (json == null) throw new ArgumentNullException("json");

			foreach (var sourceProperty in json.Properties()) {
				IPropertyAccessor<T> targetProperty;
				if (!TypeAccessor<T>.TryGetJsonProperty(sourceProperty.Name, out targetProperty))
					continue;   // Skip extra properties
				targetProperty.FromJson(target, sourceProperty.Value);
			}
			return target;
		}

		///<summary>Updates an existing JSON document from a <see cref="SettingsBase{T}"/> instance.</summary>
		///<returns>The <paramref name="source"/> instance, for chaining.</returns>
		///<remarks>The order of any existing properties in the JSON objects will be preserved.</remarks>
		public static T UpdateJson<T>(this T source, JObject json) where T : SettingsBase<T> {
			if (source == null) throw new ArgumentNullException("source");
			if (json == null) throw new ArgumentNullException("json");

			var missingProperties = new HashSet<IPropertyAccessor<T>>(TypeAccessor<T>.Properties);

			foreach (var jsonProperty in json.Properties()) {
				IPropertyAccessor<T> sourceProperty;
				if (!TypeAccessor<T>.TryGetJsonProperty(jsonProperty.Name, out sourceProperty))
					continue;   // Ignore extra properties
				sourceProperty.UpdateJsonProperty(source, jsonProperty);
				missingProperties.Remove(sourceProperty);
			}
			foreach (var sourceProperty in missingProperties.OrderBy(p => p.JsonName)) {
				var jsonProperty = new JProperty(sourceProperty.JsonName, null);
				sourceProperty.UpdateJsonProperty(source, jsonProperty);
				json.Add(jsonProperty);
			}
			return source;
		}

		///<summary>Serializes a <see cref="SettingsBase{T}"/> to a new JSON document.</summary>
		/// <remarks>The properties in the JSON will be sorted alphabetically.</remarks>
		public static JObject ToJson<T>(this T source) where T : SettingsBase<T> {
			if (source == null) throw new ArgumentNullException("source");

			var json = new JObject();
			foreach (var sourceProperty in TypeAccessor<T>.Properties.OrderBy(p => p.JsonName)) {
				var jsonProperty = new JProperty(sourceProperty.JsonName, null);
				sourceProperty.UpdateJsonProperty(source, jsonProperty);
				json.Add(jsonProperty);
			}
			return json;
		}
	}
}
