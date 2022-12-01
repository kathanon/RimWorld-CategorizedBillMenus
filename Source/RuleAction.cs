using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CategorizedBillMenus {
    public abstract class RuleAction {
        protected bool copies;

        protected RuleAction(bool copies) {
            this.copies = copies;
        }

        public bool Copies => copies;

        public abstract MenuNode Apply(BillMenuEntry entry, MenuNode parent, MenuNode root);

        public abstract RuleAction Copy();
    }
}
