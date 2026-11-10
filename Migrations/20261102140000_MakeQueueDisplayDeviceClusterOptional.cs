using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// rawat-jalan Cetak Antrean Admisi v1.1 — display berlayout <c>AdmissionBoard</c> tidak terikat
    /// Nurse Station Cluster, sehingga <c>MstQueueDisplayDevice.NurseStationClusterId</c> boleh kosong.
    /// Data yang ada tidak berubah; FK dan index tetap.
    /// </summary>
    /// <inheritdoc />
    public partial class MakeQueueDisplayDeviceClusterOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "NurseStationClusterId",
                schema: "public",
                table: "MstQueueDisplayDevice",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Mengembalikan NOT NULL gagal bila sudah ada display Admisi tanpa cluster; pindahkan
            // atau hapus perangkat tersebut lebih dulu.
            migrationBuilder.AlterColumn<Guid>(
                name: "NurseStationClusterId",
                schema: "public",
                table: "MstQueueDisplayDevice",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
