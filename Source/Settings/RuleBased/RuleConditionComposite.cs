using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static CategorizedBillMenus.ScribeUtils;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class RuleConditionOr : RuleConditionComposite {
        static RuleConditionOr() {
            Register(new RuleConditionOr());
        }

        public RuleConditionOr(params RuleCondition[] initial)
            : base(Strings.CondOrName, Strings.CondOrID, Strings.CondOrDesc, true, initial) { }

        public override RuleCondition Copy() => new RuleConditionOr();

        protected override bool Combine(IEnumerable<bool> values) => values.Any(v => v);
    }


    [StaticConstructorOnStartup]
    public class RuleConditionAnd : RuleConditionComposite {
        static RuleConditionAnd() {
            Register(new RuleConditionAnd());
        }

        public RuleConditionAnd(params RuleCondition[] initial)
            : base(Strings.CondAndName, Strings.CondAndID, Strings.CondAndDesc, true, initial) { }

        public override RuleCondition Copy() => new RuleConditionAnd();

        protected override bool Combine(IEnumerable<bool> values) => values.All(v => v);
    }


    [StaticConstructorOnStartup]
    public class RuleConditionNot : RuleConditionComposite {
        static RuleConditionNot() {
            Register(new RuleConditionNot());
        }

        public RuleConditionNot(params RuleCondition[] initial)
            : base(Strings.CondNotName, Strings.CondNotID, Strings.CondNotDesc, false, initial) { }

        public override RuleCondition Copy() => new RuleConditionNot();

        protected override bool Combine(IEnumerable<bool> values) 
            => parts[0] != null && !values.Any(v => v);
    }


    public abstract class RuleConditionComposite : RuleCondition {
        protected List<RuleCondition> parts = new List<RuleCondition>();
        private readonly bool multiple;

        protected RuleConditionComposite(
            string name, string id, string description, bool multiple = true) 
            : base(name, id, description) {
            this.multiple = multiple;
            if (!multiple) parts.Add(null);
        }

        protected RuleConditionComposite(
                string name, string id, string description, bool multiple = true,
                params RuleCondition[] initial) 
                : base(name, id, description) {
            this.multiple = multiple;
            parts.AddRange(initial);
            if (!multiple && parts.Count == 0) {
                parts.Add(null);
            }
        }

        protected abstract bool Combine(IEnumerable<bool> values);

        public override void CopyFrom(RuleCondition from) {
            base.CopyFrom(from);
            if (from is RuleConditionComposite comp) {
                parts = comp.parts;
            }
        }

        public override bool Test(BillMenuEntry entry, bool first) 
            => Combine(parts.Select(c => c?.Test(entry, first) ?? false));

        public override bool Test(BillMenuEntry entry, MenuNode parent) 
            => Combine(parts.Select(c => c?.Test(entry, parent) ?? false));

        protected override void DoSettingsOpen(WidgetRow prevRow, Rect rect, ref float curY) {
            var nextY = prevRow.FinalY + RowHeight;
            if (nextY > curY) curY = nextY;
            var row = new WidgetRow();
            int i = 0;
            Action add = null;
            if (multiple) add = () => AddMenu(parts);
            ExtraWidgets.EditableList(parts, add, DoItem, rect, ref curY);

            void DoItem(RuleCondition item, Rect r, float offset, ref float y) {
                row.Init(r.x, y, Dir, r.width, Margin / 2);
                row.SelectMenuButton(item, c => parts[i] = c);
                item?.DoSettings(row, r, ref y, false);
            }
        }

        public override string SettingsClosedLabel(CategoryRule rule)
            => multiple ? $"{Name}... ({parts.Count} conditions)" 
                        : $"{Name} {parts[0]?.SettingsClosedLabel(rule) ?? "-"}";

        public override void ExposeData() {
            base.ExposeData();
            Registerable_Look(ref parts, "parts");
        }
    }
}
