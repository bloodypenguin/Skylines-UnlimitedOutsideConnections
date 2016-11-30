using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using ColossalFramework.Math;
using UnlimitedOutsideConnections.Redirection.Attributes;

namespace UnlimitedOutsideConnections.Detours
{
    [TargetType(typeof(OutsideConnectionAI))]
    public class OutsideConnectionAIDetour : BuildingAI
    {
        [RedirectMethod]
        public override void CreateBuilding(ushort buildingID, ref Building data)
        {
            base.CreateBuilding(buildingID, ref data);

            data.m_flags |= Building.Flags.Active;
            Singleton<BuildingManager>.instance.AddOutsideConnection(buildingID);
            //begin mod
            SimulationManager.instance.AddAction(() =>
            {
                CreateOutsideConnectionLines(buildingID);
            });
            //end mod
        }

        private static void CreateOutsideConnectionLines(ushort buildingID)
        {
#if DEBUG
            Debug.Log($"UnlimitedOutsideConnections - CreateOutsideConnectionLines. buildingID={buildingID}");
#endif
            var instance = BuildingManager.instance;
            var serviceBuildings = FindServiceBuildings(instance.m_buildings.m_buffer[buildingID]);
            foreach (var id in serviceBuildings)
            {
                var ai = instance.m_buildings.m_buffer[id].Info.GetAI() as TransportStationAI;
                if (ai?.m_transportLineInfo == null)
                {
                    continue;
                }
                var gateIndex = 0;
                if (ai.m_spawnPoints != null && ai.m_spawnPoints.Length != 0)
                {
                    var randomizer = new Randomizer(id);
                    gateIndex = randomizer.Int32((uint)ai.m_spawnPoints.Length);
                }
                instance.m_buildings.m_buffer[buildingID].m_flags |= Building.Flags.IncomingOutgoing;
                TransportStationAIDetour.CreateConnectionLines(ai, id, ref instance.m_buildings.m_buffer[id], buildingID, ref instance.m_buildings.m_buffer[buildingID], gateIndex);
                TransportStationAIDetour.ReleaseVehicles(ai, id, ref instance.m_buildings.m_buffer[id]);
            }
        }

        private static IEnumerable<ushort> FindServiceBuildings(Building data)
        {
            if (data.Info.m_class.m_service != ItemClass.Service.PublicTransport)
            {
                return new List<ushort>();
            }
            var instance = BuildingManager.instance;
            var allServiceBuildings = instance.GetServiceBuildings(data.Info.m_class.m_service).ToArray();
            if (allServiceBuildings == null)
            {
                return new List<ushort>();
            }
            var subServiceBuildings = allServiceBuildings.Where(
                id =>
                {
                    var building = instance.m_buildings.m_buffer[id];
                    if (building.m_flags == Building.Flags.None || building.Info == null)
                    {
                        return false;
                    }
                    return building.Info.m_class.m_subService == data.Info.m_class.m_subService;
                });
            return subServiceBuildings;
        }

        [RedirectMethod]
        public override void ReleaseBuilding(ushort buildingID, ref Building data)
        {
#if DEBUG
            Debug.Log($"UnlimitedOutsideConnections - ReleaseBuilding. buildingID={buildingID}");
#endif
            //begin mod
            var outsideConnectionAI = data.Info?.m_buildingAI as OutsideConnectionAI;
            var m_dummyTrafficReason = outsideConnectionAI.m_dummyTrafficReason;
            //end mod

            OutsideConnectionAI.RemoveConnectionOffers(buildingID, ref data, m_dummyTrafficReason);
            Singleton<BuildingManager>.instance.RemoveOutsideConnection(buildingID);
            base.ReleaseBuilding(buildingID, ref data);

            //begin mod
            DisconnectBuildings(data);
            //end mod
        }

        private static void DisconnectBuildings(Building data)
        {
            var serviceBuildings = FindServiceBuildings(data);
            foreach (var id in serviceBuildings)
            {
                var ai = Singleton<BuildingManager>.instance.m_buildings.m_buffer[id].Info.GetAI() as TransportStationAI;
                if (ai == null)
                {
                    continue;
                }
                TransportStationAIDetour.ReleaseVehicles(ai, id, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[id]);
            }
        }
    }
}