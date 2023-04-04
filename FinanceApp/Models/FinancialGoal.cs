using Microsoft.AspNetCore.Identity;
using System;

namespace FinanceApp.Models
{
    public class FinancialGoal
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public GoalStatus Status { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public Currency Currency { get; set; }

        // Add this property
        public decimal Progress => CurrentAmount / TargetAmount * 100;
    }

    public enum GoalStatus
    {
        InProgress,
        Completed,
        Cancelled
    }
}
