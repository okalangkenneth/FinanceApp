using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Models;



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
        }

    }
}

