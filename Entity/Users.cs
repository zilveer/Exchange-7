using System;
using System.Collections.Generic;

namespace Entity
{
    public partial class Users
    {
        public Users()
        {
            Balance = new HashSet<Balance>();
            DepositWithdrawRequest = new HashSet<DepositWithdrawRequest>();
            Orders = new HashSet<Orders>();
            ReceivingAddress = new HashSet<ReceivingAddress>();
            SendReceiveTransaction = new HashSet<SendReceiveTransaction>();
            SendingAddress = new HashSet<SendingAddress>();
            Transaction = new HashSet<Transaction>();
        }

        public int Id { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsClosed { get; set; }
        public bool IsFreezed { get; set; }
        public bool IsDebitAllowed { get; set; }
        public string FreezedReason { get; set; }
        public string DebitBlockedReason { get; set; }
        public Guid UniqueId { get; set; }

        public virtual KycInfo KycInfo { get; set; }
        public virtual UserCredentials UserCredentials { get; set; }
        public virtual ICollection<Balance> Balance { get; set; }
        public virtual ICollection<DepositWithdrawRequest> DepositWithdrawRequest { get; set; }
        public virtual ICollection<Orders> Orders { get; set; }
        public virtual ICollection<ReceivingAddress> ReceivingAddress { get; set; }
        public virtual ICollection<SendReceiveTransaction> SendReceiveTransaction { get; set; }
        public virtual ICollection<SendingAddress> SendingAddress { get; set; }
        public virtual ICollection<Transaction> Transaction { get; set; }
    }
}
