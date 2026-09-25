using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    /// <summary>
    /// Telaah obat akhir oleh apoteker — gerbang wajib sebelum obat boleh diserahkan.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Telaah ini wajib bagi SELURUH resep. Penyiapan yang selesai membawa resep ke
    /// <c>AwaitingFinalCheck</c>, dan hanya telaah inilah yang boleh menetapkan
    /// <c>ReadyToDispense</c>. Sebelumnya penyiapan langsung menetapkan <c>ReadyToDispense</c>
    /// sementara telaah ini justru mensyaratkannya, sehingga telaah berjalan SESUDAH gerbang
    /// dan bukan sebagai gerbang.
    /// </para>
    /// <para>
    /// Telaah yang gagal TIDAK menolak resepnya. Resep tetap sah secara klinis dan sudah lolos
    /// telaah farmasi; yang tidak lolos adalah obat yang disiapkan. Karena itu resep kembali ke
    /// <c>InPreparation</c> dan penyiapannya dibuka lagi untuk dikoreksi, bukan ditandai
    /// <c>Rejected</c>.
    /// </para>
    /// <para>
    /// Setiap percobaan telaah disimpan sebagai BARIS SENDIRI. Percobaan sebelumnya ditutup
    /// dengan <c>IsActive = false</c> dan dibiarkan utuh — tidak dihapus, tidak ditimpa — supaya
    /// bukti telaah yang gagal tetap dapat ditelusuri. Model saat ini belum punya kolom nomor
    /// percobaan; urutannya terbaca dari <c>StartedAt</c> dan <c>CreateDateTime</c>.
    /// </para>
    /// </remarks>
    public class PrescriptionFinalCheckService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly PrescriptionFinancialClearanceService _financialClearanceService;

        public PrescriptionFinalCheckService(
            ApplicationDbContext dbContext,
            PrescriptionFinancialClearanceService financialClearanceService)
        {
            _dbContext = dbContext;
            _financialClearanceService = financialClearanceService;
        }

        public async Task<PrescriptionFinalCheckResponse> CompleteAsync(Guid prescriptionId, CompletePrescriptionFinalCheckRequest request, Guid actorUserId, CancellationToken ct = default)
        {
            var prescription = await _dbContext.Set<PhmPrescription>()
                .FirstOrDefaultAsync(x => x.Id == prescriptionId && !x.IsDelete, ct)
                ?? throw new InvalidOperationException("Resep tidak ditemukan.");

            if (prescription.FulfillmentStatus != PrescriptionFulfillmentStatus.AwaitingFinalCheck)
                throw new InvalidOperationException(
                    "Telaah obat akhir hanya dapat dilakukan ketika resep menunggu telaah obat akhir.");

            if (request.Items.Count == 0)
                throw new InvalidOperationException("Kriteria telaah obat akhir wajib diisi.");

            // Gerbang finansial ketiga dari empat (PHA-BE-005). Telaah akhir yang lolos akan
            // menetapkan ReadyToDispense, sehingga izin yang sudah dicabut harus menghentikannya
            // di sini — bukan nanti di meja penyerahan.
            await _financialClearanceService.EnsureGateAllowedAsync(
                prescriptionId, PrescriptionClearanceGate.FinalCheck, ct);

            var now = DateTime.UtcNow;

            // Percobaan sebelumnya ditutup, bukan dipakai ulang. Isinya dibiarkan apa adanya
            // supaya hasil telaah yang gagal tetap tersimpan sebagai riwayat.
            var previousAttempts = await _dbContext.Set<TrxPrescriptionFinalCheck>()
                .Where(x => x.PrescriptionId == prescriptionId && !x.IsDelete && x.IsActive)
                .ToListAsync(ct);

            foreach (var attempt in previousAttempts)
            {
                attempt.IsActive = false;
                attempt.UpdateDateTime = now;
                attempt.UpdateBy = actorUserId;
            }

            var finalCheck = new TrxPrescriptionFinalCheck
            {
                Id = Guid.NewGuid(),
                PrescriptionId = prescriptionId,
                Status = PrescriptionFinalCheckStatus.InReview,
                CheckedByUserId = actorUserId,
                StartedAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsActive = true
            };
            _dbContext.Set<TrxPrescriptionFinalCheck>().Add(finalCheck);

            foreach (var input in request.Items)
            {
                finalCheck.Items.Add(new TrxPrescriptionFinalCheckItem
                {
                    Id = Guid.NewGuid(),
                    PrescriptionFinalCheckId = finalCheck.Id,
                    CriterionCode = input.CriterionCode.Trim(),
                    CriterionName = input.CriterionName.Trim(),
                    Result = input.Result,
                    Finding = Normalize(input.Finding),
                    SortOrder = input.SortOrder,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsActive = true
                });
            }

            var passed = request.Items.All(x => x.Result is PrescriptionReviewResult.Compliant or PrescriptionReviewResult.NotApplicable or PrescriptionReviewResult.WarningAccepted);

            finalCheck.Status = passed ? PrescriptionFinalCheckStatus.Passed : PrescriptionFinalCheckStatus.Failed;
            finalCheck.CheckedByUserId = actorUserId;
            finalCheck.CompletedAt = now;
            finalCheck.CheckNote = Normalize(request.CheckNote);
            finalCheck.UpdateDateTime = now;
            finalCheck.UpdateBy = actorUserId;

            if (passed)
            {
                prescription.FulfillmentStatus = PrescriptionFulfillmentStatus.ReadyToDispense;
            }
            else
            {
                // Gagal telaah berarti obatnya perlu disiapkan ulang, bukan resepnya ditolak.
                prescription.FulfillmentStatus = PrescriptionFulfillmentStatus.InPreparation;

                // Penyiapan dibuka kembali supaya petugas farmasi dapat mengoreksi itemnya lalu
                // menyelesaikannya lagi. Baris penyiapannya tidak dihapus.
                var preparation = await _dbContext.Set<TrxPrescriptionPreparation>()
                    .FirstOrDefaultAsync(x => x.PrescriptionId == prescriptionId && !x.IsDelete && x.IsActive, ct);

                if (preparation != null && preparation.Status == PrescriptionPreparationStatus.Prepared)
                {
                    preparation.Status = PrescriptionPreparationStatus.InPreparation;
                    preparation.PreparationCompletedAt = null;
                    preparation.UpdateDateTime = now;
                    preparation.UpdateBy = actorUserId;
                }
            }

            prescription.UpdateDateTime = now;
            prescription.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(ct);
            return Map(finalCheck);
        }

        private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static PrescriptionFinalCheckResponse Map(TrxPrescriptionFinalCheck x) => new()
        {
            Id = x.Id, PrescriptionId = x.PrescriptionId, Status = x.Status,
            CheckedByUserId = x.CheckedByUserId, StartedAt = x.StartedAt,
            CompletedAt = x.CompletedAt, CheckNote = x.CheckNote,
            Items = x.Items.Where(i => !i.IsDelete && i.IsActive).OrderBy(i => i.SortOrder).Select(i => new PrescriptionFinalCheckItemResponse
            {
                Id = i.Id, CriterionCode = i.CriterionCode, CriterionName = i.CriterionName,
                Result = i.Result, Finding = i.Finding, SortOrder = i.SortOrder
            }).ToList()
        };
    }
}
