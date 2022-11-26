using FloatSubMenus;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    public abstract class MenuNode {
        public static MenuNode Root() => new RootNode();

        public virtual void Add(RecipeDef recipe, FloatMenuOption opt) { }

        public virtual void Collapse() {}

        protected virtual MenuNode CollapseTo => this;

        protected virtual bool Match(ThingCategoryDef cat) => false;

        protected virtual bool Match(string cat) => false;

        protected virtual void UseEmptyIcon() { }

        protected virtual bool HasIcon => false;

        public virtual List<FloatMenuOption> List => null;

        protected virtual FloatMenuOption Option => null;

        protected virtual void Add(MenuNode n) { }

        protected virtual MenuNode For(ThingCategoryDef cat) => null;

        protected virtual MenuNode For(string cat, Texture2D icon = null, int pos = -1) => null;

        private abstract class NonLeaf : MenuNode {
            protected readonly List<MenuNode> children = new List<MenuNode>();
            protected Texture2D icon = null;

            public override List<FloatMenuOption> List => children.Select(c => c.Option).ToList();

            public override void Collapse() {
                bool useIcon = false;
                for (int i = 0; i < children.Count; i++) {
                    children[i].Collapse();
                    children[i] = children[i].CollapseTo;
                    useIcon |= children[i].HasIcon;
                }
                if (useIcon) {
                    children.ForEach(n => n.UseEmptyIcon());
                }
            }

            protected override MenuNode CollapseTo => 
                (children.Count == 1) ? children[0] : this;

            protected override bool HasIcon => icon != null && icon != BaseContent.ClearTex;

            protected override void UseEmptyIcon() => icon = icon ?? BaseContent.ClearTex;

            protected override void Add(MenuNode n) => children.Add(n);

            protected override MenuNode For(ThingCategoryDef cat) => 
                children.FirstOrDefault(n => n.Match(cat)) ?? new ThingCategory(cat, children);

            protected override MenuNode For(string cat, Texture2D icon = null, int pos = -1) => 
                children.FirstOrDefault(n => n.Match(cat)) ?? new ExtraCategory(cat, icon, children, pos);
        }

        private class Leaf : MenuNode {
            private readonly FloatMenuOption opt;

            public Leaf(FloatMenuOption opt) {
                this.opt = opt;
            }

            protected override bool HasIcon => true;

            protected override FloatMenuOption Option => opt;
        }

        private class ThingCategory : NonLeaf {
            private readonly ThingCategoryDef cat;

            public ThingCategory(ThingCategoryDef cat, List<MenuNode> children) {
                this.cat = cat;
                if (cat.icon != BaseContent.BadTex) icon = cat.icon;
                children.Add(this);
            }

            protected override FloatMenuOption Option => 
                new FloatSubMenu(cat.LabelCap, List, icon, Color.white);

            protected override bool Match(ThingCategoryDef cat) => cat == this.cat;
        }

        private class ExtraCategory : NonLeaf {
            private readonly string cat;

            public ExtraCategory(string cat, Texture2D icon, List<MenuNode> children, int pos) {
                this.cat = cat;
                this.icon = icon;
                if (pos < 0) {
                    children.Add(this);
                } else {
                    children.Insert(pos, this);
                }
            }

            protected override MenuNode CollapseTo => this;

            protected override FloatMenuOption Option => 
                new FloatSubMenu(cat, List, icon, Color.white);

            protected override bool Match(string cat) => cat == this.cat;
        }

        private class RootNode : NonLeaf {
            public override void Add(RecipeDef recipe, FloatMenuOption opt) {
                var thing = recipe.ProducedThingDef;
                MenuNode n = this;
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

            private MenuNode Fav => For(Strings.FavCat, Textures.FavIcon, 0);
        }
    }
}
