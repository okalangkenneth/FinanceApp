using System.Globalization;
using FinanceApp.Helpers;
using Xunit;

namespace FinanceApp.Tests
{
    public class CurrencyHelperTests
    {
        // Audit item 44: SEK and USH used to fall through to the GBP default
        // and render with a £ symbol. These tests pin the culture mapping.

        [Fact]
        public void FormatAmount_Sek_UsesSwedishFormatting_NotPoundSign()
        {
            string result = CurrencyHelper.FormatAmount(1234.50m, "SEK");

            Assert.Equal(string.Format(CultureInfo.GetCultureInfo("sv-SE"), "{0:C}", 1234.50m), result);
            Assert.Contains("kr", result);
            Assert.DoesNotContain("£", result);
        }

        [Fact]
        public void FormatAmount_Ush_UsesUgandanFormatting_NotPoundSign()
        {
            string result = CurrencyHelper.FormatAmount(1234.50m, "USH");

            Assert.Equal(string.Format(CultureInfo.GetCultureInfo("en-UG"), "{0:C}", 1234.50m), result);
            Assert.DoesNotContain("£", result);
        }

        [Theory]
        [InlineData("USD", "en-US")]
        [InlineData("GBP", "en-GB")]
        [InlineData("EUR", "fr-FR")]
        public void FormatAmount_KnownCurrencies_MatchTheirCulture(string currency, string culture)
        {
            string result = CurrencyHelper.FormatAmount(99.99m, currency);

            Assert.Equal(string.Format(CultureInfo.GetCultureInfo(culture), "{0:C}", 99.99m), result);
        }

        [Fact]
        public void FormatAmount_UnknownCurrency_AppendsCodeInsteadOfGuessingSymbol()
        {
            string result = CurrencyHelper.FormatAmount(1234.5m, "XYZ");

            Assert.Equal("1,234.50 XYZ", result);
        }
    }
}
