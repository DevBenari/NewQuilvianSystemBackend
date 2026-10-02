using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencyDoctorAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmgDoctorAssignment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmergencyVisitId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_EmgDoctorAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmgDoctorAssignment_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmgDoctorAssignment_EmgVisit_EmergencyVisitId",
                        column: x => x.EmergencyVisitId,
                        principalSchema: "public",
                        principalTable: "EmgVisit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmgDoctorAssignment_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmgDoctorAssignment_AssignedByUserId",
                schema: "public",
                table: "EmgDoctorAssignment",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgDoctorAssignment_DoctorId",
                schema: "public",
                table: "EmgDoctorAssignment",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgDoctorAssignment_EffectiveTo",
                schema: "public",
                table: "EmgDoctorAssignment",
                column: "EffectiveTo");

            migrationBuilder.CreateIndex(
                name: "IX_EmgDoctorAssignment_EmergencyVisitId_Active",
                schema: "public",
                table: "EmgDoctorAssignment",
                column: "EmergencyVisitId",
                unique: true,
                filter: "\"EffectiveTo\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EmgDoctorAssignment_EmergencyVisitId_EffectiveFrom",
                schema: "public",
                table: "EmgDoctorAssignment",
                columns: new[] { "EmergencyVisitId", "EffectiveFrom" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmgDoctorAssignment",
                schema: "public");
        }
    }
}
