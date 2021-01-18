using HarmonyLib;

namespace UnlimitedOutsideConnections.Patches
{
    [HarmonyPatch(typeof(BuildingManager))]
    [HarmonyPatch("CalculateOutsideConnectionCount")]
    public static class CalculateOutsideConnectionCountPatch
    {
        public static void Postfix(ItemClass.Service service, ItemClass.SubService subService, ref int incoming, ref int outgoing)
        {
            if (incoming > 3)
            {
                incoming = 3;
            }
            if (outgoing > 3)
            {
                outgoing = 3;
            }
        }
    }
}