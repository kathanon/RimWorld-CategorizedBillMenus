using RimWorld;
using Verse;

namespace CategorizedBillMenus;
[DefOf]
public class MyDefOf {
    public static ThingCategoryDef BodyPartsBionic;

    public static ThingCategoryDef BodyPartsArchotech;

    public static ThingCategoryDef BodyPartsProsthetic;

    static MyDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(MyDefOf));
    }
}
