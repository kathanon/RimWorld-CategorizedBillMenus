using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class RuleActionNoop : RuleAction {
        public static readonly RuleActionNoop Instance = new RuleActionNoop();

        static RuleActionNoop() {
            Register(new RuleActionNoop());
        }

        private RuleActionNoop() : base(false, Strings.ActionNoopName, Strings.ActionNoopDesc) {}

        protected override bool FixedCopies => true;

        public override MenuNode Apply(BillMenuEntry entry, MenuNode parent, MenuNode root) => parent;

        public override RuleAction Copy() => this;
    }
}
