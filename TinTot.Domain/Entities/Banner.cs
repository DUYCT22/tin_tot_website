namespace TinTot.Domain.Entities
{
    public class Banner
    {
        public int Id { get; set; }

        public string? Link { get; set; }
        public string? Image { get; set; }

        public bool Status { get; set; }
        public int Orders { get; set; }

        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        // Navigation
        public User? CreatedByUser { get; set; }
        public User? UpdatedByUser { get; set; }
    }
}
