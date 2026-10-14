using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddInpatientBedManagementReadinessAndLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                schema: "public",
                table: "InpBedReservation",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CancelledByUserId",
                schema: "public",
                table: "InpBedReservation",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationSnapshotJson",
                schema: "public",
                table: "InpBedPlacement",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransferCategory",
                schema: "public",
                table: "InpBedPlacement",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TransferFromPlacementId",
                schema: "public",
                table: "InpBedPlacement",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InpBedOperationReceipt",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OperationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ResultKind = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResultEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResultVersion = table.Column<long>(type: "bigint", nullable: true),
                    CommittedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_InpBedOperationReceipt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InpBedOperationReceipt_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpBedReadiness",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BedId = table.Column<Guid>(type: "uuid", nullable: false),
                    CycleId = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    LastReleasedPlacementId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_InpBedReadiness", x => x.Id);
                    table.CheckConstraint("CK_InpBedReadiness_Ready", "\"State\" <> 4 OR (\"VerifiedAtUtc\" IS NOT NULL AND \"VerificationReference\" IS NOT NULL)");
                    table.CheckConstraint("CK_InpBedReadiness_State", "\"State\" BETWEEN 0 AND 4");
                    table.CheckConstraint("CK_InpBedReadiness_Verification", "(\"VerifiedAtUtc\" IS NULL) = (\"VerifiedByUserId\" IS NULL)");
                    table.CheckConstraint("CK_InpBedReadiness_Version", "\"Version\" > 0");
                    table.ForeignKey(
                        name: "FK_InpBedReadiness_AspNetUsers_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpBedReadiness_InpBedPlacement_LastReleasedPlacementId",
                        column: x => x.LastReleasedPlacementId,
                        principalSchema: "public",
                        principalTable: "InpBedPlacement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpBedReadiness_MstBed_BedId",
                        column: x => x.BedId,
                        principalSchema: "public",
                        principalTable: "MstBed",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpBedCleaningAttempt",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReadinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    CycleId = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SopReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_InpBedCleaningAttempt", x => x.Id);
                    table.CheckConstraint("CK_InpBedCleaningAttempt_Completion", "(\"CompletedAtUtc\" IS NULL) = (\"CompletedByUserId\" IS NULL)");
                    table.CheckConstraint("CK_InpBedCleaningAttempt_Decision", "\"State\" NOT IN (3,4) OR \"VerifiedAtUtc\" IS NOT NULL");
                    table.CheckConstraint("CK_InpBedCleaningAttempt_Rejection", "\"State\" <> 4 OR (\"RejectionReason\" IS NOT NULL AND length(trim(\"RejectionReason\")) > 0)");
                    table.CheckConstraint("CK_InpBedCleaningAttempt_State", "\"State\" BETWEEN 1 AND 5");
                    table.CheckConstraint("CK_InpBedCleaningAttempt_Verification", "(\"VerifiedAtUtc\" IS NULL) = (\"VerifiedByUserId\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_InpBedCleaningAttempt_AspNetUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpBedCleaningAttempt_AspNetUsers_StartedByUserId",
                        column: x => x.StartedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpBedCleaningAttempt_AspNetUsers_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpBedCleaningAttempt_InpBedReadiness_ReadinessId",
                        column: x => x.ReadinessId,
                        principalSchema: "public",
                        principalTable: "InpBedReadiness",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpBedLifecycleEvent",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReadinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    CycleId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationReceiptId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlacementId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReservationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CleaningAttemptId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BeforeState = table.Column<int>(type: "integer", nullable: false),
                    AfterState = table.Column<int>(type: "integer", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecordedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EvidenceReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_InpBedLifecycleEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InpBedLifecycleEvent_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpBedLifecycleEvent_InpBedCleaningAttempt_CleaningAttemptId",
                        column: x => x.CleaningAttemptId,
                        principalSchema: "public",
                        principalTable: "InpBedCleaningAttempt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpBedLifecycleEvent_InpBedOperationReceipt_OperationReceip~",
                        column: x => x.OperationReceiptId,
                        principalSchema: "public",
                        principalTable: "InpBedOperationReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpBedLifecycleEvent_InpBedPlacement_PlacementId",
                        column: x => x.PlacementId,
                        principalSchema: "public",
                        principalTable: "InpBedPlacement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpBedLifecycleEvent_InpBedReadiness_ReadinessId",
                        column: x => x.ReadinessId,
                        principalSchema: "public",
                        principalTable: "InpBedReadiness",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpBedLifecycleEvent_InpBedReservation_ReservationId",
                        column: x => x.ReservationId,
                        principalSchema: "public",
                        principalTable: "InpBedReservation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InpBedReservation_CancelledByUserId",
                schema: "public",
                table: "InpBedReservation",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedPlacement_TransferFromPlacementId",
                schema: "public",
                table: "InpBedPlacement",
                column: "TransferFromPlacementId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InpBedPlacement_TransferCategory",
                schema: "public",
                table: "InpBedPlacement",
                sql: "\"TransferCategory\" IS NULL OR \"TransferCategory\" BETWEEN 1 AND 3");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedCleaningAttempt_Active",
                schema: "public",
                table: "InpBedCleaningAttempt",
                columns: new[] { "ReadinessId", "CycleId" },
                unique: true,
                filter: "\"State\" IN (1,2)");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedCleaningAttempt_CompletedByUserId",
                schema: "public",
                table: "InpBedCleaningAttempt",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedCleaningAttempt_ReadinessId_CycleId_StartedAtUtc",
                schema: "public",
                table: "InpBedCleaningAttempt",
                columns: new[] { "ReadinessId", "CycleId", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_InpBedCleaningAttempt_StartedByUserId",
                schema: "public",
                table: "InpBedCleaningAttempt",
                column: "StartedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedCleaningAttempt_VerifiedByUserId",
                schema: "public",
                table: "InpBedCleaningAttempt",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedLifecycleEvent_ActorUserId",
                schema: "public",
                table: "InpBedLifecycleEvent",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedLifecycleEvent_CleaningAttemptId",
                schema: "public",
                table: "InpBedLifecycleEvent",
                column: "CleaningAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedLifecycleEvent_OperationReceiptId",
                schema: "public",
                table: "InpBedLifecycleEvent",
                column: "OperationReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedLifecycleEvent_PlacementId",
                schema: "public",
                table: "InpBedLifecycleEvent",
                column: "PlacementId");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedLifecycleEvent_ReadinessId_OccurredAtUtc_Id",
                schema: "public",
                table: "InpBedLifecycleEvent",
                columns: new[] { "ReadinessId", "OccurredAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_InpBedLifecycleEvent_ReservationId",
                schema: "public",
                table: "InpBedLifecycleEvent",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedOperationReceipt_ActorUserId_IdempotencyKey",
                schema: "public",
                table: "InpBedOperationReceipt",
                columns: new[] { "ActorUserId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpBedOperationReceipt_CommittedAtUtc",
                schema: "public",
                table: "InpBedOperationReceipt",
                column: "CommittedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedReadiness_BedId",
                schema: "public",
                table: "InpBedReadiness",
                column: "BedId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpBedReadiness_LastReleasedPlacementId",
                schema: "public",
                table: "InpBedReadiness",
                column: "LastReleasedPlacementId");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedReadiness_VerifiedByUserId",
                schema: "public",
                table: "InpBedReadiness",
                column: "VerifiedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_InpBedPlacement_InpBedPlacement_TransferFromPlacementId",
                schema: "public",
                table: "InpBedPlacement",
                column: "TransferFromPlacementId",
                principalSchema: "public",
                principalTable: "InpBedPlacement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InpBedReservation_AspNetUsers_CancelledByUserId",
                schema: "public",
                table: "InpBedReservation",
                column: "CancelledByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InpBedPlacement_InpBedPlacement_TransferFromPlacementId",
                schema: "public",
                table: "InpBedPlacement");

            migrationBuilder.DropForeignKey(
                name: "FK_InpBedReservation_AspNetUsers_CancelledByUserId",
                schema: "public",
                table: "InpBedReservation");

            migrationBuilder.DropTable(
                name: "InpBedLifecycleEvent",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpBedCleaningAttempt",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpBedOperationReceipt",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpBedReadiness",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_InpBedReservation_CancelledByUserId",
                schema: "public",
                table: "InpBedReservation");

            migrationBuilder.DropIndex(
                name: "IX_InpBedPlacement_TransferFromPlacementId",
                schema: "public",
                table: "InpBedPlacement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InpBedPlacement_TransferCategory",
                schema: "public",
                table: "InpBedPlacement");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                schema: "public",
                table: "InpBedReservation");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                schema: "public",
                table: "InpBedReservation");

            migrationBuilder.DropColumn(
                name: "LocationSnapshotJson",
                schema: "public",
                table: "InpBedPlacement");

            migrationBuilder.DropColumn(
                name: "TransferCategory",
                schema: "public",
                table: "InpBedPlacement");

            migrationBuilder.DropColumn(
                name: "TransferFromPlacementId",
                schema: "public",
                table: "InpBedPlacement");
        }
    }
}
