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
		///<summary>Creates a <see cref="JValue"/> holding a strongly typed value.</summary>
		public static readonly Func<T, JValue> ToJson = CreateJsonAccessor();

		private static readonly ParameterExpression jParam = Expression.Parameter(typeof(JValue));

		///<summary>Parses a <see cref="JValue"/> value into a strongly-typed value.</summary>
		public static readonly Func<JValue, T> FromJson = Expression.Lambda<Func<JValue, T>>(
			Expression.Convert(jParam, typeof(T)),
			jParam
		).Compile();

		private static Func<T, JValue> CreateJsonAccessor() {
			var param = Expression.Parameter(typeof(T));
			//TODO: Investigate performance of boxing vs. string conversions in JSON.Net.
			// The value to pass to the JValue ctor
			Expression innerValue = param;

			Type underlyingType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
			Type convertTo;
			if (ScalarTypeInfo.JsonConvertibleTypes.TryGetValue(underlyingType, out convertTo))
				innerValue = Expression.Convert(innerValue, convertTo);
			if (underlyingType == typeof(TimeSpan))
				innerValue = Expression.Call(innerValue, "ToString", null);

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
		static readonly ConstructorInfo nullJValueCtor = typeof(JValue).GetConstructor(new[] { typeof(object), typeof(JTokenType) });
		public static readonly Expression JNull = Expression.New(
			nullJValueCtor,
			Expression.Constant(null), Expression.Constant(JTokenType.Null)
		);

		///<summary>Maps types that have no JToken constructor to types they're convertible to that do.</summary>
		public static readonly Dictionary<Type, Type> JsonConvertibleTypes = new Dictionary<Type, Type> {
			{ typeof(short),    typeof(long)  },
			{ typeof(int),      typeof(long)  },
			{ typeof(ushort),   typeof(ulong) },
			{ typeof(uint),     typeof(ulong) },
		};
	}
}
