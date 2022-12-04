using FloatSubMenus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    public abstract class Registerable<T> : IExposable where T : Registerable<T> {
        private string name;
        private string description;

        private readonly bool editable;

        private static readonly List<T> available = new List<T>();
        private static bool availableDirty = false;

        protected Registerable(string name, string description, bool editable = false) {
            this.name = name;
            this.description = description;
            this.editable = editable;
        }

        public string Name => name;
        public string Description => description;

        protected static void Register(T add) {
            available.Add(add);
            availableDirty = true;
        }

        public static IEnumerable<T> Available {
            get {
                if (availableDirty) {
                    available.SortBy(x => x.Name);
                }
                return available;
            }
        }

        public static void SelectionMenu(Action<T> set, T notIfSame = null) {
            var menu = Available.Select(x => MenuOption(x, set, notIfSame, 0));
            Find.WindowStack.Add(new FloatMenu(menu.ToList()));
        }

        private static FloatMenuOption MenuOption(T elem, Action<T> set, T notIfSame, int level) {
            var subOptions = elem.SubOptions(level);
            if (subOptions.Any()) {
                var subMenu = subOptions.Select(x => MenuOption(x, set, notIfSame, level + 1));
                return new FloatSubMenu(
                    elem.Name,
                    subMenu.ToList());
            } else {
                return new FloatMenuOption(
                    elem.Name,
                    () => { if (elem.GetType() != notIfSame?.GetType()) set(elem); },
                    mouseoverGuiAction: r => TooltipHandler.TipRegion(r, elem.Description));
            }
        }

        protected virtual IEnumerable<T> SubOptions(int level) => Enumerable.Empty<T>();

        public static void AddMenu(List<T> list) => SelectionMenu(list.Add);

        public abstract T Copy();

        public void DoName(Rect rect, bool edit) {
            if (edit && editable) {
                rect.width += 8f;
                name = Widgets.TextField(rect, name);
            } else {
                Widgets.Label(rect, name);
            }
        }

        public virtual void ExposeData() {
            if (editable) {
                Scribe_Values.Look(ref name, "name");
                Scribe_Values.Look(ref description, "description");
            }
        }
    }
}
