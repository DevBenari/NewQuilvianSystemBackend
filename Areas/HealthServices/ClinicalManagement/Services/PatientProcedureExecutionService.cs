using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Penyelesaian order tindakan pasien (<c>BE-RWI-178</c>, <c>RWI-DEC-196</c>, backend 12.7).
    /// Logika <c>PATCH patient-procedures/{id}/execute</c> dipindah dari controller ke sini supaya
    /// Kamar Operasi dapat menyelesaikan order tanpa memanggil HTTP.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Perubahan struktur, bukan perilaku.</b> <see cref="ExecuteAsync"/> menghasilkan status,
    /// pendaftaran dokumen rekam medis, dan fakta tagih yang sama persis dengan endpoint lama: order
    /// <c>Completed</c> dan dokumen tindakan tertanda tangan pada satu <c>SaveChanges</c>, lalu fakta
    /// tagih diterbitkan <b>sesudah</b> commit.
    /// </para>
    /// <para>
    /// <see cref="ExecuteFromOperatingRoomAsync"/> dipakai efek kasus OK selesai: pelaksana = dokter
    /// operator, waktu = kasus selesai, <b>tanpa</b> dokumen tindakan baru (dokumen klinisnya laporan
    /// operasi final OK). Order yang sudah <c>Completed</c> dilewati sehingga efek boleh dijalankan
    /// ulang tanpa baris tagih ganda (<c>INV-RWF-29</c>).
    /// </para>
    /// </remarks>
    public sealed class PatientProcedureExecutionService
    {
        private const string LogCategory = "HealthServices.Clinical";

        private readonly ApplicationDbContext _dbContext;
        private readonly ClinicalMilestoneFactProducer _clinicalMilestoneFactProducer;
        private readonly ClinicalDocumentIntegrityService _integrityService;
        private readonly PatientProcedureOrderService _procedureOrderService;
        private readonly LoggerService _loggerService;

        public PatientProcedureExecutionService(
            ApplicationDbContext dbContext,
            ClinicalMilestoneFactProducer clinicalMilestoneFactProducer,
            ClinicalDocumentIntegrityService integrityService,
            PatientProcedureOrderService procedureOrderService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _clinicalMilestoneFactProducer = clinicalMilestoneFactProducer;
            _integrityService = integrityService;
            _procedureOrderService = procedureOrderService;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Perilaku lama endpoint <c>execute</c>, apa adanya. Kode status dan pesan dikembalikan
        /// supaya controller tetap menjawab persis seperti sebelum ekstraksi.
        /// </summary>
        public async Task<PatientProcedureExecutionResult> ExecuteAsync(
            Guid procedureId,
            ExecutePatientProcedureRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            string? deviceInfo,
            string? ipAddress,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxPatientProcedure>()
                .FirstOrDefaultAsync(x => x.Id == procedureId && !x.IsDelete, cancellationToken);

            if (entity == null)
                return PatientProcedureExecutionResult.Fail(StatusCodes.Status404NotFound, "Tindakan pasien tidak ditemukan.");

            if (entity.ProcedureStatus == PatientProcedureStatus.Cancelled)
                return PatientProcedureExecutionResult.Fail(StatusCodes.Status400BadRequest,
                    "Tindakan yang sudah cancelled tidak dapat dieksekusi.");

            if (entity.IsNeedApproval && !entity.IsApproved)
                return PatientProcedureExecutionResult.Fail(StatusCodes.Status400BadRequest,
                    "Tindakan membutuhkan approval sebelum dieksekusi.");

            // BE-RWI-051 kriteria 2. Tindakan yang sudah ditandai dikerjakan tidak dikerjakan
            // ulang. Tanpa penjaga ini, permintaan yang diulang karena jaringan terputus
            // menerbitkan fakta klinis kedua ke Billing - dan fakta kedua berujung pada pasien
            // membayar dua kali untuk satu tindakan yang sama.
            //
            // Jawabannya 200, bukan 409: bagi dokter yang menekan tombol sekali lagi, hasilnya
            // memang sudah tercapai. Menjawab 409 membuatnya mengira pencatatannya gagal.
            if (entity.IsExecuted && entity.ProcedureStatus == PatientProcedureStatus.Completed)
                return PatientProcedureExecutionResult.AlreadyExecuted("Tindakan pasien sudah ditandai dikerjakan sebelumnya.");

            var now = DateTime.UtcNow;

            // BE-RWI-098 kriteria 4 / FR-DOK-103, state matrix 0.6.0 bagian 8.3. Pada tindakan rawat
            // inap, pelaksana dari akun login menjadi penulis dan penanda tangan catatan
            // pelaksanaan, sehingga ia wajib berwenang: dokter lewat penugasan aktif, perawat lewat
            // penempatan pada unit episode. Perawatan yang sudah ditutup ditolak 422. Tindakan
            // poliklinik, medical check-up, dan IGD tidak membawa episode dan tidak tersentuh.
            var penjagaPelaksana = await _procedureOrderService.EnsureInpatientExecutorAsync(
                entity, user, actorUserId, cancellationToken);

            if (penjagaPelaksana != null)
                return PatientProcedureExecutionResult.Fail(penjagaPelaksana.StatusCode, penjagaPelaksana.Message);

            entity.ProcedureStatus = PatientProcedureStatus.Completed;
            entity.IsExecuted = true;
            entity.ExecutedAt = now;
            entity.ExecutedByUserId = actorUserId;
            entity.PerformedAt = request.PerformedAt ?? now;
            entity.PerformedByUserId = actorUserId;
            entity.StartedAt ??= now;
            entity.CompletedAt = now;
            entity.ResultNote = NormalizeNullableText(request.ResultNote) ?? entity.ResultNote;
            entity.DispositionNote = NormalizeNullableText(request.DispositionNote) ?? entity.DispositionNote;
            entity.ComplicationNote = NormalizeNullableText(request.ComplicationNote) ?? entity.ComplicationNote;
            entity.FollowUpInstruction = NormalizeNullableText(request.FollowUpInstruction) ?? entity.FollowUpInstruction;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            // BE-RWI-038, RWI-AC-157. Menandai tindakan sudah dikerjakan sekaligus
            // mendaftarkannya ke mesin keutuhan rekam medis sebagai dokumen tertanda tangan,
            // pada SaveChanges yang sama.
            //
            // Kenapa satu SaveChanges: tindakan yang sudah Completed tidak dapat disunting
            // lagi. Bila pendaftarannya gagal dan tetap dibiarkan, catatan tindakan itu tidak
            // dapat disunting maupun dikoreksi selamanya. Karena itu kegagalan pendaftaran
            // membatalkan penandaan, bukan didiamkan.
            //
            // Penanda tangan adalah PELAKSANA tindakan, karena dialah yang bertanggung jawab
            // atas isi catatan pelaksanaan.
            var authorUserId = entity.PerformedByUserId is { } pelaksana && pelaksana != Guid.Empty
                ? pelaksana
                : actorUserId;

            try
            {
                await _integrityService.RegisterSignedAsync(
                    ClinicalDocumentKind.Procedure,
                    entity.Id,
                    entity.PatientId,
                    entity.EncounterId,
                    authorUserId,
                    deviceInfo: deviceInfo,
                    ipAddress: ipAddress,
                    nowUtc: now,
                    cancellationToken: cancellationToken);
            }
            catch (InvalidOperationException pendaftaranGagal)
            {
                return PatientProcedureExecutionResult.Fail(StatusCodes.Status400BadRequest,
                    "Tindakan tidak dapat ditandai dikerjakan karena pendaftaran pada rekam " +
                    $"medis gagal: {pendaftaranGagal.Message}");
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // RJ-BIL-BE-002. Milestone charge tindakan menurut RJ-BIL-DEC-002 adalah tindakan
            // benar-benar dieksekusi dan berstatus Completed; pemilihan atau order saja bukan
            // pemicu charge. Fakta diserahkan setelah perubahan klinis tersimpan.
            var emission = await _clinicalMilestoneFactProducer.EmitChargeEligibilityAsync(
                BuildProcedureFactRequest(entity, now),
                actorUserId,
                cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientProcedure.ExecuteProcedure",
                "Mengeksekusi tindakan pasien dan menyerahkan fakta ke Billing.",
                new
                {
                    ProcedureId = entity.Id,
                    entity.EncounterId,
                    BillingHandoff = emission.Kind.ToString(),
                    emission.MilestoneFactVersion
                });

            return PatientProcedureExecutionResult.Executed(
                emission.Kind.ToString(),
                emission.IsClinicallySafe,
                emission.IsClinicallySafe
                    ? "Tindakan pasien berhasil dieksekusi."
                    : "Tindakan pasien berhasil dieksekusi, tetapi penyerahan fakta ke Billing memerlukan tinjauan.");
        }

        /// <summary>
        /// Menyelesaikan order tindakan dari kasus Kamar Operasi yang <c>Completed</c>
        /// (<c>BE-RWI-179</c> memanggilnya). Idempoten: order yang sudah <c>Completed</c> tidak
        /// membuat apa pun.
        /// </summary>
        /// <param name="procedureId">Order tindakan yang dirujuk <c>OprCaseProcedure</c>.</param>
        /// <param name="operatorDoctorId">Dokter operator kasus (<c>OprCase.PrimarySurgeonId</c>).</param>
        /// <param name="completedAt">Waktu kasus selesai; menjadi waktu pelaksanaan order.</param>
        /// <param name="actorUserId">Akun yang perintahnya membuat kasus selesai.</param>
        /// <param name="cancellationToken">Token pembatalan.</param>
        public async Task<PatientProcedureExecutionResult> ExecuteFromOperatingRoomAsync(
            Guid procedureId,
            Guid operatorDoctorId,
            DateTime completedAt,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxPatientProcedure>()
                .FirstOrDefaultAsync(x => x.Id == procedureId && !x.IsDelete, cancellationToken);

            if (entity == null)
                return PatientProcedureExecutionResult.Fail(StatusCodes.Status404NotFound, "Tindakan pasien tidak ditemukan.");

            // Idempoten (AC 2): order yang sudah selesai tidak menyentuh apa pun dan tidak
            // menerbitkan fakta tagih kedua.
            if (entity.IsExecuted && entity.ProcedureStatus == PatientProcedureStatus.Completed)
                return PatientProcedureExecutionResult.AlreadyExecuted("Order tindakan sudah selesai sebelumnya.");

            if (entity.ProcedureStatus == PatientProcedureStatus.Cancelled || entity.IsCancel)
                return PatientProcedureExecutionResult.Fail(StatusCodes.Status400BadRequest,
                    "Order tindakan sudah dibatalkan sehingga tidak diselesaikan dari Kamar Operasi.");

            if (entity.IsNeedApproval && !entity.IsApproved)
                return PatientProcedureExecutionResult.Fail(StatusCodes.Status400BadRequest,
                    "Order tindakan membutuhkan approval sebelum dapat diselesaikan.");

            // Pelaksana = dokter operator; akunnya dicari dari tautan dokter pada akun pengguna.
            var operatorUserId = await _dbContext.Users.AsNoTracking()
                .Where(x => x.DoctorId == operatorDoctorId && x.IsActive)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var now = DateTime.UtcNow;
            var performedAt = completedAt == default ? now : completedAt;

            entity.ProcedureStatus = PatientProcedureStatus.Completed;
            entity.IsExecuted = true;
            entity.ExecutedAt = now;
            entity.ExecutedByUserId = actorUserId;
            entity.PerformedAt = performedAt;
            entity.PerformedByUserId = operatorUserId ?? actorUserId;
            entity.StartedAt ??= performedAt;
            entity.CompletedAt = performedAt;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            // Tanpa pendaftaran dokumen tindakan baru: dokumen klinisnya laporan operasi final OK.
            await _dbContext.SaveChangesAsync(cancellationToken);

            var emission = await _clinicalMilestoneFactProducer.EmitChargeEligibilityAsync(
                BuildProcedureFactRequest(entity, performedAt),
                actorUserId,
                cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientProcedure.ExecuteFromOperatingRoom",
                "Menyelesaikan order tindakan dari kasus Kamar Operasi dan menyerahkan fakta ke Billing.",
                new
                {
                    ProcedureId = entity.Id,
                    entity.EncounterId,
                    OperatorDoctorId = operatorDoctorId,
                    BillingHandoff = emission.Kind.ToString(),
                    emission.MilestoneFactVersion
                });

            return PatientProcedureExecutionResult.Executed(
                emission.Kind.ToString(),
                emission.IsClinicallySafe,
                "Order tindakan diselesaikan dari Kamar Operasi.");
        }

        /// <summary>
        /// Fakta milestone tindakan. Dipakai juga oleh pembatalan tindakan di controller
        /// (<c>includeSnapshot = false</c>) supaya bentuk fakta hanya punya satu definisi.
        /// </summary>
        internal static ClinicalMilestoneFactRequest BuildProcedureFactRequest(
            TrxPatientProcedure entity,
            DateTime occurredAt,
            bool includeSnapshot = true)
        {
            var hasQuantity = includeSnapshot &&
                              entity.Quantity > 0 &&
                              !string.IsNullOrWhiteSpace(entity.UnitNameSnapshot);

            return new ClinicalMilestoneFactRequest
            {
                SourceContext = BillingSourceContract.ProcedureSourceContext,
                SourceAggregateId = entity.Id,
                EffectType = BillingSourceContract.ProcedureChargeEffectType,
                EncounterId = entity.EncounterId,
                OccurredAt = occurredAt,
                Quantity = hasQuantity ? entity.Quantity : null,
                Unit = hasQuantity ? entity.UnitNameSnapshot : null,
                TariffSnapshot = includeSnapshot
                    ? System.Text.Json.JsonSerializer.Serialize(new
                    {
                        source = "ClinicalSnapshot",
                        procedureCode = entity.ProcedureCodeSnapshot,
                        procedureName = entity.ProcedureNameSnapshot,
                        unitPrice = entity.UnitPrice,
                        totalPrice = entity.TotalPrice,
                        isFreeOfCharge = entity.IsFreeOfCharge,
                        isBillable = entity.IsBillable
                    })
                    : null,
                CorrelationId = entity.ConsultationId
            };
        }

        private static string? NormalizeNullableText(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>Hasil penyelesaian order tindakan.</summary>
    public sealed class PatientProcedureExecutionResult
    {
        public int StatusCode { get; private init; }
        public string Message { get; private init; } = string.Empty;

        /// <summary>Jenis serah fakta ke Billing, mis. <c>Emitted</c>, atau <c>AlreadyExecuted</c>.</summary>
        public string? BillingHandoff { get; private init; }

        public bool IsClinicallySafe { get; private init; } = true;

        /// <summary>Order baru saja diselesaikan pada pemanggilan ini.</summary>
        public bool WasExecuted { get; private init; }

        public bool IsSuccess => StatusCode == StatusCodes.Status200OK;

        public static PatientProcedureExecutionResult Executed(string billingHandoff, bool isClinicallySafe, string message) =>
            new()
            {
                StatusCode = StatusCodes.Status200OK, BillingHandoff = billingHandoff,
                IsClinicallySafe = isClinicallySafe, WasExecuted = true, Message = message
            };

        public static PatientProcedureExecutionResult AlreadyExecuted(string message) =>
            new() { StatusCode = StatusCodes.Status200OK, BillingHandoff = "AlreadyExecuted", Message = message };

        public static PatientProcedureExecutionResult Fail(int statusCode, string message) =>
            new() { StatusCode = statusCode, Message = message, IsClinicallySafe = false };
    }
}
