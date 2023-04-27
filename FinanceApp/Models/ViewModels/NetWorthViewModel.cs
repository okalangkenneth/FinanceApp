using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Models
{
    public class NetWorthViewModel
    {
        [Required]
        [Display(Name = "Assets")]
        public decimal TotalAssets { get; set; }

        [Required]
        [Display(Name = "Liabilities")]
        public decimal TotalLiabilities { get; set; }
    }
}
