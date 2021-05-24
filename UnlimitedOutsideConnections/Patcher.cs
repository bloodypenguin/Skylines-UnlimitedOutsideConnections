using CitiesHarmony.API;
using HarmonyLib;

namespace UnlimitedOutsideConnections
{
    /// <summary>
    /// Class to manage the mod's Harmony patches.
    /// </summary>
    internal static class Patcher
    {
        // Unique harmony identifier.
        private const string HarmonyId = "github.com/bloodypenguin/Skylines-UnlimitedOutsideConnections";

        // Flag.
        internal static bool Patched => _patched;
        private static bool _patched = false;


        /// <summary>
        /// Apply all Harmony patches.
        /// </summary>
        public static void PatchAll()
        {
            // Don't do anything if already patched.
            if (!_patched)
            {
                // Ensure Harmony is ready before patching.
                if (HarmonyHelper.IsHarmonyInstalled)
                {
                    // Apply all annotated patches and update flag.
                    Harmony harmonyInstance = new Harmony(HarmonyId);
                    harmonyInstance.PatchAll();
                    _patched = true;
                }
            }
        }


        public static void UnpatchAll()
        {
            // Only unapply if patches appplied.
            if (_patched)
            {
                // Unapply patches, but only with our HarmonyID.
                Harmony harmonyInstance = new Harmony(HarmonyId);
                harmonyInstance.UnpatchAll(HarmonyId);
                _patched = false;
            }
        }
    }
}