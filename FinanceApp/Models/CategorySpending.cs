using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceApp.Models
{
    public class CategorySpending
    {
        public TransactionCategory Category { get; set; }
        public decimal Amount { get; set; }
    }
}
