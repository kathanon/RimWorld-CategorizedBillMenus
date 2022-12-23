using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CategorizedBillMenus {
    public static class ScribeUtils {
        public static bool Saving             => Scribe.mode == LoadSaveMode.Saving;
        public static bool LoadingVars        => Scribe.mode == LoadSaveMode.LoadingVars;
        public static bool ResolvingCrossRefs => Scribe.mode == LoadSaveMode.ResolvingCrossRefs;
        public static bool PostLoadInit       => Scribe.mode == LoadSaveMode.PostLoadInit;
    }
}
