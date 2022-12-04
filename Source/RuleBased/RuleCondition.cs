using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    public abstract class RuleCondition : Registerable<RuleCondition>, ISettingsEntry {
        public const UIDirection Dir = UIDirection.RightThenDown;
        public const float Margin = Settings.Margin;

        private bool onCopied = false;
        private bool onMoved  = true;

        protected RuleCondition(string name, string description)
            : base(name, description) { }

        public bool OnCopied => onCopied;
        public bool OnMoved => onMoved;

        public abstract bool Test(BillMenuEntry entry, bool first);

        public abstract bool Test(BillMenuEntry entry, MenuNode parent);

        public virtual void DoSettings(WidgetRow row, Rect rect, ref float curY) {
            var y = row.FinalY + Settings.IconStep;
            if (y > curY) curY = y;
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref onCopied, "onCopied");
            Scribe_Values.Look(ref onMoved,  "onMoved", true);
        }
    }
}
