using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public abstract class ComparisonValueDef<T> : ComparisonValue where T : Def {
        private List<(string name, Func<T, string> get)> getters;
        private int index;

        protected ComparisonValueDef(string name, string description, params (string name, Func<T, string> get)[] getters) 
            : base(name, description) {
            this.getters = new List<(string name, Func<T, string> get)> {
                ( "label",    d => d.label ),
                ( "def name", d => d.defName ),
            };
            this.getters.AddRange(getters);
            index = 0;
        }

        protected ComparisonValueDef(string name, string description, int _) 
            : base(name, description) {}

        protected ComparisonValueDef<T> CopyTo(ComparisonValueDef<T> other) {
            other.getters = getters;
            other.index = index;
            return other;
        }

        protected Func<T, string> Getter => getters[index].get;

        protected abstract T GetDef(BillMenuEntry entry);

        public override string Get(BillMenuEntry entry) => Getter(GetDef(entry));

        public override string Get(BillMenuEntry entry, MenuNode parent) => Get(entry);

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref index, "getter");
        }
    }
}
