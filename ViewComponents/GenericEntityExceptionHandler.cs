using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.CommonLibrary.Exceptions
{
    public class GenericExceptionHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GenericExceptionHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private const string _sessionKey = "Error";

        public static Dictionary<string, string> dictionary = new Dictionary<string, string>()
        {
            { "DELETE", "Cannot DELETE! This entry is referred in other tables." },
            { "Violation of PRIMARY KEY", "Cannot insert duplicate value" },
            { "duplicate key", "This record already exists." }
        };

        // ✅ Handle Exception
        public void HandleException(Exception ex, string context = "")
        {
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(context))
                sb.AppendLine(context);

            sb.AppendLine(ex.Message);

            if (ex.InnerException != null)
                sb.AppendLine(ex.InnerException.Message);

            string finalMessage = GetMessageForException(sb.ToString());

            // ✅ Store in Session (Core way)
            _httpContextAccessor.HttpContext.Session.SetString(_sessionKey, finalMessage);

            // ✅ Write to file
            ErrorLogWrite(finalMessage);
        }

        // ✅ Map DB errors
        public static string GetMessageForException(string message)
        {
            try
            {
                foreach (var pair in dictionary)
                {
                    if (message.Contains(pair.Key))
                        return pair.Value;
                }
                return message;
            }
            catch
            {
                return message;
            }
        }

        // ✅ Get Exception from Session
        public string ExceptionMessage()
        {
            return _httpContextAccessor.HttpContext.Session.GetString(_sessionKey) ?? "";
        }

        // ✅ Clear Session
        public void ClearExceptionMessage()
        {
            _httpContextAccessor.HttpContext.Session.Remove(_sessionKey);
        }

        // ✅ File Logging (FIXED)
        private static readonly object _lock = new object();

        public static void ErrorLogWrite(string logMessage)
        {
            try
            {
                string logDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");

                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                string filePath = Path.Combine(
                    logDir,
                    $"Error_Log_{DateTime.Now:ddMMyyyy}.txt"
                );

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("--------------------------------------------------");
                sb.AppendLine($"Time: {DateTime.Now}");
                sb.AppendLine(logMessage);
                sb.AppendLine("--------------------------------------------------");

                lock (_lock)
                {
                    File.AppendAllText(filePath, sb.ToString());
                }
            }
            catch
            {
                // Do not crash app
            }
        }

        // Constants (same as your code)
        public const int ForeignKeyConstraint = 547;
        public const int UniqueKeyConstraint = 2627;
        public const int UserGenericException = 50000;
    }
}