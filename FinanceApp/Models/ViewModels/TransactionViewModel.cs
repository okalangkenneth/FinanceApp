using System;

namespace FinanceApp.Models.ViewModels
{
    public class TransactionViewModel
    {
        public string Category { get; set; }
        public string SubType { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } // Add this line
        public string Currency { get; set; } // Add this line
    }

}
