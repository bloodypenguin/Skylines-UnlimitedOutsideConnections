using System;
using HarmonyLib;


namespace UOCRevisited.Patches
{
    /// <summary>
    /// Harmony reverse patch to access TransportStationAI.CreateConnectionLines (private method).
    /// </summary>
    [HarmonyPatch]
    public class TransportStationAIPatch //serves only for accessing TransportStationAI's private methods
    {
        [HarmonyReversePatch]
        [HarmonyPatch((typeof(TransportStationAI)), "CreateConnectionLines")]
        [HarmonyPatch(new Type[] { typeof(ushort), typeof(Building), typeof(ushort), typeof(Building), typeof(int) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        public static void CreateConnectionLines(TransportStationAI ai, ushort buildingID, ref Building data, ushort targetID, ref Building target, int gateIndex)
        {
            string message = "TransportStationAI.CreateConnectionLines reverse Harmony patch wasn't applied";
            Logging.Error(message);
            throw new NotImplementedException(message);
        }
    }
}