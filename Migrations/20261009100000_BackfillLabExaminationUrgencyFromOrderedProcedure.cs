using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// BE-LAB-94, LAB-DEC-228 (diperluas LAB-REQ-022 butir 4). Migration DATA SAJA — nol perubahan
    /// skema, sehingga ApplicationDbContextModelSnapshot tidak berubah.
    ///
    /// Sampai BE-LAB-94, setiap pemeriksaan lahir Routine walau permintaannya Cito, dan wadah
    /// pengganti menghapus Tandai Cito (LAB-CONFLICT-019). Migration ini memulihkan pekerjaan yang
    /// MASIH BERJALAN: pesanan bukan Completed (5)/Cancelled (8), pemeriksaan bukan Voided (3)/
    /// Cancelled (4). Pesanan selesai sengaja tidak disentuh (sejalan VAL-04), sehingga laporan
    /// TAT masa lalu tidak bergeser.
    ///
    /// Baris hasil migration ini dikenali dari riwayat Examination.SetUrgency ber-ReasonCode
    /// 'LAB-CONFLICT-019' dengan pelaku sistem Guid.Empty. Setiap pernyataan idempoten: sesudah
    /// diubah, pemeriksaan tidak lagi Routine sehingga jalan kedua mengubah 0 baris.
    ///
    /// Enum yang dipakai (disimpan int): Urgency 1 Routine / 2 Cito; OrderedStatus 2 Fulfilled;
    /// ExaminationStatus 3 Voided / 4 Cancelled; OrderStatus 5 Completed / 8 Cancelled;
    /// LabTransitionScope 3 LabExamination.
    /// </remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261009100000_BackfillLabExaminationUrgencyFromOrderedProcedure")]
    public partial class BackfillLabExaminationUrgencyFromOrderedProcedure : Migration
    {
        private const string Penanda = "LAB-CONFLICT-019";
        private const string Sistem = "00000000-0000-0000-0000-000000000000";

        // Langkah 1-2: permintaan Cito yang pemeriksaan tertautnya lahir Routine dan tidak pernah
        // disentuh Tandai Cito (penanda kosong). Penanda tetap kosong sesudahnya (LAB-DEC-226).
        private const string TargetPermintaan = @"
            SELECT DISTINCT e.""Id"", e.""LabOrderId"", e.""SpecimenId"", o.""EncounterId""
            FROM public.""LabOrderedProcedure"" p
            JOIN public.""LabExamination"" e ON e.""Id"" = p.""FulfilledExaminationId""
            JOIN public.""LabOrder"" o ON o.""Id"" = e.""LabOrderId""
            WHERE NOT p.""IsDelete""
              AND p.""Urgency"" = 2
              AND p.""OrderedStatus"" = 2
              AND NOT e.""IsDelete""
              AND e.""Urgency"" = 1
              AND e.""UrgencyMarkedAt"" IS NULL
              AND e.""ExaminationStatus"" NOT IN (3, 4)
              AND NOT o.""IsDelete""
              AND o.""OrderStatus"" NOT IN (5, 8)";

        // Langkah 3 (LAB-REQ-022 butir 4): pemeriksaan di wadah pengganti yang kehilangan Cito.
        // Sumbernya dicari MUNDUR sepanjang rantai SupersededSpecimenId untuk prosedur yang sama,
        // berhenti pada pemeriksaan pertama yang bersikap — Cito, atau berpenanda (dokter pernah
        // memutuskan). Pemeriksaan perantara pada rantai A -> B -> C sudah Voided dan tidak
        // dipulihkan, sehingga membaca satu tingkat saja akan melewatkan C. Hanya sumber Cito yang
        // dipulihkan; pencabutan oleh dokter (Routine berpenanda) dihormati. Kedalaman dibatasi 20
        // sebagai pengaman bila data rantai rusak.
        private const string SumberPengganti = @"
            WITH RECURSIVE rantai AS (
                SELECT eb.""Id"" AS target_id, eb.""ProcedureId"" AS procedure_id,
                       sb.""SupersededSpecimenId"" AS cari_specimen, 1 AS kedalaman
                FROM public.""LabExamination"" eb
                JOIN public.""LabSpecimen"" sb ON sb.""Id"" = eb.""SpecimenId""
                JOIN public.""LabOrder"" o ON o.""Id"" = eb.""LabOrderId""
                WHERE sb.""SupersededSpecimenId"" IS NOT NULL
                  AND NOT sb.""IsDelete""
                  AND NOT eb.""IsDelete""
                  AND eb.""Urgency"" = 1
                  AND eb.""UrgencyMarkedAt"" IS NULL
                  AND eb.""ExaminationStatus"" NOT IN (3, 4)
                  AND NOT o.""IsDelete""
                  AND o.""OrderStatus"" NOT IN (5, 8)
                UNION ALL
                SELECT r.target_id, r.procedure_id, s.""SupersededSpecimenId"", r.kedalaman + 1
                FROM rantai r
                JOIN public.""LabSpecimen"" s ON s.""Id"" = r.cari_specimen
                WHERE s.""SupersededSpecimenId"" IS NOT NULL
                  AND r.kedalaman < 20
                  AND NOT EXISTS (
                      SELECT 1 FROM public.""LabExamination"" x
                      WHERE x.""SpecimenId"" = r.cari_specimen
                        AND x.""ProcedureId"" = r.procedure_id
                        AND NOT x.""IsDelete""
                        AND x.""ExaminationStatus"" <> 4
                        AND (x.""Urgency"" = 2 OR x.""UrgencyMarkedAt"" IS NOT NULL))
            ),
            bersikap AS (
                SELECT DISTINCT ON (r.target_id)
                       r.target_id, ea.""Urgency"" AS urgency,
                       ea.""UrgencyMarkedAt"" AS marked_at, ea.""UrgencyMarkedByUserId"" AS marked_by
                FROM rantai r
                JOIN public.""LabExamination"" ea
                  ON ea.""SpecimenId"" = r.cari_specimen
                 AND ea.""ProcedureId"" = r.procedure_id
                 AND NOT ea.""IsDelete""
                 AND ea.""ExaminationStatus"" <> 4
                WHERE ea.""Urgency"" = 2 OR ea.""UrgencyMarkedAt"" IS NOT NULL
                ORDER BY r.target_id, r.kedalaman
            )";

        private static string SisipRiwayat(string sumber, string catatan) => $@"
            INSERT INTO public.""LabTransitionHistory"" (
                ""Id"", ""LabOrderId"", ""LabSpecimenId"", ""LabExaminationId"", ""EncounterId"",
                ""Scope"", ""Action"", ""FromStatus"", ""ToStatus"", ""ReasonCode"", ""ReasonNote"",
                ""ActorUserId"", ""OccurredAt"", ""CorrelationId"",
                ""CreateDateTime"", ""CreateBy"", ""UpdateBy"", ""DeleteBy"", ""CancelBy"",
                ""IsCancel"", ""IsDelete"")
            SELECT
                gen_random_uuid(), t.""LabOrderId"", t.""SpecimenId"", t.""Id"", t.""EncounterId"",
                3, 'Examination.SetUrgency', 'Routine', 'Cito', '{Penanda}', '{catatan}',
                '{Sistem}'::uuid, NOW(), t.""LabOrderId"",
                NOW(), '{Sistem}'::uuid, '{Sistem}'::uuid, '{Sistem}'::uuid, '{Sistem}'::uuid,
                FALSE, FALSE
            FROM ({sumber}) t;";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Langkah 1 — riwayat lebih dulu, selagi predikat (Urgency = Routine) masih benar.
            migrationBuilder.Sql(SisipRiwayat(
                TargetPermintaan,
                "Perbaikan data: CITO dari pemesanan"));

            // Langkah 2 — predikat yang sama. Version dinaikkan karena ia token konkurensi:
            // penyimpanan yang memegang versi lama menerima 409 sekali, bukan menimpa diam-diam.
            migrationBuilder.Sql($@"
                UPDATE public.""LabExamination"" e
                SET ""Urgency"" = 2,
                    ""Version"" = e.""Version"" + 1,
                    ""UpdateDateTime"" = NOW(),
                    ""UpdateBy"" = '{Sistem}'::uuid
                WHERE e.""Id"" IN (SELECT t.""Id"" FROM ({TargetPermintaan}) t);");

            // Langkah 3 — sesudah langkah 1-2, sehingga pemeriksaan yang sudah pulih lewat
            // permintaan tidak lagi Routine dan tidak tersentuh dua kali.
            migrationBuilder.Sql(SumberPengganti + $@"
                INSERT INTO public.""LabTransitionHistory"" (
                    ""Id"", ""LabOrderId"", ""LabSpecimenId"", ""LabExaminationId"", ""EncounterId"",
                    ""Scope"", ""Action"", ""FromStatus"", ""ToStatus"", ""ReasonCode"", ""ReasonNote"",
                    ""ActorUserId"", ""OccurredAt"", ""CorrelationId"",
                    ""CreateDateTime"", ""CreateBy"", ""UpdateBy"", ""DeleteBy"", ""CancelBy"",
                    ""IsCancel"", ""IsDelete"")
                SELECT
                    gen_random_uuid(), e.""LabOrderId"", e.""SpecimenId"", e.""Id"", o.""EncounterId"",
                    3, 'Examination.SetUrgency', 'Routine', 'Cito', '{Penanda}',
                    'Perbaikan data: CITO dari pemeriksaan yang digantikan',
                    '{Sistem}'::uuid, NOW(), e.""LabOrderId"",
                    NOW(), '{Sistem}'::uuid, '{Sistem}'::uuid, '{Sistem}'::uuid, '{Sistem}'::uuid,
                    FALSE, FALSE
                FROM bersikap b
                JOIN public.""LabExamination"" e ON e.""Id"" = b.target_id
                JOIN public.""LabOrder"" o ON o.""Id"" = e.""LabOrderId""
                WHERE b.urgency = 2;");

            // Kesegeraan DAN penanda pemeriksaan yang digantikan disalin (LAB-DEC-227).
            migrationBuilder.Sql(SumberPengganti + $@"
                UPDATE public.""LabExamination"" e
                SET ""Urgency"" = 2,
                    ""UrgencyMarkedAt"" = b.marked_at,
                    ""UrgencyMarkedByUserId"" = b.marked_by,
                    ""Version"" = e.""Version"" + 1,
                    ""UpdateDateTime"" = NOW(),
                    ""UpdateBy"" = '{Sistem}'::uuid
                FROM bersikap b
                WHERE e.""Id"" = b.target_id
                  AND b.urgency = 2;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Hanya pemeriksaan yang SESUDAH migration ini tidak disentuh lagi lewat Tandai Cito
            // (tidak ada riwayat Examination.SetUrgency yang lebih baru) yang dikembalikan ke
            // keadaan semula: Routine, penanda kosong. Keputusan dokter sesudahnya dibiarkan,
            // berikut baris riwayat penandanya — riwayat itu tetap benar.
            migrationBuilder.Sql($@"
                WITH penanda AS (
                    SELECT h.""LabExaminationId"" AS examination_id, h.""OccurredAt"" AS occurred_at
                    FROM public.""LabTransitionHistory"" h
                    WHERE h.""ReasonCode"" = '{Penanda}'
                      AND h.""Action"" = 'Examination.SetUrgency'
                      AND h.""ActorUserId"" = '{Sistem}'::uuid
                ),
                dapat_dibalik AS (
                    SELECT p.examination_id
                    FROM penanda p
                    JOIN public.""LabExamination"" e ON e.""Id"" = p.examination_id
                    WHERE e.""Urgency"" = 2
                      AND NOT EXISTS (
                          SELECT 1 FROM public.""LabTransitionHistory"" h2
                          WHERE h2.""LabExaminationId"" = p.examination_id
                            AND h2.""Action"" = 'Examination.SetUrgency'
                            AND h2.""OccurredAt"" > p.occurred_at)
                ),
                dibalik AS (
                    UPDATE public.""LabExamination"" e
                    SET ""Urgency"" = 1,
                        ""UrgencyMarkedAt"" = NULL,
                        ""UrgencyMarkedByUserId"" = NULL,
                        ""Version"" = e.""Version"" + 1,
                        ""UpdateDateTime"" = NOW(),
                        ""UpdateBy"" = '{Sistem}'::uuid
                    WHERE e.""Id"" IN (SELECT examination_id FROM dapat_dibalik)
                    RETURNING e.""Id""
                )
                DELETE FROM public.""LabTransitionHistory"" h
                WHERE h.""ReasonCode"" = '{Penanda}'
                  AND h.""ActorUserId"" = '{Sistem}'::uuid
                  AND h.""LabExaminationId"" IN (SELECT ""Id"" FROM dibalik);");
        }
    }
}
