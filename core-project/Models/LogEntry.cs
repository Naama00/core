using System;

namespace Library.Models
{
    /// <summary>
    /// מייצג ערך יומן (log entry) של בקשה HTTPnew namespace Library.Models
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// תאריך וזמן התחלת הבקשה
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// שם ה-Controller
        /// </summary>
        public string? ControllerName { get; set; }

        /// <summary>
        /// שם ה-Action
        /// </summary>
        public string? ActionName { get; set; }

        /// <summary>
        /// שם המשתמש המחובר (אם קיים)
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// משך זמן הביצוע במילישניות
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// HTTP Method (GET, POST, PUT, DELETE, וכו')
        /// </summary>
        public string? HttpMethod { get; set; }

        /// <summary>
        /// HTTP Status Code של התגובה
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// קו עלייה בתבנית טקסטית כדי לשמור בקובץ
        /// </summary>
        public override string ToString()
        {
            return $"[{StartTime:yyyy-MM-dd HH:mm:ss.fff}] | {HttpMethod} | {ControllerName}/{ActionName} | " +
                   $"User: {(string.IsNullOrEmpty(Username) ? "Anonymous" : Username)} | " +
                   $"Status: {StatusCode} | Duration: {DurationMs}ms";
        }
    }
}
