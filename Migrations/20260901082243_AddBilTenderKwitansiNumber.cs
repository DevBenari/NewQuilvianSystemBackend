using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Menambal satu-satunya selisih skema yang tersisa setelah `20260830151340_RepairPostCanonicalIntegration`
    /// di-baseline: kolom `BilTender.KwitansiNumber` beserta indexnya.
    ///
    /// Latar belakangnya begini. Basis data pengembangan dibangun dari rantai migration lama —
    /// `20260826034557_AddMedicalRecordIntegrityTables`, `20260826081755_AddMedicalRecordAccessAuditTables`,
    /// dan `20260828045534_AddBillingLatestChanges` — yang pada codebase sekarang sudah ditulis
    /// ulang menjadi `20260830151340`. Ketiganya masih tercatat di `__EFMigrationsHistory`
    /// meski berkasnya tidak ada lagi.
    ///
    /// Karena itu hampir seluruh isi `20260830151340` sudah terpasang lebih dulu di basis data
    /// tersebut, dan menjalankannya ulang justru gagal pada `CashierReferenceNote already exists`.
    /// Jalan keluarnya adalah men-baseline migration itu — mencatatnya sebagai applied tanpa
    /// menjalankannya. Hanya `KwitansiNumber` dan indexnya yang benar-benar tidak ikut terbawa
    /// rantai lama, sehingga cuma itu yang perlu ditambal di sini.
    ///
    /// Keduanya dijaga `IF NOT EXISTS` supaya migration ini juga aman pada basis data yang
    /// urutannya normal. Di sana `20260830151340` berjalan utuh dan sudah membuat kolom beserta
    /// indexnya, jadi migration ini lewat begitu saja sebagai no-op.
    ///
    /// Bentuk kolom dan indexnya disalin persis dari `20260830151340` supaya kedua jalur —
    /// basis data lama yang ditambal dan basis data baru yang lurus — bermuara pada skema yang
    /// sama: `character varying(50)`, nullable, dengan index non-unique satu kolom.
    /// </summary>
    public partial class AddBilTenderKwitansiNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE public."BilTender"
                    ADD COLUMN IF NOT EXISTS "KwitansiNumber" character varying(50) NULL;
                """);

            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_BilTender_KwitansiNumber"
                    ON public."BilTender" ("KwitansiNumber");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS public."IX_BilTender_KwitansiNumber";
                """);

            migrationBuilder.Sql("""
                ALTER TABLE public."BilTender" DROP COLUMN IF EXISTS "KwitansiNumber";
                """);
        }
    }
}
