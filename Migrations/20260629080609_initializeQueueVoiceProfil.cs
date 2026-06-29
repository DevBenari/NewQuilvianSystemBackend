using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeQueueVoiceProfil : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstQueueVoiceProfile",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoiceCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    VoiceName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Female"),
                    Language = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "id-ID"),
                    ModelPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LengthScale = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 1.08m),
                    NoiseScale = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0.65m),
                    NoiseW = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0.80m),
                    Volume = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 1.15m),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstQueueVoiceProfile", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstQueueVoiceProfile_IsActive_IsDelete_SortOrder",
                schema: "public",
                table: "MstQueueVoiceProfile",
                columns: new[] { "IsActive", "IsDelete", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MstQueueVoiceProfile_IsDefault_IsActive_IsDelete",
                schema: "public",
                table: "MstQueueVoiceProfile",
                columns: new[] { "IsDefault", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstQueueVoiceProfile_VoiceCode",
                schema: "public",
                table: "MstQueueVoiceProfile",
                column: "VoiceCode",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstQueueVoiceProfile",
                schema: "public");
        }
    }
}
