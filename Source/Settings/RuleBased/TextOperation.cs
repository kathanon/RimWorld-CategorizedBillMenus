using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class TextEquals : TextOperation {
        static TextEquals() {
            Register(new TextEquals());
        }

        public static readonly TextEquals Instance = new TextEquals();

        public TextEquals() : base("equals", "equals", "Matches if the value is equal to the text.") {}

        protected override bool DoComparison(string value, string expected) 
            => value.Equals(expected, IgnoreCase);
    }

    [StaticConstructorOnStartup]
    public class TextContains : TextOperation {
        static TextContains() {
            Register(new TextContains());
        }

        public static readonly TextContains Instance = new TextContains();

        public TextContains() : base("contains", "contains", "Matches if the value contains the text.") {}

        protected override bool DoComparison(string value, string expected) 
            => value.IndexOf(expected, IgnoreCase) >= 0;
    }

    [StaticConstructorOnStartup]
    public class TextStarts : TextOperation {
        static TextStarts() {
            Register(new TextStarts());
        }

        public static readonly TextStarts Instance = new TextStarts();

        public TextStarts() : base("starts with", "starts", "Matches if the value starts with the text.") {}

        protected override bool DoComparison(string value, string expected) 
            => value.StartsWith(expected, IgnoreCase);
    }

    [StaticConstructorOnStartup]
    public class TextEnds : TextOperation {
        static TextEnds() {
            Register(new TextEnds());
        }

        public static readonly TextEnds Instance = new TextEnds();

        public TextEnds() : base("ends with", "ends", "Matches if the value ends with the text.") {}

        protected override bool DoComparison(string value, string expected) 
            => value.IndexOf(expected, IgnoreCase) >= 0;
    }

    public abstract class TextOperation : RegisterableById<TextOperation> {
        public static string AlwaysMatchMarker = new StringBuilder("dummy").ToString();

        public const StringComparison IgnoreCase = StringComparison.OrdinalIgnoreCase;

        protected TextOperation(string name, string id, string description) 
            : base(name, id, description) {
        }

        public static TextOperation Of(Comparison comp)
            => comp switch {
                Comparison.Equals   => TextEquals.Instance,
                Comparison.Contains => TextContains.Instance,
                Comparison.Starts   => TextStarts.Instance,
                Comparison.Ends     => TextEnds.Instance,
                _ => throw new NotImplementedException()
            };

        public override TextOperation Copy() => this;

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
