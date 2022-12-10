using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CategorizedBillMenus {
    public abstract class RuleActionExtra : RuleAction {
        protected RuleActionExtra(bool copies, string name, string description) 
            : base(copies, name, description) {}

        protected abstract IEnumerable<string> Categories(BillMenuEntry entry);

        public override MenuNode Apply(BillMenuEntry entry, MenuNode parent, MenuNode root) {
            foreach (var category in Categories(entry)) {
                parent = parent.For(category);
            }
            return parent;
        }
    }
}
