using HarmonyLib;
using Verse;
using UnityEngine;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class Main : Mod {
        public static Main Instance { get; private set; }

        private Settings settings = null;

        public Settings Settings => 
            settings ?? (settings = GetSettings<Settings>().EnsureSetup());

        static Main() {
            var harmony = new Harmony(Strings.ID);
            harmony.PatchAll();
        }

        public Main(ModContentPack content) : base(content) {
            Instance = this;
        }

        public override void DoSettingsWindowContents(Rect inRect) => 
            Settings.DoWindowContents(inRect);

        public override string SettingsCategory() => Strings.Name;
    }
}
