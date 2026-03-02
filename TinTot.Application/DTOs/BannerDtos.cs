using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinTot.Application.DTOs
{
    public class BannerUpsertDto
    {
        public string Link { get; set; } = string.Empty;
        public bool Status { get; set; }
        public int Orders { get; set; }
        public int? ActorUserId { get; set; }
    }
    public class BannerDto
    {
        public int Id { get; set; }
        public string? Link { get; set; }
        public string? Image { get; set; }
        public bool Status { get; set; }
        public int Orders { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
