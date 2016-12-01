using System.Collections.Generic;
using System.Linq;

namespace UnlimitedOutsideConnections
{
    public static class ServiceUtil
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
    }
}