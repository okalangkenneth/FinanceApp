using FinanceApp.Enums;

namespace FinanceApp.Models
{
    public class IncomeVsExpense
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public string Label { get; set; }
        public decimal Amount { get; set; }
        public TransactionCategory Category { get; set; } // Enum
        public TransactionSubType SubType { get; set; } // Enum


    } 
}

