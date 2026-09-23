using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBbkIssuanceCorrection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BbkIssuanceCorrection",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BloodUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    WhatWasWrong = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    WhatIsCorrect = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SupportingEvidenceNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CorrectionStatus = table.Column<int>(type: "integer", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_BbkIssuanceCorrection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BbkIssuanceCorrection_BbkBloodUnit_BloodUnitId",
                        column: x => x.BloodUnitId,
                        principalSchema: "public",
                        principalTable: "BbkBloodUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BbkIssuanceCorrection_BloodUnitId",
                schema: "public",
                table: "BbkIssuanceCorrection",
                column: "BloodUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkIssuanceCorrection_CorrectionStatus",
                schema: "public",
                table: "BbkIssuanceCorrection",
                column: "CorrectionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_BbkIssuanceCorrection_RequestedByUserId",
                schema: "public",
                table: "BbkIssuanceCorrection",
                column: "RequestedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BbkIssuanceCorrection",
                schema: "public");
        }
    }
}
