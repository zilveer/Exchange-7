using System;

namespace Auth.Models
{
    public class AuthTempState
    {
        public int UserId { get; set; }
        public DateTime LoginTime { get; set; }
    }
}
