using System.Collections.Generic;
using System.Linq;
using ColossalFramework;

namespace UOCRevisited
{
    public static class BuildingUtil
    {
        public static IEnumerable<ushort> FindServiceBuildings(uint connectionBuildingID)
        {
            if (connectionBuildingID < 1)
            {
                return new List<ushort>();
            }
            var info = BuildingManager.instance.m_buildings.m_buffer[connectionBuildingID].Info;
            var ai = info?.m_buildingAI as OutsideConnectionAI;
            if(ai == null || info.m_class.m_service != ItemClass.Service.PublicTransport) { 
                return new List<ushort>();
            }
            var instance = BuildingManager.instance;
            var allServiceBuildings = instance.GetServiceBuildings(ItemClass.Service.PublicTransport);
            if (allServiceBuildings == null)
            {
                return new List<ushort>();
            }
            var subServiceBuildings = allServiceBuildings.ToArray().Where(
                id =>
                {
                    var building = instance.m_buildings.m_buffer[id];
                    if (building.m_flags == Building.Flags.None || building.Info == null)
                    {
                        return false;
                    }
                    return building.Info.m_class.m_subService == info.m_class.m_subService;
                }).ToArray();
            return subServiceBuildings;
        }

        public static void ReleaseTargetedVehicles(ushort buildingID)
        {
            if (buildingID < 1)
            {
                return;
            }
            var ai = BuildingManager.instance.m_buildings.m_buffer[buildingID].Info?.m_buildingAI;
            if (ai == null)
            {
                return;
            }

            var instance = Singleton<VehicleManager>.instance;
            uint maxCount = System.Math.Min(instance.m_vehicles.m_size, 65535);

            for (ushort i = 1; i < maxCount; i++)
            {
                if (instance.m_vehicles.m_buffer[i].m_sourceBuilding == buildingID ||
                    instance.m_vehicles.m_buffer[i].m_targetBuilding == buildingID)
                {
                    instance.ReleaseVehicle(i);
                }
            }

        }

        public static void ReleaseOwnVehicles(ushort buildingID)
        {
            if (buildingID < 1)
            {
                return;
            }
            var data = BuildingManager.instance.m_buildings.m_buffer[buildingID];
            var buildingInfo = data.Info;
            var ai = buildingInfo?.m_buildingAI;
            if (ai == null)
            {
                return;
            }

            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort vehicleID = data.m_ownVehicles;
            int num = 0;
            while ((int)vehicleID != 0)
            {
                if ((int)instance.m_vehicles.m_buffer[(int)vehicleID].m_transportLine == 0)
                {
                    VehicleInfo info = instance.m_vehicles.m_buffer[(int)vehicleID].Info;
                    if (info.m_class.m_service == buildingInfo.m_class.m_service && info.m_class.m_subService == buildingInfo.m_class.m_subService)
                        info.m_vehicleAI.SetTarget(vehicleID, ref instance.m_vehicles.m_buffer[(int)vehicleID], (ushort)0);
                }
                vehicleID = instance.m_vehicles.m_buffer[(int)vehicleID].m_nextOwnVehicle;
                if (++num > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }
        }
    }
}