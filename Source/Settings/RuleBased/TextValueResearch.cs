using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class TextValueResearch : TextValueDefs<ResearchProjectDef> {
        static TextValueResearch() {
            Register(new TextValueResearch());
        }

        public TextValueResearch() 
            : base(Strings.ValueResearchName, Strings.ValueResearchDesc) { }

        public TextValueResearch(int combinerIndex, int getterIndex)
            : base(Strings.ValueResearchName, Strings.ValueResearchDesc, combinerIndex, getterIndex) { }

        private TextValueResearch(float _) 
            : base(Strings.ValueResearchName, Strings.ValueResearchDesc, 0f) {}

        public override TextValue Copy() => CopyTo(new TextValueResearch(0));
        protected override IEnumerable<ResearchProjectDef> GetDefs(BillMenuEntry entry) {
            var def = entry.Recipe.researchPrerequisite;
            if (def != null) yield return def;
            var defs = entry.Recipe.researchPrerequisites ?? Enumerable.Empty<ResearchProjectDef>();
            foreach (var def2 in defs) {
                yield return def2;
            }
        }
    }//CategorizedBillMenus.Strings
}
