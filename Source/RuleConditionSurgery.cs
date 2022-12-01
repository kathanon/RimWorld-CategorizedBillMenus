using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CategorizedBillMenus {
    public class RuleConditionSurgery : RuleCondition {
        public readonly static RuleConditionSurgery Instance = new RuleConditionSurgery();

        public override bool Test(BillMenuEntry entry, bool _) => entry.Recipe.IsSurgery;

        public override bool Test(BillMenuEntry entry, MenuNode _) => true;

        public override RuleCondition Copy() => this;
    }
}
