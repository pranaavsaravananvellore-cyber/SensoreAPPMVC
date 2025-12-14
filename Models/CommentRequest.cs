namespace SensoreAPPMVC.Models
{
    public class CommentRequest
    {
        public int PressureMapId { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}