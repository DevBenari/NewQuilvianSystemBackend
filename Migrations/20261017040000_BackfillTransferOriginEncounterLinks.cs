using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// BE-BKC-087, BKC-DEC-160, BKC-DEC-163, BKC-DES-088, BIL-INT-022.
    /// Migration DATA SAJA — nol perubahan skema, ApplicationDbContextModelSnapshot tidak berubah.
    ///
    /// Menautkan kunjungan transfer IGD/Poli lama ke invoice Rawat Inap (BilInvoiceEncounterLink)
    /// yang terjadi sebelum penulis runtime BE-BKC-082 dirilis.
    ///
    /// Kriteria 100% identik dengan runtime BE-BKC-082 (BKC-DES-077/079) dan skrip dry-run BE-BKC-086:
    /// 1. Kunjungan rawat inap E memiliki SourceEncounterId dan AdmissionTransferSourceType IN ('Emergency', 'Outpatient') serta tidak dihapus.
    /// 2. Kunjungan asal S cocok pasiennya (S.PatientId = E.PatientId), tidak dihapus, dan tidak dibatalkan (IsCancel = false).
    /// 3. Ada invoice rawat inap aktif R (ServiceType = 'RANAP', !IsDelete).
    /// 4. Ada tanda terima ADMISSION_CONFIRMED yang valid T (Outcome IN ('INVOICE_OPENED', 'INVOICE_ALREADY_OPEN')).
    ///    Bila lebih dari satu, dipilih ReceivedAtUtc paling awal.
    /// 5. Belum ada tautan aktif untuk pasangan (R, S).
    ///
    /// Nilai kolom audit:
    /// - ReceiptId = T.Id
    /// - LinkedAt = T.ReceivedAtUtc (waktu tanda terima asli, BUKAN waktu migration dijalankan)
    /// - CreateBy = '7b1f0163-0000-4000-8000-000000000163' (penanda backfill resmi)
    ///
    /// Idempoten: pernyataan aman diulang, jalan kedua menyisipkan 0 baris.
    /// Down: menghapus secara selektif baris dengan CreateBy penanda backfill di atas saja.
    /// </remarks>
    public partial class BackfillTransferOriginEncounterLinks : Migration
    {
        public const string BackfillCreatorGuid = "7b1f0163-0000-4000-8000-000000000163";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                WITH first_receipt AS (
                    SELECT
                        t.""Id"" AS receipt_id,
                        t.""EncounterId"" AS encounter_id,
                        t.""InvoiceId"" AS invoice_id,
                        t.""ReceivedAtUtc"" AS received_at_utc,
                        ROW_NUMBER() OVER (
                            PARTITION BY t.""EncounterId"", t.""InvoiceId""
                            ORDER BY t.""ReceivedAtUtc"" ASC, t.""Id"" ASC
                        ) AS rn
                    FROM public.""BilInpatientEventReceipt"" t
                    WHERE t.""EventType"" = 'ADMISSION_CONFIRMED'
                      AND t.""Outcome"" IN ('INVOICE_OPENED', 'INVOICE_ALREADY_OPEN')
                      AND t.""IsDelete"" = false
                )
                INSERT INTO public.""BilInvoiceEncounterLink"" (
                    ""Id"",
                    ""RanapInvoiceId"",
                    ""LinkedEncounterId"",
                    ""LinkReason"",
                    ""SourceReferralId"",
                    ""ReceiptId"",
                    ""LinkedAt"",
                    ""CreateDateTime"",
                    ""CreateBy"",
                    ""UpdateDateTime"",
                    ""UpdateBy"",
                    ""DeleteDateTime"",
                    ""DeleteBy"",
                    ""IsDelete"",
                    ""IsCancel"",
                    ""CancelDateTime"",
                    ""CancelBy""
                )
                SELECT
                    gen_random_uuid(),
                    r.""Id"",
                    s.""Id"",
                    CASE
                        WHEN e.""AdmissionTransferSourceType"" = 'Emergency' THEN 'EMERGENCY_ORIGIN'
                        WHEN e.""AdmissionTransferSourceType"" = 'Outpatient' THEN 'OUTPATIENT_ORIGIN'
                        ELSE 'UNKNOWN_ORIGIN'
                    END,
                    NULL,
                    fr.receipt_id,
                    fr.received_at_utc,
                    NOW(),
                    '{BackfillCreatorGuid}'::uuid,
                    NULL,
                    NULL,
                    NULL,
                    NULL,
                    false,
                    false,
                    NULL,
                    NULL
                FROM public.""RegPatientEncounter"" e
                JOIN public.""RegPatientEncounter"" s
                    ON s.""Id"" = e.""SourceEncounterId""
                   AND s.""PatientId"" = e.""PatientId""
                   AND s.""IsDelete"" = false
                   AND s.""IsCancel"" = false
                JOIN public.""BilInvoice"" r
                    ON r.""EncounterId"" = e.""Id""
                   AND r.""ServiceType"" = 'RANAP'
                   AND r.""IsDelete"" = false
                JOIN first_receipt fr
                    ON fr.encounter_id = e.""Id""
                   AND fr.invoice_id = r.""Id""
                   AND fr.rn = 1
                WHERE e.""SourceEncounterId"" IS NOT NULL
                  AND e.""AdmissionTransferSourceType"" IN ('Emergency', 'Outpatient')
                  AND e.""IsDelete"" = false
                  AND NOT EXISTS (
                      SELECT 1
                      FROM public.""BilInvoiceEncounterLink"" l
                      WHERE l.""RanapInvoiceId"" = r.""Id""
                        AND l.""LinkedEncounterId"" = s.""Id""
                        AND l.""IsDelete"" = false
                  );");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DELETE FROM public.""BilInvoiceEncounterLink""
                WHERE ""CreateBy"" = '{BackfillCreatorGuid}'::uuid;");
        }
    }
}
