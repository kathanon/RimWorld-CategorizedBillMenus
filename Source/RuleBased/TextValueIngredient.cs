using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class TextValueIngredient : TextValueDefs<ThingDef> {
        private const string ValueIngredientName = "ingredient (fixed)";
        private const string ValueIngredientDesc = "Compare with the fixed ingredients.";

        static TextValueIngredient() {
            Register(new TextValueIngredient());
        }

        public TextValueIngredient() : base(ValueIngredientName, ValueIngredientDesc) { }

        public TextValueIngredient(int combinerIndex, int getterIndex)
            : base(ValueIngredientName, ValueIngredientDesc, combinerIndex, getterIndex) { }

        private TextValueIngredient(float _) : base(ValueIngredientName, ValueIngredientDesc, 0f) {}

        public override TextValue Copy() => CopyTo(new TextValueIngredient(0f));

        protected override IEnumerable<ThingDef> GetDefs(BillMenuEntry entry) 
            => entry.Recipe.ingredients.Where(i => i.IsFixedIngredient).Select(i => i.FixedIngredient);
    }


    [StaticConstructorOnStartup]
    public class ComparisonValueIngredientAll : TextValueDefs<ThingDef> {
        private const string ValueIngredientName = "ingredient (all)";
        private const string ValueIngredientDesc = "Compare with all ingredients, including every alternative.";

        static ComparisonValueIngredientAll() {
            Register(new ComparisonValueIngredientAll());
        }

        public ComparisonValueIngredientAll() : base(ValueIngredientName, ValueIngredientDesc) { }

        private ComparisonValueIngredientAll(int _) : base(ValueIngredientName, ValueIngredientDesc, getterIndex: 0) { }

        public override TextValue Copy() => CopyTo(new ComparisonValueIngredientAll(0));

        protected override IEnumerable<ThingDef> GetDefs(BillMenuEntry entry) 
            => entry.Recipe.ingredients.SelectMany(i => i.filter.AllowedThingDefs);
    }
}
