using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalCheckupManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "McuDailySchedule",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsOpen = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Quota = table.Column<int>(type: "integer", nullable: false),
                    UsedQuota = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ApprovedOverrideQuota = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    DutyMcuDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_McuDailySchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McuDailySchedule_MstDoctor_DutyMcuDoctorId",
                        column: x => x.DutyMcuDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "McuExaminationType",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExaminationTypeCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ExaminationTypeName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
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
                    table.PrimaryKey("PK_McuExaminationType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "McuFitnessCategory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FitnessCategoryCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FitnessCategoryName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_McuFitnessCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "McuPackage",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PackageName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    GeneralTariffId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_McuPackage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McuPackage_MstTariff_GeneralTariffId",
                        column: x => x.GeneralTariffId,
                        principalSchema: "public",
                        principalTable: "MstTariff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "McuQuotaOverride",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DailyScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdditionalQuota = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_McuQuotaOverride", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McuQuotaOverride_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuQuotaOverride_McuDailySchedule_DailyScheduleId",
                        column: x => x.DailyScheduleId,
                        principalSchema: "public",
                        principalTable: "McuDailySchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "McuExaminationField",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExaminationTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FieldLabel = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FieldType = table.Column<int>(type: "integer", nullable: false),
                    UnitName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    OptionValues = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FieldNumber = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_McuExaminationField", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McuExaminationField_McuExaminationType_ExaminationTypeId",
                        column: x => x.ExaminationTypeId,
                        principalSchema: "public",
                        principalTable: "McuExaminationType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "McuBookingImport",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CompanyGuarantorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExaminationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TotalRows = table.Column<int>(type: "integer", nullable: false),
                    CreatedRows = table.Column<int>(type: "integer", nullable: false),
                    FailedRows = table.Column<int>(type: "integer", nullable: false),
                    FailureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("PK_McuBookingImport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McuBookingImport_McuPackage_PackageId",
                        column: x => x.PackageId,
                        principalSchema: "public",
                        principalTable: "McuPackage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuBookingImport_MstCompanyGuarantor_CompanyGuarantorId",
                        column: x => x.CompanyGuarantorId,
                        principalSchema: "public",
                        principalTable: "MstCompanyGuarantor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "McuPackageCompanyPrice",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyGuarantorId = table.Column<Guid>(type: "uuid", nullable: false),
                    TariffId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveEndDate = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_McuPackageCompanyPrice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McuPackageCompanyPrice_McuPackage_PackageId",
                        column: x => x.PackageId,
                        principalSchema: "public",
                        principalTable: "McuPackage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuPackageCompanyPrice_MstCompanyGuarantor_CompanyGuarantor~",
                        column: x => x.CompanyGuarantorId,
                        principalSchema: "public",
                        principalTable: "MstCompanyGuarantor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuPackageCompanyPrice_MstTariff_TariffId",
                        column: x => x.TariffId,
                        principalSchema: "public",
                        principalTable: "MstTariff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "McuPackageComponent",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentType = table.Column<int>(type: "integer", nullable: false),
                    ProcedureId = table.Column<Guid>(type: "uuid", nullable: true),
                    RadModalityId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExaminationTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    RefundAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    MedicalFeeAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
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
                    table.PrimaryKey("PK_McuPackageComponent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McuPackageComponent_McuExaminationType_ExaminationTypeId",
                        column: x => x.ExaminationTypeId,
                        principalSchema: "public",
                        principalTable: "McuExaminationType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuPackageComponent_McuPackage_PackageId",
                        column: x => x.PackageId,
                        principalSchema: "public",
                        principalTable: "McuPackage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuPackageComponent_MstProcedure_ProcedureId",
                        column: x => x.ProcedureId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuPackageComponent_MstRadModality_RadModalityId",
                        column: x => x.RadModalityId,
                        principalSchema: "public",
                        principalTable: "MstRadModality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "McuOrder",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageCodeSnapshot = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PackageNameSnapshot = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParticipantName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ParticipantNik = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    ParticipantBirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ParticipantPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PatientMatchMethod = table.Column<int>(type: "integer", nullable: true),
                    PatientMatchConfirmedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    SourceEncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecommendingDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyGuarantorId = table.Column<Guid>(type: "uuid", nullable: true),
                    BookingImportId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExaminationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DailyScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderStatus = table.Column<int>(type: "integer", nullable: false),
                    ArrivalConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    CheckInChannel = table.Column<int>(type: "integer", nullable: true),
                    CheckedInAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckedInByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    McuDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    AppliedTariffId = table.Column<Guid>(type: "uuid", nullable: true),
                    PackageInvoiceItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClearanceBasis = table.Column<int>(type: "integer", nullable: true),
                    ExaminationStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NoShowAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_McuOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McuOrder_AspNetUsers_CheckedInByUserId",
                        column: x => x.CheckedInByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrder_AspNetUsers_PatientMatchConfirmedByUserId",
                        column: x => x.PatientMatchConfirmedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrder_BilInvoiceItem_PackageInvoiceItemId",
                        column: x => x.PackageInvoiceItemId,
                        principalSchema: "public",
                        principalTable: "BilInvoiceItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrder_McuBookingImport_BookingImportId",
                        column: x => x.BookingImportId,
                        principalSchema: "public",
                        principalTable: "McuBookingImport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrder_McuDailySchedule_DailyScheduleId",
                        column: x => x.DailyScheduleId,
                        principalSchema: "public",
                        principalTable: "McuDailySchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrder_McuPackage_PackageId",
                        column: x => x.PackageId,
                        principalSchema: "public",
                        principalTable: "McuPackage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrder_MstCompanyGuarantor_CompanyGuarantorId",
                        column: x => x.CompanyGuarantorId,
                        principalSchema: "public",
                        principalTable: "MstCompanyGuarantor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrder_MstDoctor_McuDoctorId",
                        column: x => x.McuDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrder_MstDoctor_RecommendingDoctorId",
                        column: x => x.RecommendingDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrder_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrder_MstTariff_AppliedTariffId",
                        column: x => x.AppliedTariffId,
                        principalSchema: "public",
                        principalTable: "MstTariff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrder_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrder_RegPatientEncounter_SourceEncounterId",
                        column: x => x.SourceEncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "McuPackageComponentRequirement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageComponentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementText = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IsConfirmationRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_McuPackageComponentRequirement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McuPackageComponentRequirement_McuPackageComponent_PackageC~",
                        column: x => x.PackageComponentId,
                        principalSchema: "public",
                        principalTable: "McuPackageComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "McuOrderComponent",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    McuOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageComponentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ComponentType = table.Column<int>(type: "integer", nullable: false),
                    ProcedureId = table.Column<Guid>(type: "uuid", nullable: true),
                    RadModalityId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExaminationTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ComponentNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsPackageCovered = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RefundAmountSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    MedicalFeeAmountSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ComponentStatus = table.Column<int>(type: "integer", nullable: false),
                    ScheduledDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LabOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    RadOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerformingDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CancelledByDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    RefundCaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    MedicalFeeReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AddedByDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_McuOrderComponent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McuOrderComponent_BilRefundCase_RefundCaseId",
                        column: x => x.RefundCaseId,
                        principalSchema: "public",
                        principalTable: "BilRefundCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrderComponent_LabOrder_LabOrderId",
                        column: x => x.LabOrderId,
                        principalSchema: "public",
                        principalTable: "LabOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrderComponent_McuExaminationType_ExaminationTypeId",
                        column: x => x.ExaminationTypeId,
                        principalSchema: "public",
                        principalTable: "McuExaminationType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrderComponent_McuOrder_McuOrderId",
                        column: x => x.McuOrderId,
                        principalSchema: "public",
                        principalTable: "McuOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrderComponent_McuPackageComponent_PackageComponentId",
                        column: x => x.PackageComponentId,
                        principalSchema: "public",
                        principalTable: "McuPackageComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrderComponent_MstDoctor_AddedByDoctorId",
                        column: x => x.AddedByDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrderComponent_MstDoctor_CancelledByDoctorId",
                        column: x => x.CancelledByDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrderComponent_MstDoctor_PerformingDoctorId",
                        column: x => x.PerformingDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrderComponent_MstProcedure_ProcedureId",
                        column: x => x.ProcedureId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrderComponent_MstRadModality_RadModalityId",
                        column: x => x.RadModalityId,
                        principalSchema: "public",
                        principalTable: "MstRadModality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrderComponent_RadOrder_RadOrderId",
                        column: x => x.RadOrderId,
                        principalSchema: "public",
                        principalTable: "RadOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "McuOrderDoctorAssignment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    McuOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_McuOrderDoctorAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McuOrderDoctorAssignment_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrderDoctorAssignment_McuOrder_McuOrderId",
                        column: x => x.McuOrderId,
                        principalSchema: "public",
                        principalTable: "McuOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrderDoctorAssignment_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "McuOrderStatusHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    McuOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: true),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_McuOrderStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McuOrderStatusHistory_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrderStatusHistory_McuOrder_McuOrderId",
                        column: x => x.McuOrderId,
                        principalSchema: "public",
                        principalTable: "McuOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "McuReleaseConsent",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    McuOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyGuarantorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsentGivenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceType = table.Column<int>(type: "integer", nullable: false),
                    EvidenceReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RevokeReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_McuReleaseConsent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McuReleaseConsent_AspNetUsers_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuReleaseConsent_AspNetUsers_RevokedByUserId",
                        column: x => x.RevokedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuReleaseConsent_McuOrder_McuOrderId",
                        column: x => x.McuOrderId,
                        principalSchema: "public",
                        principalTable: "McuOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuReleaseConsent_MstCompanyGuarantor_CompanyGuarantorId",
                        column: x => x.CompanyGuarantorId,
                        principalSchema: "public",
                        principalTable: "MstCompanyGuarantor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "McuResume",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    McuOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    ResumeStatus = table.Column<int>(type: "integer", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    GeneralConditionText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ImportantFindingsText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RecommendationText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    FitnessCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkRecommendationText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AuthorDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReturnReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReopenReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReopenedByDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReopenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_McuResume", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McuResume_AspNetUsers_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuResume_McuFitnessCategory_FitnessCategoryId",
                        column: x => x.FitnessCategoryId,
                        principalSchema: "public",
                        principalTable: "McuFitnessCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuResume_McuOrder_McuOrderId",
                        column: x => x.McuOrderId,
                        principalSchema: "public",
                        principalTable: "McuOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuResume_MstDoctor_ApprovedByDoctorId",
                        column: x => x.ApprovedByDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuResume_MstDoctor_AuthorDoctorId",
                        column: x => x.AuthorDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuResume_MstDoctor_ReopenedByDoctorId",
                        column: x => x.ReopenedByDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "McuExaminationResult",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    McuOrderComponentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConclusionText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FilledByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FilledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_McuExaminationResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McuExaminationResult_AspNetUsers_FilledByUserId",
                        column: x => x.FilledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuExaminationResult_McuOrderComponent_McuOrderComponentId",
                        column: x => x.McuOrderComponentId,
                        principalSchema: "public",
                        principalTable: "McuOrderComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "McuOrderRequirementCheck",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    McuOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    McuOrderComponentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementTextSnapshot = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Outcome = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CheckedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_McuOrderRequirementCheck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McuOrderRequirementCheck_AspNetUsers_CheckedByUserId",
                        column: x => x.CheckedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrderRequirementCheck_McuOrderComponent_McuOrderComponen~",
                        column: x => x.McuOrderComponentId,
                        principalSchema: "public",
                        principalTable: "McuOrderComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuOrderRequirementCheck_McuOrder_McuOrderId",
                        column: x => x.McuOrderId,
                        principalSchema: "public",
                        principalTable: "McuOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "McuResultDelivery",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    McuOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    McuResumeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientType = table.Column<int>(type: "integer", nullable: false),
                    ContentType = table.Column<int>(type: "integer", nullable: false),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    DestinationText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeliveredByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RequiresCorrectionNotice = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CorrectionNoticeRecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_McuResultDelivery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McuResultDelivery_AspNetUsers_DeliveredByUserId",
                        column: x => x.DeliveredByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuResultDelivery_McuOrder_McuOrderId",
                        column: x => x.McuOrderId,
                        principalSchema: "public",
                        principalTable: "McuOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuResultDelivery_McuResume_McuResumeId",
                        column: x => x.McuResumeId,
                        principalSchema: "public",
                        principalTable: "McuResume",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "McuExaminationResultValue",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExaminationResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExaminationFieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValueText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_McuExaminationResultValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McuExaminationResultValue_McuExaminationField_ExaminationFi~",
                        column: x => x.ExaminationFieldId,
                        principalSchema: "public",
                        principalTable: "McuExaminationField",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuExaminationResultValue_McuExaminationResult_ExaminationR~",
                        column: x => x.ExaminationResultId,
                        principalSchema: "public",
                        principalTable: "McuExaminationResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "McuExaminationResultValueRevision",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResultValueId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousValueText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResumeVersionNumber = table.Column<int>(type: "integer", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_McuExaminationResultValueRevision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McuExaminationResultValueRevision_AspNetUsers_ChangedByUser~",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_McuExaminationResultValueRevision_McuExaminationResultValue~",
                        column: x => x.ResultValueId,
                        principalSchema: "public",
                        principalTable: "McuExaminationResultValue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_McuBookingImport_CompanyGuarantorId",
                schema: "public",
                table: "McuBookingImport",
                column: "CompanyGuarantorId");

            migrationBuilder.CreateIndex(
                name: "IX_McuBookingImport_ImportNumber",
                schema: "public",
                table: "McuBookingImport",
                column: "ImportNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_McuBookingImport_PackageId",
                schema: "public",
                table: "McuBookingImport",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_McuDailySchedule_DutyMcuDoctorId",
                schema: "public",
                table: "McuDailySchedule",
                column: "DutyMcuDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_McuDailySchedule_ScheduleDate",
                schema: "public",
                table: "McuDailySchedule",
                column: "ScheduleDate",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_McuExaminationField_Type_FieldCode",
                schema: "public",
                table: "McuExaminationField",
                columns: new[] { "ExaminationTypeId", "FieldCode" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_McuExaminationField_Type_FieldNumber",
                schema: "public",
                table: "McuExaminationField",
                columns: new[] { "ExaminationTypeId", "FieldNumber" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_McuExaminationResult_Component",
                schema: "public",
                table: "McuExaminationResult",
                column: "McuOrderComponentId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_McuExaminationResult_FilledByUserId",
                schema: "public",
                table: "McuExaminationResult",
                column: "FilledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_McuExaminationResultValue_ExaminationFieldId",
                schema: "public",
                table: "McuExaminationResultValue",
                column: "ExaminationFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_McuExaminationResultValue_Result_Field",
                schema: "public",
                table: "McuExaminationResultValue",
                columns: new[] { "ExaminationResultId", "ExaminationFieldId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_McuExaminationResultValueRevision_ChangedByUserId",
                schema: "public",
                table: "McuExaminationResultValueRevision",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_McuExaminationResultValueRevision_ResultValueId",
                schema: "public",
                table: "McuExaminationResultValueRevision",
                column: "ResultValueId");

            migrationBuilder.CreateIndex(
                name: "IX_McuExaminationType_ExaminationTypeCode",
                schema: "public",
                table: "McuExaminationType",
                column: "ExaminationTypeCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_McuFitnessCategory_Code",
                schema: "public",
                table: "McuFitnessCategory",
                column: "FitnessCategoryCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrder_AppliedTariffId",
                schema: "public",
                table: "McuOrder",
                column: "AppliedTariffId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrder_BookingImportId",
                schema: "public",
                table: "McuOrder",
                column: "BookingImportId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrder_CheckedInByUserId",
                schema: "public",
                table: "McuOrder",
                column: "CheckedInByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrder_CompanyGuarantorId",
                schema: "public",
                table: "McuOrder",
                column: "CompanyGuarantorId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrder_DailyScheduleId",
                schema: "public",
                table: "McuOrder",
                column: "DailyScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrder_EncounterId",
                schema: "public",
                table: "McuOrder",
                column: "EncounterId",
                unique: true,
                filter: "\"EncounterId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrder_ExaminationDate_OrderStatus",
                schema: "public",
                table: "McuOrder",
                columns: new[] { "ExaminationDate", "OrderStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_McuOrder_McuDoctorId",
                schema: "public",
                table: "McuOrder",
                column: "McuDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrder_OrderNumber",
                schema: "public",
                table: "McuOrder",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_McuOrder_PackageId",
                schema: "public",
                table: "McuOrder",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrder_PackageInvoiceItemId",
                schema: "public",
                table: "McuOrder",
                column: "PackageInvoiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrder_ParticipantNik",
                schema: "public",
                table: "McuOrder",
                column: "ParticipantNik");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrder_PatientId",
                schema: "public",
                table: "McuOrder",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrder_PatientMatchConfirmedByUserId",
                schema: "public",
                table: "McuOrder",
                column: "PatientMatchConfirmedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrder_RecommendingDoctorId",
                schema: "public",
                table: "McuOrder",
                column: "RecommendingDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrder_SourceEncounterId",
                schema: "public",
                table: "McuOrder",
                column: "SourceEncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderComponent_AddedByDoctorId",
                schema: "public",
                table: "McuOrderComponent",
                column: "AddedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderComponent_CancelledByDoctorId",
                schema: "public",
                table: "McuOrderComponent",
                column: "CancelledByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderComponent_ComponentStatus",
                schema: "public",
                table: "McuOrderComponent",
                column: "ComponentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderComponent_ExaminationTypeId",
                schema: "public",
                table: "McuOrderComponent",
                column: "ExaminationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderComponent_LabOrderId",
                schema: "public",
                table: "McuOrderComponent",
                column: "LabOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderComponent_MedicalFeeReleasedAt",
                schema: "public",
                table: "McuOrderComponent",
                column: "MedicalFeeReleasedAt");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderComponent_Order_ExaminationType",
                schema: "public",
                table: "McuOrderComponent",
                columns: new[] { "McuOrderId", "ExaminationTypeId" },
                unique: true,
                filter: "\"ExaminationTypeId\" IS NOT NULL AND \"ComponentStatus\" <> 6 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderComponent_Order_Procedure",
                schema: "public",
                table: "McuOrderComponent",
                columns: new[] { "McuOrderId", "ProcedureId" },
                unique: true,
                filter: "\"ProcedureId\" IS NOT NULL AND \"ComponentStatus\" <> 6 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderComponent_PackageComponentId",
                schema: "public",
                table: "McuOrderComponent",
                column: "PackageComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderComponent_PerformingDoctorId",
                schema: "public",
                table: "McuOrderComponent",
                column: "PerformingDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderComponent_ProcedureId",
                schema: "public",
                table: "McuOrderComponent",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderComponent_RadModalityId",
                schema: "public",
                table: "McuOrderComponent",
                column: "RadModalityId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderComponent_RadOrderId",
                schema: "public",
                table: "McuOrderComponent",
                column: "RadOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderComponent_RefundCaseId",
                schema: "public",
                table: "McuOrderComponent",
                column: "RefundCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderDoctorAssignment_Active",
                schema: "public",
                table: "McuOrderDoctorAssignment",
                column: "McuOrderId",
                unique: true,
                filter: "\"EndedAt\" IS NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderDoctorAssignment_AssignedByUserId",
                schema: "public",
                table: "McuOrderDoctorAssignment",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderDoctorAssignment_DoctorId",
                schema: "public",
                table: "McuOrderDoctorAssignment",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderRequirementCheck_CheckedByUserId",
                schema: "public",
                table: "McuOrderRequirementCheck",
                column: "CheckedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderRequirementCheck_McuOrderComponentId",
                schema: "public",
                table: "McuOrderRequirementCheck",
                column: "McuOrderComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderRequirementCheck_McuOrderId",
                schema: "public",
                table: "McuOrderRequirementCheck",
                column: "McuOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderStatusHistory_ChangedByUserId",
                schema: "public",
                table: "McuOrderStatusHistory",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_McuOrderStatusHistory_Order_ChangedAt",
                schema: "public",
                table: "McuOrderStatusHistory",
                columns: new[] { "McuOrderId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_McuPackage_GeneralTariffId",
                schema: "public",
                table: "McuPackage",
                column: "GeneralTariffId");

            migrationBuilder.CreateIndex(
                name: "IX_McuPackage_PackageCode",
                schema: "public",
                table: "McuPackage",
                column: "PackageCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_McuPackageCompanyPrice_CompanyGuarantorId",
                schema: "public",
                table: "McuPackageCompanyPrice",
                column: "CompanyGuarantorId");

            migrationBuilder.CreateIndex(
                name: "IX_McuPackageCompanyPrice_PackageId_CompanyGuarantorId",
                schema: "public",
                table: "McuPackageCompanyPrice",
                columns: new[] { "PackageId", "CompanyGuarantorId" });

            migrationBuilder.CreateIndex(
                name: "IX_McuPackageCompanyPrice_TariffId",
                schema: "public",
                table: "McuPackageCompanyPrice",
                column: "TariffId");

            migrationBuilder.CreateIndex(
                name: "IX_McuPackageComponent_ExaminationTypeId",
                schema: "public",
                table: "McuPackageComponent",
                column: "ExaminationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_McuPackageComponent_PackageId_ExaminationTypeId",
                schema: "public",
                table: "McuPackageComponent",
                columns: new[] { "PackageId", "ExaminationTypeId" },
                unique: true,
                filter: "\"ExaminationTypeId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_McuPackageComponent_PackageId_ProcedureId",
                schema: "public",
                table: "McuPackageComponent",
                columns: new[] { "PackageId", "ProcedureId" },
                unique: true,
                filter: "\"ProcedureId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_McuPackageComponent_ProcedureId",
                schema: "public",
                table: "McuPackageComponent",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_McuPackageComponent_RadModalityId",
                schema: "public",
                table: "McuPackageComponent",
                column: "RadModalityId");

            migrationBuilder.CreateIndex(
                name: "IX_McuPackageComponentRequirement_PackageComponentId",
                schema: "public",
                table: "McuPackageComponentRequirement",
                column: "PackageComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_McuQuotaOverride_ApprovedByUserId",
                schema: "public",
                table: "McuQuotaOverride",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_McuQuotaOverride_DailyScheduleId",
                schema: "public",
                table: "McuQuotaOverride",
                column: "DailyScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_McuReleaseConsent_Active",
                schema: "public",
                table: "McuReleaseConsent",
                column: "McuOrderId",
                unique: true,
                filter: "\"RevokedAt\" IS NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_McuReleaseConsent_CompanyGuarantorId",
                schema: "public",
                table: "McuReleaseConsent",
                column: "CompanyGuarantorId");

            migrationBuilder.CreateIndex(
                name: "IX_McuReleaseConsent_RecordedByUserId",
                schema: "public",
                table: "McuReleaseConsent",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_McuReleaseConsent_RevokedByUserId",
                schema: "public",
                table: "McuReleaseConsent",
                column: "RevokedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_McuResultDelivery_DeliveredByUserId",
                schema: "public",
                table: "McuResultDelivery",
                column: "DeliveredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_McuResultDelivery_McuResumeId",
                schema: "public",
                table: "McuResultDelivery",
                column: "McuResumeId");

            migrationBuilder.CreateIndex(
                name: "IX_McuResultDelivery_Order",
                schema: "public",
                table: "McuResultDelivery",
                column: "McuOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_McuResultDelivery_RequiresCorrectionNotice",
                schema: "public",
                table: "McuResultDelivery",
                column: "RequiresCorrectionNotice");

            migrationBuilder.CreateIndex(
                name: "IX_McuResume_ApprovedByDoctorId",
                schema: "public",
                table: "McuResume",
                column: "ApprovedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_McuResume_AuthorDoctorId",
                schema: "public",
                table: "McuResume",
                column: "AuthorDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_McuResume_FitnessCategoryId",
                schema: "public",
                table: "McuResume",
                column: "FitnessCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_McuResume_Order_Current",
                schema: "public",
                table: "McuResume",
                column: "McuOrderId",
                unique: true,
                filter: "\"IsCurrent\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_McuResume_Order_Version",
                schema: "public",
                table: "McuResume",
                columns: new[] { "McuOrderId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_McuResume_ReopenedByDoctorId",
                schema: "public",
                table: "McuResume",
                column: "ReopenedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_McuResume_VerifiedByUserId",
                schema: "public",
                table: "McuResume",
                column: "VerifiedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "McuExaminationResultValueRevision",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McuOrderDoctorAssignment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McuOrderRequirementCheck",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McuOrderStatusHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McuPackageCompanyPrice",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McuPackageComponentRequirement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McuQuotaOverride",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McuReleaseConsent",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McuResultDelivery",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McuExaminationResultValue",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McuResume",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McuExaminationField",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McuExaminationResult",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McuFitnessCategory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McuOrderComponent",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McuOrder",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McuPackageComponent",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McuBookingImport",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McuDailySchedule",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McuExaminationType",
                schema: "public");

            migrationBuilder.DropTable(
                name: "McuPackage",
                schema: "public");
        }
    }
}
