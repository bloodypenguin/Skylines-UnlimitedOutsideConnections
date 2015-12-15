using System;
using System.Linq;
using ICities;
using UnityEngine;
using UnlimitedOutsideConnections.Detours;

namespace UnlimitedOutsideConnections
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private static LoadMode _loadMode;

        public override void OnLevelLoaded(LoadMode mode)
        {
            _loadMode = mode;
            try
            {
                if (mode == LoadMode.LoadMap || mode == LoadMode.NewMap)
                {
                    BuildingManagerDetour.Deploy();
                }
                else if (_loadMode == LoadMode.NewGame || _loadMode == LoadMode.LoadGame)
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
            string[] buildAnywherePlugins = { "CrossTheLine", "81 Tile Unlock", "81 Tiles (Fixed for C:S 1.2+)" };
            var result = false;
            foreach (var name in buildAnywherePlugins.Where(Util.IsModActive))
            {
                Debug.Log($"UnlimitedOutsideConnections: {name} is active");
                result = true;
            }
            return result;
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
                if (_loadMode == LoadMode.NewGame || _loadMode == LoadMode.LoadGame)
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