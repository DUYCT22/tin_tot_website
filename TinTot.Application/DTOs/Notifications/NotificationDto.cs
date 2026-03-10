namespace TinTot.Application.DTOs.Notifications
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? RelatedUserId { get; set; }
        public int? ListingId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}