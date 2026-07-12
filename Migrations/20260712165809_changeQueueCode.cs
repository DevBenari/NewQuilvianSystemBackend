using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeQueueCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrxQueue_QueueCode",
                schema: "public",
                table: "TrxQueue");

            migrationBuilder.DropIndex(
                name: "IX_TrxQueue_QueueDate_ServiceUnitId_ClinicId_DoctorId_QueueNum~",
                schema: "public",
                table: "TrxQueue");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_QueueCode",
                schema: "public",
                table: "TrxQueue",
                column: "QueueCode");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_QueueDate_ServiceUnitId_ClinicId_QueueCode",
                schema: "public",
                table: "TrxQueue",
                columns: new[] { "QueueDate", "ServiceUnitId", "ClinicId", "QueueCode" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"ClinicId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_QueueDate_ServiceUnitId_ClinicId_QueueNumber",
                schema: "public",
                table: "TrxQueue",
                columns: new[] { "QueueDate", "ServiceUnitId", "ClinicId", "QueueNumber" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"ClinicId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_QueueDate_ServiceUnitId_QueueCode",
                schema: "public",
                table: "TrxQueue",
                columns: new[] { "QueueDate", "ServiceUnitId", "QueueCode" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"ClinicId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_QueueDate_ServiceUnitId_QueueNumber",
                schema: "public",
                table: "TrxQueue",
                columns: new[] { "QueueDate", "ServiceUnitId", "QueueNumber" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"ClinicId\" IS NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrxQueue_QueueCode",
                schema: "public",
                table: "TrxQueue");

            migrationBuilder.DropIndex(
                name: "IX_TrxQueue_QueueDate_ServiceUnitId_ClinicId_QueueCode",
                schema: "public",
                table: "TrxQueue");

            migrationBuilder.DropIndex(
                name: "IX_TrxQueue_QueueDate_ServiceUnitId_ClinicId_QueueNumber",
                schema: "public",
                table: "TrxQueue");

            migrationBuilder.DropIndex(
                name: "IX_TrxQueue_QueueDate_ServiceUnitId_QueueCode",
                schema: "public",
                table: "TrxQueue");

            migrationBuilder.DropIndex(
                name: "IX_TrxQueue_QueueDate_ServiceUnitId_QueueNumber",
                schema: "public",
                table: "TrxQueue");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_QueueCode",
                schema: "public",
                table: "TrxQueue",
                column: "QueueCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxQueue_QueueDate_ServiceUnitId_ClinicId_DoctorId_QueueNum~",
                schema: "public",
                table: "TrxQueue",
                columns: new[] { "QueueDate", "ServiceUnitId", "ClinicId", "DoctorId", "QueueNumber" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }
    }
}
