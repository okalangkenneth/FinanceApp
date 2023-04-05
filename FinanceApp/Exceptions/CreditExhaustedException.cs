using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceApp.Exceptions
{
    public class CreditExhaustedException : Exception
    {
        public CreditExhaustedException()
        {
        }

        public CreditExhaustedException(string message) : base(message)
        {
        }

        public CreditExhaustedException(string message, Exception inner) : base(message, inner)
        {
        }
    }

}
