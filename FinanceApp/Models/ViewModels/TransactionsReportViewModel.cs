using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceApp.Models.ViewModels
{
    public class TransactionsReportViewModel
    {
        public string PreferredCurrency { get; set; }
        public List<Transaction> Transactions { get; set; }

    }
}
