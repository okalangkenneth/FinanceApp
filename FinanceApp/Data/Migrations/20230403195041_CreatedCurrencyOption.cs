using Microsoft.EntityFrameworkCore.Migrations;

namespace FinanceApp.Data.Migrations
{
    public partial class CreatedCurrencyOption : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Currency",
                table: "FinancialGoals",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "FinancialGoals");
        }
    }
}
