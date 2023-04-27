using FinanceApp.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;


namespace FinanceApp.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }

        public int? GoalId { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public decimal Amount { get; set; }

        public TransactionCategory Category { get; set; }

        public TransactionSubType SubType { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

        public string PreferredCurrency { get; set; }

        public Currency Currency { get; set; }

        public FinancialGoal FinancialGoal { get; set; }
    }

    public enum Currency
    {
        USD, // US Dollar
        GBP, // British Pound
        EUR, // Euro
        SEK, // Swedish Kronor

        // Add other currencies as needed
    }
}
