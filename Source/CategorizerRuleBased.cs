using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CategorizedBillMenus {
    public class CategorizerRuleBased : CategorizerEditable {
        private const string RuleBasedName = "Rule-based";
        private const string RuleBasedDesc = "Applies an editable list of simple rules.";

        public static readonly CategorizerRuleBased PresetByLimb =
            new CategorizerRuleBased(
                new List<CategoryRule> {
                    new CategoryRule(RuleConditionSurgery.Instance, new RuleActionByLimb()),
                }, 
                "Medical - By Body Part", 
                "Organizes operation bills after what body part they apply to.");

        private readonly List<CategoryRule> rules;
        private bool allowAfter;

        public CategorizerRuleBased() 
            : this(new List<CategoryRule>()) {}

        public CategorizerRuleBased(List<CategoryRule> rules) 
            : this(rules, RuleBasedName, RuleBasedDesc) {}

        private CategorizerRuleBased(List<CategoryRule> rules, string name, string description) 
            : base(name, description) {
            this.rules = rules;
        }

        public override bool AppliesTo(BillMenuEntry entry, bool first) 
            => (first || allowAfter) && rules.Any(r => r.AppliesTo(entry, first));

        public override IEnumerable<MenuNode> Apply(BillMenuEntry entry, MenuNode parent, MenuNode root, bool first) {
            var nodes = new List<MenuNode> { parent };
            bool moveLeft = true, moved = false;
            foreach (var rule in rules.Where(r => r.AppliesTo(entry, first))) {
                for (int i = 0, n = nodes.Count; i < n; i++) {
                    if (rule.AppliesOn(!moveLeft || i > 0, moved)) {
                        var node = rule.Apply(entry, parent, root);
                        if (nodes[i] != null) {
                            if (rule.AllowAfter) {
                                if (rule.Copies) {
                                    nodes.Add(node);
                                } else {
                                    nodes[i] = node;
                                }
                            } else { // AllowAfter
                                if (!rule.Copies) {
                                    nodes.RemoveAt(i);
                                    if (i == 0) moveLeft = false;
                                    n--;
                                    i--;
                                }
                                yield return node;
                            }
                        }
                    }
                }
            }
            foreach (var node in nodes) {
                yield return node;
            }
        }

        public override Categorizer Create() 
            => new CategorizerRuleBased(rules.Select(r => r.Copy()).ToList(),
                                        Name,
                                        Description);
    }
}
