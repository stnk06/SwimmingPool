using System;

namespace SwimmingPool.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string AuthToken { get; set; }
        public DateTime? TokenExpiry { get; set; }

        public bool IsAdmin => Role == "Admin";
    }
}