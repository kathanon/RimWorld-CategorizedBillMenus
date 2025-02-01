using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    using CombinerFunc = Func<IEnumerable<bool>, Func<bool, bool>, bool>;

    public abstract class TextValueDefs<T> : TextValueDef<T> where T : Def {
        private static readonly List<(string name, CombinerFunc func)> combiners = [
                ("any", Enumerable.Any),
                ("all", Enumerable.All),
            ];

        private int index = 0;

        protected TextValueDefs(string name,
                                string id,
                                string description,
                                int combinerIndex = 0,
                                int getterIndex = 0,
                                params IDefValueGetter<T>[] getters)
                : base(name, id, description, getterIndex, getters) {
            index = combinerIndex;
        }

        protected TextValueDefs<T> CopyTo(TextValueDefs<T> other) {
            other.index = index;
            base.CopyTo(other);
            return other;
        }

        protected CombinerFunc Combiner => combiners[index].func;

        protected string CombinerName => combiners[index].name;

        protected bool Compare(TextOperation comparison, IEnumerable<T> defs, string expected)
            => Combiner(defs.Select(d => Getter.Compare(comparison, d, expected)), v => v);

        public override bool Compare(TextOperation comparison, BillMenuEntry entry, string expected)
            => Compare(comparison, GetDefs(entry), expected);

        protected override T GetDef(BillMenuEntry entry) 
            => throw new NotSupportedException();

        protected abstract IEnumerable<T> GetDefs(BillMenuEntry entry);

        public override void DoSettings(WidgetRow row, Rect rect, ref float curY) {
            row.SelectMenuButton(combiners[index], combiners, i => index = i, c => c.name);
            base.DoSettings(row, rect, ref curY);
        }

        public override string SettingsClosedLabel => $"{Name} {CombinerName} {GetterClosedLabel}";

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref index, "combiner");
        }
    }

    public class TextValueDefsConcrete<T> : TextValueDefs<T> where T : Def {
        private Func<BillMenuEntry, IEnumerable<T>> getDefs;

        public TextValueDefsConcrete(string name,
                                     string id, 
                                     string description,
                                     Func<BillMenuEntry, IEnumerable<T>> getDefs,
                                     int getterIndex,
                                     int combinerIndex,
                                     params IDefValueGetter<T>[] getters)
            : base(name, id, description, getterIndex, combinerIndex, getters) {
            this.getDefs = getDefs;
        }

        public override TextValue Copy()
            => CopyTo(new TextValueDefsConcrete<T>(Name, ID, Description, null, 0, 0));

        protected TextValueDefsConcrete<T> CopyTo(TextValueDefsConcrete<T> other) {
            other.getDefs = getDefs;
            base.CopyTo(other);
            return other;
        }

        protected override IEnumerable<T> GetDefs(BillMenuEntry entry) => getDefs(entry);
    }
}
