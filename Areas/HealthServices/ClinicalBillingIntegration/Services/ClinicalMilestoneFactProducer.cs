using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalBillingIntegration.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalBillingIntegration.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalBillingIntegration.Services
{
    /// <summary>
    /// Penerbit fakta klinis untuk Billing (<c>RJ-BIL-BE-002</c>).
    ///
    /// Modul klinis tidak lagi menetapkan status finansial. Modul klinis hanya menyatakan
    /// peristiwa klinis, dan Billing yang menentukan akibat finansialnya. Kelas ini adalah
    /// satu-satunya jalur resmi penyerahan tersebut.
    ///
    /// Urutan kerja yang wajib dipatuhi pemanggil:
    ///
    /// <list type="number">
    ///   <item>Simpan dan commit perubahan klinis lebih dulu.</item>
    ///   <item>Baru panggil kelas ini.</item>
    /// </list>
    ///
    /// Urutan ini bukan preferensi gaya. Kegagalan Billing tidak boleh membatalkan kebenaran
    /// klinis yang sudah terjadi, dan <see cref="BillingFolioService"/> membuka transaksinya
    /// sendiri sehingga tidak dapat dijalankan di dalam transaksi klinis yang masih terbuka.
    /// </summary>
    public class ClinicalMilestoneFactProducer
    {
        public const string LogCategory = "HealthServices.ClinicalBillingIntegration";

        private const int MaxVersionAllocationAttempts = 3;
        private const int MaxSnapshotLength = 20_000;

        private readonly ApplicationDbContext _dbContext;
        private readonly BillingFolioService _billingFolioService;
        private readonly LoggerService _loggerService;

        public ClinicalMilestoneFactProducer(
            ApplicationDbContext dbContext,
            BillingFolioService billingFolioService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _billingFolioService = billingFolioService;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Menerbitkan fakta bahwa milestone charge eligibility klinis telah tercapai.
        /// </summary>
        public Task<ClinicalFactEmissionResult> EmitChargeEligibilityAsync(
            ClinicalMilestoneFactRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default) =>
            EmitAsync(request, actorUserId, ClinicalMilestoneKind.ChargeEligibility, cancellationToken);

        /// <summary>
        /// Menerbitkan fakta pembatalan klinis.
        ///
        /// Pembatalan klinis bukan pembatalan finansial. Bila charge belum pernah terbentuk,
        /// tidak ada apa pun yang dikirim ke Billing. Bila charge sudah terbentuk, yang dikirim
        /// adalah revisi baru atas fakta yang sama, sehingga charge lama tetap utuh dan Billing
        /// yang memutuskan koreksinya.
        /// </summary>
        public Task<ClinicalFactEmissionResult> EmitClinicalCancellationAsync(
            ClinicalMilestoneFactRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default) =>
            EmitAsync(request, actorUserId, ClinicalMilestoneKind.ClinicalCancellation, cancellationToken);

        private async Task<ClinicalFactEmissionResult> EmitAsync(
            ClinicalMilestoneFactRequest request,
            Guid actorUserId,
            ClinicalMilestoneKind milestoneKind,
            CancellationToken cancellationToken)
        {
            if (_dbContext.Database.CurrentTransaction != null)
            {
                throw new InvalidOperationException(
                    "Fakta klinis hanya boleh diterbitkan setelah transaksi klinis di-commit. " +
                    "Pindahkan pemanggilan ClinicalMilestoneFactProducer ke luar transaksi.");
            }

            var validation = Validate(request, actorUserId);
            if (validation != null)
            {
                return ClinicalFactEmissionResult.Failure(
                    ClinicalFactEmissionKind.Invalid,
                    validation.Value.Code,
                    validation.Value.Message);
            }

            var normalized = Normalize(request);
            var fingerprint = BuildFingerprint(normalized, milestoneKind);

            for (var attempt = 1; attempt <= MaxVersionAllocationAttempts; attempt++)
            {
                var latest = await FindLatestFactAsync(normalized, cancellationToken);

                var decision = DecideNextStep(latest, milestoneKind, fingerprint);
                if (decision.ShortCircuit != null)
                    return decision.ShortCircuit;

                if (decision.RedispatchExisting != null)
                {
                    return await DispatchAsync(
                        decision.RedispatchExisting,
                        normalized,
                        actorUserId,
                        ClinicalFactEmissionKind.Emitted,
                        cancellationToken);
                }

                var fact = new BilClinicalMilestoneFact
                {
                    Id = Guid.NewGuid(),
                    SourceContext = normalized.SourceContext,
                    SourceAggregateId = normalized.SourceAggregateId,
                    SourceItemId = normalized.SourceItemId,
                    EffectType = normalized.EffectType,
                    MilestoneFactId = latest?.MilestoneFactId ?? Guid.NewGuid(),
                    MilestoneFactVersion = (latest?.MilestoneFactVersion ?? 0) + 1,
                    MilestoneKind = milestoneKind,
                    EncounterId = normalized.EncounterId,
                    OccurredAt = normalized.OccurredAt,
                    Quantity = normalized.Quantity,
                    Unit = normalized.Unit,
                    TariffSnapshot = normalized.TariffSnapshot,
                    RuleSnapshot = normalized.RuleSnapshot,
                    PayloadFingerprint = fingerprint,
                    DispatchStatus = decision.SuppressDispatch
                        ? ClinicalFactDispatchStatus.SuppressedNoPriorCharge
                        : ClinicalFactDispatchStatus.Pending,
                    CorrelationId = normalized.CorrelationId ?? Guid.NewGuid(),
                    CausationId = normalized.CausationId,
                    ActorUserId = actorUserId,
                    Version = 1,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                fact.IdempotencyKey = BuildIdempotencyKey(fact.MilestoneFactId, fact.MilestoneFactVersion);

                try
                {
                    _dbContext.Set<BilClinicalMilestoneFact>().Add(fact);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (Exception exception) when (IsUniqueViolation(exception))
                {
                    // Penerbit lain mendahului pada revisi yang sama. Lepas entitas yang gagal
                    // lalu hitung ulang dari keadaan terbaru; jangan memaksakan nomor revisi.
                    _dbContext.Entry(fact).State = EntityState.Detached;

                    if (attempt == MaxVersionAllocationAttempts)
                    {
                        return ClinicalFactEmissionResult.Failure(
                            ClinicalFactEmissionKind.ReconciliationRequired,
                            "CLIN_FACT_VERSION_RACE",
                            "Revisi fakta klinis sedang diterbitkan proses lain. Ulangi setelah keadaan stabil.");
                    }

                    continue;
                }

                if (decision.SuppressDispatch)
                {
                    await _loggerService.AuditAsync(
                        LogCategory,
                        "ClinicalFact.SuppressCancellation",
                        "Mencatat pembatalan klinis tanpa konsekuensi finansial karena charge belum terbentuk.",
                        new
                        {
                            UserId = actorUserId,
                            ClinicalMilestoneFactId = fact.Id,
                            fact.SourceContext,
                            fact.SourceAggregateId,
                            fact.SourceItemId,
                            fact.MilestoneFactId,
                            fact.MilestoneFactVersion,
                            fact.EffectType,
                            fact.EncounterId,
                            fact.CorrelationId,
                            fact.OccurredAt
                        });

                    return ClinicalFactEmissionResult.Emitted(
                        ClinicalFactEmissionKind.SuppressedNoPriorCharge,
                        fact.Id,
                        fact.MilestoneFactId,
                        fact.MilestoneFactVersion,
                        fact.DispatchStatus,
                        billingFolioId: null,
                        billingChargeLineId: null);
                }

                return await DispatchAsync(
                    fact,
                    normalized,
                    actorUserId,
                    ClinicalFactEmissionKind.Emitted,
                    cancellationToken);
            }

            return ClinicalFactEmissionResult.Failure(
                ClinicalFactEmissionKind.ReconciliationRequired,
                "CLIN_FACT_VERSION_RACE",
                "Revisi fakta klinis tidak dapat dialokasikan setelah beberapa percobaan.");
        }

        private sealed class EmissionDecision
        {
            public ClinicalFactEmissionResult? ShortCircuit { get; init; }

            public BilClinicalMilestoneFact? RedispatchExisting { get; init; }

            public bool SuppressDispatch { get; init; }
        }

        /// <summary>
        /// Menentukan langkah berikutnya berdasarkan revisi terakhir yang tercatat.
        /// Seluruh kebijakan pembatalan author (CASE A, CASE B, CASE C) diputuskan di sini.
        /// </summary>
        private static EmissionDecision DecideNextStep(
            BilClinicalMilestoneFact? latest,
            ClinicalMilestoneKind milestoneKind,
            string fingerprint)
        {
            if (latest == null)
            {
                // CASE A — pembatalan tanpa charge yang pernah terbentuk.
                return new EmissionDecision
                {
                    SuppressDispatch = milestoneKind == ClinicalMilestoneKind.ClinicalCancellation
                };
            }

            // CASE C — hasil revisi sebelumnya tidak diketahui. Dilarang menebak, dilarang
            // mengoreksi buta. Rekonsiliasi lebih dulu.
            if (latest.DispatchStatus == ClinicalFactDispatchStatus.OutcomeUnknown)
            {
                return new EmissionDecision
                {
                    ShortCircuit = ClinicalFactEmissionResult.Failure(
                        ClinicalFactEmissionKind.ReconciliationRequired,
                        "CLIN_FACT_RECONCILIATION_REQUIRED",
                        "Hasil penyerahan fakta sebelumnya belum dapat dipastikan. " +
                        "Selesaikan rekonsiliasi sebelum menerbitkan revisi baru.",
                        latest.Id,
                        latest.MilestoneFactId,
                        latest.MilestoneFactVersion)
                };
            }

            // Revisi identik yang sudah pernah tersampaikan. Tidak menerbitkan versi baru.
            if (latest.DispatchStatus == ClinicalFactDispatchStatus.Dispatched &&
                latest.MilestoneKind == milestoneKind &&
                string.Equals(latest.PayloadFingerprint, fingerprint, StringComparison.Ordinal))
            {
                return new EmissionDecision
                {
                    ShortCircuit = ClinicalFactEmissionResult.Emitted(
                        ClinicalFactEmissionKind.Replayed,
                        latest.Id,
                        latest.MilestoneFactId,
                        latest.MilestoneFactVersion,
                        latest.DispatchStatus,
                        latest.BillingFolioId,
                        latest.BillingChargeLineId)
                };
            }

            // Revisi tersimpan tetapi belum pernah terkonfirmasi diterima Billing. Kirim ulang
            // memakai kunci idempotency yang sama; Billing yang menjamin tidak ada duplikasi.
            if (latest.DispatchStatus == ClinicalFactDispatchStatus.Pending &&
                latest.MilestoneKind == milestoneKind &&
                string.Equals(latest.PayloadFingerprint, fingerprint, StringComparison.Ordinal))
            {
                return new EmissionDecision { RedispatchExisting = latest };
            }

            // Pembatalan klinis atas fakta yang tidak pernah diterima Billing tidak memiliki
            // konsekuensi finansial apa pun.
            if (milestoneKind == ClinicalMilestoneKind.ClinicalCancellation &&
                latest.DispatchStatus is ClinicalFactDispatchStatus.Rejected
                    or ClinicalFactDispatchStatus.SuppressedNoPriorCharge)
            {
                return new EmissionDecision { SuppressDispatch = true };
            }

            // CASE B — charge sudah terbentuk. Terbitkan revisi baru atas identitas yang sama.
            return new EmissionDecision();
        }

        private async Task<ClinicalFactEmissionResult> DispatchAsync(
            BilClinicalMilestoneFact fact,
            ClinicalMilestoneFactRequest normalized,
            Guid actorUserId,
            ClinicalFactEmissionKind successKind,
            CancellationToken cancellationToken)
        {
            // Identitas diambil dari ledger, isi material diambil dari permintaan yang sudah
            // dinormalisasi. Pemisahan ini disengaja: pengiriman ulang hanya terjadi ketika
            // sidik jari permintaan sama persis dengan yang tersimpan, sehingga isi materialnya
            // identik, sementara nilai yang dibaca kembali dari kolom jsonb sudah diformat ulang
            // PostgreSQL dan akan menghasilkan sidik jari berbeda bila dipakai apa adanya.
            var billingRequest = new RecognizeBillingMilestoneRequest
            {
                IdempotencyKey = fact.IdempotencyKey,
                MilestoneFactId = fact.MilestoneFactId,
                MilestoneFactVersion = fact.MilestoneFactVersion,
                EncounterId = fact.EncounterId,
                SourceContext = fact.SourceContext,
                SourceAggregateId = fact.SourceAggregateId,
                SourceItemId = fact.SourceItemId,
                EffectType = fact.EffectType,
                OccurredAt = normalized.OccurredAt,
                Quantity = normalized.Quantity,
                Unit = normalized.Unit,
                TariffSnapshot = normalized.TariffSnapshot,
                RuleSnapshot = normalized.RuleSnapshot,
                CorrelationId = fact.CorrelationId,
                CausationId = fact.CausationId
            };

            BillingServiceResult<RecognizeBillingMilestoneResponse>? billingResult = null;
            var dispatchFailed = false;

            try
            {
                billingResult = await _billingFolioService.RecognizeMilestoneAsync(
                    billingRequest,
                    actorUserId,
                    BillingFolioService.ClinicalFactConsumer,
                    BillingFolioService.RecognizeMilestoneOperation,
                    cancellationToken);
            }
            catch (Exception exception)
            {
                // Pengiriman terputus. Kita tidak tahu apakah Billing sempat commit, sehingga
                // tidak boleh disebut gagal maupun berhasil.
                dispatchFailed = true;
                await _loggerService.AuditAsync(
                    LogCategory,
                    "ClinicalFact.DispatchOutcomeUnknown",
                    "Penyerahan fakta klinis ke Billing tidak dapat dipastikan hasilnya.",
                    new
                    {
                        UserId = actorUserId,
                        ClinicalMilestoneFactId = fact.Id,
                        fact.SourceContext,
                        fact.MilestoneFactId,
                        fact.MilestoneFactVersion,
                        fact.EffectType,
                        fact.EncounterId,
                        fact.CorrelationId,
                        ExceptionType = exception.GetType().Name
                    });
            }

            return await ApplyDispatchOutcomeAsync(
                fact.Id,
                billingResult,
                dispatchFailed,
                actorUserId,
                successKind,
                cancellationToken);
        }

        private async Task<ClinicalFactEmissionResult> ApplyDispatchOutcomeAsync(
            Guid clinicalMilestoneFactId,
            BillingServiceResult<RecognizeBillingMilestoneResponse>? billingResult,
            bool dispatchFailed,
            Guid actorUserId,
            ClinicalFactEmissionKind successKind,
            CancellationToken cancellationToken)
        {
            // Dibaca ulang karena BillingFolioService dapat membersihkan change tracker ketika
            // menangani konflik concurrency.
            var fact = await _dbContext.Set<BilClinicalMilestoneFact>()
                .FirstOrDefaultAsync(x => x.Id == clinicalMilestoneFactId, cancellationToken);

            if (fact == null)
            {
                return ClinicalFactEmissionResult.Failure(
                    ClinicalFactEmissionKind.ReconciliationRequired,
                    "CLIN_FACT_NOT_FOUND",
                    "Catatan fakta klinis tidak ditemukan setelah penyerahan.");
            }

            var now = DateTime.UtcNow;
            fact.DispatchAttemptCount += 1;
            fact.DispatchedAt = now;
            fact.UpdateDateTime = now;
            fact.UpdateBy = actorUserId;
            fact.Version += 1;

            ClinicalFactEmissionKind resultKind;

            if (dispatchFailed || billingResult == null)
            {
                fact.DispatchStatus = ClinicalFactDispatchStatus.OutcomeUnknown;
                fact.BillingOutcomeCode = "BIL_OUTCOME_UNKNOWN";
                fact.BillingOutcomeMessage = "Penyerahan fakta tidak dapat dipastikan hasilnya.";
                resultKind = ClinicalFactEmissionKind.OutcomeUnknown;
            }
            else if (billingResult.Kind == BillingServiceResultKind.Success && billingResult.Value != null)
            {
                fact.DispatchStatus = ClinicalFactDispatchStatus.Dispatched;
                fact.BillingProcessingEffectId = billingResult.Value.ProcessingEffectId;
                fact.BillingFolioId = billingResult.Value.FolioId;
                fact.BillingChargeLineId = billingResult.Value.ChargeLineId;
                fact.BillingOutcomeCode = billingResult.Value.CalculationStatus?.ToString();
                fact.BillingOutcomeMessage = null;
                resultKind = successKind;
            }
            else if (string.Equals(billingResult.ErrorCode, "BIL_OUTCOME_UNKNOWN", StringComparison.Ordinal))
            {
                fact.DispatchStatus = ClinicalFactDispatchStatus.OutcomeUnknown;
                fact.BillingOutcomeCode = billingResult.ErrorCode;
                fact.BillingOutcomeMessage = Truncate(billingResult.ErrorMessage, 1000);
                resultKind = ClinicalFactEmissionKind.OutcomeUnknown;
            }
            else
            {
                fact.DispatchStatus = ClinicalFactDispatchStatus.Rejected;
                fact.BillingOutcomeCode = billingResult.ErrorCode;
                fact.BillingOutcomeMessage = Truncate(billingResult.ErrorMessage, 1000);
                resultKind = ClinicalFactEmissionKind.RejectedByBilling;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "ClinicalFact.Dispatch",
                "Menyerahkan fakta klinis ke Billing.",
                new
                {
                    UserId = actorUserId,
                    ClinicalMilestoneFactId = fact.Id,
                    fact.SourceContext,
                    fact.SourceAggregateId,
                    fact.SourceItemId,
                    fact.MilestoneFactId,
                    fact.MilestoneFactVersion,
                    fact.MilestoneKind,
                    fact.EffectType,
                    fact.EncounterId,
                    fact.DispatchStatus,
                    fact.DispatchAttemptCount,
                    fact.BillingFolioId,
                    fact.BillingChargeLineId,
                    fact.BillingProcessingEffectId,
                    fact.BillingOutcomeCode,
                    fact.CorrelationId,
                    fact.CausationId,
                    IdempotencyKeyReference = HashReference(fact.IdempotencyKey),
                    fact.OccurredAt
                });

            if (resultKind == ClinicalFactEmissionKind.RejectedByBilling)
            {
                return ClinicalFactEmissionResult.Failure(
                    resultKind,
                    fact.BillingOutcomeCode ?? "BIL_SOURCE_INVALID",
                    fact.BillingOutcomeMessage ?? "Billing menolak fakta klinis.",
                    fact.Id,
                    fact.MilestoneFactId,
                    fact.MilestoneFactVersion);
            }

            return ClinicalFactEmissionResult.Emitted(
                resultKind,
                fact.Id,
                fact.MilestoneFactId,
                fact.MilestoneFactVersion,
                fact.DispatchStatus,
                fact.BillingFolioId,
                fact.BillingChargeLineId,
                fact.BillingOutcomeCode,
                fact.BillingOutcomeMessage);
        }

        private async Task<BilClinicalMilestoneFact?> FindLatestFactAsync(
            ClinicalMilestoneFactRequest request,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<BilClinicalMilestoneFact>()
                .AsNoTracking()
                .Where(x => x.SourceContext == request.SourceContext &&
                            x.SourceAggregateId == request.SourceAggregateId &&
                            x.SourceItemId == request.SourceItemId &&
                            x.EffectType == request.EffectType &&
                            !x.IsDelete)
                .OrderByDescending(x => x.MilestoneFactVersion)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static (string Code, string Message)? Validate(
            ClinicalMilestoneFactRequest request,
            Guid actorUserId)
        {
            if (actorUserId == Guid.Empty)
                return ("CLIN_FACT_ACTOR_INVALID", "Aktor klinis tidak valid.");

            if (request.EncounterId == Guid.Empty)
                return ("CLIN_FACT_SOURCE_INVALID", "EncounterId wajib diisi.");

            if (request.SourceAggregateId == Guid.Empty)
                return ("CLIN_FACT_SOURCE_INVALID", "SourceAggregateId wajib diisi.");

            if (request.SourceItemId == Guid.Empty)
                return ("CLIN_FACT_SOURCE_INVALID", "SourceItemId tidak boleh Guid kosong.");

            var sourceContext = request.SourceContext?.Trim();
            var effectType = request.EffectType?.Trim();

            if (!BillingSourceContract.IsKnownSourceContext(sourceContext))
                return ("CLIN_FACT_SOURCE_INVALID", "SourceContext belum diizinkan oleh kontrak Billing.");

            if (!BillingSourceContract.IsAllowedEffectType(sourceContext, effectType))
                return ("CLIN_FACT_SOURCE_INVALID", "EffectType belum diizinkan untuk SourceContext tersebut.");

            if (request.OccurredAt == default)
                return ("CLIN_FACT_SOURCE_INVALID", "OccurredAt wajib diisi.");

            if (request.Quantity.HasValue && request.Quantity.Value <= 0)
                return ("CLIN_FACT_SOURCE_INVALID", "Quantity harus lebih besar dari nol.");

            if (request.Quantity.HasValue != !string.IsNullOrWhiteSpace(request.Unit))
                return ("CLIN_FACT_SOURCE_INVALID", "Quantity dan Unit wajib tersedia bersama-sama.");

            return ValidateSnapshot("TariffSnapshot", request.TariffSnapshot) ??
                   ValidateSnapshot("RuleSnapshot", request.RuleSnapshot);
        }

        private static (string Code, string Message)? ValidateSnapshot(string name, string? value)
        {
            if (value == null)
                return null;

            if (string.IsNullOrWhiteSpace(value) || value.Length > MaxSnapshotLength)
                return ("CLIN_FACT_SOURCE_INVALID", $"{name} harus berupa JSON object yang tidak kosong.");

            try
            {
                using var document = JsonDocument.Parse(value);
                if (document.RootElement.ValueKind is not JsonValueKind.Object)
                    return ("CLIN_FACT_SOURCE_INVALID", $"{name} harus berupa JSON object.");
            }
            catch (JsonException)
            {
                return ("CLIN_FACT_SOURCE_INVALID", $"{name} bukan JSON yang valid.");
            }

            return null;
        }

        private static ClinicalMilestoneFactRequest Normalize(ClinicalMilestoneFactRequest request)
        {
            return new ClinicalMilestoneFactRequest
            {
                SourceContext = request.SourceContext.Trim(),
                SourceAggregateId = request.SourceAggregateId,
                SourceItemId = request.SourceItemId,
                EffectType = request.EffectType.Trim(),
                EncounterId = request.EncounterId,
                OccurredAt = TruncateToDatabasePrecision(request.OccurredAt.ToUniversalTime()),
                Quantity = NormalizeToDatabaseScale(request.Quantity),
                Unit = string.IsNullOrWhiteSpace(request.Unit) ? null : request.Unit.Trim(),
                TariffSnapshot = NormalizeJson(request.TariffSnapshot),
                RuleSnapshot = NormalizeJson(request.RuleSnapshot),
                CorrelationId = request.CorrelationId == Guid.Empty ? null : request.CorrelationId,
                CausationId = request.CausationId == Guid.Empty ? null : request.CausationId
            };
        }

        /// <summary>
        /// Memotong waktu ke ketelitian mikrodetik, mengikuti ketelitian kolom
        /// <c>timestamp with time zone</c> PostgreSQL.
        ///
        /// Ini bukan kosmetik. Sidik jari permintaan Billing memuat <c>OccurredAt</c>. Tanpa
        /// pemotongan ini, nilai di memori berketelitian 100 nanodetik sedangkan nilai yang
        /// dibaca kembali dari database berketelitian mikrodetik, sehingga pengiriman ulang
        /// dari baris tersimpan menghasilkan sidik jari berbeda dan ditolak Billing sebagai
        /// <c>BIL_IDEMPOTENCY_CONFLICT</c> — persis kebalikan dari perilaku retry yang benar.
        /// </summary>
        private static DateTime TruncateToDatabasePrecision(DateTime value)
        {
            const long ticksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000;
            return new DateTime(
                value.Ticks - (value.Ticks % ticksPerMicrosecond),
                value.Kind);
        }

        /// <summary>
        /// Menyamakan skala desimal dengan kolom <c>numeric(18,6)</c>.
        ///
        /// Alasannya sama dengan <see cref="TruncateToDatabasePrecision"/>. Nilai <c>2</c> di
        /// memori berskala nol dan menghasilkan untaian <c>"2"</c>, sedangkan nilai yang sama
        /// setelah melewati kolom <c>numeric(18,6)</c> kembali sebagai <c>2.000000</c>.
        /// Keduanya menghasilkan sidik jari berbeda, sehingga baris ledger tidak dapat dipakai
        /// mengirim ulang secara idempotent. Penyeragaman skala dilakukan sekali di sini agar
        /// nilai yang dikirim dan nilai yang disimpan selalu sama persis.
        ///
        /// Penambahan dengan <c>0.000000m</c> dipakai karena hasil penjumlahan desimal memakai
        /// skala terbesar di antara kedua operan; <c>decimal.Round</c> saja tidak menaikkan
        /// skala.
        /// </summary>
        private static decimal? NormalizeToDatabaseScale(decimal? value)
        {
            const int databaseScale = 6;
            const decimal scaleAnchor = 0.000000m;

            return value.HasValue
                ? decimal.Round(value.Value, databaseScale, MidpointRounding.ToEven) + scaleAnchor
                : null;
        }

        private static string? NormalizeJson(string? value)
        {
            if (value == null)
                return null;

            using var document = JsonDocument.Parse(value);
            return JsonSerializer.Serialize(document.RootElement);
        }

        /// <summary>
        /// Kunci idempotency dibentuk dari identitas fakta dan nomor revisinya, bukan dari waktu
        /// atau nilai acak. Karena itu retry menghasilkan kunci yang sama persis.
        /// </summary>
        private static string BuildIdempotencyKey(Guid milestoneFactId, int milestoneFactVersion) =>
            $"CF-{milestoneFactId:N}-{milestoneFactVersion.ToString(CultureInfo.InvariantCulture)}";

        private static string BuildFingerprint(
            ClinicalMilestoneFactRequest request,
            ClinicalMilestoneKind milestoneKind)
        {
            var material = string.Join(
                "|",
                request.SourceContext,
                request.SourceAggregateId.ToString("D"),
                request.SourceItemId?.ToString("D") ?? string.Empty,
                request.EffectType,
                milestoneKind.ToString(),
                request.EncounterId.ToString("D"),
                request.OccurredAt.ToString("O", CultureInfo.InvariantCulture),
                request.Quantity?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                request.Unit ?? string.Empty,
                request.TariffSnapshot ?? string.Empty,
                request.RuleSnapshot ?? string.Empty);

            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(material)))
                .ToLowerInvariant();
        }

        private static string HashReference(string value) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty)))
                .ToLowerInvariant();

        private static string? Truncate(string? value, int maxLength) =>
            value == null || value.Length <= maxLength ? value : value[..maxLength];

        private static bool IsUniqueViolation(Exception exception)
        {
            var postgresException = exception as PostgresException ??
                                    exception.InnerException as PostgresException;

            return postgresException?.SqlState == PostgresErrorCodes.UniqueViolation;
        }
    }
}
