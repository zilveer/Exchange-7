using System;
using System.Collections.Generic;

namespace Entity
{
    public partial class UserCredentials
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Salt { get; set; }
        public string PasswordHash { get; set; }
        public DateTime? LoginUnblockOn { get; set; }
        public short LoginFailedAttempt { get; set; }
        public DateTime? LoginFailedStartOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string TwoFactorKey { get; set; }

        public virtual Users User { get; set; }
    }
}
