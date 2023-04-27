using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    public class Budget
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ApplicationUser")]
        [Column(TypeName = "nvarchar(450)")]
        public string UserId { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }
}
