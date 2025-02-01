using System.Collections.Generic;

namespace CategorizedBillMenus;
public abstract class RuleActionExtra : RuleAction {
    protected RuleActionExtra(bool copies, string id, string name, string description) 
        : base(copies, id, name, description) {}

    protected abstract IEnumerable<string> Categories(BillMenuEntry entry);

    public override MenuNode Apply(BillMenuEntry entry, MenuNode parent, MenuNode root) {
        foreach (var category in Categories(entry)) {
            parent = parent.For(category);
        }
        return parent;
    }
}
