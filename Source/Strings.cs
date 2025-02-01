using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    public static class Strings {
        public const string ID   = "kathanon.CategorizedBillMenus";
        public const string Name = "Categorized Bill Menus";

        // Vanilla strings
        public static readonly string Administer = "RecipeAdminister".Translate("").Trim();

        // Identifiers for saving and determining "same-ness"
        public const string ValueLabelID           = "label";
        public const string ValueDefNameID         = "def";
        public const string ValueModID             = "mod";
        public const string ValueRecipeID          = "recipe";
        public const string ValueLimbID            = "limb";
        public const string ValueResearchID        = "research";
        public const string ValueSingleProductID   = "productSingle";
        public const string ValueProductID         = "productAll";
        public const string ValueIngredientFixedID = "ingredientFixed";
        public const string ValueIngredientAllID   = "ingredientAll";
        public const string ValueCategoryID        = "category";

        public const string CondTextID    = "text";
        public const string CondSurgeryID = "surgery";
        public const string CondOrID      = "or";
        public const string CondAndID     = "and";
        public const string CondNotID     = "not";

        public const string ActionByLimbID     = "byLimb";
        public const string ActionByResearchID = "byResearch";
        public const string ActionByModID      = "byMod";
        public const string ActionNamedID      = "named";
        public const string ActionNoopID       = "noop";

        // Strings.xml
        public static readonly string FavCat        = (ID + ".FavCat"       ).Translate();
        public static readonly string NoBodyPartCat = (ID + ".NoBodyPartCat").Translate();

        // Settings.xml
        public static readonly string GeneralTitle      = (ID + ".GeneralTitle"     ).Translate();
        public static readonly string GeneralTooltip    = (ID + ".GeneralTooltip"   ).Translate();
        public static readonly string SubMenusTitle     = (ID + ".SubMenusTitle"    ).Translate();
        public static readonly string SubMenusTooltip   = (ID + ".SubMenusTooltip"  ).Translate();
        public static readonly string CategoriesTitle   = (ID + ".CategoriesTitle"  ).Translate();
        public static readonly string CategoriesTooltip = (ID + ".CategoriesTooltip").Translate();
        public static readonly string OpenTooltip       = (ID + ".OpenTooltip"      ).Translate();
        public static readonly string ClosedTooltip     = (ID + ".ClosedTooltip"    ).Translate();
        public static readonly string CollapseOption    = (ID + ".CollapseOption"   ).Translate();
        public static readonly string RightAlignOption  = (ID + ".RightAlignOption" ).Translate();
        public static readonly string SearchOption      = (ID + ".SearchOption"     ).Translate();
        public static readonly string NoneSelectedLabel = (ID + ".NoneSelectedLabel").Translate();

        public static readonly string RuleBasedName  = (ID + ".RuleBasedName" ).Translate();
        public static readonly string RuleBasedDesc  = (ID + ".RuleBasedDesc" ).Translate();
        public static readonly string ByLimbName     = (ID + ".ByLimbName"    ).Translate();
        public static readonly string ByLimbDesc     = (ID + ".ByLimbDesc"    ).Translate();
        public static readonly string ByTypeName     = (ID + ".ByTypeName"    ).Translate();
        public static readonly string ByTypeDesc     = (ID + ".ByTypeDesc"    ).Translate();
        public static readonly string ManualName     = (ID + ".ManualName"    ).Translate();
        public static readonly string ManualDesc     = (ID + ".ManualDesc"    ).Translate();
        public static readonly string CategoriesName = (ID + ".CategoriesName").Translate();
        public static readonly string CategoriesDesc = (ID + ".CategoriesDesc").Translate();

        private const string SearchIfOptionKey = ID + ".SearchIfOption";
        public static string SearchIfOption(int n) => SearchIfOptionKey.Translate(n);


        // Rules.xml
        public static readonly string ActionByLimbName         = (ID + ".ActionByLimbName"        ).Translate();
        public static readonly string ActionByLimbDesc         = (ID + ".ActionByLimbDesc"        ).Translate();
        public static readonly string ActionByLimbPlain        = (ID + ".ActionByLimbPlain"       ).Translate();
        public static readonly string ActionByLimbPlainDesc    = (ID + ".ActionByLimbPlainDesc"   ).Translate();
        public static readonly string ActionByLimbAnnotate     = (ID + ".ActionByLimbAnnotate"    ).Translate();
        public static readonly string ActionByLimbAnnotateDesc = (ID + ".ActionByLimbAnnotateDesc").Translate();
        public static readonly string ActionNamedName          = (ID + ".ActionNamedName"         ).Translate();
        public static readonly string ActionNamedDesc          = (ID + ".ActionNamedDesc"         ).Translate();
        public static readonly string ActionByResearchName     = (ID + ".ActionByResearchName"    ).Translate();
        public static readonly string ActionByResearchDesc     = (ID + ".ActionByResearchDesc"    ).Translate();
        public static readonly string ActionByModName          = (ID + ".ActionByModName"         ).Translate();
        public static readonly string ActionByModDesc          = (ID + ".ActionByModDesc"         ).Translate();
        public static readonly string ActionNoopName           = (ID + ".ActionNoopName"          ).Translate();
        public static readonly string ActionNoopDesc           = (ID + ".ActionNoopDesc"          ).Translate();
        public static readonly string ActionNoopStop           = (ID + ".ActionNoopStop"          ).Translate();

        public static readonly string CondSurgeryName = (ID + ".CondSurgeryName").Translate();
        public static readonly string CondSurgeryDesc = (ID + ".CondSurgeryDesc").Translate();
        public static readonly string CondOrName      = (ID + ".CondOrName"     ).Translate();
        public static readonly string CondOrDesc      = (ID + ".CondOrDesc"     ).Translate();
        public static readonly string CondAndName     = (ID + ".CondAndName"    ).Translate();
        public static readonly string CondAndDesc     = (ID + ".CondAndDesc"    ).Translate();
        public static readonly string CondNotName     = (ID + ".CondNotName"    ).Translate();
        public static readonly string CondNotDesc     = (ID + ".CondNotDesc"    ).Translate();
        public static readonly string CondTextName    = (ID + ".CondTextName"   ).Translate();
        public static readonly string CondTextDesc    = (ID + ".CondTextDesc"   ).Translate();

        public static readonly string ValueLabelName           = (ID + ".ValueLabelName"          ).Translate();
        public static readonly string ValueDefNameName         = (ID + ".ValueDefNameName"        ).Translate();
        public static readonly string ValueModName             = (ID + ".ValueModName"            ).Translate();
        public static readonly string ValueResearchName        = (ID + ".ValueResearchName"       ).Translate();
        public static readonly string ValueResearchDesc        = (ID + ".ValueResearchDesc"       ).Translate();
        public static readonly string ValueSingleProductName   = (ID + ".ValueSingleProductName"  ).Translate();
        public static readonly string ValueSingleProductDesc   = (ID + ".ValueSingleProductDesc"  ).Translate();
        public static readonly string ValueProductName         = (ID + ".ValueProductName"        ).Translate();
        public static readonly string ValueProductDesc         = (ID + ".ValueProductDesc"        ).Translate();
        public static readonly string ValueIngredientFixedName = (ID + ".ValueIngredientFixedName").Translate();
        public static readonly string ValueIngredientFixedDesc = (ID + ".ValueIngredientFixedDesc").Translate();
        public static readonly string ValueIngredientAllName   = (ID + ".ValueIngredientAllName"  ).Translate();
        public static readonly string ValueIngredientAllDesc   = (ID + ".ValueIngredientAllDesc"  ).Translate();
        public static readonly string ValueCategoryName        = (ID + ".ValueCategoryName"       ).Translate();
        public static readonly string ValueCategoryDesc        = (ID + ".ValueCategoryDesc"       ).Translate();
        public static readonly string ValueRecipeName          = (ID + ".ValueRecipeName"         ).Translate();
        public static readonly string ValueRecipeDesc          = (ID + ".ValueRecipeDesc"         ).Translate();
        public static readonly string ValueLimbName            = (ID + ".ValueLimbName"           ).Translate();
        public static readonly string ValueLimbDesc            = (ID + ".ValueLimbDesc"           ).Translate();

        public static readonly string PresetPrimitive = (ID + ".PresetPrimitive").Translate();
        public static readonly string PresetDrugs     = (ID + ".PresetDrugs"    ).Translate();
        public static readonly string PresetFertility = (ID + ".PresetFertility").Translate();
        public static readonly string PresetAmputate  = (ID + ".PresetAmputate" ).Translate();

        public static readonly string ConditionPrefix   = (ID + ".ConditionPrefix"  ).Translate();
        public static readonly string ActionPrefix      = (ID + ".ActionPrefix"     ).Translate();
        public static readonly string AllowAfterNoTips  = (ID + ".AllowAfterNoTips" ).Translate();
        public static readonly string AllowAfterYesTips = (ID + ".AllowAfterYesTips").Translate();
        public static readonly string CopyNoTip         = (ID + ".CopyNoTip"        ).Translate();
        public static readonly string CopyYesTip        = (ID + ".CopyYesTip"       ).Translate();
        public static readonly string OnCopiedNoTip     = (ID + ".OnCopiedNoTip"    ).Translate();
        public static readonly string OnCopiedYesTip    = (ID + ".OnCopiedYesTip"   ).Translate();
        public static readonly string OnMovedNoTip      = (ID + ".OnMovedNoTip"     ).Translate();
        public static readonly string OnMovedYesTip     = (ID + ".OnMovedYesTip"    ).Translate();

        // Parameterized
        private const string ActionByLimbAnnotatePatternID = ID + ".ActionByLimbAnnotatePattern";
        public static string ActionByLimbAnnotatePattern(string limb, string annotation) 
            => ActionByLimbAnnotatePatternID.Translate(limb, annotation);
    }
}
