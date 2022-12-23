using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    public interface ITextValueDefChained<TPrev, TNext> 
        : IDefValueGetter<TPrev>
            where TPrev : Def
            where TNext : Def {
        public void SetGetter(IDefValueGetter<TNext> getter);
    }

    public abstract class TextValueDefChained<TPrev, TNext> 
        : TextValueDef<TNext>, ITextValueDefChained<TPrev, TNext>
            where TPrev : Def 
            where TNext : Def {

        protected static void RegisterGetter(Func<IDefValueGetter<TPrev>> create) 
            => TextValueDef<TPrev>.RegisterGetter(create);

        protected TextValueDefChained(string name, string id, string description, float _) 
            : base(name, id, description, _) {
        }

        protected TextValueDefChained(
                string name, string id, string description, int getterIndex = 0, 
                params IDefValueGetter<TNext>[] getters) 
            : base(name, id, description, getterIndex, getters) {
        }

        public bool Compare(TextOperation comparison, TPrev def, string expected)
            => Getter.Compare(comparison, GetDef(def), expected);

        public void CopyToGetter(IDefValueGetter<TPrev> other) 
            => base.CopyToGetter(other);

        protected override TNext GetDef(BillMenuEntry entry) 
            => throw new NotImplementedException();

        protected abstract TNext GetDef(TPrev def);
    }

    public class TextValueDefChainedConcrete<TPrev, TNext> 
        : TextValueDefChained<TPrev, TNext>
            where TPrev : Def
            where TNext : Def {

        private Func<TPrev, TNext> getDef;

        private TextValueDefChainedConcrete(string name, string id, string description)
            : base(name, id, description, 0f) { }

        public TextValueDefChainedConcrete(string name,
                                           string id,
                                           string description,
                                           Func<TPrev, TNext> getDef,
                                           int getterIndex,
                                           params IDefValueGetter<TNext>[] getters)
            : base(name, id, description, getterIndex, getters) {
            this.getDef = getDef;
        }

        public override TextValue Copy()
            => CopyTo(new TextValueDefChainedConcrete<TPrev, TNext>(Name, ID, Description));

        protected TextValueDefChainedConcrete<TPrev, TNext> 
                CopyTo(TextValueDefChainedConcrete<TPrev, TNext> other) {
            other.getDef = getDef;
            base.CopyTo(other);
            return other;
        }

        protected override TNext GetDef(TPrev def) => getDef(def);
    }
}
