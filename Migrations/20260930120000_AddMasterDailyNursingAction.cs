using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Master data tindakan harian keperawatan rawat inap — tabel <c>MstDailyNursingAction</c>
    /// (<c>rencana-kerja/asuhan-keperawatan/tindakan-harian/tindakan-harian.md</c>).
    /// </summary>
    /// <remarks>
    /// Entity, konfigurasi, <c>DbSet</c>, dan seeder-nya masuk pada commit <c>06678c61</c> tanpa
    /// migration maupun entri snapshot, sehingga EF 9 menolak <c>database update</c> dengan
    /// <c>PendingModelChangesWarning</c> dan <c>MstDailyNursingActionSeeder</c> gagal saat startup.
    /// Isi <c>Up</c> diambil dari selisih yang dihasilkan EF sendiri terhadap model runtime.
    /// </remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260930120000_AddMasterDailyNursingAction")]
    public partial class AddMasterDailyNursingAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstDailyNursingAction",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ActionName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DefaultNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_MstDailyNursingAction", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstDailyNursingAction_ActionCode",
                schema: "public",
                table: "MstDailyNursingAction",
                column: "ActionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstDailyNursingAction_Category_IsActive_IsDelete",
                schema: "public",
                table: "MstDailyNursingAction",
                columns: new[] { "Category", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDailyNursingAction_SortOrder_ActionName",
                schema: "public",
                table: "MstDailyNursingAction",
                columns: new[] { "SortOrder", "ActionName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstDailyNursingAction",
                schema: "public");
        }
    }
}
