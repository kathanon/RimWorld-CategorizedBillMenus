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

        private ComparisonValueRecipe(int _) : base(ValueRecipeName, ValueRecipeDesc, 0) {}

        public override ComparisonValue Copy() => CopyTo(new ComparisonValueRecipe(0));

        protected override RecipeDef GetDef(BillMenuEntry entry) => entry.Recipe;
    }
}
