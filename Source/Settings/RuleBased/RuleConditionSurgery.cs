using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class RuleConditionSurgery : RuleCondition {
        static RuleConditionSurgery() {
            Register(new RuleConditionSurgery());
        }

        public RuleConditionSurgery() : base(Strings.CondSurgeryName, Strings.CondSurgeryDesc) {}

        public readonly static RuleConditionSurgery Instance = new RuleConditionSurgery();

        public override bool Test(BillMenuEntry entry, bool _) => entry.Recipe.IsSurgery;

        public override bool Test(BillMenuEntry entry, MenuNode _) => entry.Recipe.IsSurgery;

        public override RuleCondition Copy() => this;
    }
}
