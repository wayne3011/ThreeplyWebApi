namespace ThreeplyWebApi.Models
{
    public class GroupsDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string GroupsCollectionName { get; set; } = null!;
    }
}
