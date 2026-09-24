using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingManagementRevisionsDoctorMemoAndRefundCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DoctorDiscountMemoFile",
                schema: "public",
                table: "BilDiscountApplication",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "RefundableCreditId",
                schema: "public",
                table: "BilRefundCase",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "RefundCategory",
                schema: "public",
                table: "BilRefundCase",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "BILLING");

            migrationBuilder.AddColumn<string>(
                name: "SelectedBillingItemIdsJson",
                schema: "public",
                table: "BilRefundCase",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedBillingItemIdsJson",
                schema: "public",
                table: "BilRefundCase");

            migrationBuilder.DropColumn(
                name: "RefundCategory",
                schema: "public",
                table: "BilRefundCase");

            migrationBuilder.AlterColumn<Guid>(
                name: "RefundableCreditId",
                schema: "public",
                table: "BilRefundCase",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "DoctorDiscountMemoFile",
                schema: "public",
                table: "BilDiscountApplication");
        }
    }
}
