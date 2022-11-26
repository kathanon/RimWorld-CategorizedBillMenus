using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    [HarmonyPatch]
    public static class BillStack_Patches {
        private const float IconSize   = 24f;
        private const float IconMargin =  5f;
        private const float IconAdjust =  2f;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BillStack), nameof(BillStack.DoListing))]
        public static void DoListing(ref Func<List<FloatMenuOption>> recipeOptionsMaker) {
            var local = recipeOptionsMaker;
            recipeOptionsMaker = () => MakeSubmenus(local());
        }

        private static List<FloatMenuOption> MakeSubmenus(List<FloatMenuOption> options) {
            if (Find.Selector.SingleSelectedThing is Building_WorkTable table) {
                var list = new List<FloatMenuOption>();
                bool useFav = Settings.UseFavorites;

                // Find what recipes match with what menu option
                var matchedRecipes = new List<RecipeDef>();
                var ideos = Faction.OfPlayer.ideos.AllIdeos;
                var recipes = table.def.AllRecipes.Where(r => r.AvailableNow && r.AvailableOnNow(table));
                int j = 0;
                foreach (var recipe in recipes) {
                    var produced = recipe.ProducedThingDef;
                    int n = 1 + ((produced == null) ? 0 : ideos.SelectMany(i => i.PreceptsFor(produced)).Count());
                    for (int i = 0; i < n; i++) {
                        matchedRecipes.Add(recipe);
                        var opt = options[j + i];
                        opt.extraPartRightJustified = true;
                        opt.extraPartWidth += IconAdjust;
                    }

                    if (useFav) {
                        var opt = options[j];
                        opt.extraPartWidth += IconSize - IconMargin;
                        var func = opt.extraPartOnGUI;
                        opt.extraPartOnGUI = (r) => DrawFavIcon(r, recipe, func);
                    }
                    j += n;
                }
                if (options.Count != matchedRecipes.Count) {
                    Log.Warning($"{Strings.Name}: Could not match recipes to bill menu options. Not altering menu.");
                    return options;
                }

                // Add to categories
                var root = MenuNode.Root();
                for (int i = 0; i < options.Count; i++) {
                    root.Add(matchedRecipes[i], options[i]);
                }
                root.Collapse();
                return root.List;
            } else { 
                return options;
            }
        }

        private static bool DrawFavIcon(Rect rect, RecipeDef recipe, Func<Rect,bool> original) {
            bool fav = Settings.IsFav(recipe);
            var color = fav ? Color.white : Color.grey;
            var mouseColor = fav ? GenUI.MouseoverColor : GenUI.SubtleMouseoverColor;
            var iconRect = new Rect(rect.x, rect.y + (rect.height - IconSize) / 2f, IconSize, IconSize);
            TooltipHandler.TipRegion(iconRect, "Favorite");
            if (Widgets.ButtonImage(iconRect, Textures.FavIcon, color, mouseColor)) {
                Settings.ToggleFav(recipe);
            }
            rect.xMin += IconSize - IconMargin;
            return original(rect);
        }

        public static IEnumerable<Precept_Building> PreceptsFor(this Ideo ideo, ThingDef def) => 
            ideo.cachedPossibleBuildings.Where(p => p.ThingDef == def);
    }
}
