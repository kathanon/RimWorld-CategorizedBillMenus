using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class TextValueParent : TextValue {
        static TextValueParent() {
            Register(new TextValueParent());
        }

        public TextValueParent() : base("parent menu", "Compare with the label of the current parent sub-menu.") {}

        public override TextValue Copy() => this;

        public override string Get(BillMenuEntry entry) => AlwaysMatchMarker;
        public override string Get(BillMenuEntry entry, MenuNode parent) => parent.Label;
    }
}
