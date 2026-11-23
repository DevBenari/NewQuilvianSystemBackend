using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    // BE-HRD-440 (kontrak v29, kamus data 30.1–30.2; izin migration pengguna 20 November 2026): hanya dua kolom, FK, dan index.
    // Operasi HrdOvertimeSetting yang ikut terbentuk karena snapshot belum memuatnya dikeluarkan: tabel itu dibuat migration manual
    // idempoten 20261118100000_AddHrdOvertimeSetting. Snapshot kini memuat HrdOvertimeSetting sesuai model.
    public partial class AddWorkflowBulkFlagAndEscalationLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EscalatedFromAssignmentId",
                schema: "public",
                table: "TrxWorkflowApproverAssignment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowBulkApproval",
                schema: "public",
                table: "MstWorkflowDefinition",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_TrxWorkflowApproverAssignment_EscalatedFromAssignmentId",
                schema: "public",
                table: "TrxWorkflowApproverAssignment",
                column: "EscalatedFromAssignmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxWorkflowApproverAssignment_TrxWorkflowApproverAssignment~",
                schema: "public",
                table: "TrxWorkflowApproverAssignment",
                column: "EscalatedFromAssignmentId",
                principalSchema: "public",
                principalTable: "TrxWorkflowApproverAssignment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxWorkflowApproverAssignment_TrxWorkflowApproverAssignment~",
                schema: "public",
                table: "TrxWorkflowApproverAssignment");

            migrationBuilder.DropIndex(
                name: "IX_TrxWorkflowApproverAssignment_EscalatedFromAssignmentId",
                schema: "public",
                table: "TrxWorkflowApproverAssignment");

            migrationBuilder.DropColumn(
                name: "EscalatedFromAssignmentId",
                schema: "public",
                table: "TrxWorkflowApproverAssignment");

            migrationBuilder.DropColumn(
                name: "AllowBulkApproval",
                schema: "public",
                table: "MstWorkflowDefinition");
        }
    }
}
