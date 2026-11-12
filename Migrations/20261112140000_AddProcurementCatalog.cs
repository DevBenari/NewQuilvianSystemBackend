using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-PBJ-003 (PBJ-DEC-022, 024, 037): master Pengadaan — jenis pengadaan, kategori, sumber dana,
    /// dan katalog barang/jasa. Empat tabel baru; FK katalog ke MstMeasurement dan MstDrug memakai
    /// Restrict. Tidak mengubah tabel lain dan aman tanpa downtime. Data awal diisi seeder idempoten
    /// ProcurementMasterDataSeeder, bukan oleh migration ini.
    /// </summary>
    public partial class AddProcurementCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstProcurementType",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TypeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsGoods = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MstProcurementType", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstProcurementType_TypeCode",
                schema: "public",
                table: "MstProcurementType",
                column: "TypeCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateTable(
                name: "MstProcurementItemCategory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CategoryName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MstProcurementItemCategory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstProcurementItemCategory_CategoryCode",
                schema: "public",
                table: "MstProcurementItemCategory",
                column: "CategoryCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateTable(
                name: "MstProcurementFundSource",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FundSourceCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FundSourceName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MstProcurementFundSource", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstProcurementFundSource_FundSourceCode",
                schema: "public",
                table: "MstProcurementFundSource",
                column: "FundSourceCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateTable(
                name: "MstProcurementItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProcurementTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    MeasurementId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefaultSpecification = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_MstProcurementItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstProcurementItem_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstProcurementItem_MstMeasurement_MeasurementId",
                        column: x => x.MeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstProcurementItem_MstProcurementItemCategory_ItemCategoryId",
                        column: x => x.ItemCategoryId,
                        principalSchema: "public",
                        principalTable: "MstProcurementItemCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstProcurementItem_MstProcurementType_ProcurementTypeId",
                        column: x => x.ProcurementTypeId,
                        principalSchema: "public",
                        principalTable: "MstProcurementType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstProcurementItem_DrugId",
                schema: "public",
                table: "MstProcurementItem",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcurementItem_ItemCategoryId",
                schema: "public",
                table: "MstProcurementItem",
                column: "ItemCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcurementItem_ItemCode",
                schema: "public",
                table: "MstProcurementItem",
                column: "ItemCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstProcurementItem_ItemName",
                schema: "public",
                table: "MstProcurementItem",
                column: "ItemName");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcurementItem_MeasurementId",
                schema: "public",
                table: "MstProcurementItem",
                column: "MeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcurementItem_ProcurementTypeId",
                schema: "public",
                table: "MstProcurementItem",
                column: "ProcurementTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "MstProcurementItem", schema: "public");
            migrationBuilder.DropTable(name: "MstProcurementFundSource", schema: "public");
            migrationBuilder.DropTable(name: "MstProcurementItemCategory", schema: "public");
            migrationBuilder.DropTable(name: "MstProcurementType", schema: "public");
        }
    }
}
