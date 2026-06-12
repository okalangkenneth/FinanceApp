using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Configuration
{
    public class AnthropicOptions
    {
        public const string SectionName = "Anthropic";

        /// <summary>
        /// Optional at boot: the spending-analysis service throws a clear
        /// error when the feature is used without a key, so the rest of the
        /// app stays usable without one.
        /// </summary>
        public string ApiKey { get; set; }

        [Required]
        public string Model { get; set; } = "claude-haiku-4-5-20251001";
    }
}
