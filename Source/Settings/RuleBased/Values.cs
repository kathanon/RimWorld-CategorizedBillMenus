using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public static class Values {
        static Values() {
            Register(ProductSingle);
            Register(Product);
            Register(Ingredient);
            Register(IngredientAll);
            Register(Research);
            Register(Category);
        }

        private static void Register<T>(Func<int, IDefValueGetter<T>> getter) where T : Def
            => TextValueDef<T>.RegisterGetter(() => getter(0));

        private static void Register<T>(Func<int, int, IDefValueGetter<T>> getter) where T : Def
            => TextValueDef<T>.RegisterGetter(() => getter(0, 0));

        private static TextValue WithGetter<T>(TextValueDef<T> value, IDefValueGetter<T> getter)
            where T : Def {
            value.Setup();
            value.SetGetter(getter);
            return value;
        }

        private static IDefValueGetter<TPrev> WithGetter<TPrev,TNext>(
                ITextValueDefChained<TPrev, TNext> value, IDefValueGetter<TNext> getter)
                where TPrev : Def 
                where TNext : Def {
            value.Setup();
            value.SetGetter(getter);
            return value;
        }


        // ===== Top level =====

        // Recipe

        public static TextValue Recipe(int getter = 0)
            => RecipeImpl(getter);

        public static TextValue Recipe(IDefValueGetter<RecipeDef> getter)
            => WithGetter(RecipeImpl(0), getter);

        private static TextValueDefConcrete<RecipeDef> RecipeImpl(int getter = 0)
            => new TextValueDefConcrete<RecipeDef>(
                Strings.ValueRecipeName, Strings.ValueRecipeID, Strings.ValueRecipeDesc,
                e => e.Recipe, getter);


        // Affected limb

        public static TextValue Limb(int getter = 0)
            => LimbImpl(getter);

        public static TextValue Limb(IDefValueGetter<BodyPartDef> getter)
            => WithGetter(LimbImpl(0), getter);

        private static TextValueDefConcrete<BodyPartDef> LimbImpl(int getter = 0)
            => new TextValueDefConcrete<BodyPartDef>(
                Strings.ValueLimbName, Strings.ValueLimbID, Strings.ValueLimbDesc,
                e => e.BodyPart.def, getter);


        // ===== From RecipeDef =====

        // Single product

        public static IDefValueGetter<RecipeDef> ProductSingle(int getter = 0)
            => ProductSingleImpl(getter);

        public static IDefValueGetter<RecipeDef> ProductSingle(IDefValueGetter<ThingDef> getter)
            => WithGetter((ITextValueDefChained<RecipeDef, ThingDef>) ProductSingleImpl(0), getter);

        private static TextValueDefChainedConcrete<RecipeDef, ThingDef> ProductSingleImpl(int getter)
             => new TextValueDefChainedConcrete<RecipeDef, ThingDef>(
                 Strings.ValueSingleProductName, Strings.ValueSingleProductID, Strings.ValueSingleProductDesc,
                 def => def.ProducedThingDef, getter);


        // All products

        public static IDefValueGetter<RecipeDef> Product(int getter = 0, int combiner = 0)
            => ProductImpl(getter, combiner);

        public static IDefValueGetter<RecipeDef> Product(IDefValueGetter<ThingDef> getter, int combiner = 0)
            => WithGetter((ITextValueDefChained<RecipeDef, ThingDef>) ProductImpl(0, combiner), getter);

        private static TextValueDefsChainedConcrete<RecipeDef, ThingDef> ProductImpl(int getter, int combiner)
            => new TextValueDefsChainedConcrete<RecipeDef, ThingDef>(
                Strings.ValueProductName, Strings.ValueProductID, Strings.ValueProductDesc,
                def => def.products.Select(x => x.thingDef), getter, combiner);


        // Fixed ingredients

        public static IDefValueGetter<RecipeDef> Ingredient(int getter = 0, int combiner = 0)
            => IngredientImpl(getter, combiner);

        public static IDefValueGetter<RecipeDef> Ingredient(IDefValueGetter<ThingDef> getter, int combiner = 0)
            => WithGetter((ITextValueDefChained<RecipeDef, ThingDef>) IngredientImpl(0, combiner), getter);

        private static TextValueDefsChainedConcrete<RecipeDef, ThingDef> IngredientImpl(int getter, int combiner) 
            => new TextValueDefsChainedConcrete<RecipeDef, ThingDef>(
                Strings.ValueIngredientFixedName, Strings.ValueIngredientFixedID, Strings.ValueIngredientFixedDesc,
                def => def.ingredients.Where(i => i.IsFixedIngredient).Select(i => i.FixedIngredient),
                getter, combiner);


        // All ingredients

        public static IDefValueGetter<RecipeDef> IngredientAll(int getter = 0, int combiner = 0)
            => IngredientAllImpl(getter, combiner);

        public static IDefValueGetter<RecipeDef> IngredientAll(IDefValueGetter<ThingDef> getter, int combiner = 0)
            => WithGetter((ITextValueDefChained<RecipeDef, ThingDef>) IngredientAllImpl(0, combiner), getter);

        private static TextValueDefsChainedConcrete<RecipeDef, ThingDef> IngredientAllImpl(int getter, int combiner)
            => new TextValueDefsChainedConcrete<RecipeDef, ThingDef>(
                Strings.ValueIngredientAllName, Strings.ValueIngredientAllID, Strings.ValueIngredientAllDesc,
                def => def.ingredients.SelectMany(i => i.filter.AllowedThingDefs),
                getter, combiner);


        // Required research

        public static IDefValueGetter<RecipeDef> Research(int getter = 0, int combiner = 0)
            => ResearchImpl(getter, combiner);

        public static IDefValueGetter<RecipeDef> Research(IDefValueGetter<ResearchProjectDef> getter, int combiner = 0)
            => WithGetter((ITextValueDefChained<RecipeDef, ResearchProjectDef>) ResearchImpl(0, combiner), getter);

        private static TextValueDefsChainedConcrete<RecipeDef, ResearchProjectDef> ResearchImpl(int getter, int combiner)
            => new TextValueDefsChainedConcrete<RecipeDef, ResearchProjectDef>(
                Strings.ValueResearchName, Strings.ValueResearchID, Strings.ValueResearchDesc,
                GetResearchDefs, getter, combiner);

        private static IEnumerable<ResearchProjectDef> GetResearchDefs(RecipeDef recipe) {
            var def = recipe.researchPrerequisite;
            if (def != null) yield return def;
            var defs = recipe.researchPrerequisites ?? Enumerable.Empty<ResearchProjectDef>();
            foreach (var def2 in defs) {
                yield return def2;
            }
        }


        // ===== From ThingDef =====

        // Thing category (just take first)

        public static IDefValueGetter<ThingDef> Category(int getter = 0)
            => CategoryImpl(getter);

        public static IDefValueGetter<ThingDef> Category(IDefValueGetter<ThingCategoryDef> getter)
            => WithGetter((ITextValueDefChained<ThingDef, ThingCategoryDef>) CategoryImpl(0), getter);

        private static TextValueDefChainedConcrete<ThingDef, ThingCategoryDef> CategoryImpl(int getter)
             => new TextValueDefChainedConcrete<ThingDef, ThingCategoryDef>(
                 Strings.ValueCategoryName, Strings.ValueCategoryID, Strings.ValueCategoryDesc,
                 def => def.FirstThingCategory, getter);
    }
}
