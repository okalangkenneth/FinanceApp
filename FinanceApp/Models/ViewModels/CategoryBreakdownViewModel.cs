using FinanceApp.Enums;
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
        public TransactionSubType SubType { get; set; }
        public decimal Total { get; set; }
    }

}
