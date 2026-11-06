using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddNurseViewPatientCall : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NvwBedTablet",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BedId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationNote = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DeviceStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    LoginUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LoginIssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LoginIssuedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastOnlineAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeactivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeactivatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeactivationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_NvwBedTablet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NvwBedTablet_AspNetUsers_LoginUserId",
                        column: x => x.LoginUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NvwBedTablet_MstBed_BedId",
                        column: x => x.BedId,
                        principalSchema: "public",
                        principalTable: "MstBed",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NvwPatientCallWaitThreshold",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "GLOBAL"),
                    WarningAfterMinutes = table.Column<int>(type: "integer", nullable: false),
                    CriticalAfterMinutes = table.Column<int>(type: "integer", nullable: false),
                    RatificationReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RatifiedOn = table.Column<DateOnly>(type: "date", nullable: false),
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
                    table.PrimaryKey("PK_NvwPatientCallWaitThreshold", x => x.Id);
                    table.CheckConstraint("CK_NvwPatientCallWaitThreshold_Order", "\"WarningAfterMinutes\" BETWEEN 1 AND 240 AND \"CriticalAfterMinutes\" > \"WarningAfterMinutes\" AND \"CriticalAfterMinutes\" <= 480");
                });

            migrationBuilder.CreateTable(
                name: "NvwPatientCall",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BedId = table.Column<Guid>(type: "uuid", nullable: false),
                    BedTabletId = table.Column<Guid>(type: "uuid", nullable: false),
                    InpBedPlacementId = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloorCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    CallStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    FirstPressedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastPressedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PressCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    EscalatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClaimedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClaimedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CloseReason = table.Column<int>(type: "integer", nullable: true),
                    CloseNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsBedsideConfirmed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_NvwPatientCall", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NvwPatientCall_InpBedPlacement_InpBedPlacementId",
                        column: x => x.InpBedPlacementId,
                        principalSchema: "public",
                        principalTable: "InpBedPlacement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NvwPatientCall_MstBed_BedId",
                        column: x => x.BedId,
                        principalSchema: "public",
                        principalTable: "MstBed",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NvwPatientCall_NvwBedTablet_BedTabletId",
                        column: x => x.BedTabletId,
                        principalSchema: "public",
                        principalTable: "NvwBedTablet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NvwPatientCallEvent",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientCallId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    ActorKind = table.Column<int>(type: "integer", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreviousClaimerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CloseReason = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_NvwPatientCallEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NvwPatientCallEvent_NvwPatientCall_PatientCallId",
                        column: x => x.PatientCallId,
                        principalSchema: "public",
                        principalTable: "NvwPatientCall",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NvwBedTablet_BedId_DeviceStatus",
                schema: "public",
                table: "NvwBedTablet",
                columns: new[] { "BedId", "DeviceStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_NvwBedTablet_BedId_InUse",
                schema: "public",
                table: "NvwBedTablet",
                column: "BedId",
                unique: true,
                filter: "\"DeviceStatus\" <> 3 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_NvwBedTablet_DeviceCode",
                schema: "public",
                table: "NvwBedTablet",
                column: "DeviceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NvwBedTablet_LoginUserId",
                schema: "public",
                table: "NvwBedTablet",
                column: "LoginUserId",
                unique: true,
                filter: "\"LoginUserId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NvwPatientCall_BedId_Active",
                schema: "public",
                table: "NvwPatientCall",
                column: "BedId",
                unique: true,
                filter: "\"CallStatus\" IN (1, 2) AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_NvwPatientCall_BedTabletId",
                schema: "public",
                table: "NvwPatientCall",
                column: "BedTabletId");

            migrationBuilder.CreateIndex(
                name: "IX_NvwPatientCall_FirstPressedAt",
                schema: "public",
                table: "NvwPatientCall",
                column: "FirstPressedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NvwPatientCall_FloorCode_CallStatus",
                schema: "public",
                table: "NvwPatientCall",
                columns: new[] { "FloorCode", "CallStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_NvwPatientCall_InpBedPlacementId",
                schema: "public",
                table: "NvwPatientCall",
                column: "InpBedPlacementId");

            migrationBuilder.CreateIndex(
                name: "IX_NvwPatientCallEvent_IdempotencyKey",
                schema: "public",
                table: "NvwPatientCallEvent",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NvwPatientCallEvent_PatientCallId_OccurredAt",
                schema: "public",
                table: "NvwPatientCallEvent",
                columns: new[] { "PatientCallId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_NvwPatientCallWaitThreshold_ScopeCode_Active",
                schema: "public",
                table: "NvwPatientCallWaitThreshold",
                column: "ScopeCode",
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NvwPatientCallEvent",
                schema: "public");

            migrationBuilder.DropTable(
                name: "NvwPatientCallWaitThreshold",
                schema: "public");

            migrationBuilder.DropTable(
                name: "NvwPatientCall",
                schema: "public");

            migrationBuilder.DropTable(
                name: "NvwBedTablet",
                schema: "public");
        }
    }
}
