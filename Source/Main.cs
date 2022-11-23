using HarmonyLib;
using Verse;
using UnityEngine;

namespace CategorizedBillMenus {
    [StaticConstructorOnStartup]
    public class Main : Mod {
        public static Main Instance { get; private set; }

        public static void LoadSettings() => Instance.GetSettings<Settings>();

        public Settings Settings => GetSettings<Settings>();

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

    [HarmonyPatch(typeof(PlayDataLoader), nameof(PlayDataLoader.LoadAllPlayData))]
    public static class GameStartedHook {
        [HarmonyPostfix]
        public static void Hook() => Main.LoadSettings();
    }
}
