using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace cookie.Logging
{
    public static class LoggerExtensions
    {
        private static readonly IReadOnlyDictionary<LogType, string> LogTypeColors = new Dictionary<LogType, string>
        {
            { LogType.Log, "#FFFFFF" },
            { LogType.Warning, "#FFDD55" },
            { LogType.Error, "#FF4444" },
            { LogType.Assert, "#FF8C00" },
            { LogType.Exception, "#FF0066" },
        };

        private static readonly IReadOnlyDictionary<LogType, Action<string, UnityEngine.Object>> LogMethods =
            new Dictionary<LogType, Action<string, UnityEngine.Object>>
            {
                { LogType.Log, Debug.Log },
                { LogType.Warning, Debug.LogWarning },
                { LogType.Error, Debug.LogError },
                { LogType.Assert, (message, context) => Debug.LogAssertion(message,  context) },
        };

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("DISABLE_LOGS")]
        public static void Log(
            this ILogEnabled logEnabled,
            string message,
            LogType logType = LogType.Log,
            UnityEngine.Object context = null,
            bool isEssential = false)
        {
            if (!CanLog(logEnabled, isEssential)) return;

            LogMethods[logType](FormatMessage(logEnabled, message, logType), context);
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("DISABLE_LOGS")]
        public static void Log(
            this ILogEnabled logEnabled,
            Exception exception,
            UnityEngine.Object context = null,
            bool isEssential = false)
        {
            if (!CanLog(logEnabled, isEssential)) return;

            var formatted = FormatMessage(logEnabled, exception.Message, LogType.Exception);
            Debug.LogException(new Exception(formatted, exception), context);
        }

        private static bool CanLog(ILogEnabled logEnabled, bool isEssential)
        {
            if (logEnabled.Mode == LogMode.Off) return false;
            if (logEnabled.Mode == LogMode.Essential && !isEssential) return false;
            return true;
        }

        private static string FormatMessage(ILogEnabled logEnabled, string message, LogType logType)
        {
            var prefixColor  = ColorUtility.ToHtmlStringRGB(logEnabled.Color);
            var messageColor = LogTypeColors[logType];
            return $"[<color=#{prefixColor}>{logEnabled.GetType().Name}</color>] <color={messageColor}>{message}</color>";
        }
    }
}