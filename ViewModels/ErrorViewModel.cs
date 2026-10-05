namespace GamificationPlatform.ViewModels
{
    public class ErrorViewModel
    {
        public int StatusCode { get; set; }
        public string Title { get; set; } = "Something went wrong";
        public string Message { get; set; } = "Please try again later.";
        public string RequestId { get; set; } = string.Empty;
    }
}
