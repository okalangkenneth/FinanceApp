using Microsoft.EntityFrameworkCore.Migrations;

namespace FinanceApp.Migrations
{
    public partial class updateModels_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Transactions",
                newName: "SubType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubType",
                table: "Transactions",
                newName: "Type");
        }
    }
}
