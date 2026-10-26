using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Migration aditif BE-MDF-003: Menyiapkan 3 tabel master data Medical Fee Management
    /// (MstMedicalFeeRole, MdfSharingAgreement, MdfSharingRule) beserta indeks, foreign key,
    /// dan check constraint sesuai blueprint MDF-DES-001.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261026130000_AddMedicalFeeMasterData")]
    public partial class AddMedicalFeeMasterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstMedicalFeeRole",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RoleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OprTeamRoleMapping = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IsPrimaryRole = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_MstMedicalFeeRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MdfSharingAgreement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgreementNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceContractHistoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayeeType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Doctor"),
                    PayeeReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveStart = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveEnd = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Draft"),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_MdfSharingAgreement", x => x.Id);
                    table.CheckConstraint("CK_MdfSharingAgreement_PayeeType", "\"PayeeType\" IN ('Doctor','Workforce')");
                    table.CheckConstraint("CK_MdfSharingAgreement_Period", "\"EffectiveEnd\" IS NULL OR \"EffectiveEnd\" >= \"EffectiveStart\"");
                    table.CheckConstraint("CK_MdfSharingAgreement_Status", "\"Status\" IN ('Draft','Active','Expired','Terminated')");
                    table.ForeignKey(
                        name: "FK_MdfSharingAgreement_WfpContractHistory_SourceContractHistoryId",
                        column: x => x.SourceContractHistoryId,
                        principalSchema: "public",
                        principalTable: "WfpContractHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MdfSharingRule",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgreementId = table.Column<Guid>(type: "uuid", nullable: false),
                    TariffId = table.Column<Guid>(type: "uuid", nullable: true),
                    TariffCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    SharingPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    EffectiveStart = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveEnd = table.Column<DateOnly>(type: "date", nullable: true),
                    SupersededByRuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MdfSharingRule", x => x.Id);
                    table.CheckConstraint("CK_MdfSharingRule_Percentage", "\"SharingPercentage\" >= 0 AND \"SharingPercentage\" <= 100");
                    table.CheckConstraint("CK_MdfSharingRule_Period", "\"EffectiveEnd\" IS NULL OR \"EffectiveEnd\" >= \"EffectiveStart\"");
                    table.ForeignKey(
                        name: "FK_MdfSharingRule_MdfSharingAgreement_AgreementId",
                        column: x => x.AgreementId,
                        principalSchema: "public",
                        principalTable: "MdfSharingAgreement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MdfSharingRule_MstMedicalFeeRole_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "public",
                        principalTable: "MstMedicalFeeRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MdfSharingRule_MdfSharingRule_SupersededByRuleId",
                        column: x => x.SupersededByRuleId,
                        principalSchema: "public",
                        principalTable: "MdfSharingRule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MdfSharingRule_MstTariff_TariffId",
                        column: x => x.TariffId,
                        principalSchema: "public",
                        principalTable: "MstTariff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MdfSharingRule_MstTariffCategory_TariffCategoryId",
                        column: x => x.TariffCategoryId,
                        principalSchema: "public",
                        principalTable: "MstTariffCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstMedicalFeeRole_IsActive",
                schema: "public",
                table: "MstMedicalFeeRole",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MstMedicalFeeRole_OprTeamRoleMapping",
                schema: "public",
                table: "MstMedicalFeeRole",
                column: "OprTeamRoleMapping",
                unique: true,
                filter: "\"IsDelete\" = false AND \"OprTeamRoleMapping\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstMedicalFeeRole_RoleCode",
                schema: "public",
                table: "MstMedicalFeeRole",
                column: "RoleCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MdfSharingAgreement_AgreementNumber",
                schema: "public",
                table: "MdfSharingAgreement",
                column: "AgreementNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MdfSharingAgreement_PayeeType_PayeeReferenceId_Status",
                schema: "public",
                table: "MdfSharingAgreement",
                columns: new[] { "PayeeType", "PayeeReferenceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MdfSharingAgreement_SourceContractHistoryId",
                schema: "public",
                table: "MdfSharingAgreement",
                column: "SourceContractHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MdfSharingRule_AgreementId_EffectiveStart_EffectiveEnd",
                schema: "public",
                table: "MdfSharingRule",
                columns: new[] { "AgreementId", "EffectiveStart", "EffectiveEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_MdfSharingRule_RoleId",
                schema: "public",
                table: "MdfSharingRule",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_MdfSharingRule_SupersededByRuleId",
                schema: "public",
                table: "MdfSharingRule",
                column: "SupersededByRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_MdfSharingRule_TariffCategoryId",
                schema: "public",
                table: "MdfSharingRule",
                column: "TariffCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MdfSharingRule_TariffId",
                schema: "public",
                table: "MdfSharingRule",
                column: "TariffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MdfSharingRule",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MdfSharingAgreement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstMedicalFeeRole",
                schema: "public");
        }
    }
}

