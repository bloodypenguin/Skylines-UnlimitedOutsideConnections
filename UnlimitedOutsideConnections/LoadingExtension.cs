using System;
using ICities;

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
                BuildingManagerDetour.Deploy(); //TODO(earalov): do it for game mode only if Cross The Line or 81 tiles is enabled
                if (loadMode == LoadMode.NewGame || loadMode == LoadMode.LoadGame)
                {
                    OutsideConnectionAIDetour.Deploy();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }

        public override void OnLevelUnloading()
        {
            try
            {
                BuildingManagerDetour.Revert();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
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
                UnityEngine.Debug.LogException(e);
            }
        }
    }
}