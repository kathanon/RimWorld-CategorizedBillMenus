using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus; 
[StaticConstructorOnStartup]
public class RuleActionByLimb : RuleActionExtra {
    static RuleActionByLimb() {
        Register(new RuleActionByLimb());
    }

    private string noPart = Strings.NoBodyPartCat;
    private bool firstPartDef = true;
    private bool annotate = true;

    public RuleActionByLimb() : this(false) {}

    private RuleActionByLimb(bool copies) 
        : base(copies, Strings.ActionByLimbName, Strings.ActionByLimbID, Strings.ActionByLimbDesc) {}

    public override RuleAction Copy() 
        => new RuleActionByLimb(Copies) { noPart = noPart, firstPartDef = firstPartDef };

    protected override IEnumerable<string> Categories(BillMenuEntry entry) {
        var part = entry.BodyPart;
        var labelPart = part;
        if (part == null) {
            if (noPart.NullOrEmpty()) {
                yield break;
            } else {
                yield return noPart;
            }
        } else {
            if (part.def == BodyPartDefOf.Arm 
                    && part.parent.def == BodyPartDefOf.Shoulder) {
                part = part.parent;
            } else if (part.def == BodyPartDefOf.Shoulder 
                    && part.parts.Count(x => x.def == BodyPartDefOf.Arm) == 1) {
                labelPart = part.parts.First(x => x.def == BodyPartDefOf.Arm);
            }

            // TODO: Add option for if we should group them like this
            if (part.body.GetPartsWithDef(part.def).Count > 1) {
                yield return labelPart.def.LabelCap;
            }
            yield return Annotate(entry, part, labelPart);
        }
    }

    private string Annotate(BillMenuEntry entry, BodyPartRecord part, BodyPartRecord labelPart) {
        string label = labelPart.LabelCap;
        if (annotate) {
            var hediffs = entry.Pawn.health.hediffSet.hediffs
                .Where(Filter);

            if (hediffs.Any()) {
                var annotation = hediffs
                    .Select(Label)
                    .ToCommaList();
                label = Strings.ActionByLimbAnnotatePattern(label, annotation);
            }
        }
        return label;

        bool Filter(Hediff h) {
            if (part == null) return h.Part == null;

            for (var hPart = h.Part; hPart != null; hPart = hPart.parent) {
                if (hPart == part) return true;
            }
            return false;
        }

        string Label(Hediff h) {
            var hPart = h.Part;
            return (hPart == part || hPart == labelPart) 
                ? h.LabelCap 
                : $"{hPart.LabelCap} {h.Label.ToLower()}";
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref noPart, "noPart", Strings.NoBodyPartCat);
        Scribe_Values.Look(ref firstPartDef, "firstPartDef", true);
        Scribe_Values.Look(ref annotate, "annotate", true);
    }

    protected override void DoSettingsOpen(WidgetRow row, Rect rect, ref float curY) {
        row.SelectMenuButton(
            annotate,
            [false, true],
            x => annotate = x,
            x => x ? Strings.ActionByLimbAnnotate : Strings.ActionByLimbPlain,
            x => x ? Strings.ActionByLimbAnnotateDesc : Strings.ActionByLimbPlainDesc);
    }
}
