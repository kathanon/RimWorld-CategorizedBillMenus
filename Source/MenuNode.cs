using FloatSubMenus;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    public abstract class MenuNode {
        public static MenuNode Root() => new RootNode();

        public virtual void Add(BillMenuEntry entry) { }

        public void AddRange(IEnumerable<BillMenuEntry> entries) {
            foreach (var entry in entries) {
                Add(entry);
            }
        }

        public virtual void Collapse() {}

        public virtual List<FloatMenuOption> List => null;

        public virtual void Add(MenuNode n) { }

        public virtual MenuNode For(ThingCategoryDef cat) => null;

        public virtual MenuNode For(string cat,
                                    Texture2D icon = null,
                                    int pos = -1,
                                    bool canCollapse = true)
            => null;

        protected virtual MenuNode CollapseTo => this;

        protected virtual bool Match(ThingCategoryDef cat) => false;

        protected virtual bool Match(string cat) => false;

        protected virtual void UseEmptyIcon() { }

        protected virtual bool HasIcon => false;

        protected virtual FloatMenuOption Option => null;

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

            protected override MenuNode CollapseTo 
                => (children.Count == 1) ? children[0] : this;

            protected override bool HasIcon => icon != null && icon != BaseContent.ClearTex;

            protected override void UseEmptyIcon() => icon = icon ?? BaseContent.ClearTex;

            public override void Add(MenuNode n) => children.Add(n);

            public override MenuNode For(ThingCategoryDef cat) 
                => children.FirstOrDefault(n => n.Match(cat)) ?? new ThingCategory(cat, children);

            public override MenuNode For(string cat,
                                         Texture2D icon = null,
                                         int pos = -1,
                                         bool canCollapse = true)
                => children.FirstOrDefault(n => n.Match(cat)) 
                   ?? new ExtraCategory(cat, icon, children, pos, canCollapse);
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
            private readonly bool canCollapse;

            public ExtraCategory(string cat, Texture2D icon, List<MenuNode> children, int pos, bool canCollapse) {
                this.cat = cat;
                this.icon = icon;
                this.canCollapse = canCollapse;
                if (pos < 0) {
                    children.Add(this);
                } else {
                    children.Insert(pos, this);
                }
            }

            protected override MenuNode CollapseTo 
                => canCollapse ? base.CollapseTo : this;

            protected override FloatMenuOption Option 
                => new FloatSubMenu(cat, List, icon, Color.white);

            protected override bool Match(string cat) => cat == this.cat;
        }

        private class RootNode : NonLeaf {
            public override void Add(BillMenuEntry entry) {
                var recipe = entry.Recipe;
                var thing = recipe.ProducedThingDef;
                var nodes = new List<MenuNode> { this };
                var nodesNew = new List<MenuNode>();
                var leaf = new Leaf(entry.Option);
                bool first = true;
                foreach (var categorizer in Settings.Categorizers) {
                    if (categorizer.AppliesTo(entry, first)) {
                        nodesNew.AddRange(nodes.SelectMany(n => categorizer.Apply(entry, n, this, first)));
                        nodes.Clear();
                        (nodes, nodesNew) = (nodesNew, nodes);
                        first = false;
                    }
                }
                foreach (var n in nodes) {
                    n.Add(leaf);
                }

                if (Settings.IsFav(recipe)) {
                    Fav.Add(leaf);
                }
            }

            public override List<FloatMenuOption> List 
                => (children.Count == 1 && children[0] is NonLeaf) ? children[0].List : base.List;

            private MenuNode Fav => For(Strings.FavCat, Textures.FavIcon, 0, false);
        }
    }
}
