using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class ComparisonValueIngredient : ComparisonValueDefs<ThingDef> {
        private const string ValueIngredientName = "ingredient (fixed)";
        private const string ValueIngredientDesc = "Compare with the fixed ingredients.";

        static ComparisonValueIngredient() {
            Register(new ComparisonValueIngredient());
        }

        public ComparisonValueIngredient() : base(ValueIngredientName, ValueIngredientDesc) {}

        private ComparisonValueIngredient(int _) : base(ValueIngredientName, ValueIngredientDesc, 0) {}

        public override ComparisonValue Copy() => CopyTo(new ComparisonValueIngredient(0));

        protected override IEnumerable<ThingDef> GetDefs(BillMenuEntry entry) 
            => entry.Recipe.ingredients.Where(i => i.IsFixedIngredient).Select(i => i.FixedIngredient);
    }


    [StaticConstructorOnStartup]
    public class ComparisonValueIngredientAll : ComparisonValueDefs<ThingDef> {
        private const string ValueIngredientName = "ingredient (all)";
        private const string ValueIngredientDesc = "Compare with all ingredients, including every alternative.";

        static ComparisonValueIngredientAll() {
            Register(new ComparisonValueIngredientAll());
        }

        public ComparisonValueIngredientAll() : base(ValueIngredientName, ValueIngredientDesc) {}

        private ComparisonValueIngredientAll(int _) : base(ValueIngredientName, ValueIngredientDesc, 0) {}

        public override ComparisonValue Copy() => CopyTo(new ComparisonValueIngredientAll(0));

        protected override IEnumerable<ThingDef> GetDefs(BillMenuEntry entry) 
            => entry.Recipe.ingredients.SelectMany(i => i.filter.AllowedThingDefs);
    }
}
