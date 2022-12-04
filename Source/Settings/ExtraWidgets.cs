using FloatSubMenus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using static CategorizedBillMenus.ExtraWidgets;

namespace CategorizedBillMenus {
    public static class ExtraWidgets {
        public const float IconSize = Settings.IconSize;
        public const float Margin   = Settings.Margin;
        public const float IconStep = IconSize + Margin;

        public static void MenuButton(this WidgetRow row, ISettingsEntry cur, Action menu) {
            if (row.ButtonText(cur?.Name ?? "(select)", cur?.Description)) {
                menu();
            }
        }

        public static void SelectMenu<T>(IEnumerable<T> list, Func<T,string> label, Action<T> set) {
            var menu = list.Select(elem => new FloatMenuOption(label(elem), () => set(elem)));
            Find.WindowStack.Add(new FloatMenu(menu.ToList()));
        }

        public static void SelectMenu<T>(IEnumerable<T> list, Func<T, string> label, Action<T, int> set) {
            var menu = list.Select((elem, i) => new FloatMenuOption(label(elem), () => set(elem, i)));
            Find.WindowStack.Add(new FloatMenu(menu.ToList()));
        }

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
            var plusMinus = new Rect(rect.x, curY, IconSize, IconSize);
            var handle = plusMinus;
            handle.x += IconStep;
            var offset = (reorder ? 2 : 1) * IconStep;
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
                    DoButtons(curY + IconSize / 2);
                    doItem(list[i], rect, offset, ref curY);
                } else {
                    doItemIndirect(list[i], DoButtons, rect, offset, ref curY);
                }
            }

            // Check if drag position is after the last item and draw marker
            if (dragEvent && mouseY > (prevY + curY) / 2) {
                draggedTo = list.Count;
            } else if (dragging && draggedTo == list.Count) {
                DragLine(curY);
            }

            // Add item button
            plusMinus.y = curY;
            if (Widgets.ButtonImage(plusMinus, TexButton.Plus)) {
                add();
            }
            curY += IconStep;

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
                plusMinus.y = handle.y = midY - IconSize / 2;

                // Remove button
                if (Widgets.ButtonImage(plusMinus, TexButton.Minus)) {
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
