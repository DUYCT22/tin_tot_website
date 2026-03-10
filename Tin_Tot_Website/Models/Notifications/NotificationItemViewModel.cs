namespace Tin_Tot_Website.Models.Notifications
{
    public class NotificationItemViewModel
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
