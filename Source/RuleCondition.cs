using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CategorizedBillMenus {
    public abstract class RuleCondition {
        protected bool onCopied = false;
        protected bool onMoved  = true;

        public bool OnCopied => onCopied;
        public bool OnMoved  => onMoved;

        public abstract bool Test(BillMenuEntry entry, bool first);

        public abstract bool Test(BillMenuEntry entry, MenuNode parent);

        public abstract RuleCondition Copy();
    }
}
