using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    public abstract class Categorizer : Registerable<Categorizer> {
        protected Categorizer(string name, string description, bool editable) 
            : base(name, description, editable) {}

        public abstract bool AppliesTo(BillMenuEntry entry, bool first);

        public abstract IEnumerable<MenuNode> Apply(
            BillMenuEntry entry, MenuNode parent, MenuNode root, bool first);

        public abstract bool Singleton { get; }

        public virtual IEnumerable<Categorizer> Presets 
            => Enumerable.Empty<Categorizer>();

        public abstract void DoSettings(Rect rect, float widthScroll, ref float curY);
    }
}
