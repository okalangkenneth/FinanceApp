using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceApp.Models.ViewModels
{
    public class MonthlyBudgetViewModel
    {
        public List<MonthlyTotal> MonthlyTotals { get; set; }
    }

    public class MonthlyTotal
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
    }
}
