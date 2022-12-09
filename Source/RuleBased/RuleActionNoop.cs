using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class RuleActionNoop : RuleAction {
        private const string ActionNoopName = "nothing";
        private const string ActionNoopDesc = "Do nothing. This can be used to skip the remaining rules in the module.";

        public static readonly RuleActionNoop Instance = new RuleActionNoop();

        static RuleActionNoop() {
            Register(new RuleActionNoop());
        }

        private RuleActionNoop() : base(false, ActionNoopName, ActionNoopDesc) {}

        protected override bool FixedCopies => true;

        public override MenuNode Apply(BillMenuEntry entry, MenuNode parent, MenuNode root) => parent;

        public override RuleAction Copy() => this;
    }
}
