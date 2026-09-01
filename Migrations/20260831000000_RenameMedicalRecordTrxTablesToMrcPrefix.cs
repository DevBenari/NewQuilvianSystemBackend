using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260831000000_RenameMedicalRecordTrxTablesToMrcPrefix")]
    public partial class RenameMedicalRecordTrxTablesToMrcPrefix : Migration
    {
        // Empat entitas operasional Rekam Medis pindah ke prefix registry `Mrc`.
        //
        // Prefix modul ini adalah `Mrc` menurut baris registry
        // `HealthServices | MedicalRecordManagement / Medical Record`, yang dinaikkan dari
        // `PLANNED` menjadi `ACTIVE` pada 31 Agustus 2026 oleh Yoga Aji Pratama.
        //
        // Kontrak menyatakan normalisasi `Trx*` belum selesai selama class, berkas,
        // configuration, DbSet, rujukan, dan tabel fisik belum dinormalkan bersama. Migration
        // ini pasangan fisik dari rename di sumber; tanpa ia, kode baru dan basis data lama
        // akan saling meleset.
        //
        // Polanya mengikuti empat migration rename IGD (`RenameEmergency*ToEmgPrefix`): nama
        // constraint dan index dicari dari katalog Postgres, tidak diketik satu per satu.
        // Sebagian nama buatan EF sudah terpotong pada batas 63 karakter dan berakhiran `~`,
        // sehingga mengetiknya manual mengundang salah ketik yang baru ketahuan saat migration
        // dijalankan.
        //
        // Tidak ada DROP+CREATE di sini (QBE-DB-002). Seluruhnya RENAME, sehingga isi tabel,
        // termasuk seluruh jejak akses rekam medis, tetap utuh.
        //
        // Tiga penggantian pertama sama panjang — `Trx` dan `Mrc` sama-sama tiga huruf —
        // sehingga tidak ada pemotongan baru maupun tabrakan nama. Yang keempat justru
        // memendek 13 huruf, dan dua nama yang semula terpotong kini muat utuh; keduanya
        // dirapikan pada langkah 4 supaya sepadan dengan nama yang diharapkan model EF.
        //
        // `MstMedicalRecordAccessPurpose` TIDAK ikut: ia master, prefix `Mst`-nya sudah benar
        // menurut registry. `TrxPatientEncounter` juga tidak: ia milik modul Registration, dan
        // menormalkannya batch tersendiri yang perlu jadwal dan koordinasi. Nama FK yang
        // memuatnya karena itu tetap menyebut `TrxPatientEncounter`.

        private const string Peta = @"
                'TrxClinicalDocumentIntegrity', 'MrcClinicalDocumentIntegrity',
                'TrxClinicalNoteAddendum', 'MrcClinicalNoteAddendum',
                'TrxClinicalNoteAuthorDelegation', 'MrcClinicalNoteAuthorDelegation',
                'TrxMedicalRecordAccessLog', 'MrcAccessLog'";

        private const string PetaBalik = @"
                'MrcClinicalDocumentIntegrity', 'TrxClinicalDocumentIntegrity',
                'MrcClinicalNoteAddendum', 'TrxClinicalNoteAddendum',
                'MrcClinicalNoteAuthorDelegation', 'TrxClinicalNoteAuthorDelegation',
                'MrcAccessLog', 'TrxMedicalRecordAccessLog'";

        // Langkah 4 pada arah maju: dua nama yang semula terpotong `~` kini muat utuh.
        private const string RapikanMaju = @"
                'FK_MrcAccessLog_MstMedicalRecordAccessPurpose_Acc~',
                'FK_MrcAccessLog_MstMedicalRecordAccessPurpose_AccessPurposeId',
                'IX_MrcAccessLog_IsFlaggedForReview_ReviewedAt_Acc~',
                'IX_MrcAccessLog_IsFlaggedForReview_ReviewedAt_AccessedAt'";

        // Arah balik: dipendekkan lagi supaya penggantian nama tabel dapat mengenalinya.
        private const string RapikanBalik = @"
                'FK_MrcAccessLog_MstMedicalRecordAccessPurpose_AccessPurposeId',
                'FK_MrcAccessLog_MstMedicalRecordAccessPurpose_Acc~',
                'IX_MrcAccessLog_IsFlaggedForReview_ReviewedAt_AccessedAt',
                'IX_MrcAccessLog_IsFlaggedForReview_ReviewedAt_Acc~'";

        private static string Skrip(string peta) => $$"""
            DO $qbe$
            DECLARE
                peta CONSTANT text[] := ARRAY[{{peta}}
                ];
                lama text;
                baru text;
                i int;
                r record;
            BEGIN
                -- 1. Nama tabel.
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];
                    IF EXISTS (
                        SELECT 1 FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public' AND c.relname = lama AND c.relkind = 'r'
                    ) THEN
                        EXECUTE format('ALTER TABLE public.%I RENAME TO %I', lama, baru);
                    END IF;
                END LOOP;

                -- 2. Constraint mana pun yang namanya memuat nama tabel lama, termasuk FK
                --    milik tabel lain yang menunjuk ke sini. Mengganti nama constraint
                --    PK/unique sekaligus mengganti nama index penopangnya.
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];
                    FOR r IN
                        SELECT t.relname AS tabel, c.conname AS nama
                        FROM pg_constraint c
                        JOIN pg_class t ON t.oid = c.conrelid
                        JOIN pg_namespace n ON n.oid = t.relnamespace
                        WHERE n.nspname = 'public' AND c.conname LIKE '%' || lama || '%'
                    LOOP
                        EXECUTE format(
                            'ALTER TABLE public.%I RENAME CONSTRAINT %I TO %I',
                            r.tabel, r.nama, replace(r.nama, lama, baru));
                    END LOOP;
                END LOOP;

                -- 3. Sisa index yang tidak ditopang constraint.
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];
                    FOR r IN
                        SELECT c.relname AS nama
                        FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public' AND c.relkind = 'i'
                          AND c.relname LIKE '%' || lama || '%'
                    LOOP
                        EXECUTE format('ALTER INDEX public.%I RENAME TO %I',
                                       r.nama, replace(r.nama, lama, baru));
                    END LOOP;
                END LOOP;
            END
            $qbe$;
            """;

        /// <summary>
        /// Langkah 4: memulihkan nama yang semula terpotong batas 63 karakter.
        ///
        /// Dijalankan setelah penggantian nama tabel, karena barulah pada saat itu namanya
        /// berbentuk `..._Acc~` dengan awalan yang sudah baru.
        /// </summary>
        private static string SkripRapikan(string peta) => $$"""
            DO $qbe$
            DECLARE
                peta CONSTANT text[] := ARRAY[{{peta}}
                ];
                lama text;
                baru text;
                i int;
                r record;
            BEGIN
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];

                    FOR r IN
                        SELECT t.relname AS tabel
                        FROM pg_constraint c
                        JOIN pg_class t ON t.oid = c.conrelid
                        JOIN pg_namespace n ON n.oid = t.relnamespace
                        WHERE n.nspname = 'public' AND c.conname = lama
                    LOOP
                        EXECUTE format(
                            'ALTER TABLE public.%I RENAME CONSTRAINT %I TO %I',
                            r.tabel, lama, baru);
                    END LOOP;

                    IF EXISTS (
                        SELECT 1 FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public' AND c.relkind = 'i' AND c.relname = lama
                    ) THEN
                        EXECUTE format('ALTER INDEX public.%I RENAME TO %I', lama, baru);
                    END IF;
                END LOOP;
            END
            $qbe$;
            """;

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(Skrip(Peta));
            migrationBuilder.Sql(SkripRapikan(RapikanMaju));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Urutannya dibalik: nama dipendekkan lebih dulu supaya penggantian nama tabel
            // pada langkah berikutnya dapat mengenali polanya.
            migrationBuilder.Sql(SkripRapikan(RapikanBalik));
            migrationBuilder.Sql(Skrip(PetaBalik));
        }
    }
}
