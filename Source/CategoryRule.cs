using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CategorizedBillMenus {
    public class CategoryRule {
        private RuleCondition condition;
        private RuleAction action;

        public bool AllowAfter { get; protected set; }
        public bool Copies   => action.Copies;
        public bool OnCopied => condition.OnCopied;
        public bool OnMoved  => condition.OnMoved;

        public CategoryRule() {
            AllowAfter = false;
        }

        public CategoryRule(RuleCondition condition, RuleAction action) : this() {
            this.condition = condition;
            this.action = action;
        }

        public bool AppliesTo(BillMenuEntry entry, bool first) => condition?.Test(entry, first) ?? false;

        public virtual MenuNode Apply(BillMenuEntry entry, MenuNode parent, MenuNode root) {
            if (condition.Test(entry, parent)) {
                return action.Apply(entry, parent, root);
            } else {
                return parent;
            }
        }

        public bool AppliesOn(bool copy, bool moved) 
            => (!copy || OnCopied) && (!moved || OnMoved);

        public CategoryRule Copy() => new CategoryRule(condition.Copy(), action.Copy());
    }
}
