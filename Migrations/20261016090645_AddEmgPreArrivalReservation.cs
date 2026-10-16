using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmgPreArrivalReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmgPreArrivalReservation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientMembershipId = table.Column<Guid>(type: "uuid", nullable: true),
                    MembershipTierId = table.Column<Guid>(type: "uuid", nullable: true),
                    CallerName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CallerPhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EstimatedArrivalDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChiefComplaint = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RequiresAmbulance = table.Column<bool>(type: "boolean", nullable: false),
                    AmbulanceArrivalModeId = table.Column<Guid>(type: "uuid", nullable: true),
                    AmbulancePickupAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AssignedEscortStaffName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ResultEmergencyVisitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArrivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_EmgPreArrivalReservation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmgPreArrivalReservation_EmgArrivalMode_AmbulanceArrivalMod~",
                        column: x => x.AmbulanceArrivalModeId,
                        principalSchema: "public",
                        principalTable: "EmgArrivalMode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmgPreArrivalReservation_EmgVisit_ResultEmergencyVisitId",
                        column: x => x.ResultEmergencyVisitId,
                        principalSchema: "public",
                        principalTable: "EmgVisit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmgPreArrivalReservation_MstMembershipTier_MembershipTierId",
                        column: x => x.MembershipTierId,
                        principalSchema: "public",
                        principalTable: "MstMembershipTier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmgPreArrivalReservation_MstPatientMembership_PatientMember~",
                        column: x => x.PatientMembershipId,
                        principalSchema: "public",
                        principalTable: "MstPatientMembership",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmgPreArrivalReservation_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmgPreArrivalReservation_AmbulanceArrivalModeId",
                schema: "public",
                table: "EmgPreArrivalReservation",
                column: "AmbulanceArrivalModeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgPreArrivalReservation_MembershipTierId",
                schema: "public",
                table: "EmgPreArrivalReservation",
                column: "MembershipTierId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgPreArrivalReservation_PatientId",
                schema: "public",
                table: "EmgPreArrivalReservation",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgPreArrivalReservation_PatientMembershipId",
                schema: "public",
                table: "EmgPreArrivalReservation",
                column: "PatientMembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgPreArrivalReservation_ReservationNumber",
                schema: "public",
                table: "EmgPreArrivalReservation",
                column: "ReservationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmgPreArrivalReservation_ResultEmergencyVisitId",
                schema: "public",
                table: "EmgPreArrivalReservation",
                column: "ResultEmergencyVisitId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgPreArrivalReservation_Status",
                schema: "public",
                table: "EmgPreArrivalReservation",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmgPreArrivalReservation",
                schema: "public");
        }
    }
}
