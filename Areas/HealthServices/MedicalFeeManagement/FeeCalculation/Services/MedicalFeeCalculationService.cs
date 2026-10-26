using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using System.Data;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Services
{
    /// <summary>
    /// Mesin perhitungan jasa medis bulanan (MDF-DES-007, 010, 012, 013, 014, 015; MDF-VAL-1.0 §5).
    /// Mengambil transaksi dari adapter, mencocokkan kesepakatan dan baris tarif berversi,
    /// menghitung alokasi nilai kotor, mencatat snapshot aturan, dan menampung layanan tertahan ke MdfUnresolvedService.
    /// Dijalankan dalam transaksi terisolasi Serializable dengan kontrol idempotensi.
    /// </summary>
    public class MedicalFeeCalculationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly MedicalFeeServiceSourceAdapter _sourceAdapter;
        private readonly ILogger<MedicalFeeCalculationService> _logger;

        public MedicalFeeCalculationService(
            ApplicationDbContext dbContext,
            MedicalFeeServiceSourceAdapter sourceAdapter,
            ILogger<MedicalFeeCalculationService> logger)
        {
            _dbContext = dbContext;
            _sourceAdapter = sourceAdapter;
            _logger = logger;
        }

        /// <summary>
        /// Menjalankan kalkulasi jasa medis untuk satu periode tertentu.
        /// </summary>
        public async Task<CalculationResultResponse> CalculatePeriodAsync(
            Guid periodId,
            CalculatePeriodRequest request,
            Guid userId,
            CancellationToken ct = default)
        {
            // Menjalankan transaksi serializable untuk mencegah konflik konkurensi (MDF-DES-007)
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, ct);

            try
            {
                var period = await _dbContext.MdfFeePeriods
                    .Include(p => p.ServiceFees.Where(sf => !sf.IsDelete))
                    .ThenInclude(sf => sf.Adjustments.Where(a => !a.IsDelete))
                    .FirstOrDefaultAsync(p => p.Id == periodId && !p.IsDelete, ct);

                if (period == null)
                {
                    throw new KeyNotFoundException($"Periode kalkulasi dengan ID '{periodId}' tidak ditemukan.");
                }

                // Validasi Status: Hanya Open atau Calculated yang dapat dihitung (MDF-STATE-1.0, MDF-DES-015)
                if (period.Status != MdfFeePeriodStatuses.Open && period.Status != MdfFeePeriodStatuses.Calculated)
                {
                    throw new InvalidOperationException(
                        $"Periode berstatus '{period.Status}' tidak dapat dihitung ulang. Perubahan hanya diperbolehkan melalui pengajuan koreksi (MDF-DES-015).");
                }

                // Concurrency Token Validation
                if (request.ExpectedRowVersion.HasValue && period.RowVersion != request.ExpectedRowVersion.Value)
                {
                    throw new DbUpdateConcurrencyException("MDF_ROWVERSION_MISMATCH: Data periode telah diperbarui oleh pengguna lain.");
                }

                _logger.LogInformation("Memulai kalkulasi jasa periode {Code} ({Start} - {End}) oleh user {User}",
                    period.PeriodCode, period.PeriodStart, period.PeriodEnd, userId);

                // =============================================================
                // 1. Pembersihan Data Kalkulasi Sebelumnya (MDF-DES-015)
                // =============================================================
                if (period.Status == MdfFeePeriodStatuses.Calculated)
                {
                    // Soft delete rincian jasa lama
                    var existingDetails = await _dbContext.MdfServiceFeeDetails
                        .Where(d => !d.IsDelete && _dbContext.MdfServiceFees
                            .Where(sf => sf.PeriodId == period.Id && !sf.IsDelete)
                            .Select(sf => sf.Id)
                            .Contains(d.ServiceFeeId))
                        .ToListAsync(ct);

                    foreach (var d in existingDetails)
                    {
                        d.IsDelete = true;
                        d.DeleteDateTime = DateTime.UtcNow;
                        d.DeleteBy = userId;
                    }

                    // Soft delete unresolved lama yang masih Open (perhitungan ulang menyegarkan daftar unresolved)
                    var existingUnresolved = await _dbContext.MdfUnresolvedServices
                        .Where(u => u.PeriodId == period.Id && !u.IsDelete && u.Status == MdfUnresolvedStatuses.Open)
                        .ToListAsync(ct);

                    foreach (var u in existingUnresolved)
                    {
                        u.IsDelete = true;
                        u.DeleteDateTime = DateTime.UtcNow;
                        u.DeleteBy = userId;
                    }

                    // Reset akumulasi kotor pada hasil jasa, pertahankan koreksi yang sudah disetujui (MDF-DES-015 butir 5)
                    foreach (var sf in period.ServiceFees)
                    {
                        sf.GrossAmount = 0m;
                        sf.FinalAmount = sf.AdjustmentAmount;
                    }
                }

                // =============================================================
                // 2. Ekstraksi Layanan dari Adapter Sumber (BE-MDF-009)
                // =============================================================
                var adaptedServices = await _sourceAdapter.ExtractServicesForPeriodAsync(
                    period.PeriodStart, period.PeriodEnd, ct);

                // =============================================================
                // 3. Pemuatan Kesepakatan Tarif & Aturan Aktif (MDF-DES-008, 009)
                // =============================================================
                var agreements = await _dbContext.MdfSharingAgreements
                    .AsNoTracking()
                    .Include(a => a.SourceContractHistory)
                    .Include(a => a.SharingRules.Where(r => !r.IsDelete))
                    .Where(a => !a.IsDelete && a.Status == MdfSharingAgreementStatuses.Active)
                    .ToListAsync(ct);

                // Pengelompokan kesepakatan: Key = "{PayeeType}_{PayeeReferenceId}"
                var agreementMap = agreements
                    .GroupBy(a => $"{a.PayeeType}_{a.PayeeReferenceId}")
                    .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

                // Map hasil jasa aktif: Key = "{PayeeType}_{PayeeReferenceId}"
                var serviceFeeMap = period.ServiceFees
                    .GroupBy(sf => $"{sf.PayeeType}_{sf.PayeeReferenceId}")
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                var newDetails = new List<MdfServiceFeeDetail>();
                var newUnresolved = new List<MdfUnresolvedService>();
                var unresolvedCounts = new Dictionary<string, int>
                {
                    [MdfUnresolvedReasons.PerformerMissing] = 0,
                    [MdfUnresolvedReasons.AgreementMissing] = 0,
                    [MdfUnresolvedReasons.RuleMissing] = 0,
                    [MdfUnresolvedReasons.ContractExpired] = 0,
                    [MdfUnresolvedReasons.RoleUnmapped] = 0,
                    [MdfUnresolvedReasons.SourceUnsupported] = 0
                };

                int feeCounter = period.ServiceFees.Count;

                // =============================================================
                // 4. Pemrosesan Setiap Layanan Medis
                // =============================================================
                foreach (var item in adaptedServices)
                {
                    // Kasus 1: Pelaksana Kosong (T-39, MDF_VAL 5.1)
                    if (item.IsPerformerMissing)
                    {
                        AddUnresolved(newUnresolved, unresolvedCounts, period.Id, item,
                            MdfUnresolvedReasons.PerformerMissing,
                            item.UnresolvedReasonDetail ?? "Pelaksana layanan medis belum ditentukan pada modul sumber.");
                        continue;
                    }

                    // Kasus 2: Peran Sumber Tidak Terpetakan (MDF_VAL 5.5)
                    if (item.IsRoleUnmapped)
                    {
                        AddUnresolved(newUnresolved, unresolvedCounts, period.Id, item,
                            MdfUnresolvedReasons.RoleUnmapped,
                            item.UnresolvedReasonDetail ?? "Peran pelaksana belum terpetakan ke data induk peran jasa medis.");
                        continue;
                    }

                    // Kasus 3: Kesepakatan Tarif Tidak Ada / Tidak Berlaku (T-40, MDF_VAL 5.2)
                    var payeeKey = $"{item.PayeeType}_{item.PayeeReferenceId}";
                    if (!agreementMap.TryGetValue(payeeKey, out var payeeAgreements) || !payeeAgreements.Any())
                    {
                        AddUnresolved(newUnresolved, unresolvedCounts, period.Id, item,
                            MdfUnresolvedReasons.AgreementMissing,
                            $"Tidak ditemukan kesepakatan tarif sharing aktif untuk {item.PayeeType} '{item.PayeeName}'.");
                        continue;
                    }

                    // Mencari kesepakatan yang mencakup tanggal layanan
                    var activeAgreement = payeeAgreements.FirstOrDefault(a =>
                        a.EffectiveStart <= item.ServiceDate &&
                        (a.EffectiveEnd == null || a.EffectiveEnd >= item.ServiceDate));

                    if (activeAgreement == null)
                    {
                        AddUnresolved(newUnresolved, unresolvedCounts, period.Id, item,
                            MdfUnresolvedReasons.AgreementMissing,
                            $"Kesepakatan tarif untuk {item.PayeeType} '{item.PayeeName}' tidak mencakup tanggal layanan {item.ServiceDate}.");
                        continue;
                    }

                    // Kasus 4: Tanggal Layanan Melewati Batas Masa Berlaku Kontrak HR (T-42, MDF_VAL 5.4)
                    var contract = activeAgreement.SourceContractHistory;
                    if (contract != null && contract.EndDate.HasValue && item.ServiceDate > contract.EndDate.Value)
                    {
                        AddUnresolved(newUnresolved, unresolvedCounts, period.Id, item,
                            MdfUnresolvedReasons.ContractExpired,
                            $"Kontrak kerja HR {contract.ContractNumber} telah berakhir pada {contract.EndDate.Value}.");
                        continue;
                    }

                    // Kasus 5: Pencocokan Baris Aturan Tarif (T-41, MDF-DES-009, MDF_VAL 5.3, 5.7)
                    var candidateRules = activeAgreement.SharingRules
                        .Where(r => r.RoleId == item.RoleId!.Value &&
                                    r.EffectiveStart <= item.ServiceDate &&
                                    (r.EffectiveEnd == null || r.EffectiveEnd >= item.ServiceDate))
                        .ToList();

                    if (!candidateRules.Any())
                    {
                        AddUnresolved(newUnresolved, unresolvedCounts, period.Id, item,
                            MdfUnresolvedReasons.RuleMissing,
                            $"Tidak ditemukan aturan tarif untuk peran '{item.RoleCode}' pada kesepakatan {activeAgreement.AgreementNumber} per {item.ServiceDate}.");
                        continue;
                    }

                    // Menentukan kekhususan aturan (Specificity: 4=Tariff, 3=Category, 2=Role Default)
                    var ruleGroups = candidateRules
                        .Select(r => new
                        {
                            Rule = r,
                            Specificity = r.TariffId.HasValue && item.TariffId.HasValue && r.TariffId == item.TariffId ? 4 :
                                          r.TariffCategoryId.HasValue && item.TariffCategoryId.HasValue && r.TariffCategoryId == item.TariffCategoryId ? 3 :
                                          !r.TariffId.HasValue && !r.TariffCategoryId.HasValue ? 2 : 1
                        })
                        .Where(x => x.Specificity > 1 || (!candidateRules.Any(cr => cr.TariffId.HasValue || cr.TariffCategoryId.HasValue)))
                        .OrderByDescending(x => x.Specificity)
                        .ToList();

                    if (!ruleGroups.Any())
                    {
                        AddUnresolved(newUnresolved, unresolvedCounts, period.Id, item,
                            MdfUnresolvedReasons.RuleMissing,
                            $"Tidak ada baris tarif dengan lingkup tindakan yang cocok untuk layanan '{item.ServiceDescription}'.");
                        continue;
                    }

                    var topSpecificity = ruleGroups.First().Specificity;
                    var bestMatches = ruleGroups.Where(x => x.Specificity == topSpecificity).Select(x => x.Rule).ToList();

                    // Konflik Aturan: Dua baris tarif pada tingkat kekhususan yang sama (MDF_VAL 5.7 -> 422)
                    if (bestMatches.Count > 1)
                    {
                        throw new InvalidOperationException(
                            $"MDF_RULE_OVERLAP: Ditemukan {bestMatches.Count} baris aturan tarif yang bertabrakan pada tingkat kekhususan {topSpecificity} untuk peran '{item.RoleCode}' pada tanggal {item.ServiceDate}.");
                    }

                    var matchedRule = bestMatches.First();

                    // =========================================================
                    // 5. Perhitungan Nilai Hak Jasa (MDF-DES-012, 013; MF-DEC-013, 014)
                    // =========================================================
                    var baseAmount = item.BaseAmount; // Kotor sebelum diskon
                    var sharingPercentage = matchedRule.SharingPercentage; // Snapshot
                    var calculatedAmount = Math.Round(baseAmount * sharingPercentage / 100.00m, 2, MidpointRounding.AwayFromZero);

                    // Menyiapkan atau mengambil entitas MdfServiceFee per penerima
                    if (!serviceFeeMap.TryGetValue(payeeKey, out var serviceFee))
                    {
                        feeCounter++;
                        var prefix = item.PayeeType == MdfPayeeTypes.Doctor ? "DOC" : "WFP";
                        var periodCodeClean = period.PeriodCode.Replace("-", "");
                        var feeNumber = $"SF-{periodCodeClean}-{prefix}-{feeCounter:D4}";

                        serviceFee = new MdfServiceFee
                        {
                            FeeNumber = feeNumber,
                            PeriodId = period.Id,
                            PayeeType = item.PayeeType,
                            PayeeReferenceId = item.PayeeReferenceId!.Value,
                            GrossAmount = 0m,
                            AdjustmentAmount = 0m,
                            FinalAmount = 0m,
                            Status = MdfServiceFeeStatuses.Calculated,
                            CalculatedAt = DateTime.UtcNow
                        };

                        _dbContext.MdfServiceFees.Add(serviceFee);
                        serviceFeeMap[payeeKey] = serviceFee;
                    }

                    // Akumulasi nilai kotor
                    serviceFee.GrossAmount += calculatedAmount;
                    serviceFee.FinalAmount = serviceFee.GrossAmount + serviceFee.AdjustmentAmount; // Penegakan CK_MdfServiceFee_Total
                    serviceFee.Status = MdfServiceFeeStatuses.Calculated;
                    serviceFee.CalculatedAt = DateTime.UtcNow;

                    // Membentuk rincian MdfServiceFeeDetail
                    var detail = new MdfServiceFeeDetail
                    {
                        ServiceFeeId = serviceFee.Id,
                        SourceDomain = item.SourceDomain,
                        SourceDetailId = item.SourceDetailId,
                        InvoiceItemId = item.InvoiceItemId,
                        TariffId = item.TariffId,
                        RoleId = item.RoleId!.Value,
                        BaseAmount = baseAmount,
                        SharingPercentage = sharingPercentage,
                        CalculatedAmount = calculatedAmount,
                        SharingRuleId = matchedRule.Id, // Wajib terisi (DoD BE-MDF-007)
                        ServiceDate = item.ServiceDate
                    };

                    newDetails.Add(detail);
                }

                // =============================================================
                // 6. Persistensi Rincian, Unresolved, dan Status Periode
                // =============================================================
                if (newDetails.Any())
                {
                    _dbContext.MdfServiceFeeDetails.AddRange(newDetails);
                }

                if (newUnresolved.Any())
                {
                    _dbContext.MdfUnresolvedServices.AddRange(newUnresolved);
                }

                period.Status = MdfFeePeriodStatuses.Calculated;
                period.CalculatedAt = DateTime.UtcNow;
                period.CalculatedBy = userId;
                period.RowVersion = Guid.NewGuid();

                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                _logger.LogInformation(
                    "Kalkulasi periode {Code} berhasil. Penerima: {Payees}, Rincian: {Details}, Unresolved: {Unresolved}, Total Kotor: {Gross}",
                    period.PeriodCode, serviceFeeMap.Count, newDetails.Count, newUnresolved.Count,
                    serviceFeeMap.Values.Sum(sf => sf.GrossAmount));

                return new CalculationResultResponse
                {
                    PeriodId = period.Id,
                    PeriodCode = period.PeriodCode,
                    Status = period.Status,
                    CalculatedAt = period.CalculatedAt.Value,
                    PayeeCount = serviceFeeMap.Count(sf => sf.Value.GrossAmount > 0 || sf.Value.FinalAmount > 0),
                    DetailCount = newDetails.Count,
                    GrossTotal = serviceFeeMap.Values.Sum(sf => sf.GrossAmount),
                    UnresolvedCount = newUnresolved.Count,
                    UnresolvedByReason = unresolvedCounts
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Kalkulasi jasa periode {PeriodId} gagal dibatalkan.", periodId);
                throw;
            }
        }

        private static void AddUnresolved(
            List<MdfUnresolvedService> list,
            Dictionary<string, int> counts,
            Guid periodId,
            AdaptedServiceItem item,
            string reason,
            string detail)
        {
            counts[reason] = counts.GetValueOrDefault(reason) + 1;

            list.Add(new MdfUnresolvedService
            {
                PeriodId = periodId,
                SourceDomain = item.SourceDomain,
                SourceDetailId = item.SourceDetailId,
                InvoiceItemId = item.InvoiceItemId,
                TariffId = item.TariffId,
                ServiceDate = item.ServiceDate,
                BaseAmount = item.BaseAmount > 0 ? item.BaseAmount : null,
                Reason = reason,
                ReasonDetail = detail,
                DetectedAt = DateTime.UtcNow,
                Status = MdfUnresolvedStatuses.Open
            });
        }
    }
}

