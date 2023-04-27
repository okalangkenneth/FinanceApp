using System.Collections.Generic;

namespace FinanceApp.Models.ViewModels
{
    public class DashboardViewModel
    {
        public List<Transaction> Transactions { get; set; }
        public List<FinancialGoal> FinancialGoals { get; set; }
        public decimal AccountBalance { get; set; }
        public List<CategorySpending> CategorySpending { get; set; } = new List<CategorySpending>();
        public List<IncomeVsExpense> IncomeVsExpenses { get; set; }
        public List<MonthlyBudget> MonthlyBudgets { get; set; }
        public NetWorth NetWorth { get; set; }
        public List<string> IncomeVsExpenseLabels { get; set; }
        public Dictionary<string, List<decimal>> IncomeVsExpenseAmounts { get; set; }
        public List<string> MonthlyBudgetLabels { get; set; }
        public List<decimal> MonthlyBudgetAmounts { get; set; }
        public List<string> MonthlyBudgetColors { get; set; }
        public List<string> MonthlyBudgetBorderColors { get; set; }
        public List<decimal> NetWorthAmounts { get; set; }
        public Dictionary<string, List<decimal>> MonthlyBudgetData { get; set; }
        public MonthlyBudgetViewModel MonthlyBudgetOverview { get; set; }
        public List<NetWorth> NetWorthData { get; set; }
        public decimal CalculatedNetWorth { get; set; }
        public List<CategoryTotal> CategoryTotals { get; set; }

    }
}



