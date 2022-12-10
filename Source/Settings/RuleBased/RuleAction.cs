using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public abstract class RuleAction : RulePart<RuleAction> {
        private static readonly string[] copiesTips 
            = { Strings.CopyNoTip, Strings.CopyYesTip };
        private static readonly Texture2D[] copiesIcons 
            = { TexButton.Paste, TexButton.Copy };

        private bool copies;

        protected RuleAction(bool copies, string name, string description) 
                : base(name, description) {
            this.copies = copies;
        }

        public bool Copies => copies;

        protected virtual bool FixedCopies => false;

        public override int NumToggles => FixedCopies ? 0 : 1;

        public override void CopyFrom(RuleAction from) {
            copies = from.copies;
        }

        public abstract MenuNode Apply(BillMenuEntry entry, MenuNode parent, MenuNode root);

        protected override void DoButtons(Rect icon) {
            if (!FixedCopies) {
                ExtraWidgets.ToggleButton(icon, ref copies, copiesIcons, copiesTips);
            }
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref copies, "copies");
        }

        private class NullAction : RuleAction {
            public NullAction() : base(false, "(select)", null) {}

            public override MenuNode Apply(BillMenuEntry entry, MenuNode parent, MenuNode root) => parent;
            public override RuleAction Copy() => this;
        }
    }
}
