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
        private HashSet<string> favorites = new HashSet<string>();
        private HashSet<string> disabledCats = new HashSet<string>();

        private readonly HashSet<RecipeDef> favoriteDefs = new HashSet<RecipeDef>();
        private readonly HashSet<ThingCategoryDef> disabledCatDefs = new HashSet<ThingCategoryDef>();

        private static Settings inst = null;

        public static Settings Instance => inst ?? (inst = Main.Instance.Settings);

        internal static bool IsFav(RecipeDef def) => Instance.IsFavInst(def);
        internal bool IsFavInst(RecipeDef def) => favoriteDefs.Contains(def);

        internal static void ToggleFav(RecipeDef def) => Instance.ToggleFavInst(def);
        internal void ToggleFavInst(RecipeDef def) {
            if (favoriteDefs.Remove(def)) {
                favorites.Remove(def.defName);
            } else {
                favoriteDefs.Add(def);
                favorites.Add(def.defName);
            }
            Write();
        }

        internal static bool IsDisabled(ThingCategoryDef def) => Instance.IsDisabledInst(def);
        internal bool IsDisabledInst(ThingCategoryDef def) => disabledCatDefs.Contains(def);

        internal static void ToggleDisabled(ThingCategoryDef def) => Instance.ToggleDisabledInst(def);
        internal void ToggleDisabledInst(ThingCategoryDef def) {
            if (disabledCatDefs.Remove(def)) {
                disabledCats.Remove(def.defName);
            } else {
                disabledCatDefs.Add(def);
                disabledCats.Add(def.defName);
            }
        }

        internal static void ToggleInactiveDisabled(string defName) => Instance.ToggleInactiveDisabledInst(defName);
        internal void ToggleInactiveDisabledInst(string defName) {
            if (!disabledCats.Remove(defName)) {
                disabledCats.Add(defName);
            }
        }

        private readonly List<string> inactive = new List<string>();
        internal List<string> InactiveDisabled => inactive;


        // Options GUI

        public const float ArrowSize    = 18f;
        public const float TitleMargin  =  5f;
        public const float ScrollMargin = 16f;
        public static readonly Color TitleHRColor = Color.gray;

        private bool open = true;
        private float height = 0f;
        private Vector2 scroll = Vector2.zero;

        public void DoWindowContents(Rect rect) {
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
            // Test
            if (FoldableSection(rect, "Title", ref open, ref curY)) {
                Widgets.Label(new Rect(rect.x, curY, rect.width, 24f), "Test test");
            }
            // TODO
            Text.Anchor = align;

            height = curY - rect.y;
            if (doScroll) {
                Widgets.EndScrollView();
            }
        }

        private bool FoldableSection(Rect rect, string title, ref bool open, ref float curY) {
            Text.Font = GameFont.Medium;
            var size = Text.CalcSize(title);
            float titleHeight = Mathf.Max(ArrowSize, size.y);
            float midY = curY + titleHeight / 2f;
            curY += titleHeight + TitleMargin;

            var arrowRect = new Rect(rect.x, midY - ArrowSize / 2f, ArrowSize, ArrowSize);
            if (Widgets.ButtonImage(arrowRect, open ? Textures.DownIcon : Textures.RightIcon)) {
                open = !open;
            }

            float titleX = rect.x + ArrowSize + TitleMargin;
            var titleRect = new Rect(titleX, midY - size.y / 2f, rect.xMax - titleX, size.y);
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

        public override void ExposeData() {
            Scribe_Collections.Look(ref favorites, "favorites", LookMode.Value);
            Scribe_Collections.Look(ref disabledCats, "disabled", LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit) {
                if (favorites == null) favorites = new HashSet<string>();
                if (disabledCats == null) disabledCats = new HashSet<string>();

                favoriteDefs.Clear();
                favoriteDefs.AddRange(DefDatabase<RecipeDef>.AllDefs.Where(d => favorites.Contains(d.defName)));
                disabledCatDefs.Clear();
                disabledCatDefs.AddRange(DefDatabase<ThingCategoryDef>.AllDefs.Where(d => disabledCats.Contains(d.defName)));
                disabledCatDefs.Add(ThingCategoryDefOf.Root);
                
                var inactiveTmp = new HashSet<string>();
                inactiveTmp.AddRange(disabledCats);
                foreach (var def in disabledCatDefs) { 
                    inactiveTmp.Remove(def.defName);
                }
                inactive.Clear();
                inactive.AddRange(inactiveTmp);
                inactive.Sort();
            }
        }
    }
}
