using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static CategorizedBillMenus.CurrentModeShorthands;

namespace CategorizedBillMenus;
public class Settings : ModSettings {
    private HashSet<string> favorites;
    private List<CatEntry> categorizers;
    private bool useFavorites = true;
    private bool rightAlign = true;
    private bool collapse = true;
    private bool search = true;
    private int searchThreshold = 20;

    private HashSet<string> favoritesBackup = null;

    private readonly HashSet<RecipeDef> favoriteRecipes = [];

    private static Settings inst = null;
    private static ThingCategoryDef root = null;

    public static Settings Instance => inst ??= Main.Instance.Settings;

    public static bool UseFavorites => Instance.useFavorites;

    public static bool RightAlign => Instance.rightAlign;

    public static bool Collapse => Instance.collapse;

    public static bool ShouldSearch(int n) => Instance.ShouldSearchInst(n);
    public bool ShouldSearchInst(int n) => search && n >= searchThreshold;

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

    public static IEnumerable<Categorizer> Categorizers => Instance.CategorizersInst;
    public  IEnumerable<Categorizer> CategorizersInst => categorizers.Where(c => c.active).Select(c => c.cat);

    private readonly List<string> inactive = [];
    public List<string> InactiveDisabled => inactive;


    public override void ExposeData() {
        Scribe_Collections.Look(ref favorites, "favorites", LookMode.Value);
        //Scribe_Collections.Look(ref categorizers, "categorizers", LookMode.Deep);
        Scribe_Values.Look(ref useFavorites, "useFavorites", true);
        Scribe_Values.Look(ref rightAlign, "rightAlign", true);
        Scribe_Values.Look(ref collapse, "collapse", true);
        Scribe_Values.Look(ref search, "search", true);
        Scribe_Values.Look(ref searchThreshold, "searchThreshold", 20);

        if (PostLoadInit) {
            Setup();
        }
    }

    public Settings EnsureSetup() {
        if (root == null || favorites == null) {
            Setup();
        }
        CategorizerThingCategory.Instance.EnsureSetup();
        return this;
    }

    private void Setup() {
        categorizers ??= DefaultCategorizers();
        favorites ??= [];
        root = ThingCategoryDefOf.Root;

        favoriteRecipes.Clear();
        favoriteRecipes.AddRange(DefDatabase<RecipeDef>.AllDefs.Where(r => favorites.Contains(r?.defName)));
    }

    private static List<CatEntry> DefaultCategorizers() 
        => [
            new(CategorizerThingCategory.Instance,        true),
            new(CategorizerRuleBased.PresetByLimb.Copy(), true),
        ];


    // Options GUI

    public const float ArrowSize     =  18f;
    public const float Margin        =  10f;
    public const float TitleMargin   =   6f;
    public const float RowMargin     =   4f;
    public const float ScrollMargin  =  24f;
    public const float ButtonMarginX =  22f;
    public const float ButtonMarginY =  12f;
    public const float SubCatMargin  =  20f;
    public const float IconSize      =  20f;
    public const float SliderWidth   = 200f;

    public const float IconStep     = IconSize + Margin;
    public const float CheckboxSize = Widgets.CheckboxSize;
    public const float CheckMargin  = 10f + CheckboxSize;
    public static readonly Color TitleHRColor = Color.gray;

    private bool generalOpen = true;
    private bool subMenusOpen = true;
    private float height = 0f;
    private Vector2 scroll = Vector2.zero;
    private CatEntry editNameCat = null;
    private float editNameWidth = -1f;

    public void DoWindowContents(Rect rect) {
        bool doScroll = Event.current.type != EventType.Layout && height > rect.height;
        if (doScroll) {
            var view = rect;
            view.width -= ScrollMargin;
            view.height = height;
            Widgets.BeginScrollView(rect, ref scroll, view);
            rect = view;
        } else {
            rect.width -= ScrollMargin;
        }

        var align = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleLeft;
        float curY = rect.y;

        //if (FoldableSection(rect, Strings.GeneralTitle, Strings.GeneralTooltip, ref generalOpen, ref curY)) {
            DoGeneral(rect, ref curY);
        //}

        //if (FoldableSection(rect, Strings.SubMenusTitle, Strings.SubMenusTooltip, ref subMenusOpen, ref curY)) {
        //    DoSubMenus(rect, ref curY);
        //}

        // TODO: more?
        Text.Anchor = align;

        height = curY - rect.y;
        if (doScroll) {
            Widgets.EndScrollView();
        }
    }

    private void DoGeneral(Rect rect, ref float curY) {
        DoFavorites(rect, ref curY);

        Text.Anchor = TextAnchor.MiddleLeft;
        float width = Text.CalcSize(Strings.CollapseOption).x + CheckMargin;
        var row = new Rect(rect.x, curY, Mathf.Min(width, rect.width), CheckboxSize);
        Widgets.CheckboxLabeled(row, Strings.CollapseOption, ref collapse, placeCheckboxNearText: true);

        width = Text.CalcSize(Strings.RightAlignOption).x + CheckMargin;
        WidgetSpace(rect, width, ref row, ref curY);
        Widgets.CheckboxLabeled(row, Strings.RightAlignOption, ref rightAlign, placeCheckboxNearText: true);

        float checkWidth = Text.CalcSize(Strings.SearchOption).x + CheckMargin + Margin;
        width = checkWidth + Text.CalcSize(Strings.SearchIfOption(99)).x + Margin + SliderWidth;
        WidgetSpace(rect, width, ref row, ref curY);
        Widgets.CheckboxLabeled(row, Strings.SearchOption, ref search, placeCheckboxNearText: true);
        row.xMin += checkWidth;
        if (!search) GUI.color = Color.gray;
        Widgets.Label(row, Strings.SearchIfOption(searchThreshold));
        GUI.color = Color.white;
        if (search) {
            row.xMin = row.xMax - SliderWidth;
            searchThreshold = (int) Widgets.HorizontalSlider(row, searchThreshold, 0f, 50f, true, roundTo: 1f);
        }

        // TODO: Any additional general options here

        curY += CheckboxSize + Margin;
        GenUI.ResetLabelAlign();
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
                favorites = [];
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

    private static void WidgetSpace(Rect rect, float width, ref Rect row, ref float curY) {
        row.x += row.width + Margin;
        row.width = width;
        if (row.xMax > rect.xMax) {
            row.y = curY += CheckboxSize + Margin;
            row.x = rect.x;
            if (row.width > rect.width) {
                row.width = rect.width;
            }
        }
    }

    private static bool DisablableButton(Rect rect, string label, bool active) {
        if (!active) GUI.color = Color.grey;
        bool res = Widgets.ButtonText(rect, label, doMouseoverSound: active, active: active);
        GUI.color = Color.white;
        return res;
    }

    private void DoSubMenus(Rect rect, ref float curY) {
        var subRect = rect.ContractedBy(SubCatMargin, 0f);
        var tempCurY = curY;

        ExtraWidgets.ReorderableList(categorizers, AddCategorizerMenu, DoCategorizer, subRect, ref curY);

        void DoCategorizer(CatEntry entry, Action<float> doButtons, Rect r, float off, ref float innerCurY) {
            var titleRect = r;
            titleRect.xMin += IconStep;
            off += IconStep;
            if (FoldableSection(titleRect,
                                entry.cat.Name,
                                entry.cat.Description,
                                ref entry.open,
                                ref innerCurY,
                                off,
                                r2 => NameWidgets(r, r2, entry, doButtons))) {
                entry.cat.DoSettings(subRect, ref innerCurY);
                DividerLine(titleRect, off, ref innerCurY);
            }
        }

        void NameWidgets(Rect itemRect, Rect nameRect, CatEntry entry, Action<float> listButtons) {
            float midY = nameRect.y + nameRect.height / 2;
            var r = new Rect(itemRect.x, midY - IconSize / 2, IconSize, IconSize);

            listButtons(midY);
            Widgets.Checkbox(r.min, ref entry.active, IconSize);

            bool edit = editNameCat == entry;
            if (!entry.cat.Singleton) {
                if (edit && editNameWidth > 0f) {
                    nameRect.width = editNameWidth;
                }
                r.x = nameRect.xMax + Margin;
                if (Widgets.ButtonImage(r, Textures.EditIcon)) {
                    edit = !edit;
                    editNameCat = edit ? entry : null;
                    editNameWidth = nameRect.width;
                } else if (edit && IsEnterDown(Event.current)) {
                    edit = false;
                    editNameCat = null;
                    Event.current.Use();
                }
            }
            entry.cat.DoName(nameRect, edit);
        }
    }

    private void AddCategorizerMenu() 
        => Categorizer.SelectionMenu(c => categorizers.Add(new CatEntry(c, true)));

    public static bool IsEnterDown(Event e)
        => e.type == EventType.KeyDown && (e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Return);

    private bool FoldableSection(Rect rect, string title, string tooltip, 
                                 ref bool open, ref float curY, 
                                 float xOffset = 0f, Action<Rect> titleFunc = null) {
        Text.Font = GameFont.Medium;
        var size = Text.CalcSize(title);
        float titleHeight = Mathf.Max(ArrowSize, size.y);
        float midY = curY + titleHeight / 2f;
        curY += titleHeight + TitleMargin;

        var arrowRect = new Rect(rect.x, midY - ArrowSize / 2f, ArrowSize, ArrowSize);
        ExtraWidgets.CollapseButton(arrowRect, ref open);

        float titleX = arrowRect.xMax + TitleMargin;
        var titleRect = new Rect(titleX, midY - size.y / 2f, Mathf.Min(rect.xMax - titleX, size.x + 2f), size.y);
        TooltipHandler.TipRegion(titleRect, tooltip);
        if (titleFunc == null) {
            Widgets.Label(titleRect, title);
        } else {
            titleFunc(titleRect);
        }
        Text.Font = GameFont.Small;

        if (open) {
            DividerLine(rect, xOffset, ref curY);
        }
        return open;
    }

    private static void DividerLine(Rect rect, float xOffset, ref float curY) {
        rect.xMin -= xOffset;
        GUI.color = TitleHRColor;
        Widgets.DrawLineHorizontal(rect.x, curY, rect.width);
        GUI.color = Color.white;
        curY += 1f + TitleMargin;
    }

    public class CatEntry : IExposable {
        public Categorizer cat;
        public bool active;
        public bool open = false;

        public CatEntry() {}

        public CatEntry(Categorizer cat, bool active) {
            this.cat = cat;
            this.active = active;
        }

        public void ExposeData() {
            Scribe_Deep.Look(ref cat, "cat");
            Scribe_Values.Look(ref active, "active");
        }
    }
}
