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
            // 1. Rename tables to Corporate/FinanceManagement Fin* prefix (QBE-DB-002: preserves existing data)
            migrationBuilder.RenameTable(
                name: "BilPettyCashBudget",
                schema: "public",
                newName: "FinPettyCashBudget",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "BilPettyCashBudgetMovement",
                schema: "public",
                newName: "FinPettyCashBudgetMovement",
                newSchema: "public");

            // 2. Add columns for funding source to FinPettyCashBudgetMovement (PC-DES-026)
            migrationBuilder.AddColumn<string>(
                name: "FundingSourceType",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransferReference",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            // 3. Drop legacy check constraints from renamed tables
            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashBudget_BudgetAmount",
                schema: "public",
                table: "FinPettyCashBudget");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashBudget_CurrentBalance",
                schema: "public",
                table: "FinPettyCashBudget");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashBudget_Status",
                schema: "public",
                table: "FinPettyCashBudget");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_Amount",
                schema: "public",
                table: "FinPettyCashBudgetMovement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_BalanceAfter",
                schema: "public",
                table: "FinPettyCashBudgetMovement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_MovementType",
                schema: "public",
                table: "FinPettyCashBudgetMovement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_Reason",
                schema: "public",
                table: "FinPettyCashBudgetMovement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_VoucherId",
                schema: "public",
                table: "FinPettyCashBudgetMovement");

            // 4. Add updated check constraints with Fin naming
            migrationBuilder.AddCheckConstraint(
                name: "CK_FinPettyCashBudget_BudgetAmount",
                schema: "public",
                table: "FinPettyCashBudget",
                sql: "\"BudgetAmount\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinPettyCashBudget_CurrentBalance",
                schema: "public",
                table: "FinPettyCashBudget",
                sql: "\"CurrentBalance\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinPettyCashBudget_Status",
                schema: "public",
                table: "FinPettyCashBudget",
                sql: "\"Status\" IN ('ACTIVE','INACTIVE','DRAFT','CLOSED')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinPettyCashBudgetMovement_Amount",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                sql: "\"Amount\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinPettyCashBudgetMovement_BalanceAfter",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                sql: "\"BalanceAfter\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinPettyCashBudgetMovement_MovementType",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                sql: "\"MovementType\" IN ('TOP_UP','DISBURSEMENT','ADJUSTMENT','RETURN','REVERSAL','CARRY_FORWARD_OUT','CARRY_FORWARD_IN')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinPettyCashBudgetMovement_Reason",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                sql: "\"MovementType\" = 'DISBURSEMENT' OR \"Reason\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinPettyCashBudgetMovement_VoucherId",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                sql: "(\"MovementType\" IN ('DISBURSEMENT','RETURN','REVERSAL') AND \"VoucherId\" IS NOT NULL) OR (\"MovementType\" NOT IN ('DISBURSEMENT','RETURN','REVERSAL') AND \"VoucherId\" IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinPettyCashBudgetMovement_FundingSourceType",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                sql: "\"FundingSourceType\" IS NULL OR \"FundingSourceType\" IN ('TRANSFER','CASH')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinPettyCashBudgetMovement_TransferReference",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                sql: "\"TransferReference\" IS NULL OR \"FundingSourceType\" = 'TRANSFER'");

            // 5. Rename indexes to Fin naming
            migrationBuilder.RenameIndex(
                name: "IX_BilPettyCashBudget_ActivePerPool",
                schema: "public",
                table: "FinPettyCashBudget",
                newName: "IX_FinPettyCashBudget_ActivePerPool");

            migrationBuilder.RenameIndex(
                name: "IX_BilPettyCashBudget_PoolCode_PeriodStart",
                schema: "public",
                table: "FinPettyCashBudget",
                newName: "IX_FinPettyCashBudget_PoolCode_PeriodStart");

            migrationBuilder.RenameIndex(
                name: "IX_BilPettyCashBudget_SupersededByBudgetId",
                schema: "public",
                table: "FinPettyCashBudget",
                newName: "IX_FinPettyCashBudget_SupersededByBudgetId");

            migrationBuilder.RenameIndex(
                name: "IX_BilPettyCashBudgetMovement_Budget_OccurredAt",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                newName: "IX_FinPettyCashBudgetMovement_Budget_OccurredAt");

            migrationBuilder.RenameIndex(
                name: "IX_BilPettyCashBudgetMovement_IdempotencyKey",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                newName: "IX_FinPettyCashBudgetMovement_IdempotencyKey");

            migrationBuilder.RenameIndex(
                name: "IX_BilPettyCashBudgetMovement_Voucher_Disbursement",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                newName: "IX_FinPettyCashBudgetMovement_Voucher_Disbursement");

            migrationBuilder.RenameIndex(
                name: "IX_BilPettyCashBudgetMovement_Voucher_Reversal",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                newName: "IX_FinPettyCashBudgetMovement_Voucher_Reversal");

            // 6. Rename/recreate foreign keys
            migrationBuilder.DropForeignKey(
                name: "FK_BilPettyCashBudget_BilPettyCashBudget_SupersededByBudgetId",
                schema: "public",
                table: "FinPettyCashBudget");

            migrationBuilder.DropForeignKey(
                name: "FK_BilPettyCashBudgetMovement_BilPettyCashBudget_BudgetId",
                schema: "public",
                table: "FinPettyCashBudgetMovement");

            migrationBuilder.DropForeignKey(
                name: "FK_BilPettyCashBudgetMovement_BilPettyCashVoucher_VoucherId",
                schema: "public",
                table: "FinPettyCashBudgetMovement");

            migrationBuilder.AddForeignKey(
                name: "FK_FinPettyCashBudget_FinPettyCashBudget_SupersededByBudgetId",
                schema: "public",
                table: "FinPettyCashBudget",
                column: "SupersededByBudgetId",
                principalSchema: "public",
                principalTable: "FinPettyCashBudget",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinPettyCashBudgetMovement_FinPettyCashBudget_BudgetId",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                column: "BudgetId",
                principalSchema: "public",
                principalTable: "FinPettyCashBudget",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinPettyCashBudgetMovement_BilPettyCashVoucher_VoucherId",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                column: "VoucherId",
                principalSchema: "public",
                principalTable: "BilPettyCashVoucher",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Drop Fin foreign keys and restore Bil foreign keys
            migrationBuilder.DropForeignKey(
                name: "FK_FinPettyCashBudgetMovement_BilPettyCashVoucher_VoucherId",
                schema: "public",
                table: "FinPettyCashBudgetMovement");

            migrationBuilder.DropForeignKey(
                name: "FK_FinPettyCashBudgetMovement_FinPettyCashBudget_BudgetId",
                schema: "public",
                table: "FinPettyCashBudgetMovement");

            migrationBuilder.DropForeignKey(
                name: "FK_FinPettyCashBudget_FinPettyCashBudget_SupersededByBudgetId",
                schema: "public",
                table: "FinPettyCashBudget");

            migrationBuilder.AddForeignKey(
                name: "FK_BilPettyCashBudget_BilPettyCashBudget_SupersededByBudgetId",
                schema: "public",
                table: "FinPettyCashBudget",
                column: "SupersededByBudgetId",
                principalSchema: "public",
                principalTable: "FinPettyCashBudget",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BilPettyCashBudgetMovement_BilPettyCashBudget_BudgetId",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                column: "BudgetId",
                principalSchema: "public",
                principalTable: "FinPettyCashBudget",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BilPettyCashBudgetMovement_BilPettyCashVoucher_VoucherId",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                column: "VoucherId",
                principalSchema: "public",
                principalTable: "BilPettyCashVoucher",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 2. Rename indexes back to Bil naming
            migrationBuilder.RenameIndex(
                name: "IX_FinPettyCashBudgetMovement_Voucher_Reversal",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                newName: "IX_BilPettyCashBudgetMovement_Voucher_Reversal");

            migrationBuilder.RenameIndex(
                name: "IX_FinPettyCashBudgetMovement_Voucher_Disbursement",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                newName: "IX_BilPettyCashBudgetMovement_Voucher_Disbursement");

            migrationBuilder.RenameIndex(
                name: "IX_FinPettyCashBudgetMovement_IdempotencyKey",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                newName: "IX_BilPettyCashBudgetMovement_IdempotencyKey");

            migrationBuilder.RenameIndex(
                name: "IX_FinPettyCashBudgetMovement_Budget_OccurredAt",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                newName: "IX_BilPettyCashBudgetMovement_Budget_OccurredAt");

            migrationBuilder.RenameIndex(
                name: "IX_FinPettyCashBudget_SupersededByBudgetId",
                schema: "public",
                table: "FinPettyCashBudget",
                newName: "IX_BilPettyCashBudget_SupersededByBudgetId");

            migrationBuilder.RenameIndex(
                name: "IX_FinPettyCashBudget_PoolCode_PeriodStart",
                schema: "public",
                table: "FinPettyCashBudget",
                newName: "IX_BilPettyCashBudget_PoolCode_PeriodStart");

            migrationBuilder.RenameIndex(
                name: "IX_FinPettyCashBudget_ActivePerPool",
                schema: "public",
                table: "FinPettyCashBudget",
                newName: "IX_BilPettyCashBudget_ActivePerPool");

            // 3. Drop Fin check constraints
            migrationBuilder.DropCheckConstraint(
                name: "CK_FinPettyCashBudgetMovement_TransferReference",
                schema: "public",
                table: "FinPettyCashBudgetMovement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinPettyCashBudgetMovement_FundingSourceType",
                schema: "public",
                table: "FinPettyCashBudgetMovement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinPettyCashBudgetMovement_VoucherId",
                schema: "public",
                table: "FinPettyCashBudgetMovement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinPettyCashBudgetMovement_Reason",
                schema: "public",
                table: "FinPettyCashBudgetMovement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinPettyCashBudgetMovement_MovementType",
                schema: "public",
                table: "FinPettyCashBudgetMovement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinPettyCashBudgetMovement_BalanceAfter",
                schema: "public",
                table: "FinPettyCashBudgetMovement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinPettyCashBudgetMovement_Amount",
                schema: "public",
                table: "FinPettyCashBudgetMovement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinPettyCashBudget_Status",
                schema: "public",
                table: "FinPettyCashBudget");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinPettyCashBudget_CurrentBalance",
                schema: "public",
                table: "FinPettyCashBudget");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinPettyCashBudget_BudgetAmount",
                schema: "public",
                table: "FinPettyCashBudget");

            // 4. Restore Bil check constraints
            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashBudget_BudgetAmount",
                schema: "public",
                table: "FinPettyCashBudget",
                sql: "\"BudgetAmount\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashBudget_CurrentBalance",
                schema: "public",
                table: "FinPettyCashBudget",
                sql: "\"CurrentBalance\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashBudget_Status",
                schema: "public",
                table: "FinPettyCashBudget",
                sql: "\"Status\" IN ('ACTIVE','INACTIVE','DRAFT','CLOSED')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_Amount",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                sql: "\"Amount\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_BalanceAfter",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                sql: "\"BalanceAfter\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_MovementType",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                sql: "\"MovementType\" IN ('TOP_UP','DISBURSEMENT','ADJUSTMENT','RETURN','REVERSAL','CARRY_FORWARD_OUT','CARRY_FORWARD_IN')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_Reason",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                sql: "\"MovementType\" = 'DISBURSEMENT' OR \"Reason\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPettyCashBudgetMovement_VoucherId",
                schema: "public",
                table: "FinPettyCashBudgetMovement",
                sql: "(\"MovementType\" IN ('DISBURSEMENT','RETURN','REVERSAL') AND \"VoucherId\" IS NOT NULL) OR (\"MovementType\" NOT IN ('DISBURSEMENT','RETURN','REVERSAL') AND \"VoucherId\" IS NULL)");

            // 5. Drop added columns
            migrationBuilder.DropColumn(
                name: "FundingSourceType",
                schema: "public",
                table: "FinPettyCashBudgetMovement");

            migrationBuilder.DropColumn(
                name: "TransferReference",
                schema: "public",
                table: "FinPettyCashBudgetMovement");

            // 6. Rename tables back to Bil*
            migrationBuilder.RenameTable(
                name: "FinPettyCashBudgetMovement",
                schema: "public",
                newName: "BilPettyCashBudgetMovement",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "FinPettyCashBudget",
                schema: "public",
                newName: "BilPettyCashBudget",
                newSchema: "public");
        }
    }
}
