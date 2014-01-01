using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ConfOxide.MemberAccess {

	///<summary>A PropertyAccessor for a scalar property.</summary>
	public class ValuePropertyAccessor<T> : IPropertyAccessor<T> {
		private readonly Func<T, JToken> jsonAccessor;

		private readonly Action<T, JToken> jsonSetter;

		private readonly Action<T> defaultSetter;
		private readonly Action<T, T> valueCopier;
		private readonly Func<T, T, bool> valueComparer;
		internal ValuePropertyAccessor(PropertyInfo property) {
			Property = property;

			var param = Expression.Parameter(typeof(T));
			var paramValue = Expression.Property(param, property);

			var param2 = Expression.Parameter(typeof(T));
			valueCopier = Expression.Lambda<Action<T, T>>(
				Expression.Call(param2, property.GetSetMethod(), paramValue),
				param, param2
			).Compile();
			valueComparer = Expression.Lambda<Func<T, T, bool>>(
				Expression.Equal(paramValue, Expression.Property(param2, property)),
				param, param2
			).Compile();

			defaultSetter = Expression.Lambda<Action<T>>(
				Expression.Call(param, property.GetSetMethod(), Expression.Default(property.PropertyType)),
				param
			).Compile();

			jsonAccessor = CreateJsonAccessor(property);

			var jParam = Expression.Parameter(typeof(JToken));
			jsonSetter = Expression.Lambda<Action<T, JToken>>(
				Expression.Call(param, property.GetSetMethod(), Expression.Convert(jParam, property.PropertyType)),
				param, jParam
			).Compile();
		}

		static readonly ConstructorInfo nullJValue = typeof(JValue).GetConstructor(new[] { typeof(object), typeof(JTokenType) });
		///<summary>Maps types that have no JToken constructor to types they're convertible to that do.</summary>
		static readonly Dictionary<Type, Type> jsonConvertibleTypes = new Dictionary<Type, Type>
		{
			{ typeof(short),    typeof(long)  },
			{ typeof(int),      typeof(long)  },
			{ typeof(ushort),   typeof(ulong) },
			{ typeof(uint),     typeof(ulong) },
		};
		private static Func<T, JToken> CreateJsonAccessor(PropertyInfo property) {
			var param = Expression.Parameter(typeof(T));
			var paramValue = Expression.Property(param, property);
			//TODO: Investigate performance of boxing vs. string conversions in JSON.Net.
			// The value to pass to the JValue ctor
			Expression innerValue = paramValue;

			Type underlyingType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
			Type convertTo;
			if (jsonConvertibleTypes.TryGetValue(underlyingType, out convertTo))
				innerValue = Expression.Convert(innerValue, convertTo);
			if (underlyingType == typeof(TimeSpan))
				innerValue = Expression.Call(innerValue, "ToString", null);

			innerValue = Expression.New(typeof(JValue).GetConstructor(new[] { convertTo ?? underlyingType }), innerValue);

			Expression outerValue;
			if (underlyingType == property.PropertyType)
				outerValue = innerValue;
			else
				outerValue = Expression.Condition(Expression.Property(paramValue, "HasValue"),
					innerValue,
					Expression.New(nullJValue, Expression.Constant(null), Expression.Constant(JTokenType.Null))
				);
			return Expression.Lambda<Func<T, JToken>>(outerValue, param).Compile();
		}

		///<summary>Gets the property being accessed.</summary>
		public PropertyInfo Property { get; private set; }

		///<summary>Copies the value of this property from one owning object to another.</summary>
		///<param name="from">The instance to read the value from.</param>
		///<param name="to">The instance to write the value to.</param>
		public void Copy(T from, T to) { valueCopier(from, to); }

		///<summary>Resets the value of this property on an owning object to its default value.</summary>
		public void ResetValue(T instance) { defaultSetter(instance); }

		///<summary>Compares the values of the property from two owning objects.</summary>
		public bool CompareValues(T x, T y) { return valueComparer(x, y); }

		///<summary>Reads this property's value from a JSON token into an instance.</summary>
		public void FromJson(T instance, JToken token) { jsonSetter(instance, token); }

		///<summary>Creates a JSON token holding this property's value.</summary>
		public JToken ToJson(T instance) { return jsonAccessor(instance); }

		///<summary>Updates the value of a JProperty to reflect this property's value.</summary>
		public void UpdateJsonProperty(T instance, JProperty jsonProperty) { jsonProperty.Value = ToJson(instance); }

		///<summary>Does nothing; value properties do not need initialization.</summary>
		public void InitializeValue(T instance) { }
	}
}
