using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ConfOxide.Tests {
	[TestClass]
	public class InheritanceTests {
		class BaseLayer<T> : SettingsBase<T> where T : BaseLayer<T> {
			[DefaultValue(1)]
			public int Version { get; set; }
		}
		sealed class Person : BaseLayer<Person> {
			public string Name { get; set; }
			public Address Address { get; private set; }
		}
		class LoggedLayer<T> : BaseLayer<T> where T : LoggedLayer<T> {
			public DateTime Timestamp { get; set; }
		}
		sealed class Address : LoggedLayer<Address> {
			public string Line1 { get; set; }
		}

		[TestMethod]
		public void InheritedDefaultValues() {
			var model = new Person();
			model.Version.Should().Be(1, "because of the default value from the base property");
			model.Address.Version.Should().Be(1, "because of the default value from the base property");
		}
		[TestMethod]
		public void InheritedEquality() {
			var model1 = new Person();
			var model2 = new Person { Address = { Version = 2 } };
			model1.IsEquivalentTo(model2).Should().BeFalse();
			model1.AssignFrom(model2);
			model1.IsEquivalentTo(model2).Should().BeTrue();
		}
		[TestMethod]
		public void InheritedJsonRoundTrip() {
			var model = new Person();
			var json = model.ToJson();
			var expectedJson = new JObject(
				new JProperty("Address", new JObject(
					new JProperty("Line1", null),
					new JProperty("Timestamp", DateTime.MinValue),
					new JProperty("Version", 1)
				)),
				new JProperty("Name", null),
				new JProperty("Version", 1)
			);
			JToken.DeepEquals(json, expectedJson).Should().BeTrue();
			var newModel = new Person { Address = { Version = 2, Line1 = "ABC!" } };
			newModel.ReadJson(json);
			newModel.IsEquivalentTo(model).Should().BeTrue();
		}
	}
}
