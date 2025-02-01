using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus;
[StaticConstructorOnStartup]
public class RuleActionByMod : RuleActionExtra {
    static RuleActionByMod() {
        Register(new RuleActionByMod());
    }

    public RuleActionByMod() : this(false) { }

    private RuleActionByMod(bool copies)
        : base(copies, Strings.ActionByModName, Strings.ActionByModID, Strings.ActionByModDesc) { }

    public override RuleAction Copy() 
        => new RuleActionByMod(Copies);

    protected override IEnumerable<string> Categories(BillMenuEntry entry) {
        var mod = entry.Recipe?.modContentPack?.Name;
        if (mod != null) {
            yield return mod;
        }
    }
}

