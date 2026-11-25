using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabOrderPlannedDateAndReschedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // BE-LAB-101 / 02-backend-architecture.md 33.7. Disunting tangan dari hasil
            // pembangkitan: EF mengusulkan NOT NULL dengan nilai bawaan 0001-01-01, yang akan
            // menempelkan tanggal rencana palsu pada setiap pesanan lama.
            //
            // Langkah 1 — kolom ditambahkan nullable lebih dulu.
            migrationBuilder.AddColumn<DateOnly>(
                name: "PlannedDate",
                schema: "public",
                table: "LabOrder",
                type: "date",
                nullable: true);

            // Langkah 2 — pesanan lama memakai tanggal waktu diminta MENURUT WIB (LAB-DEC-253),
            // bukan tanggal UTC: diminta 2026-10-08 18:30 UTC = 9 Oktober 01.30 WIB → 2026-10-09.
            // RequestedAt kosong pada pesanan sangat lama → waktu dibuat. Tidak menulis baris
            // LabOrderReschedule: riwayat hanya untuk keputusan manusia (33.10 no. 5).
            migrationBuilder.Sql(
                "UPDATE public.\"LabOrder\" " +
                "SET \"PlannedDate\" = (COALESCE(\"RequestedAt\", \"CreateDateTime\") AT TIME ZONE 'Asia/Jakarta')::date " +
                "WHERE \"PlannedDate\" IS NULL;");

            // Langkah 3 — wajib sesudah setiap baris terisi. Tanpa nilai bawaan basis data:
            // aplikasi yang mengisinya (hari ini WIB bila pemanggil tidak mengirim).
            migrationBuilder.AlterColumn<DateOnly>(
                name: "PlannedDate",
                schema: "public",
                table: "LabOrder",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "LabOrderReschedule",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LabOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousPlannedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    NewPlannedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_LabOrderReschedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabOrderReschedule_LabOrder_LabOrderId",
                        column: x => x.LabOrderId,
                        principalSchema: "public",
                        principalTable: "LabOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabOrder_PlannedDate",
                schema: "public",
                table: "LabOrder",
                column: "PlannedDate");

            migrationBuilder.CreateIndex(
                name: "IX_LabOrderReschedule_LabOrderId_OccurredAt",
                schema: "public",
                table: "LabOrderReschedule",
                columns: new[] { "LabOrderId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabOrderReschedule",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_LabOrder_PlannedDate",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropColumn(
                name: "PlannedDate",
                schema: "public",
                table: "LabOrder");
        }
    }
}
