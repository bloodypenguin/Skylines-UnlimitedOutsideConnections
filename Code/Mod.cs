using ICities;
using CitiesHarmony.API;


namespace UOCRevisited
{
    /// <summary>
    /// The base mod class for instantiation by the game.
    /// </summary>
    public class UOCRMod : IUserMod
    {
        // Public mod name and description.
        public string Name => ModName + " " + Version;
        public string Description => "Place as many roads with outside connections as you want";


        // Internal and private name and version components.
        internal static string ModName => "Unlimited Outside Connections Revisited";
        internal static string Version => BaseVersion + " " + Beta;
        internal static string Beta => "BETA 2";
        private static string BaseVersion => "1.0";


        /// <summary>
        /// Called by the game when the mod is enabled.
        /// </summary>
        public void OnEnabled()
        {
            // Apply Harmony patches via Cities Harmony.
            // Called here instead of OnCreated to allow the auto-downloader to do its work prior to launch.
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }


        /// <summary>
        /// Called by the game when the mod is disabled.
        /// </summary>
        public void OnDisabled()
        {
            // Unapply Harmony patches via Cities Harmony.
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                Patcher.UnpatchAll();
            }
        }
    }
}
