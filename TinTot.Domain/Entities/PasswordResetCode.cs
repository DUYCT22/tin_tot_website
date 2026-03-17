using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinTot.Domain.Entities
{
    public class PasswordResetCode
    {
        public required string Email { get; init; }
        public required string Code { get; init; }
        public DateTime ExpiresAtUtc { get; init; }
        public bool IsVerified { get; set; }
    }
}
