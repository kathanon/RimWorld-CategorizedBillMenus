using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public abstract class TextValueDef<T> : TextValue where T : Def {
        private List<(string name, Func<T, string> get)> getters;
        private int index;

        protected TextValueDef(string name, string description, int getterIndex = 0, 
                params (string name, Func<T, string> get)[] getters) 
                : base(name, description) {
            this.getters = new List<(string name, Func<T, string> get)> {
                ( "label",    d => d.label ),
                ( "def-name", d => d.defName ),
            };
            this.getters.AddRange(getters);
            index = getterIndex;
        }

        protected TextValueDef(string name, string description, float _) 
            : base(name, description) {}

        protected TextValueDef<T> CopyTo(TextValueDef<T> other) {
            other.getters = getters;
            other.index = index;
            return other;
        }

        protected Func<T, string> Getter => getters[index].get;

        protected string GetterName => getters[index].name;

        protected abstract T GetDef(BillMenuEntry entry);

        public override string Get(BillMenuEntry entry) => Getter(GetDef(entry));

        public override string Get(BillMenuEntry entry, MenuNode parent) => Get(entry);

        public override void DoSettings(WidgetRow row, Rect rect, ref float curY) {
            row.SelectMenuButton(getters[index], getters, i => index = i, g => g.name);
        }

        public override string SettingsClosedLabel => $"{Name} {GetterName}";

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref index, "getter");
        }
    }
}
