using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddFundingSourceToPettyCashMovement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FundingSourceType",
                schema: "public",
                table: "BilPettyCashBudgetMovement",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransferReference",
                schema: "public",
                table: "BilPettyCashBudgetMovement",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_FundingSourceType",
                schema: "public",
                table: "BilPettyCashBudgetMovement",
                sql: "\"FundingSourceType\" IS NULL OR \"FundingSourceType\" IN ('TRANSFER','CASH')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_TransferReference",
                schema: "public",
                table: "BilPettyCashBudgetMovement",
                sql: "\"TransferReference\" IS NULL OR \"FundingSourceType\" = 'TRANSFER'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_FundingSourceType",
                schema: "public",
                table: "BilPettyCashBudgetMovement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_TransferReference",
                schema: "public",
                table: "BilPettyCashBudgetMovement");

            migrationBuilder.DropColumn(
                name: "FundingSourceType",
                schema: "public",
                table: "BilPettyCashBudgetMovement");

            migrationBuilder.DropColumn(
                name: "TransferReference",
                schema: "public",
                table: "BilPettyCashBudgetMovement");
        }
    }
}

