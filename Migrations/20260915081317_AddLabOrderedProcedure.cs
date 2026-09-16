using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabOrderedProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabOrderedProcedure",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LabOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProcedureNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DisciplineSnapshot = table.Column<int>(type: "integer", nullable: true),
                    Urgency = table.Column<int>(type: "integer", nullable: false),
                    OrderedStatus = table.Column<int>(type: "integer", nullable: false),
                    FulfilledExaminationId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_LabOrderedProcedure", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabOrderedProcedure_LabExamination_FulfilledExaminationId",
                        column: x => x.FulfilledExaminationId,
                        principalSchema: "public",
                        principalTable: "LabExamination",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabOrderedProcedure_LabOrder_LabOrderId",
                        column: x => x.LabOrderId,
                        principalSchema: "public",
                        principalTable: "LabOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabOrderedProcedure_MstProcedure_ProcedureId",
                        column: x => x.ProcedureId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabOrderedProcedure_FulfilledExaminationId",
                schema: "public",
                table: "LabOrderedProcedure",
                column: "FulfilledExaminationId");

            migrationBuilder.CreateIndex(
                name: "IX_LabOrderedProcedure_LabOrderId_ProcedureId",
                schema: "public",
                table: "LabOrderedProcedure",
                columns: new[] { "LabOrderId", "ProcedureId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_LabOrderedProcedure_OrderedStatus",
                schema: "public",
                table: "LabOrderedProcedure",
                column: "OrderedStatus");

            migrationBuilder.CreateIndex(
                name: "IX_LabOrderedProcedure_ProcedureId",
                schema: "public",
                table: "LabOrderedProcedure",
                column: "ProcedureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabOrderedProcedure",
                schema: "public");
        }
    }
}
