using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class RuleConditionComparison : RuleCondition {
        public const float Margin = Settings.Margin;

        private ComparisonValue value;
        private Comparison comparison;
        private string expected;

        static RuleConditionComparison() {
            Register(new RuleConditionComparison());
        }

        public RuleConditionComparison()
            : base("compare", "Matches if the specified comparison is true.") { }

        private RuleConditionComparison(RuleConditionComparison toCopy) : this() {
            value = toCopy.value;
            comparison = toCopy.comparison;
            expected = toCopy.expected;
        }

        public override RuleCondition Copy() => new RuleConditionComparison(this);

        public bool Complete => comparison != null && value != null && !expected.NullOrEmpty();

        public override bool Test(BillMenuEntry entry, bool first) 
            => Complete && value.Compare(comparison, entry, expected);

        public override bool Test(BillMenuEntry entry, MenuNode parent) 
            => Complete && value.Compare(comparison, entry, parent, expected);

        public override void DoSettings(WidgetRow row, Rect rect, ref float curY) {
            row.MenuButton(value, () => ComparisonValue.SelectionMenu(c => value = c, value));
            row.MenuButton(comparison, () => Comparison.SelectionMenu(c => comparison = c, comparison));
            var r = row.ButtonRect(null, 70f);
            r.height -= 2f;
            expected = Widgets.TextField(r, expected);
            curY = r.yMax + Margin;
            base.DoSettings(row, rect, ref curY);
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look  (ref value,      "value");
            Scribe_Deep.Look  (ref comparison, "comparison");
            Scribe_Values.Look(ref expected,   "expected");
        }
    }
}
