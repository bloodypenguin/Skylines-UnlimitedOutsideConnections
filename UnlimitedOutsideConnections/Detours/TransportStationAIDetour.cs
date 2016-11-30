using UnlimitedOutsideConnections.Redirection.Attributes;

namespace UnlimitedOutsideConnections.Detours
{
    [TargetType(typeof(TransportStationAI))]
    public class TransportStationAIDetour
    {
        [RedirectReverse]
        public static void CreateConnectionLines(TransportStationAI ai, ushort buildingID, ref Building data, ushort targetID, ref Building target, int gateIndex)
        {
            UnityEngine.Debug.LogError($"UnlimitedOutsideConnections - TransportStationAIDetour - failed to detour CreateConnectionLines");
        }

        [RedirectReverse]
        public static void ReleaseVehicles(TransportStationAI ai, ushort buildingID, ref Building data)
        {
            UnityEngine.Debug.LogError($"UnlimitedOutsideConnections - TransportStationAIDetour - failed to detour ReleaseVehicles");
        }
    }
}