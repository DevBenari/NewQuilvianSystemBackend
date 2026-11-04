using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services
{
    /// <summary>Hasil penyerahan fakta tagihan pemeriksaan ke producer klinis.</summary>
    public sealed record OptBillingEmission(bool Recorded, string? Code, string? Message);

    /// <summary>
    /// Jalur tagihan Optik ke Billing lewat <see cref="ClinicalMilestoneFactProducer"/> (<c>BE-OPT-031</c>, <c>INT-OPT-04</c>,
    /// pola <c>LabOrderService</c>). Optik tidak pernah menyimpan harga; Billing membaca tarif tindakan (<c>BE-OPT-030</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Sekali per pemeriksaan</b> (<c>OPT-DEC-042</c>, <c>OPT-AC-024</c>). Isi fakta disusun hanya dari data yang tidak berubah
    /// sesudah final (id, nomor, waktu final, tindakan), sehingga penyerahan ulang menghasilkan sidik jari yang sama dan
    /// producer menjawab <c>Replayed</c> tanpa revisi baru. Revisi hasil ukur tidak menerbitkan fakta (<c>OPT-DEC-047</c>).
    /// </para>
    /// <para>
    /// <b>Sesudah commit.</b> Producer menolak dipanggil di dalam transaksi. Bila Billing tidak menjawab, fakta tetap tersimpan
    /// <c>Pending</c> dan dikirim ulang <c>ClinicalFactDispatchWorker</c>; pemeriksaan tetap final.
    /// </para>
    /// <para>
    /// <b>Tindakan "Pemeriksaan Refraksi"</b> dicari dari kode di <see cref="RefractionExamProcedureCodeKey"/> pada
    /// <see cref="MstProcedure"/> aktif. Kode kosong atau tindakan tidak ada → fakta tidak diterbitkan dan pemeriksaan
    /// tercatat belum tertagih, tanpa menebak tindakan lain.
    /// </para>
    /// </remarks>
    public class OptBillingGateway
    {
        public const string RefractionExamProcedureCodeKey = "OpticalManagement:Billing:RefractionExamProcedureCode";
        public const string ExamUnit = "Pemeriksaan";

        public const string ProcedureNotConfiguredCode = "OPT-BIL-PROCEDURE-MISSING";
        public const string ProcedureNotConfiguredMessage =
            "Tarif pemeriksaan belum dapat ditagihkan karena tindakan Pemeriksaan Refraksi belum diatur. Hubungi bagian Master Data.";
        public const string BillingUnavailableCode = "OPT-BIL-UNAVAILABLE";

        private readonly ApplicationDbContext _dbContext;
        private readonly ClinicalMilestoneFactProducer _producer;
        private readonly LoggerService _loggerService;
        private readonly string? _procedureCode;
        private readonly BillingSourceTariffResolver _tariffResolver;
        private readonly InpServiceClassResolver _serviceClassResolver;
        private readonly BillingClinicalChargeBridgeService _bridge;
        private readonly BillingFinalizationService? _finalization;

        public const string ItemUnit = "unit";
        public const string ItemMilestoneCancelled = "CANCELLED";
        public const string OrderBillingUnavailableMessage =
            "Pesanan belum dapat ditagihkan karena layanan tagihan sedang tidak dapat diakses. Pesanan tetap Draf; silakan coba lagi.";

        public OptBillingGateway(
            ApplicationDbContext dbContext,
            ClinicalMilestoneFactProducer producer,
            LoggerService loggerService,
            IConfiguration configuration,
            BillingSourceTariffResolver tariffResolver,
            InpServiceClassResolver serviceClassResolver,
            BillingClinicalChargeBridgeService bridge,
            BillingFinalizationService? finalization = null)
        {
            _finalization = finalization;
            _dbContext = dbContext;
            _producer = producer;
            _loggerService = loggerService;
            _procedureCode = configuration[RefractionExamProcedureCodeKey]?.Trim();
            _tariffResolver = tariffResolver;
            _serviceClassResolver = serviceClassResolver;
            _bridge = bridge;
        }

        /// <summary>
        /// Menyerahkan fakta tarif satu pemeriksaan final. <c>Recorded</c> benar bila producer sudah mencatat faktanya
        /// (terkirim, diulang, atau menunggu kirim ulang); sejak itu penyerahan ke Billing menjadi tanggung jawab producer.
        /// </summary>
        public async Task<OptBillingEmission> EmitExamChargeAsync(OptRefractionExam exam, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(exam);
            if (exam.Status != OptRefractionExamStatus.Final || exam.FinalizedAt is not { } finalizedAt)
                return new OptBillingEmission(false, OptErrorCodes.InvalidState, "Hanya pemeriksaan final yang ditagihkan.");

            try
            {
                var procedureId = string.IsNullOrEmpty(_procedureCode)
                    ? null
                    : await _dbContext.Set<MstProcedure>().AsNoTracking()
                        .Where(x => x.ProcedureCode == _procedureCode && x.IsActive && !x.IsDelete)
                        .OrderBy(x => x.CreateDateTime)
                        .Select(x => (Guid?)x.Id)
                        .FirstOrDefaultAsync(cancellationToken);
                if (procedureId is null)
                {
                    await _loggerService.WarningAsync("Optical", "OptBilling.ExamCharge",
                        $"Tarif pemeriksaan {exam.ExamNumber} belum ditagihkan: tindakan Pemeriksaan Refraksi (kode '{_procedureCode}') tidak ditemukan.",
                        new { ExamNumber = exam.ExamNumber });
                    return new OptBillingEmission(false, ProcedureNotConfiguredCode, ProcedureNotConfiguredMessage);
                }

                var hasil = await _producer.EmitChargeEligibilityAsync(new ClinicalMilestoneFactRequest
                {
                    SourceContext = BillingSourceContract.OpticalSourceContext,
                    EffectType = BillingSourceContract.OpticalExamChargeEffectType,
                    SourceAggregateId = exam.Id,
                    SourceItemId = procedureId,
                    EncounterId = exam.EncounterId,
                    OccurredAt = DateTime.SpecifyKind(finalizedAt, DateTimeKind.Utc),
                    Quantity = 1m,
                    Unit = ExamUnit,
                    RuleSnapshot = RuleSnapshot(exam.ExamNumber),
                    // Korelasi per pemeriksaan, untuk penelusuran. Keidentikan penyerahan ulang dijaga isi material
                    // (waktu final, tindakan, jumlah, RuleSnapshot), bukan oleh ruas ini.
                    CorrelationId = exam.Id,
                }, actorUserId, cancellationToken);

                // Emitted/Replayed: diterima. OutcomeUnknown: fakta tersimpan, hasil penyerahan belum pasti, dan
                // ClinicalFactDispatchWorker mengirim ulang dengan identitas yang sama. Ketiganya aman secara klinis.
                if (hasil.ClinicalMilestoneFactId is not null && hasil.IsClinicallySafe
                    && hasil.Kind != ClinicalFactEmissionKind.SuppressedNoPriorCharge)
                    return new OptBillingEmission(true, null, null);

                await _loggerService.WarningAsync("Optical", "OptBilling.ExamCharge",
                    $"Tarif pemeriksaan {exam.ExamNumber} belum diterima producer: {hasil.Kind} {hasil.Code} {hasil.Message}",
                    new { ExamNumber = exam.ExamNumber });
                return new OptBillingEmission(false, hasil.Code, hasil.Message);
            }
            catch (Exception ex) when (ex is not OperationCanceledException and not InvalidOperationException)
            {
                // Pemeriksaan sudah final dan tetap final; penandanya kosong sehingga penyerahan dicoba lagi.
                await _loggerService.WarningAsync("Optical", "OptBilling.ExamCharge",
                    $"Tarif pemeriksaan {exam.ExamNumber} gagal diserahkan ({ex.GetType().Name}); akan dicoba lagi.",
                    new { ExamNumber = exam.ExamNumber });
                return new OptBillingEmission(false, BillingUnavailableCode,
                    "Tarif pemeriksaan belum masuk tagihan karena layanan tagihan sedang tidak dapat diakses.");
            }
        }

        // ---------------------------------------------------------------- barang pesanan (BE-OPT-032)

        /// <summary>
        /// Memeriksa tarif tiap baris <b>sebelum</b> menerbitkan fakta, dengan resolver Billing yang sama dengan jembatan, supaya
        /// tarif yang belum ada ditolak <c>400 OPT-VAL-045</c> tanpa sebagian baris terlanjur masuk tagihan.
        /// </summary>
        public async Task<OptResult<bool>> CheckItemTariffsAsync(OptOrder order, IReadOnlyCollection<OptOrderItem> items, DateTime occurredAt, CancellationToken cancellationToken = default)
        {
            try
            {
                var clinicId = await _dbContext.Set<RegPatientEncounter>().AsNoTracking()
                    .Where(x => x.Id == order.EncounterId).Select(x => x.ClinicId).FirstOrDefaultAsync(cancellationToken);
                var kelas = await _serviceClassResolver.ResolveAsync(order.EncounterId, occurredAt, cancellationToken);
                foreach (var item in items)
                {
                    var tarif = await _tariffResolver.ResolveAsync(new BillingTariffResolutionRequest(
                        BillingBridgeSourceDomains.Optical, order.Id, item.Id, item.Quantity, clinicId, kelas.Id,
                        DateTime.SpecifyKind(occurredAt, DateTimeKind.Utc), null, BillingSourceContract.OpticalItemChargeEffectType), cancellationToken);
                    if (tarif.Kind == BillingTariffResolutionKind.Resolved)
                        continue;
                    if (tarif.Code == BillingBridgeCodes.TariffNotFound || tarif.Code == BillingBridgeCodes.TariffAmbiguous)
                        return OptResult<bool>.Invalid(OptErrorCodes.Val045, await TariffMissingMessageAsync(item, cancellationToken));
                    return OptResult<bool>.Conflict(OptErrorCodes.InvalidState, tarif.Message ?? "Barang ini tidak dapat ditagihkan.");
                }
                return OptResult<bool>.Ok(true);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return OptResult<bool>.Unavailable(OptErrorCodes.BillingUnavailable, OrderBillingUnavailableMessage);
            }
        }

        /// <summary>
        /// Menerbitkan fakta <c>OpticalItemCharge</c> per baris lalu memastikan Billing sudah memasukkannya ke tagihan
        /// (<c>OPT-STATE-v1</c>: fakta tagihan diterima Billing). Penerbitan ulang dengan isi sama menjadi <c>Replayed</c>, tanpa
        /// baris ganda. Satu baris saja yang belum masuk berarti <c>503</c> dan pemanggil membiarkan pesanan tetap <c>Draft</c>.
        /// </summary>
        public async Task<OptResult<bool>> EmitItemChargesAsync(
            OptOrder order, IReadOnlyCollection<OptOrderItem> items, string milestone, DateTime occurredAt, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var item in items)
                {
                    var hasil = await _producer.EmitChargeEligibilityAsync(ItemFact(order, item, milestone, occurredAt), actorUserId, cancellationToken);
                    if (!hasil.IsClinicallySafe || hasil.ClinicalMilestoneFactId is null)
                    {
                        await _loggerService.WarningAsync("Optical", "OptBilling.ItemCharge",
                            $"Baris {item.LineNo} pesanan {order.OrderNumber} belum diterima producer: {hasil.Kind} {hasil.Code}.");
                        return OptResult<bool>.Unavailable(OptErrorCodes.BillingUnavailable, OrderBillingUnavailableMessage);
                    }
                }

                foreach (var item in items)
                {
                    var keadaan = await _bridge.GetChargeStateAsync(BillingSourceContract.OpticalSourceContext, order.Id, item.Id, cancellationToken);
                    if (keadaan.State == BillingSourceChargeStates.InInvoice)
                        continue;
                    if (keadaan.Code is BillingBridgeCodes.TariffNotFound or BillingBridgeCodes.TariffAmbiguous)
                        return OptResult<bool>.Invalid(OptErrorCodes.Val045, await TariffMissingMessageAsync(item, cancellationToken));
                    await _loggerService.WarningAsync("Optical", "OptBilling.ItemCharge",
                        $"Baris {item.LineNo} pesanan {order.OrderNumber} belum masuk tagihan: {keadaan.State} {keadaan.Code}.");
                    return OptResult<bool>.Unavailable(OptErrorCodes.BillingUnavailable, OrderBillingUnavailableMessage);
                }
                return OptResult<bool>.Ok(true);
            }
            catch (Exception ex) when (ex is not OperationCanceledException and not InvalidOperationException)
            {
                return OptResult<bool>.Unavailable(OptErrorCodes.BillingUnavailable, OrderBillingUnavailableMessage);
            }
        }

        /// <summary>
        /// Menerbitkan pembatalan untuk baris yang pernah punya fakta tagihan (<c>INT-OPT-05</c>). Billing memilih void
        /// (masih <c>ORDERED</c>) atau adjustment kredit. Mengembalikan baris yang pembatalannya belum tercatat producer.
        /// </summary>
        public async Task<IReadOnlyList<OptOrderItem>> EmitItemCancellationsAsync(
            OptOrder order, IReadOnlyCollection<OptOrderItem> items, DateTime occurredAt, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var gagal = new List<OptOrderItem>();
            foreach (var item in items)
            {
                try
                {
                    var pernahDitagih = await _dbContext.Set<CliClinicalMilestoneFact>().AsNoTracking()
                        .AnyAsync(x => x.SourceContext == BillingSourceContract.OpticalSourceContext
                                       && x.SourceAggregateId == order.Id && x.SourceItemId == item.Id && !x.IsDelete, cancellationToken);
                    if (!pernahDitagih)
                        continue;

                    var hasil = await _producer.EmitClinicalCancellationAsync(
                        ItemFact(order, item, ItemMilestoneCancelled, occurredAt), actorUserId, cancellationToken);
                    if (!hasil.IsClinicallySafe)
                        gagal.Add(item);
                }
                catch (Exception ex) when (ex is not OperationCanceledException and not InvalidOperationException)
                {
                    gagal.Add(item);
                }
            }

            if (gagal.Count > 0)
                await _loggerService.WarningAsync("Optical", "OptBilling.ItemCancellation",
                    $"Pembatalan {gagal.Count} baris pesanan {order.OrderNumber} belum diterima producer.");
            return gagal;
        }

        /// <summary>
        /// Pemicu penutupan kunjungan (<c>INT-OPT-02</c>, <c>OPT-DEC-043</c>): bila seluruh pesanan di kunjungan sudah terminal,
        /// meminta Billing memfinalisasi tagihan yang lunas. Billing sendiri yang memeriksa semua baris selesai, kalkulasi
        /// terkini, dan sisa nol; finalisasinya menutup kunjungan. Gagal atau belum memenuhi syarat bukan kesalahan Optik.
        /// </summary>
        public async Task<(bool Completed, string? Reason)> TryCompleteVisitBillingAsync(Guid encounterId, Guid orderId, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            if (_finalization is null)
                return (false, null);
            try
            {
                var masihBerjalan = await _dbContext.OptOrders.AsNoTracking().AnyAsync(x => x.EncounterId == encounterId && !x.IsDelete
                    && x.Status != OptOrderStatus.HandedOver && x.Status != OptOrderStatus.Cancelled && x.Status != OptOrderStatus.NotCollected, cancellationToken);
                if (masihBerjalan)
                    return (false, null);

                var invoiceId = await _dbContext.Set<BillingManagement.Billing.Models.BilInvoice>().AsNoTracking()
                    .Where(x => x.EncounterId == encounterId && !x.IsDelete && x.Status == BillingManagement.Billing.Models.BillingInvoiceStatuses.Open)
                    .Select(x => (Guid?)x.Id).FirstOrDefaultAsync(cancellationToken);
                if (invoiceId is null)
                    return (false, null);

                return await _finalization.TryCompleteSettledInvoiceAsync(invoiceId.Value, actorUserId, DateTimeOffset.UtcNow, orderId, orderId, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                await _loggerService.WarningAsync("Optical", "OptBilling.CompleteVisit",
                    $"Finalisasi tagihan kunjungan sesudah pesanan selesai gagal ({ex.GetType().Name}); kasir dapat memfinalisasi manual.");
                return (false, null);
            }
        }

        /// <summary><c>RuleSnapshot</c> fakta barang (<c>INT-OPT-04</c>): <c>{ milestone, orderNumber, lineNo, itemCategory, lensSupply }</c>.</summary>
        public static string ItemRuleSnapshot(string milestone, string orderNumber, OptOrderItem item) =>
            System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["milestone"] = milestone,
                ["orderNumber"] = orderNumber,
                ["lineNo"] = item.LineNo,
                ["itemCategory"] = item.ItemCategory.ToString(),
                ["lensSupply"] = item.LensSupply?.ToString(),
            });

        private static ClinicalMilestoneFactRequest ItemFact(OptOrder order, OptOrderItem item, string milestone, DateTime occurredAt) => new()
        {
            SourceContext = BillingSourceContract.OpticalSourceContext,
            EffectType = BillingSourceContract.OpticalItemChargeEffectType,
            SourceAggregateId = order.Id,
            SourceItemId = item.Id,
            EncounterId = order.EncounterId,
            OccurredAt = DateTime.SpecifyKind(occurredAt, DateTimeKind.Utc),
            Quantity = item.Quantity,
            Unit = ItemUnit,
            RuleSnapshot = ItemRuleSnapshot(milestone, order.OrderNumber, item),
            CorrelationId = order.Id,
        };

        private async Task<string> TariffMissingMessageAsync(OptOrderItem item, CancellationToken cancellationToken)
        {
            var kode = await _dbContext.Set<MstDrug>().AsNoTracking()
                .Where(x => x.Id == item.DrugId).Select(x => x.DrugCode).FirstOrDefaultAsync(cancellationToken);
            return $"Tarif barang {kode ?? item.DrugId.ToString("D")} belum ada. Hubungi bagian tarif.";
        }

        /// <summary><c>RuleSnapshot</c> fakta pemeriksaan (<c>INT-OPT-04</c>): <c>{ milestone: "ExamFinalized", examNumber }</c>.</summary>
        public static string RuleSnapshot(string examNumber) =>
            string.Create(CultureInfo.InvariantCulture,
                $"{{\"milestone\":\"{BillingSourceContract.OpticalMilestoneExamFinalized}\",\"examNumber\":{System.Text.Json.JsonSerializer.Serialize(examNumber)}}}");
    }
}
