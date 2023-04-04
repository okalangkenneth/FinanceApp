using FinanceApp.Models;
using System;
using System.Linq;

namespace FinanceApp.Data
{
    public static class SeedData
    {
        public static void Seed(ApplicationDbContext context)
        {
            // Check if the database has any data, if not, seed the data
            if (!context.Transactions.Any())
            {
                context.Transactions.AddRange(
                    new Transaction
                    {
                        UserId = "06c1cda4-3143-448e-bfd7-27b0a2568186",
                        Amount = 200,
                        Category = TransactionCategory.Food,
                        Type = TransactionType.Expense,
                        Date = DateTime.Now.AddDays(-10),
                        Description = "Grocery shopping at the supermarket"
                    },
                    new Transaction
                    {
                        UserId = "06c1cda4-3143-448e-bfd7-27b0a2568186",
                        Amount = 1000,
                        Category = TransactionCategory.Other,
                        Type = TransactionType.Income,
                        Date = DateTime.Now.AddDays(-15),
                        Description = "Monthly salary payment"
                    }

                // Add more sample transactions here
                );

                context.SaveChanges();
            }

            if (!context.FinancialGoals.Any())
            {
                context.FinancialGoals.AddRange(
                    new FinancialGoal
                    {
                        UserId = "06c1cda4-3143-448e-bfd7-27b0a2568186",
                        Title = "Buy a new car",
                        Description = "Save for a down payment on a new car",
                        TargetAmount = 5000,
                        
                        EndDate = DateTime.Now.AddYears(1)
                    },
                    new FinancialGoal
                    {
                        UserId = "06c1cda4-3143-448e-bfd7-27b0a2568186",
                        Title = "Emergency fund",
                        Description = "Save for unexpected expenses",
                        TargetAmount = 10000,
                        
                        EndDate = DateTime.Now.AddYears(2)
                    }
                // Add more sample financial goals here
                );

                context.SaveChanges();
            }
        }

    }
}



