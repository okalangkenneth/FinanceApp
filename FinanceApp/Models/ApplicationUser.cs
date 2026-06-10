using Microsoft.AspNetCore.Identity;

namespace FinanceApp.Models
{

    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PreferredCurrency { get; set; }
    }
}
