using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class RuleActionByResearch : RuleActionExtra {
        static RuleActionByResearch() {
            Register(new RuleActionByResearch());
        }

        public RuleActionByResearch() : this(false) {}

        private RuleActionByResearch(bool copies) 
            : base(copies, Strings.ActionByResearchName, Strings.ActionByResearchID, Strings.ActionByResearchDesc) {}

        public override RuleAction Copy() => new RuleActionByResearch(Copies);

        protected override IEnumerable<string> Categories(BillMenuEntry entry) {
            var research = entry.Recipe.researchPrerequisite 
                ?? entry.Recipe.researchPrerequisites?.FirstOrDefault();
            if (research != null) {
                yield return research.LabelCap;
            }
        }
    }
}
