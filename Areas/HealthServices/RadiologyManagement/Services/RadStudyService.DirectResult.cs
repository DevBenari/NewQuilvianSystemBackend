using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Simpan Form Hasil v1 tanpa konsol pemeriksaan — <c>RAD-DEC-023</c> (keputusan pemilik modul
    /// 2026-10-19): verifikasi pasien, gerbang keselamatan, dan akuisisi tidak lagi dijalankan;
    /// petugas langsung mengisi hasil seperti di v1, dan hasil itu langsung final.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Yang <b>tetap dijaga</b> walau konsolnya hilang:
    /// </para>
    /// <list type="bullet">
    /// <item><b>Tagihan.</b> Fakta kelayakan tagih yang sebelumnya terbit saat mutu citra diterima
    /// kini terbit saat hasil pertama disimpan — sekali per study, idempoten lewat
    /// <c>BillingFactSubmitted</c> dan kunci produsen fakta.</item>
    /// <item><b>Jejak.</b> Study yang lahir atau dilompati tercatat di riwayat dengan kode
    /// <c>RAD-DEC-023</c>, sehingga pemeriksaan tanpa gerbang keselamatan dapat dibedakan.</item>
    /// <item><b>Bacaan berversi.</b> Lihat <see cref="RadReportService.SaveFinalResultAsync"/>.</item>
    /// </list>
    /// </remarks>
    public partial class RadStudyService
    {
        internal const string DirectResultReasonCode = "RAD-DEC-023";

        private static readonly RadStudyStatus[] StatusBelumSelesai =
        {
            RadStudyStatus.Planned,
            RadStudyStatus.PatientVerified,
            RadStudyStatus.SafetyCleared,
            RadStudyStatus.AcquisitionStarted,
            RadStudyStatus.Acquired,
            RadStudyStatus.OnHold,
        };

        public async Task<RadOperationResult<RadReportDetailResponse>> SaveDirectResultAsync(
            Guid radOrderId,
            RadOrderResultRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var findings = request.Findings?.Trim() ?? string.Empty;
            if (findings.Length == 0)
            {
                return RadOperationResult<RadReportDetailResponse>.Validation(
                    RadErrorCodes.ValidationFailed, "Hasil pemeriksaan radiologi wajib diisi.");
            }

            // v1: Catatan Tambahan opsional. Kesimpulan V2 wajib — tanpa catatan, hasilnya sendiri
            // yang menjadi kesimpulan yang dibaca dokter pengirim.
            var catatan = request.Notes?.Trim();
            var impression = string.IsNullOrEmpty(catatan) ? findings : catatan;
            if (impression.Length > 4000)
            {
                return RadOperationResult<RadReportDetailResponse>.Validation(
                    RadErrorCodes.ValidationFailed,
                    string.IsNullOrEmpty(catatan)
                        ? "Hasil lebih dari 4.000 huruf. Isi Catatan Tambahan sebagai kesimpulan singkat."
                        : "Catatan tambahan terlalu panjang, maksimal 4.000 huruf.");
            }

            var siap = await EnsureDirectResultStudyAsync(radOrderId, cancellationToken);
            if (siap.Kind != RadOperationResultKind.Success)
            {
                return RadOperationResult<RadReportDetailResponse>.From(siap);
            }

            var study = siap.Value!;
            await TerbitkanTagihanBilaBelumAsync(study, cancellationToken);

            return await _radReportService.SaveFinalResultAsync(study.Id, findings, impression, cancellationToken);
        }

        /// <summary>
        /// Study tempat hasil ditulis: study layak yang sudah ada, study alur lama yang belum
        /// selesai (dilompati ke layak), atau study baru yang langsung layak.
        /// </summary>
        private async Task<RadOperationResult<RadStudy>> EnsureDirectResultStudyAsync(
            Guid radOrderId,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            await using var transaksi = await MulaiTransaksiBilaPerluAsync(cancellationToken);

            var order = await _dbContext.RadOrders
                .Include(x => x.Studies.Where(s => !s.IsDelete))
                .FirstOrDefaultAsync(x => x.Id == radOrderId && !x.IsDelete, cancellationToken);

            if (order == null)
            {
                return RadOperationResult<RadStudy>.NotFound(RadErrorCodes.OrderNotFound, "Pesanan radiologi tidak ditemukan.");
            }

            switch (order.OrderStatus)
            {
                case RadOrderStatus.Cancelled or RadOrderStatus.Rejected or RadOrderStatus.CancelRequested:
                    return RadOperationResult<RadStudy>.Validation(
                        RadErrorCodes.InvalidTransition, "Pesanan yang dibatalkan atau ditolak tidak dapat diisi hasilnya.");
                case RadOrderStatus.Draft or RadOrderStatus.Requested:
                    return RadOperationResult<RadStudy>.Validation(
                        RadErrorCodes.InvalidTransition, "Konfirmasi pesanan terlebih dahulu di Detail Pasien sebelum mengisi hasil.");
                case RadOrderStatus.OnHold:
                    return RadOperationResult<RadStudy>.Validation(
                        RadErrorCodes.InvalidTransition, "Pesanan sedang ditahan. Lanjutkan pesanan lebih dulu sebelum mengisi hasil.");
            }

            var layak = order.Studies
                .Where(s => s.IsUsable == true && s.StudyStatus == RadStudyStatus.QualityAccepted)
                .OrderByDescending(s => s.StudySequence)
                .FirstOrDefault();

            if (layak == null)
            {
                layak = order.Studies
                    .Where(s => StatusBelumSelesai.Contains(s.StudyStatus))
                    .OrderByDescending(s => s.StudySequence)
                    .FirstOrDefault();

                if (layak == null)
                {
                    layak = new RadStudy
                    {
                        RadOrderId = order.Id,
                        EncounterId = order.EncounterId,
                        ProcedureId = order.ProcedureId,
                        ModalityId = order.ModalityId,
                        StudySequence = order.Studies.Count == 0 ? 1 : order.Studies.Max(x => x.StudySequence) + 1,
                        StudyNumber = await _radStudyNumberService.AllocateOneAsync(cancellationToken),
                        StudyStatus = RadStudyStatus.Planned,
                        CreateBy = actorUserId,
                        CreateDateTime = now,
                    };
                    _dbContext.RadStudies.Add(layak);
                    AddHistory(order, layak, "Study.Create", null, RadStudyStatus.Planned.ToString(),
                        DirectResultReasonCode, "Dibuat dari Form Hasil", actorUserId, now);
                }

                var dari = layak.StudyStatus;
                layak.StudyStatus = RadStudyStatus.QualityAccepted;
                layak.StatusBeforeHold = null;
                layak.AcquiredAt ??= now;
                layak.IsUsable = true;
                layak.QualityDecidedAt = now;
                layak.QualityDecidedByUserId = actorUserId;
                layak.QualityNote = "Hasil diisi langsung lewat Form Hasil, tanpa konsol pemeriksaan (RAD-DEC-023).";
                if (layak.CreateDateTime != now)
                {
                    Touch(layak, actorUserId, now);
                }

                AddHistory(order, layak, "Study.DirectResult", dari.ToString(), layak.StudyStatus.ToString(),
                    DirectResultReasonCode, "Hasil diisi lewat Form Hasil tanpa konsol pemeriksaan", actorUserId, now);
            }

            // Pesanan yang belum dikerjakan berpindah ke sedang dikerjakan; rilis hasil menyelesaikannya.
            // Versi pesanan selalu dinaikkan supaya dua Simpan bersamaan tidak melahirkan dua study.
            if (order.OrderStatus is RadOrderStatus.Accepted or RadOrderStatus.Scheduled)
            {
                var statusPesanan = order.OrderStatus;
                order.OrderStatus = RadOrderStatus.InProgress;
                AddHistory(order, null, "Order.Start", statusPesanan.ToString(), order.OrderStatus.ToString(),
                    "AutoOnDirectResult", null, actorUserId, now);
            }

            order.UpdateBy = actorUserId;
            order.UpdateDateTime = now;
            order.Version += 1;

            await SaveWithConcurrencyGuardAsync(cancellationToken);
            if (transaksi != null) await transaksi.CommitAsync(cancellationToken);

            return RadOperationResult<RadStudy>.Success(layak);
        }

        /// <summary>
        /// Fakta kelayakan tagih — sama dengan yang diterbitkan penilaian mutu di konsol. Terbit di
        /// luar transaksi (produsen fakta menolak dipanggil di dalam transaksi) dan hanya sekali.
        /// Kegagalannya tidak menggagalkan penyimpanan hasil: study tetap bertanda belum menyerahkan
        /// fakta dan Simpan berikutnya mencobanya lagi.
        /// </summary>
        private async Task TerbitkanTagihanBilaBelumAsync(RadStudy study, CancellationToken cancellationToken)
        {
            if (study.BillingFactSubmitted)
            {
                return;
            }

            try
            {
                var handoff = await EmitChargeEligibilityAsync(study, GetCurrentUserId(), cancellationToken);

                // BE-MCU-020: pesanan paket MCU tidak menagih apa pun — bukan kegagalan kirim.
                if (handoff == null)
                {
                    return;
                }

                if (handoff.Kind is ClinicalFactEmissionKind.Emitted or ClinicalFactEmissionKind.Replayed)
                {
                    study.BillingFactSubmitted = true;
                    study.BillingFactSubmittedAt = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    return;
                }

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "RadStudy.DirectResultBillingNotSubmitted",
                    $"Fakta tagihan radiologi study {study.StudyNumber} (pesanan {study.RadOrderId}) belum terkirim: {handoff.Kind}. Dicoba lagi pada Simpan berikutnya.");
            }
            catch (Exception exception)
            {
                await _loggerService.ErrorAsync(
                    LogCategory,
                    "RadStudy.DirectResultBillingFailed",
                    $"Fakta tagihan radiologi study {study.StudyNumber} (pesanan {study.RadOrderId}) gagal dikirim. Dicoba lagi pada Simpan berikutnya.",
                    exception,
                    new { RadStudyId = study.Id, study.RadOrderId });
            }
        }
    }
}
