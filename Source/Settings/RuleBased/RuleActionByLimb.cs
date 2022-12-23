using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class RuleActionByLimb : RuleActionExtra {
        static RuleActionByLimb() {
            Register(new RuleActionByLimb());
        }

        private string noPart = Strings.NoBodyPartCat;
        private bool firstPartDef = true;

        public RuleActionByLimb() : this(false) {}

        private RuleActionByLimb(bool copies) 
            : base(copies, Strings.ActionByLimbName, Strings.ActionByLimbID, Strings.ActionByLimbDesc) {}

        public override RuleAction Copy() 
            => new RuleActionByLimb(Copies) { noPart = noPart, firstPartDef = firstPartDef };

        protected override IEnumerable<string> Categories(BillMenuEntry entry) {
            var part = entry.BodyPart;
            if (part == null) {
                if (noPart.NullOrEmpty()) {
                    yield break;
                } else {
                    yield return noPart;
                }
            } else {
                if (part.body.GetPartsWithDef(part.def).Count > 1) {
                    yield return part.def.LabelCap;
                }
                yield return part.LabelCap;
            }
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref noPart, "noPart", Strings.NoBodyPartCat);
            Scribe_Values.Look(ref firstPartDef, "firstPartDef", true);
        }
    }
}
