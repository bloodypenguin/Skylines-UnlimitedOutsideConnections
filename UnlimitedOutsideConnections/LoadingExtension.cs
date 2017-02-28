using System;
using System.Linq;
using ICities;
using UnityEngine;
using UnlimitedOutsideConnections.Detours;
using UnlimitedOutsideConnections.Redirection;

namespace UnlimitedOutsideConnections
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private static LoadMode _loadMode;

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            Redirector<TransportStationAIDetour>.Deploy();
        }

        public override void OnReleased()
        {
            base.OnReleased();
            Redirector<TransportStationAIDetour>.Revert();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            _loadMode = mode;
            try
            {
                if (mode == LoadMode.LoadMap || mode == LoadMode.NewMap)
                {
                    BuildingManagerDetour.Deploy();
                }
                else if (_loadMode == LoadMode.NewGame || _loadMode == LoadMode.LoadGame || _loadMode == LoadMode.NewGameFromScenario)
                {
                    if (!IsBuildAnywherePluginActive())
                    {
                        return;
                    }
                    BuildingManagerDetour.Deploy();
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
            base.OnLevelUnloading();
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