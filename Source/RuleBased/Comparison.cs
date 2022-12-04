using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class ComparisonEquals : Comparison {
        static ComparisonEquals() {
            Register(new ComparisonEquals());
        }

        public ComparisonEquals() : base("equals", "Matches if the values are equal.") {}

        protected override bool DoComparison(string value, string expected) 
            => value.Equals(expected, IgnoreCase);
    }

    [StaticConstructorOnStartup]
    public class ComparisonContains : Comparison {
        static ComparisonContains() {
            Register(new ComparisonContains());
        }

        public ComparisonContains() : base("contains", "Matches if the first value contains the second.") {}

        protected override bool DoComparison(string value, string expected) 
            => value.IndexOf(expected, IgnoreCase) == 0;
    }

    public abstract class Comparison : Registerable<Comparison>, ISettingsEntry {
        public static string AlwaysMatchMarker = new StringBuilder("dummy").ToString();

        public const StringComparison IgnoreCase = StringComparison.OrdinalIgnoreCase;

        protected Comparison(string name, string description) 
            : base(name, description) {
        }

        public override Comparison Copy() => this;

        public bool Compare(string value, string expected) {
            if (ReferenceEquals(value, AlwaysMatchMarker)) return true;
            if (value == null || expected == null) return false;
            return DoComparison(value, expected);
        }

        protected abstract bool DoComparison(string value, string expected);

        public virtual void DoSettings(WidgetRow row, Rect rect, ref float curY) { }
    }
}
