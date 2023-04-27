using FinanceApp.Enums;

namespace FinanceApp.Models
{
    public class MonthlyBudget
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public TransactionSubType Category { get; set; }
        public decimal Amount { get; set; }
    }

}

