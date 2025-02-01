using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CategorizedBillMenus {
    public static class CurrentModeShorthands {
        // Save & load
        public static bool Saving             => Scribe.mode == LoadSaveMode.Saving;
        public static bool LoadingVars        => Scribe.mode == LoadSaveMode.LoadingVars;
        public static bool ResolvingCrossRefs => Scribe.mode == LoadSaveMode.ResolvingCrossRefs;
        public static bool PostLoadInit       => Scribe.mode == LoadSaveMode.PostLoadInit;

        // UI modes
        public static bool Layout  => Event.current.type == EventType.Layout;
        public static bool Repaint => Event.current.type == EventType.Repaint;
    }
}
