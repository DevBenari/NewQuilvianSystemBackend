using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBbkEmergencyAuthorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BbkEmergencyAuthorization",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BloodUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorizedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ReasonNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BypassScope = table.Column<int>(type: "integer", nullable: false),
                    AuthorizerRole = table.Column<int>(type: "integer", nullable: false),
                    EmergencyConditionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
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
                    table.PrimaryKey("PK_BbkEmergencyAuthorization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BbkEmergencyAuthorization_BbkBloodUnit_BloodUnitId",
                        column: x => x.BloodUnitId,
                        principalSchema: "public",
                        principalTable: "BbkBloodUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkEmergencyAuthorization_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BbkEmergencyAuthorization_AuthorizerRole",
                schema: "public",
                table: "BbkEmergencyAuthorization",
                column: "AuthorizerRole");

            migrationBuilder.CreateIndex(
                name: "IX_BbkEmergencyAuthorization_BloodUnitId",
                schema: "public",
                table: "BbkEmergencyAuthorization",
                column: "BloodUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkEmergencyAuthorization_PatientId",
                schema: "public",
                table: "BbkEmergencyAuthorization",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BbkEmergencyAuthorization",
                schema: "public");
        }
    }
}
