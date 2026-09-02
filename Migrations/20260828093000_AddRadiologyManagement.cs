using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260828093000_AddRadiologyManagement")]
    public partial class AddRadiologyManagement : Migration
    {
        // Modul RadiologyManagement — RJ-BIL-BE-004, di bawah RJ-BIL-DEC-014.
        //
        // Migration ini ditulis tangan, dan alasannya perlu dicatat. `dotnet ef migrations add`
        // pada 2026-08-28 menghasilkan 13.549 baris: dari 1.006 operasi pada Up(), hanya 34 yang
        // milik Radiology. Sisanya — 24 RenameTable, 35 DropColumn kolom bayangan `TempId*`,
        // 1 DropTable `TrxEmergencyTransfer`, dan 18 CreateTable milik modul Opr*/Emg* — berasal
        // dari melesetnya ApplicationDbContextModelSnapshot terhadap model sebenarnya. Empat
        // migration sebelumnya ditulis tangan tanpa memperbarui snapshot, sehingga EF mengira
        // pekerjaan modul lain masih tertunda dan menyelipkannya ke sini.
        //
        // Menerapkan berkas hasil generate itu berarti menjatuhkan tabel dan kolom milik tim lain
        // serta membuat tabel dari pekerjaan mereka yang belum selesai. Maka yang dipakai di sini
        // hanya 34 operasi milik Radiology, disalin apa adanya dari keluaran EF.
        //
        // Melesetnya snapshot itu sendiri BUKAN milik task ini dan tidak diperbaiki di sini.
        // Lihat RJ-BIL-DEC-018.

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstRadModality",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModalityCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ModalityName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UsesIonisingRadiation = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsContrast = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_MstRadModality", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstRadSafetyRequirement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequirementName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RequiresNote = table.Column<bool>(type: "boolean", nullable: false),
                    SourceNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_MstRadSafetyRequirement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RadOrder",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModalityId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderStatus = table.Column<int>(type: "integer", nullable: false),
                    StatusBeforeHold = table.Column<int>(type: "integer", nullable: true),
                    ClinicalIndication = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_RadOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RadOrder_MstProcedure_ProcedureId",
                        column: x => x.ProcedureId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadOrder_MstRadModality_ModalityId",
                        column: x => x.ModalityId,
                        principalSchema: "public",
                        principalTable: "MstRadModality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadOrder_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstRadModalitySafetyRule",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModalityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureId = table.Column<Guid>(type: "uuid", nullable: true),
                    SafetyRequirementId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RuleVersion = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_MstRadModalitySafetyRule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstRadModalitySafetyRule_MstProcedure_ProcedureId",
                        column: x => x.ProcedureId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstRadModalitySafetyRule_MstRadModality_ModalityId",
                        column: x => x.ModalityId,
                        principalSchema: "public",
                        principalTable: "MstRadModality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstRadModalitySafetyRule_MstRadSafetyRequirement_SafetyRequ~",
                        column: x => x.SafetyRequirementId,
                        principalSchema: "public",
                        principalTable: "MstRadSafetyRequirement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RadStudy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RadOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModalityId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudySequence = table.Column<int>(type: "integer", nullable: false),
                    StudyNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StudyStatus = table.Column<int>(type: "integer", nullable: false),
                    StatusBeforeHold = table.Column<int>(type: "integer", nullable: true),
                    PatientVerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PatientVerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SafetyClearedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SafetyClearedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SafetyRuleVersionAtClearance = table.Column<int>(type: "integer", nullable: true),
                    AcquisitionStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcquisitionStartedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcquiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsUsable = table.Column<bool>(type: "boolean", nullable: true),
                    QualityDecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QualityDecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    QualityNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AbortCause = table.Column<int>(type: "integer", nullable: true),
                    AbortReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AbortedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PerformedPortionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RepeatOfStudyId = table.Column<Guid>(type: "uuid", nullable: true),
                    RepeatCause = table.Column<int>(type: "integer", nullable: true),
                    RepeatReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AdditionalOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    RepeatAuthorizedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    BillingFactSubmitted = table.Column<bool>(type: "boolean", nullable: false),
                    BillingFactSubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExternalStudyUid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ClosureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_RadStudy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RadStudy_MstProcedure_ProcedureId",
                        column: x => x.ProcedureId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadStudy_MstRadModality_ModalityId",
                        column: x => x.ModalityId,
                        principalSchema: "public",
                        principalTable: "MstRadModality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadStudy_RadOrder_RadOrderId",
                        column: x => x.RadOrderId,
                        principalSchema: "public",
                        principalTable: "RadOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadStudy_RadStudy_RepeatOfStudyId",
                        column: x => x.RepeatOfStudyId,
                        principalSchema: "public",
                        principalTable: "RadStudy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RadAcquisitionConsumption",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RadStudyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemType = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConsumedDespiteFailure = table.Column<bool>(type: "boolean", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_RadAcquisitionConsumption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RadAcquisitionConsumption_RadStudy_RadStudyId",
                        column: x => x.RadStudyId,
                        principalSchema: "public",
                        principalTable: "RadStudy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RadStudySafetyCheck",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RadStudyId = table.Column<Guid>(type: "uuid", nullable: false),
                    SafetyRequirementId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequirementNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsMandatorySnapshot = table.Column<bool>(type: "boolean", nullable: false),
                    RuleVersionSnapshot = table.Column<int>(type: "integer", nullable: false),
                    CheckState = table.Column<int>(type: "integer", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_RadStudySafetyCheck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RadStudySafetyCheck_MstRadSafetyRequirement_SafetyRequireme~",
                        column: x => x.SafetyRequirementId,
                        principalSchema: "public",
                        principalTable: "MstRadSafetyRequirement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadStudySafetyCheck_RadStudy_RadStudyId",
                        column: x => x.RadStudyId,
                        principalSchema: "public",
                        principalTable: "RadStudy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RadTransitionHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RadOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    RadStudyId = table.Column<Guid>(type: "uuid", nullable: true),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Scope = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReasonNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_RadTransitionHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RadTransitionHistory_RadOrder_RadOrderId",
                        column: x => x.RadOrderId,
                        principalSchema: "public",
                        principalTable: "RadOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadTransitionHistory_RadStudy_RadStudyId",
                        column: x => x.RadStudyId,
                        principalSchema: "public",
                        principalTable: "RadStudy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstRadModality_IsActive",
                schema: "public",
                table: "MstRadModality",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MstRadModality_ModalityCode",
                schema: "public",
                table: "MstRadModality",
                column: "ModalityCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstRadModalitySafetyRule_ModalityId_IsActive",
                schema: "public",
                table: "MstRadModalitySafetyRule",
                columns: new[] { "ModalityId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MstRadModalitySafetyRule_ModalityId_ProcedureId_SafetyRequi~",
                schema: "public",
                table: "MstRadModalitySafetyRule",
                columns: new[] { "ModalityId", "ProcedureId", "SafetyRequirementId" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_MstRadModalitySafetyRule_ProcedureId",
                schema: "public",
                table: "MstRadModalitySafetyRule",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_MstRadModalitySafetyRule_SafetyRequirementId",
                schema: "public",
                table: "MstRadModalitySafetyRule",
                column: "SafetyRequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_MstRadSafetyRequirement_Category_SortOrder",
                schema: "public",
                table: "MstRadSafetyRequirement",
                columns: new[] { "Category", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MstRadSafetyRequirement_RequirementCode",
                schema: "public",
                table: "MstRadSafetyRequirement",
                column: "RequirementCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_RadAcquisitionConsumption_RadStudyId_ItemType",
                schema: "public",
                table: "RadAcquisitionConsumption",
                columns: new[] { "RadStudyId", "ItemType" });

            migrationBuilder.CreateIndex(
                name: "IX_RadOrder_EncounterId",
                schema: "public",
                table: "RadOrder",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_RadOrder_ModalityId_OrderStatus",
                schema: "public",
                table: "RadOrder",
                columns: new[] { "ModalityId", "OrderStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_RadOrder_OrderStatus",
                schema: "public",
                table: "RadOrder",
                column: "OrderStatus");

            migrationBuilder.CreateIndex(
                name: "IX_RadOrder_ProcedureId",
                schema: "public",
                table: "RadOrder",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_RadStudy_EncounterId",
                schema: "public",
                table: "RadStudy",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_RadStudy_ModalityId",
                schema: "public",
                table: "RadStudy",
                column: "ModalityId");

            migrationBuilder.CreateIndex(
                name: "IX_RadStudy_ProcedureId",
                schema: "public",
                table: "RadStudy",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_RadStudy_RadOrderId_StudySequence",
                schema: "public",
                table: "RadStudy",
                columns: new[] { "RadOrderId", "StudySequence" });

            migrationBuilder.CreateIndex(
                name: "IX_RadStudy_RepeatOfStudyId",
                schema: "public",
                table: "RadStudy",
                column: "RepeatOfStudyId");

            migrationBuilder.CreateIndex(
                name: "IX_RadStudy_StudyNumber",
                schema: "public",
                table: "RadStudy",
                column: "StudyNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_RadStudy_StudyStatus",
                schema: "public",
                table: "RadStudy",
                column: "StudyStatus");

            migrationBuilder.CreateIndex(
                name: "IX_RadStudySafetyCheck_RadStudyId_CheckState",
                schema: "public",
                table: "RadStudySafetyCheck",
                columns: new[] { "RadStudyId", "CheckState" });

            migrationBuilder.CreateIndex(
                name: "IX_RadStudySafetyCheck_RadStudyId_SafetyRequirementId",
                schema: "public",
                table: "RadStudySafetyCheck",
                columns: new[] { "RadStudyId", "SafetyRequirementId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_RadStudySafetyCheck_SafetyRequirementId",
                schema: "public",
                table: "RadStudySafetyCheck",
                column: "SafetyRequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_RadTransitionHistory_EncounterId",
                schema: "public",
                table: "RadTransitionHistory",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_RadTransitionHistory_RadOrderId_OccurredAt",
                schema: "public",
                table: "RadTransitionHistory",
                columns: new[] { "RadOrderId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RadTransitionHistory_RadStudyId_OccurredAt",
                schema: "public",
                table: "RadTransitionHistory",
                columns: new[] { "RadStudyId", "OccurredAt" });

            SeedRadiologyVocabulary(migrationBuilder);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Urutan terbalik terhadap ketergantungan foreign key.
            migrationBuilder.DropTable(name: "RadTransitionHistory", schema: "public");
            migrationBuilder.DropTable(name: "RadStudySafetyCheck", schema: "public");
            migrationBuilder.DropTable(name: "RadAcquisitionConsumption", schema: "public");
            migrationBuilder.DropTable(name: "RadStudy", schema: "public");
            migrationBuilder.DropTable(name: "RadOrder", schema: "public");
            migrationBuilder.DropTable(name: "MstRadModalitySafetyRule", schema: "public");
            migrationBuilder.DropTable(name: "MstRadSafetyRequirement", schema: "public");
            migrationBuilder.DropTable(name: "MstRadModality", schema: "public");
        }

        /// <summary>
        /// Mengisi kosakata radiologi: modalitas dan katalog butir keselamatan.
        ///
        /// <b>Yang diisi di sini hanya nama-nama.</b> Kode modalitas mengikuti kode DICOM yang
        /// berlaku umum — kosakata teknis internasional, bukan SOP rumah sakit. Katalog butir
        /// keselamatan mengikuti praktik umum rumah sakit di Indonesia sesuai
        /// <c>RJ-BIL-DEC-014</c>, dan setiap barisnya membawa <c>SourceNote</c> yang menyatakan
        /// dirinya belum diverifikasi terhadap SOP rumah sakit ini.
        ///
        /// <b>Yang tidak diisi di sini adalah kebijakannya.</b> Tabel
        /// <c>MstRadModalitySafetyRule</c> — yang menetapkan butir mana wajib untuk modalitas
        /// mana — sengaja dibiarkan kosong. <c>RJ-BIL-GATE-DEC-004</c> menyatakan daftar akhir
        /// gerbang keselamatan mengikuti SOP dan otoritas klinis, "bukan keputusan dokumen ini".
        /// Mengisinya dari migration berarti sebuah program menetapkan kapan pasien boleh
        /// disinari.
        ///
        /// Akibatnya sistem lahir dalam keadaan menolak setiap acquisition sampai admin
        /// menetapkan aturannya. Itu memang yang dikehendaki.
        /// </summary>
        private static void SeedRadiologyVocabulary(MigrationBuilder migrationBuilder)
        {
            const string emptyGuid = "00000000-0000-0000-0000-000000000000";
            const string sumber =
                "Baseline standardisasi umum rumah sakit di Indonesia per RJ-BIL-DEC-014. " +
                "BELUM diverifikasi terhadap SOP rumah sakit ini dan bukan SOP yang disahkan.";

            var modalities = new (string Id, string Code, string Name, bool Ionising, bool Contrast, int Sort)[]
            {
                ("7a1c0e10-0001-4d20-8e01-3c2b0f6a9d01", "CR", "Computed Radiography", true, false, 1),
                ("7a1c0e10-0002-4d20-8e01-3c2b0f6a9d02", "DX", "Digital Radiography", true, false, 2),
                ("7a1c0e10-0003-4d20-8e01-3c2b0f6a9d03", "CT", "Computed Tomography", true, true, 3),
                ("7a1c0e10-0004-4d20-8e01-3c2b0f6a9d04", "MR", "Magnetic Resonance", false, true, 4),
                ("7a1c0e10-0005-4d20-8e01-3c2b0f6a9d05", "US", "Ultrasonography", false, true, 5),
                ("7a1c0e10-0006-4d20-8e01-3c2b0f6a9d06", "MG", "Mammography", true, false, 6),
                ("7a1c0e10-0007-4d20-8e01-3c2b0f6a9d07", "RF", "Radiofluoroscopy", true, true, 7),
                ("7a1c0e10-0008-4d20-8e01-3c2b0f6a9d08", "XA", "X-Ray Angiography", true, true, 8),
                ("7a1c0e10-0009-4d20-8e01-3c2b0f6a9d09", "NM", "Nuclear Medicine", true, false, 9),
            };

            foreach (var m in modalities)
            {
                migrationBuilder.Sql(
                    "INSERT INTO public.\"MstRadModality\" " +
                    "(\"Id\", \"ModalityCode\", \"ModalityName\", \"Description\", " +
                    "\"UsesIonisingRadiation\", \"SupportsContrast\", \"IsActive\", \"SortOrder\", " +
                    "\"CreateDateTime\", \"CreateBy\", \"UpdateBy\", \"DeleteBy\", \"CancelBy\", " +
                    "\"IsCancel\", \"IsDelete\") VALUES (" +
                    $"'{m.Id}', '{m.Code}', '{m.Name.Replace("'", "''")}', NULL, " +
                    $"{(m.Ionising ? "true" : "false")}, {(m.Contrast ? "true" : "false")}, " +
                    $"true, {m.Sort}, NOW(), " +
                    $"'{emptyGuid}', '{emptyGuid}', '{emptyGuid}', '{emptyGuid}', " +
                    "false, false) ON CONFLICT DO NOTHING;");
            }

            var requirements = new (string Id, string Code, string Name, string Category, bool RequiresNote, int Sort)[]
            {
                ("9b2d1f20-0001-4e30-9a02-5d3c1a7b8e01", "PATIENT_IDENTITY", "Verifikasi identitas pasien", "Identity", false, 1),
                ("9b2d1f20-0002-4e30-9a02-5d3c1a7b8e02", "ORDER_PROCEDURE_MATCH", "Kesesuaian pesanan, pemeriksaan, dan modalitas", "Identity", false, 2),
                ("9b2d1f20-0003-4e30-9a02-5d3c1a7b8e03", "INFORMED_CONSENT", "Persetujuan tindakan", "Consent", false, 3),
                ("9b2d1f20-0004-4e30-9a02-5d3c1a7b8e04", "PREGNANCY_SCREENING", "Skrining kehamilan", "Radiation", true, 4),
                ("9b2d1f20-0005-4e30-9a02-5d3c1a7b8e05", "RADIATION_JUSTIFICATION", "Justifikasi paparan radiasi", "Radiation", false, 5),
                ("9b2d1f20-0006-4e30-9a02-5d3c1a7b8e06", "METAL_IMPLANT_SCREENING", "Skrining implan dan benda logam", "Magnetic", true, 6),
                ("9b2d1f20-0007-4e30-9a02-5d3c1a7b8e07", "DEVICE_COMPATIBILITY", "Kompatibilitas alat medis dengan medan magnet", "Magnetic", true, 7),
                ("9b2d1f20-0008-4e30-9a02-5d3c1a7b8e08", "CONTRAST_ALLERGY_HISTORY", "Riwayat alergi media kontras", "Contrast", true, 8),
                ("9b2d1f20-0009-4e30-9a02-5d3c1a7b8e09", "RENAL_FUNCTION_CHECK", "Pemeriksaan fungsi ginjal sebelum kontras", "Contrast", true, 9),
                ("9b2d1f20-0010-4e30-9a02-5d3c1a7b8e10", "SEDATION_FASTING", "Puasa sebelum sedasi", "Sedation", true, 10),
                ("9b2d1f20-0011-4e30-9a02-5d3c1a7b8e11", "SEDATION_AIRWAY", "Penilaian jalan napas sebelum sedasi", "Sedation", true, 11),
                ("9b2d1f20-0012-4e30-9a02-5d3c1a7b8e12", "COAGULATION_PROFILE", "Profil koagulasi sebelum tindakan intervensi", "Interventional", true, 12),
            };

            foreach (var r in requirements)
            {
                migrationBuilder.Sql(
                    "INSERT INTO public.\"MstRadSafetyRequirement\" " +
                    "(\"Id\", \"RequirementCode\", \"RequirementName\", \"Description\", " +
                    "\"Category\", \"RequiresNote\", \"SourceNote\", \"IsActive\", \"SortOrder\", " +
                    "\"CreateDateTime\", \"CreateBy\", \"UpdateBy\", \"DeleteBy\", \"CancelBy\", " +
                    "\"IsCancel\", \"IsDelete\") VALUES (" +
                    $"'{r.Id}', '{r.Code}', '{r.Name.Replace("'", "''")}', NULL, " +
                    $"'{r.Category}', {(r.RequiresNote ? "true" : "false")}, " +
                    $"'{sumber.Replace("'", "''")}', true, {r.Sort}, NOW(), " +
                    $"'{emptyGuid}', '{emptyGuid}', '{emptyGuid}', '{emptyGuid}', " +
                    "false, false) ON CONFLICT DO NOTHING;");
            }

            // MstRadModalitySafetyRule sengaja TIDAK diisi. Lihat ringkasan method ini.
        }
    }
}
