using System;
using BC.Mobile.Logging;
using Plugin.BLE.Abstractions;

namespace Plugin.BLE
{
    static class DefaultTrace
    {
        private static readonly ILogger _logger = LoggerFactory.CreateLogger("Plugin.BLE");

        public static void DefaultTraceInit()
        {
            Trace.TraceDebugImplementation = (parameter, args) =>
            {
                try
                {
                    _logger.Debug(() => string.Format(parameter, args));
                }
                catch (Exception ex)
                {
                    _logger.Debug(() => $"Failed to format string with arguments, ex: {ex.Message}");
                }
            };

            Trace.TraceInfoImplementation = (parameter, args) =>
            {
                try
                {
                    _logger.Info(string.Format(parameter, args));
                }
                catch (Exception ex)
                {
                    _logger.Info($"Failed to format string with arguments, ex: {ex.Message}");
                }
            };
        }
    }
}