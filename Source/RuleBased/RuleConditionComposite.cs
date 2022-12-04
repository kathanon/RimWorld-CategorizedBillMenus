using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class RuleConditionOr : RuleConditionComposite {
        static RuleConditionOr() {
            Register(new RuleConditionOr());
        }

        public RuleConditionOr()
            : base(Strings.CondOrName, Strings.CondOrDesc) { }

        protected override RuleConditionComposite Create() => new RuleConditionOr();

        protected override bool Combine(IEnumerable<bool> values) => values.Any(v => v);
    }


    [StaticConstructorOnStartup]
    public class RuleConditionAnd : RuleConditionComposite {
        static RuleConditionAnd() {
            Register(new RuleConditionAnd());
        }

        public RuleConditionAnd()
            : base(Strings.CondAndName, Strings.CondAndDesc) { }

        protected override RuleConditionComposite Create() => new RuleConditionAnd();

        protected override bool Combine(IEnumerable<bool> values) => values.All(v => v);
    }


    public abstract class RuleConditionComposite : RuleCondition {
        protected List<RuleCondition> parts = new List<RuleCondition>();

        protected RuleConditionComposite(string name, string description) 
            : base(name, description) {}

        protected abstract bool Combine(IEnumerable<bool> values);

        protected abstract RuleConditionComposite Create();

        public override RuleCondition Copy() {
            var res = Create();
            res.parts.AddRange(parts.Select(c => c.Copy()));
            return res;
        }

        public override bool Test(BillMenuEntry entry, bool first) 
            => Combine(parts.Select(c => c.Test(entry, first)));

        public override bool Test(BillMenuEntry entry, MenuNode parent) 
            => Combine(parts.Select(c => c.Test(entry, parent)));

        public override void DoSettings(WidgetRow _, Rect rect, ref float curY) {
            var row = new WidgetRow();
            if (parts == null) parts = new List<RuleCondition>();
            ExtraWidgets.EditableList(parts, () => AddMenu(parts), DoItem, rect, ref curY);

            void DoItem(RuleCondition item, Rect r, float offset, ref float y) {
                row.Init(r.x, y, Dir, r.width, Margin / 2);
                row.MenuButton(item, () => SelectionMenu(c => parts.Replace(item, c), item));
                item.DoSettings(row, r, ref y);
            }
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref parts, "parts", LookMode.Deep);
        }
    }
}
