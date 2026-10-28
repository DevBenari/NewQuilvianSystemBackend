using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Penambahan enam kolom pelaksana dan jejak asal DoctorShare beserta empat check constraint
    /// pada tabel BilInvoiceItem untuk gelombang revisi 2.4 (MVP-38 / BE-BKC-089).
    /// Menjaga invariant INV-MF-3 (pelaksana wajib bila berjasa) dan INV-MF-5 (jejak resolusi konsisten).
    /// Seluruh baris lama tetap valid (IsFeeBearing = false, kolom lain NULL).
    /// </summary>
    /// <inheritdoc />
    public partial class AddBilInvoiceItemPerformerAndDoctorShareTrace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFeeBearing",
                schema: "public",
                table: "BilInvoiceItem",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PerformerType",
                schema: "public",
                table: "BilInvoiceItem",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PerformerReferenceId",
                schema: "public",
                table: "BilInvoiceItem",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerformerRoleCode",
                schema: "public",
                table: "BilInvoiceItem",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorShareResolution",
                schema: "public",
                table: "BilInvoiceItem",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DoctorShareSharingRuleId",
                schema: "public",
                table: "BilInvoiceItem",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilInvoiceItem_PerformerRequired",
                schema: "public",
                table: "BilInvoiceItem",
                sql: "NOT \"IsFeeBearing\" OR (\"PerformerType\" IS NOT NULL AND \"PerformerReferenceId\" IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilInvoiceItem_PerformerOnlyWhenFeeBearing",
                schema: "public",
                table: "BilInvoiceItem",
                sql: "\"IsFeeBearing\" OR (\"PerformerType\" IS NULL AND \"PerformerReferenceId\" IS NULL AND \"PerformerRoleCode\" IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilInvoiceItem_PerformerType",
                schema: "public",
                table: "BilInvoiceItem",
                sql: "\"PerformerType\" IS NULL OR \"PerformerType\" IN ('DOCTOR','WORKFORCE')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilInvoiceItem_DoctorShareResolution",
                schema: "public",
                table: "BilInvoiceItem",
                sql: "\"DoctorShareResolution\" IS NULL OR (\"DoctorShareResolution\" IN ('MEDICAL_FEE','NO_RULE','UNAVAILABLE','NOT_APPLICABLE') AND (\"DoctorShareResolution\" = 'MEDICAL_FEE') = (\"DoctorShareSharingRuleId\" IS NOT NULL))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BilInvoiceItem_DoctorShareResolution",
                schema: "public",
                table: "BilInvoiceItem");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilInvoiceItem_PerformerType",
                schema: "public",
                table: "BilInvoiceItem");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilInvoiceItem_PerformerOnlyWhenFeeBearing",
                schema: "public",
                table: "BilInvoiceItem");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilInvoiceItem_PerformerRequired",
                schema: "public",
                table: "BilInvoiceItem");

            migrationBuilder.DropColumn(
                name: "DoctorShareSharingRuleId",
                schema: "public",
                table: "BilInvoiceItem");

            migrationBuilder.DropColumn(
                name: "DoctorShareResolution",
                schema: "public",
                table: "BilInvoiceItem");

            migrationBuilder.DropColumn(
                name: "PerformerRoleCode",
                schema: "public",
                table: "BilInvoiceItem");

            migrationBuilder.DropColumn(
                name: "PerformerReferenceId",
                schema: "public",
                table: "BilInvoiceItem");

            migrationBuilder.DropColumn(
                name: "PerformerType",
                schema: "public",
                table: "BilInvoiceItem");

            migrationBuilder.DropColumn(
                name: "IsFeeBearing",
                schema: "public",
                table: "BilInvoiceItem");
        }
    }
}
