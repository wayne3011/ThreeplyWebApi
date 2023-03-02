using Microsoft.AspNetCore.Mvc;
using ThreeplyWebApi.Services.ScheduleParser.ScheduleParserExceptions;

namespace ThreeplyWebApi.Controllers.Extensions
{
    public static class ExceptionExtension
    {
        public static ProblemDetails toProblemDetails(this ScheduleParserException ex)
        {
            ProblemDetails details = new ProblemDetails();
            details.Title = ex.Message;
            details.Extensions.Add("groupName", ex.GroupName);
            details.Status = _getStatusCode(ex);
            return details;
        }
        private static int _getStatusCode(ScheduleParserException ex)
        {
            switch (ex)
            {
                case InvalidGroupNameException:
                    return 404;
                case BrokenWebSiteConnectionException:
                    return 502;
                case AbsenceScheduleObjectsException:
                    return 500;
                default:
                    return 200;
            }
        }
    }
}
