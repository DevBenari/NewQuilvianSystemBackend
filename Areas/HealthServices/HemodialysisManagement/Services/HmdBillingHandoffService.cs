using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Services
{
    /// <summary>
    /// Menyerahkan fakta tindakan sesi yang sudah disahkan ke Billing (<c>BE-HMD-18</c>,
    /// <c>HMD-DEC-012</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Di luar transaksi finalisasi, dan itu disengaja.</b> Service ini hanya dipanggil setelah
    /// transaksi pengesahan di-<c>commit</c>. Kegagalan Billing tidak pernah membuka kembali catatan
    /// klinis: sesi tetap <c>Finalized</c> dan terkunci, sedangkan <c>BillingHandoffStatus</c> menjadi
    /// <c>Failed</c> atau <c>Pending</c> untuk diulang lewat endpoint pengulangan.
    /// </para>
    /// <para>
    /// <b>Jalur resmi, tanpa sumber tagihan baru.</b> Penyerahan memakai
    /// <see cref="ClinicalMilestoneFactProducer"/> dengan sumber <c>Procedure</c> dan efek
    /// <c>ProcedureCharge</c> yang sudah terdaftar pada <see cref="BillingSourceContract"/>. Waktu
    /// kejadian diambil dari waktu pengesahan yang tersimpan, sehingga pengulangan menghasilkan sidik
    /// jari yang sama dan Billing menjawab <c>Replayed</c> — tidak pernah dua tagihan untuk satu sesi.
    /// </para>
    /// <para>
    /// <b>Sesi dihentikan tidak menagih.</b> Sesi <c>Stopped</c> tetap membentuk tindakan, tetapi
    /// bertanda <c>IsBillable = false</c> dan tidak ada fakta tagihan yang diserahkan
    /// (<c>BillingHandoffStatus = NotRequired</c>). Bila rumah sakit berhak menagih bahan terpakai,
    /// kasir menambahkannya lewat jalur tagihan bebas.
    /// </para>
    /// </remarks>
    public class HmdBillingHandoffService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ClinicalMilestoneFactProducer _factProducer;

        public HmdBillingHandoffService(ApplicationDbContext dbContext, ClinicalMilestoneFactProducer factProducer)
        {
            _dbContext = dbContext;
            _factProducer = factProducer;
        }

        public async Task<HmdResult<HmdBillingHandoffResponse>> GetStatusAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            var status = await ReadAsync(sessionId, cancellationToken);
            return status == null
                ? HmdResult<HmdBillingHandoffResponse>.NotFound("Sesi hemodialisa tidak ditemukan atau sudah dihapus.")
                : HmdResult<HmdBillingHandoffResponse>.Ok(status);
        }

        /// <summary>
        /// Mengulang penyerahan yang tertunda atau gagal. Idempoten: penyerahan yang sudah berhasil
        /// dijawab apa adanya tanpa mengirim ulang.
        /// </summary>
        public async Task<HmdResult<HmdBillingHandoffResponse>> RetryAsync(Guid sessionId, Guid actorUserId, CancellationToken cancellationToken)
        {
            var session = await _dbContext.HmdSessions.AsNoTracking()
                .Where(x => x.Id == sessionId && !x.IsDelete)
                .Select(x => new { x.SessionStatus, x.BillingHandoffStatus })
                .FirstOrDefaultAsync(cancellationToken);

            if (session == null)
                return HmdResult<HmdBillingHandoffResponse>.NotFound("Sesi hemodialisa tidak ditemukan atau sudah dihapus.");

            if (session.SessionStatus != HmdSessionStatus.Finalized)
            {
                return HmdResult<HmdBillingHandoffResponse>.Rule(
                    HmdErrorCodes.InvalidTransition,
                    "Penyerahan ke Billing hanya dapat diulang setelah catatan sesi disahkan.");
            }

            if (session.BillingHandoffStatus is HmdBillingHandoffStatus.Pending or HmdBillingHandoffStatus.Failed)
                return HmdResult<HmdBillingHandoffResponse>.Ok(await HandOffAsync(sessionId, actorUserId, cancellationToken));

            return HmdResult<HmdBillingHandoffResponse>.Ok((await ReadAsync(sessionId, cancellationToken))!);
        }

        /// <summary>
        /// Menyerahkan fakta tindakan satu sesi yang sudah disahkan. Wajib dipanggil di luar
        /// transaksi; <see cref="ClinicalMilestoneFactProducer"/> menolak berjalan di dalam transaksi.
        /// </summary>
        public async Task<HmdBillingHandoffResponse> HandOffAsync(Guid sessionId, Guid actorUserId, CancellationToken cancellationToken)
        {
            var session = await _dbContext.HmdSessions.FirstAsync(x => x.Id == sessionId, cancellationToken);

            if (session.SessionStatus != HmdSessionStatus.Finalized ||
                session.BillingHandoffStatus is HmdBillingHandoffStatus.Succeeded)
            {
                return (await ReadAsync(sessionId, cancellationToken))!;
            }

            var now = DateTime.UtcNow;

            var procedure = session.PatientProcedureId.HasValue
                ? await _dbContext.Set<TrxPatientProcedure>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == session.PatientProcedureId.Value, cancellationToken)
                : null;

            if (session.StopReason.HasValue || procedure is { IsBillable: false })
            {
                session.BillingHandoffStatus = HmdBillingHandoffStatus.NotRequired;
                session.BillingHandoffError = null;
                await _dbContext.SaveChangesAsync(cancellationToken);
                return (await ReadAsync(sessionId, cancellationToken))!;
            }

            if (procedure == null)
            {
                session.BillingHandoffStatus = HmdBillingHandoffStatus.Failed;
                session.BillingHandoffError = "Tindakan pasien untuk sesi ini tidak ditemukan.";
                await _dbContext.SaveChangesAsync(cancellationToken);
                return (await ReadAsync(sessionId, cancellationToken))!;
            }

            try
            {
                var emission = await _factProducer.EmitChargeEligibilityAsync(
                    BuildFactRequest(procedure, session.SignedAt ?? now, session.Id),
                    actorUserId,
                    cancellationToken);

                switch (emission.Kind)
                {
                    case ClinicalFactEmissionKind.Emitted:
                    case ClinicalFactEmissionKind.Replayed:
                        session.BillingHandoffStatus = HmdBillingHandoffStatus.Succeeded;
                        session.BillingHandoffAt = now;
                        session.BillingHandoffError = null;
                        break;

                    case ClinicalFactEmissionKind.OutcomeUnknown:
                        session.BillingHandoffStatus = HmdBillingHandoffStatus.Pending;
                        session.BillingHandoffError = Truncate(emission.Message ?? "Hasil penyerahan ke Billing belum dapat dipastikan.");
                        break;

                    default:
                        session.BillingHandoffStatus = HmdBillingHandoffStatus.Failed;
                        session.BillingHandoffError = Truncate($"{emission.Code}: {emission.Message}");
                        break;
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // Billing gagal dihubungi. Catatan klinis tetap Finalized; hanya status penyerahan
                // yang mencatat kegagalan supaya dapat diulang.
                HmdServiceSupport.DetachFailedEntries(_dbContext);
                session = await _dbContext.HmdSessions.FirstAsync(x => x.Id == sessionId, cancellationToken);
                session.BillingHandoffStatus = HmdBillingHandoffStatus.Failed;
                session.BillingHandoffError = Truncate(exception.Message);
            }

            session.UpdateDateTime = now;
            session.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return (await ReadAsync(sessionId, cancellationToken))!;
        }

        /// <summary>
        /// Menyusun fakta tindakan dengan bentuk yang sama dengan <c>PatientProcedureController</c>,
        /// sehingga Billing memperlakukan tindakan hemodialisa persis seperti tindakan lain.
        /// </summary>
        private static ClinicalMilestoneFactRequest BuildFactRequest(TrxPatientProcedure procedure, DateTime occurredAt, Guid sessionId)
        {
            var hasQuantity = procedure.Quantity > 0 && !string.IsNullOrWhiteSpace(procedure.UnitNameSnapshot);

            return new ClinicalMilestoneFactRequest
            {
                SourceContext = BillingSourceContract.ProcedureSourceContext,
                SourceAggregateId = procedure.Id,
                EffectType = BillingSourceContract.ProcedureChargeEffectType,
                EncounterId = procedure.EncounterId,
                OccurredAt = occurredAt,
                Quantity = hasQuantity ? procedure.Quantity : null,
                Unit = hasQuantity ? procedure.UnitNameSnapshot : null,
                TariffSnapshot = System.Text.Json.JsonSerializer.Serialize(new
                {
                    source = "ClinicalSnapshot",
                    procedureCode = procedure.ProcedureCodeSnapshot,
                    procedureName = procedure.ProcedureNameSnapshot,
                    unitPrice = procedure.UnitPrice,
                    totalPrice = procedure.TotalPrice,
                    isFreeOfCharge = procedure.IsFreeOfCharge,
                    isBillable = procedure.IsBillable
                }),
                CorrelationId = sessionId
            };
        }

        private async Task<HmdBillingHandoffResponse?> ReadAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            var row = await _dbContext.HmdSessions.AsNoTracking()
                .Where(x => x.Id == sessionId && !x.IsDelete)
                .Select(x => new HmdBillingHandoffResponse
                {
                    SessionId = x.Id,
                    SessionStatus = x.SessionStatus,
                    PatientProcedureId = x.PatientProcedureId,
                    IsBillable = x.PatientProcedure != null && x.PatientProcedure.IsBillable,
                    BillingHandoffStatus = x.BillingHandoffStatus,
                    BillingHandoffAt = x.BillingHandoffAt,
                    BillingHandoffError = x.BillingHandoffError
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (row != null)
                row.BillingHandoffStatusName = HmdLabels.BillingHandoff(row.BillingHandoffStatus);

            return row;
        }

        private static string Truncate(string value) => value.Length > 1000 ? value[..1000] : value;
    }
}
