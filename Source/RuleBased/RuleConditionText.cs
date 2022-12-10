using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class RuleConditionText : RuleCondition {
        private TextValue value;
        private TextOperation comparison;
        private string expected;

        static RuleConditionText() {
            Register(new RuleConditionText());
        }

        public RuleConditionText()
            : base("text", "Compares the specified value to a given text.") { }

        public RuleConditionText(TextValue value, TextOperation comparison, string expected)
                : this() {
            this.value = value;
            this.comparison = comparison;
            this.expected = expected;
        }

        public RuleConditionText(TextValue value, Comparison comparison, string expected)
                : this(value, TextOperation.Of(comparison), expected) {}

        private RuleConditionText(RuleConditionText toCopy) : this() {
            value = toCopy.value;
            comparison = toCopy.comparison;
            expected = toCopy.expected;
        }

        public override RuleCondition Copy() => new RuleConditionText(this);

        public bool Complete => comparison != null && value != null && !expected.NullOrEmpty();

        public override bool Test(BillMenuEntry entry, bool first) 
            => Complete && value.Compare(comparison, entry, expected);

        public override bool Test(BillMenuEntry entry, MenuNode parent) 
            => Complete && value.Compare(comparison, entry, parent, expected);

        protected override void DoSettingsOpen(WidgetRow row, Rect rect, ref float curY) {
            row.SelectMenuButton(value, v => value = v);
            value?.DoSettings(row, rect, ref curY);
            row.SelectMenuButton(comparison, c => comparison = c);
            comparison?.DoSettings(row, rect, ref curY);
            row.TextField(ref expected, ref curY);
        }

        public override string SettingsClosedLabel 
            => $"{Name} {value?.SettingsClosedLabel} {comparison?.SettingsClosedLabel} \"{expected}\"";

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look  (ref value,      "value");
            Scribe_Deep.Look  (ref comparison, "comparison");
            Scribe_Values.Look(ref expected,   "expected");
        }
    }
}
