using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class TextValueIngredient : TextValueDefs<ThingDef> {
        static TextValueIngredient() {
            Register(new TextValueIngredient());
        }

        public TextValueIngredient() : base(Strings.ValueIngredientFixedName, Strings.ValueIngredientFixedDesc) { }

        public TextValueIngredient(int combinerIndex, int getterIndex)
            : base(Strings.ValueIngredientFixedName, Strings.ValueIngredientFixedDesc, combinerIndex, getterIndex) { }

        private TextValueIngredient(float _) : base(Strings.ValueIngredientFixedName, Strings.ValueIngredientFixedDesc, 0f) {}

        public override TextValue Copy() => CopyTo(new TextValueIngredient(0f));

        protected override IEnumerable<ThingDef> GetDefs(BillMenuEntry entry) 
            => entry.Recipe.ingredients.Where(i => i.IsFixedIngredient).Select(i => i.FixedIngredient);
    }


    [StaticConstructorOnStartup]
    public class ComparisonValueIngredientAll : TextValueDefs<ThingDef> {
        static ComparisonValueIngredientAll() {
            Register(new ComparisonValueIngredientAll());
        }

        public ComparisonValueIngredientAll() : base(Strings.ValueIngredientAllName, Strings.ValueIngredientAllDesc) { }

        private ComparisonValueIngredientAll(int _) : base(Strings.ValueIngredientFixedName, Strings.ValueIngredientFixedDesc, getterIndex: 0) { }

        public override TextValue Copy() => CopyTo(new ComparisonValueIngredientAll(0));

        protected override IEnumerable<ThingDef> GetDefs(BillMenuEntry entry) 
            => entry.Recipe.ingredients.SelectMany(i => i.filter.AllowedThingDefs);
    }
}
