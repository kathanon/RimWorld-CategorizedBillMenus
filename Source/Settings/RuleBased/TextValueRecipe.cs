using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class TextValueRecipe : TextValueDef<RecipeDef> {
        static TextValueRecipe() {
            Register(new TextValueRecipe());
        }

        public TextValueRecipe() : base(Strings.ValueRecipeName, Strings.ValueRecipeDesc) {}

        public TextValueRecipe(int getterIndex)
            : base(Strings.ValueRecipeName, Strings.ValueRecipeDesc, getterIndex) { }

        private TextValueRecipe(float _) : base(Strings.ValueRecipeName, Strings.ValueRecipeDesc, 0f) {}

        public override TextValue Copy() => CopyTo(new TextValueRecipe(0f));

        protected override RecipeDef GetDef(BillMenuEntry entry) => entry.Recipe;
    }
}
