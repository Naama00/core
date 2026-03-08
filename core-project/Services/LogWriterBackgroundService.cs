using System.Threading.Channels;
using Library.Models;

namespace Library.Services
{
    /// <summary>
    /// Background Service שקורא מה-log queue וכותב לקובץ asynchronously
    /// </summary>
    public class LogWriterBackgroundService : BackgroundService
    {
        private readonly Channel<LogEntry> _logChannel;
        private readonly string _logFilePath;
        private readonly ILogger<LogWriterBackgroundService> _logger;

        public LogWriterBackgroundService(Channel<LogEntry> logChannel, ILogger<LogWriterBackgroundService> logger)
        {
            _logChannel = logChannel;
            _logger = logger;

            // הגדר את הנתיב של קובץ הלוג
            var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }

            _logFilePath = Path.Combine(logsDirectory, $"requests_{DateTime.Now:yyyy-MM-dd}.log");
        }

        /// <summary>
        /// ביצוע השירות ברקע
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🔄 Log Writer Background Service started");

            try
            {
                await foreach (var logEntry in _logChannel.Reader.ReadAllAsync(stoppingToken))
                {
                    await WriteLogAsync(logEntry);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("🛑 Log Writer Background Service cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in Log Writer Background Service: {ex.Message}");
            }
        }

        /// <summary>
        /// כתוב LogEntry לקובץ
        /// </summary>
        private async Task WriteLogAsync(LogEntry logEntry)
        {
            try
            {
                var logLine = logEntry.ToString();
                
                // כתוב לקובץ בצורה thread-safe עם lock
                lock (this)
                {
                    using (var writer = new StreamWriter(_logFilePath, true))
                    {
                        writer.WriteLine(logLine);
                    }
                }

                Console.WriteLine($"✅ Logged: {logLine}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error writing log entry: {ex.Message}");
            }

            // הוספת Delay קטן כדי לא להרעיש את הדיסק יותר מדי
            await Task.Delay(1);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("⏹️ Log Writer Background Service stopping");
            _logChannel.Writer.Complete();
            await base.StopAsync(cancellationToken);
        }
    }
}
