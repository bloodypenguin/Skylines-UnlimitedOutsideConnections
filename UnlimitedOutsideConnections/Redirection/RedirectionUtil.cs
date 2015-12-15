using System;
using System.Linq;
using System.Reflection;

namespace UnlimitedOutsideConnections.Redirection
{
    public static class RedirectionUtil
    {
        public static Tuple<MethodInfo, RedirectCallsState> RedirectMethod(Type targetType, MethodInfo detour)
        {
            var parameters = detour.GetParameters();
            Type[] types;
            if (parameters.Length > 0 && (
                (!targetType.IsValueType && parameters[0].ParameterType == targetType) ||
                (targetType.IsValueType && parameters[0].ParameterType == targetType.MakeByRefType()))){
                types = parameters.Skip(1).Select(p => p.ParameterType).ToArray();
            } else { 
                types = parameters.Select(p => p.ParameterType).ToArray();
            }
            var originalMethod = targetType.GetMethod(detour.Name,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static, null, types,
                null);
            var redirectCallsState = RedirectionHelper.RedirectCalls(originalMethod, detour);
            return Tuple.New(originalMethod, redirectCallsState);
        }
    }
}