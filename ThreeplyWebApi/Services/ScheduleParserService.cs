using AngleSharp;
using AngleSharp.Dom;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using ThreeplyWebApi.Services.ScheduleParser.ScheduleParserExceptions;
using MongoDB.Driver.Core.Misc;
using static System.Net.Mime.MediaTypeNames;
using ThreeplyWebApi.Models.GroupModel;
using MongoDB.Driver.Linq;
using System.Globalization;

namespace ThreeplyWebApi.Services.ScheduleParser
{
    public class ScheduleParserService
    {
        public string ScheduleUrl { get; set; }
        readonly private IBrowsingContext _context = BrowsingContext.New(new Configuration().WithDefaultLoader());
        public ScheduleParserService(string scheduleUrl)
        {
            this.ScheduleUrl = scheduleUrl;
        }
        async public Task<Schedule> GetGroupScheduleAsync(string groupName) => await _getGroupSchedule(groupName);
        async public Task<string> FormatGroupNameAsync(string groupName) => await _formatGroupName(groupName);
        async private Task<Schedule> _getGroupSchedule(string groupName)
        {
            //if (await _formatGroupName(groupName) == string.Empty) throw new InvalidGroupNameException(ScheduleUrl,groupName);
            Schedule schedule = new Schedule();
            int studyWeekCount = await _getStudyWeekCount(groupName);
            for (int weekDay = 1; weekDay <= studyWeekCount; weekDay++)
            {
                var document = await _context.OpenAsync(ScheduleUrl + $"/index.php?group={groupName}&week={weekDay}");
                if (document == null) throw new BrokenWebSiteConnectionException(ScheduleUrl,groupName);
                var dayCards = document.QuerySelectorAll("body>main>div>div>div.col-lg-8.me-auto.mb-7.mb-lg-0>article>ul>li>div>div");
                //TODO: WARNING DISIGN CHANGED!!!
                if (dayCards.Length == 0)
                {
                    var isHaveScedule = document.QuerySelector("body>main>div>div>div.col-lg-8.me-auto.mb-7.mb-lg-0>article>div.w-md-75.w-xl-50.text-center.mx-md-auto.mb-5.mb-md-9>img");
                    if (isHaveScedule == null) throw new AbsenceScheduleObjectsException(ScheduleUrl,groupName);
                }
                
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
                        classes.Type = classesType[classesNameRow.Substring(classesNameRow.Length - 2)];
                        classes.Name = classesNameRow.Substring(0, classesNameRow.Length - 2);
                        var classesInfoRow = classesCard[1].Children;
                        classes.Ordinal = _getOrdinalFromClassesTime(classesInfoRow[0].Text());
                        if (classesInfoRow.Length == 3)
                        {
                            classes.Teacher = classesInfoRow[1].Text();
                            classes.Location = classesInfoRow[2].Text();
                        }
                        else if(classesInfoRow.Length == 4)
                        {
                            classes.Teacher = classesInfoRow[1].Text()+"/"+ classesInfoRow[2].Text();
                            classes.Location = classesInfoRow[3].Text();
                        }
                        else
                        {
                            classes.Location = classesInfoRow[1].Text();
                        }
                        daysSchedule.Classes.Add(classes);
                        Console.WriteLine(classes.Name);
                        Console.WriteLine(classes.Location);
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
            int weekCount = studyWeeks.Length;
            return weekCount;
        }
        
        async private Task<string> _formatGroupName(string groupName)
        {
            int facultyNumber;
            int courseNumber;
            string courseNumberString;
            string formattedGroupName = "";
            groupName = groupName.ToLower();
            groupName = Regex.Replace(groupName, "-", "");
            //if (!Regex.IsMatch(groupName, "\\w[0-9]{1,2}|И\\w-[0-9]{3}\\w{1,3}-[0-9]{2}")) return false;
            if (!Regex.IsMatch(groupName, "\\w[0-9]{1,2}|и\\w[0-9]{3}\\w{1,3}[0-9]{2}")) throw new InvalidGroupNameException(ScheduleUrl, groupName); ;
            string facultyNumberString = Regex.Match(groupName, "[0-9]{1,2}|и").Value;
            if (facultyNumberString == "и")
            {
                facultyNumber = 10;
                courseNumberString = Regex.Matches(groupName, "[0-9]").ElementAt(0).Value;
                formattedGroupName += groupName.Substring(0, 3).ToUpper();
            }
            else
            {
                if (!int.TryParse(facultyNumberString, out facultyNumber)) throw new InvalidGroupNameException(ScheduleUrl, groupName); ;
                formattedGroupName += Regex.Match(groupName, "\\w[0-9]{1,2}\\w").Value.ToUpper() + "-";  
                courseNumberString = Regex.Matches(groupName, "[0-9]").ElementAt(1).Value;
            }
            courseNumber = int.Parse(courseNumberString);
            if (facultyNumber < 1 || facultyNumber > 12) throw new InvalidGroupNameException(ScheduleUrl, groupName); ;
            if (courseNumber < 1 || courseNumber > 6) throw new InvalidGroupNameException(ScheduleUrl, groupName); ;
            formattedGroupName += Regex.Match(groupName, "[0-9]{3}");
            string learningProfile = string.Join("",Regex.Matches(groupName, "[а-я]"));
            learningProfile = learningProfile.Substring(2);
            learningProfile = Char.ToUpper(learningProfile[0]) + learningProfile.Substring(1);
            formattedGroupName += learningProfile + "-" + groupName.Substring(groupName.Length - 2);          
            var document = await _context.OpenAsync(ScheduleUrl + $"/groups.php?department=Институт+№{facultyNumber}&course={courseNumber}");
            var groupList = document.QuerySelector("body>main>div>div>div.col-lg-8.me-auto.mb-7.mb-lg-0>article>div.tab-content")?.Text().ClearNbsp();
            if (groupList == null) throw new Exception("Invalid scheduleURL");
            if (!Regex.IsMatch(groupList, formattedGroupName)) throw new InvalidGroupNameException(ScheduleUrl, groupName); ;
            return formattedGroupName;
        }
        static private int _getOrdinalFromClassesTime(string time)
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
        static private Dictionary<string, string> classesType = new Dictionary<string, string>()
        {
            //lecture practical laboratory test exam
            {"ЛК", "lecture"},
            {"ПЗ", "practical"},
            {"ЛР", "laboratory"},
            {"ЭКЗ", "exam"},
        };
    }
    public static class StringExtensions
    {
        public static string ClearNbsp(this string str) => Regex.Replace(str, "[\t\n]", String.Empty);
    }

    namespace ScheduleParserExceptions
    {
        [Serializable]
        public class ScheduleParserException : Exception
        {
            public string ScheduleUrl { get; } = "";
            public string GroupName { get; } = "";
            public ScheduleParserException() { }
            public ScheduleParserException(string message) : base(message) { }
            public ScheduleParserException(string message, Exception inner) : base(message, inner) { }
            protected ScheduleParserException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

            public ScheduleParserException(string message, string scheduleUrl) : base(message)
            {
                ScheduleUrl = scheduleUrl;
            }
            public ScheduleParserException(string message, string scheduleUrl, string groupName) : base(message)
            {
                ScheduleUrl = scheduleUrl;
                GroupName = groupName;
            }
        }

        [Serializable]
        public class InvalidGroupNameException : ScheduleParserException
        {
            private static string _defaultMessage = "Invalid groupName";
            public InvalidGroupNameException() { }
            public InvalidGroupNameException(string message) : base(message) { }
            public InvalidGroupNameException(string message, Exception inner) : base(message, inner) { }
            public InvalidGroupNameException(string scheduleUrl, string groupName) : base(_defaultMessage, scheduleUrl, groupName) { }
            public InvalidGroupNameException(string message, string scheduleUrl, string groupName) : base(message, scheduleUrl, groupName) { }
            protected InvalidGroupNameException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
        [Serializable]
        public class BrokenWebSiteConnectionException : ScheduleParserException
        {
            private static string _defaultMessage = "Durimg parsing, it was not to possible to get the next object of the Schedule.\n Connection with the site might be broken";
            public BrokenWebSiteConnectionException() { }
            public BrokenWebSiteConnectionException(string message) : base(message) { }
            public BrokenWebSiteConnectionException(string message, Exception inner) : base(message, inner) { }
            public BrokenWebSiteConnectionException(string scheduleUrl, string groupName) : base(_defaultMessage, scheduleUrl, groupName) { }
            public BrokenWebSiteConnectionException(string message, string scheduleUrl, string groupName) : base(message, scheduleUrl, groupName) { }
            protected BrokenWebSiteConnectionException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        [Serializable]
        public class AbsenceScheduleObjectsException : ScheduleParserException
        {
            private static string _defaultMessage = "Failed to get the objects with the schedule";
            public AbsenceScheduleObjectsException() { }
            public AbsenceScheduleObjectsException(string message) : base(message) { }
            public AbsenceScheduleObjectsException(string message, Exception inner) : base(message, inner) { }
            public AbsenceScheduleObjectsException(string scheduleUrl, string groupName) : base(_defaultMessage, scheduleUrl, groupName) { }
            public AbsenceScheduleObjectsException(string message, string scheduleUrl, string groupName) : base(message, scheduleUrl, groupName) { }
            protected AbsenceScheduleObjectsException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
    }
   



}

