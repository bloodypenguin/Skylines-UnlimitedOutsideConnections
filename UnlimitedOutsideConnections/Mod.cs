using ICities;
using CitiesHarmony.API;


namespace UnlimitedOutsideConnections
{
    public class Mod : IUserMod
    {
        public string Name => "UnlimitedOutsideConnections";
        public string Description => "Place as many roads with outside connections as you want";
        
        public void OnEnabled() {
            HarmonyHelper.EnsureHarmonyInstalled();
        }
    }
}
