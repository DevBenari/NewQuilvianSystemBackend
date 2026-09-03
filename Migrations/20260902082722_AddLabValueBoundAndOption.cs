using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabValueBoundAndOption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabValueBound",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResultForm = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    NormalLow = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    NormalHigh = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    CriticalLow = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    CriticalHigh = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    GenderScope = table.Column<int>(type: "integer", nullable: false),
                    AgeCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    CitoTurnaroundMinutes = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_LabValueBound", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabValueBound_MstAgeCategory_AgeCategoryId",
                        column: x => x.AgeCategoryId,
                        principalSchema: "public",
                        principalTable: "MstAgeCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabValueBound_MstProcedure_ProcedureId",
                        column: x => x.ProcedureId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LabValueOption",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ValueBoundId = table.Column<Guid>(type: "uuid", nullable: false),
                    OptionCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OptionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsOutOfReference = table.Column<bool>(type: "boolean", nullable: false),
                    IsCritical = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_LabValueOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabValueOption_LabValueBound_ValueBoundId",
                        column: x => x.ValueBoundId,
                        principalSchema: "public",
                        principalTable: "LabValueBound",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabValueBound_AgeCategoryId",
                schema: "public",
                table: "LabValueBound",
                column: "AgeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_LabValueBound_Procedure_Gender_AgeCategory",
                schema: "public",
                table: "LabValueBound",
                columns: new[] { "ProcedureId", "GenderScope", "AgeCategoryId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_LabValueOption_ValueBoundId_OptionCode",
                schema: "public",
                table: "LabValueOption",
                columns: new[] { "ValueBoundId", "OptionCode" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabValueOption",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LabValueBound",
                schema: "public");
        }
    }
}
