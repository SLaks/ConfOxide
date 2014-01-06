using System;
using System.ComponentModel;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConfOxide.Tests {
	[TestClass]
	public class ScalarTests {
		sealed class DefaultValues : SettingsBase<DefaultValues> {
			public int DefZero { get; set; }
			[DefaultValue(42)]
			public int DefFourtyTwo { get; set; }
			[DefaultValue("Hello")]
			public string Greeting { get; set; }
			[DefaultValue("2000-01-01")]
			public DateTime Century { get; set; }
		}

		[TestMethod]
		public void ScalarDefaultValues() {
			var instance = new DefaultValues();
			instance.DefZero.Should().Be(0);
			instance.DefFourtyTwo.Should().Be(42);
			instance.Greeting.Should().Be("Hello");
			instance.Century.Should().Be(new DateTime(2000, 01, 01));
		}

		[TestMethod]
		public void ScalarReset() {
			var instance = new DefaultValues { Greeting = "Hola", Century = DateTime.Now, DefZero = 12 };
			instance.ResetValues();

			instance.DefZero.Should().Be(0);
			instance.Greeting.Should().Be("Hello");
			instance.Century.Should().Be(new DateTime(2000, 01, 01));
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
		}
	}
}
