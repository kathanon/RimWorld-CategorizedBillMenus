using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class TextValueRecipe : TextValueDef<RecipeDef> {
        private const string ValueRecipeName = "recipe";
        private const string ValueRecipeDesc = "Compare with the recipe.";

        static TextValueRecipe() {
            Register(new TextValueRecipe());
        }

        public TextValueRecipe() : base(ValueRecipeName, ValueRecipeDesc) {}

        public TextValueRecipe(int getterIndex)
            : base(ValueRecipeName, ValueRecipeDesc, getterIndex) { }

        private TextValueRecipe(float _) : base(ValueRecipeName, ValueRecipeDesc, 0f) {}

        public override TextValue Copy() => CopyTo(new TextValueRecipe(0f));

        protected override RecipeDef GetDef(BillMenuEntry entry) => entry.Recipe;
    }
}
