using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class RuleActionNamed : RuleActionExtra {
        private string extra;

        static RuleActionNamed() {
            Register(new RuleActionNamed());
        }

        public RuleActionNamed() : this(false) { }

        private RuleActionNamed(bool copies)
            : base(copies, Strings.ActionNamedName, Strings.ActionNamedDesc) { }

        public override RuleAction Copy() => new RuleActionNamed(Copies);

        protected override IEnumerable<string> Categories(BillMenuEntry entry) {
            if (extra.NullOrEmpty()) yield break;
            // TODO: multiple at once? using separator? multiple text fields?
            yield return extra;
        }

        protected override void DoSettingsLocal(WidgetRow row, Rect rect, ref float curY) {
            row.TextField(ref extra);
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref extra, "extra");
        }
    }
}
