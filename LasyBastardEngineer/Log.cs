using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LasyBastardEngineer
{
    internal static class Log
    {
        public static bool enableDebugging;
        internal static ManualLogSource _logSource;

        public static void DebugBreakpoint(string methodName, int breakpointNumber = -1)
        {
            string s = $"{LazyBastardPlugin.modName}: {methodName} IL hook failed!";
            if (breakpointNumber >= 0)
                s += $" (breakpoint {breakpointNumber})";
            Log.Error(s);
        }

        internal static void Init(ManualLogSource logSource)
        {
            enableDebugging = false;
            _logSource = logSource;
        }
        internal static string Combine(params string[] parameters)
        {
            string s = $"{LazyBastardPlugin.modName} : ";
            foreach (string s2 in parameters)
            {
                s += $"{s2} : ";
            }
            return s;
        }
        internal static void Debug(object data)
        {
            if (enableDebugging)
                _logSource.LogDebug(data);
        }
        internal static void Error(object data) => _logSource.LogError(data);
        internal static void ErrorAssetBundle(string assetName, string bundleName) =>
            Log.Error($"failed to load asset, {assetName}, because it does not exist in asset bundle, {bundleName}");
        internal static void Fatal(object data) => _logSource.LogFatal(data);
        internal static void Info(object data) => _logSource.LogInfo(data);
        internal static void Message(object data) => _logSource.LogMessage(data);
        internal static void Warning(object data) => _logSource.LogWarning(data);
    }
}
