using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class ComparisonEquals : ComparisonOperation {
        static ComparisonEquals() {
            Register(new ComparisonEquals());
        }

        public static readonly ComparisonEquals Instance = new ComparisonEquals();

        private ComparisonEquals() : base("equals", "Matches if the value is equal to the text.") {}

        protected override bool DoComparison(string value, string expected) 
            => value.Equals(expected, IgnoreCase);
    }

    [StaticConstructorOnStartup]
    public class ComparisonContains : ComparisonOperation {
        static ComparisonContains() {
            Register(new ComparisonContains());
        }

        public static readonly ComparisonContains Instance = new ComparisonContains();

        private ComparisonContains() : base("contains", "Matches if the value contains the text.") {}

        protected override bool DoComparison(string value, string expected) 
            => value.IndexOf(expected, IgnoreCase) >= 0;
    }

    [StaticConstructorOnStartup]
    public class ComparisonStarts : ComparisonOperation {
        static ComparisonStarts() {
            Register(new ComparisonStarts());
        }

        public static readonly ComparisonStarts Instance = new ComparisonStarts();

        private ComparisonStarts() : base("starts with", "Matches if the value starts with the text.") {}

        protected override bool DoComparison(string value, string expected) 
            => value.StartsWith(expected, IgnoreCase);
    }

    [StaticConstructorOnStartup]
    public class ComparisonEnds : ComparisonOperation {
        static ComparisonEnds() {
            Register(new ComparisonEnds());
        }

        public static readonly ComparisonEnds Instance = new ComparisonEnds();

        private ComparisonEnds() : base("ends with", "Matches if the value ends with the text.") {}

        protected override bool DoComparison(string value, string expected) 
            => value.IndexOf(expected, IgnoreCase) >= 0;
    }

    public abstract class ComparisonOperation : Registerable<ComparisonOperation> {
        public static string AlwaysMatchMarker = new StringBuilder("dummy").ToString();

        public const StringComparison IgnoreCase = StringComparison.OrdinalIgnoreCase;

        protected ComparisonOperation(string name, string description) 
            : base(name, description) {
        }

        public static ComparisonOperation Of(Comparison comp)
            => comp switch {
                Comparison.Equals   => ComparisonEquals.Instance,
                Comparison.Contains => ComparisonContains.Instance,
                Comparison.Starts   => ComparisonStarts.Instance,
                Comparison.Ends     => ComparisonEnds.Instance,
                _ => throw new NotImplementedException()
            };

        public override ComparisonOperation Copy() => this;

        public bool Compare(string value, string expected) {
            if (ReferenceEquals(value, AlwaysMatchMarker)) return true;
            if (value == null || expected == null) return false;
            return DoComparison(value, expected);
        }

        protected abstract bool DoComparison(string value, string expected);

        public virtual void DoSettings(WidgetRow row, Rect rect, ref float curY) { }

        public virtual string SettingsClosedLabel => Name;
    }

    public enum Comparison { Equals, Contains, Starts, Ends }
}
