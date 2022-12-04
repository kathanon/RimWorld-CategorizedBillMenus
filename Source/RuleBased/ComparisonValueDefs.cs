using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    using CombinerFunc = Func<IEnumerable<string>, Func<string, bool>, bool>;
    public abstract class ComparisonValueDefs<T> : ComparisonValueDef<T> where T : Def {
        private static readonly List<(string name, CombinerFunc func)> combiners = 
            new List<(string name, CombinerFunc)>{
                ("any", Enumerable.Any),
                ("all", Enumerable.All),
            };

        private int index = 0;

        protected ComparisonValueDefs(string name, string description, params (string name, Func<T, string> get)[] getters) 
            : base(name, description, getters) {}

        protected ComparisonValueDefs(string name, string description, int _) 
            : base(name, description, _) {}

        protected ComparisonValueDefs<T> CopyTo(ComparisonValueDefs<T> other) {
            other.index = index;
            base.CopyTo(other);
            return other;
        }

        protected CombinerFunc Combiner => combiners[index].func;

        public override bool Compare(Comparison comparison, BillMenuEntry entry, string expected)
            => Combiner(GetAll(entry), s => comparison.Compare(s, expected));

        public override bool Compare(Comparison comparison, BillMenuEntry entry, MenuNode parent, string expected)
            => true; // If first variant returned true, then this should as well

        protected override T GetDef(BillMenuEntry entry) 
            => throw new NotSupportedException();

        public virtual IEnumerable<string> GetAll(BillMenuEntry entry)
            => GetDefs(entry).Select(Getter);

        protected abstract IEnumerable<T> GetDefs(BillMenuEntry entry);

        public override string Get(BillMenuEntry entry)
            => GetAll(entry).FirstOrDefault();

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref index, "combiner");
        }
    }
}
