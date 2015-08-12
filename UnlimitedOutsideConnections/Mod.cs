using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using ColossalFramework;
using ColossalFramework.Math;
using ICities;

namespace UnlimitedOutsideConnections
{
    public class Mod : LoadingExtensionBase, IUserMod
    {

        private static RedirectCallsState _state;
        private static IntPtr _originalPtr;
        private static IntPtr _detourPtr;
        private static object _lock = new object();

        public string Name
        {
            get
            {
                _originalPtr = typeof(BuildingManager).GetMethod("CalculateOutsideConnectionCount",
                    BindingFlags.Instance | BindingFlags.Public).MethodHandle.GetFunctionPointer();
                _detourPtr = typeof(Mod).GetMethod("CalculateOutsideConnectionCount",
                    BindingFlags.Instance | BindingFlags.Public).MethodHandle.GetFunctionPointer();
                return "UnlimitedOutsideConnections";
            }

        }

        public string Description
        {
            get { return "Place as many roads with outside connections as you want"; }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            //TODO(earalov): restore as option in the future?
            if (/*(mode == LoadMode.LoadMap || mode == LoadMode.NewMap) && */_state == null)
            {
                _state = RedirectionHelper.PatchJumpTo(_originalPtr, _detourPtr);
            }
        }

        public override void OnLevelUnloading()
        {
            if (_state != null)
            {
                RedirectionHelper.RevertJumpTo(_originalPtr, _state);
                _state = null;
            }
        }


        public void CalculateOutsideConnectionCount(ItemClass.Service service, ItemClass.SubService subService,
            out int incoming, out int outgoing)
        {
            do
                ;
            while (!Monitor.TryEnter(_lock, SimulationManager.SYNCHRONIZE_TIMEOUT));
            try
            {
                RedirectionHelper.RevertJumpTo(_originalPtr, _state);
                Singleton<BuildingManager>.instance.CalculateOutsideConnectionCount(service, subService, out incoming,
                    out outgoing);
                if (incoming > 3)
                {
                    incoming = 3;
                }
                if (outgoing > 3)
                {
                    outgoing = 3;
                }
                RedirectionHelper.PatchJumpTo(_originalPtr, _detourPtr);
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }
    }
}
