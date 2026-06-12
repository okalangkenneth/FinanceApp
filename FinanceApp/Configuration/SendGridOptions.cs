namespace FinanceApp.Configuration
{
    public class SendGridOptions
    {
        public const string SectionName = "SendGrid";

        /// <summary>
        /// Optional: when empty the app runs without outgoing email and
        /// SendGridEmailService logs a warning per skipped send (Phase 2
        /// decision). Validation only requires the sender when a key is set.
        /// </summary>
        public string ApiKey { get; set; }

        public string SenderEmail { get; set; }

        public string SenderName { get; set; } = "FinTrak";
    }
}
