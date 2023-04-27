using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Models;
using Microsoft.AspNetCore.Identity;

namespace FinanceApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<FinancialGoal> FinancialGoals { get; set; }
        public DbSet<IncomeVsExpense> IncomeVsExpenses { get; set; }
        public DbSet<MonthlyBudget> MonthlyBudgets { get; set; }
        public DbSet<NetWorth> NetWorths { get; set; }
        public DbSet<Budget> Budgets { get; set; } 


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Get the current database provider
            var provider = this.Database.ProviderName;

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(string))
                    {
                        // Apply data type based on the provider
                        if (provider == "Microsoft.EntityFrameworkCore.SqlServer")
                        {
                            property.SetColumnType("nvarchar(MAX)");
                        }
                        else if (provider == "Npgsql.EntityFrameworkCore.PostgreSQL")
                        {
                            property.SetColumnType("text");
                        }
                    }
                }
            }

            // Set precision and scale for decimal properties
            modelBuilder.Entity<FinancialGoal>().Property(e => e.CurrentAmount).HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<FinancialGoal>().Property(e => e.TargetAmount).HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<IncomeVsExpense>().Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<MonthlyBudget>().Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<NetWorth>().Property(e => e.TotalAssets).HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<NetWorth>().Property(e => e.TotalLiabilities).HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<Transaction>().Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<Budget>().Property(e => e.Amount).HasColumnType("decimal(18, 2)");


            modelBuilder.Entity<IdentityRole>(entity =>
            {
                entity.Property(e => e.Id).HasColumnType("nvarchar(450)");
            });


            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.ApplicationUser)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .IsRequired();

            modelBuilder.Entity<FinancialGoal>()
                .HasOne(g => g.ApplicationUser)
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .IsRequired();

            modelBuilder.Entity<IncomeVsExpense>()
                .HasOne(ie => ie.ApplicationUser)
                .WithMany()
                .HasForeignKey(ie => ie.UserId)
                .IsRequired();

            modelBuilder.Entity<MonthlyBudget>()
                .HasOne(mb => mb.ApplicationUser)
                .WithMany()
                .HasForeignKey(mb => mb.UserId)
                .IsRequired();

            modelBuilder.Entity<NetWorth>()
                .HasOne(nw => nw.ApplicationUser)
                .WithMany()
                .HasForeignKey(nw => nw.UserId)
                .IsRequired();
            modelBuilder.Entity<Budget>() // Add this block of code
                .HasOne(b => b.ApplicationUser)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .IsRequired();
        }
    }
}

