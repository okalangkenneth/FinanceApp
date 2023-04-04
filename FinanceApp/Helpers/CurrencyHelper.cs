using System.Globalization;

namespace FinanceApp.Helpers
{
    public static class CurrencyHelper
    {
        public static string FormatAmount(decimal amount, string currency)
        {
            CultureInfo cultureInfo;
            switch (currency)
            {
                case "USD":
                    cultureInfo = new CultureInfo("en-US");
                    break;
                case "EUR":
                    cultureInfo = new CultureInfo("fr-FR");
                    break;
                default: // Default to GBP
                    cultureInfo = new CultureInfo("en-GB");
                    break;
            }

            return string.Format(cultureInfo, "{0:C}", amount);
        }
    }

}
