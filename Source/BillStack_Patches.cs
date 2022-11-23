using FloatSubMenus;
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
        private const float IconSize = 24f;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BillStack), nameof(BillStack.DoListing))]
        public static void DoListing(ref Func<List<FloatMenuOption>> recipeOptionsMaker) {
            var local = recipeOptionsMaker;
            recipeOptionsMaker = () => MakeSubmenus(local());
        }

        private static List<FloatMenuOption> MakeSubmenus(List<FloatMenuOption> options) {
            if (Find.Selector.SingleSelectedThing is Building_WorkTable table) {
                var list = new List<FloatMenuOption>();

                // Find what recipes match with what menu option
                var matchedRecipes = new List<RecipeDef>();
                var ideos = Faction.OfPlayer.ideos.AllIdeos;
                var recipes = table.def.AllRecipes.Where(r => r.AvailableNow && r.AvailableOnNow(table));
                foreach (var recipe in recipes) {
                    var produced = recipe.ProducedThingDef;
                    int n = 1 + ((produced == null) ? 0 : ideos.SelectMany(i => i.PreceptsFor(produced)).Count());
                    int j = matchedRecipes.Count();
                    for (int i = 0; i < n; i++) {
                        matchedRecipes.Add(recipe);
                    }
                    var opt = options[j];
                    opt.extraPartWidth += IconSize;
                    var func = opt.extraPartOnGUI;
                    opt.extraPartOnGUI = (r) => DrawFavIcon(r, recipe, func);
                    opt.extraPartRightJustified = true;
                }
                if (options.Count != matchedRecipes.Count) {
                    Log.Warning($"{Strings.Name}: Could not match recipes to bill menu options. Not altering menu.");
                    return options;
                }

                // Add to categories
                var root = Node.Root();
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
            var iconRect = new Rect(rect.x + 2f, rect.y + (rect.height - IconSize) / 2f, IconSize, IconSize);
            TooltipHandler.TipRegion(iconRect, "Favorite");
            if (Widgets.ButtonImage(iconRect, Textures.FavIcon, color, mouseColor)) {
                Settings.ToggleFav(recipe);
            }
            rect.xMin += IconSize - 3f;
            return original(rect);
        }

        private abstract class Node {
            public static Node Root() => new RootNode();

            public virtual void Add(RecipeDef recipe, FloatMenuOption opt) { }

            public virtual void Collapse() {}

            protected virtual Node CollapseTo => this;

            protected virtual bool Match(ThingCategoryDef cat) => false;

            protected virtual bool Match(string cat) => false;

            public virtual List<FloatMenuOption> List => null;

            protected virtual FloatMenuOption Option => null;

            protected virtual void Add(Node n) { }

            protected virtual Node For(ThingCategoryDef cat) => null;

            protected virtual Node For(string cat, int pos = -1) => null;

            private abstract class NonLeaf : Node {
                protected readonly List<Node> children = new List<Node>();

                public override List<FloatMenuOption> List => children.Select(c => c.Option).ToList();

                public override void Collapse() {
                    for (int i = 0; i < children.Count; i++) {
                        children[i] = children[i].CollapseTo;
                        children[i].Collapse();
                    }
                }

                protected override Node CollapseTo => 
                    (children.Count == 1) ? children[0].CollapseTo : this;


                protected override void Add(Node n) => children.Add(n);

                protected override Node For(ThingCategoryDef cat) => 
                    children.FirstOrDefault(n => n.Match(cat)) ?? new ThingCategory(cat, children);

                protected override Node For(string cat, int pos = -1) => 
                    children.FirstOrDefault(n => n.Match(cat)) ?? new ExtraCategory(cat, children, pos);
            }

            private class Leaf : Node {
                private readonly FloatMenuOption opt;

                public Leaf(FloatMenuOption opt) {
                    this.opt = opt;
                }

                protected override FloatMenuOption Option => opt;
            }

            private class ThingCategory : NonLeaf {
                private readonly ThingCategoryDef cat;

                public ThingCategory(ThingCategoryDef cat, List<Node> children) {
                    this.cat = cat;
                    children.Add(this);
                }

                protected override FloatMenuOption Option => 
                    new FloatSubMenu(cat.LabelCap, List, Icon, Color.white);

                private Texture2D Icon => (cat.icon == BaseContent.BadTex) ? null : cat.icon;

                protected override bool Match(ThingCategoryDef cat) => cat == this.cat;
            }

            private class ExtraCategory : NonLeaf {
                private readonly string cat;

                public ExtraCategory(string cat, List<Node> children, int pos) {
                    this.cat = cat;
                    if (pos < 0) {
                        children.Add(this);
                    } else {
                        children.Insert(pos, this);
                    }
                }

                protected override Node CollapseTo => this;

                protected override FloatMenuOption Option => new FloatSubMenu(cat, List);

                protected override bool Match(string cat) => cat == this.cat;
            }

            private class RootNode : NonLeaf {
                public override void Add(RecipeDef recipe, FloatMenuOption opt) {
                    var thing = recipe.ProducedThingDef;
                    Node n = this;
                    if (thing != null && !thing.thingCategories.NullOrEmpty()) {
                        var category = thing.thingCategories[0];
                        foreach (var cat in category.Parents.Reverse().AddItem(category)) {
                            if (!Settings.IsDisabled(cat)) {
                                n = n.For(cat);
                            }
                        }
                    }

                    var leaf = new Leaf(opt);
                    n.Add(leaf);
                    if (Settings.IsFav(recipe)) {
                        Fav.Add(leaf);
                    }
                }

                public override List<FloatMenuOption> List => 
                    (children.Count == 1 && children[0] is NonLeaf) ? children[0].List : base.List;

                private Node Fav => For(Strings.FavCat, 0);
            }
        }

        public static IEnumerable<Precept_Building> PreceptsFor(this Ideo ideo, ThingDef def) => 
            ideo.cachedPossibleBuildings.Where(p => p.ThingDef == def);
    }
}
