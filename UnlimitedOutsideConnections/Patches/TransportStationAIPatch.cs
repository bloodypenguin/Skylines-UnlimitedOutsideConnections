using System;
using HarmonyLib;


namespace UnlimitedOutsideConnections.Patches
{
    [HarmonyPatch]
    public class TransportStationAIPatch //serves only for accessing TransportStationAI's private methods
    {
        [HarmonyReversePatch]
        [HarmonyPatch((typeof(TransportStationAI)), "CreateConnectionLines")]
        [HarmonyPatch(new Type[] { typeof(ushort), typeof(Building), typeof(ushort), typeof(Building), typeof(int) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        public static void CreateConnectionLines(TransportStationAI ai, ushort buildingID, ref Building data, ushort targetID, ref Building target, int gateIndex)
        {
            UnityEngine.Debug.LogError($"UnlimitedOutsideConnections - TransportStationAIDetour - failed to detour CreateConnectionLines");
        }
    }
}