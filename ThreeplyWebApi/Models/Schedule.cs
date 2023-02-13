using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ThreeplyWebApi.Models
{
public class Schedule
{
    public Schedule()
    {
        week = new List<List<Day>>();
        for (int i = 0; i < 7; i++)
        {
            week.Add(new List<Day>());
        }
    }
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string groupName { get; set; }
    public List<List<Day>> week { get; set; }

    public List<Lesson> GetLessons(DateOnly date)
    {
        foreach (var day in week[(int)date.DayOfWeek-1])
        {
            if (day.dates.Exists(x => x.Equals(date))) return day.lessons;
        }
        throw new Exception();
    }
}

public struct Day
{
    public List<DateOnly> dates = new List<DateOnly>();
    public List<Lesson> lessons = new List<Lesson>();
        public Day(List<DateOnly> dates, List<Lesson> lessons)
        {
            for (int i = 0; i < dates.Count; i++)
            {
                this.dates.Add(dates[i]);
            }
            for (int i = 0; i < lessons.Count; i++)
            {
                this.lessons.Add(lessons[i]);
            }
        }
    }

public struct Lesson
{
    public String name { get; set; }
    public int number { get; set; }
    public String audienceNumber { get; set; }

}
}
