using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddOpticalManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OptItemProfile",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemCategory = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FrameSize = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FrameMaterial = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LensIndex = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    LensDesign = table.Column<int>(type: "integer", nullable: true),
                    LensCoating = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContactLensBaseCurve = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    ContactLensDiameter = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    ContactLensReplacement = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_OptItemProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptItemProfile_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OptRefractionExam",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExaminerWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExaminerLicenseNumberSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentRevisionNo = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinalizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinalizedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    BillingFactEmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_OptRefractionExam", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptRefractionExam_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OptRefractionExam_MstWorkforceProfile_ExaminerWorkforceProf~",
                        column: x => x.ExaminerWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OptRefractionExam_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OptSetting",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    DownPaymentMinPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    PickupReminderDays = table.Column<int>(type: "integer", nullable: false),
                    NotCollectedCloseDays = table.Column<int>(type: "integer", nullable: false),
                    PrescriptionValidityMonths = table.Column<int>(type: "integer", nullable: false),
                    WarrantyFitDays = table.Column<int>(type: "integer", nullable: false),
                    WarrantyLensDefectMonths = table.Column<int>(type: "integer", nullable: false),
                    WarrantyFrameDefectMonths = table.Column<int>(type: "integer", nullable: false),
                    ContactLensBaseCurveMin = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    ContactLensBaseCurveMax = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    ContactLensDiameterMin = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    ContactLensDiameterMax = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_OptSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptSetting_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OptPrescription",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    RefractionExamId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrescriptionType = table.Column<int>(type: "integer", nullable: false),
                    Origin = table.Column<int>(type: "integer", nullable: false),
                    ExternalAuthorName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ExternalFacilityName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    IssuedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidityMonthsSnapshot = table.Column<int>(type: "integer", nullable: false),
                    ValidUntil = table.Column<DateOnly>(type: "date", nullable: false),
                    CurrentVersionNo = table.Column<int>(type: "integer", nullable: false),
                    LockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockedByOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_OptPrescription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptPrescription_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OptPrescription_OptRefractionExam_RefractionExamId",
                        column: x => x.RefractionExamId,
                        principalSchema: "public",
                        principalTable: "OptRefractionExam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OptPrescription_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OptRefractionExamRevision",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RefractionExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisionNo = table.Column<int>(type: "integer", nullable: false),
                    VisusDistanceUncorrectedOd = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    VisusDistanceUncorrectedOs = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    VisusDistanceCorrectedOd = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    VisusDistanceCorrectedOs = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    VisusNearUncorrectedOd = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    VisusNearUncorrectedOs = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    VisusNearCorrectedOd = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    VisusNearCorrectedOs = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    AutorefOdSph = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    AutorefOdCyl = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    AutorefOdAxis = table.Column<int>(type: "integer", nullable: true),
                    AutorefOsSph = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    AutorefOsCyl = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    AutorefOsAxis = table.Column<int>(type: "integer", nullable: true),
                    SubjectiveOdSph = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    SubjectiveOdCyl = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    SubjectiveOdAxis = table.Column<int>(type: "integer", nullable: true),
                    SubjectiveOdAdd = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    SubjectiveOsSph = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    SubjectiveOsCyl = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    SubjectiveOsAxis = table.Column<int>(type: "integer", nullable: true),
                    SubjectiveOsAdd = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    PdDistance = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    PdNear = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    KeratometryNote = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ClinicalNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RevisedByWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevisionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_OptRefractionExamRevision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptRefractionExamRevision_MstWorkforceProfile_RevisedByWork~",
                        column: x => x.RevisedByWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OptRefractionExamRevision_OptRefractionExam_RefractionExamId",
                        column: x => x.RefractionExamId,
                        principalSchema: "public",
                        principalTable: "OptRefractionExam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OptSettingRevision",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SettingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedFieldsSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ChangedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_OptSettingRevision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptSettingRevision_OptSetting_SettingId",
                        column: x => x.SettingId,
                        principalSchema: "public",
                        principalTable: "OptSetting",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OptPrescriptionVersion",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNo = table.Column<int>(type: "integer", nullable: false),
                    OdSph = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    OdCyl = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    OdAxis = table.Column<int>(type: "integer", nullable: true),
                    OdAdd = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    OsSph = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    OsCyl = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    OsAxis = table.Column<int>(type: "integer", nullable: true),
                    OsAdd = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    PdDistance = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    PdNear = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    OdClPower = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    OdClCyl = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    OdClAxis = table.Column<int>(type: "integer", nullable: true),
                    OdClBaseCurve = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    OdClDiameter = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    OsClPower = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    OsClCyl = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    OsClAxis = table.Column<int>(type: "integer", nullable: true),
                    OsClBaseCurve = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    OsClDiameter = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    ContactLensBrandOrType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ContactLensReplacement = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsTransposed = table.Column<bool>(type: "boolean", nullable: false),
                    OriginalNotationText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RangeOverrideReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AuthorWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorLicenseNumberSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AuthoredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevisionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_OptPrescriptionVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptPrescriptionVersion_MstWorkforceProfile_AuthorWorkforceP~",
                        column: x => x.AuthorWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OptPrescriptionVersion_OptPrescription_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalSchema: "public",
                        principalTable: "OptPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OptOrder",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderKind = table.Column<int>(type: "integer", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrescriptionVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DownPaymentMinPercentSnapshot = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    WorkshopType = table.Column<int>(type: "integer", nullable: true),
                    ExternalLabSupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    EstimatedReadyDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BilledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SentToWorkAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReadyAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HandedOverAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HandedOverBy = table.Column<Guid>(type: "uuid", nullable: true),
                    HandoverIdentityVerified = table.Column<bool>(type: "boolean", nullable: false),
                    HandoverReceiverName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    HandoverReceiverRelationship = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FittingNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WarrantyFitDaysSnapshot = table.Column<int>(type: "integer", nullable: true),
                    WarrantyLensDefectMonthsSnapshot = table.Column<int>(type: "integer", nullable: true),
                    WarrantyFrameDefectMonthsSnapshot = table.Column<int>(type: "integer", nullable: true),
                    NotCollectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsRemake = table.Column<bool>(type: "boolean", nullable: false),
                    ParentOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_OptOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptOrder_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OptOrder_MstSupplier_ExternalLabSupplierId",
                        column: x => x.ExternalLabSupplierId,
                        principalSchema: "public",
                        principalTable: "MstSupplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OptOrder_OptOrder_ParentOrderId",
                        column: x => x.ParentOrderId,
                        principalSchema: "public",
                        principalTable: "OptOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OptOrder_OptPrescriptionVersion_PrescriptionVersionId",
                        column: x => x.PrescriptionVersionId,
                        principalSchema: "public",
                        principalTable: "OptPrescriptionVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OptOrder_OptPrescription_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalSchema: "public",
                        principalTable: "OptPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OptOrder_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OptOrderCancellation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DecidedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_OptOrderCancellation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptOrderCancellation_OptOrder_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "public",
                        principalTable: "OptOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OptOrderItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemCategory = table.Column<int>(type: "integer", nullable: false),
                    LensSupply = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsLensCut = table.Column<bool>(type: "boolean", nullable: true),
                    StockIssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StockReturnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BillingFactVersion = table.Column<int>(type: "integer", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_OptOrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptOrderItem_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OptOrderItem_OptOrder_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "public",
                        principalTable: "OptOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OptOrderStatusHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: true),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_OptOrderStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptOrderStatusHistory_OptOrder_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "public",
                        principalTable: "OptOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OptWarrantyClaim",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComplaintType = table.Column<int>(type: "integer", nullable: false),
                    ComplaintNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsWithinWarranty = table.Column<bool>(type: "boolean", nullable: false),
                    WarrantyDeadlineSnapshot = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExaminedByWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExaminedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExaminationFinding = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReexamRefractionExamId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RemakeOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_OptWarrantyClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptWarrantyClaim_MstWorkforceProfile_ExaminedByWorkforcePro~",
                        column: x => x.ExaminedByWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OptWarrantyClaim_OptOrder_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "public",
                        principalTable: "OptOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OptWarrantyClaim_OptOrder_RemakeOrderId",
                        column: x => x.RemakeOrderId,
                        principalSchema: "public",
                        principalTable: "OptOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OptWarrantyClaim_OptRefractionExam_ReexamRefractionExamId",
                        column: x => x.ReexamRefractionExamId,
                        principalSchema: "public",
                        principalTable: "OptRefractionExam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OptWarrantyClaim_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OptItemProfile_ItemCategory",
                schema: "public",
                table: "OptItemProfile",
                column: "ItemCategory");

            migrationBuilder.CreateIndex(
                name: "UX_OptItemProfile_DrugId",
                schema: "public",
                table: "OptItemProfile",
                column: "DrugId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptOrder_EncounterId_Status",
                schema: "public",
                table: "OptOrder",
                columns: new[] { "EncounterId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OptOrder_ExternalLabSupplierId",
                schema: "public",
                table: "OptOrder",
                column: "ExternalLabSupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_OptOrder_ParentOrderId",
                schema: "public",
                table: "OptOrder",
                column: "ParentOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OptOrder_PatientId",
                schema: "public",
                table: "OptOrder",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_OptOrder_PrescriptionId",
                schema: "public",
                table: "OptOrder",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_OptOrder_PrescriptionVersionId",
                schema: "public",
                table: "OptOrder",
                column: "PrescriptionVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_OptOrder_Status_ReadyAt",
                schema: "public",
                table: "OptOrder",
                columns: new[] { "Status", "ReadyAt" });

            migrationBuilder.CreateIndex(
                name: "UX_OptOrder_OrderNumber",
                schema: "public",
                table: "OptOrder",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_OptOrderCancellation_OrderId_Requested",
                schema: "public",
                table: "OptOrderCancellation",
                column: "OrderId",
                unique: true,
                filter: "\"Status\" = 1 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_OptOrderItem_DrugId",
                schema: "public",
                table: "OptOrderItem",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "UX_OptOrderItem_OrderId_LineNo",
                schema: "public",
                table: "OptOrderItem",
                columns: new[] { "OrderId", "LineNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptOrderStatusHistory_OrderId_OccurredAt",
                schema: "public",
                table: "OptOrderStatusHistory",
                columns: new[] { "OrderId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OptPrescription_EncounterId",
                schema: "public",
                table: "OptPrescription",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_OptPrescription_PatientId_ValidUntil",
                schema: "public",
                table: "OptPrescription",
                columns: new[] { "PatientId", "ValidUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_OptPrescription_RefractionExamId",
                schema: "public",
                table: "OptPrescription",
                column: "RefractionExamId");

            migrationBuilder.CreateIndex(
                name: "UX_OptPrescription_PrescriptionNumber",
                schema: "public",
                table: "OptPrescription",
                column: "PrescriptionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptPrescriptionVersion_AuthorWorkforceProfileId",
                schema: "public",
                table: "OptPrescriptionVersion",
                column: "AuthorWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "UX_OptPrescriptionVersion_PrescriptionId_VersionNo",
                schema: "public",
                table: "OptPrescriptionVersion",
                columns: new[] { "PrescriptionId", "VersionNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptRefractionExam_EncounterId_Status",
                schema: "public",
                table: "OptRefractionExam",
                columns: new[] { "EncounterId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OptRefractionExam_ExaminerWorkforceProfileId",
                schema: "public",
                table: "OptRefractionExam",
                column: "ExaminerWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_OptRefractionExam_PatientId_FinalizedAt",
                schema: "public",
                table: "OptRefractionExam",
                columns: new[] { "PatientId", "FinalizedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_OptRefractionExam_ExamNumber",
                schema: "public",
                table: "OptRefractionExam",
                column: "ExamNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptRefractionExamRevision_RevisedByWorkforceProfileId",
                schema: "public",
                table: "OptRefractionExamRevision",
                column: "RevisedByWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "UX_OptRefractionExamRevision_RefractionExamId_RevisionNo",
                schema: "public",
                table: "OptRefractionExamRevision",
                columns: new[] { "RefractionExamId", "RevisionNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_OptSetting_ServiceUnitId",
                schema: "public",
                table: "OptSetting",
                column: "ServiceUnitId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptSettingRevision_SettingId_ChangedAt",
                schema: "public",
                table: "OptSettingRevision",
                columns: new[] { "SettingId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OptWarrantyClaim_EncounterId",
                schema: "public",
                table: "OptWarrantyClaim",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_OptWarrantyClaim_ExaminedByWorkforceProfileId",
                schema: "public",
                table: "OptWarrantyClaim",
                column: "ExaminedByWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_OptWarrantyClaim_OrderId",
                schema: "public",
                table: "OptWarrantyClaim",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OptWarrantyClaim_ReexamRefractionExamId",
                schema: "public",
                table: "OptWarrantyClaim",
                column: "ReexamRefractionExamId");

            migrationBuilder.CreateIndex(
                name: "UX_OptWarrantyClaim_ClaimNumber",
                schema: "public",
                table: "OptWarrantyClaim",
                column: "ClaimNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_OptWarrantyClaim_RemakeOrderId",
                schema: "public",
                table: "OptWarrantyClaim",
                column: "RemakeOrderId",
                unique: true,
                filter: "\"RemakeOrderId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OptItemProfile",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OptOrderCancellation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OptOrderItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OptOrderStatusHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OptRefractionExamRevision",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OptSettingRevision",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OptWarrantyClaim",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OptSetting",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OptOrder",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OptPrescriptionVersion",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OptPrescription",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OptRefractionExam",
                schema: "public");
        }
    }
}
