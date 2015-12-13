using System;
using System.Linq;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ICities;
using UnityEngine;

namespace UnlimitedOutsideConnections
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private static LoadMode loadMode;

        public override void OnLevelLoaded(LoadMode mode)
        {
            loadMode = mode;
            try
            {
                if (mode == LoadMode.LoadMap || mode == LoadMode.NewMap)
                {
                    BuildingManagerDetour.Deploy();
                }
                else if (loadMode == LoadMode.NewGame || loadMode == LoadMode.LoadGame)
                {
                    if (IsBuildAnywherePluginActive())
                    {
                        BuildingManagerDetour.Deploy();
                        OutsideConnectionAIDetour.Deploy();
                    }
                }
                GameObject gameObjectWithTag = GameObject.FindGameObjectWithTag("MainCamera");
                if (!((gameObjectWithTag != null)))
                    return;
                var cameraController = gameObjectWithTag.GetComponent<CameraController>();
                cameraController.m_unlimitedCamera = true;

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static bool IsBuildAnywherePluginActive()
        {
            var plugins = PluginManager.instance.GetPluginsInfo();
            foreach (var name in from plugin in plugins.Where(p => p.isEnabled)
                                 select plugin.GetInstances<IUserMod>()
                                     into instances
                                     where instances.Any()
                                     select instances[0].Name into name
                                     where name == "CrossTheLine" || name == "81 Tile Unlock"
                                     select name)
            {
                Debug.Log(String.Format("UnlimitedOutsideConnections: {0} is active", name));
                return true;
            }
            return false;
        }

        public override void OnLevelUnloading()
        {
            try
            {
                BuildingManagerDetour.Revert();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            try
            {
                if (loadMode == LoadMode.NewGame || loadMode == LoadMode.LoadGame)
                {
                    OutsideConnectionAIDetour.Revert();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}