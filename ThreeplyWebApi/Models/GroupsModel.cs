using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;
namespace ThreeplyWebApi.Models.GroupModel
{
    [Serializable]
    public class Group
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        [JsonIgnore] public string? Id { get; set; }
        public string GroupName { get; set; }
        public Schedule Schedule { get; set; }
        public DateTime LastTimeUpdate { get; set; }
        public Group()
        {
            GroupName = "UNKNOWN";
            Schedule = new Schedule();
        }
        public Group(string groupName)
        {
            this.GroupName = groupName;
            Schedule = new Schedule();
        }

    }

    [Serializable]
    public class Schedule
    {
        public Schedule()
        {
            for (int i = 1; i <= 7; i++)
            {
                Week.Add(new Weekday(i));
            }
        }
        public List<Weekday> Week { get; set; } = new List<Weekday>();
    }
    [Serializable]
    public class Weekday
    {
        public List<DaysSchedule> DaysSchedules { get; set; } = new List<DaysSchedule>();
        public int DayNumber { get; set; } = 0;
        public Weekday()
        {

        }
        public Weekday(int dayNumber)
        {
            DayNumber = dayNumber;
        }
        public void InsertDaysChedule(DaysSchedule currentDaysSchedule)
        {
            foreach (var el in DaysSchedules)
            {
                if (el.HashSum == currentDaysSchedule.HashSum)
                {
                    el.Dates.AddRange(currentDaysSchedule.Dates);
                    return;
                }
            }
            DaysSchedules.Add(currentDaysSchedule);
        }
    }

    public class DaysSchedule
    {
        public List<string> Dates { get; set; } = new List<string>();
        public List<Classes> Classes { get; set; } = new List<Classes>();
        public string HashSum { get; set; } = "";

    }
    public class Classes
    {
        [BsonElement("ordinal")]
        public int Ordinal { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("teacher")]
        public string? Teacher { get; set; }
        [BsonElement("type")]
        public string Type { get; set; }
        [BsonElement("location")]
        public string Location { get; set; }
        public Classes()
        {
            Ordinal = 0;
            Name = "UNKNOWN";
            Type = "UNDEFINED";
            Location = "UNDEFINED";
        }
        public Classes(int ordinal, string name, string teacher, string type, string location)
        {
            this.Ordinal = ordinal;
            this.Name = name;
            this.Teacher = teacher;
            this.Type = type;
            this.Location = location;
        }
    }

}
