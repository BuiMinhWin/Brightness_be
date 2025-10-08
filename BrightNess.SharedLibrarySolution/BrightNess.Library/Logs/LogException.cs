
using System;
using System.IO;

namespace BrightNess.Library.Logs
{
    public static class LogException
    {
        public static void LogExceptionx(Exception ex)
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if(!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            string filePath = Path.Combine(logPath, $"log-{DateTime.Now:yyyy-MM-dd}.txt");
            File.AppendAllText(filePath, $"{DateTime.Now} | Exception: {ex.Message} | Stack Trace: {ex.StackTrace}" + Environment.NewLine);
        }
    }
}
