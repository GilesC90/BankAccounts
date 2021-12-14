using System;
using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Models
{
    public class Balance
    {
        [Key]
        public int BalanceId {get; set; }
        public int UserId {get; set; }
        [Required]
        [Display(Name = "Withdrawal/Deposit Amount")]
        public double Amount {get; set; }
        public DateTime CreatedAt {get; set; } = DateTime.Now;
        public DateTime UpdatedAt {get; set; } = DateTime.Now;
    }
}