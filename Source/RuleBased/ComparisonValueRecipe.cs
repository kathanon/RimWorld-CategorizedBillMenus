using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class ComparisonValueRecipe : ComparisonValueDef<RecipeDef> {
        private const string ValueRecipeName = "recipe";
        private const string ValueRecipeDesc = "Compare with the recipe.";

        static ComparisonValueRecipe() {
            Register(new ComparisonValueRecipe());
        }

        public ComparisonValueRecipe() : base(ValueRecipeName, ValueRecipeDesc) {}

        public ComparisonValueRecipe(int getterIndex)
            : base(ValueRecipeName, ValueRecipeDesc, getterIndex) { }

        private ComparisonValueRecipe(float _) : base(ValueRecipeName, ValueRecipeDesc, 0f) {}

        public override ComparisonValue Copy() => CopyTo(new ComparisonValueRecipe(0f));

        protected override RecipeDef GetDef(BillMenuEntry entry) => entry.Recipe;
    }
}
