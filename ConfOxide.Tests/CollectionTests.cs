using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConfOxide.Tests {
	[TestClass]
	public class CollectionTests {
		sealed class MyModel : SettingsBase<MyModel> {
			public IList<int> Numbers { get; private set; }
			public Collection<TimeSpan> Times { get; private set; }
			public ReadOnlyCollectionBuilder<decimal?> Rates { get; private set; }
		}
		[TestMethod]
		public void CollectionInstantiation() {
			var instance = new MyModel();
			instance.Numbers.Should().BeOfType<List<int>>("because IList<T> should be created as List<T>");
			instance.Times.Should().BeOfType<Collection<TimeSpan>>();
			instance.Rates.Should().BeOfType<ReadOnlyCollectionBuilder<decimal?>>();
		}

		[TestMethod]
		public void CollectionReset() {
			var instance = new MyModel {
				Numbers = { 1, 2, 3 },
				Rates = { .1234m, .5678m },
				Times = { TimeSpan.FromHours(1), TimeSpan.FromHours(2) }
			};
			instance.ResetValues();
			instance.Numbers.Should().BeEmpty();
			instance.Rates.Should().BeEmpty();
			instance.Times.Should().BeEmpty();
		}

		[TestMethod]
		public void CollectionEquality() {
			var instance = new MyModel {
				Numbers = { 1, 2, 3 },
				Rates = { .1234m, .5678m },
				Times = { TimeSpan.FromHours(1), TimeSpan.FromHours(2) }
			};
			var instance2 = new MyModel {
				Numbers = { 1, 2, 3 },
				Rates = { .1234m, .5678m },
				Times = { TimeSpan.FromHours(1), TimeSpan.FromHours(2) }
			};
			instance.IsEquivalentTo(instance2).Should().BeTrue();
			instance2.IsEquivalentTo(instance).Should().BeTrue();

			instance2.Rates.Add(null);

			instance.IsEquivalentTo(instance2).Should().BeFalse();
			instance2.IsEquivalentTo(instance).Should().BeFalse();

			instance.IsEquivalentTo(new MyModel()).Should().BeFalse();
			new MyModel().IsEquivalentTo(instance).Should().BeFalse();

		}

		[TestMethod]
		public void CollectionCopy() {
			var instance = new MyModel {
				Numbers = { 1, 2, 3 },
				Rates = { .1234m, .5678m },
				Times = { TimeSpan.FromHours(1), TimeSpan.FromHours(2) }
			};
			var copy = instance.CreateCopy();
			copy.IsEquivalentTo(instance).Should().BeTrue();
			instance.IsEquivalentTo(copy).Should().BeTrue();
		}

		[TestMethod]
		public void CollectionAssignFrom() {
			var instance = new MyModel {
				Numbers = { 1, 2, 3 },
				Rates = { .1234m, .5678m },
				Times = { TimeSpan.FromHours(1), TimeSpan.FromHours(2) }
			};
			var target = new MyModel {
				Numbers = { 4,5,6 },
				Rates = { 99999999999,8888888888 },
				Times = { TimeSpan.MinValue, TimeSpan.MaxValue }
			};
			target.AssignFrom(instance);
			instance.IsEquivalentTo(target).Should().BeTrue();
		}

		[TestMethod]
		public void CollectionJsonRoundTrip() {
			var instance = new MyModel {
				Numbers = { 1, 2, 3 },
				Rates = { .1234m, .5678m },
				Times = { TimeSpan.FromHours(1), TimeSpan.FromHours(2) }
			};
			var copy = new MyModel();
			copy.ReadJson(instance.ToJson());

			copy.IsEquivalentTo(instance).Should().BeTrue();
		}
	}
}
