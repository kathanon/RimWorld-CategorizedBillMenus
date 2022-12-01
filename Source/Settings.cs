using RimWorld;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    public class Settings : ModSettings {
        private HashSet<string> favorites;
        private HashSet<string> disabledCats;
        private List<(Categorizer cat, bool active)> categorizers;
        private bool useFavorites = true;
        private bool rightAlign = true;
        private bool collapse = true;

        private HashSet<string> favoritesBackup = null;

        private readonly HashSet<RecipeDef> favoriteRecipes = new HashSet<RecipeDef>();
        private readonly HashSet<ThingCategoryDef> disabledCatDefs = new HashSet<ThingCategoryDef>();
        private readonly List<ThingCategoryDef> allCatDefs = new List<ThingCategoryDef>();
        private readonly Dictionary<ThingCategoryDef, string> replacementNames = 
            new Dictionary<ThingCategoryDef, string>();

        private static Settings inst = null;
        private static ThingCategoryDef root = null;

        public static Settings Instance => inst ?? (inst = Main.Instance.Settings);

        public static bool UseFavorites => Instance.useFavorites;
        public bool UseFavoritesInst => useFavorites;

        public static bool RightAlign => Instance.rightAlign;
        public bool RightAlignInst => rightAlign;

        public static bool Collapse => Instance.collapse;
        public bool CollapseInst => collapse;

        public static bool IsFav(RecipeDef def) => Instance.IsFavInst(def);
        public bool IsFavInst(RecipeDef def) => useFavorites && favoriteRecipes.Contains(def);

        public static void ToggleFav(RecipeDef def) => Instance.ToggleFavInst(def);
        public void ToggleFavInst(RecipeDef def) {
            if (favoriteRecipes.Remove(def)) {
                favorites.Remove(def.defName);
            } else {
                favoriteRecipes.Add(def);
                favorites.Add(def.defName);
            }
            favoritesBackup = null;
            Write();
        }

        public static bool IsDisabled(ThingCategoryDef def) => Instance.IsDisabledInst(def);
        public bool IsDisabledInst(ThingCategoryDef def) => disabledCatDefs.Contains(def);

        public static void ToggleDisabled(ThingCategoryDef def) => Instance.ToggleDisabledInst(def);
        public void ToggleDisabledInst(ThingCategoryDef def) {
            if (disabledCatDefs.Remove(def)) {
                disabledCats.Remove(def.defName);
            } else {
                disabledCatDefs.Add(def);
                disabledCats.Add(def.defName);
            }
        }

        public static void ToggleInactiveDisabled(string defName) => Instance.ToggleInactiveDisabledInst(defName);
        public void ToggleInactiveDisabledInst(string defName) {
            if (!disabledCats.Remove(defName)) {
                disabledCats.Add(defName);
            }
        }

        public static IEnumerable<Categorizer> Categorizers => Instance.CategorizersInst;
        public  IEnumerable<Categorizer> CategorizersInst => categorizers.Where(c => c.active).Select(c => c.cat);

        private readonly List<string> inactive = new List<string>();
        public List<string> InactiveDisabled => inactive;


        public override void ExposeData() {
            Scribe_Collections.Look(ref favorites, "favorites", LookMode.Value);
            Scribe_Collections.Look(ref disabledCats, "disabled", LookMode.Value);
            Scribe_Values.Look(ref useFavorites, "useFavorites", true);

            if (Scribe.mode == LoadSaveMode.PostLoadInit) {
                Setup();
            }
        }

        public Settings EnsureSetup() {
            if (root == null || favorites == null || disabledCats == null) {
                Setup();
            }
            return this;
        }

        private void Setup() {
            // TODO: Make configurable
            if (categorizers == null) {
                categorizers = DefaultCategorizers();
            }
            // ---


            if (favorites == null) favorites = new HashSet<string>();
            if (disabledCats == null) disabledCats = new HashSet<string>();
            root = ThingCategoryDefOf.Root;

            favoriteRecipes.Clear();
            favoriteRecipes.AddRange(DefDatabase<RecipeDef>.AllDefs.Where(r => favorites.Contains(r?.defName)));
            allCatDefs.Clear();
            allCatDefs.AddRange(DefDatabase<ThingCategoryDef>.AllDefs.Where(c => c != root && c != null));
            allCatDefs.SortBy(c => c.LabelCap.ToString());
            disabledCatDefs.Clear();
            disabledCatDefs.AddRange(allCatDefs.Where(c => disabledCats.Contains(c.defName)));
            disabledCatDefs.Add(root);

            var inactiveTmp = new HashSet<string>();
            inactiveTmp.AddRange(disabledCats);
            foreach (var def in disabledCatDefs) {
                inactiveTmp.Remove(def.defName);
            }
            inactive.Clear();
            inactive.AddRange(inactiveTmp);
            inactive.Sort();

            replacementNames.Clear();
            string prev = null;
            ThingCategoryDef prevCat = null;
            foreach (var curCat in allCatDefs) {
                string cur = curCat.LabelCap;
                if (cur == prev) {
                    if (prevCat.parent != null) replacementNames[prevCat] = $"{prev} ({prevCat.parent.LabelCap})";
                    if (curCat .parent != null) replacementNames[curCat ] = $"{cur } ({curCat .parent.LabelCap})";
                }
                prev = cur;
                prevCat = curCat;
            }

            maxCatWidth = -1f;
            treeCatWidth = -1f;
        }

        private static List<(Categorizer cat, bool active)> DefaultCategorizers() {
            return new List<(Categorizer cat, bool active)> {
                    (CategorizerDefault.Instance,                true),
                    (CategorizerRuleBased.PresetByLimb.Create(), true),
            };
        }


        // Options GUI

        public const float ArrowSize     = 18f;
        public const float Margin        = 10f;
        public const float TitleMargin   =  6f;
        public const float RowMargin     =  4f;
        public const float ScrollMargin  = 24f;
        public const float ColMargin     =  8f;
        public const float ButtonMarginX = 22f;
        public const float ButtonMarginY = 12f;

        public const float CheckboxSize = Widgets.CheckboxSize;
        public const float CheckMargin  = 10f + CheckboxSize;
        public const float TreeIndent   = CheckboxSize;
        public const float RowHeight    = CheckboxSize;
        public const float RowStep      = RowHeight + RowMargin;
        public static readonly Color TitleHRColor = Color.gray;

        private bool categoriesOpen = true;
        private bool categoriesTree = false;
        private float height = 0f;
        private float maxCatWidth = -1f;
        private float treeCatWidth = -1f;
        private int[] treeColStarts = null;
        private Vector2 scroll = Vector2.zero;

        public void DoWindowContents(Rect rect) {
            var widthScroll = rect.width - ScrollMargin;
            bool doScroll = Event.current.type != EventType.Layout && height > rect.height;
            if (doScroll) {
                var view = rect.AtZero();
                view.height = height;
                view.width -= ScrollMargin;
                Widgets.BeginScrollView(rect, ref scroll, view);
                rect = view;
            }

            var align = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            float curY = rect.y;
            // TODO: Clear favorites button (use confirmation dialog)
            DoFavorites(rect, ref curY);
            // TODO: Any general options here
            if (FoldableSection(rect, Strings.CategoriesTitle, Strings.CategoriesTooltip, ref categoriesOpen, ref curY)) {
                DoCategories(rect, widthScroll, ref curY);
            }
            // TODO: more?
            Text.Anchor = align;

            height = curY - rect.y;
            if (doScroll) {
                Widgets.EndScrollView();
            }
        }

        private void DoFavorites(Rect rect, ref float curY) {
            var UseFav = "Use favorites";
            var size = Text.CalcSize(UseFav);
            var height = Mathf.Max(size.y + ButtonMarginY, CheckboxSize);
            var r = new Rect(rect.x, curY, (float) (size.x + CheckMargin), height);
            Widgets.CheckboxLabeled(r, UseFav, ref useFavorites);
            if (useFavorites) {
                r.x += r.width + Margin;
                var ClearFav = "Clear favorites";
                r.width = Text.CalcSize(ClearFav).x + ButtonMarginX;
                if (DisablableButton(r, ClearFav, favorites.Count > 0)) {
                    favoritesBackup = favorites;
                    favorites = new HashSet<string>();
                }

                r.x += r.width + Margin;
                var Undo = "Undo";
                r.width = Text.CalcSize(Undo).x + ButtonMarginX;
                if (DisablableButton(r, Undo, favoritesBackup != null)) {
                    favorites = favoritesBackup;
                    favoritesBackup = null;
                }

                r.x += r.width + Margin;
                var label = (favoritesBackup != null) 
                    ? $"{favorites.Count} favorites ({favoritesBackup.Count} to undo)" 
                    : $"{favorites.Count} favorites";
                r.width = Text.CalcSize(label).x;
                Widgets.Label(r, label);
            }
            curY += height + Margin;
        }

        private static bool DisablableButton(Rect rect, string label, bool active) {
            if (!active) GUI.color = Color.grey;
            bool res = Widgets.ButtonText(rect, label, doMouseoverSound: active, active: active);
            GUI.color = Color.white;
            return res;
        }

        private void DoCategories(Rect rect, float widthScroll, ref float curY) {
            var widgetRect = new Rect(rect.x, curY, rect.width, RowHeight);
            Widgets.CheckboxLabeled(widgetRect, "Show as tree", ref categoriesTree, placeCheckboxNearText: true);
            curY += widgetRect.height + TitleMargin;
            if (categoriesTree) {
                if (treeCatWidth < 0f) {
                    // Subtract one step since root is not shown.
                    treeCatWidth = (CalcTreeCatWidth(root) ?? 0f) + CheckMargin - TreeIndent;
                    treeColStarts = null;
                }
                CalcColumns(rect, treeCatWidth, curY, widthScroll,
                    out Rect row, out int cols, out float colWidth, out float firstCol);
                var cats = root.childCategories;
                if (treeColStarts == null) {
                    treeColStarts = TreeSplitColumns(cols, cats);
                }
                float colCurY = curY, colMaxY = 0f;
                for (int i = 0, col = 0; i < cats.Count; i++) {
                    if (col + 1 < treeColStarts.Length && i >= treeColStarts[col + 1]) {
                        if (colCurY > colMaxY) colMaxY = colCurY;
                        colCurY = curY;
                        col++;
                        row.x += colWidth;
                    }
                    DoTreeCategories(row, cats[i], ref colCurY);
                }
                curY = Math.Max(colMaxY, colCurY);
            } else {
                if (maxCatWidth < 0f) {
                    maxCatWidth = allCatDefs
                        .Select(c => Text.CalcSize(CatNameWithReplacements(c)).x)
                        .Concat(0f).Max() + CheckMargin;
                }
                CalcColumns(rect, maxCatWidth, curY, widthScroll,
                    out Rect row, out int cols, out float colWidth, out float firstCol);
                int rows = (allCatDefs.Count + cols - 1) / cols;
                for (int r = 0; r < rows; r++) {
                    for (int c = 0; c < cols; c++) {
                        int i = r + c * rows;
                        if (i < allCatDefs.Count) {
                            CategoryCheckbox(row, allCatDefs[i], true);
                            row.x += colWidth;
                        }
                    }
                    row.y += RowStep;
                    row.x = firstCol;
                }
                curY = row.y + RowStep;
            }
        }

        private void DoTreeCategories(Rect row, ThingCategoryDef cat, ref float curY) {
            row.y = curY;
            curY += RowStep;
            CategoryCheckbox(row, cat, false);
            var next = row;
            next.xMin += TreeIndent;
            foreach (var child in cat.childCategories) {
                DoTreeCategories(next, child, ref curY);
            }
        }

        private void CalcColumns(
                Rect rect, float width, float curY, float widthScroll,
                out Rect row, out int cols, out float colWidth, out float firstCol) {
            cols = (int) ((widthScroll + ColMargin) / (width + ColMargin));
            colWidth = (rect.width + ColMargin) / cols;
            firstCol = (colWidth - width) / 2f + rect.x;
            row = new Rect(firstCol, curY, width, RowHeight);
        }

        private int[] TreeSplitColumns(int cols, List<ThingCategoryDef> childCategories) {
            var lengths = childCategories.Select(TreeLength).ToList();
            int avg = lengths.Sum() / cols;
            return TreeTrySplitColumn(lengths, cols, avg, 0, new int[0], out int _);
        }

        private int[] TreeTrySplitColumn(List<int> lengths, int cols, int avg, int start, int[] prev, out int max) {
            int[] cur = new int[prev.Length + 1];
            Array.Copy(prev, cur, prev.Length);
            cur[prev.Length] = start;
            if (cur.Length >= cols || start == lengths.Count - 1) {
                max = lengths.Skip(start).Sum();
                return cur;
            }

            int high = start + 1;
            int lowSum = lengths[high - 1];
            int highSum = lowSum + lengths[high];
            while (high < lengths.Count - 1 && highSum < avg) {
                lowSum = highSum;
                high++;
                highSum += lengths[high];
            }

            var highRes = TreeTrySplitColumn(lengths, cols, avg, high + 1, cur, out int highMax);
            var lowRes  = TreeTrySplitColumn(lengths, cols, avg, high,     cur, out int lowMax);
            highMax = Math.Max(highMax, highSum);
            lowMax = Math.Max(lowMax, lowSum);
            if (lowMax < highMax) {
                max = lowMax;
                return lowRes;
            } else {
                max = highMax;
                return highRes;
            }
        }

        private int TreeLength(ThingCategoryDef cat) => 1 + cat.childCategories?.Sum(TreeLength) ?? 0;

        private void CategoryCheckbox(Rect rect, ThingCategoryDef cat, bool useReplNames) {
            CheckboxValue val = !IsDisabledInst(cat);
            string label = useReplNames ? CatNameWithReplacements(cat) : (string) cat.LabelCap;
            Widgets.CheckboxLabeled(rect, label, ref val.Value);
            if (val.Changed) ToggleDisabledInst(cat);
        }

        private string CatNameWithReplacements(ThingCategoryDef cat) => 
            replacementNames.TryGetValue(cat, out var repl) ? repl : (string) cat.LabelCap;
        
        private float? CalcTreeCatWidth(ThingCategoryDef cat) => 
            Mathf.Max(Text.CalcSize(cat?.LabelCap ?? "").x, TreeIndent + cat?.childCategories?.Max(CalcTreeCatWidth) ?? 0f);

        private bool FoldableSection(Rect rect, string title, string tooltip, ref bool open, ref float curY) {
            Text.Font = GameFont.Medium;
            var size = Text.CalcSize(title);
            float titleHeight = Mathf.Max(ArrowSize, size.y);
            float midY = curY + titleHeight / 2f;
            curY += titleHeight + TitleMargin;

            var arrowRect = new Rect(rect.x, midY - ArrowSize / 2f, ArrowSize, ArrowSize);
            TooltipHandler.TipRegion(arrowRect, open ? Strings.OpenTooltip : Strings.ClosedTooltip);
            if (Widgets.ButtonImage(arrowRect, open ? Textures.DownIcon : Textures.RightIcon)) {
                open = !open;
            }

            float titleX = rect.x + ArrowSize + TitleMargin;
            var titleRect = new Rect(titleX, midY - size.y / 2f, Mathf.Min(rect.xMax - titleX, size.x), size.y);
            TooltipHandler.TipRegion(titleRect, tooltip);
            Widgets.Label(titleRect, title);
            Text.Font = GameFont.Small;

            if (open) { 
                GUI.color = TitleHRColor;
                Widgets.DrawLineHorizontal(rect.x, curY, rect.width);
                GUI.color = Color.white;
                curY += 1f + TitleMargin;
            }
            return open;
        }
    }
}
