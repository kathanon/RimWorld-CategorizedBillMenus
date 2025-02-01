using System.Collections.Generic;
using System.Linq;
using Verse;
using static CategorizedBillMenus.CurrentModeShorthands;

namespace CategorizedBillMenus;
public abstract class RegisterableById<T> : Registerable<T> where T : RegisterableById<T> {
    private readonly string id;

    protected RegisterableById(string name, string id, string description, bool editable = false) 
        : base(name, description, editable) {
        this.id = id;
    }

    public string ID => id;

    public static void Registerable_Look(ref T elem, string label) {
        if (Scribe.EnterNode(label)) {
            try {
                string id = null;
                if (!LoadingVars) id = elem?.ID;
                Scribe_Values.Look(ref id, "id");

                if (LoadingVars) {
                    elem = Available.FirstOrDefault(x => x.ID == id)?.Copy();
                }
                elem?.ExposeData();
            } finally {
                Scribe.ExitNode();
            }
        }
    }

    public static void Registerable_Look(ref List<T> list, string label) {
        if (Scribe.EnterNode(label)) {
            try {
                int n = 0;
                if (!LoadingVars) n = list.Count;
                Scribe_Values.Look(ref n, "n");
                if (LoadingVars) list = new List<T>(n);

                for (int i = 0; i < n; i++) {
                    T cond = LoadingVars ? null : list[i];
                    Registerable_Look(ref cond, Saving ? "li" : (i + 1).ToStringCached());
                    if (LoadingVars) list.Add(cond);
                }
            } finally {
                Scribe.ExitNode();
            }
        }
    }
}
