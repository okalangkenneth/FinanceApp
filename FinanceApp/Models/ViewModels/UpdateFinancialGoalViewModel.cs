using System;
using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Models.ViewModels
{
    public class UpdateFinancialGoalViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public decimal TargetAmount { get; set; }

        [Required]
        public decimal CurrentAmount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public GoalStatus Status { get; set; }

        [Required]
        public Currency Currency { get; set; }

    }
}


