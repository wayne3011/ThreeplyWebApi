namespace ThreeplyWebApi.Models
{
    public class LogRecord
    {
        public DateTime Date { get; set; }
        public string UserId { get; set; } = "UNKNOWN";
        public string Message { get; set; } = "";
        public LogLevel LogLevel { get; set; } = LogLevel.None;

    }
}
