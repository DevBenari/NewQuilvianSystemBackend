using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Mengubah kolom snapshot ledger fakta klinis dari <c>jsonb</c> menjadi <c>text</c>.
    ///
    /// PostgreSQL memformat ulang nilai <c>jsonb</c> ketika dibaca kembali, sehingga baris
    /// ledger tidak lagi identik dengan apa yang benar-benar dikirim ke Billing. Padahal sidik
    /// jari permintaan Billing dihitung dari untaian karakternya, sehingga pengiriman ulang
    /// dari baris tersimpan akan ditolak sebagai <c>BIL_IDEMPOTENCY_CONFLICT</c>. Kolom
    /// <c>text</c> menjaga bukti tetap sama persis.
    ///
    /// Konversi ditulis manual dengan klausa <c>USING</c> karena PostgreSQL tidak menyediakan
    /// cast otomatis dari <c>jsonb</c> ke <c>text</c> pada <c>ALTER COLUMN TYPE</c>.
    /// </summary>
    public partial class StoreClinicalFactSnapshotAsText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE public.\"TrxClinicalMilestoneFact\" " +
                "ALTER COLUMN \"TariffSnapshot\" TYPE text USING \"TariffSnapshot\"::text;");

            migrationBuilder.Sql(
                "ALTER TABLE public.\"TrxClinicalMilestoneFact\" " +
                "ALTER COLUMN \"RuleSnapshot\" TYPE text USING \"RuleSnapshot\"::text;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE public.\"TrxClinicalMilestoneFact\" " +
                "ALTER COLUMN \"TariffSnapshot\" TYPE jsonb USING \"TariffSnapshot\"::jsonb;");

            migrationBuilder.Sql(
                "ALTER TABLE public.\"TrxClinicalMilestoneFact\" " +
                "ALTER COLUMN \"RuleSnapshot\" TYPE jsonb USING \"RuleSnapshot\"::jsonb;");
        }
    }
}
