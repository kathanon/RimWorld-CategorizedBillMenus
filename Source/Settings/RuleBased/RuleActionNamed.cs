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

        public RuleActionNamed() : this("", false) {}

        public RuleActionNamed(string extra) : this(extra, false) {}

        private RuleActionNamed(string extra, bool copies = false)
                : base(copies, Strings.ActionNamedName, Strings.ActionNamedID, Strings.ActionNamedDesc) {
            this.extra = extra;
        }

        public override RuleAction Copy() => new RuleActionNamed(extra, Copies);

        protected override IEnumerable<string> Categories(BillMenuEntry entry) {
            if (extra.NullOrEmpty()) yield break;
            // TODO: multiple at once? using separator? multiple text fields?
            yield return extra;
        }

        protected override void DoSettingsOpen(WidgetRow row, Rect rect, ref float curY) {
            row.TextField(ref extra);
        }

        public override string SettingsClosedLabel(CategoryRule rule) 
            => $"{Name} \"{extra}\"";

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref extra, "extra");
        }
    }
}
