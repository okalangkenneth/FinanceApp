using System.Collections.Generic;

namespace FinanceApp.Models.ViewModels
{
    public class DashboardViewModel
    {
        public List<Transaction> Transactions { get; set; }
        public List<FinancialGoal> FinancialGoals { get; set; }
        public decimal AccountBalance { get; set; }
        public List<SpendingAnalysis> SpendingAnalysis { get; set; }
        public List<CategorySpending> CategorySpending { get; set; } = new List<CategorySpending>();


    }

}
