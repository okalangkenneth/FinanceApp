using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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

            // UTC strategy: all DateTimes are stored as timestamptz in UTC.
            // Npgsql 6+ rejects non-UTC kinds for timestamptz, but form model
            // binding produces Kind=Unspecified — these are date-valued user
            // inputs, so they are interpreted as UTC rather than shifted.
            var utcDateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.Kind == DateTimeKind.Utc
                    ? v
                    : v.Kind == DateTimeKind.Local
                        ? v.ToUniversalTime()
                        : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(utcDateTimeConverter);
                    }
                    else if (property.ClrType == typeof(decimal))
                    {
                        // Money rule: explicit precision on every monetary column
                        property.SetPrecision(18);
                        property.SetScale(2);
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
