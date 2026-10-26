using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Simpan Form Hasil v1 — hasil langsung final (<c>RAD-DEC-023</c>, keputusan pemilik modul
    /// 2026-10-19, menggantikan pengesahan radiolog <c>RAD-DEC-003</c> untuk jalur ini).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Di v1, hasil yang disimpan langsung berlaku. Jalur ini menulis versi bacaan yang langsung
    /// berstatus <c>Released</c>, dengan penyimpan tercatat sebagai penulis sekaligus pengesah.
    /// </para>
    /// <para>
    /// <b>Simpan ulang tidak pernah menimpa.</b> Hasil yang sudah dirilis mungkin sudah dibaca dokter
    /// pengirim; perubahan isinya menjadi versi koreksi baru dan versi lama berpindah ke
    /// <c>Superseded</c> — kaidah yang sama dengan <see cref="CreateAmendmentAsync"/> dan
    /// <see cref="ReleaseAsync"/>. Isi yang tidak berubah tidak melahirkan versi baru.
    /// </para>
    /// </remarks>
    public partial class RadReportService
    {
        internal const string FinalResultAmendmentReason = "Hasil diperbarui lewat Form Hasil.";

        /// <summary>Menulis hasil final atas study yang citranya layak.</summary>
        public async Task<RadOperationResult<RadReportDetailResponse>> SaveFinalResultAsync(
            Guid radStudyId,
            string? findings,
            string impression,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var isi = ValidateContent(findings, impression, null);
            if (isi != null)
            {
                return isi;
            }

            var peran = await HasRadiologistAuthorityAsync(cancellationToken)
                ? RadReportAuthorRole.Radiologist
                : RadReportAuthorRole.Radiographer;

            var transaction = await BeginGuardedAsync($"RAD_REPORT_STUDY_{radStudyId:N}", cancellationToken);

            try
            {
                var study = await LoadStudyAsync(radStudyId, cancellationToken);
                if (study == null)
                {
                    return RadOperationResult<RadReportDetailResponse>.NotFound(
                        RadErrorCodes.StudyNotFound,
                        "Pemeriksaan yang dimaksud tidak ditemukan.");
                }

                var kelayakan = ValidateStudyIsReadable(study);
                if (kelayakan != null)
                {
                    return kelayakan;
                }

                var report = await FindReportByStudyAsync(radStudyId, cancellationToken);
                if (report == null)
                {
                    report = NewReport(study, actorUserId, now);
                    _dbContext.RadReports.Add(report);
                }

                var versi = await _dbContext.RadReportVersions
                    .Where(x => x.RadReportId == report.Id && !x.IsDelete)
                    .OrderByDescending(x => x.VersionNumber)
                    .ToListAsync(cancellationToken);

                var temuan = Rapikan(findings);
                var kesimpulan = impression.Trim();
                var terakhir = versi.FirstOrDefault();

                // Versi kerja dari alur lama (draf/disahkan, belum dirilis) dipakai sebagai versi
                // yang dirilis sekarang; versi rilis yang berlaku tidak pernah ditulis ulang.
                RadReportVersion dirilis;
                if (terakhir is { VersionStatus: RadReportVersionStatus.Drafted or RadReportVersionStatus.Validated })
                {
                    dirilis = terakhir;
                    dirilis.Findings = temuan;
                    dirilis.Impression = kesimpulan;
                    dirilis.ValidatorUserId = actorUserId;
                    dirilis.ValidatedAt = now;
                }
                else if (terakhir == null)
                {
                    dirilis = VersiBaru(report.Id, 1, null, false, temuan, kesimpulan, peran, actorUserId, now);
                }
                else
                {
                    if (string.Equals(terakhir.Findings, temuan, StringComparison.Ordinal) &&
                        string.Equals(terakhir.Impression, kesimpulan, StringComparison.Ordinal))
                    {
                        // Isi sama tidak melahirkan versi baru, tetapi pesanan yang masih terbuka tetap selesai.
                        await SelesaikanPesananAsync(report, actorUserId, now, cancellationToken);
                        await _dbContext.SaveChangesAsync(cancellationToken);

                        if (transaction != null)
                        {
                            await transaction.CommitAsync(cancellationToken);
                        }

                        return RadOperationResult<RadReportDetailResponse>.Success(
                            await MapAsync(report.Id, cancellationToken));
                    }

                    dirilis = VersiBaru(report.Id, terakhir.VersionNumber + 1, terakhir.Id, true,
                        temuan, kesimpulan, peran, actorUserId, now);
                    dirilis.AmendmentReason = FinalResultAmendmentReason;
                }

                dirilis.VersionStatus = RadReportVersionStatus.Released;
                dirilis.ReleasedAt = now;
                if (dirilis.CreateDateTime != now)
                {
                    Touch(dirilis, actorUserId, now);
                }

                if (dirilis.PreviousVersionId.HasValue)
                {
                    var digantikan = versi.FirstOrDefault(x => x.Id == dirilis.PreviousVersionId.Value);
                    if (digantikan is { VersionStatus: RadReportVersionStatus.Released })
                    {
                        digantikan.VersionStatus = RadReportVersionStatus.Superseded;
                        Touch(digantikan, actorUserId, now);
                    }
                }

                report.ReportStatus = dirilis.IsAmendment ? RadReportStatus.AmendmentReleased : RadReportStatus.Released;
                report.CurrentVersionNumber = dirilis.VersionNumber;
                report.FirstReleasedAt ??= now;
                report.LastReleasedAt = now;
                if (report.CreateDateTime != now)
                {
                    Touch(report, actorUserId, now);
                }

                // Alur v1: pemeriksaan berstatus Selesai begitu hasilnya disimpan — juga pada simpan
                // ulang, agar pesanan yang hasilnya dirilis lewat alur lama tidak tertahan di InProgress.
                await SelesaikanPesananAsync(report, actorUserId, now, cancellationToken);

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException)
                {
                    return RadOperationResult<RadReportDetailResponse>.Conflict(
                        RadErrorCodes.ReportAlreadyExists,
                        "Hasil pemeriksaan ini baru saja diubah petugas lain. Muat ulang lalu simpan kembali.");
                }

                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                await _loggerService.AuditAsync(
                    LogCategory,
                    "RadReport.SaveFinalResult",
                    $"Hasil radiologi {report.ReportNumber} versi {dirilis.VersionNumber} disimpan final lewat Form Hasil " +
                    $"(pesanan {report.RadOrderId}, {(dirilis.IsAmendment ? "koreksi" : "hasil pertama")}).",
                    RadReportLogPayload.For(report, dirilis, actorUserId));

                return RadOperationResult<RadReportDetailResponse>.Success(
                    await MapAsync(report.Id, cancellationToken));
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        private RadReportVersion VersiBaru(
            Guid reportId,
            int nomor,
            Guid? sebelumnya,
            bool koreksi,
            string? temuan,
            string kesimpulan,
            RadReportAuthorRole peran,
            Guid actorUserId,
            DateTime now)
        {
            var versi = new RadReportVersion
            {
                RadReportId = reportId,
                VersionNumber = nomor,
                PreviousVersionId = sebelumnya,
                IsAmendment = koreksi,
                Findings = temuan,
                Impression = kesimpulan,
                AuthorUserId = actorUserId,
                AuthorRoleSnapshot = peran,
                DraftedAt = now,
                ValidatorUserId = actorUserId,
                ValidatedAt = now,
                CreateBy = actorUserId,
                CreateDateTime = now,
            };
            _dbContext.RadReportVersions.Add(versi);
            return versi;
        }

        private async Task SelesaikanPesananAsync(RadReport report, Guid actorUserId, DateTime now, CancellationToken cancellationToken)
        {
            var pesanan = await _dbContext.RadOrders
                .FirstOrDefaultAsync(o => o.Id == report.RadOrderId && !o.IsDelete, cancellationToken);

            if (pesanan is not { OrderStatus: RadOrderStatus.Accepted or RadOrderStatus.Scheduled or RadOrderStatus.InProgress })
            {
                return;
            }

            var statusPesanan = pesanan.OrderStatus;
            pesanan.OrderStatus = RadOrderStatus.Completed;
            pesanan.CompletedAt = now;
            pesanan.UpdateBy = actorUserId;
            pesanan.UpdateDateTime = now;
            pesanan.Version += 1;

            _dbContext.RadTransitionHistories.Add(new RadTransitionHistory
            {
                RadOrderId = pesanan.Id,
                RadStudyId = report.RadStudyId,
                EncounterId = pesanan.EncounterId,
                Scope = RadTransitionScope.RadOrder,
                Action = "Order.Complete",
                FromStatus = statusPesanan.ToString(),
                ToStatus = pesanan.OrderStatus.ToString(),
                ReasonCode = "AutoOnFinalResult",
                ActorUserId = actorUserId,
                OccurredAt = now,
                CreateBy = actorUserId,
                CreateDateTime = now,
            });
        }
    }
}
