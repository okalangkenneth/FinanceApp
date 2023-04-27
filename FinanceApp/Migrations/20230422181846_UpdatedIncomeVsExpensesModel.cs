using Microsoft.EntityFrameworkCore.Migrations;

namespace FinanceApp.Migrations
{
    public partial class UpdatedIncomeVsExpensesModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "IncomeVsExpenses",
                newName: "SubType");

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "IncomeVsExpenses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "IncomeVsExpenses");

            migrationBuilder.RenameColumn(
                name: "SubType",
                table: "IncomeVsExpenses",
                newName: "Type");
        }
    }
}
