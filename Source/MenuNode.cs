using FloatSubMenus;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus; 
public abstract class MenuNode {
    public static MenuNode Root() => new RootNode();

    public virtual void Add(BillMenuEntry entry) { }

    public void AddRange(IEnumerable<BillMenuEntry> entries) {
        foreach (var entry in entries) {
            Add(entry);
        }
    }

    public virtual string Label => null;

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

    protected virtual int TotalCount => 1;

    private abstract class NonLeaf : MenuNode {
        protected readonly List<MenuNode> children = [];
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

        protected override int TotalCount => children.Sum(x => x.TotalCount);

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

    private class Leaf(FloatMenuOption opt) : MenuNode {
        private readonly FloatMenuOption opt = opt;

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

        public override string Label => cat.LabelCap;

        protected override FloatMenuOption Option => 
            new FloatSubMenu(Label, List, icon, Color.white);

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

        public override string Label => cat;

        protected override MenuNode CollapseTo 
            => canCollapse ? base.CollapseTo : this;

        protected override FloatMenuOption Option 
            => new FloatSubMenu(cat, List, icon, Color.white);

        protected override bool Match(string cat) 
            => cat == this.cat;
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

        public override List<FloatMenuOption> List {
            get {
                NonLeaf root = this;
                List<FloatMenuOption> res;
                if (children.Count == 1 && children[0] is NonLeaf child) {
                    root = child;
                    res = child.List;
                } else {
                    res = base.List;
                }
                if (Settings.ShouldSearch(root.TotalCount)) {
                    res.Insert(0, new FloatMenuSearch(true));
                }
                return res;
            }
        }

        private MenuNode Fav => For(Strings.FavCat, Textures.FavIcon, 0, false);
    }
}
