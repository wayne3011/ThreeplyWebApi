using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

public class Group
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string groupName { get; set; }
    public Schedule schedule { get; set; }
    public Group()
    {
        groupName = "UNKNOWN";
        schedule = new Schedule();
    }
    public Group(string groupName)
    {
        this.groupName = groupName;
        schedule = new Schedule();
    }

}

public class Schedule
{
    public List<Weekday> week = new List<Weekday>();
}

public class Weekday
{
    public List<DaysSchedule> daysSchedules { get; set; } = new List<DaysSchedule>();
    public int dayNumber { get; set; } = 0;
}

public class DaysSchedule
{
    public List<string> dates = new List<string>();
    public List<Classes> classes = new List<Classes>();
}
public class Classes
{
    [BsonElement("ordinal")]
    public int ordinal { get; set; }
    [BsonElement("name")]
    public string name { get; set; }
    [BsonElement("teacher")]
    public string? teacher { get; set; }
    [BsonElement("type")]
    public string type { get; set; }
    [BsonElement("location")]
    public string location { get; set; }
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
