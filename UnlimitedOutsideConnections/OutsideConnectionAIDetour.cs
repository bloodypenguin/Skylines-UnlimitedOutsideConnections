using System.Linq;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace UnlimitedOutsideConnections
{
    public class OutsideConnectionAIDetour : BuildingAI
    {

        private static bool _deployed;
        private static RedirectCallsState _state;
        private static MethodInfo _originalInfo;
        private static MethodInfo _detourInfo;

        private static MethodInfo _createConnectionLinesInfo;

        public static void Deploy()
        {
            if (_deployed) return;
            if (_createConnectionLinesInfo == null)
            {
                _createConnectionLinesInfo = typeof(TransportStationAI).GetMethod("CreateConnectionLines",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            }

            if (_originalInfo == null)
            {
                _originalInfo = typeof(OutsideConnectionAI).GetMethod("CreateBuilding");
            }
            if (_detourInfo == null)
            {
                _detourInfo = typeof(OutsideConnectionAIDetour).GetMethod("CreateBuilding");
            }
            _state = RedirectionHelper.RedirectCalls(_originalInfo, _detourInfo);
            _deployed = true;
        }

        public static void Revert()
        {
            if (!_deployed) return;
            if (_originalInfo != null && _detourInfo != null)
            {
                RedirectionHelper.RevertRedirect(_originalInfo, _state);
            }
            _deployed = false;
        }


        public override void CreateBuilding(ushort buildingID, ref Building data)
        {
            base.CreateBuilding(buildingID, ref data);

            data.m_flags |= Building.Flags.Active;
            Singleton<BuildingManager>.instance.AddOutsideConnection(buildingID);

            SimulationManager.instance.AddAction(() =>
            {
                CreateOutsideConnectionLines(buildingID);
            });
        }

        private static void CreateOutsideConnectionLines(ushort buildingID)
        {

            UnityEngine.Debug.Log("UnlimitedOutsideConnections - CreateOutsideConnectionLines");
            var instance = BuildingManager.instance;
            var data = instance.m_buildings.m_buffer[buildingID];
            var serviceBuildings = instance.GetServiceBuildings(data.Info.m_class.m_service).ToArray().Where(
                id =>
                {
                    var building = instance.m_buildings.m_buffer[id];
                    if (building.m_flags == Building.Flags.None || building.Info == null)
                    {
                        return false;
                    }
                    return building.Info.m_class.m_subService == data.Info.m_class.m_subService;
                });
            foreach (var id in serviceBuildings)
            {
                var ai = instance.m_buildings.m_buffer[id].Info.GetAI() as TransportStationAI;
                if (ai == null)
                {
                    continue;
                }
                UnityEngine.Debug.Log("UnlimitedOutsideConnections - Creating outisde connection line for building " + id);
                var b = 1;
                var num1 = 1;
                var gateIndex = 0;
                if (ai.m_spawnPoints != null && ai.m_spawnPoints.Length != 0)
                {
                    var randomizer = new Randomizer((int) id);
                    num1 = ai.m_spawnPoints.Length;
                    gateIndex = randomizer.Int32((uint) num1);
                }
                data.m_flags |= Building.Flags.IncomingOutgoing;
                var args = new object[] {id, instance.m_buildings.m_buffer[id], buildingID, data, gateIndex};
                _createConnectionLinesInfo.Invoke(ai, args);
            }
        }
    }
}