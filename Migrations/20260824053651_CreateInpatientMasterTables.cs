using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class CreateInpatientMasterTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstInpatientClearanceItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_MstInpatientClearanceItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstInpatientSetting",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BedReservationMinutes = table.Column<int>(type: "integer", nullable: false),
                    DraftEpisodeExpiryHours = table.Column<int>(type: "integer", nullable: false),
                    InitialAssessmentTargetHours = table.Column<int>(type: "integer", nullable: false),
                    ProgressNoteVerificationTargetHours = table.Column<int>(type: "integer", nullable: false),
                    PendingClosureThresholdHours = table.Column<int>(type: "integer", nullable: false),
                    EpisodeNumberPrefix = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_MstInpatientSetting", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstInpatientClearanceItem_IsActive",
                schema: "public",
                table: "MstInpatientClearanceItem",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MstInpatientClearanceItem_IsMandatory",
                schema: "public",
                table: "MstInpatientClearanceItem",
                column: "IsMandatory");

            migrationBuilder.CreateIndex(
                name: "IX_MstInpatientClearanceItem_ItemCode",
                schema: "public",
                table: "MstInpatientClearanceItem",
                column: "ItemCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstInpatientSetting_Code",
                schema: "public",
                table: "MstInpatientSetting",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstInpatientSetting_IsActive_IsDefault",
                schema: "public",
                table: "MstInpatientSetting",
                columns: new[] { "IsActive", "IsDefault" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstInpatientClearanceItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstInpatientSetting",
                schema: "public");
        }
    }
}
