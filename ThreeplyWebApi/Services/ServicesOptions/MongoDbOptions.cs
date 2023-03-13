namespace ThreeplyWebApi.Services.ServicesOptions
{
    public class MongoDbOptions
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public int DatabaseRespondTimeoutMS { get; set; } = 0;
    }
}
