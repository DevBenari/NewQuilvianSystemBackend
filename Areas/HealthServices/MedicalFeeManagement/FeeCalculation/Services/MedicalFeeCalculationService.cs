using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
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
        private readonly MedicalFeeSharingRuleMatcher _ruleMatcher;
        private readonly ILogger<MedicalFeeCalculationService> _logger;

        public MedicalFeeCalculationService(
            ApplicationDbContext dbContext,
            MedicalFeeServiceSourceAdapter sourceAdapter,
            MedicalFeeSharingRuleMatcher ruleMatcher,
            ILogger<MedicalFeeCalculationService> logger)
        {
            _dbContext = dbContext;
            _sourceAdapter = sourceAdapter;
            _ruleMatcher = ruleMatcher;
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
                var existingUnresolved = new List<MdfUnresolvedService>();
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

                    // Muat daftar unresolved lama yang masih Open untuk rekonsiliasi T-44 dan T-45
                    existingUnresolved = await _dbContext.MdfUnresolvedServices
                        .Where(u => u.PeriodId == period.Id && !u.IsDelete && u.Status == MdfUnresolvedStatuses.Open)
                        .ToListAsync(ct);

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
                    if (contract != null && contract.EndDate.HasValue && item.ServiceDate > DateOnly.FromDateTime(contract.EndDate.Value))
                    {
                        AddUnresolved(newUnresolved, unresolvedCounts, period.Id, item,
                            MdfUnresolvedReasons.ContractExpired,
                            $"Kontrak kerja HR {contract.ContractNumber} telah berakhir pada {contract.EndDate.Value}.");
                        continue;
                    }

                    // Kasus 5: Pencocokan Baris Aturan Tarif (T-41, T-102, MDF-DES-009, MDF-DES-020, MDF_VAL 5.3, 5.7)
                    var matchResult = _ruleMatcher.MatchRule(
                        activeAgreement.SharingRules,
                        item.RoleId!.Value,
                        item.ServiceDate,
                        item.TariffId,
                        item.TariffCategoryId,
                        item.RoleCode,
                        item.ServiceDescription,
                        activeAgreement.AgreementNumber);

                    if (matchResult.Status == MedicalFeeSharingRuleMatchStatus.NoRule)
                    {
                        AddUnresolved(newUnresolved, unresolvedCounts, period.Id, item,
                            MdfUnresolvedReasons.RuleMissing,
                            matchResult.Message!);
                        continue;
                    }

                    if (matchResult.Status == MedicalFeeSharingRuleMatchStatus.Overlap)
                    {
                        throw new InvalidOperationException(matchResult.Message);
                    }

                    var matchedRule = matchResult.Rule!;

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

                // Rekonsiliasi Layanan Unresolved: T-44 dan T-45
                if (existingUnresolved.Any())
                {
                    var resolvedSourceKeys = newDetails
                        .Select(d => $"{d.SourceDomain}_{d.SourceDetailId}")
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    var newUnresolvedByKey = newUnresolved
                        .GroupBy(u => $"{u.SourceDomain}_{u.SourceDetailId}", StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                    foreach (var oldUnresolved in existingUnresolved)
                    {
                        var key = $"{oldUnresolved.SourceDomain}_{oldUnresolved.SourceDetailId}";

                        // T-45: Layanan yang sebelumnya unresolved kini berhasil dihitung -> status Resolved
                        if (resolvedSourceKeys.Contains(key))
                        {
                            oldUnresolved.Status = MdfUnresolvedStatuses.Resolved;
                            oldUnresolved.ResolvedAt = DateTime.UtcNow;
                            oldUnresolved.ResolvedBy = userId;
                            oldUnresolved.ResolutionNote = "Terselesaikan otomatis melalui pembaruan data dan kalkulasi ulang jasa medis.";
                            oldUnresolved.UpdateDateTime = DateTime.UtcNow;
                            oldUnresolved.UpdateBy = userId;
                            oldUnresolved.IsDelete = false;
                        }
                        // T-44: Layanan masih belum dapat dihitung -> perbarui agar tidak menghasilkan baris ganda
                        else if (newUnresolvedByKey.TryGetValue(key, out var updated))
                        {
                            oldUnresolved.Reason = updated.Reason;
                            oldUnresolved.ReasonDetail = updated.ReasonDetail;
                            oldUnresolved.BaseAmount = updated.BaseAmount;
                            oldUnresolved.DetectedAt = DateTime.UtcNow;
                            oldUnresolved.UpdateDateTime = DateTime.UtcNow;
                            oldUnresolved.UpdateBy = userId;
                            oldUnresolved.IsDelete = false;

                            // Hapus dari list newUnresolved agar tidak di-insert ganda
                            newUnresolved.Remove(updated);
                        }
                        else
                        {
                            // Layanan tidak lagi ditemukan pada modul sumber -> soft delete
                            oldUnresolved.IsDelete = true;
                            oldUnresolved.DeleteDateTime = DateTime.UtcNow;
                            oldUnresolved.DeleteBy = userId;
                        }
                    }
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

        // =====================================================================
        // Unresolved Services Management (BE-MDF-011, MDF-DES-014, MDF-API-1.0 §6)
        // =====================================================================

        /// <summary>
        /// Mengambil metadata filter dan opsi sort untuk tabel layanan belum terhitung.
        /// </summary>
        public UnresolvedFilterMetadataResponse GetUnresolvedFilterMetadata()
        {
            return new UnresolvedFilterMetadataResponse
            {
                DefaultFilter = new UnresolvedDefaultFilterResponse
                {
                    SortBy = "DetectedAt",
                    SortDirection = "desc",
                    PageNumber = 1,
                    PageSize = 25
                },
                SortOptions = new List<UnresolvedSortOptionResponse>
                {
                    new() { Value = "DetectedAt", Label = "Waktu Terdeteksi" },
                    new() { Value = "ServiceDate", Label = "Tanggal Layanan" },
                    new() { Value = "Reason", Label = "Alasan Belum Terhitung" },
                    new() { Value = "SourceDomain", Label = "Modul Sumber" },
                    new() { Value = "Status", Label = "Status Penyelesaian" },
                    new() { Value = "BaseAmount", Label = "Nilai Dasar (Bruto)" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                StatusOptions = new List<string>
                {
                    MdfUnresolvedStatuses.Open,
                    MdfUnresolvedStatuses.Resolved,
                    MdfUnresolvedStatuses.Waived
                },
                ReasonOptions = new List<string>
                {
                    MdfUnresolvedReasons.PerformerMissing,
                    MdfUnresolvedReasons.AgreementMissing,
                    MdfUnresolvedReasons.RuleMissing,
                    MdfUnresolvedReasons.ContractExpired,
                    MdfUnresolvedReasons.RoleUnmapped,
                    MdfUnresolvedReasons.SourceUnsupported
                },
                SourceDomainOptions = new List<string>
                {
                    MdfServiceSourceDomains.OperatingRoom,
                    MdfServiceSourceDomains.Procedure,
                    MdfServiceSourceDomains.Laboratory,
                    MdfServiceSourceDomains.Adhoc,
                    MdfServiceSourceDomains.AdhocCatalog
                },
                ResetButtonLabel = "Reset"
            };
        }

        /// <summary>
        /// Mengambil ringkasan rekapitulasi layanan belum terhitung per alasan dan modul sumber.
        /// </summary>
        public async Task<UnresolvedSummaryResponse> GetUnresolvedSummaryAsync(Guid? periodId, CancellationToken ct = default)
        {
            var query = _dbContext.MdfUnresolvedServices
                .AsNoTracking()
                .Where(u => !u.IsDelete);

            if (periodId.HasValue)
            {
                query = query.Where(u => u.PeriodId == periodId.Value);
            }

            var list = await query
                .Select(u => new { u.Status, u.Reason, u.SourceDomain })
                .ToListAsync(ct);

            var byReason = new Dictionary<string, int>
            {
                [MdfUnresolvedReasons.PerformerMissing] = 0,
                [MdfUnresolvedReasons.AgreementMissing] = 0,
                [MdfUnresolvedReasons.RuleMissing] = 0,
                [MdfUnresolvedReasons.ContractExpired] = 0,
                [MdfUnresolvedReasons.RoleUnmapped] = 0,
                [MdfUnresolvedReasons.SourceUnsupported] = 0
            };

            var bySourceDomain = new Dictionary<string, int>
            {
                [MdfServiceSourceDomains.OperatingRoom] = 0,
                [MdfServiceSourceDomains.Procedure] = 0,
                [MdfServiceSourceDomains.Laboratory] = 0,
                [MdfServiceSourceDomains.Adhoc] = 0,
                [MdfServiceSourceDomains.AdhocCatalog] = 0
            };

            int openCount = 0;
            int resolvedCount = 0;
            int waivedCount = 0;

            foreach (var item in list)
            {
                if (item.Status == MdfUnresolvedStatuses.Open) openCount++;
                else if (item.Status == MdfUnresolvedStatuses.Resolved) resolvedCount++;
                else if (item.Status == MdfUnresolvedStatuses.Waived) waivedCount++;

                if (!string.IsNullOrEmpty(item.Reason))
                {
                    byReason[item.Reason] = byReason.GetValueOrDefault(item.Reason) + 1;
                }

                if (!string.IsNullOrEmpty(item.SourceDomain))
                {
                    bySourceDomain[item.SourceDomain] = bySourceDomain.GetValueOrDefault(item.SourceDomain) + 1;
                }
            }

            return new UnresolvedSummaryResponse
            {
                Total = list.Count,
                OpenCount = openCount,
                ResolvedCount = resolvedCount,
                WaivedCount = waivedCount,
                ByReason = byReason,
                BySourceDomain = bySourceDomain
            };
        }

        /// <summary>
        /// Mengambil daftar layanan belum terhitung secara terpaginasi dengan filter dan pencarian.
        /// </summary>
        public async Task<PagedResult<UnresolvedServiceItemResponse>> GetUnresolvedPagedAsync(
            UnresolvedServicePagedQuery query,
            CancellationToken ct = default)
        {
            var baseQuery = _dbContext.MdfUnresolvedServices
                .AsNoTracking()
                .Include(u => u.Period)
                .Include(u => u.Tariff)
                .Where(u => !u.IsDelete);

            if (query.PeriodId.HasValue)
            {
                baseQuery = baseQuery.Where(u => u.PeriodId == query.PeriodId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Reason))
            {
                var reasonTrim = query.Reason.Trim();
                baseQuery = baseQuery.Where(u => u.Reason == reasonTrim);
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var statusTrim = query.Status.Trim();
                baseQuery = baseQuery.Where(u => u.Status == statusTrim);
            }

            if (!string.IsNullOrWhiteSpace(query.SourceDomain))
            {
                var domainTrim = query.SourceDomain.Trim();
                baseQuery = baseQuery.Where(u => u.SourceDomain == domainTrim);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = $"%{query.Search.Trim()}%";
                baseQuery = baseQuery.Where(u =>
                    EF.Functions.ILike(u.SourceDetailId, term) ||
                    (u.ReasonDetail != null && EF.Functions.ILike(u.ReasonDetail, term)) ||
                    (u.ResolutionNote != null && EF.Functions.ILike(u.ResolutionNote, term)) ||
                    (u.Period != null && EF.Functions.ILike(u.Period.PeriodCode, term)) ||
                    (u.Tariff != null && EF.Functions.ILike(u.Tariff.TariffName, term)));
            }

            var totalData = await baseQuery.CountAsync(ct);

            var isDesc = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            baseQuery = (query.SortBy?.ToLowerInvariant()) switch
            {
                "servicedate" => isDesc ? baseQuery.OrderByDescending(u => u.ServiceDate).ThenByDescending(u => u.DetectedAt)
                                        : baseQuery.OrderBy(u => u.ServiceDate).ThenBy(u => u.DetectedAt),
                "reason" => isDesc ? baseQuery.OrderByDescending(u => u.Reason).ThenByDescending(u => u.DetectedAt)
                                   : baseQuery.OrderBy(u => u.Reason).ThenBy(u => u.DetectedAt),
                "status" => isDesc ? baseQuery.OrderByDescending(u => u.Status).ThenByDescending(u => u.DetectedAt)
                                   : baseQuery.OrderBy(u => u.Status).ThenBy(u => u.DetectedAt),
                "sourcedomain" => isDesc ? baseQuery.OrderByDescending(u => u.SourceDomain).ThenByDescending(u => u.DetectedAt)
                                         : baseQuery.OrderBy(u => u.SourceDomain).ThenBy(u => u.DetectedAt),
                "baseamount" => isDesc ? baseQuery.OrderByDescending(u => u.BaseAmount).ThenByDescending(u => u.DetectedAt)
                                       : baseQuery.OrderBy(u => u.BaseAmount).ThenBy(u => u.DetectedAt),
                _ => isDesc ? baseQuery.OrderByDescending(u => u.DetectedAt)
                            : baseQuery.OrderBy(u => u.DetectedAt)
            };

            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 25;

            var items = await baseQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UnresolvedServiceItemResponse
                {
                    Id = u.Id,
                    PeriodId = u.PeriodId,
                    PeriodCode = u.Period != null ? u.Period.PeriodCode : string.Empty,
                    SourceDomain = u.SourceDomain,
                    SourceDetailId = u.SourceDetailId,
                    InvoiceItemId = u.InvoiceItemId,
                    TariffId = u.TariffId,
                    TariffName = u.Tariff != null ? u.Tariff.TariffName : null,
                    ServiceDate = u.ServiceDate,
                    BaseAmount = u.BaseAmount,
                    Reason = u.Reason,
                    ReasonDetail = u.ReasonDetail,
                    DetectedAt = u.DetectedAt,
                    Status = u.Status,
                    ResolvedBy = u.ResolvedBy,
                    ResolvedAt = u.ResolvedAt,
                    ResolutionNote = u.ResolutionNote
                })
                .ToListAsync(ct);

            return new PagedResult<UnresolvedServiceItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>
        /// Mengambil detail satu layanan belum terhitung beserta penjelasannya.
        /// </summary>
        public async Task<UnresolvedServiceItemResponse> GetUnresolvedByIdAsync(Guid id, CancellationToken ct = default)
        {
            var u = await _dbContext.MdfUnresolvedServices
                .AsNoTracking()
                .Include(u => u.Period)
                .Include(u => u.Tariff)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDelete, ct);

            if (u == null)
            {
                throw new KeyNotFoundException($"Layanan belum terhitung dengan ID '{id}' tidak ditemukan.");
            }

            return new UnresolvedServiceItemResponse
            {
                Id = u.Id,
                PeriodId = u.PeriodId,
                PeriodCode = u.Period != null ? u.Period.PeriodCode : string.Empty,
                SourceDomain = u.SourceDomain,
                SourceDetailId = u.SourceDetailId,
                InvoiceItemId = u.InvoiceItemId,
                TariffId = u.TariffId,
                TariffName = u.Tariff != null ? u.Tariff.TariffName : null,
                ServiceDate = u.ServiceDate,
                BaseAmount = u.BaseAmount,
                Reason = u.Reason,
                ReasonDetail = u.ReasonDetail,
                DetectedAt = u.DetectedAt,
                Status = u.Status,
                ResolvedBy = u.ResolvedBy,
                ResolvedAt = u.ResolvedAt,
                ResolutionNote = u.ResolutionNote
            };
        }

        /// <summary>
        /// Mengesampingkan layanan belum terhitung dengan catatan wajib (MDF-DES-014, MDF-VAL-1.0 §7.1, T-46..T-48).
        /// Status berubah menjadi Waived dan tidak lagi menahan penutupan periode (MDF-DES-016).
        /// </summary>
        public async Task<UnresolvedServiceItemResponse> WaiveUnresolvedAsync(
            Guid id,
            WaiveUnresolvedServiceRequest request,
            Guid userId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request?.ResolutionNote) || request.ResolutionNote.Trim().Length < 10)
            {
                throw new InvalidOperationException(
                    "MDF_WAIVER_NOTE_REQUIRED: Pengesampingan layanan belum terhitung wajib disertai catatan penyelesaian minimal 10 karakter (MDF-VAL-1.0 §7.1).");
            }

            var unresolved = await _dbContext.MdfUnresolvedServices
                .Include(u => u.Period)
                .Include(u => u.Tariff)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDelete, ct);

            if (unresolved == null)
            {
                throw new KeyNotFoundException($"Layanan belum terhitung dengan ID '{id}' tidak ditemukan.");
            }

            if (unresolved.Status != MdfUnresolvedStatuses.Open)
            {
                throw new InvalidOperationException(
                    $"MDF_UNRESOLVED_STATE_INVALID: Layanan dengan status '{unresolved.Status}' tidak dapat dikesampingkan. Hanya layanan berstatus 'Open' yang dapat dikesampingkan (MDF-VAL-1.0 §7.3).");
            }

            var now = DateTime.UtcNow;
            unresolved.Status = MdfUnresolvedStatuses.Waived;
            unresolved.ResolvedBy = userId;
            unresolved.ResolvedAt = now;
            unresolved.ResolutionNote = request.ResolutionNote.Trim();
            unresolved.UpdateDateTime = now;
            unresolved.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Layanan unresolved {Id} (alasan: {Reason}) berhasil dikesampingkan oleh user {UserId} dengan catatan: {Note}",
                unresolved.Id, unresolved.Reason, userId, unresolved.ResolutionNote);

            return new UnresolvedServiceItemResponse
            {
                Id = unresolved.Id,
                PeriodId = unresolved.PeriodId,
                PeriodCode = unresolved.Period != null ? unresolved.Period.PeriodCode : string.Empty,
                SourceDomain = unresolved.SourceDomain,
                SourceDetailId = unresolved.SourceDetailId,
                InvoiceItemId = unresolved.InvoiceItemId,
                TariffId = unresolved.TariffId,
                TariffName = unresolved.Tariff != null ? unresolved.Tariff.TariffName : null,
                ServiceDate = unresolved.ServiceDate,
                BaseAmount = unresolved.BaseAmount,
                Reason = unresolved.Reason,
                ReasonDetail = unresolved.ReasonDetail,
                DetectedAt = unresolved.DetectedAt,
                Status = unresolved.Status,
                ResolvedBy = unresolved.ResolvedBy,
                ResolvedAt = unresolved.ResolvedAt,
                ResolutionNote = unresolved.ResolutionNote
            };
        }
    }
}

