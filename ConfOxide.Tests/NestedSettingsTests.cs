using System;
using System.ComponentModel;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ConfOxide.Tests {
	[TestClass]
	public class NestedSettingsTests {
		sealed class Outer : SettingsBase<Outer> {
			[DefaultValue("Joe")]
			public string Name { get; set; }
			public Inner Inner { get; private set; }
			public int Version { get; set; }
		}
		sealed class Inner : SettingsBase<Inner> {
			[DefaultValue("1987-06-05")]
			public DateTimeOffset? EventTime { get; set; }
			[DefaultValue(96)]
			public decimal? Value { get; set; }
			public string Description { get; set; }
		}

		[TestMethod]
		public void NestedSettingsInstantiation() {
			var instance = new Outer();
			instance.Inner.Should().NotBeNull();
			instance.Inner.Value.Should().Be(96);
		}
		[TestMethod]
		public void NestedSettingsReset() {
			var instance = new Outer {
				Inner = { Value = 23 }
			};
			instance.ResetValues();
			instance.Inner.Value.Should().Be(96);
		}

		[TestMethod]
		public void NestedSettingsEquality() {
			var instance = new Outer {
				Inner = { Value = 23 }
			};
			var instance2 = new Outer {
				Inner = { Value = 23 }
			};
			instance.IsEquivalentTo(instance2).Should().BeTrue();
			instance2.IsEquivalentTo(instance).Should().BeTrue();

			instance2.Inner.Value = 12;

			instance.IsEquivalentTo(instance2).Should().BeFalse();
			instance2.IsEquivalentTo(instance).Should().BeFalse();
		}

		[TestMethod]
		public void NestedSettingsCopy() {
			var instance = new Outer {
				Inner = { Value = 23 }
			};
			var copy = instance.CreateCopy();
			copy.IsEquivalentTo(instance).Should().BeTrue();
			instance.IsEquivalentTo(copy).Should().BeTrue();
		}

		[TestMethod]
		public void NestedSettingsAssignFrom() {
			var instance = new Outer {
				Inner = { Value = 23 }
			};
			var target = new Outer {
				Inner = { Value = 12 }
			};
			target.AssignFrom(instance);
			instance.IsEquivalentTo(target).Should().BeTrue();
		}

		[TestMethod]
		public void NestedSettingsJsonRoundTrip() {
			var instance = new Outer {
				Name = "Fred",
				Inner = { Value = 23 }
			};
			var copy = new Outer();
			copy.ReadJson(instance.ToJson());

			copy.IsEquivalentTo(instance).Should().BeTrue();
		}

		[TestMethod]
		public void NestedJsonPreservesPropertyOrder() {
			var json = JObject.Parse(@"{
					""Name"": null,
					""Inner"": {
						""Value"": 97,
						""EventTime"": ""2001-01-01""
					}		
				}");
			var instance = new Outer();
			instance.ReadJson(json);
			instance.Version++;

			instance.Name.Should().BeNull();
			instance.Inner.Value.Should().Be(97);
			instance.Inner.Description.Should().BeNull();
			instance.Inner.EventTime.Should().Be(new DateTimeOffset(new DateTime(2001, 1, 1)));

			instance.UpdateJson(json);
			json.Properties().Select(p => p.Name).Should().Equal(new[]
			{
				"Name",
				"Inner",
				"Version"
			});
			json.Value<JObject>("Inner").Properties().Select(p => p.Name).Should().Equal(new[]
			{
				"Value",
				"EventTime",
				"Description"
			});
		}

	}
}
