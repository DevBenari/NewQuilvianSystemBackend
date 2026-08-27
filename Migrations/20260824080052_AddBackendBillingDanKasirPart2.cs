using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBackendBillingDanKasirPart2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BilCashierShift",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CashierId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegisterId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpeningCash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SystemCash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PhysicalCash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Variance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OpenedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_BilCashierShift", x => x.Id);
                    table.CheckConstraint("CK_BilCashierShift_OpeningCash", "\"OpeningCash\" >= 0");
                    table.CheckConstraint("CK_BilCashierShift_PhysicalCash", "\"PhysicalCash\" >= 0");
                    table.CheckConstraint("CK_BilCashierShift_Status", "\"Status\" IN ('OPEN','HANDED_OVER','CLOSED','CLOSED_WITH_VARIANCE','REVIEWED','REOPENED')");
                    table.CheckConstraint("CK_BilCashierShift_SystemCash", "\"SystemCash\" >= 0");
                    table.ForeignKey(
                        name: "FK_BilCashierShift_AspNetUsers_CashierId",
                        column: x => x.CashierId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilDepositAccount",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AvailableBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_BilDepositAccount", x => x.Id);
                    table.CheckConstraint("CK_BilDepositAccount_AvailableBalance", "\"AvailableBalance\" >= 0");
                    table.CheckConstraint("CK_BilDepositAccount_Status", "\"Status\" IN ('ACTIVE','CLOSED')");
                });

            migrationBuilder.CreateTable(
                name: "BilDiscountApplication",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiscountPolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiscountType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RequestedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ApprovalStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
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
                    table.PrimaryKey("PK_BilDiscountApplication", x => x.Id);
                    table.CheckConstraint("CK_BilDiscountApplication_Amounts", "\"RequestedAmount\" >= 0 AND \"Amount\" >= 0");
                    table.CheckConstraint("CK_BilDiscountApplication_Approval", "(\"ApprovalStatus\" = 'APPROVED' AND (\"DiscountType\" <> 'DOCTOR' OR \"ApprovedBy\" IS NOT NULL)) OR \"ApprovalStatus\" IN ('PENDING_DOCTOR','PENDING_FINANCE')");
                    table.CheckConstraint("CK_BilDiscountApplication_Status", "\"ApprovalStatus\" IN ('APPROVED','PENDING_DOCTOR','PENDING_FINANCE')");
                    table.CheckConstraint("CK_BilDiscountApplication_Target", "(\"DiscountType\" = 'PROMO_TOTAL' AND \"InvoiceItemId\" IS NULL) OR (\"DiscountType\" IN ('PROMO_ITEM','DOCTOR') AND \"InvoiceItemId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_BilDiscountApplication_BilInvoiceItem_InvoiceItemId",
                        column: x => x.InvoiceItemId,
                        principalSchema: "public",
                        principalTable: "BilInvoiceItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilDiscountApplication_BilInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "BilInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilDiscountApplication_MstDiscountPolicy_DiscountPolicyId",
                        column: x => x.DiscountPolicyId,
                        principalSchema: "public",
                        principalTable: "MstDiscountPolicy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilFinalizationRecord",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalculationVersion = table.Column<int>(type: "integer", nullable: false),
                    OutstandingAtFinalization = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsDepartureException = table.Column<bool>(type: "boolean", nullable: false),
                    DepartureReason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DebtorIdentity = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DebtorRelationship = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    PayloadHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinalizedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_BilFinalizationRecord", x => x.Id);
                    table.CheckConstraint("CK_BilFinalizationRecord_DepartureConsistency", "(\"IsDepartureException\" = FALSE AND \"DepartureReason\" IS NULL) OR (\"IsDepartureException\" = TRUE AND \"DepartureReason\" IS NOT NULL AND \"DebtorIdentity\" IS NOT NULL)");
                    table.CheckConstraint("CK_BilFinalizationRecord_DepartureReason", "\"DepartureReason\" IS NULL OR \"DepartureReason\" IN ('DEATH','EMERGENCY_TRANSFER','DAMA')");
                    table.CheckConstraint("CK_BilFinalizationRecord_OutstandingNonNegative", "\"OutstandingAtFinalization\" >= 0");
                    table.ForeignKey(
                        name: "FK_BilFinalizationRecord_BilInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "BilInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilRefundableCredit",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AvailableAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RecognizedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_BilRefundableCredit", x => x.Id);
                    table.CheckConstraint("CK_BilRefundableCredit_Amounts", "\"OriginalAmount\" > 0 AND \"AvailableAmount\" >= 0 AND \"AvailableAmount\" <= \"OriginalAmount\"");
                    table.CheckConstraint("CK_BilRefundableCredit_SourceType", "\"SourceType\" IN ('ALLOCATION_EXCESS','SETTLEMENT')");
                    table.CheckConstraint("CK_BilRefundableCredit_Status", "\"Status\" IN ('AVAILABLE','EXHAUSTED')");
                    table.ForeignKey(
                        name: "FK_BilRefundableCredit_BilInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "BilInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilWriteOffCase",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsFullSettlement = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    PayloadHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PostedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_BilWriteOffCase", x => x.Id);
                    table.CheckConstraint("CK_BilWriteOffCase_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_BilWriteOffCase_Status", "\"Status\" IN ('SUBMITTED','POSTED','REJECTED')");
                    table.ForeignKey(
                        name: "FK_BilWriteOffCase_BilInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "BilInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstInpatientClearanceItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_MstInpatientClearanceItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstInpatientSetting",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BedReservationMinutes = table.Column<int>(type: "integer", nullable: false),
                    DraftEpisodeExpiryHours = table.Column<int>(type: "integer", nullable: false),
                    InitialAssessmentTargetHours = table.Column<int>(type: "integer", nullable: false),
                    ProgressNoteVerificationTargetHours = table.Column<int>(type: "integer", nullable: false),
                    PendingClosureThresholdHours = table.Column<int>(type: "integer", nullable: false),
                    EpisodeNumberPrefix = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_MstInpatientSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BilCashierShiftCommand",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: false),
                    CashierId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegisterId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CommandType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorRole = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Authority = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: true),
                    PayloadHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusBefore = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    StatusAfter = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OpeningCash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SystemCash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PhysicalCash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Variance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SourceType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResponseJson = table.Column<string>(type: "jsonb", nullable: false),
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
                    table.PrimaryKey("PK_BilCashierShiftCommand", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BilCashierShiftCommand_BilCashierShift_ShiftId",
                        column: x => x.ShiftId,
                        principalSchema: "public",
                        principalTable: "BilCashierShift",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilCashierShiftHandover",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceShiftId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivingShiftId = table.Column<Guid>(type: "uuid", nullable: true),
                    OutgoingCashierId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncomingCashierId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    InitiatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_BilCashierShiftHandover", x => x.Id);
                    table.CheckConstraint("CK_BilCashierShiftHandover_Status", "\"Status\" IN ('PENDING','CONFIRMED')");
                    table.ForeignKey(
                        name: "FK_BilCashierShiftHandover_AspNetUsers_IncomingCashierId",
                        column: x => x.IncomingCashierId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilCashierShiftHandover_AspNetUsers_OutgoingCashierId",
                        column: x => x.OutgoingCashierId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilCashierShiftHandover_BilCashierShift_ReceivingShiftId",
                        column: x => x.ReceivingShiftId,
                        principalSchema: "public",
                        principalTable: "BilCashierShift",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilCashierShiftHandover_BilCashierShift_SourceShiftId",
                        column: x => x.SourceShiftId,
                        principalSchema: "public",
                        principalTable: "BilCashierShift",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilCashVarianceReview",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Variance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Resolution = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReopenAuthorizedBy = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_BilCashVarianceReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BilCashVarianceReview_AspNetUsers_ReopenAuthorizedBy",
                        column: x => x.ReopenAuthorizedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilCashVarianceReview_AspNetUsers_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilCashVarianceReview_BilCashierShift_ShiftId",
                        column: x => x.ShiftId,
                        principalSchema: "public",
                        principalTable: "BilCashierShift",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilSettlement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepositAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    Purpose = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RequestedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SuccessfulAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    PayloadHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_BilSettlement", x => x.Id);
                    table.CheckConstraint("CK_BilSettlement_Amounts", "\"RequestedAmount\" > 0 AND \"SuccessfulAmount\" >= 0 AND \"AllocatedAmount\" >= 0 AND \"AllocatedAmount\" <= \"SuccessfulAmount\" AND \"SuccessfulAmount\" <= \"RequestedAmount\"");
                    table.CheckConstraint("CK_BilSettlement_Purpose", "\"Purpose\" IN ('DEPOSIT_TOP_UP','INVOICE_PAYMENT')");
                    table.CheckConstraint("CK_BilSettlement_PurposeTarget", "((\"Purpose\" = 'INVOICE_PAYMENT' AND \"InvoiceId\" IS NOT NULL AND \"DepositAccountId\" IS NULL) OR (\"Purpose\" = 'DEPOSIT_TOP_UP' AND \"InvoiceId\" IS NULL AND \"DepositAccountId\" IS NOT NULL))");
                    table.CheckConstraint("CK_BilSettlement_Status", "\"Status\" IN ('DRAFT','IN_PROGRESS','PARTIALLY_SETTLED','SETTLED','FAILED')");
                    table.CheckConstraint("CK_BilSettlement_Target", "((\"InvoiceId\" IS NOT NULL AND \"DepositAccountId\" IS NULL) OR (\"InvoiceId\" IS NULL AND \"DepositAccountId\" IS NOT NULL))");
                    table.ForeignKey(
                        name: "FK_BilSettlement_BilDepositAccount_DepositAccountId",
                        column: x => x.DepositAccountId,
                        principalSchema: "public",
                        principalTable: "BilDepositAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilSettlement_BilInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "BilInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilApHandoff",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinalizationRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ReadinessStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    HandoffKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReadyAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_BilApHandoff", x => x.Id);
                    table.CheckConstraint("CK_BilApHandoff_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_BilApHandoff_ReadinessStatus", "\"ReadinessStatus\" IN ('NOT_READY','READY')");
                    table.CheckConstraint("CK_BilApHandoff_Status", "\"Status\" IN ('CREATED','ACKNOWLEDGED')");
                    table.ForeignKey(
                        name: "FK_BilApHandoff_BilFinalizationRecord_FinalizationRecordId",
                        column: x => x.FinalizationRecordId,
                        principalSchema: "public",
                        principalTable: "BilFinalizationRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilApHandoff_BilInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "BilInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilArHandoff",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinalizationRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    DebtorType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DebtorReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DueDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    HandoffKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AcknowledgedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_BilArHandoff", x => x.Id);
                    table.CheckConstraint("CK_BilArHandoff_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_BilArHandoff_DebtorType", "\"DebtorType\" IN ('PATIENT_GUARANTOR','PAYER')");
                    table.CheckConstraint("CK_BilArHandoff_Status", "\"Status\" IN ('CREATED','ACKNOWLEDGED')");
                    table.ForeignKey(
                        name: "FK_BilArHandoff_BilFinalizationRecord_FinalizationRecordId",
                        column: x => x.FinalizationRecordId,
                        principalSchema: "public",
                        principalTable: "BilFinalizationRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilArHandoff_BilInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "BilInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilRefundCase",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    RefundableCreditId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AdjustmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    PayloadHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_BilRefundCase", x => x.Id);
                    table.CheckConstraint("CK_BilRefundCase_RequestedAmount", "\"RequestedAmount\" > 0");
                    table.CheckConstraint("CK_BilRefundCase_Status", "\"Status\" IN ('SUBMITTED','APPROVED','REJECTED','PARTIALLY_EXECUTED','EXECUTED')");
                    table.ForeignKey(
                        name: "FK_BilRefundCase_BilInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "BilInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilRefundCase_BilRefundableCredit_RefundableCreditId",
                        column: x => x.RefundableCreditId,
                        principalSchema: "public",
                        principalTable: "BilRefundableCredit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilAdjustment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Direction = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReversesAdjustmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReversesWriteOffCaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    PayloadHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PostedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_BilAdjustment", x => x.Id);
                    table.CheckConstraint("CK_BilAdjustment_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_BilAdjustment_Direction", "\"Direction\" IN ('DEBIT','CREDIT')");
                    table.CheckConstraint("CK_BilAdjustment_ReversesOneSource", "NOT (\"ReversesAdjustmentId\" IS NOT NULL AND \"ReversesWriteOffCaseId\" IS NOT NULL)");
                    table.CheckConstraint("CK_BilAdjustment_Status", "\"Status\" IN ('SUBMITTED','POSTED','REJECTED')");
                    table.ForeignKey(
                        name: "FK_BilAdjustment_BilAdjustment_ReversesAdjustmentId",
                        column: x => x.ReversesAdjustmentId,
                        principalSchema: "public",
                        principalTable: "BilAdjustment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilAdjustment_BilInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "BilInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilAdjustment_BilWriteOffCase_ReversesWriteOffCaseId",
                        column: x => x.ReversesWriteOffCaseId,
                        principalSchema: "public",
                        principalTable: "BilWriteOffCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilDepositMovement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepositAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SettlementId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentMethodId = table.Column<Guid>(type: "uuid", nullable: true),
                    CashierShiftId = table.Column<Guid>(type: "uuid", nullable: true),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    PayloadHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReversesMovementId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_BilDepositMovement", x => x.Id);
                    table.CheckConstraint("CK_BilDepositMovement_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_BilDepositMovement_Type", "\"MovementType\" IN ('TOP_UP','ALLOCATION','RELEASE','REVERSAL')");
                    table.ForeignKey(
                        name: "FK_BilDepositMovement_BilCashierShift_CashierShiftId",
                        column: x => x.CashierShiftId,
                        principalSchema: "public",
                        principalTable: "BilCashierShift",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilDepositMovement_BilDepositAccount_DepositAccountId",
                        column: x => x.DepositAccountId,
                        principalSchema: "public",
                        principalTable: "BilDepositAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilDepositMovement_BilDepositMovement_ReversesMovementId",
                        column: x => x.ReversesMovementId,
                        principalSchema: "public",
                        principalTable: "BilDepositMovement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilDepositMovement_BilSettlement_SettlementId",
                        column: x => x.SettlementId,
                        principalSchema: "public",
                        principalTable: "BilSettlement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilDepositMovement_MstPaymentMethod_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalSchema: "public",
                        principalTable: "MstPaymentMethod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilPaymentAllocation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CalculationVersion = table.Column<int>(type: "integer", nullable: true),
                    AllocatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReversesAllocationId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_BilPaymentAllocation", x => x.Id);
                    table.CheckConstraint("CK_BilPaymentAllocation_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_BilPaymentAllocation_CalculationVersion", "\"CalculationVersion\" IS NULL OR \"CalculationVersion\" > 0");
                    table.CheckConstraint("CK_BilPaymentAllocation_TargetType", "\"TargetType\" = 'INVOICE'");
                    table.ForeignKey(
                        name: "FK_BilPaymentAllocation_BilInvoice_TargetId",
                        column: x => x.TargetId,
                        principalSchema: "public",
                        principalTable: "BilInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilPaymentAllocation_BilPaymentAllocation_ReversesAllocatio~",
                        column: x => x.ReversesAllocationId,
                        principalSchema: "public",
                        principalTable: "BilPaymentAllocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilPaymentAllocation_BilSettlement_SettlementId",
                        column: x => x.SettlementId,
                        principalSchema: "public",
                        principalTable: "BilSettlement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilTender",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentMethodId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ProviderReference = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ProviderStatusCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    PayloadHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SettledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CashierShiftId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProviderOccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastProviderEventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastProviderPayloadHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_BilTender", x => x.Id);
                    table.CheckConstraint("CK_BilTender_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_BilTender_Status", "\"Status\" IN ('CREATED','PENDING','SUCCEEDED','FAILED','EXPIRED','REVERSED')");
                    table.ForeignKey(
                        name: "FK_BilTender_BilCashierShift_CashierShiftId",
                        column: x => x.CashierShiftId,
                        principalSchema: "public",
                        principalTable: "BilCashierShift",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilTender_BilSettlement_SettlementId",
                        column: x => x.SettlementId,
                        principalSchema: "public",
                        principalTable: "BilSettlement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilTender_MstPaymentMethod_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalSchema: "public",
                        principalTable: "MstPaymentMethod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilHandoffAdjustment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ArHandoffId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApHandoffId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceAdjustmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceWriteOffCaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    Direction = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_BilHandoffAdjustment", x => x.Id);
                    table.CheckConstraint("CK_BilHandoffAdjustment_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_BilHandoffAdjustment_Direction", "\"Direction\" IN ('DEBIT','CREDIT')");
                    table.CheckConstraint("CK_BilHandoffAdjustment_ExactlyOneSource", "NOT (\"SourceAdjustmentId\" IS NOT NULL AND \"SourceWriteOffCaseId\" IS NOT NULL)");
                    table.CheckConstraint("CK_BilHandoffAdjustment_ExactlyOneTarget", "((\"ArHandoffId\" IS NOT NULL AND \"ApHandoffId\" IS NULL) OR (\"ArHandoffId\" IS NULL AND \"ApHandoffId\" IS NOT NULL))");
                    table.ForeignKey(
                        name: "FK_BilHandoffAdjustment_BilApHandoff_ApHandoffId",
                        column: x => x.ApHandoffId,
                        principalSchema: "public",
                        principalTable: "BilApHandoff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilHandoffAdjustment_BilArHandoff_ArHandoffId",
                        column: x => x.ArHandoffId,
                        principalSchema: "public",
                        principalTable: "BilArHandoff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilRefundLine",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RefundCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalTenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentMethodId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ProviderReference = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ProviderStatusCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SettledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_BilRefundLine", x => x.Id);
                    table.CheckConstraint("CK_BilRefundLine_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_BilRefundLine_Status", "\"Status\" IN ('PENDING','SUCCEEDED','FAILED')");
                    table.ForeignKey(
                        name: "FK_BilRefundLine_BilRefundCase_RefundCaseId",
                        column: x => x.RefundCaseId,
                        principalSchema: "public",
                        principalTable: "BilRefundCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilRefundLine_BilTender_OriginalTenderId",
                        column: x => x.OriginalTenderId,
                        principalSchema: "public",
                        principalTable: "BilTender",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BilAdjustment_CorrelationId",
                schema: "public",
                table: "BilAdjustment",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilAdjustment_IdempotencyKey",
                schema: "public",
                table: "BilAdjustment",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilAdjustment_InvoiceId_Status",
                schema: "public",
                table: "BilAdjustment",
                columns: new[] { "InvoiceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BilAdjustment_ReversesAdjustmentId",
                schema: "public",
                table: "BilAdjustment",
                column: "ReversesAdjustmentId",
                unique: true,
                filter: "\"ReversesAdjustmentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BilAdjustment_ReversesWriteOffCaseId",
                schema: "public",
                table: "BilAdjustment",
                column: "ReversesWriteOffCaseId",
                unique: true,
                filter: "\"ReversesWriteOffCaseId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BilApHandoff_CorrelationId",
                schema: "public",
                table: "BilApHandoff",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilApHandoff_FinalizationRecordId",
                schema: "public",
                table: "BilApHandoff",
                column: "FinalizationRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_BilApHandoff_HandoffKey",
                schema: "public",
                table: "BilApHandoff",
                column: "HandoffKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilApHandoff_InvoiceId_DoctorId",
                schema: "public",
                table: "BilApHandoff",
                columns: new[] { "InvoiceId", "DoctorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilArHandoff_CorrelationId",
                schema: "public",
                table: "BilArHandoff",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilArHandoff_FinalizationRecordId",
                schema: "public",
                table: "BilArHandoff",
                column: "FinalizationRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_BilArHandoff_HandoffKey",
                schema: "public",
                table: "BilArHandoff",
                column: "HandoffKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilArHandoff_InvoiceId_DebtorType",
                schema: "public",
                table: "BilArHandoff",
                columns: new[] { "InvoiceId", "DebtorType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilCashierShift_CashierId",
                schema: "public",
                table: "BilCashierShift",
                column: "CashierId",
                unique: true,
                filter: "\"Status\" IN ('OPEN','REOPENED') AND \"IsDelete\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_BilCashierShift_CashierId_Status",
                schema: "public",
                table: "BilCashierShift",
                columns: new[] { "CashierId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BilCashierShift_RegisterId",
                schema: "public",
                table: "BilCashierShift",
                column: "RegisterId",
                unique: true,
                filter: "\"Status\" IN ('OPEN','REOPENED') AND \"IsDelete\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_BilCashierShift_RegisterId_Status",
                schema: "public",
                table: "BilCashierShift",
                columns: new[] { "RegisterId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BilCashierShift_ShiftNumber",
                schema: "public",
                table: "BilCashierShift",
                column: "ShiftNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilCashierShiftCommand_CorrelationId",
                schema: "public",
                table: "BilCashierShiftCommand",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilCashierShiftCommand_IdempotencyKey",
                schema: "public",
                table: "BilCashierShiftCommand",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BilCashierShiftCommand_ShiftId_OccurredAt",
                schema: "public",
                table: "BilCashierShiftCommand",
                columns: new[] { "ShiftId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BilCashierShiftCommand_SourceType_SourceId",
                schema: "public",
                table: "BilCashierShiftCommand",
                columns: new[] { "SourceType", "SourceId" },
                unique: true,
                filter: "\"SourceType\" IS NOT NULL AND \"SourceId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BilCashierShiftHandover_IncomingCashierId",
                schema: "public",
                table: "BilCashierShiftHandover",
                column: "IncomingCashierId");

            migrationBuilder.CreateIndex(
                name: "IX_BilCashierShiftHandover_OutgoingCashierId",
                schema: "public",
                table: "BilCashierShiftHandover",
                column: "OutgoingCashierId");

            migrationBuilder.CreateIndex(
                name: "IX_BilCashierShiftHandover_ReceivingShiftId",
                schema: "public",
                table: "BilCashierShiftHandover",
                column: "ReceivingShiftId",
                unique: true,
                filter: "\"ReceivingShiftId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BilCashierShiftHandover_SourceShiftId",
                schema: "public",
                table: "BilCashierShiftHandover",
                column: "SourceShiftId",
                unique: true,
                filter: "\"Status\" = 'PENDING' AND \"IsDelete\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_BilCashVarianceReview_ReopenAuthorizedBy",
                schema: "public",
                table: "BilCashVarianceReview",
                column: "ReopenAuthorizedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BilCashVarianceReview_ReviewerId",
                schema: "public",
                table: "BilCashVarianceReview",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_BilCashVarianceReview_ShiftId_ReviewedAt",
                schema: "public",
                table: "BilCashVarianceReview",
                columns: new[] { "ShiftId", "ReviewedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BilDepositAccount_AccountNumber",
                schema: "public",
                table: "BilDepositAccount",
                column: "AccountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilDepositAccount_EncounterId",
                schema: "public",
                table: "BilDepositAccount",
                column: "EncounterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilDepositMovement_CashierShiftId",
                schema: "public",
                table: "BilDepositMovement",
                column: "CashierShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_BilDepositMovement_CorrelationId",
                schema: "public",
                table: "BilDepositMovement",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilDepositMovement_DepositAccountId_OccurredAt",
                schema: "public",
                table: "BilDepositMovement",
                columns: new[] { "DepositAccountId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BilDepositMovement_IdempotencyKey",
                schema: "public",
                table: "BilDepositMovement",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilDepositMovement_PaymentMethodId",
                schema: "public",
                table: "BilDepositMovement",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_BilDepositMovement_ReversesMovementId",
                schema: "public",
                table: "BilDepositMovement",
                column: "ReversesMovementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilDepositMovement_SettlementId",
                schema: "public",
                table: "BilDepositMovement",
                column: "SettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_BilDiscountApplication_DiscountPolicyId",
                schema: "public",
                table: "BilDiscountApplication",
                column: "DiscountPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_BilDiscountApplication_InvoiceId_ApprovalStatus_IsDelete",
                schema: "public",
                table: "BilDiscountApplication",
                columns: new[] { "InvoiceId", "ApprovalStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_BilDiscountApplication_InvoiceId_DiscountPolicyId_InvoiceIt~",
                schema: "public",
                table: "BilDiscountApplication",
                columns: new[] { "InvoiceId", "DiscountPolicyId", "InvoiceItemId", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_BilDiscountApplication_InvoiceItemId",
                schema: "public",
                table: "BilDiscountApplication",
                column: "InvoiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BilFinalizationRecord_CorrelationId",
                schema: "public",
                table: "BilFinalizationRecord",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilFinalizationRecord_IdempotencyKey",
                schema: "public",
                table: "BilFinalizationRecord",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilFinalizationRecord_InvoiceId",
                schema: "public",
                table: "BilFinalizationRecord",
                column: "InvoiceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilHandoffAdjustment_ApHandoffId",
                schema: "public",
                table: "BilHandoffAdjustment",
                column: "ApHandoffId");

            migrationBuilder.CreateIndex(
                name: "IX_BilHandoffAdjustment_ArHandoffId",
                schema: "public",
                table: "BilHandoffAdjustment",
                column: "ArHandoffId");

            migrationBuilder.CreateIndex(
                name: "IX_BilHandoffAdjustment_CorrelationId",
                schema: "public",
                table: "BilHandoffAdjustment",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilHandoffAdjustment_SourceAdjustmentId",
                schema: "public",
                table: "BilHandoffAdjustment",
                column: "SourceAdjustmentId",
                unique: true,
                filter: "\"SourceAdjustmentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BilHandoffAdjustment_SourceWriteOffCaseId",
                schema: "public",
                table: "BilHandoffAdjustment",
                column: "SourceWriteOffCaseId",
                unique: true,
                filter: "\"SourceWriteOffCaseId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BilPaymentAllocation_ReversesAllocationId",
                schema: "public",
                table: "BilPaymentAllocation",
                column: "ReversesAllocationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilPaymentAllocation_SettlementId_AllocatedAt",
                schema: "public",
                table: "BilPaymentAllocation",
                columns: new[] { "SettlementId", "AllocatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BilPaymentAllocation_TargetId",
                schema: "public",
                table: "BilPaymentAllocation",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_BilPaymentAllocation_TargetType_TargetId_AllocatedAt",
                schema: "public",
                table: "BilPaymentAllocation",
                columns: new[] { "TargetType", "TargetId", "AllocatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BilRefundableCredit_InvoiceId_Status",
                schema: "public",
                table: "BilRefundableCredit",
                columns: new[] { "InvoiceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BilRefundableCredit_SourceType_SourceId",
                schema: "public",
                table: "BilRefundableCredit",
                columns: new[] { "SourceType", "SourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilRefundCase_CorrelationId",
                schema: "public",
                table: "BilRefundCase",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilRefundCase_IdempotencyKey",
                schema: "public",
                table: "BilRefundCase",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilRefundCase_InvoiceId",
                schema: "public",
                table: "BilRefundCase",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BilRefundCase_RefundableCreditId_Status",
                schema: "public",
                table: "BilRefundCase",
                columns: new[] { "RefundableCreditId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BilRefundLine_OriginalTenderId",
                schema: "public",
                table: "BilRefundLine",
                column: "OriginalTenderId");

            migrationBuilder.CreateIndex(
                name: "IX_BilRefundLine_RefundCaseId_OriginalTenderId",
                schema: "public",
                table: "BilRefundLine",
                columns: new[] { "RefundCaseId", "OriginalTenderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilSettlement_CorrelationId",
                schema: "public",
                table: "BilSettlement",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilSettlement_DepositAccountId_Status",
                schema: "public",
                table: "BilSettlement",
                columns: new[] { "DepositAccountId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BilSettlement_IdempotencyKey",
                schema: "public",
                table: "BilSettlement",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilSettlement_InvoiceId_Status",
                schema: "public",
                table: "BilSettlement",
                columns: new[] { "InvoiceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BilTender_CashierShiftId",
                schema: "public",
                table: "BilTender",
                column: "CashierShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_BilTender_CorrelationId",
                schema: "public",
                table: "BilTender",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilTender_IdempotencyKey",
                schema: "public",
                table: "BilTender",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilTender_PaymentMethodId",
                schema: "public",
                table: "BilTender",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_BilTender_ProviderReference",
                schema: "public",
                table: "BilTender",
                column: "ProviderReference",
                unique: true,
                filter: "\"ProviderReference\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BilTender_SettlementId_Status",
                schema: "public",
                table: "BilTender",
                columns: new[] { "SettlementId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BilWriteOffCase_CorrelationId",
                schema: "public",
                table: "BilWriteOffCase",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilWriteOffCase_IdempotencyKey",
                schema: "public",
                table: "BilWriteOffCase",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilWriteOffCase_InvoiceId_Status",
                schema: "public",
                table: "BilWriteOffCase",
                columns: new[] { "InvoiceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInpatientClearanceItem_IsActive",
                schema: "public",
                table: "MstInpatientClearanceItem",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MstInpatientClearanceItem_IsMandatory",
                schema: "public",
                table: "MstInpatientClearanceItem",
                column: "IsMandatory");

            migrationBuilder.CreateIndex(
                name: "IX_MstInpatientClearanceItem_ItemCode",
                schema: "public",
                table: "MstInpatientClearanceItem",
                column: "ItemCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstInpatientSetting_Code",
                schema: "public",
                table: "MstInpatientSetting",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstInpatientSetting_IsActive_IsDefault",
                schema: "public",
                table: "MstInpatientSetting",
                columns: new[] { "IsActive", "IsDefault" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BilAdjustment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilCashierShiftCommand",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilCashierShiftHandover",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilCashVarianceReview",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilDepositMovement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilDiscountApplication",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilHandoffAdjustment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilPaymentAllocation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilRefundLine",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstInpatientClearanceItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstInpatientSetting",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilWriteOffCase",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilApHandoff",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilArHandoff",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilRefundCase",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilTender",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilFinalizationRecord",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilRefundableCredit",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilCashierShift",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilSettlement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilDepositAccount",
                schema: "public");
        }
    }
}
