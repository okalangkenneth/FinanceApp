//using FinanceApp.Models;
//using System;
//using System.Linq;

//namespace FinanceApp.Data
//{
//    public static class SeedData
//    {
//        public static void Seed(ApplicationDbContext context)
//        {
//            // Check if the database has any data, if not, seed the data
//            if (!context.Transactions.Any())
//            {
//                context.Transactions.AddRange(
//                    new Transaction
//                    {
//                        UserId = "06c1cda4-3143-448e-bfd7-27b0a2568186",
//                        Amount = 200,
//                        Category = TransactionCategory.Food,
//                        Type = TransactionType.Expense,
//                        Date = DateTime.Now.AddDays(-10),
//                        Description = "Grocery shopping at the supermarket"
//                    },
//                    new Transaction
//                    {
//                        UserId = "06c1cda4-3143-448e-bfd7-27b0a2568186",
//                        Amount = 1000,
//                        Category = TransactionCategory.Other,
//                        Type = TransactionType.Income,
//                        Date = DateTime.Now.AddDays(-15),
//                        Description = "Monthly salary payment"
//                    }

//                // Add more sample transactions here
//                );

//                context.SaveChanges();
//            }

//            if (!context.FinancialGoals.Any())
//            {
//                context.FinancialGoals.AddRange(
//                    new FinancialGoal
//                    {
//                        UserId = "06c1cda4-3143-448e-bfd7-27b0a2568186",
//                        Title = "Buy a new car",
//                        Description = "Save for a down payment on a new car",
//                        TargetAmount = 5000,

//                        EndDate = DateTime.Now.AddYears(1)
//                    },
//                    new FinancialGoal
//                    {
//                        UserId = "06c1cda4-3143-448e-bfd7-27b0a2568186",
//                        Title = "Emergency fund",
//                        Description = "Save for unexpected expenses",
//                        TargetAmount = 10000,

//                        EndDate = DateTime.Now.AddYears(2)
//                    }
//                // Add more sample financial goals here
//                );

//                context.SaveChanges();
//            }
//        }

//    }
//}

using FinanceApp.Enums;
using FinanceApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinanceApp.Data
{
    public static class SeedData
    {
        public static void Seed(ApplicationDbContext context)
        {
            // Use a sample UserId for demonstration purposes
            string sampleUserId = "9753cbd5-e90e-4516-a001-eaa49e2145c7";

            // Seed Transactions
            if (!context.Transactions.Any())
            {
                context.Transactions.AddRange(
                // Add sample transactions here
                new Transaction
                {
                    Amount = 200,
                    Category = TransactionCategory.Expense,
                    SubType = TransactionSubType.Groceries,
                    Date = DateTime.Now.AddDays(-10),
                    Description = "Grocery shopping at the supermarket"
                }

                ); 

                context.SaveChanges();
            }
            // Seed FinancialGoals
            if (!context.FinancialGoals.Any())
            {
                context.FinancialGoals.AddRange(
                // Add sample financial goals here
                 new FinancialGoal
                 {
                     Title = "Buy a new car",
                     Description = "Save for a down payment on a new car",
                     TargetAmount = 5000,

                     EndDate = DateTime.Now.AddYears(1)
                 }
                );
                context.SaveChanges();
            }

            // Seed IncomeExpenses
            if (!context.IncomeVsExpenses.Any())
            {
                context.IncomeVsExpenses.AddRange(GetSampleIncomeVsExpenses(sampleUserId));
                context.SaveChanges();
            }

            // Seed MonthlyBudgets
            if (!context.MonthlyBudgets.Any())
            {
                context.MonthlyBudgets.AddRange(GetSampleMonthlyBudgets(sampleUserId));
                context.SaveChanges();
            }

            // Seed NetWorths
            if (!context.NetWorths.Any())
            {
                context.NetWorths.AddRange(GetSampleNetWorths(sampleUserId));
                context.SaveChanges();
            }
        }

        public static List<IncomeVsExpense> GetSampleIncomeVsExpenses(string userId)
        {
            // Add sample IncomeExpense data here
            var sampleData = new List<IncomeVsExpense>
    {
        new IncomeVsExpense { UserId = userId, Label = "Salary", Amount = 5000, Category = TransactionCategory.Income, SubType = TransactionSubType.Salary },
        new IncomeVsExpense { UserId = userId, Label = "Investments", Amount = 1000, Category = TransactionCategory.Income, SubType = TransactionSubType.Other },
        new IncomeVsExpense { UserId = userId, Label = "Rent", Amount = 1500, Category = TransactionCategory.Expense, SubType = TransactionSubType.Rent },
        new IncomeVsExpense { UserId = userId, Label = "Groceries", Amount = 600, Category = TransactionCategory.Expense, SubType = TransactionSubType.Groceries },
        new IncomeVsExpense { UserId = userId, Label = "Utilities", Amount = 200, Category = TransactionCategory.Expense, SubType = TransactionSubType.Utilities },
        new IncomeVsExpense { UserId = userId, Label = "Entertainment", Amount = 300, Category = TransactionCategory.Expense, SubType = TransactionSubType.Entertainment },
    };

            return sampleData;
        }


        public static List<MonthlyBudget> GetSampleMonthlyBudgets(string userId)
        {
            // Add sample MonthlyBudget data here
            return new List<MonthlyBudget>
            {
                new MonthlyBudget{UserId = userId,Category = TransactionSubType.Rent,Amount = 1500},
                new MonthlyBudget{UserId = userId,Category = TransactionSubType.Groceries,Amount = 600},
                new MonthlyBudget{UserId = userId,Category = TransactionSubType.Utilities,Amount = 200},
                new MonthlyBudget{UserId = userId,Category = TransactionSubType.Entertainment,Amount = 300},
                new MonthlyBudget{UserId = userId,Category = TransactionSubType.Transportation,Amount = 100},
                new MonthlyBudget{UserId = userId,Category = TransactionSubType.Savings,Amount = 800}
            };

        }

        public static List<NetWorth> GetSampleNetWorths(string userId)
        {
            // Add sample NetWorth data here
            var sampleData = new List<NetWorth>
            {
                new NetWorth { UserId = userId, TotalAssets = 55000, TotalLiabilities = 5000, Date = new DateTime(2023, 1, 1) },
                new NetWorth { UserId = userId, TotalAssets = 57000, TotalLiabilities = 5000, Date = new DateTime(2023, 2, 1) },
                new NetWorth { UserId = userId, TotalAssets = 59000, TotalLiabilities = 5000, Date = new DateTime(2023, 3, 1) },
                new NetWorth { UserId = userId, TotalAssets = 61000, TotalLiabilities = 5000, Date = new DateTime(2023, 4, 1) },
                new NetWorth { UserId = userId, TotalAssets = 63000, TotalLiabilities = 5000, Date = new DateTime(2023, 5, 1) },
                new NetWorth { UserId = userId, TotalAssets = 65000, TotalLiabilities = 5000, Date = new DateTime(2023, 6, 1) },
            };

            return sampleData;





        }
    }
}




