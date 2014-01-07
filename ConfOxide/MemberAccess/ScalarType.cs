using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ConfOxide.MemberAccess {
	///<summary>Holds compiled delegates that interact with strongly-typed scalar values.</summary>
	static class ScalarType<T> {
		private static readonly Type underlyingType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

		///<summary>Creates a <see cref="JValue"/> holding a strongly typed value.</summary>
		public static readonly Func<T, JValue> ToJson = CreateJsonAccessor();

		// This must be declared as JToken because Expression.Convert() won't search parent types.
		private static readonly ParameterExpression jParam = Expression.Parameter(typeof(JToken));
		///<summary>Parses a <see cref="JValue"/> value into a strongly-typed value.</summary>
		public static readonly Func<JValue, T> FromJson = Expression.Lambda<Func<JToken, T>>(
			Expression.Convert(jParam, typeof(T)),
			jParam
		).Compile();

		private static Func<T, JValue> CreateJsonAccessor() {
			var param = Expression.Parameter(typeof(T));

			// The value to pass to the JValue ctor
			Expression innerValue = param;
			if (underlyingType != typeof(T))
				innerValue = Expression.Property(innerValue, "Value");

			Type convertTo;
			if (ScalarTypeInfo.JsonConvertibleTypes.TryGetValue(underlyingType, out convertTo))
				innerValue = Expression.Convert(innerValue, convertTo);
			innerValue = Expression.New(typeof(JValue).GetConstructor(new[] { convertTo ?? underlyingType }), innerValue);

			Expression outerValue;
			if (underlyingType == typeof(T))
				outerValue = innerValue;
			else
				outerValue = Expression.Condition(Expression.Property(param, "HasValue"),
					innerValue,
					ScalarTypeInfo.JNull
				);
			return Expression.Lambda<Func<T, JValue>>(outerValue, param).Compile();
		}
	}
	///<summary>Holds static fields used by <see cref="ScalarType{T}"/>.  Using a separate class prevents us from having one copy of the field for each type.</summary>
	static class ScalarTypeInfo {
		static readonly ConstructorInfo nullJValueCtor = typeof(JValue).GetConstructor(new[] { typeof(object) });
		public static readonly Expression JNull = Expression.New(
			nullJValueCtor,
			Expression.Constant(null)
		);

		///<summary>Maps types that have no JToken constructor to types they're convertible to that do.</summary>
		public static readonly Dictionary<Type, Type> JsonConvertibleTypes = new Dictionary<Type, Type> {
			{ typeof(short),    typeof(long)  },
			{ typeof(int),      typeof(long)  },
			{ typeof(ushort),   typeof(ulong) },
			{ typeof(uint),     typeof(ulong) },
			{ typeof(decimal),  typeof(object)},        // TODO: Remove this https://github.com/JamesNK/Newtonsoft.Json/pull/183 is merged (for JValue(decimal) ctor)
		};
	}
}
