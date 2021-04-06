using System.Collections.Generic;
using ColossalFramework;


namespace UOCRevisited
{
    public static class BuildingUtil
    {
        /// <summary>
        /// Returns a list of all service buildings matching the specified outside connection.
        /// </summary>
        /// <param name="connectionBuildingID"></param>
        /// <returns>New list of service buildings (empty list if none)</returns>
        public static IEnumerable<ushort> FindServiceBuildings(uint connectionBuildingID)
        {
            // Return list.
            List<ushort> buildingList = new List<ushort>();

            // Need valid building ID.
            if (connectionBuildingID == 0)
            {
                // Invalid building ID - return empty list.
                return buildingList;
            }

            // Local references.
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            Building[] buildingBuffer = buildingManager.m_buildings.m_buffer;
            BuildingInfo buildingInfo = buildingBuffer[connectionBuildingID].Info;
            OutsideConnectionAI connectionAI = buildingInfo?.GetAI() as OutsideConnectionAI;

            // Make sure this is an outside connection building with the PublicTransport service.
            if (connectionAI == null || buildingInfo.GetService() != ItemClass.Service.PublicTransport)
            {
                // Not a valid connection - return empty list.
                return buildingList;
            }

            // Get all current public transport service buildings.
            FastList<ushort> serviceBuildings = buildingManager.GetServiceBuildings(ItemClass.Service.PublicTransport);
            if (serviceBuildings == null || serviceBuildings.m_size == 0)
            {
                // No public transport buildings - return empty list.
                return buildingList;
            }

            // Iterate through each public transport building, looking for a subservice match with this one.
            ItemClass.Service service = buildingInfo.GetService();
            ItemClass.SubService subService = buildingInfo.GetSubService();
            foreach (ushort buildingID in serviceBuildings)
            {
                BuildingInfo serviceInfo = buildingBuffer[buildingID].Info;

                // Note that intercity bus routes need to match to roads (no direct subservice match).
                if (buildingBuffer[buildingID].m_flags != Building.Flags.None && serviceInfo != null && ((service == ItemClass.Service.Road && serviceInfo.GetSubService() == ItemClass.SubService.PublicTransportBus) || serviceInfo.GetSubService() == subService))
                {
                    buildingList.Add(buildingID);
                }
            }

            return buildingList;
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