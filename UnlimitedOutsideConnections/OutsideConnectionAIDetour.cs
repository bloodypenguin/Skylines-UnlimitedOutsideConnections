using System;
using System.Collections.Generic;
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
        private static RedirectCallsState _state1;
        private static MethodInfo _originalInfo1;
        private static MethodInfo _detourInfo1;

        private static RedirectCallsState _state2;
        private static MethodInfo _originalInfo2;
        private static MethodInfo _detourInfo2;

        private static MethodInfo _createConnectionLinesInfo;
        private static MethodInfo _releaseVehiclesInfo;

        public static void Deploy()
        {
            if (_deployed) return;
            if (_createConnectionLinesInfo == null)
            {
                _createConnectionLinesInfo = typeof(TransportStationAI).GetMethod("CreateConnectionLines",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            }
            if (_releaseVehiclesInfo == null)
            {
                _releaseVehiclesInfo = typeof(TransportStationAI).GetMethod("ReleaseVehicles",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            }

            if (_originalInfo1 == null)
            {
                _originalInfo1 = typeof(OutsideConnectionAI).GetMethod("CreateBuilding");
            }
            if (_detourInfo1 == null)
            {
                _detourInfo1 = typeof(OutsideConnectionAIDetour).GetMethod("CreateBuilding");
            }
            _state1 = RedirectionHelper.RedirectCalls(_originalInfo1, _detourInfo1);
            if (_originalInfo2 == null)
            {
                _originalInfo2 = typeof(OutsideConnectionAI).GetMethod("ReleaseBuilding");
            }
            if (_detourInfo2 == null)
            {
                _detourInfo2 = typeof(OutsideConnectionAIDetour).GetMethod("ReleaseBuilding");
            }
            _state1 = RedirectionHelper.RedirectCalls(_originalInfo2, _detourInfo2);
            _deployed = true;
        }

        public static void Revert()
        {
            if (!_deployed) return;
            if (_originalInfo1 != null && _detourInfo1 != null)
            {
                RedirectionHelper.RevertRedirect(_originalInfo1, _state1);
            }
            if (_originalInfo2 != null && _detourInfo2 != null)
            {
                RedirectionHelper.RevertRedirect(_originalInfo1, _state2);
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

            Debug.Log("UnlimitedOutsideConnections - CreateOutsideConnectionLines");
            var instance = BuildingManager.instance;
            var serviceBuildings = FindServiceBuildings(instance.m_buildings.m_buffer[buildingID]);
            foreach (var id in serviceBuildings)
            {
                var ai = instance.m_buildings.m_buffer[id].Info.GetAI() as TransportStationAI;
                if (ai == null)
                {
                    continue;
                }
                Debug.Log("UnlimitedOutsideConnections - Creating outside connection line for building " + id);
                var gateIndex = 0;
                if (ai.m_spawnPoints != null && ai.m_spawnPoints.Length != 0)
                {
                    var randomizer = new Randomizer(id);
                    gateIndex = randomizer.Int32((uint)ai.m_spawnPoints.Length);
                }
                instance.m_buildings.m_buffer[buildingID].m_flags |= Building.Flags.IncomingOutgoing;
                CreateConnectionLines(ai, id, ref instance.m_buildings.m_buffer[id], buildingID, ref instance.m_buildings.m_buffer[buildingID], gateIndex);
                ReleaseVehicles(ai, id, ref instance.m_buildings.m_buffer[id]);
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

        public override void ReleaseBuilding(ushort buildingID, ref Building data)
        {
            OutsideConnectionAI.RemoveConnectionOffers(buildingID, ref data, TransferManager.TransferReason.None);
            var instance = Singleton<BuildingManager>.instance;
            instance.RemoveOutsideConnection(buildingID);
            base.ReleaseBuilding(buildingID, ref data);
            var serviceBuildings = FindServiceBuildings(data);
            foreach (var id in serviceBuildings)
            {
                var ai = instance.m_buildings.m_buffer[id].Info.GetAI() as TransportStationAI;
                if (ai == null)
                {
                    continue;
                }
                Debug.Log("UnlimitedOutsideConnections - Releasing vehicles of building " + id);
                ReleaseVehicles(ai, id, ref instance.m_buildings.m_buffer[id]);
            }
        }

        private static void CreateConnectionLines(TransportStationAI ai, ushort buildingID, ref Building data, ushort targetID,
            ref Building target, int gateIndex)
        {
            var args = new object[] { buildingID, data, targetID, target, gateIndex };
            _createConnectionLinesInfo.Invoke(ai, args);
            data = (Building)args[1];
            target = (Building)args[3];
        }

        private static void ReleaseVehicles(TransportStationAI ai, ushort buildingID, ref Building data)
        {
            var args = new object[] { buildingID, data };
            _releaseVehiclesInfo.Invoke(ai, args);
            data = (Building)args[1];
        }
    }
}