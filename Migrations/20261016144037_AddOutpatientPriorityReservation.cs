using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddOutpatientPriorityReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegPriorityReservation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservationNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitDate = table.Column<DateTime>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ArrivalCodeHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ArrivalCodeIssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArrivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ArrivalSource = table.Column<int>(type: "integer", nullable: true),
                    ArrivalMarkedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScreeningCough = table.Column<bool>(type: "boolean", nullable: true),
                    ScreeningFever = table.Column<bool>(type: "boolean", nullable: true),
                    ScreeningFallRisk = table.Column<bool>(type: "boolean", nullable: true),
                    ScreenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScreenedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsPrioritySnapshot = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PatientMembershipIdSnapshot = table.Column<Guid>(type: "uuid", nullable: true),
                    MembershipTierIdSnapshot = table.Column<Guid>(type: "uuid", nullable: true),
                    MembershipTierCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    QueuePriorityLevelSnapshot = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    QueueAudienceSnapshot = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PublicDisplayModeSnapshot = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
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
                    table.PrimaryKey("PK_RegPriorityReservation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegPriorityReservation_MstPatientMembership_PatientMembersh~",
                        column: x => x.PatientMembershipIdSnapshot,
                        principalSchema: "public",
                        principalTable: "MstPatientMembership",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegPriorityReservation_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegPriorityReservation_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegPriorityReservationHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PriorityReservationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: true),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_RegPriorityReservationHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegPriorityReservationHistory_RegPriorityReservation_Priori~",
                        column: x => x.PriorityReservationId,
                        principalSchema: "public",
                        principalTable: "RegPriorityReservation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegPriorityReservation_ArrivalCodeHash",
                schema: "public",
                table: "RegPriorityReservation",
                column: "ArrivalCodeHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegPriorityReservation_EncounterId",
                schema: "public",
                table: "RegPriorityReservation",
                column: "EncounterId",
                unique: true,
                filter: "\"EncounterId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RegPriorityReservation_PatientMembershipIdSnapshot",
                schema: "public",
                table: "RegPriorityReservation",
                column: "PatientMembershipIdSnapshot");

            migrationBuilder.CreateIndex(
                name: "IX_RegPriorityReservation_ReservationNumber",
                schema: "public",
                table: "RegPriorityReservation",
                column: "ReservationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegPriorityReservation_VisitDate_Status",
                schema: "public",
                table: "RegPriorityReservation",
                columns: new[] { "VisitDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "UX_RegPriorityReservation_Open_Patient_VisitDate",
                schema: "public",
                table: "RegPriorityReservation",
                columns: new[] { "PatientId", "VisitDate" },
                unique: true,
                filter: "\"Status\" IN (1, 2) AND NOT \"IsDelete\"");

            migrationBuilder.CreateIndex(
                name: "IX_RegPriorityReservationHistory_PriorityReservationId_Occurre~",
                schema: "public",
                table: "RegPriorityReservationHistory",
                columns: new[] { "PriorityReservationId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegPriorityReservationHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RegPriorityReservation",
                schema: "public");
        }
    }
}
