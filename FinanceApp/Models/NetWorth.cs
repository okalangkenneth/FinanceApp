using System;

namespace FinanceApp.Models
{
    public class NetWorth
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal Amount => TotalAssets - TotalLiabilities;
        public DateTime Date { get; set; }
    }
}

