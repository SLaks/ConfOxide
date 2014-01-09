using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ConfOxide.Tests {
	[TestClass]
	public class SettingsCollectionTests {
		sealed class MyModel : SettingsBase<MyModel> {
			public IList<InnerModel> Items { get; private set; }
		}
		sealed class InnerModel : SettingsBase<InnerModel> {
			public string Name { get; set; }
			[DefaultValue(5)]
			public int Level { get; set; }
		}
		[TestMethod]
		public void SettingsCollectionInstantiation() {
			var instance = new MyModel();
			instance.Items.Should().BeOfType<List<InnerModel>>("because IList<T> should be created as List<T>");
		}

		[TestMethod]
		public void SettingsCollectionReset() {
			var instance = new MyModel {
				Items = { new InnerModel() }
			};
			instance.ResetValues();
			instance.Items.Should().BeEmpty();
		}

		[TestMethod]
		public void SettingsCollectionEquality() {
			var instance = new MyModel {
				Items = { new InnerModel { Name = "Fred" } }
			};
			var instance2 = new MyModel {
				Items = { new InnerModel { Name = "Fred" } }
			};
			instance.IsEquivalentTo(instance2).Should().BeTrue();
			instance2.IsEquivalentTo(instance).Should().BeTrue();

			instance2.Items[0].Level = 2;

			instance.IsEquivalentTo(instance2).Should().BeFalse();
			instance2.IsEquivalentTo(instance).Should().BeFalse();

			instance.Items[0].Level = 2;
			instance.IsEquivalentTo(instance2).Should().BeTrue();
			instance.Items.Add(new InnerModel());
			instance.IsEquivalentTo(instance2).Should().BeFalse();
		}

		[TestMethod]
		public void SettingsCollectionCopy() {
			var instance = new MyModel {
				Items = { new InnerModel { Name = "Fred" } }
			};
			var copy = instance.CreateCopy();
			copy.IsEquivalentTo(instance).Should().BeTrue();
			instance.IsEquivalentTo(copy).Should().BeTrue();

			copy.Items[0].Should().NotBeSameAs(instance.Items[0]);
			copy.Items.Clear();
			instance.Items.Should().HaveCount(1, "because clearing the copy should not affect the original");
		}

		[TestMethod]
		public void SettingsCollectionAssignFrom() {
			var instance = new MyModel {
				Items = { new InnerModel { Name = "Fred" } }
			};
			var target = new MyModel {
				Items = { new InnerModel { Name = "Joe" }, new InnerModel { Name = "Schmoe" } }
			};
			target.AssignFrom(instance);
			instance.IsEquivalentTo(target).Should().BeTrue();
		}

		[TestMethod]
		public void SettingsCollectionJsonRoundTrip() {
			var instance = new MyModel {
				Items = { new InnerModel { Name = "Fred" } }
			};
			var copy = new MyModel();
			copy.ReadJson(instance.ToJson());

			copy.IsEquivalentTo(instance).Should().BeTrue();
		}
		[TestMethod]
		public void SettingsCollectionReadJsonPreservesInstances() {
			var json = JObject.Parse(@"{
				""Items"": [
					{ ""Name"": ""Jason"" },
					{ ""Level"": 57 }
				]
			}");
			var instance = new MyModel {
				Items = { new InnerModel() }
			};
			var oldItems = instance.Items.ToList();
			instance.ReadJson(json);
			instance.Items[0].Should().BeSameAs(oldItems[0], "because ReadJson() should preserve existing instances");
			instance.IsEquivalentTo(new MyModel {
				Items =
				{
					new InnerModel { Name = "Jason" },
					new InnerModel { Level = 57 }
				}
			}).Should().BeTrue();
			json.Value<JArray>("Items").RemoveAt(0);
			instance.ReadJson(json);
			instance.Items.Should().Equal(oldItems, "Removing an element should preserve earlier elements");
		}
		[TestMethod]
		public void SettingsCollectionUpdateJsonPropertyOrder() {
			var json = JObject.Parse(@"{
				""Items"": [
					{ ""Name"": ""Jason"", ""otherProperty"": true },
					{ ""Level"": 57 }
				]
			}");
			var instance = new MyModel {
				Items = { new InnerModel() }
			};

			instance.UpdateJson(json);
			var arr = json.Value<JArray>("Items");
			arr.Should().HaveCount(1);
			arr.Value<JObject>(0)
				.Properties()
				.Select(p => p.Name)
				.Should().Equal(new[] { "Name", "otherProperty", "Level" });

			arr[0]["Name"].Value<string>().Should().BeNull();
			arr[0]["Level"].Value<int>().Should().Be(5);

			instance.Items.Add(new InnerModel { Level = 70 });
			instance.UpdateJson(json);

			arr.Should().HaveCount(2);
			arr.Value<JObject>(1)
				.Properties()
				.Select(p => p.Name)
				.Should().Equal(new[] { "Level", "Name" });
			arr[1]["Level"].Value<int>().Should().Be(70);
		}
	}
}
