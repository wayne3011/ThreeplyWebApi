using AngleSharp;
using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;
using System.Security.Cryptography;

namespace ScheduleParser
{
    public class ScheduleParserService
    {
        public string ScheduleUrl { get; set; }
        private IBrowsingContext _context = BrowsingContext.New(new Configuration().WithDefaultLoader());
        public ScheduleParserService(string scheduleUrl)
        {
            this.ScheduleUrl = scheduleUrl;
        }
        async public Task<Schedule> GetGroupScheduleAsync(string groupName) => await _getGroupSchedule(groupName);
        async public Task<bool> CheckGroup(string groupName) => await CheckGroup(groupName);
        async private Task<Schedule> _getGroupSchedule(string groupName)
        {
            if (!await _checkGroup(groupName)) throw new Exception("Invalid groupName");
            Schedule schedule = new Schedule();
            for (int weekDay = 1; weekDay < await _getStudyWeekCount(groupName); weekDay++)
            {
                var document = await _context.OpenAsync(ScheduleUrl + $"?group={groupName}&week={weekDay}");
                var dayCards = document.QuerySelectorAll("body>main>div>div>div.col-lg-8.me-auto.mb-7.mb-lg-0>article>ul>li>div>div");
                foreach (var dayCard in dayCards)//parse study day
                {
                    DaysSchedule daysSchedule = new DaysSchedule();
                    DateTime date = Convert.ToDateTime(
                    dayCard.FirstElementChild.Text().Trim(new char[] { '\n', '\t' }).Remove(0, 4)
                    );
                    daysSchedule.Dates.Add(date.ToShortDateString());                   
                    var dayCardElements = dayCard.Children.Where(el => el.TagName == "DIV");//get schoolday schedule
                    foreach (var el in dayCardElements)//for each classes parse it
                    {
                        var classesCard = el.Children;
                        Classes classes = new Classes();
                        string classesNameRow = classesCard[0].Text().ClearNbsp();
                        classes.Type = classesNameRow.Substring(classesNameRow.Length - 2);
                        classes.Name = classesNameRow.Substring(0, classesNameRow.Length - 2);
                        var classesInfoRow = classesCard[1].Children;
                        classes.Ordinal = _getOrdinalFromClassesTime(classesInfoRow[0].Text());
                        if (classesInfoRow.Length == 3)
                        {
                            classes.Teacher = classesInfoRow[1].Text();
                            classes.Location = classesInfoRow[2].Text();
                        }
                        else
                        {
                            classes.Location = classesInfoRow[1].Text();
                        }
                        daysSchedule.Classes.Add(classes);
                    }
                    daysSchedule.HashSum = _computeDaysScheduleHashSum(daysSchedule.Classes);
                    schedule.Week[(int)date.DayOfWeek - 1].InsertDaysChedule(daysSchedule);
                }
            }
            
            return schedule;
        }
        async private Task<int> _getStudyWeekCount(string groupName)
        {
            var document = await _context.OpenAsync(ScheduleUrl + $"?group={groupName}");
            var studyWeeks = document.QuerySelectorAll("#collapseWeeks>div>div>ul>li");
            //if (weekCount == 0) throw new Exception("Unable to open Schedule Url for getStudyWeekCount");
            int weekCount = studyWeeks.Count();
            //if (document.GetElementsByClassName("step mb-5"))
            
            return weekCount;
        }
        async private Task<bool> _checkGroup(string groupName)
        {
            int facultyNumber;
            int courseNumber;
            if(!Regex.IsMatch(groupName, "\\w[0-9]{1,2}|И\\w-[0-9]{3}\\w{1,2}-[0-9]{1,2}")) return false;
            string facultyNumberString = Regex.Match(groupName, "[0-9]{1,2}").Value;
            if (facultyNumberString == "И") facultyNumber = 10;
            else if (!int.TryParse(facultyNumberString, out facultyNumber)) return false;
            if (facultyNumber < 1 || facultyNumber > 12) return false;
            string courseNumberString = Regex.Matches(groupName, "[0-9]").ElementAt(1).Value;
            return true;
        }
        private int _getOrdinalFromClassesTime(string time)
        {
            switch (time)
            {
                case "09:00 – 10:30":
                    return 1;
                case "10:45 – 12:15":
                    return 2;
                case "13:00 – 14:30":
                    return 3;
                case "14:45 – 16:15":
                    return 4;
                case "16:30 – 18:00":
                    return 5;
                case "18:15 – 19:45":
                    return 6;
                case "20:00 – 21:30":
                    return 7;
                default:
                    throw new Exception("Invalid class time");
            }
        }
        private string _computeDaysScheduleHashSum(List<Classes> classes)
        {
            using MD5 mD5 = MD5.Create();
            StringBuilder classesHashSB = new StringBuilder();
            foreach (var el in classes)
            {
                classesHashSB.Append(el.Ordinal);
                classesHashSB.Append(el.Name);
                classesHashSB.Append(el.Type);
                classesHashSB.Append(el.Teacher);
                classesHashSB.Append(el.Location);
            }
            return Convert.ToHexString(mD5.ComputeHash(Encoding.UTF8.GetBytes(classesHashSB.ToString())));
        }
    }
}

public static class StringExtensions
{
    public static string ClearNbsp(this string str) => Regex.Replace(str, "[\t\n]", String.Empty);
}
