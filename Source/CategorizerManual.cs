using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CategorizedBillMenus {
    public class CategorizerManual : CategorizerEditable {
        private const string ManualName = "Manual";
        private const string ManualDesc = "Lets you freely reorder recipies and categories, as well as add new categories.";

        public CategorizerManual() : base(ManualName, ManualDesc) {}

        public override bool AppliesTo(BillMenuEntry entry, bool first) => throw new NotImplementedException();
        public override IEnumerable<MenuNode> Apply(BillMenuEntry entry, MenuNode parent, MenuNode root, bool first) => throw new NotImplementedException();
        public override Categorizer Create() => throw new NotImplementedException();
    }
}
