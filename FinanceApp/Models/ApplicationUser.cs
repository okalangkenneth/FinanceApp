using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Models
{
    
    public class ApplicationUser : IdentityUser
    {

        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PreferredCurrency { get; set; }
    }
}
