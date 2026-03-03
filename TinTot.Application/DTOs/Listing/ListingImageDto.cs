using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinTot.Application.DTOs.Listing
{
    public class ListingImageDto
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public string? ImageUrl { get; set; }
    }
}
