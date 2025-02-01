using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static CategorizedBillMenus.CurrentModeShorthands;

namespace CategorizedBillMenus;
[StaticConstructorOnStartup]
public class CategorizerThingCategory : CategorizerSingleton {
    static CategorizerThingCategory() {
        Register(new CategorizerThingCategory());
    }

    public static readonly CategorizerThingCategory Instance = new();

    private HashSet<string> disabledCats;

    private readonly HashSet<ThingCategoryDef> disabledCatDefs = [];
    private readonly List<ThingCategoryDef> allCatDefs = [];
    private readonly List<string> inactive = [];
    private readonly Dictionary<ThingCategoryDef, string> replacementNames = [];

    private static ThingCategoryDef root = null;

    private float maxCatWidth = -1f;
    private float treeCatWidth = -1f;
    private bool categoriesTree = false;
    private int[] treeColStarts = null;

    public const float ColMargin    = 8f;

    public const float CheckboxSize = Widgets.CheckboxSize;
    public const float TreeIndent   = CheckboxSize;
    public const float RowHeight    = CheckboxSize;
    public const float RowStep      = RowHeight + RowMargin;

    public const float TitleMargin  = Settings.TitleMargin;
    public const float RowMargin    = Settings.RowMargin;
    public const float CheckMargin  = Settings.CheckMargin;

    public CategorizerThingCategory() : base(Strings.CategoriesName, Strings.CategoriesDesc) {}

    public override bool AppliesTo(BillMenuEntry entry, bool first) 
        => first && (entry.Recipe.ProducedThingDef?.thingCategories?.Any() ?? false);

    public override IEnumerable<MenuNode> Apply(BillMenuEntry entry, MenuNode parent, MenuNode root, bool first) {
        yield return ToCategory(
            root,
            entry.Recipe.ProducedThingDef.thingCategories[0],
            IsEnabled);
    }

    public static MenuNode ToCategory(MenuNode n, ThingCategoryDef category, Func<ThingCategoryDef, bool> enabled) {
        foreach (var cat in category.Parents.Reverse().AddItem(category)) {
            if (enabled(cat)) {
                n = n.For(cat);
            }
        }

        return n;
    }

    public bool IsEnabled(ThingCategoryDef def) 
        => !disabledCatDefs.Contains(def);

    public bool IsDisabled(ThingCategoryDef def) 
        => disabledCatDefs.Contains(def);

    public void ToggleDisabled(ThingCategoryDef def) {
        if (disabledCatDefs.Remove(def)) {
            disabledCats.Remove(def.defName);
        } else {
            disabledCatDefs.Add(def);
            disabledCats.Add(def.defName);
        }
    }

    public void ToggleInactiveDisabled(string defName) {
        if (!disabledCats.Remove(defName)) {
            disabledCats.Add(defName);
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Collections.Look(ref disabledCats, "disabled", LookMode.Value);

        if (PostLoadInit) {
            Setup();
        }
    }


    // Options GUI

    public override void DoSettings(Rect rect, ref float curY) {
        var widgetRect = new Rect(rect.x, curY, rect.width, RowHeight);
        Widgets.CheckboxLabeled(widgetRect, "Show as tree", ref categoriesTree, placeCheckboxNearText: true);
        curY += widgetRect.height + TitleMargin;
        if (categoriesTree) {
            if (treeCatWidth < 0f) {
                // Subtract one step since root is not shown.
                treeCatWidth = (CalcTreeCatWidth(root) ?? 0f) + CheckMargin - TreeIndent;
                treeColStarts = null;
            }
            CalcColumns(rect, treeCatWidth, curY, out Rect row,
                out int cols, out float colWidth, out float firstCol);
            var cats = root.childCategories;
            treeColStarts ??= TreeSplitColumns(cols, cats);
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
            CalcColumns(rect, maxCatWidth, curY, out Rect row,
                out int cols, out float colWidth, out float firstCol);
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
            Rect rect, float width, float curY, out Rect row,
            out int cols, out float colWidth, out float firstCol) {
        cols = (int) ((rect.width + ColMargin) / (width + ColMargin));
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
        CheckboxValue val = !IsDisabled(cat);
        string label = useReplNames ? CatNameWithReplacements(cat) : (string) cat.LabelCap;
        Widgets.CheckboxLabeled(rect, label, ref val.Value);
        if (val.Changed) ToggleDisabled(cat);
    }

    private string CatNameWithReplacements(ThingCategoryDef cat) =>
        replacementNames.TryGetValue(cat, out var repl) ? repl : (string) cat.LabelCap;

    private float? CalcTreeCatWidth(ThingCategoryDef cat) =>
        Mathf.Max(Text.CalcSize(cat?.LabelCap ?? "").x, TreeIndent + cat?.childCategories?.Max(CalcTreeCatWidth) ?? 0f);


    // Initialization

    public void EnsureSetup() {
        if (root == null || disabledCats == null) {
            Setup();
        }
    }

    private void Setup() {
        disabledCats ??= [];
        root = ThingCategoryDefOf.Root;

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
                if (curCat.parent != null) replacementNames[curCat] = $"{cur} ({curCat.parent.LabelCap})";
            }
            prev = cur;
            prevCat = curCat;
        }

        maxCatWidth = -1f;
        treeCatWidth = -1f;
    }
}
