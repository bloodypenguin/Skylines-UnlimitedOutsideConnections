using System;
using System.Reflection;
using System.Threading;
using ColossalFramework;
using UnlimitedOutsideConnections.Redirection;

namespace UnlimitedOutsideConnections.Detours
{
    public class BuildingManagerDetour : BuildingManager
    {
        private static RedirectCallsState _state;
        private static bool _deployed;
        private static IntPtr _originalPtr = IntPtr.Zero;
        private static IntPtr _detourPtr = IntPtr.Zero;
        private static readonly object Lock = new object();


        public static void Deploy()
        {
            if (_deployed) return;
            if (_originalPtr == IntPtr.Zero)
            {
                _originalPtr = typeof(BuildingManager).GetMethod("CalculateOutsideConnectionCount", BindingFlags.Instance | BindingFlags.Public).MethodHandle.GetFunctionPointer();
            }
            if (_detourPtr == IntPtr.Zero)
            {
                _detourPtr = typeof(BuildingManagerDetour).GetMethod("CalculateOutsideConnectionCount", BindingFlags.Instance | BindingFlags.Public).MethodHandle.GetFunctionPointer();
            }
            _state = RedirectionHelper.PatchJumpTo(_originalPtr, _detourPtr);
            _deployed = true;
        }

        public static void Revert()
        {
            if (!_deployed) return;
            if (_originalPtr != IntPtr.Zero && _detourPtr != IntPtr.Zero)
            {
                RedirectionHelper.RevertJumpTo(_originalPtr, _state);
            }
            _deployed = false;
        }


        public new void CalculateOutsideConnectionCount(ItemClass.Service service, ItemClass.SubService subService, out int incoming, out int outgoing)
        {
            do
                ;
            while (!Monitor.TryEnter(Lock, SimulationManager.SYNCHRONIZE_TIMEOUT));
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
                Monitor.Exit(Lock);
            }
        }
    }
}