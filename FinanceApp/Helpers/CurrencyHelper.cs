using System.Globalization;

namespace FinanceApp.Helpers
{
    public static class CurrencyHelper
    {
        public static string FormatAmount(decimal amount, string currency)
        {
            // Culture-based formatting gives the right symbol AND the right
            // digit grouping/placement per currency. The old code formatted
            // everything unknown — including SEK and USH — as GBP.
            string cultureName = currency switch
            {
                "USD" => "en-US",
                "GBP" => "en-GB",
                "EUR" => "fr-FR",
                "SEK" => "sv-SE",
                "USH" => "en-UG", // Ugandan shilling; enum name predates the ISO code UGX
                _ => null
            };

            if (cultureName == null)
            {
                // Unknown code: be honest rather than guessing a symbol.
                return string.Create(CultureInfo.InvariantCulture, $"{amount:N2} {currency}");
            }

            return string.Format(CultureInfo.GetCultureInfo(cultureName), "{0:C}", amount);
        }
    }
}
