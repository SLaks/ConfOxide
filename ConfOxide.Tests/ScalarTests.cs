using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConfOxide.Tests {
	[TestClass]
	public class ScalarTests {
		enum MyOption {
			None,
			Partial = 123456,
			Full = 654321
		}

		sealed class DefaultValues : SettingsBase<DefaultValues> {
			[DefaultValue(null)]
			public byte? DefNull { get; set; }

			[DefaultValue(MyOption.Full)]
			public MyOption StandardLevel { get; set; }
			public MyOption? ExtraLevel { get; set; }

			[DefaultValue("67")]
			public ushort? ComplexConversions { get; set; }
			public int DefZero { get; set; }
			[DefaultValue("42.1234")]
			public decimal? DefFourtyTwo { get; set; }
			[DefaultValue("Hello")]
			public string Greeting { get; set; }
			[DefaultValue("2000-01-01")]
			public DateTime Century { get; set; }
			[DefaultValue("02:00:00")]
			public TimeSpan? LongTime { get; set; }
		}

		[TestMethod]
		public void ScalarDefaultValues() {
			var instance = new DefaultValues();
			instance.DefNull.Should().NotHaveValue();
			instance.StandardLevel.Should().Be(MyOption.Full);
			instance.ExtraLevel.Should().BeNull();
			instance.DefZero.Should().Be(0);
			instance.DefFourtyTwo.Should().Be(42.1234m);
			instance.Greeting.Should().Be("Hello");
			instance.Century.Should().Be(new DateTime(2000, 01, 01));
			instance.LongTime.Should().Be(TimeSpan.FromHours(2));
		}

		[TestMethod]
		public void ScalarReset() {
			var instance = new DefaultValues { DefNull = 12, Greeting = "Hola", Century = DateTime.Now, DefZero = 12 };
			instance.ResetValues();

			instance.DefNull.Should().NotHaveValue();
			instance.DefZero.Should().Be(0);
			instance.Greeting.Should().Be("Hello");
			instance.Century.Should().Be(new DateTime(2000, 01, 01));
			instance.LongTime.Should().Be(TimeSpan.FromHours(2));
		}

		[TestMethod]
		public void ScalarCopy() {
			var source = new DefaultValues { Greeting = "Hola", Century = DateTime.Now, DefZero = 12 };
			var target = source.CreateCopy();

			target.DefZero.Should().Be(12);
			target.Greeting.Should().Be("Hola");
			target.Century.Should().Be(source.Century);
		}

		[TestMethod]
		public void ScalarEquality() {
			var source = new DefaultValues { Greeting = "Hola", Century = DateTime.Now, DefZero = 12 };
			var target = source.CreateCopy();
			target.IsEquivalentTo(source).Should().BeTrue();
			source.IsEquivalentTo(target).Should().BeTrue();

			target.ExtraLevel = MyOption.Partial;

			target.IsEquivalentTo(source).Should().BeFalse();
			source.IsEquivalentTo(target).Should().BeFalse();
		}

		[TestMethod]
		public void ScalarJsonRoundTrip() {
			var instance = new DefaultValues {
				DefNull = 84,
				Greeting = "Hola",
				Century = DateTime.Now,
				DefZero = 12,
				LongTime = TimeSpan.FromMinutes(3)
			};
			var copy = new DefaultValues();
			copy.ReadJson(instance.ToJson());

			copy.IsEquivalentTo(instance).Should().BeTrue();
		}
		[TestMethod]
		public void JsonPreservesPropertyOrder() {
			var json = JObject.Parse(@"{
					""LongTime"": null,
					""Greeting"": ""Bye"",
					""ExtraLevel"": ""Partial"",
					""StandardLevel"": 123456,
					""DefNull"": 24					
				}");
			var instance = new DefaultValues {
				DefNull = 84,
				Greeting = "Hola",
				DefZero = 12,
				LongTime = TimeSpan.FromMinutes(3)
			};
			instance.ReadJson(json);

			instance.ExtraLevel.Should().Be(MyOption.Partial);
			instance.StandardLevel.Should().Be(MyOption.Partial);
			instance.LongTime.Should().NotHaveValue();
			instance.DefNull.Should().Be(24);
			instance.Greeting.Should().Be("Bye");

			instance.UpdateJson(json);
			json.Properties().Select(p => p.Name).Should().Equal(new[]
			{
				"LongTime",
				"Greeting",
				"ExtraLevel",
				"StandardLevel",
				"DefNull",
				"Century",
				"ComplexConversions",
				"DefFourtyTwo",
				"DefZero",
			});
		}

		sealed class JsonNames : SettingsBase<JsonNames> {
			[JsonProperty("myDate")]
			public DateTimeOffset? NullDate { get; set; }
			[DefaultValue(67)]
			[DataMember(Name = "myNum")]
			public decimal DefValue { get; set; }
		}
		[TestMethod]
		public void JsonPropertyNames() {
			var instance = new JsonNames();
			instance.ToJson()
					.Properties()
					.Select(p => p.Name)
					.Should().Equal(new[] { "myDate", "myNum" });

			var json = JObject.Parse(@"{
				""myNum"": 24,
				""myDate"": ""2012-11-10""
			}");
			instance.ReadJson(json);
			instance.DefValue.Should().Be(24);
			instance.NullDate.Should().Be(new DateTimeOffset(new DateTime(2012, 11, 10)));
		}
	}
}
