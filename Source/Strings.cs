﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CategorizedBillMenus {
    public static class Strings {
        public const string ID   = "kathanon.CategorizedBillMenus";
        public const string Name = "Categorized Bill Menus";

        public const string SubMenusTitle = "Sub-menu setup";
        public const string SubMenusTooltip = "Configure what sub-menus are generated";
        public const string CategoriesTitle = "Included categories";
        public const string CategoriesTooltip = "Uncheck any categories that should not be used in menus."; //TODO: improve
        public const string OpenTooltip = "Collapse section";
        public const string ClosedTooltip = "Expand section";
        public const string CollapseOption = "Collapse sub-menus with only one item";
        public const string NoBodyPartCat = "Whole body";
        public const string RuleBasedName = "Rule-based";
        public const string RuleBasedDesc = "Applies an editable list of simple rules.";
        public const string ByBodyPartName = "Medical - By Body Part";
        public const string ByBodyPartDesc = "Organizes operation bills after what body part they apply to.";
        public const string ManualName = "Manual";
        public const string ManualDesc = "Freely reorder recipies and categories, as well as add new categories.";
        public const string DefaultName = "Thing categories";
        public const string DefaultDesc = "Place each item in a sub-menu corresponding to the first thing category it belongs to.";
        public const string ActionByLimbName = "by body part";
        public const string ActionByLimbDesc = "Sort operations based on the target body part";

        public const string CondSurgeryName = "is operation";
        public const string CondSurgeryDesc = "Matches all operation recipes";
        public const string ActionByResearchName = "by research";
        public const string ActionByResearchDesc = "Sort by required research. If there are several, uses the first.";
        public const string CondOrName = "any of";
        public const string CondOrDesc = "Matches if any of the contained conditions matches.";
        public const string CondAndName = "all of";
        public const string CondAndDesc = "Matches if all of the contained conditions matches.";

        public static readonly string FavCat = "Favorites";
    }
}
