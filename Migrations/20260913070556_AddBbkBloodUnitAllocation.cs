using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBbkBloodUnitAllocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BbkBloodUnitAllocation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BloodUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    BloodOrderLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocationStatus = table.Column<int>(type: "integer", nullable: false),
                    AllocatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CancelReasonCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CancelReasonNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_BbkBloodUnitAllocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BbkBloodUnitAllocation_BbkBloodOrderLine_BloodOrderLineId",
                        column: x => x.BloodOrderLineId,
                        principalSchema: "public",
                        principalTable: "BbkBloodOrderLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkBloodUnitAllocation_BbkBloodUnit_BloodUnitId",
                        column: x => x.BloodUnitId,
                        principalSchema: "public",
                        principalTable: "BbkBloodUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodUnitAllocation_ActiveUnit",
                schema: "public",
                table: "BbkBloodUnitAllocation",
                column: "BloodUnitId",
                unique: true,
                filter: "\"AllocationStatus\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodUnitAllocation_AllocationStatus",
                schema: "public",
                table: "BbkBloodUnitAllocation",
                column: "AllocationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodUnitAllocation_BloodOrderLineId",
                schema: "public",
                table: "BbkBloodUnitAllocation",
                column: "BloodOrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodUnitAllocation_BloodUnitId",
                schema: "public",
                table: "BbkBloodUnitAllocation",
                column: "BloodUnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BbkBloodUnitAllocation",
                schema: "public");
        }
    }
}
