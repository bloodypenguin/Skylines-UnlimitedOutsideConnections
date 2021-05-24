using System;
using System.Runtime.CompilerServices;
using HarmonyLib;


namespace UnlimitedOutsideConnections.Patches
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
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void CreateConnectionLines(TransportStationAI ai, ushort buildingID, ref Building data)
        {
            string message = "TransportStationAI.CreateConnectionLines reverse Harmony patch wasn't applied";
            throw new NotImplementedException(message);
        }
    }
}