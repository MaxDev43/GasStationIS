using System;
using System.IO;
using System.Text;

namespace GasStationIS.Core
{
    /// <summary>
    /// Простой файловый логгер.
    /// Все ошибки пишутся в Logs/app_{дата}.log рядом с exe.
    /// </summary>
    public static class Logger
    {
        private static readonly string LogDirectory =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        private static string LogFilePath =>
            Path.Combine(LogDirectory, $"azs_{DateTime.Now:yyyy-MM-dd}.log");

        private static readonly object _lock = new object();

        static Logger()
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                    Directory.CreateDirectory(LogDirectory);
            }
            catch { /* Если не удалось создать папку — работаем без логов */ }
        }

        public static void Info(string message) =>
            Write("INFO ", message);

        public static void Warning(string message) =>
            Write("WARN ", message);

        public static void Error(string message, Exception ex = null)
        {
            var sb = new StringBuilder(message);
            if (ex != null)
            {
                sb.AppendLine();
                sb.Append("  Exception: ").AppendLine(ex.Message);
                if (ex.InnerException != null)
                    sb.Append("  Inner:     ").AppendLine(ex.InnerException.Message);
                sb.Append("  StackTrace: ").AppendLine(ex.StackTrace);
            }
            Write("ERROR", sb.ToString());
        }

        private static void Write(string level, string message)
        {
            try
            {
                lock (_lock)
                {
                    var line = $"[{DateTime.Now:HH:mm:ss.fff}] [{level}] {message}{Environment.NewLine}";
                    File.AppendAllText(LogFilePath, line, Encoding.UTF8);
                }
            }
            catch { /* Глотаем ошибки логирования, чтобы не ломать приложение */ }
        }
    }
}
