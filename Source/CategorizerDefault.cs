using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    public class CategorizerDefault : CategorizerSingleton {
        private const string DefaultName = "Thing categories";
        private const string DefaultDesc = "Place each item in a sub-menu corresponding to the first thing category it belongs to.";
        public static readonly CategorizerDefault Instance = new CategorizerDefault();

        private CategorizerDefault() : base(DefaultName, DefaultDesc) {}

        public override bool AppliesTo(BillMenuEntry entry, bool first)
            => first && (entry.Recipe.ProducedThingDef?.thingCategories?.Any() ?? false);

        public override IEnumerable<MenuNode> Apply(BillMenuEntry entry, MenuNode parent, MenuNode root, bool first) {
            var n = parent;
            var category = entry.Recipe.ProducedThingDef.thingCategories[0];
            foreach (var cat in category.Parents.Reverse().AddItem(category)) {
                if (!Settings.IsDisabled(cat)) {
                    n = n.For(cat);
                }
            }
            yield return n;
        }
    }
}
