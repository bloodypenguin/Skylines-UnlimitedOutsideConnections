using UnlimitedOutsideConnections.Redirection.Attributes;

namespace UnlimitedOutsideConnections.Detours
{
    [TargetType(typeof(TransportStationAI))]
    public class TransportStationAIDetour //serves only for accessing TransportStationAI's private methods
    {
        [RedirectReverse]
        public static void CreateConnectionLines(TransportStationAI ai, ushort buildingID, ref Building data, ushort targetID, ref Building target, int gateIndex)
        {
            UnityEngine.Debug.LogError($"UnlimitedOutsideConnections - TransportStationAIDetour - failed to detour CreateConnectionLines");
        }
    }
}