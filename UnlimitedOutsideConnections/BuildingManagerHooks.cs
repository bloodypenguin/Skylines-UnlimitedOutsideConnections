using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.Math;
using UnlimitedOutsideConnections.Patches;


namespace UnlimitedOutsideConnections
{
    /// <summary>
    /// Event hooks for the building manager.
    /// </summary>
    internal static class BuildingManagerHooks
    {
        /// <summary>
        ///  Deploy event hooks for when the building manager creates and releases buildings.
        /// </summary>
        internal static void Deploy()
        {
            // Revert hooks if already applied.
            Revert();

            // Add hooks.
            BuildingManager.instance.EventBuildingCreated += OnBuildingCreated;
            BuildingManager.instance.EventBuildingReleased += OnBuildingReleased;
        }


        /// <summary>
        /// Removes event hooks.
        /// </summary>
        internal static void Revert()
        {
            BuildingManager.instance.EventBuildingCreated -= OnBuildingCreated;
            BuildingManager.instance.EventBuildingReleased -= OnBuildingReleased;
        }


        /// <summary>
        /// Event hook for BuildingManager.EventBuildingCreated.
        /// This makes sure that transport station AI buildings are 'aware' of the new connections and will create transport lines to and/or from them as appropriate.
        /// </summary>
        /// <param name="buildingID">ID of created building</param>
        public static void OnBuildingCreated(ushort buildingID)
        {
            // Local ref.
            BuildingManager instance = Singleton<BuildingManager>.instance;

            // Is this an outside connection?
            OutsideConnectionAI connectionAI = instance.m_buildings.m_buffer[buildingID].Info?.m_buildingAI as OutsideConnectionAI;
            if (connectionAI == null)
            {
                // No - don't do anything.
                return;
            }

            // Yes - this is an outside connection.
            Logging.Message("found building ", buildingID.ToString(), " with outside connection AI");

            // Find all service buildings in map and iterate through.
            IEnumerable<ushort> serviceBuildings = BuildingUtil.FindServiceBuildings(buildingID);
            foreach (ushort serviceID in serviceBuildings)
            {
                // Check if this building has transport line info.
                TransportStationAI stationAI = instance.m_buildings.m_buffer[serviceID].Info.GetAI() as TransportStationAI;
                if (stationAI?.m_transportLineInfo == null)
                {
                    // No transport line info - skip to next building.
                    continue;
                }

                // Transport line building - need to update it to include the new connection.
                int gateIndex = 0;

                // Randomize spawnpoint for buildings with multiple spawnpoints.
                if (stationAI.m_spawnPoints != null && stationAI.m_spawnPoints.Length != 0)
                {
                    gateIndex = new Randomizer(serviceID).Int32((uint)stationAI.m_spawnPoints.Length);
                }

                // Create connection line to/from new outside connection.
                instance.m_buildings.m_buffer[buildingID].m_flags |= Building.Flags.IncomingOutgoing;
                TransportStationAIPatch.CreateConnectionLines(stationAI, serviceID, ref instance.m_buildings.m_buffer[serviceID], buildingID, ref instance.m_buildings.m_buffer[buildingID], gateIndex);

                // Release building's existing service vehicles.
                BuildingUtil.ReleaseOwnVehicles(serviceID);
            }
        }


        /// <summary>
        /// Event hook for BuildingManager.EventBuildingReleased.
        /// This removes transport lines from station AI buildings to the now-demolished connection.
        /// </summary>
        /// <param name="buildingID">ID of demolished building</param>
        public static void OnBuildingReleased(ushort buildingID)
        {
            // Local ref.
            BuildingManager instance = Singleton<BuildingManager>.instance;

            // Is this an outside connection?
            OutsideConnectionAI connectionAI = instance.m_buildings.m_buffer[buildingID].Info?.m_buildingAI as OutsideConnectionAI;
            if (connectionAI == null)
            {
                // No - don't do anything.
                return;
            }

            // Yes - this is an outside connection.
            Logging.Message("found building ", buildingID.ToString(), " with outside connection AI");

            // Find all service buildings in map and iterate through.
            IEnumerable<ushort> serviceBuildings = BuildingUtil.FindServiceBuildings(buildingID);
            foreach (ushort serviceID in serviceBuildings)
            {
                // Check if this building has transport line info.
                TransportStationAI stationAI = instance.m_buildings.m_buffer[serviceID].Info.GetAI() as TransportStationAI;
                if (stationAI?.m_transportLineInfo == null)
                {
                    // No transport line info - skip to next building.
                    continue;
                }

                // Release building's existing service vehicles.
                BuildingUtil.ReleaseOwnVehicles(serviceID);
            }

            // Release any other vehicles heading to/from this (now-demolished) connection.
            BuildingUtil.ReleaseTargetedVehicles(buildingID);
        }
    }
}