using System;
using HarmonyLib;


#pragma warning disable IDE0060 // Remove unused parameter


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
        [HarmonyPatch(new Type[] { typeof(ushort), typeof(Building) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref })]
        public static void CreateConnectionLines(TransportStationAI ai, ushort buildingID, ref Building data)
        {
            string message = "TransportStationAI.CreateConnectionLines reverse Harmony patch wasn't applied";
            Logging.Error(message);
            throw new NotImplementedException(message);
        }
    }
}

#pragma warning restore IDE0060 // Remove unused parameter