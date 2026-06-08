using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeStructureBank : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankName",
                schema: "public",
                table: "WfpBankAccount");

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdateBy",
                schema: "public",
                table: "WfpBankAccount",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeleteBy",
                schema: "public",
                table: "WfpBankAccount",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreateBy",
                schema: "public",
                table: "WfpBankAccount",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "CancelBy",
                schema: "public",
                table: "WfpBankAccount",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "BankId",
                schema: "public",
                table: "WfpBankAccount",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "MstBank",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BankCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BankName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BankShortName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankCategory = table.Column<int>(type: "integer", nullable: false),
                    ClearingCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstBank", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WfpBankAccount_BankId",
                schema: "public",
                table: "WfpBankAccount",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpBankAccount_WorkforceProfileId_BankId_AccountNumber_IsDe~",
                schema: "public",
                table: "WfpBankAccount",
                columns: new[] { "WorkforceProfileId", "BankId", "AccountNumber", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpBankAccount_WorkforceProfileId_BankId_IsActive_IsDelete",
                schema: "public",
                table: "WfpBankAccount",
                columns: new[] { "WorkforceProfileId", "BankId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpBankAccount_WorkforceProfileId_IsActive_IsDelete",
                schema: "public",
                table: "WfpBankAccount",
                columns: new[] { "WorkforceProfileId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpBankAccount_WorkforceProfileId_IsPrimary_IsActive_IsDele~",
                schema: "public",
                table: "WfpBankAccount",
                columns: new[] { "WorkforceProfileId", "IsPrimary", "IsActive", "IsDelete" });

            migrationBuilder.AddForeignKey(
                name: "FK_WfpBankAccount_MstBank_BankId",
                schema: "public",
                table: "WfpBankAccount",
                column: "BankId",
                principalSchema: "public",
                principalTable: "MstBank",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WfpBankAccount_MstBank_BankId",
                schema: "public",
                table: "WfpBankAccount");

            migrationBuilder.DropTable(
                name: "MstBank",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_WfpBankAccount_BankId",
                schema: "public",
                table: "WfpBankAccount");

            migrationBuilder.DropIndex(
                name: "IX_WfpBankAccount_WorkforceProfileId_BankId_AccountNumber_IsDe~",
                schema: "public",
                table: "WfpBankAccount");

            migrationBuilder.DropIndex(
                name: "IX_WfpBankAccount_WorkforceProfileId_BankId_IsActive_IsDelete",
                schema: "public",
                table: "WfpBankAccount");

            migrationBuilder.DropIndex(
                name: "IX_WfpBankAccount_WorkforceProfileId_IsActive_IsDelete",
                schema: "public",
                table: "WfpBankAccount");

            migrationBuilder.DropIndex(
                name: "IX_WfpBankAccount_WorkforceProfileId_IsPrimary_IsActive_IsDele~",
                schema: "public",
                table: "WfpBankAccount");

            migrationBuilder.DropColumn(
                name: "BankId",
                schema: "public",
                table: "WfpBankAccount");

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdateBy",
                schema: "public",
                table: "WfpBankAccount",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "DeleteBy",
                schema: "public",
                table: "WfpBankAccount",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "CreateBy",
                schema: "public",
                table: "WfpBankAccount",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "CancelBy",
                schema: "public",
                table: "WfpBankAccount",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                schema: "public",
                table: "WfpBankAccount",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
