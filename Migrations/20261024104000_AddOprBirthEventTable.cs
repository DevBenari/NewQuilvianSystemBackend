using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-OPR-014 (OPS-DEC-041, OPS-DEC-042) — Tabel OprBirthEvent untuk pencatatan kelahiran bayi intraoperatif SC.
    /// Operasi murni aditif: CreateTable dan CreateIndex.
    /// </summary>
    public partial class AddOprBirthEventTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: true)
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
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OprBirthEvent_OprCaseId",
                schema: "public",
                table: "OprBirthEvent",
                column: "OprCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_OprBirthEvent_BirthDateTime",
                schema: "public",
                table: "OprBirthEvent",
                column: "BirthDateTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OprBirthEvent",
                schema: "public");
        }
    }
}
