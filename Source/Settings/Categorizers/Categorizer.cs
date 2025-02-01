using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CategorizedBillMenus;
public abstract class Categorizer : Registerable<Categorizer> {
    protected Categorizer(string name, string description, bool editable) 
        : base(name, description, editable) {}

    public abstract bool AppliesTo(BillMenuEntry entry, bool first);

    public abstract IEnumerable<MenuNode> Apply(
        BillMenuEntry entry, MenuNode parent, MenuNode root, bool first);

    public abstract bool Singleton { get; }

    public virtual IEnumerable<Categorizer> Presets 
        => [];

    public abstract void DoSettings(Rect rect, ref float curY);
}
