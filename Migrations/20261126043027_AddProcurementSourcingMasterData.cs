using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-PBJ-PS-001 (pemilihan-penyedia, urutan migration 8 di <c>02-module-map.md</c>): tiga tabel master pemilihan
    /// penyedia — <c>MstProcurementMethod</c>, <c>MstProcurementPackageDocumentType</c>,
    /// <c>MstProcurementPackageDocumentRequirement</c>. Data awal diisi <c>ProcurementSourcingMasterSeeder</c>, bukan oleh
    /// migration ini.
    /// </summary>
    /// <remarks>
    /// Saat dibuat, snapshot model tertinggal dari migration yang sudah terpasang (hilang pada merge <c>c97642bb</c>):
    /// <c>20261117074530_AddProcurementVendorManagement</c> (tiga FK master vendor), <c>20261120090000_AddHrdRetirementPolicyAge</c>,
    /// <c>20261120092000_AddHrdContractDecisionNoticeEvidence</c>, dan <c>20261120093000_AddHrdAssignmentLetter</c>. Operasi
    /// untuk objek itu dibuang dari migration ini karena objeknya sudah ada di basis data; snapshot dan Designer tetap
    /// memuat model lengkap.
    /// </remarks>
    public partial class AddProcurementSourcingMasterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstProcurementMethod",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MethodCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MethodName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MinHpsAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MaxHpsAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MinimumQuotationCount = table.Column<int>(type: "integer", nullable: false),
                    RequiresTechnicalEvaluation = table.Column<bool>(type: "boolean", nullable: false),
                    IsSample = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MstProcurementMethod", x => x.Id);
                    table.CheckConstraint("CK_MstProcurementMethod_HpsRange", "\"MinHpsAmount\" IS NULL OR \"MaxHpsAmount\" IS NULL OR \"MinHpsAmount\" < \"MaxHpsAmount\"");
                    table.CheckConstraint("CK_MstProcurementMethod_MaxHpsAmount", "\"MaxHpsAmount\" IS NULL OR \"MaxHpsAmount\" > 0");
                    table.CheckConstraint("CK_MstProcurementMethod_MinHpsAmount", "\"MinHpsAmount\" IS NULL OR \"MinHpsAmount\" >= 0");
                    table.CheckConstraint("CK_MstProcurementMethod_MinimumQuotationCount", "\"MinimumQuotationCount\" BETWEEN 1 AND 10");
                });

            migrationBuilder.CreateTable(
                name: "MstProcurementPackageDocumentType",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DocumentTypeName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Stage = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsPriceSensitive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_MstProcurementPackageDocumentType", x => x.Id);
                    table.CheckConstraint("CK_MstProcurementPackageDocumentType_Stage", "\"Stage\" IN ('PREPARATION', 'QUOTATION', 'NEGOTIATION')");
                });

            migrationBuilder.CreateTable(
                name: "MstProcurementPackageDocumentRequirement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    MethodId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcurementTypeId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_MstProcurementPackageDocumentRequirement", x => x.Id);
                    table.CheckConstraint("CK_MstProcurementPackageDocumentRequirement_ExactlyOneScope", "(\"MethodId\" IS NOT NULL AND \"ProcurementTypeId\" IS NULL) OR (\"MethodId\" IS NULL AND \"ProcurementTypeId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_MstProcurementPackageDocumentRequirement_MstProcurementMeth~",
                        column: x => x.MethodId,
                        principalSchema: "public",
                        principalTable: "MstProcurementMethod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstProcurementPackageDocumentRequirement_MstProcurementPack~",
                        column: x => x.DocumentTypeId,
                        principalSchema: "public",
                        principalTable: "MstProcurementPackageDocumentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstProcurementPackageDocumentRequirement_MstProcurementType~",
                        column: x => x.ProcurementTypeId,
                        principalSchema: "public",
                        principalTable: "MstProcurementType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstProcurementMethod_MethodCode",
                schema: "public",
                table: "MstProcurementMethod",
                column: "MethodCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcurementPackageDocumentRequirement_DocumentTypeId_Meth",
                schema: "public",
                table: "MstProcurementPackageDocumentRequirement",
                columns: new[] { "DocumentTypeId", "MethodId" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"MethodId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcurementPackageDocumentRequirement_DocumentTypeId_Proc",
                schema: "public",
                table: "MstProcurementPackageDocumentRequirement",
                columns: new[] { "DocumentTypeId", "ProcurementTypeId" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"ProcurementTypeId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcurementPackageDocumentRequirement_MethodId",
                schema: "public",
                table: "MstProcurementPackageDocumentRequirement",
                column: "MethodId");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcurementPackageDocumentRequirement_ProcurementTypeId",
                schema: "public",
                table: "MstProcurementPackageDocumentRequirement",
                column: "ProcurementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcurementPackageDocumentType_DocumentTypeCode",
                schema: "public",
                table: "MstProcurementPackageDocumentType",
                column: "DocumentTypeCode",
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstProcurementPackageDocumentRequirement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstProcurementMethod",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstProcurementPackageDocumentType",
                schema: "public");
        }
    }
}
