using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    public abstract class Categorizer {
        public string Name { get; protected set; }
        public string Description { get; protected set; }

        protected Categorizer(string name, string description) {
            Name = name;
            Description = description;

        }

        public abstract bool AppliesTo(BillMenuEntry entry, bool first);

        public abstract IEnumerable<MenuNode> Apply(
            BillMenuEntry entry, MenuNode parent, MenuNode root, bool first);

        public abstract bool Singleton { get; }

        public virtual IEnumerable<Categorizer> Presets 
            => Enumerable.Empty<Categorizer>();

        public abstract Categorizer Create();
    }
}
