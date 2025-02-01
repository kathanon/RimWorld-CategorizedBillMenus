using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static CategorizedBillMenus.CurrentModeShorthands;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class CategorizerManual : CategorizerEditable {
        private List<Entry> entries = [];

        private readonly Dictionary<RecipeDef, HashSet<Entry>> recipeLookup = [];
        private readonly Dictionary<ThingCategoryDef, Tree> catLookup = [];
        private Tree tree;

        static CategorizerManual() {
            // Disabled for now
            //Register(new CategorizerManual());
        }

        public CategorizerManual() 
            : base(Strings.ManualName, Strings.ManualDesc) {
            FromEntries();
            UpdateRecipeList();
        }

        private CategorizerManual(CategorizerManual other) : this() {
            entries.AddRange(other.entries.Select(x => x.Copy()));
        }

        public override bool AppliesTo(BillMenuEntry entry, bool first) 
            => recipeLookup.ContainsKey(entry.Recipe);

        public override IEnumerable<MenuNode> Apply(
                BillMenuEntry entry, MenuNode parent, MenuNode root, bool first) {
            if (recipeLookup.TryGetValue(entry.Recipe, out var list)) {
                foreach (var item in list) {
                    yield return item.Apply(root);
                }
            } else {
                yield return parent;
            }
        }

        public override Categorizer Copy() => new CategorizerManual(this);

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref entries, "entries", LookMode.Deep);
            if (PostLoadInit) FromEntries();
        }

        private void FromEntries() {
            tree = new TreeCat(ThingCategoryDefOf.Root, catLookup);
            foreach (var item in entries) {
                item.AddToTree(tree, catLookup, recipeLookup);
            }
        }


        // Settings UI

        public const float RecipesPart     =   0.35f;
        public const float MaxHeight       = 540f;
        public const float MinHeight       = 300f;
        public const float RecipesMinWidth = 200f;
        public const float RowHeight       =  20f;
        public const float RowMargin       =   4f;
        public const float IndentStep      =  30f;

        public const float Margin       = Settings.Margin;
        public const float ScrollMargin = Settings.ScrollMargin;

        public const float RowStep = RowHeight + RowMargin;

        private static readonly object recipeListDropTarget = new object();
        private static readonly Vector2 dragIconSize = new Vector2(24f, 24f);
        private static readonly Vector2 dragIconAdjust = new Vector2(30f, 30f);
        private static readonly Color transparent = new Color(1f, 1f, 1f, 0.4f);

        private float height = -1f;
        private float treeHeight = -1f;
        private int dragGroup = -1;
        private readonly QuickSearchWidget search = new QuickSearchWidget();
        private readonly List<RecipeDef> recipes = new List<RecipeDef>();
        private Vector2 recipeScroll;
        private Vector2 treeScroll;
        private static CategorizerManual current;
        private TreeExtra currentEdit;
        private object currentDrag;

        public override void DoSettings(Rect rect, ref float curY) {
            current = this;
            dragGroup = DragAndDropWidget.NewGroup();
            Text.WordWrap = false;
            Text.Anchor = TextAnchor.MiddleLeft;
            float startY = curY;

            float listWidth = Mathf.Max(rect.width * RecipesPart, RecipesMinWidth);
            var treeRect = new Rect(rect.x, startY, rect.width - listWidth - 2 * Margin, height);
            DoTree(treeRect, ref curY);
            if (Layout) {
                treeHeight = curY - startY;
                height = Mathf.Min(Mathf.Max(treeHeight, MinHeight), MaxHeight);
            }

            var listRect = new Rect(rect.xMax - listWidth, startY, listWidth, height);
            DoRecipeList(listRect);

            DoDragIndicator();

            curY = startY + height + Margin;
            GenUI.ResetLabelAlign();
            Text.WordWrap = true;
            current = null;
        }

        private void DoTree(Rect rect, ref float curY) {
            var view = rect;
            view.width -= ScrollMargin;
            view.height = treeHeight;
            bool scroll = !Layout && treeHeight > height;
            if (scroll) Widgets.BeginScrollView(rect, ref treeScroll, view);

            var row = new Rect(view.x, curY, view.width, RowHeight);
            tree.DoContents(ref row, dragGroup);
            curY = row.y;

            // TODO: Top level "+" button.

            Widgets.TextField(new Rect(rect.x - 100f, curY, 40f, 10f), "");
            if (scroll) Widgets.EndScrollView();
        }

        private void DoRecipeList(Rect rect) {
            var searchRect = rect.TopPartPixels(QuickSearchWidget.WidgetHeight);
            rect.yMin += QuickSearchWidget.WidgetHeight + Margin;
            search.OnGUI(searchRect, UpdateRecipeList);

            var view = rect;
            view.width -= ScrollMargin;
            view.height = recipes.Count * RowStep - RowMargin;
            bool scroll = view.height > rect.height;
            if (scroll) Widgets.BeginScrollView(rect, ref recipeScroll, view);

            var row = view.TopPartPixels(RowHeight);
            foreach (var recipe in recipes) {
                DoRecipe(ref row, recipe, dragGroup);
            }

            if (scroll) Widgets.EndScrollView();

            DropArea(rect, recipeListDropTarget, dragGroup, x => x is RecipeDef);
        }

        private static void DoRecipe(ref Rect row, RecipeDef recipe, int dragGroup, object context = null) {
            context ??= recipe;
            Draggable(row, context, dragGroup);

            var icon = recipe.UIIconThing;
            if (icon != null) {
                Widgets.ThingIcon(row.LeftPartPixels(row.height), icon);
            }
            Widgets.Label(row.RightPartPixels(row.width - row.height - RowMargin), recipe.LabelCap);

            row.y += RowStep;
        }

        private void DoDragIndicator() {
            if (currentDrag != DragAndDropWidget.CurrentlyDraggedDraggable()) {
                currentDrag = null;
            }

            Texture2D icon;
            string name;
            if (currentDrag is RecipeDef recipe) {
                icon = DragIcon(recipe);
                name = recipe.LabelCap;
            } else if (currentDrag is IDragItem item) {
                icon = item.Icon;
                name = item.Name;
            } else {
                return;
            }

            var rect = new Rect(Event.current.mousePosition + dragIconAdjust, dragIconSize);
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = transparent;
            GUI.DrawTexture(rect, icon);
            rect.x += rect.width + 2f;
            rect.width = Text.CalcSize(name).x;
            Widgets.Label(rect, name);
            GUI.color = Color.white;
            GenUI.ResetLabelAlign();
        }

        private static Texture2D DragIcon(RecipeDef recipe) {
            var icon = recipe.UIIconThing.uiIcon;
            return (icon == null || icon == BaseContent.BadTex) ? BaseContent.ClearTex : icon;
        }

        private static void Draggable(Rect area, object draggable, int dragGroup) {
            // TODO: Add an indicator to show that we are dragging
            // TODO: If dragging stops an edit, then can not drop??
            // TODO: Drag added menu to self -> crash
            bool hover = Mouse.IsOver(area) && !DragAndDropWidget.Dragging;
            var localCur = current;
            if (DragAndDropWidget.Draggable(dragGroup, area, draggable, onStartDragging: Start) || hover) {
                Widgets.DrawBoxSolid(area, Color.grey);
            }

            void Start() => localCur.StartDrag(draggable);
        }

        private static void DropArea(
                Rect area, object target, int dragGroup, Func<object, bool> exceptIf = null) {
            if (exceptIf != null) {
                object dragged = DragAndDropWidget.CurrentlyDraggedDraggable();
                if (exceptIf(dragged)) return;
            }
            DragAndDropWidget.DropArea(dragGroup, area, DropAction(target), target);
            if (ReferenceEquals(DragAndDropWidget.HoveringDropArea(dragGroup), target) 
                    && DragAndDropWidget.Dragging) {
                DrawBox(area, Color.gray);
            }
        }

        private static Action<object> DropAction(object target) {
            var currentLocal = current;
            return d => currentLocal.Drop(d, target);
        }

        private void Drop(object dropped, object target) {
            var node = target as Tree;
            if (dropped is RecipeDef recipe) {
                new Entry(recipe, node, recipeLookup);
            } else if (dropped is IDragItem item) {
                item.DropOn(node, recipeLookup);
            }
        }

        private static void DrawBox(Rect box, Color color) {
            GUI.color = color;
            Widgets.DrawBox(box);
            GUI.color = Color.white;
        }

        private void UpdateRecipeList() {
            var list = DefDatabase<RecipeDef>.AllDefs
                .Where(x => !recipeLookup.ContainsKey(x));
            if (search.filter.Active) {
                list = list.Where(x => search.filter.Matches(x.label));
            }
            recipes.Clear();
            recipes.AddRange(list);
            recipes.SortBy(x => x.label);
        }

        private void StartDrag(object dragged) {
            currentDrag = dragged;
            SetCurrentEdit(null, false);
        }

        private void SetCurrentEdit(TreeExtra edit, bool value) {
            if (value) {
                if (currentEdit != null && currentEdit != edit) {
                    currentEdit.Editing = false;
                }
                currentEdit = edit;
            } else if (currentEdit == edit) {
                currentEdit = null;
            } else if (edit == null) {
                currentEdit.Editing = false;
            }
        }


        // Internal classes

        public interface IDragItem {
            void DropOn(Tree node, Dictionary<RecipeDef, HashSet<Entry>> recipes);
            void RemoveFrom(Tree node);
            Texture2D Icon { get; }
            string Name { get; }
        }

        public abstract class Tree {
            protected List<Tree> children = new List<Tree>();
            protected List<Entry> entries = new List<Entry>();
            protected Tree parent;
            protected float width = 10f;
            private bool open = true;
            private bool inContents = false;
            private IDragItem remove;

            public Tree Extra(string name) {
                Tree res = children.OfType<TreeExtra>().FirstOrDefault(x => x.name == name);
                if (res == null) {
                    res = new TreeExtra { name = name };
                    Add(res);
                }
                return res;
            }

            public void Add(Entry entry) {
                entries.Add(entry);
                entry.treeNode = this;
            }

            public void Add(Tree child) {
                children.Add(child);
                child.parent = this;
            }

            public void RemoveNow(Entry entry) 
                => entries.Remove(entry);

            public void RemoveNow(Tree child) 
                => children.Remove(child);

            public void Remove(IDragItem item) {
                if (inContents) {
                    remove = item;
                } else {
                    item.RemoveFrom(this);
                }
            }

            public abstract string Name { get; }

            public virtual bool CanAccept => true;

            public bool Empty 
                => children.Count + entries.Count == 0;

            public bool ShowInOverview 
                => entries.Count > 0 || children.Any(x => x.ShowInOverview);

            protected virtual void DoIcons(ref Rect iconRect, float iconStep) {
                if (Widgets.ButtonImage(iconRect, TexButton.Plus)) {
                    var item = new TreeExtra {
                        name = "",
                        parent = this,
                        Editing = true
                    };
                    item.FocusName();
                    children.Add(item);
                }
                iconRect.x += iconStep;
            }

            public abstract void FillEntry(Entry entry);

            public void DoContents(ref Rect row, int dragGroup) {
                inContents = true;
                foreach (var item in children) item.DoEntry(ref row, dragGroup);
                foreach (var item in entries)  item.DoEntry(ref row, dragGroup);
                inContents = false;
                remove?.RemoveFrom(this);
                remove = null;
            }

            protected virtual void DoEntry(ref Rect row, int dragGroup) {
                if (CanAccept) DropArea(row.LeftPartPixels(width), this, dragGroup);

                var partRect = row.LeftPartPixels(row.height);
                float iconStep = partRect.width + RowMargin;

                if (!Empty) ExtraWidgets.CollapseButton(partRect, ref open);
                partRect.x += iconStep;

                DoIcons(ref partRect, iconStep);

                partRect.width = Text.CalcSize(Name).x;
                if (partRect.xMax > row.xMax) partRect.xMax = row.xMax;
                DoName(ref partRect);

                width = partRect.xMax - row.x + 1f;
                row.y += RowStep;
                if (open) {
                    var next = row;
                    next.xMin += IndentStep;
                    DoContents(ref next, dragGroup);
                    row.y = next.y;
                }
            }

            protected virtual void DoName(ref Rect rect) 
                => Widgets.Label(rect, Name);

            protected void RefillAll() {
                foreach (var child in children) {
                    child.RefillAll();
                }
                foreach (var entry in entries) {
                    entry.Refill();
                }
            }
        }

        private class TreeCat : Tree {
            public ThingCategoryDef cat;

            public TreeCat(ThingCategoryDef cat, Dictionary<ThingCategoryDef, Tree> lookup) {
                this.cat = cat;
                cat.childCategories.Select(x => new TreeCat(x, lookup)).Do(Add);
                lookup[cat] = this;
            }

            public override string Name => cat.LabelCap;

            protected override void DoIcons(ref Rect iconRect, float iconStep) {
                if (cat.icon != BaseContent.BadTex) GUI.DrawTexture(iconRect, cat.icon);
                iconRect.x += iconStep;
                base.DoIcons(ref iconRect, iconStep);
            }

            public override void FillEntry(Entry entry) 
                => entry.parent = cat;
        }

        private class TreeExtra : Tree, IDragItem {
            private const float EditBoxMin = 150f;
            private const float EditBoxMax = 300f;
            private static readonly string[] editTip = { "Edit name", "Stop editing name" };
            private static readonly string editCtrlName = $"kathanon.{typeof(TreeExtra).FullName}.name";

            private bool takeFocus = false;
            private bool editing = false;

            public string name;

            public override string Name => name;

            public Texture2D Icon => BaseContent.ClearTex;

            public override bool CanAccept => !editing;

            public bool Editing { 
                get { 
                    return editing; 
                }
                set {
                    if (value != editing) {
                        editing = value;
                        current?.SetCurrentEdit(this, value);
                    }
                }
            }

            protected override void DoEntry(ref Rect row, int dragGroup) {
                if (!editing) Draggable(row.LeftPartPixels(width), this, dragGroup);
                base.DoEntry(ref row, dragGroup);
            }

            protected override void DoIcons(ref Rect iconRect, float iconStep) {
                if (ExtraWidgets.ButtonImage(iconRect, TexButton.Minus, Empty)) {
                    parent.Remove(this);
                }
                iconRect.x += iconStep;

                base.DoIcons(ref iconRect, iconStep);

                bool editTemp = editing;
                ExtraWidgets.EditButton(iconRect, ref editTemp, editTip);
                if (editing != editTemp) Editing = editTemp;
                iconRect.x += iconStep;
            }

            protected override void DoName(ref Rect rect) {
                if (editing) {
                    rect.width = Mathf.Clamp(rect.width, EditBoxMin, EditBoxMax);
                    rect = rect.ExpandedBy(2f);
                    if (takeFocus) GUI.SetNextControlName(editCtrlName);
                    name = Widgets.TextField(rect, name);
                    if (takeFocus) GUI.FocusControl(editCtrlName);
                } else {
                    base.DoName(ref rect);
                }
                if (Event.current.type == EventType.Repaint) takeFocus = false;
            }

            public void FocusName() => takeFocus = true;

            public void DropOn(Tree node, Dictionary<RecipeDef, HashSet<Entry>> _) {
                parent.Remove(this);
                node?.Add(this);
                RefillAll();
            }
            
            public override void FillEntry(Entry entry) {
                parent?.FillEntry(entry);
                entry.subPath.Add(name);
            }

            public void RemoveFrom(Tree node) 
                => node.RemoveNow(this);
        }

        public class Entry : IExposable, IDragItem {
            public Tree treeNode;
            public RecipeDef recipe;
            public ThingCategoryDef parent;
            public List<string> subPath = new List<string>();

            public Texture2D Icon => DragIcon(recipe);

            public string Name => recipe.LabelCap;

            public Entry() {}

            public Entry(RecipeDef recipe, 
                         Tree node, 
                         Dictionary<RecipeDef, HashSet<Entry>> recipes) {
                this.recipe = recipe;
                node.FillEntry(this);
                node.Add(this);
                AddTo(recipes);
            }

            public void Refill() {
                subPath.Clear();
                treeNode?.FillEntry(this);
            }

            public MenuNode Apply(MenuNode n) {
                if (parent != null) {
                    n = CategorizerThingCategory.ToCategory(n, parent, _ => true);
                }
                foreach (var item in subPath) {
                    n = n.For(item);
                }
                
                return n;
            }

            public Entry Copy() 
                => new Entry {
                    recipe = recipe,
                    parent = parent,
                    subPath = subPath.ToList()
                };

            public void DropOn(Tree node,
                               Dictionary<RecipeDef, HashSet<Entry>> recipes) {
                treeNode.Remove(this);
                node?.Add(this);
                Refill();
                if (node == null) recipes[recipe].Remove(this);
            }


            public void AddToTree(Tree root,
                                  Dictionary<ThingCategoryDef, Tree> cats,
                                  Dictionary<RecipeDef, HashSet<Entry>> recipes) {
                if (parent != null && cats.TryGetValue(parent, out var catNode)) {
                    root = catNode;
                }
                foreach (var item in subPath) {
                    root = root.Extra(item);
                }
                root.Add(this);
                AddTo(recipes);
            }

            private void AddTo(Dictionary<RecipeDef, HashSet<Entry>> recipes) {
                if (!recipes.ContainsKey(recipe)) {
                    recipes.Add(recipe, new HashSet<Entry>());
                }
                recipes[recipe].Add(this);
            }

            public void RemoveFrom(Tree node)
                => node.RemoveNow(this);

            public void DoEntry(ref Rect row, int dragGroup) 
                => DoRecipe(ref row, recipe, dragGroup, this);

            public void ExposeData() {
                Scribe_Defs.Look(ref parent, "parent");
                Scribe_Defs.Look(ref recipe, "recipe");
                Scribe_Collections.Look(ref subPath, "subPath", LookMode.Value);
            }
        }
    }
}
