using FinanceApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceApp.Data
{
    /// <summary>
    /// Dev/demo seeding, run from Program.cs only when SEED_DEMO_DATA=true
    /// (docker-compose dev stack; also what the Phase 6 recorded demo shows).
    /// The demo password comes from configuration (SeedDemo:Password via the
    /// gitignored .env) — with no password configured, seeding is skipped.
    /// </summary>
    public static class SeedData
    {
        public const string DemoEmail = "demo@fintrak.example";

        public static async Task SeedDemoAsync(IServiceProvider services, ILogger logger)
        {
            var configuration = services.GetRequiredService<IConfiguration>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var context = services.GetRequiredService<ApplicationDbContext>();

            string password = configuration["SeedDemo:Password"];
            if (string.IsNullOrEmpty(password))
            {
                logger.LogWarning("[SeedData] SEED_DEMO_DATA=true but SeedDemo:Password is not configured — skipping demo seeding");
                return;
            }

            var user = await userManager.FindByEmailAsync(DemoEmail);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = DemoEmail,
                    Email = DemoEmail,
                    FirstName = "Demo",
                    LastName = "User",
                    PreferredCurrency = "SEK",
                    EmailConfirmed = true // no live email flow in the dev stack
                };
                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    logger.LogError("[SeedData] Could not create demo user: {Errors}",
                        string.Join("; ", result.Errors.Select(e => e.Description)));
                    return;
                }
                logger.LogInformation("[SeedData] Created demo user {Email}", DemoEmail);
            }

            if (context.Transactions.Any(t => t.UserId == user.Id))
            {
                logger.LogInformation("[SeedData] Demo data already present — skipping");
                return;
            }

            // Four months of a simple, readable story (amounts in SEK):
            // salary comes in monthly, rent/groceries/transport/streaming go
            // out, and a fixed savings transfer feeds the emergency fund goal.
            var transactions = new List<Transaction>();
            DateTime monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            for (int monthsBack = 3; monthsBack >= 0; monthsBack--)
            {
                DateTime m = monthStart.AddMonths(-monthsBack);

                transactions.Add(Make(user.Id, m.AddDays(24), 34200m, TransactionType.Income, TransactionCategory.Salary, "Monthly salary"));
                transactions.Add(Make(user.Id, m, 9800m, TransactionType.Expense, TransactionCategory.Rent, "Rent"));
                transactions.Add(Make(user.Id, m.AddDays(2), 5000m, TransactionType.Expense, TransactionCategory.Savings, "Transfer to emergency fund"));
                transactions.Add(Make(user.Id, m.AddDays(3), 612m, TransactionType.Expense, TransactionCategory.Utilities, "Electricity"));
                transactions.Add(Make(user.Id, m.AddDays(5), 379m, TransactionType.Expense, TransactionCategory.Entertainment, "Streaming subscriptions"));
                transactions.Add(Make(user.Id, m.AddDays(6), 970m, TransactionType.Expense, TransactionCategory.Transportation, "Monthly transit card"));

                // Weekly groceries with slight variation so charts look real
                for (int week = 0; week < 4; week++)
                {
                    transactions.Add(Make(user.Id, m.AddDays(4 + week * 7), 740m + week * 55m,
                        TransactionType.Expense, TransactionCategory.Food, "Groceries"));
                }
            }

            // One-offs that give the spending analysis something to find
            transactions.Add(Make(user.Id, monthStart.AddMonths(-2).AddDays(11), 2890m, TransactionType.Expense, TransactionCategory.Health, "Dental visit"));
            transactions.Add(Make(user.Id, monthStart.AddMonths(-1).AddDays(17), 4350m, TransactionType.Expense, TransactionCategory.Entertainment, "Concert tickets"));
            transactions.Add(Make(user.Id, monthStart.AddMonths(-1).AddDays(20), 6500m, TransactionType.Income, TransactionCategory.Freelance, "Freelance article"));

            context.Transactions.AddRange(transactions);

            context.FinancialGoals.AddRange(
                new FinancialGoal
                {
                    UserId = user.Id,
                    Title = "Emergency fund",
                    Description = "Six months of expenses in a savings account",
                    TargetAmount = 90000m,
                    CurrentAmount = 47500m,
                    StartDate = monthStart.AddMonths(-8),
                    EndDate = monthStart.AddMonths(10),
                    Status = GoalStatus.InProgress,
                    Currency = Currency.SEK
                },
                new FinancialGoal
                {
                    UserId = user.Id,
                    Title = "Summer trip to Portugal",
                    Description = "Flights, hotel and spending money",
                    TargetAmount = 18000m,
                    CurrentAmount = 5200m,
                    StartDate = monthStart.AddMonths(-2),
                    EndDate = monthStart.AddMonths(4),
                    Status = GoalStatus.InProgress,
                    Currency = Currency.SEK
                },
                new FinancialGoal
                {
                    UserId = user.Id,
                    Title = "New laptop",
                    Description = "Replaced the 2019 machine",
                    TargetAmount = 16000m,
                    CurrentAmount = 16000m,
                    StartDate = monthStart.AddMonths(-7),
                    EndDate = monthStart.AddMonths(-1),
                    Status = GoalStatus.Completed,
                    Currency = Currency.SEK
                });

            await context.SaveChangesAsync();
            logger.LogInformation("[SeedData] Seeded {Count} transactions and 3 goals for {Email}",
                transactions.Count, DemoEmail);
        }

        private static Transaction Make(string userId, DateTime date, decimal amount,
            TransactionType type, TransactionCategory category, string description) => new Transaction
            {
                UserId = userId,
                Date = DateTime.SpecifyKind(date, DateTimeKind.Utc),
                Amount = amount,
                Type = type,
                Category = category,
                Description = description,
                Currency = Currency.SEK
            };
    }
}
