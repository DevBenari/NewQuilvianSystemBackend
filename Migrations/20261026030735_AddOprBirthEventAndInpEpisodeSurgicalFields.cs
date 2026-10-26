using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddOprBirthEventAndInpEpisodeSurgicalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BedHoldStatus",
                schema: "public",
                table: "InpEpisode",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClinicalSubCategory",
                schema: "public",
                table: "InpEpisode",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRoomingInBabyPlanned",
                schema: "public",
                table: "InpEpisode",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "MaternityPackageId",
                schema: "public",
                table: "InpEpisode",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SurgicalBookingNumber",
                schema: "public",
                table: "InpEpisode",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OprBirthEvent",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OprCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    BirthDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    WeightGrams = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    LengthCentimeters = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    HeadCircumferenceCm = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    ApgarScore1Min = table.Column<int>(type: "integer", nullable: false),
                    ApgarScore5Min = table.Column<int>(type: "integer", nullable: false),
                    ApgarScore10Min = table.Column<int>(type: "integer", nullable: true),
                    ResuscitationNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ConditionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RecordedByDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_OprBirthEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OprBirthEvent_OprCase_OprCaseId",
                        column: x => x.OprCaseId,
                        principalSchema: "public",
                        principalTable: "OprCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OprBirthEvent_OprCaseId",
                schema: "public",
                table: "OprBirthEvent",
                column: "OprCaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OprBirthEvent",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "BedHoldStatus",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "ClinicalSubCategory",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "IsRoomingInBabyPlanned",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "MaternityPackageId",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "SurgicalBookingNumber",
                schema: "public",
                table: "InpEpisode");
        }
    }
}
