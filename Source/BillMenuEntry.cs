using Verse;

namespace CategorizedBillMenus; 
public class BillMenuEntry(FloatMenuOption option, RecipeDef recipe, Pawn pawn, BodyPartRecord bodyPart) {
    public readonly FloatMenuOption Option = option;
    public readonly RecipeDef Recipe = recipe;
    public readonly Pawn Pawn = pawn;
    public readonly BodyPartRecord BodyPart = bodyPart;

    public BillMenuEntry(FloatMenuOption option, RecipeDef recipe) 
        : this(option, recipe, null, null) {}
}
