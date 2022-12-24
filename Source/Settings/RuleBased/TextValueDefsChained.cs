using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    public abstract class TextValueDefsChained<TPrev, TNext>
        : TextValueDefs<TNext>, ITextValueDefChained<TPrev, TNext>
        where TPrev : Def
        where TNext : Def {

        protected static void RegisterGetter(Func<IDefValueGetter<TPrev>> create)
            => TextValueDef<TPrev>.RegisterGetter(create);

        protected TextValueDefsChained(
                string name, string id, string description, 
                int combinerIndex = 0, int getterIndex = 0, 
                params IDefValueGetter<TNext>[] getters) 
            : base(name, id, description, combinerIndex, getterIndex, getters) {
        }

        public bool Compare(TextOperation comparison, TPrev def, string expected) 
            => Compare(comparison, GetDefs(def), expected);

        public void CopyToGetter(IDefValueGetter<TPrev> other) => base.CopyToGetter(other);

        protected override IEnumerable<TNext> GetDefs(BillMenuEntry entry) 
            => Enumerable.Empty<TNext>();

        protected abstract IEnumerable<TNext> GetDefs(TPrev def);
    }

    public class TextValueDefsChainedConcrete<TPrev, TNext>
        : TextValueDefsChained<TPrev, TNext>
            where TPrev : Def
            where TNext : Def {

        private Func<TPrev, IEnumerable<TNext>> getDefs;

        public TextValueDefsChainedConcrete(string name,
                                            string id,
                                            string description,
                                            Func<TPrev, IEnumerable<TNext>> getDefs,
                                            int getterIndex,
                                            int combinerIndex,
                                            params IDefValueGetter<TNext>[] getters)
            : base(name, id, description, getterIndex, combinerIndex, getters) {
            this.getDefs = getDefs;
        }

        public override TextValue Copy()
            => CopyTo(new TextValueDefsChainedConcrete<TPrev, TNext>(Name, ID, Description, null, 0, 0));

        protected TextValueDefsChainedConcrete<TPrev, TNext>
                CopyTo(TextValueDefsChainedConcrete<TPrev, TNext> other) {
            other.getDefs = getDefs;
            base.CopyTo(other);
            return other;
        }

        protected override IEnumerable<TNext> GetDefs(TPrev def) => getDefs(def);
    }
}
