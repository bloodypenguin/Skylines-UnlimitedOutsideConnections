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

            // Find all service buildings in map and iterate through.
            Building[] buildingBuffer = instance.m_buildings.m_buffer;
            IEnumerable<ushort> serviceBuildings = BuildingUtil.FindServiceBuildings(buildingID);
            foreach (ushort serviceID in serviceBuildings)
            {
                // Check if this building has transport line info.
                if (buildingBuffer[buildingID].Info.GetAI() is TransportStationAI stationAI)
                {
                    if (stationAI.m_transportLineInfo == null || (buildingBuffer[buildingID].m_flags & Building.Flags.Downgrading) != 0)
                    {
                        // No transport line info or building is downgrading - CreateConnectionLines will return without doing anything, so just skip to next building.
                        continue;
                    }

                    // Create connection line to/from new outside connection.
                    buildingBuffer[buildingID].m_flags |= Building.Flags.IncomingOutgoing;
                    TransportStationAIPatch.CreateConnectionLines(stationAI, buildingID, ref buildingBuffer[buildingID]);

                    // Release building's existing service vehicles.
                    BuildingUtil.ReleaseOwnVehicles(serviceID);
                }
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