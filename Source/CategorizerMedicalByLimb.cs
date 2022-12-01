using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    public class CategorizerMedicalByLimb : CategorizerSingleton {
        private const string ByLimbName = "Operations - By body part";
        private const string ByLimbDesc = "Adds sub-menus for each body part, with all operations affecting that specific part.";
        private const string NoBodyPart = "General";
        public static readonly CategorizerMedicalByLimb Instance = new CategorizerMedicalByLimb();

        private CategorizerMedicalByLimb() : base(ByLimbName, ByLimbDesc) {}

        public override bool AppliesTo(BillMenuEntry entry, bool first)
            => entry.Recipe.IsSurgery;

        public override IEnumerable<MenuNode> Apply(BillMenuEntry entry, MenuNode parent, MenuNode root, bool first) {
            var part = entry.BodyPart;
            var n = parent.For(part?.def.LabelCap ?? NoBodyPart);
            if (part != null) n = n.For(part.LabelCap);
            yield return n;
        }
    }
}
