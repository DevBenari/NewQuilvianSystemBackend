using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeTelaahResep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstPrescriptionReviewCriterion",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CriterionCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CriterionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    DefaultSeverity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsApplicableToRegular = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsApplicableToCompound = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsSystemCheckSupported = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstPrescriptionReviewCriterion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrxPrescriptionDrugSubstitution",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrescriptionCompoundItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    OriginalDrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubstituteDrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReasonNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ProposedByPharmacistId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovalStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ApprovedByDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DoctorApprovalNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPrescriptionDrugSubstitution", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionDrugSubstitution_AspNetUsers_ApprovedByDocto~",
                        column: x => x.ApprovedByDoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionDrugSubstitution_AspNetUsers_ProposedByPharm~",
                        column: x => x.ProposedByPharmacistId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionDrugSubstitution_MstDrug_OriginalDrugId",
                        column: x => x.OriginalDrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionDrugSubstitution_MstDrug_SubstituteDrugId",
                        column: x => x.SubstituteDrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionDrugSubstitution_TrxPrescription_Prescriptio~",
                        column: x => x.PrescriptionId,
                        principalSchema: "public",
                        principalTable: "TrxPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionDrugSubstitution_TrxPrescriptionCompoundItem~",
                        column: x => x.PrescriptionCompoundItemId,
                        principalSchema: "public",
                        principalTable: "TrxPrescriptionCompoundItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionDrugSubstitution_TrxPrescriptionItem_Prescri~",
                        column: x => x.PrescriptionItemId,
                        principalSchema: "public",
                        principalTable: "TrxPrescriptionItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPrescriptionFinalCheck",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CheckedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPrescriptionFinalCheck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionFinalCheck_AspNetUsers_CheckedByUserId",
                        column: x => x.CheckedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionFinalCheck_TrxPrescription_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalSchema: "public",
                        principalTable: "TrxPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPrescriptionPreparation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PreparedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreparationStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PreparationCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PreparationNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPrescriptionPreparation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionPreparation_AspNetUsers_PreparedByUserId",
                        column: x => x.PreparedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionPreparation_TrxPrescription_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalSchema: "public",
                        principalTable: "TrxPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPrescriptionReview",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewVersion = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ReviewedByPharmacistId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HasAdministrativeProblem = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    HasPharmaceuticalProblem = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    HasClinicalProblem = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    HasCompoundFormulaProblem = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RequiresDoctorClarification = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    GeneralNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PrescriptionSignatureSnapshot = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, defaultValue: ""),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPrescriptionReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionReview_AspNetUsers_ReviewedByPharmacistId",
                        column: x => x.ReviewedByPharmacistId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionReview_TrxPrescription_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalSchema: "public",
                        principalTable: "TrxPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPrescriptionFinalCheckItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionFinalCheckId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriterionCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CriterionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Result = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Finding = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPrescriptionFinalCheckItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionFinalCheckItem_TrxPrescriptionFinalCheck_Pre~",
                        column: x => x.PrescriptionFinalCheckId,
                        principalSchema: "public",
                        principalTable: "TrxPrescriptionFinalCheck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPrescriptionPreparationItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionPreparationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrescriptionCompoundItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    TheoreticalQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    ActualQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    WasteQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    MeasurementId = table.Column<Guid>(type: "uuid", nullable: true),
                    MeasurementNameSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BatchNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPrescriptionPreparationItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionPreparationItem_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionPreparationItem_MstMeasurement_MeasurementId",
                        column: x => x.MeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionPreparationItem_TrxPrescriptionCompoundItem_~",
                        column: x => x.PrescriptionCompoundItemId,
                        principalSchema: "public",
                        principalTable: "TrxPrescriptionCompoundItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionPreparationItem_TrxPrescriptionItem_Prescrip~",
                        column: x => x.PrescriptionItemId,
                        principalSchema: "public",
                        principalTable: "TrxPrescriptionItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionPreparationItem_TrxPrescriptionPreparation_P~",
                        column: x => x.PrescriptionPreparationId,
                        principalSchema: "public",
                        principalTable: "TrxPrescriptionPreparation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPrescriptionReviewItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriterionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    CriterionCodeSnapshot = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CriterionNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Result = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Severity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Finding = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Recommendation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsSystemDetected = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SystemRuleCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PrescriptionItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrescriptionCompoundId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrescriptionCompoundItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPrescriptionReviewItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionReviewItem_AspNetUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionReviewItem_MstPrescriptionReviewCriterion_Cr~",
                        column: x => x.CriterionId,
                        principalSchema: "public",
                        principalTable: "MstPrescriptionReviewCriterion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionReviewItem_TrxPrescriptionCompound_Prescript~",
                        column: x => x.PrescriptionCompoundId,
                        principalSchema: "public",
                        principalTable: "TrxPrescriptionCompound",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionReviewItem_TrxPrescriptionCompoundItem_Presc~",
                        column: x => x.PrescriptionCompoundItemId,
                        principalSchema: "public",
                        principalTable: "TrxPrescriptionCompoundItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionReviewItem_TrxPrescriptionItem_PrescriptionI~",
                        column: x => x.PrescriptionItemId,
                        principalSchema: "public",
                        principalTable: "TrxPrescriptionItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionReviewItem_TrxPrescriptionReview_Prescriptio~",
                        column: x => x.PrescriptionReviewId,
                        principalSchema: "public",
                        principalTable: "TrxPrescriptionReview",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPrescriptionClarification",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionReviewItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrescriptionItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrescriptionCompoundId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrescriptionCompoundItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProblemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProblemDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PharmacistRecommendation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Severity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RequestedByPharmacistId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RespondedByDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DoctorResponse = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPrescriptionClarification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionClarification_AspNetUsers_ClosedByUserId",
                        column: x => x.ClosedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionClarification_AspNetUsers_RequestedByPharmac~",
                        column: x => x.RequestedByPharmacistId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionClarification_AspNetUsers_RespondedByDoctorId",
                        column: x => x.RespondedByDoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionClarification_TrxPrescription_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalSchema: "public",
                        principalTable: "TrxPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionClarification_TrxPrescriptionCompound_Prescr~",
                        column: x => x.PrescriptionCompoundId,
                        principalSchema: "public",
                        principalTable: "TrxPrescriptionCompound",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionClarification_TrxPrescriptionCompoundItem_Pr~",
                        column: x => x.PrescriptionCompoundItemId,
                        principalSchema: "public",
                        principalTable: "TrxPrescriptionCompoundItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionClarification_TrxPrescriptionItem_Prescripti~",
                        column: x => x.PrescriptionItemId,
                        principalSchema: "public",
                        principalTable: "TrxPrescriptionItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionClarification_TrxPrescriptionReview_Prescrip~",
                        column: x => x.PrescriptionReviewId,
                        principalSchema: "public",
                        principalTable: "TrxPrescriptionReview",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPrescriptionClarification_TrxPrescriptionReviewItem_Pres~",
                        column: x => x.PrescriptionReviewItemId,
                        principalSchema: "public",
                        principalTable: "TrxPrescriptionReviewItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstPrescriptionReviewCriterion_Category_IsActive_IsDelete_S~",
                schema: "public",
                table: "MstPrescriptionReviewCriterion",
                columns: new[] { "Category", "IsActive", "IsDelete", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPrescriptionReviewCriterion_CriterionCode",
                schema: "public",
                table: "MstPrescriptionReviewCriterion",
                column: "CriterionCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstPrescriptionReviewCriterion_IsApplicableToRegular_IsAppl~",
                schema: "public",
                table: "MstPrescriptionReviewCriterion",
                columns: new[] { "IsApplicableToRegular", "IsApplicableToCompound", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionClarification_ClosedByUserId",
                schema: "public",
                table: "TrxPrescriptionClarification",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionClarification_PrescriptionCompoundId",
                schema: "public",
                table: "TrxPrescriptionClarification",
                column: "PrescriptionCompoundId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionClarification_PrescriptionCompoundItemId",
                schema: "public",
                table: "TrxPrescriptionClarification",
                column: "PrescriptionCompoundItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionClarification_PrescriptionId_Status_Severity~",
                schema: "public",
                table: "TrxPrescriptionClarification",
                columns: new[] { "PrescriptionId", "Status", "Severity", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionClarification_PrescriptionItemId",
                schema: "public",
                table: "TrxPrescriptionClarification",
                column: "PrescriptionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionClarification_PrescriptionReviewId_Status_Is~",
                schema: "public",
                table: "TrxPrescriptionClarification",
                columns: new[] { "PrescriptionReviewId", "Status", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionClarification_PrescriptionReviewItemId",
                schema: "public",
                table: "TrxPrescriptionClarification",
                column: "PrescriptionReviewItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionClarification_RequestedAt",
                schema: "public",
                table: "TrxPrescriptionClarification",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionClarification_RequestedByPharmacistId",
                schema: "public",
                table: "TrxPrescriptionClarification",
                column: "RequestedByPharmacistId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionClarification_RespondedByDoctorId",
                schema: "public",
                table: "TrxPrescriptionClarification",
                column: "RespondedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionDrugSubstitution_ApprovedByDoctorId",
                schema: "public",
                table: "TrxPrescriptionDrugSubstitution",
                column: "ApprovedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionDrugSubstitution_OriginalDrugId",
                schema: "public",
                table: "TrxPrescriptionDrugSubstitution",
                column: "OriginalDrugId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionDrugSubstitution_PrescriptionCompoundItemId",
                schema: "public",
                table: "TrxPrescriptionDrugSubstitution",
                column: "PrescriptionCompoundItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionDrugSubstitution_PrescriptionId_ApprovalStat~",
                schema: "public",
                table: "TrxPrescriptionDrugSubstitution",
                columns: new[] { "PrescriptionId", "ApprovalStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionDrugSubstitution_PrescriptionItemId",
                schema: "public",
                table: "TrxPrescriptionDrugSubstitution",
                column: "PrescriptionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionDrugSubstitution_ProposedAt",
                schema: "public",
                table: "TrxPrescriptionDrugSubstitution",
                column: "ProposedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionDrugSubstitution_ProposedByPharmacistId",
                schema: "public",
                table: "TrxPrescriptionDrugSubstitution",
                column: "ProposedByPharmacistId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionDrugSubstitution_SubstituteDrugId",
                schema: "public",
                table: "TrxPrescriptionDrugSubstitution",
                column: "SubstituteDrugId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionFinalCheck_CheckedByUserId_Status_IsDelete",
                schema: "public",
                table: "TrxPrescriptionFinalCheck",
                columns: new[] { "CheckedByUserId", "Status", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionFinalCheck_PrescriptionId_Status_IsActive_Is~",
                schema: "public",
                table: "TrxPrescriptionFinalCheck",
                columns: new[] { "PrescriptionId", "Status", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionFinalCheckItem_PrescriptionFinalCheckId_Crit~",
                schema: "public",
                table: "TrxPrescriptionFinalCheckItem",
                columns: new[] { "PrescriptionFinalCheckId", "CriterionCode", "IsDelete" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionFinalCheckItem_PrescriptionFinalCheckId_Resu~",
                schema: "public",
                table: "TrxPrescriptionFinalCheckItem",
                columns: new[] { "PrescriptionFinalCheckId", "Result", "SortOrder", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionPreparation_PreparedByUserId_Status_IsDelete",
                schema: "public",
                table: "TrxPrescriptionPreparation",
                columns: new[] { "PreparedByUserId", "Status", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionPreparation_PrescriptionId_Status_IsActive_I~",
                schema: "public",
                table: "TrxPrescriptionPreparation",
                columns: new[] { "PrescriptionId", "Status", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionPreparationItem_BatchNumber",
                schema: "public",
                table: "TrxPrescriptionPreparationItem",
                column: "BatchNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionPreparationItem_DrugId",
                schema: "public",
                table: "TrxPrescriptionPreparationItem",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionPreparationItem_ExpiryDate",
                schema: "public",
                table: "TrxPrescriptionPreparationItem",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionPreparationItem_MeasurementId",
                schema: "public",
                table: "TrxPrescriptionPreparationItem",
                column: "MeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionPreparationItem_PrescriptionCompoundItemId",
                schema: "public",
                table: "TrxPrescriptionPreparationItem",
                column: "PrescriptionCompoundItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionPreparationItem_PrescriptionItemId",
                schema: "public",
                table: "TrxPrescriptionPreparationItem",
                column: "PrescriptionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionPreparationItem_PrescriptionPreparationId_So~",
                schema: "public",
                table: "TrxPrescriptionPreparationItem",
                columns: new[] { "PrescriptionPreparationId", "SortOrder", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionReview_PrescriptionId_ReviewVersion",
                schema: "public",
                table: "TrxPrescriptionReview",
                columns: new[] { "PrescriptionId", "ReviewVersion" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionReview_PrescriptionId_Status_IsActive_IsDele~",
                schema: "public",
                table: "TrxPrescriptionReview",
                columns: new[] { "PrescriptionId", "Status", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionReview_ReviewedByPharmacistId_Status_IsDelete",
                schema: "public",
                table: "TrxPrescriptionReview",
                columns: new[] { "ReviewedByPharmacistId", "Status", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionReviewItem_CriterionId",
                schema: "public",
                table: "TrxPrescriptionReviewItem",
                column: "CriterionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionReviewItem_PrescriptionCompoundId",
                schema: "public",
                table: "TrxPrescriptionReviewItem",
                column: "PrescriptionCompoundId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionReviewItem_PrescriptionCompoundItemId",
                schema: "public",
                table: "TrxPrescriptionReviewItem",
                column: "PrescriptionCompoundItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionReviewItem_PrescriptionItemId",
                schema: "public",
                table: "TrxPrescriptionReviewItem",
                column: "PrescriptionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionReviewItem_PrescriptionReviewId_Category_Res~",
                schema: "public",
                table: "TrxPrescriptionReviewItem",
                columns: new[] { "PrescriptionReviewId", "Category", "Result", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionReviewItem_PrescriptionReviewId_CriterionCod~",
                schema: "public",
                table: "TrxPrescriptionReviewItem",
                columns: new[] { "PrescriptionReviewId", "CriterionCodeSnapshot", "PrescriptionItemId", "PrescriptionCompoundId", "PrescriptionCompoundItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescriptionReviewItem_ReviewedByUserId",
                schema: "public",
                table: "TrxPrescriptionReviewItem",
                column: "ReviewedByUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrxPrescriptionClarification",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPrescriptionDrugSubstitution",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPrescriptionFinalCheckItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPrescriptionPreparationItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPrescriptionReviewItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPrescriptionFinalCheck",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPrescriptionPreparation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPrescriptionReviewCriterion",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPrescriptionReview",
                schema: "public");
        }
    }
}
