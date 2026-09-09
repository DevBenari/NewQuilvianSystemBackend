using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddNutritionManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GzDietType",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DietTypeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DietTypeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsSpecialDiet = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_GzDietType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GzFoodForm",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FoodFormCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FoodFormName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_GzFoodForm", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GzMealSchedule",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MealScheduleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MealScheduleName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ServingTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    IsMainMeal = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_GzMealSchedule", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GzNutritionOrder",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterDoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedWorkforceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ReasonForReferral = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ScreeningRiskStatus = table.Column<int>(type: "integer", nullable: true),
                    ScreeningScore = table.Column<int>(type: "integer", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosingNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_GzNutritionOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GzNutritionOrder_MstDoctor_RequesterDoctorId",
                        column: x => x.RequesterDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GzNutritionOrder_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GzNutritionOrder_MstWorkforceProfile_AssignedWorkforceId",
                        column: x => x.AssignedWorkforceId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GzNutritionOrder_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GzProductionBatch",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ServiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    MealScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalPortion = table.Column<int>(type: "integer", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReadyAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_GzProductionBatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GzProductionBatch_GzMealSchedule_MealScheduleId",
                        column: x => x.MealScheduleId,
                        principalSchema: "public",
                        principalTable: "GzMealSchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GzNutritionCareRecord",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NutritionOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitSequence = table.Column<int>(type: "integer", nullable: false),
                    VisitAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecordedByWorkforceId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordType = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    Height = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    Bmi = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    AssessmentNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    NutritionDiagnosisId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiagnosisNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    InterventionNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DietPrescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EnergyRequirementKcal = table.Column<int>(type: "integer", nullable: true),
                    IntakeRecallNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IntakePercent = table.Column<int>(type: "integer", nullable: true),
                    EvaluationNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ProgressNoteId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_GzNutritionCareRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GzNutritionCareRecord_GzNutritionOrder_NutritionOrderId",
                        column: x => x.NutritionOrderId,
                        principalSchema: "public",
                        principalTable: "GzNutritionOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GzNutritionCareRecord_MstDiagnosis_NutritionDiagnosisId",
                        column: x => x.NutritionDiagnosisId,
                        principalSchema: "public",
                        principalTable: "MstDiagnosis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GzNutritionCareRecord_MstWorkforceProfile_RecordedByWorkfor~",
                        column: x => x.RecordedByWorkforceId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GzNutritionCareRecord_TrxPatientIntegratedProgressNote_Prog~",
                        column: x => x.ProgressNoteId,
                        principalSchema: "public",
                        principalTable: "TrxPatientIntegratedProgressNote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GzNutritionOrderHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NutritionOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: true),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_GzNutritionOrderHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GzNutritionOrderHistory_GzNutritionOrder_NutritionOrderId",
                        column: x => x.NutritionOrderId,
                        principalSchema: "public",
                        principalTable: "GzNutritionOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GzPatientDiet",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NutritionOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    DietTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    FoodFormId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnergyRequirementKcal = table.Column<int>(type: "integer", nullable: true),
                    Instruction = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ChangeReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PrescribedByWorkforceId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_GzPatientDiet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GzPatientDiet_GzDietType_DietTypeId",
                        column: x => x.DietTypeId,
                        principalSchema: "public",
                        principalTable: "GzDietType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GzPatientDiet_GzFoodForm_FoodFormId",
                        column: x => x.FoodFormId,
                        principalSchema: "public",
                        principalTable: "GzFoodForm",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GzPatientDiet_GzNutritionOrder_NutritionOrderId",
                        column: x => x.NutritionOrderId,
                        principalSchema: "public",
                        principalTable: "GzNutritionOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GzPatientDiet_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GzPatientDiet_MstWorkforceProfile_PrescribedByWorkforceId",
                        column: x => x.PrescribedByWorkforceId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GzPatientDiet_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GzProductionBatchDetail",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductionBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientDietId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MedicalRecordNumberSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RoomNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BedNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DoctorNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DietTypeNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FoodFormNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EnergyRequirementKcalSnapshot = table.Column<int>(type: "integer", nullable: true),
                    InstructionSnapshot = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Portion = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_GzProductionBatchDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GzProductionBatchDetail_GzPatientDiet_PatientDietId",
                        column: x => x.PatientDietId,
                        principalSchema: "public",
                        principalTable: "GzPatientDiet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GzProductionBatchDetail_GzProductionBatch_ProductionBatchId",
                        column: x => x.ProductionBatchId,
                        principalSchema: "public",
                        principalTable: "GzProductionBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GzProductionBatchDetail_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GzProductionBatchDetail_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GzMealDelivery",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductionBatchDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredByWorkforceId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeftoverPercent = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_GzMealDelivery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GzMealDelivery_GzProductionBatchDetail_ProductionBatchDetai~",
                        column: x => x.ProductionBatchDetailId,
                        principalSchema: "public",
                        principalTable: "GzProductionBatchDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GzMealDelivery_MstWorkforceProfile_DeliveredByWorkforceId",
                        column: x => x.DeliveredByWorkforceId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GzDietType_DietTypeCode",
                schema: "public",
                table: "GzDietType",
                column: "DietTypeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GzFoodForm_FoodFormCode",
                schema: "public",
                table: "GzFoodForm",
                column: "FoodFormCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GzMealDelivery_DeliveredByWorkforceId",
                schema: "public",
                table: "GzMealDelivery",
                column: "DeliveredByWorkforceId");

            migrationBuilder.CreateIndex(
                name: "IX_GzMealDelivery_ProductionBatchDetailId",
                schema: "public",
                table: "GzMealDelivery",
                column: "ProductionBatchDetailId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_GzMealSchedule_MealScheduleCode",
                schema: "public",
                table: "GzMealSchedule",
                column: "MealScheduleCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GzNutritionCareRecord_NutritionDiagnosisId",
                schema: "public",
                table: "GzNutritionCareRecord",
                column: "NutritionDiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_GzNutritionCareRecord_NutritionOrderId_VisitSequence",
                schema: "public",
                table: "GzNutritionCareRecord",
                columns: new[] { "NutritionOrderId", "VisitSequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GzNutritionCareRecord_ProgressNoteId",
                schema: "public",
                table: "GzNutritionCareRecord",
                column: "ProgressNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_GzNutritionCareRecord_RecordedByWorkforceId",
                schema: "public",
                table: "GzNutritionCareRecord",
                column: "RecordedByWorkforceId");

            migrationBuilder.CreateIndex(
                name: "IX_GzNutritionCareRecord_VisitAt",
                schema: "public",
                table: "GzNutritionCareRecord",
                column: "VisitAt");

            migrationBuilder.CreateIndex(
                name: "IX_GzNutritionOrder_AssignedWorkforceId",
                schema: "public",
                table: "GzNutritionOrder",
                column: "AssignedWorkforceId");

            migrationBuilder.CreateIndex(
                name: "IX_GzNutritionOrder_EncounterId",
                schema: "public",
                table: "GzNutritionOrder",
                column: "EncounterId",
                unique: true,
                filter: "\"Status\" IN (1, 2) AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_GzNutritionOrder_OrderNumber",
                schema: "public",
                table: "GzNutritionOrder",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GzNutritionOrder_PatientId_RequestedAt",
                schema: "public",
                table: "GzNutritionOrder",
                columns: new[] { "PatientId", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GzNutritionOrder_RequesterDoctorId",
                schema: "public",
                table: "GzNutritionOrder",
                column: "RequesterDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_GzNutritionOrder_Status",
                schema: "public",
                table: "GzNutritionOrder",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_GzNutritionOrderHistory_Action_CorrelationId",
                schema: "public",
                table: "GzNutritionOrderHistory",
                columns: new[] { "Action", "CorrelationId" },
                unique: true,
                filter: "\"CorrelationId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_GzNutritionOrderHistory_NutritionOrderId_OccurredAt",
                schema: "public",
                table: "GzNutritionOrderHistory",
                columns: new[] { "NutritionOrderId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GzPatientDiet_DietTypeId",
                schema: "public",
                table: "GzPatientDiet",
                column: "DietTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_GzPatientDiet_EncounterId",
                schema: "public",
                table: "GzPatientDiet",
                column: "EncounterId",
                unique: true,
                filter: "\"Status\" = 1 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_GzPatientDiet_EncounterId_StartAt",
                schema: "public",
                table: "GzPatientDiet",
                columns: new[] { "EncounterId", "StartAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GzPatientDiet_FoodFormId",
                schema: "public",
                table: "GzPatientDiet",
                column: "FoodFormId");

            migrationBuilder.CreateIndex(
                name: "IX_GzPatientDiet_NutritionOrderId",
                schema: "public",
                table: "GzPatientDiet",
                column: "NutritionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_GzPatientDiet_PatientId",
                schema: "public",
                table: "GzPatientDiet",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_GzPatientDiet_PrescribedByWorkforceId",
                schema: "public",
                table: "GzPatientDiet",
                column: "PrescribedByWorkforceId");

            migrationBuilder.CreateIndex(
                name: "IX_GzProductionBatch_BatchNumber",
                schema: "public",
                table: "GzProductionBatch",
                column: "BatchNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GzProductionBatch_MealScheduleId",
                schema: "public",
                table: "GzProductionBatch",
                column: "MealScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_GzProductionBatch_ServiceDate_MealScheduleId",
                schema: "public",
                table: "GzProductionBatch",
                columns: new[] { "ServiceDate", "MealScheduleId" },
                unique: true,
                filter: "\"Status\" <> 6 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_GzProductionBatch_ServiceDate_Status",
                schema: "public",
                table: "GzProductionBatch",
                columns: new[] { "ServiceDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_GzProductionBatchDetail_EncounterId",
                schema: "public",
                table: "GzProductionBatchDetail",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_GzProductionBatchDetail_PatientDietId",
                schema: "public",
                table: "GzProductionBatchDetail",
                column: "PatientDietId");

            migrationBuilder.CreateIndex(
                name: "IX_GzProductionBatchDetail_PatientId",
                schema: "public",
                table: "GzProductionBatchDetail",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_GzProductionBatchDetail_ProductionBatchId_EncounterId",
                schema: "public",
                table: "GzProductionBatchDetail",
                columns: new[] { "ProductionBatchId", "EncounterId" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GzMealDelivery",
                schema: "public");

            migrationBuilder.DropTable(
                name: "GzNutritionCareRecord",
                schema: "public");

            migrationBuilder.DropTable(
                name: "GzNutritionOrderHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "GzProductionBatchDetail",
                schema: "public");

            migrationBuilder.DropTable(
                name: "GzPatientDiet",
                schema: "public");

            migrationBuilder.DropTable(
                name: "GzProductionBatch",
                schema: "public");

            migrationBuilder.DropTable(
                name: "GzDietType",
                schema: "public");

            migrationBuilder.DropTable(
                name: "GzFoodForm",
                schema: "public");

            migrationBuilder.DropTable(
                name: "GzNutritionOrder",
                schema: "public");

            migrationBuilder.DropTable(
                name: "GzMealSchedule",
                schema: "public");
        }
    }
}
