using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Kontrak kejadian outbox modul Operasi (`BE-OPR-009`).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Seluruhnya additive: lima kolom pada <c>OprIntegrationDelivery</c> dan satu indeks unik.
    /// Tidak ada kolom yang dihapus, tidak ada tipe yang berubah, dan tidak ada tabel lain yang
    /// tersentuh.
    /// </para>
    /// <para>
    /// Baris yang sudah ada diisi nilai turunan, bukan dikosongkan: <c>EventId</c> diacak per
    /// baris supaya indeks uniknya tetap sah, <c>OccurredAt</c> mengikuti waktu pembuatan
    /// barisnya, dan <c>PayloadJson</c> diisi amplop minimal yang menandai bahwa baris itu
    /// dibuat sebelum kontrak ini berlaku. Menandainya penting: integration layer harus dapat
    /// membedakan pesan lama yang isinya memang belum lengkap dari pesan baru yang lengkap.
    /// </para>
    /// <para>
    /// Atribut <c>[Migration]</c> sengaja ditulis di berkas ini, bukan hanya di berkas Designer,
    /// supaya migration tetap terlihat EF ketika proyek dibangun dengan
    /// <c>-p:SkipMigrationMetadata=true</c>.
    /// </para>
    /// </remarks>
    [Microsoft.EntityFrameworkCore.Migrations.Migration("20260929000000_AddOperatingRoomOutboxEventContract")]
    public partial class AddOperatingRoomOutboxEventContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EventId",
                schema: "public",
                table: "OprIntegrationDelivery",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<string>(
                name: "EventType",
                schema: "public",
                table: "OprIntegrationDelivery",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EventVersion",
                schema: "public",
                table: "OprIntegrationDelivery",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "OccurredAt",
                schema: "public",
                table: "OprIntegrationDelivery",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "'-infinity'::timestamptz");

            migrationBuilder.AddColumn<string>(
                name: "PayloadJson",
                schema: "public",
                table: "OprIntegrationDelivery",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb");

            // Baris yang dibuat sebelum kontrak ini berlaku diberi identitas dan penanda,
            // supaya indeks unik di bawah dapat dibuat dan supaya pembacanya tahu bahwa isi
            // amplopnya memang belum pernah ada.
            migrationBuilder.Sql(@"
                UPDATE public.""OprIntegrationDelivery""
                SET ""EventId"" = gen_random_uuid(),
                    ""EventType"" = 'operating-room.legacy.unspecified',
                    ""EventVersion"" = '0',
                    ""OccurredAt"" = ""CreateDateTime"",
                    ""PayloadJson"" = jsonb_build_object(
                        'eventVersion', '0',
                        'payloadReference', ""PayloadReference"",
                        'note', 'Baris dibuat sebelum kontrak kejadian outbox berlaku.')
                WHERE ""EventId"" = '00000000-0000-0000-0000-000000000000'::uuid;");

            migrationBuilder.CreateIndex(
                name: "IX_OprIntegrationDelivery_EventId",
                schema: "public",
                table: "OprIntegrationDelivery",
                column: "EventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OprIntegrationDelivery_EventId",
                schema: "public",
                table: "OprIntegrationDelivery");

            migrationBuilder.DropColumn(
                name: "PayloadJson",
                schema: "public",
                table: "OprIntegrationDelivery");

            migrationBuilder.DropColumn(
                name: "OccurredAt",
                schema: "public",
                table: "OprIntegrationDelivery");

            migrationBuilder.DropColumn(
                name: "EventVersion",
                schema: "public",
                table: "OprIntegrationDelivery");

            migrationBuilder.DropColumn(
                name: "EventType",
                schema: "public",
                table: "OprIntegrationDelivery");

            migrationBuilder.DropColumn(
                name: "EventId",
                schema: "public",
                table: "OprIntegrationDelivery");
        }
    }
}
