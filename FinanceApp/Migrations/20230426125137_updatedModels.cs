using Microsoft.EntityFrameworkCore.Migrations;

namespace FinanceApp.Migrations
{
    public partial class updatedModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FinancialGoalId",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GoalId",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_FinancialGoalId",
                table: "Transactions",
                column: "FinancialGoalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_FinancialGoals_FinancialGoalId",
                table: "Transactions",
                column: "FinancialGoalId",
                principalTable: "FinancialGoals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_FinancialGoals_FinancialGoalId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_FinancialGoalId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "FinancialGoalId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "GoalId",
                table: "Transactions");
        }
    }
}
