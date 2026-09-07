using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class RenameLaboratoryTrxTablesToLabPrefix : Migration
    {
        // Dua entitas operasional Laboratorium pindah ke prefix registry `Lab`.
        //
        // Prefix modul ini adalah `Lab` menurut baris registry
        // `HealthServices | LaboratoryManagement / Laboratory`, yang dinaikkan dari `PLANNED`
        // menjadi `ACTIVE` pada 2 September 2026. Seluruh entity Laboratorium yang dibuat
        // sesudahnya sudah memakai prefix itu — `LabOrder`, `LabExamination`, `LabValueBound`,
        // dan seterusnya. Dua tabel ini tertinggal karena lahir lebih dulu.
        //
        // Kontrak menyatakan normalisasi `Trx*` belum selesai selama class, berkas,
        // configuration, DbSet, seluruh rujukan, dan tabel fisik belum dinormalkan bersama
        // (QBE-NAM-003). Migration ini pasangan fisik dari rename di sumber; tanpa ia, kode baru
        // dan basis data lama akan saling meleset.
        //
        // Polanya mengikuti `20260831000000_RenameMedicalRecordTrxTablesToMrcPrefix`: nama
        // constraint dan index dicari dari katalog Postgres, tidak diketik satu per satu.
        // Mengetiknya manual mengundang salah ketik yang baru ketahuan saat migration
        // dijalankan.
        //
        // Tidak ada DROP+CREATE di sini (QBE-DB-002). Seluruhnya RENAME, sehingga isi tabel
        // tetap utuh — walaupun pada dev pemilik modul jumlah barisnya kebetulan nol.
        // EF sendiri menghasilkan DROP+CREATE untuk perubahan ini dan memperingatkan
        // "may result in the loss of data"; badan migration itu sengaja diganti.
        //
        // Kedua nama memendek tiga huruf, dan nama terpanjang yang terdampak
        // (`FK_TrxLabTransitionHistory_TrxPatientEncounter_EncounterId`, 58 huruf) masih di
        // bawah batas 63 huruf Postgres. Karena itu tidak ada nama terpotong `~` yang perlu
        // dirapikan, tidak seperti pada migration Rekam Medis.
        //
        // `MstLabRejectionReason` TIDAK ikut: ia master, dan catatan registry 2 September 2026
        // menetapkannya diperlakukan legacy dan tidak dinamai ulang. `TrxPatientEncounter` juga
        // tidak: ia milik modul Registration, dan menormalkannya batch tersendiri. Nama FK yang
        // memuatnya karena itu tetap menyebut `TrxPatientEncounter`.

        private const string Peta = @"
                'TrxLabSpecimen', 'LabSpecimen',
                'TrxLabTransitionHistory', 'LabTransitionHistory'";

        private const string PetaBalik = @"
                'LabSpecimen', 'TrxLabSpecimen',
                'LabTransitionHistory', 'TrxLabTransitionHistory'";

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

                -- 2. Constraint mana pun yang namanya memuat nama tabel lama, termasuk FK milik
                --    tabel lain yang menunjuk ke sini — misalnya
                --    FK_LabExamination_TrxLabSpecimen_SpecimenId pada LabExamination. Mengganti
                --    nama constraint PK/unique sekaligus mengganti nama index penopangnya.
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

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(Skrip(Peta));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(Skrip(PetaBalik));
        }
    }
}
