namespace ThreeplyWebApi.Models
{
    public class GroupValidation
    {
        public string requestedGroup { get; set; } = "";
        public string formattedGroup { get; set; } = "";
        public bool isValid { get; set; } = false;
    }
}
