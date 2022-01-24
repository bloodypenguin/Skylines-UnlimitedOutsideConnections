using System;
using System.Linq;
using CitiesHarmony.API;
using ICities;
using UnityEngine;

namespace UnlimitedOutsideConnections
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private static LoadMode _loadMode;


        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            _loadMode = mode;
            try
            {
                if (_loadMode == LoadMode.NewGame || _loadMode == LoadMode.LoadGame || _loadMode == LoadMode.NewGameFromScenario || _loadMode == LoadMode.NewMap || _loadMode == LoadMode.LoadMap)
                {
                    if (_loadMode != LoadMode.NewMap && _loadMode != LoadMode.LoadMap && !IsBuildAnywherePluginActive())
                    {
                        return;
                    }
                    Patcher.PatchAll();
                    BuildingManagerHooks.Deploy();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static bool IsBuildAnywherePluginActive()
        {
            var result = false;
            try
            {
                string[] buildAnywherePlugins = {"CrossTheLine", "81 Tile Unlock", "81 Tiles (Fixed for C:S 1.2+)"};

                foreach (var name in buildAnywherePlugins.Where(Util.IsModActive))
                {
                    Debug.Log($"UnlimitedOutsideConnections: {name} is active");
                    result = true;
                }
            }
            catch
            {
                result = true;
            }
            return result;
        }

        public override void OnLevelUnloading()
        {
            try
            {
                if (_loadMode == LoadMode.NewGame || _loadMode == LoadMode.LoadGame || _loadMode == LoadMode.NewGameFromScenario || _loadMode == LoadMode.NewMap || _loadMode == LoadMode.LoadMap)
                {
                    if (!HarmonyHelper.IsHarmonyInstalled)
                    {
                        return;
                    }
                    Patcher.UnpatchAll();
                    BuildingManagerHooks.Revert();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}