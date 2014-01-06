using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ConfOxide.MemberAccess {
	///<summary>Contains extension methods for Type instances.</summary>
	public static class TypeExtensions {

		static readonly HashSet<Type> scalarTypes = new HashSet<Type> { typeof(string), typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan), typeof(Uri) };
		///<summary>Checks whether a type is valid as a single settings value.</summary>
		public static bool IsScalarType(this Type type) {
			if (type == typeof(string))
				return true;
			type = Nullable.GetUnderlyingType(type) ?? type;
			if (type.IsPrimitive)
				return true;
			if (scalarTypes.Contains(type))
				return true;
			return false;
		}

		///<summary>Checks whether a type inherits <see cref="SettingsBase{T}"/>.</summary>
		public static bool IsSettingsType(this Type type) {
			return type.BaseType.IsGenericType
				&& type.BaseType.GetGenericTypeDefinition() == typeof(SettingsBase<>)
				&& type.BaseType.GetGenericArguments()[0] == type;				
		}

		///<summary>Gets the element type of a type inheriting <see cref="IList{T}"/>, or null for non-List types.</summary>
		public static Type GetListElementType(this Type type) {
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
				return type.GetGenericArguments()[0];
			var superInterface = type.GetInterface("System.Collections.Generic.IList`1");
			if (superInterface == null)
				return null;
			return superInterface.GetGenericArguments()[0];
		}

		///<summary>Gets a custom attribute, if defined on the specified member.</summary>
		public static TAttribute GetCustomAttribute<TAttribute>(this ICustomAttributeProvider member) where TAttribute : Attribute{
			return member.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
		}
	}
}
