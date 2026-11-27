using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-PBJ-PS-003 (pemilihan-penyedia, urutan migration 9 di <c>02-module-map.md</c>; menyentuh tabel
    /// perencanaan-permintaan atas <c>PBJ-DEC-095</c>): kolom <c>PackageId</c> dan <c>PackagedAt</c> pada
    /// <c>PrcPurchaseRequestLine</c> beserta index <c>PackageId</c>. Keduanya nullable, sehingga baris PR lama tetap kosong
    /// dan migration aman dijalankan tanpa henti layanan.
    /// </summary>
    public partial class AddProcurementPurchaseRequestLinePackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PackageId",
                schema: "public",
                table: "PrcPurchaseRequestLine",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PackagedAt",
                schema: "public",
                table: "PrcPurchaseRequestLine",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrcPurchaseRequestLine_PackageId",
                schema: "public",
                table: "PrcPurchaseRequestLine",
                column: "PackageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PrcPurchaseRequestLine_PackageId",
                schema: "public",
                table: "PrcPurchaseRequestLine");

            migrationBuilder.DropColumn(
                name: "PackageId",
                schema: "public",
                table: "PrcPurchaseRequestLine");

            migrationBuilder.DropColumn(
                name: "PackagedAt",
                schema: "public",
                table: "PrcPurchaseRequestLine");
        }
    }
}
