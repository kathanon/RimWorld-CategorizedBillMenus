using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    using CombinerFunc = Func<IEnumerable<string>, Func<string, bool>, bool>;
    public abstract class TextValueDefs<T> : TextValueDef<T> where T : Def {
        private static readonly List<(string name, CombinerFunc func)> combiners = 
            new List<(string name, CombinerFunc)>{
                ("any", Enumerable.Any),
                ("all", Enumerable.All),
            };

        private int index = 0;

        protected TextValueDefs(string name,
                                      string description,
                                      int combinerIndex = 0,
                                      int getterIndex = 0,
                                      params (string name, Func<T, string> get)[] getters)
                : base(name, description, getterIndex, getters) {
            index = combinerIndex;
        }

        protected TextValueDefs(string name, string description, float _) 
            : base(name, description, 0f) {}

        protected TextValueDefs<T> CopyTo(TextValueDefs<T> other) {
            other.index = index;
            base.CopyTo(other);
            return other;
        }

        protected CombinerFunc Combiner => combiners[index].func;

        protected string CombinerName => combiners[index].name;

        public override bool Compare(TextOperation comparison, BillMenuEntry entry, string expected)
            => Combiner(GetAll(entry), s => comparison.Compare(s, expected));

        public override bool Compare(TextOperation comparison, BillMenuEntry entry, MenuNode parent, string expected)
            => true; // If first variant returned true, then this should as well

        protected override T GetDef(BillMenuEntry entry) 
            => throw new NotSupportedException();

        public virtual IEnumerable<string> GetAll(BillMenuEntry entry)
            => GetDefs(entry).Select(Getter);

        protected abstract IEnumerable<T> GetDefs(BillMenuEntry entry);

        public override string Get(BillMenuEntry entry)
            => GetAll(entry).FirstOrDefault();

        public override void DoSettings(WidgetRow row, Rect rect, ref float curY) {
            row.SelectMenuButton(combiners[index], combiners, i => index = i, c => c.name);
            base.DoSettings(row, rect, ref curY);
        }

        public override string SettingsClosedLabel => $"{Name} {CombinerName} {GetterName}";

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref index, "combiner");
        }
    }
}
