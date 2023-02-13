using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
public class Group
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    string groupName { get; set; }
    List<List<DaysSchedule>> schedule { get; set; }
    public Group()
    {
        groupName = "UNKNOWN";
        schedule = new List<List<DaysSchedule>>();
    }
    public Group(string groupName)
    {
        this.groupName = groupName;
        schedule = new List<List<DaysSchedule>>();
    }

}

public class DaysSchedule 
{
    public List<string> dates = new List<string>();
    public List<Classes> classes = new List<Classes>();
}
public class Classes
{
    int ordinal { get; set; }
    string name { get; set; }
    string? teacher { get; set; }
    string type { get; set; }
    string location { get; set; }
    public Classes()
    {
        ordinal = 0;
        name = "UNKNOWN";
        type = "UNDEFINED";
        location = "UNDEFINED";
    }
    public Classes(int ordinal, string name, string teacher, string type, string location)
    {
        this.ordinal = ordinal;
        this.name = name;
        this.teacher = teacher;
        this.type = type;
        this.location = location;
    }
}
