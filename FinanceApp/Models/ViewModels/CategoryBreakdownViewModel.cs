using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceApp.Models.ViewModels
{
    public class CategoryBreakdownViewModel
    {
        public List<CategoryTotal> CategoryTotals { get; set; }
    }

    public class CategoryTotal
    {
        public TransactionCategory Category { get; set; }
        public decimal Total { get; set; }
    }

}
