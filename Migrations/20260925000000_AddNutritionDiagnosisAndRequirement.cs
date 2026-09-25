using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Struktur keputusan V1 modul Gizi: master diagnosis berkode dan kebutuhan nutrisi
    /// berparameter (`GIZ-DEC-011`, `GIZ-DEC-012`).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Tujuh tabel baru, dua tabel Gizi disesuaikan. Tidak ada tabel milik modul lain yang
    /// disentuh — khususnya <c>MstDiagnosis</c>, yang sebelumnya ditumpangi diagnosis gizi dan
    /// kini ditinggalkan karena bentuknya ICD dan tidak menyediakan tempat bagi domain IDNT.
    /// </para>
    /// <para>
    /// Pencabutan kolom pada <c>GziNutritionCareRecord</c> aman: seluruh tabel Gizi berisi nol
    /// baris saat migration ini ditulis, sehingga tidak ada data yang hilang.
    /// </para>
    /// <para>
    /// Yang diisi migration ini hanya dua daftar yang disebut langsung oleh keputusan pemilik
    /// proses: tiga domain diagnosis dan lima parameter nutrisi. Master diagnosis dan registry
    /// rumus dibiarkan <b>kosong</b> — isinya belum disahkan siapa pun, dan daftar karangan akan
    /// terlihat resmi padahal bukan.
    /// </para>
    /// <para>
    /// Atribut <c>[Migration]</c> ditulis inline supaya migration ini tetap terbaca EF ketika
    /// <c>SkipMigrationMetadata</c> menyala.
    /// </para>
    /// </remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260925000000_AddNutritionDiagnosisAndRequirement")]
    public partial class AddNutritionDiagnosisAndRequirement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---------------------------------------------------------- master diagnosis

            migrationBuilder.CreateTable(
                name: "GziNutritionDiagnosisDomain",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DomainCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DomainName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_GziNutritionDiagnosisDomain", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GziNutritionDiagnosis",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiagnosisDomainId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentDiagnosisId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiagnosisCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DiagnosisName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Standard = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StandardVersion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IsSelectable = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_GziNutritionDiagnosis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GziNutritionDiagnosis_GziNutritionDiagnosisDomain_Diagnosis~",
                        column: x => x.DiagnosisDomainId,
                        principalSchema: "public",
                        principalTable: "GziNutritionDiagnosisDomain",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GziNutritionDiagnosis_GziNutritionDiagnosis_ParentDiagnosis~",
                        column: x => x.ParentDiagnosisId,
                        principalSchema: "public",
                        principalTable: "GziNutritionDiagnosis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // ------------------------------------------------- master parameter dan rumus

            migrationBuilder.CreateTable(
                name: "GziNutritionParameter",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParameterCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ParameterName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UnitCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ValueScale = table.Column<int>(type: "integer", nullable: false),
                    MinValue = table.Column<decimal>(type: "numeric(12,3)", nullable: true),
                    MaxValue = table.Column<decimal>(type: "numeric(12,3)", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_GziNutritionParameter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GziNutritionFormula",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormulaCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FormulaName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FormulaVersion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SourceReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImplementationKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_GziNutritionFormula", x => x.Id);
                });

            // ------------------------------------------------------- kebutuhan nutrisi

            migrationBuilder.CreateTable(
                name: "GziNutritionRequirement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NutritionOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CareRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CalculationFormulaId = table.Column<Guid>(type: "uuid", nullable: true),
                    CalculationInput = table.Column<string>(type: "jsonb", nullable: true),
                    CalculationPerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeterminedByWorkforceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangeReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_GziNutritionRequirement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GziNutritionRequirement_GziNutritionOrder_NutritionOrderId",
                        column: x => x.NutritionOrderId,
                        principalSchema: "public",
                        principalTable: "GziNutritionOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GziNutritionRequirement_GziNutritionCareRecord_CareRecordId",
                        column: x => x.CareRecordId,
                        principalSchema: "public",
                        principalTable: "GziNutritionCareRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GziNutritionRequirement_GziNutritionFormula_CalculationForm~",
                        column: x => x.CalculationFormulaId,
                        principalSchema: "public",
                        principalTable: "GziNutritionFormula",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GziNutritionRequirement_MstWorkforceProfile_DeterminedByWor~",
                        column: x => x.DeterminedByWorkforceId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GziNutritionRequirementItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NutritionRequirementId = table.Column<Guid>(type: "uuid", nullable: false),
                    NutritionParameterId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalculatedValue = table.Column<decimal>(type: "numeric(12,3)", nullable: true),
                    FinalValue = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    AdjustmentReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AdjustedByWorkforceId = table.Column<Guid>(type: "uuid", nullable: true),
                    AdjustedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_GziNutritionRequirementItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GziNutritionRequirementItem_GziNutritionRequirement_Nutriti~",
                        column: x => x.NutritionRequirementId,
                        principalSchema: "public",
                        principalTable: "GziNutritionRequirement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GziNutritionRequirementItem_GziNutritionParameter_Nutrition~",
                        column: x => x.NutritionParameterId,
                        principalSchema: "public",
                        principalTable: "GziNutritionParameter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GziNutritionRequirementItem_MstWorkforceProfile_AdjustedByW~",
                        column: x => x.AdjustedByWorkforceId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // ------------------------------------------------- diagnosis per kunjungan

            migrationBuilder.CreateTable(
                name: "GziNutritionCareRecordDiagnosis",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CareRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    NutritionDiagnosisId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_GziNutritionCareRecordDiagnosis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GziNutritionCareRecordDiagnosis_GziNutritionCareRecord_Care~",
                        column: x => x.CareRecordId,
                        principalSchema: "public",
                        principalTable: "GziNutritionCareRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GziNutritionCareRecordDiagnosis_GziNutritionDiagnosis_Nutri~",
                        column: x => x.NutritionDiagnosisId,
                        principalSchema: "public",
                        principalTable: "GziNutritionDiagnosis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // ------------------------------------------- penyesuaian tabel yang sudah ada

            // Diagnosis pindah ke master milik Gizi dan menjadi tabel anak; diet tidak lagi
            // diketik bebas; kebutuhan nutrisi tidak lagi satu angka. Aman dijatuhkan karena
            // seluruh tabel Gizi masih nol baris.
            migrationBuilder.DropForeignKey(
                name: "FK_GziNutritionCareRecord_MstDiagnosis_NutritionDiagnosisId",
                schema: "public",
                table: "GziNutritionCareRecord");

            migrationBuilder.DropIndex(
                name: "IX_GziNutritionCareRecord_NutritionDiagnosisId",
                schema: "public",
                table: "GziNutritionCareRecord");

            migrationBuilder.DropColumn(
                name: "NutritionDiagnosisId",
                schema: "public",
                table: "GziNutritionCareRecord");

            migrationBuilder.DropColumn(
                name: "DietPrescription",
                schema: "public",
                table: "GziNutritionCareRecord");

            migrationBuilder.DropColumn(
                name: "EnergyRequirementKcal",
                schema: "public",
                table: "GziNutritionCareRecord");

            migrationBuilder.AddColumn<Guid>(
                name: "PatientDietId",
                schema: "public",
                table: "GziNutritionCareRecord",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NutritionRequirementId",
                schema: "public",
                table: "GziNutritionCareRecord",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NutritionRequirementId",
                schema: "public",
                table: "GziPatientDiet",
                type: "uuid",
                nullable: true);

            // ------------------------------------------------------------------ indeks

            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionDiagnosisDomain_DomainCode",
                schema: "public",
                table: "GziNutritionDiagnosisDomain",
                column: "DomainCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionDiagnosis_DiagnosisCode",
                schema: "public",
                table: "GziNutritionDiagnosis",
                column: "DiagnosisCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionDiagnosis_DiagnosisDomainId_SortOrder",
                schema: "public",
                table: "GziNutritionDiagnosis",
                columns: new[] { "DiagnosisDomainId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionDiagnosis_ParentDiagnosisId",
                schema: "public",
                table: "GziNutritionDiagnosis",
                column: "ParentDiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionParameter_ParameterCode",
                schema: "public",
                table: "GziNutritionParameter",
                column: "ParameterCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionFormula_FormulaCode_FormulaVersion",
                schema: "public",
                table: "GziNutritionFormula",
                columns: new[] { "FormulaCode", "FormulaVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionRequirement_NutritionOrderId_RevisionNumber",
                schema: "public",
                table: "GziNutritionRequirement",
                columns: new[] { "NutritionOrderId", "RevisionNumber" },
                unique: true);

            // `GIZ020`. Ditegakkan basis data, bukan hanya service: dua revisi yang disimpan
            // bersamaan tidak boleh sama-sama mengaku berlaku, karena dapur lalu memilih salah
            // satu tanpa cara mengetahui mana yang benar.
            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionRequirement_NutritionOrderId_Current",
                schema: "public",
                table: "GziNutritionRequirement",
                column: "NutritionOrderId",
                unique: true,
                filter: "\"IsCurrent\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionRequirement_CareRecordId",
                schema: "public",
                table: "GziNutritionRequirement",
                column: "CareRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionRequirement_CalculationFormulaId",
                schema: "public",
                table: "GziNutritionRequirement",
                column: "CalculationFormulaId");

            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionRequirement_DeterminedByWorkforceId",
                schema: "public",
                table: "GziNutritionRequirement",
                column: "DeterminedByWorkforceId");

            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionRequirementItem_Requirement_Parameter",
                schema: "public",
                table: "GziNutritionRequirementItem",
                columns: new[] { "NutritionRequirementId", "NutritionParameterId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionRequirementItem_NutritionParameterId",
                schema: "public",
                table: "GziNutritionRequirementItem",
                column: "NutritionParameterId");

            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionRequirementItem_AdjustedByWorkforceId",
                schema: "public",
                table: "GziNutritionRequirementItem",
                column: "AdjustedByWorkforceId");

            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionCareRecordDiagnosis_CareRecord_Diagnosis",
                schema: "public",
                table: "GziNutritionCareRecordDiagnosis",
                columns: new[] { "CareRecordId", "NutritionDiagnosisId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            // `GIZ018`. Paling banyak satu diagnosis primer per kunjungan.
            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionCareRecordDiagnosis_CareRecordId_Primary",
                schema: "public",
                table: "GziNutritionCareRecordDiagnosis",
                column: "CareRecordId",
                unique: true,
                filter: "\"IsPrimary\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionCareRecordDiagnosis_NutritionDiagnosisId",
                schema: "public",
                table: "GziNutritionCareRecordDiagnosis",
                column: "NutritionDiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionCareRecord_PatientDietId",
                schema: "public",
                table: "GziNutritionCareRecord",
                column: "PatientDietId");

            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionCareRecord_NutritionRequirementId",
                schema: "public",
                table: "GziNutritionCareRecord",
                column: "NutritionRequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_GziPatientDiet_NutritionRequirementId",
                schema: "public",
                table: "GziPatientDiet",
                column: "NutritionRequirementId");

            // -------------------------------------------------------- foreign key susulan

            migrationBuilder.AddForeignKey(
                name: "FK_GziNutritionCareRecord_GziPatientDiet_PatientDietId",
                schema: "public",
                table: "GziNutritionCareRecord",
                column: "PatientDietId",
                principalSchema: "public",
                principalTable: "GziPatientDiet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GziNutritionCareRecord_GziNutritionRequirement_NutritionReq~",
                schema: "public",
                table: "GziNutritionCareRecord",
                column: "NutritionRequirementId",
                principalSchema: "public",
                principalTable: "GziNutritionRequirement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GziPatientDiet_GziNutritionRequirement_NutritionRequirement~",
                schema: "public",
                table: "GziPatientDiet",
                column: "NutritionRequirementId",
                principalSchema: "public",
                principalTable: "GziNutritionRequirement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ----------------------------------------------------------------- isi master

            // Tiga domain dan lima parameter disebut langsung oleh keputusan pemilik proses
            // (`GIZ-DEC-011`, `GIZ-DEC-012`), jadi mengisinya menjalankan keputusan — bukan
            // menebak. Master diagnosis dan registry rumus TIDAK diisi: isinya belum disahkan.
            //
            // Ditulis sebagai InsertData dengan kunci tetap, bukan SQL dengan gen_random_uuid(),
            // supaya baris ini juga terekam pada snapshot lewat HasData. Kunci yang dibangkitkan
            // saat migration berjalan tidak dapat direkam, sehingga snapshot dan basis data akan
            // bercerita berbeda sejak hari pertama.
            var seedAt = new DateTime(2026, 9, 25, 0, 0, 0, DateTimeKind.Utc);
            var kosong = Guid.Empty;

            migrationBuilder.InsertData(
                schema: "public",
                table: "GziNutritionDiagnosisDomain",
                columns: new[]
                {
                    "Id", "DomainCode", "DomainName", "Description", "SortOrder", "IsActive",
                    "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy",
                    "IsCancel", "IsDelete"
                },
                // Tipe kolom ditulis eksplisit supaya migration ini mandiri: berkas Designer
                // sengaja tidak dikompilasi demi waktu build, sehingga EF tidak punya model
                // rujukan untuk menebak tipe baris yang disisipkan.
                columnTypes: new[]
                {
                    "uuid", "character varying(20)", "character varying(200)",
                    "character varying(1000)", "integer", "boolean",
                    "timestamp with time zone", "uuid", "uuid", "uuid", "uuid",
                    "boolean", "boolean"
                },
                values: new object[,]
                {
                    {
                        new Guid("9a1e0001-0000-4000-8000-000000000001"), "NI", "Nutrition Intake",
                        "Domain asupan gizi menurut IDNT.", 1, true,
                        seedAt, kosong, kosong, kosong, kosong, false, false
                    },
                    {
                        new Guid("9a1e0001-0000-4000-8000-000000000002"), "NC", "Nutrition Clinical",
                        "Domain klinis gizi menurut IDNT.", 2, true,
                        seedAt, kosong, kosong, kosong, kosong, false, false
                    },
                    {
                        new Guid("9a1e0001-0000-4000-8000-000000000003"), "NB", "Nutrition Behavioral-Environmental",
                        "Domain perilaku dan lingkungan menurut IDNT.", 3, true,
                        seedAt, kosong, kosong, kosong, kosong, false, false
                    }
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "GziNutritionParameter",
                columns: new[]
                {
                    "Id", "ParameterCode", "ParameterName", "UnitCode", "ValueScale",
                    "MinValue", "MaxValue", "SortOrder", "IsActive",
                    "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy",
                    "IsCancel", "IsDelete"
                },
                columnTypes: new[]
                {
                    "uuid", "character varying(30)", "character varying(200)",
                    "character varying(30)", "integer",
                    "numeric(12,3)", "numeric(12,3)", "integer", "boolean",
                    "timestamp with time zone", "uuid", "uuid", "uuid", "uuid",
                    "boolean", "boolean"
                },
                values: new object[,]
                {
                    {
                        new Guid("9a1e0002-0000-4000-8000-000000000001"), "ENERGY", "Energi", "kkal/hari", 0,
                        1m, 10000m, 1, true,
                        seedAt, kosong, kosong, kosong, kosong, false, false
                    },
                    {
                        new Guid("9a1e0002-0000-4000-8000-000000000002"), "PROTEIN", "Protein", "gram/hari", 1,
                        null, null, 2, true,
                        seedAt, kosong, kosong, kosong, kosong, false, false
                    },
                    {
                        new Guid("9a1e0002-0000-4000-8000-000000000003"), "FAT", "Lemak", "gram/hari", 1,
                        null, null, 3, true,
                        seedAt, kosong, kosong, kosong, kosong, false, false
                    },
                    {
                        new Guid("9a1e0002-0000-4000-8000-000000000004"), "CARBOHYDRATE", "Karbohidrat", "gram/hari", 1,
                        null, null, 4, true,
                        seedAt, kosong, kosong, kosong, kosong, false, false
                    },
                    {
                        new Guid("9a1e0002-0000-4000-8000-000000000005"), "FLUID", "Cairan", "ml/hari", 0,
                        null, null, 5, true,
                        seedAt, kosong, kosong, kosong, kosong, false, false
                    }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GziPatientDiet_GziNutritionRequirement_NutritionRequirement~",
                schema: "public",
                table: "GziPatientDiet");

            migrationBuilder.DropForeignKey(
                name: "FK_GziNutritionCareRecord_GziNutritionRequirement_NutritionReq~",
                schema: "public",
                table: "GziNutritionCareRecord");

            migrationBuilder.DropForeignKey(
                name: "FK_GziNutritionCareRecord_GziPatientDiet_PatientDietId",
                schema: "public",
                table: "GziNutritionCareRecord");

            migrationBuilder.DropTable(name: "GziNutritionCareRecordDiagnosis", schema: "public");
            migrationBuilder.DropTable(name: "GziNutritionRequirementItem", schema: "public");
            migrationBuilder.DropTable(name: "GziNutritionRequirement", schema: "public");
            migrationBuilder.DropTable(name: "GziNutritionFormula", schema: "public");
            migrationBuilder.DropTable(name: "GziNutritionParameter", schema: "public");
            migrationBuilder.DropTable(name: "GziNutritionDiagnosis", schema: "public");
            migrationBuilder.DropTable(name: "GziNutritionDiagnosisDomain", schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_GziPatientDiet_NutritionRequirementId",
                schema: "public",
                table: "GziPatientDiet");

            migrationBuilder.DropIndex(
                name: "IX_GziNutritionCareRecord_NutritionRequirementId",
                schema: "public",
                table: "GziNutritionCareRecord");

            migrationBuilder.DropIndex(
                name: "IX_GziNutritionCareRecord_PatientDietId",
                schema: "public",
                table: "GziNutritionCareRecord");

            migrationBuilder.DropColumn(
                name: "NutritionRequirementId", schema: "public", table: "GziPatientDiet");
            migrationBuilder.DropColumn(
                name: "NutritionRequirementId", schema: "public", table: "GziNutritionCareRecord");
            migrationBuilder.DropColumn(
                name: "PatientDietId", schema: "public", table: "GziNutritionCareRecord");

            migrationBuilder.AddColumn<Guid>(
                name: "NutritionDiagnosisId",
                schema: "public",
                table: "GziNutritionCareRecord",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DietPrescription",
                schema: "public",
                table: "GziNutritionCareRecord",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EnergyRequirementKcal",
                schema: "public",
                table: "GziNutritionCareRecord",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionCareRecord_NutritionDiagnosisId",
                schema: "public",
                table: "GziNutritionCareRecord",
                column: "NutritionDiagnosisId");

            migrationBuilder.AddForeignKey(
                name: "FK_GziNutritionCareRecord_MstDiagnosis_NutritionDiagnosisId",
                schema: "public",
                table: "GziNutritionCareRecord",
                column: "NutritionDiagnosisId",
                principalSchema: "public",
                principalTable: "MstDiagnosis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
