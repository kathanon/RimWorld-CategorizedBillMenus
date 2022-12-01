using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    public class RuleActionByLimb : RuleActionExtra {
        private string noPart = "General";
        private bool firstPartDef = true;

        public RuleActionByLimb() : base(false) {}

        public override RuleAction Copy() 
            => new RuleActionByLimb() { noPart = noPart, firstPartDef = firstPartDef };

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
    }
}
