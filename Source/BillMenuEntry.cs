using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    public class BillMenuEntry {
        public readonly FloatMenuOption Option;
        public readonly RecipeDef Recipe;
        public readonly BodyPartRecord BodyPart;

        public BillMenuEntry(FloatMenuOption option, RecipeDef recipe) {
            Option = option;
            Recipe = recipe;
            BodyPart = null;
        }

        public BillMenuEntry(FloatMenuOption option, RecipeDef recipe, BodyPartRecord bodyPart) 
            : this(option, recipe) {
            BodyPart = bodyPart;
        }
    }
}
