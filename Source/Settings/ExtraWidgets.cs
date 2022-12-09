using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public static class ExtraWidgets {
        public const float TextFieldExtra = 6f;
        public const float IconSize = Settings.IconSize;
        public const float Margin   = Settings.Margin;
        public const float IconStep = IconSize + Margin;
        public const float IconMid  = IconSize / 2;

        public static Texture2D[] collapseIcons = { TexButton.Reveal, TexButton.Collapse };
        public static string[]    collapseTips  = { Strings.OpenTooltip, Strings.ClosedTooltip };

        public static void ForceGap(this WidgetRow row, float width) {
            if (width > 0f) {
                row.ButtonRect(null, width - WidgetRow.ButtonExtraSpace);
            }
        }


        public static void CollapseButton(Rect rect, ref bool open) 
            => ToggleButton(rect, ref open, collapseIcons, collapseTips);

        public static void ToggleButton(
                Rect rect, ref bool value, Texture2D[] icons, string[] tips, Texture2D button = null, float iconXAdj = 0f) {
            int i = value ? 1 : 0;
            TooltipHandler.TipRegion(rect, tips[i]);
            Texture2D overlay = icons[i];
            if (button == null) {
                button = overlay;
                overlay = null;
            }
            if (Widgets.ButtonImage((iconXAdj == 0) ? rect : rect.ExpandedBy(iconXAdj, 0f), button)) {
                value = !value;
            }
            if (overlay != null) {
                Widgets.DrawTextureFitted(rect, overlay, 1f);
            }
        }


        public static bool TextField(
                this WidgetRow row, ref string text, float min = 80f, float max = 120f) {
            float curY = 0f;
            return TextField(row, ref text, ref curY, min, max);
        }

        public static bool TextField(
                this WidgetRow row, ref string text, ref float curY, float min = 80f, float max = 120f) {
            var width = Mathf.Clamp(Text.CalcSize(text).x + TextFieldExtra, min, max);
            var r = row.ButtonRect(text, width - WidgetRow.ButtonExtraSpace);
            r.height -= WidgetRow.LabelGap;
            var old = text;
            text = Widgets.TextField(r, text);
            curY = r.yMax + Margin;
            return old != text;
        }

        // TODO: If you switch something out, and the new one has some of the same fields,
        //       we should copy the values. Not sure if that should be done here or not.

        public static void SelectMenuButton<T>(
                this WidgetRow row, T cur, IEnumerable<T> list, Action<T> set, 
                Func<T, string> label, Func<T, string> description = null) {
            if (row.SelectButton(cur, label, description)) {
                SelectMenu(list, set, label, description);
            }
        }

        public static void SelectMenuButton<T>(
                this WidgetRow row, T cur, IEnumerable<T> list, Action<int> set, 
                Func<T, string> label, Func<T, string> description = null) {
            if (row.SelectButton(cur, label, description)) {
                SelectMenu(list, set, label, description);
            }
        }

        public static void SelectMenuButton<T>(this WidgetRow row, T cur, Action<T> set) 
                where T : Registerable<T> {
            if (row.SelectButton(cur, e => e?.Name, e => e?.Description)) {
                Registerable<T>.SelectionMenu(LocalSet, cur);
            }

            void LocalSet(T item) {
                item = item.Copy();
                if (cur != null) item.CopyFrom(cur);
                set(item);
            }
        }

        private static bool SelectButton<T>(
                this WidgetRow row, T cur, Func<T, string> label, Func<T, string> description = null) 
            => row.ButtonText(label(cur) ?? "(select)", description?.Invoke(cur));

        public static void SelectMenu(
                IEnumerable<ISettingsEntry> list, Action<ISettingsEntry> set) 
            => SelectMenu(list, (e, _) => set(e), e => e.Name, e => e.Description);

        public static void SelectMenu<T>(
                IEnumerable<T> list, Action<T> set, Func<T, string> label, Func<T, string> description = null) 
            => SelectMenu(list, (e, _) => set(e), label, description);

        public static void SelectMenu<T>(
                IEnumerable<T> list, Action<int> set, Func<T, string> label, Func<T, string> description = null)
            => SelectMenu(list, (_, i) => set(i), label, description);

        private static void SelectMenu<T>(
                IEnumerable<T> list, Action<T,int> set, Func<T, string> label, Func<T, string> description) {
            var menu = list.Select((elem, i) => new FloatMenuOption(
                label:              label(elem),
                action:             () => set(elem, i),
                mouseoverGuiAction: ToolTip(description?.Invoke(elem))));
            Find.WindowStack.Add(new FloatMenu(menu.ToList()));
        }

        private static Action<Rect> ToolTip(string tip) 
            => (tip == null) ? (Action<Rect>) null : (Rect r) => TooltipHandler.TipRegion(r, tip);

        public delegate void DoListItem<T>(T item, Rect rect, float offset, ref float curY);
        public delegate void DoListItemIndirect<T>(T item, Action<float> doButtons, Rect rect, float offset, ref float curY);

        public static void EditableList<T>(
                List<T> list, Action add, DoListItem<T> doItem, Rect rect, ref float curY)
            => ListImpl(list, add, doItem, null, rect, ref curY, false);

        public static void ReorderableList<T>(
                List<T> list, Action add, DoListItem<T> doItem, Rect rect, ref float curY)
            => ListImpl(list, add, doItem, null, rect, ref curY, true);

        public static void ReorderableList<T>(
                List<T> list, Action add, DoListItemIndirect<T> doItem, Rect rect, ref float curY) 
            => ListImpl(list, add, null, doItem, rect, ref curY, true);


        private static int draggedFrom = -1;
        private static int draggedTo = -1;
        private static object draggedList = null;
        private static void ListImpl<T>(List<T> list,
                                        Action add,
                                        DoListItem<T> doItem,
                                        DoListItemIndirect<T> doItemIndirect,
                                        Rect rect,
                                        ref float curY,
                                        bool reorder) {
            var dragging = ReferenceEquals(list, draggedList);
            bool hasAdd = add != null;
            var plusMinus = new Rect(rect.x, curY, IconSize, IconSize);
            var handle = plusMinus;
            if (hasAdd) handle.x += IconStep;
            int buttons = 0;
            if (reorder) buttons++;
            if (hasAdd) buttons++;
            var offset = buttons * IconStep;
            rect.xMin += offset;

            // Get info on drag in progress
            bool dragEvent = dragging && Event.current.type == EventType.MouseDrag;
            float mouseY = dragEvent ? Event.current.mousePosition.y : 0f;

            float prevY = -100000f;
            int i, remove = -1;
            for (i = 0; i < list.Count; i++) {
                // Check if drag position is just before this item and draw marker
                if (dragEvent && mouseY > prevY && mouseY < curY) {
                    draggedTo = (i > 0 && mouseY - prevY < curY - mouseY) ? i - 1 : i;
                } else if (dragging && draggedTo == i) {
                    DragLine(curY);
                }
                prevY = curY;

                // Render the item
                if (doItem != null) {
                    DoButtons(curY + IconMid);
                    doItem(list[i], rect, offset, ref curY);
                } else {
                    doItemIndirect(list[i], DoButtons, rect, offset, ref curY);
                }

                // Minimum step
                if (curY < prevY + IconStep) {
                    curY = prevY + IconStep;
                }
            }

            // Check if drag position is after the last item and draw marker
            if (dragEvent && mouseY > (prevY + curY) / 2) {
                draggedTo = list.Count;
            } else if (dragging && draggedTo == list.Count) {
                DragLine(curY);
            }

            // Add item button
            if (hasAdd) {
                plusMinus.y = curY;
                if (Widgets.ButtonImage(plusMinus, TexButton.Plus)) {
                    add();
                }
                curY += IconStep;
            }

            // Handle drop after drag
            if (dragging && Event.current.type == EventType.MouseUp && Event.current.button == 0) {
                if (draggedTo > draggedFrom) draggedTo--;
                if (draggedTo != draggedFrom && draggedTo >= 0 && draggedTo < list.Count) {
                    var entry = list[draggedFrom];
                    list.RemoveAt(draggedFrom);
                    list.Insert(draggedTo, entry);
                }
                draggedFrom = -1;
                draggedTo = -1;
                draggedList = null;
            }

            // If a remove button was clicked, remove that item
            if (remove >= 0) {
                list.RemoveAt(remove);
            }

            void DoButtons(float midY) {
                plusMinus.y = handle.y = midY - IconMid;

                // Remove button
                if (hasAdd && Widgets.ButtonImage(plusMinus, TexButton.Minus)) {
                    remove = i;
                }

                // Drag handle
                if (reorder && Widgets.ButtonImageDraggable(handle, TexButton.DragHash) == Widgets.DraggableResult.Dragged) {
                    draggedFrom = i;
                    draggedList = list;
                }
            }

            void DragLine(float y) 
                => Widgets.DrawLineHorizontal(rect.x, y - 2f, rect.width);
        }
    }
}
