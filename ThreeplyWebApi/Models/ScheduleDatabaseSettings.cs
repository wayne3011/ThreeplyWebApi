namespace ThreeplyWebApi.Models
{
    public class ScheduleDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string SchedulesCollectionName { get; set; } = null!;
    }
}
