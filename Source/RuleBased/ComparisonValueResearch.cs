using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class ComparisonValueResearch : ComparisonValueDefs<ResearchProjectDef> {
        private const string ValueResearchName = "requires research";
        private const string ValueResearchDesc = "Compare with research projects required by the recipe.";

        static ComparisonValueResearch() {
            Register(new ComparisonValueResearch());
        }

        public ComparisonValueResearch() 
            : base(ValueResearchName, ValueResearchDesc) { }

        public ComparisonValueResearch(int combinerIndex, int getterIndex)
            : base(ValueResearchName, ValueResearchDesc, combinerIndex, getterIndex) { }

        private ComparisonValueResearch(float _) 
            : base(ValueResearchName, ValueResearchDesc, 0f) {}

        public override ComparisonValue Copy() => CopyTo(new ComparisonValueResearch(0));
        protected override IEnumerable<ResearchProjectDef> GetDefs(BillMenuEntry entry) {
            var def = entry.Recipe.researchPrerequisite;
            if (def != null) yield return def;
            var defs = entry.Recipe.researchPrerequisites ?? Enumerable.Empty<ResearchProjectDef>();
            foreach (var def2 in defs) {
                yield return def2;
            }
        }
    }
}
