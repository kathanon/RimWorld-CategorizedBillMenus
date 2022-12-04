using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    [HarmonyPatch]
    public static class BillStack_Patches {

        public static readonly RecipeCollector Collector = new RecipeCollector();

        private const float IconSize   = 24f;
        private const float IconMargin =  5f;
        private const float IconAdjust =  2f;

        private enum Generators {
            None,
            WorkTable,
            Operations,
            Length
        }

        private static readonly bool[] activeFlags = new bool[(int) Generators.Length];
        private static Generators currentFlag = Generators.None;

        private static bool CurrentActive {
            set => activeFlags[(int) currentFlag] = value;
        }

        private static bool Active(Generators gen) => activeFlags[(int) gen];

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BillStack), nameof(BillStack.DoListing))]
        public static void DoListing(ref Func<List<FloatMenuOption>> recipeOptionsMaker) {
            if (currentFlag != Generators.None) {
                var local = recipeOptionsMaker;
                recipeOptionsMaker = () => MakeSubmenus(local);
            }
        }


        // Work table

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ITab_Bills), "FillTab")]
        public static void ITab_Bills_Pre() => currentFlag = Generators.WorkTable;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ITab_Bills), "FillTab")]
        public static void ITab_Bills_Post() => currentFlag = Generators.None;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RecipeDef), nameof(RecipeDef.UIIconThing), MethodType.Getter)]
        public static void RecipeDef_UIIconThing(RecipeDef __instance) {
            if (Active(Generators.WorkTable)) Collector.NextRecipe = __instance;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FloatMenuOption), MethodType.Constructor, 
            typeof(string), typeof(Action), typeof(ThingDef), typeof(ThingStyleDef), typeof(bool), 
            typeof(MenuOptionPriority), typeof(Action<Rect>), typeof(Thing), typeof(float), 
            typeof(Func<Rect, bool>), typeof(WorldObject), typeof(bool), typeof(int), typeof(int?))]
        public static void FloatMenuOption_Ctor(FloatMenuOption __instance,
                                                ref bool ___drawPlaceHolderIcon,
                                                ref Texture2D ___itemIcon) {
            if (Active(Generators.WorkTable)) {
                Collector.Add(__instance);
            } else if (Active(Generators.Operations) && ___drawPlaceHolderIcon) {
                ___drawPlaceHolderIcon = false;
                ___itemIcon = BaseContent.ClearTex;
            }
        }

        // Operations

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HealthCardUtility), "DrawMedOperationsTab")]
        public static void HealthCardUtility_Pre() 
            => currentFlag = Generators.Operations;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HealthCardUtility), "DrawMedOperationsTab")]
        public static void HealthCardUtility_Post() 
            => currentFlag = Generators.None;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HealthCardUtility), "GenerateSurgeryOption")]
        public static void GenerateSurgeryOption(FloatMenuOption __result, RecipeDef recipe, BodyPartRecord part) {
            if (Active(Generators.Operations)) {
                Collector.Add(new BillMenuEntry(__result, recipe, part));
            }
        }

        public class RecipeCollector {
            public readonly List<BillMenuEntry> Entries = new List<BillMenuEntry>();
            public RecipeDef NextRecipe = null;

            public void Add(FloatMenuOption opt) => Entries.Add(new BillMenuEntry(opt, NextRecipe));

            public void Add(BillMenuEntry entry) => Entries.Add(entry);

            public void Reset() {
                Entries.Clear();
                NextRecipe = null;
            }
        }

        private static List<FloatMenuOption> MakeSubmenus(Func<List<FloatMenuOption>> optionsMaker) {
            List<FloatMenuOption> res = null;
            CurrentActive = true;
            var options = optionsMaker();
            CurrentActive = false;
            var entries = Collector.Entries;

            bool useFav = Settings.UseFavorites;
            bool rightAlign = Settings.RightAlign;
            if (useFav || rightAlign) {
                foreach (var entry in entries) {
                    var opt = entry.Option;
                    if (rightAlign) {
                        opt.extraPartRightJustified = true;
                        opt.extraPartWidth += IconAdjust;
                    }
                    if (useFav) {
                        opt.extraPartWidth += IconSize - IconMargin;
                        var func = opt.extraPartOnGUI;
                        opt.extraPartOnGUI = (r) => DrawFavIcon(r, entry.Recipe, func);
                    }
                }
            }

            if (entries.Count == 0) {
                res = options;
            } else {
                var root = MenuNode.Root();
                root.AddRange(entries);
                if (Settings.Collapse) root.Collapse();
                res = root.List;
            }

            Collector.Reset();
            return res;
        }

        private static bool DrawFavIcon(Rect rect, RecipeDef recipe, Func<Rect, bool> original) {
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
