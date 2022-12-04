using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    public abstract class RuleAction : Registerable<RuleAction>, ISettingsEntry {
        private bool copies;

        protected RuleAction(bool copies, string name, string description) 
            : base(name, description) {
            this.copies = copies;
        }

        public bool Copies => copies;

        public abstract MenuNode Apply(BillMenuEntry entry, MenuNode parent, MenuNode root);

        public virtual void DoSettings(WidgetRow row) {}

        public virtual void DoSettings(WidgetRow row, Rect rect, ref float curY) => DoSettings(row);

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref copies, "copies");
        }
    }
}
