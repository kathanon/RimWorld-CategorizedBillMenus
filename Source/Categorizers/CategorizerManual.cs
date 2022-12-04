using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class CategorizerManual : CategorizerEditable {
        public CategorizerManual() : base(Strings.ManualName, Strings.ManualDesc) {}

        public override bool AppliesTo(BillMenuEntry entry, bool first) => throw new NotImplementedException();
        public override IEnumerable<MenuNode> Apply(BillMenuEntry entry, MenuNode parent, MenuNode root, bool first) => throw new NotImplementedException();
        public override Categorizer Copy() => throw new NotImplementedException();
        public override void DoSettings(Rect rect, float widthScroll, ref float curY) => throw new NotImplementedException();
        public override void ExposeData() => base.ExposeData();
    }
}
