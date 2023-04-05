using System;
using System.ComponentModel.DataAnnotations.Schema;


namespace FinanceApp.Models
{

    public class Transaction
    {
        public int Id { get; set; }

        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public decimal Amount { get; set; }

        public TransactionType Type { get; set; }

        public TransactionCategory Category { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

        public string PreferredCurrency { get; set; }

        public Currency Currency { get; set; }
    }

    public enum TransactionType
    {
        Income,
        Expense
    }
    public enum Currency
    {
        USD, // US Dollar
        GBP, // British Pound
        EUR, // Euro
        SEK, // Swedish Kronor
        USH, // Uganda Shilling
        // Add other currencies as needed
    }

}
