using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinTot.Domain.Entities
{
    public class ContactMessage
    {
        public string SenderEmail { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Issue { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}