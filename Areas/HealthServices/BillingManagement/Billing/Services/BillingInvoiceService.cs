using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public sealed class BillingInvoiceService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing";
    private readonly ApplicationDbContext _dbContext;
    private readonly IBillingChargeSourceAdapter _sourceAdapter;
    private readonly BillingNumberSeriesService _numberSeries;
    private readonly BillingCalculationService _calculationService;
    private readonly LoggerService _loggerService;

    private readonly InsuranceCoverageService _insuranceCoverageService;

    public BillingInvoiceService(
        ApplicationDbContext dbContext,
        IBillingChargeSourceAdapter sourceAdapter,
        BillingNumberSeriesService numberSeries,
        BillingCalculationService calculationService,
        LoggerService loggerService,
        InsuranceCoverageService insuranceCoverageService)
    {
        _dbContext = dbContext;
        _sourceAdapter = sourceAdapter;
        _numberSeries = numberSeries;
        _calculationService = calculationService;
        _loggerService = loggerService;
        _insuranceCoverageService = insuranceCoverageService;
    }

    // BKC-DEC-060: preview read-only, tanpa efek samping - dipakai layar entri sebelum item
    // benar-benar ditambahkan. Menyeluruhnya dipakai InsuranceCoverageService.ResolveTariffAsync
    // yang sudah ada (Clinical Management, satu-satunya tempat kalkulasi coverage) - logika
    // matching rule/tarif asuransi TIDAK ditulis ulang di sini. Hasil boleh berbeda dari
    // kalkulasi final invoice sungguhan (RegistrationBillingCoverageAdapter, BE-BKC-021) karena
    // preview ini tidak memperhitungkan diskon/coverage waterfall di tingkat invoice.
    public async Task<CatalogChargeCoveragePreviewResponse> GetCatalogChargeCoveragePreviewAsync(
        Guid encounterId,
        Guid tariffId,
        decimal quantity,
        CancellationToken cancellationToken)
    {
        var result = await _insuranceCoverageService.ResolveTariffAsync(
            encounterId, tariffId, quantity, cancellationToken: cancellationToken);
        if (!result.IsValid)
            throw new BillingInvoiceValidationException(
                result.ErrorMessage ?? "Tarif atau kunjungan tidak valid untuk preview coverage.");

        return new CatalogChargeCoveragePreviewResponse
        {
            CoverageStatus = result.CoverageStatus,
            CoveragePercent = result.CoveragePercent,
            UnitPrice = result.UnitPrice,
            TotalPrice = result.TotalPrice,
            CoveredAmount = result.CoveredAmount,
            PatientPayAmount = result.PatientPayAmount,
            IsNeedApproval = result.IsNeedApproval,
            CoverageNote = result.CoverageNote
        };
    }

    public async Task<PagedResult<InvoiceSummaryResponse>> GetPagedAsync(BillingInvoiceQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.BilInvoices.AsNoTracking().Where(x => !x.IsDelete);
        if (request.EncounterId.HasValue) query = query.Where(x => x.EncounterId == request.EncounterId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim().ToUpperInvariant();
            query = query.Where(x => x.Status == status);
        }
        if (!string.IsNullOrWhiteSpace(request.ServiceType))
        {
            var serviceType = request.ServiceType.Trim().ToUpperInvariant();
            query = query.Where(x => x.ServiceType == serviceType);
        }
        // Identitas pasien di-join dari encounter. Left join dipertahankan lewat DefaultIfEmpty
        // supaya invoice dengan encounter yang tidak terbaca tetap muncul di daftar - hilang dari
        // daftar tagihan jauh lebih berbahaya daripada tampil tanpa nama.
        var joined =
            from invoice in query
            join encounter in _dbContext.RegPatientEncounters.AsNoTracking()
                on invoice.EncounterId equals encounter.Id into encounterGroup
            from encounter in encounterGroup.DefaultIfEmpty()
            join patient in _dbContext.MstPatients.AsNoTracking()
                on encounter.PatientId equals patient.Id into patientGroup
            from patient in patientGroup.DefaultIfEmpty()
            select new { invoice, patient };

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            joined = joined.Where(x =>
                x.invoice.InvoiceNumber.ToUpper().Contains(search) ||
                (x.patient != null && x.patient.FullName.ToUpper().Contains(search)) ||
                (x.patient != null && x.patient.MedicalRecordNumber.ToUpper().Contains(search)));
        }

        var total = await joined.CountAsync(cancellationToken);
        var items = await joined.OrderByDescending(x => x.invoice.CreateDateTime)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new InvoiceSummaryResponse
            {
                Id = x.invoice.Id,
                EncounterId = x.invoice.EncounterId,
                InvoiceNumber = x.invoice.InvoiceNumber,
                PatientName = x.patient != null ? x.patient.FullName : string.Empty,
                MedicalRecordNumber = x.patient != null ? x.patient.MedicalRecordNumber : string.Empty,
                ServiceType = x.invoice.ServiceType,
                Status = x.invoice.Status,
                CurrentCalculationVersion = x.invoice.CurrentCalculationVersion,
                RunningGrossAmount = x.invoice.Items.Where(i => !i.IsDelete && i.Status != BillingInvoiceItemStatuses.Voided)
                    .Sum(i => i.Quantity * i.UnitPrice),
                ActiveItemCount = x.invoice.Items.Count(i => !i.IsDelete && i.Status != BillingInvoiceItemStatuses.Voided),
                CreateDateTime = x.invoice.CreateDateTime,
                RowVersion = x.invoice.RowVersion
            }).ToListAsync(cancellationToken);
        return new PagedResult<InvoiceSummaryResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    // Ad-hoc, di luar roadmap, permintaan langsung pengguna: halaman "Riwayat Pembayaran" -
    // BillingPaymentHistoryDtos.cs § komentar header untuk keputusan scope lengkap. Dua-pass
    // (pola sama dengan GetActiveEncounterOptionsAsync di file ini): pass pertama menentukan
    // halaman invoice yang dipaginasi, pass kedua mengambil data pendukung per batch ID (bukan
    // correlated subquery per baris) supaya query tetap sederhana dan mudah diverifikasi.
    public async Task<PagedResult<PaymentHistoryItemResponse>> GetPaymentHistoryAsync(
        PaymentHistoryQuery request, CancellationToken cancellationToken)
    {
        // Invoice yang punya minimal satu tender SUCCEEDED pada settlement bertujuan
        // InvoicePayment (bukan DepositTopUp - top-up deposit bukan "pembayaran tagihan").
        var invoiceIdsWithPayment = _dbContext.BilSettlements.AsNoTracking()
            .Where(s => s.InvoiceId != null && s.Purpose == BillingSettlementPurposes.InvoicePayment
                && s.Tenders.Any(t => t.Status == BillingTenderStatuses.Succeeded))
            .Select(s => s.InvoiceId!.Value)
            .Distinct();

        var query = _dbContext.BilInvoices.AsNoTracking().Where(x => !x.IsDelete)
            .Where(x => invoiceIdsWithPayment.Contains(x.Id));

        if (!string.IsNullOrWhiteSpace(request.ServiceType))
        {
            var serviceType = request.ServiceType.Trim().ToUpperInvariant();
            query = query.Where(x => x.ServiceType == serviceType);
        }

        var joined =
            from invoice in query
            join encounter in _dbContext.RegPatientEncounters.AsNoTracking()
                on invoice.EncounterId equals encounter.Id into encounterGroup
            from encounter in encounterGroup.DefaultIfEmpty()
            join patient in _dbContext.MstPatients.AsNoTracking()
                on encounter.PatientId equals patient.Id into patientGroup
            from patient in patientGroup.DefaultIfEmpty()
            select new { invoice, encounter, patient };

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            joined = joined.Where(x =>
                x.invoice.InvoiceNumber.ToUpper().Contains(search) ||
                (x.patient != null && x.patient.FullName.ToUpper().Contains(search)) ||
                (x.patient != null && x.patient.MedicalRecordNumber.ToUpper().Contains(search)));
        }

        if (request.VisitDateFrom.HasValue)
            joined = joined.Where(x => x.encounter != null && x.encounter.EncounterDate >= request.VisitDateFrom.Value);
        if (request.VisitDateTo.HasValue)
            joined = joined.Where(x => x.encounter != null && x.encounter.EncounterDate <= request.VisitDateTo.Value);

        var total = await joined.CountAsync(cancellationToken);
        var page = await joined
            .OrderByDescending(x => x.encounter != null ? x.encounter.EncounterDate : x.invoice.CreateDateTime)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new
            {
                x.invoice.Id,
                x.invoice.InvoiceNumber,
                x.invoice.ServiceType,
                EncounterId = x.invoice.EncounterId,
                VisitDate = x.encounter != null ? x.encounter.EncounterDate : (DateTime?)null,
                PatientName = x.patient != null ? x.patient.FullName : string.Empty,
                MedicalRecordNumber = x.patient != null ? x.patient.MedicalRecordNumber : string.Empty
            })
            .ToListAsync(cancellationToken);

        var invoiceIds = page.Select(x => x.Id).ToList();
        var encounterIds = page.Select(x => x.EncounterId).Distinct().ToList();

        // Nama/tipe penjamin dari snapshot pendaftaran (sama seperti GetActiveEncounterOptionsAsync
        // di file ini) - relevan bagi kasir adalah penjamin yang tercatat SAAT kunjungan itu.
        var guarantorRows = encounterIds.Count == 0
            ? []
            : await _dbContext.RegPatientEncounterGuarantors.AsNoTracking()
                .Where(x => encounterIds.Contains(x.EncounterId) && x.IsActive && !x.IsDelete)
                .Select(x => new
                {
                    x.EncounterId,
                    x.PaymentType,
                    x.PaymentSourceNameSnapshot,
                    x.InsuranceProviderId
                })
                .ToListAsync(cancellationToken);
        var guarantorByEncounter = guarantorRows.GroupBy(x => x.EncounterId).ToDictionary(g => g.Key, g => g.First());

        var providerIds = guarantorByEncounter.Values
            .Where(x => x.InsuranceProviderId.HasValue).Select(x => x.InsuranceProviderId!.Value).Distinct().ToList();
        var claimMethodByProvider = providerIds.Count == 0
            ? new Dictionary<Guid, string?>()
            : await _dbContext.MstInsuranceProviders.AsNoTracking()
                .Where(x => providerIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => (string?)x.ClaimMethod, cancellationToken);

        // PatientAmount kalkulasi TERAKHIR per invoice - diambil dari seluruh baris lalu
        // dikelompokkan di memori (bukan GroupBy+OrderBy+FirstOrDefault dalam query) supaya tidak
        // bergantung pada dukungan translasi SQL provider untuk pola itu; volumenya terbatas
        // (jumlah versi kalkulasi per invoice pada satu halaman, bukan seluruh tabel).
        var calcRows = invoiceIds.Count == 0
            ? []
            : await _dbContext.BilCalculationVersions.AsNoTracking()
                .Where(x => invoiceIds.Contains(x.InvoiceId))
                .Select(x => new { x.InvoiceId, x.VersionNo, x.PatientAmount })
                .ToListAsync(cancellationToken);
        var patientAmountByInvoice = calcRows.GroupBy(x => x.InvoiceId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.VersionNo).First().PatientAmount);

        // Seluruh tender SUCCEEDED lintas SEMUA settlement InvoicePayment milik invoice-invoice
        // pada halaman ini - satu invoice bisa punya lebih dari satu settlement (mis. cicilan
        // kedua dibuka di wadah baru setelah wadah pertama SETTLED, lihat use-billing-settlement.js
        // komentar baris ~279).
        var settlementRows = invoiceIds.Count == 0
            ? []
            : await _dbContext.BilSettlements.AsNoTracking()
                .Where(s => s.InvoiceId != null && invoiceIds.Contains(s.InvoiceId!.Value)
                    && s.Purpose == BillingSettlementPurposes.InvoicePayment)
                .Select(s => new { s.Id, InvoiceId = s.InvoiceId!.Value })
                .ToListAsync(cancellationToken);
        var settlementToInvoice = settlementRows.ToDictionary(s => s.Id, s => s.InvoiceId);
        var settlementIds = settlementRows.Select(s => s.Id).ToList();

        var tenderRows = settlementIds.Count == 0
            ? []
            : await _dbContext.BilTenders.AsNoTracking()
                .Where(t => settlementIds.Contains(t.SettlementId) && t.Status == BillingTenderStatuses.Succeeded)
                .Select(t => new { t.Id, t.SettlementId, t.KwitansiNumber, t.Amount, t.AttemptedAt })
                .ToListAsync(cancellationToken);
        var tendersByInvoice = tenderRows
            .GroupBy(t => settlementToInvoice[t.SettlementId])
            .ToDictionary(g => g.Key, g => g.OrderBy(t => t.AttemptedAt).ToList());

        var items = page.Select(x =>
        {
            guarantorByEncounter.TryGetValue(x.EncounterId, out var guarantor);
            var tenders = tendersByInvoice.TryGetValue(x.Id, out var invoiceTenders)
                ? invoiceTenders
                : [];
            var totalPaid = tenders.Sum(t => t.Amount);
            var patientAmount = patientAmountByInvoice.TryGetValue(x.Id, out var pa) ? pa : 0m;
            string? claimMethod = guarantor?.InsuranceProviderId.HasValue == true
                && claimMethodByProvider.TryGetValue(guarantor.InsuranceProviderId.Value, out var cm)
                ? cm
                : null;

            return new PaymentHistoryItemResponse
            {
                Id = x.Id,
                InvoiceNumber = x.InvoiceNumber,
                VisitDate = x.VisitDate,
                MedicalRecordNumber = x.MedicalRecordNumber,
                PatientName = x.PatientName,
                ServiceType = x.ServiceType,
                PatientType = guarantor != null ? MapPaymentTypeLabel(guarantor.PaymentType) : "Tunai",
                GuarantorName = guarantor?.PaymentSourceNameSnapshot ?? "Tunai",
                ClaimMethod = claimMethod,
                TotalBillAmount = patientAmount,
                TotalPaidAmount = totalPaid,
                IsFullyPaid = patientAmount > 0 && totalPaid >= patientAmount,
                Tenders = tenders.Select(t => new PaymentHistoryTenderResponse
                {
                    Id = t.Id,
                    SettlementId = t.SettlementId,
                    KwitansiNumber = t.KwitansiNumber ?? string.Empty,
                    Amount = t.Amount,
                    AttemptedAt = t.AttemptedAt
                }).ToList()
            };
        }).ToList();

        return new PagedResult<PaymentHistoryItemResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    // Dedicated read model/query untuk halaman "Invoice & Billing Kasir" (Cashier Overview).
    // Dua-pass batch query (efisien, zero N+1) mengumpulkan data Pasien, Penjamin, Kalkulasi,
    // Riwayat Tender Succeeded, Deposit Rawat Inap, dan Reminder.
    public async Task<PagedResult<CashierBillingInvoiceListItemResponse>> GetCashierOverviewAsync(
        CashierBillingInvoiceQuery request, CancellationToken cancellationToken)
    {
        // 1. Evaluasi PeriodPreset jika ada
        if (!string.IsNullOrWhiteSpace(request.PeriodPreset))
        {
            var preset = request.PeriodPreset.Trim().ToUpperInvariant();
            var utcNow = DateTime.UtcNow;
            if (preset == CashierBillingPeriodPresets.All)
            {
                request.VisitDateFrom = null;
                request.VisitDateTo = null;
            }
            else if (preset == CashierBillingPeriodPresets.Today)
            {
                var todayStart = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, 0, 0, 0, DateTimeKind.Utc);
                request.VisitDateFrom = todayStart;
                request.VisitDateTo = todayStart.AddDays(1).AddTicks(-1);
            }
            else if (preset == CashierBillingPeriodPresets.Last7Days)
            {
                var end = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, 23, 59, 59, 999, DateTimeKind.Utc);
                request.VisitDateFrom = end.Date.AddDays(-7);
                request.VisitDateTo = end;
            }
            else if (preset == CashierBillingPeriodPresets.Last30Days)
            {
                var end = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, 23, 59, 59, 999, DateTimeKind.Utc);
                request.VisitDateFrom = end.Date.AddDays(-30);
                request.VisitDateTo = end;
            }
        }

        // 2. Validasi Tanggal
        if (request.VisitDateFrom.HasValue && request.VisitDateTo.HasValue
            && request.VisitDateFrom.Value > request.VisitDateTo.Value)
        {
            throw new BillingInvoiceValidationException("Tanggal Mulai tidak boleh lebih besar dari Tanggal Akhir.");
        }

        // 3. Base Query
        var query = _dbContext.BilInvoices.AsNoTracking().Where(x => !x.IsDelete);

        if (!string.IsNullOrWhiteSpace(request.ServiceType))
        {
            var serviceType = request.ServiceType.Trim().ToUpperInvariant();
            query = query.Where(x => x.ServiceType == serviceType);
        }

        var joined =
            from invoice in query
            join encounter in _dbContext.RegPatientEncounters.AsNoTracking()
                on invoice.EncounterId equals encounter.Id into encounterGroup
            from encounter in encounterGroup.DefaultIfEmpty()
            join patient in _dbContext.MstPatients.AsNoTracking()
                on encounter.PatientId equals patient.Id into patientGroup
            from patient in patientGroup.DefaultIfEmpty()
            select new { invoice, encounter, patient };

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            joined = joined.Where(x =>
                x.invoice.InvoiceNumber.ToUpper().Contains(search) ||
                (x.patient != null && x.patient.FullName.ToUpper().Contains(search)) ||
                (x.patient != null && x.patient.MedicalRecordNumber.ToUpper().Contains(search)) ||
                _dbContext.RegPatientEncounterGuarantors.Any(g =>
                    g.EncounterId == x.invoice.EncounterId &&
                    g.IsActive && !g.IsDelete &&
                    g.PaymentSourceNameSnapshot.ToUpper().Contains(search)));
        }

        if (request.VisitDateFrom.HasValue)
            joined = joined.Where(x => x.encounter != null && x.encounter.EncounterDate >= request.VisitDateFrom.Value);
        if (request.VisitDateTo.HasValue)
            joined = joined.Where(x => x.encounter != null && x.encounter.EncounterDate <= request.VisitDateTo.Value);

        var total = await joined.CountAsync(cancellationToken);
        var page = await joined
            .OrderByDescending(x => x.encounter != null ? x.encounter.EncounterDate : x.invoice.CreateDateTime)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new
            {
                x.invoice.Id,
                x.invoice.EncounterId,
                x.invoice.InvoiceNumber,
                x.invoice.ServiceType,
                x.invoice.Status,
                x.invoice.InvoiceDate,
                x.invoice.CreateDateTime,
                PatientId = x.patient != null ? x.patient.Id : Guid.Empty,
                PatientName = x.patient != null ? x.patient.FullName : string.Empty,
                MedicalRecordNumber = x.patient != null ? x.patient.MedicalRecordNumber : string.Empty,
                EncounterType = x.encounter != null ? x.encounter.EncounterType.ToString() : string.Empty,
                VisitDate = x.encounter != null ? (DateTime?)x.encounter.EncounterDate : null
            })
            .ToListAsync(cancellationToken);

        var invoiceIds = page.Select(x => x.Id).ToList();
        var encounterIds = page.Select(x => x.EncounterId).Distinct().ToList();

        // 4. Batch query Penjamin & Payer
        var guarantorRows = encounterIds.Count == 0
            ? []
            : await _dbContext.RegPatientEncounterGuarantors.AsNoTracking()
                .Where(x => encounterIds.Contains(x.EncounterId) && x.IsActive && !x.IsDelete)
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.Priority)
                .Select(x => new
                {
                    x.EncounterId,
                    x.PaymentType,
                    x.PaymentSourceNameSnapshot,
                    x.InsuranceProviderId,
                    x.CompanyGuarantorId
                })
                .ToListAsync(cancellationToken);
        var guarantorByEncounter = guarantorRows.GroupBy(x => x.EncounterId).ToDictionary(g => g.Key, g => g.First());

        var providerIds = guarantorByEncounter.Values
            .Where(x => x.InsuranceProviderId.HasValue).Select(x => x.InsuranceProviderId!.Value).Distinct().ToList();
        var claimMethodByProvider = providerIds.Count == 0
            ? new Dictionary<Guid, string?>()
            : await _dbContext.MstInsuranceProviders.AsNoTracking()
                .Where(x => providerIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => (string?)x.ClaimMethod, cancellationToken);

        // 5. Batch query Kalkulasi Terakhir (TotalBillAmount)
        var calcRows = invoiceIds.Count == 0
            ? []
            : await _dbContext.BilCalculationVersions.AsNoTracking()
                .Where(x => invoiceIds.Contains(x.InvoiceId))
                .Select(x => new { x.InvoiceId, x.VersionNo, x.PatientAmount })
                .ToListAsync(cancellationToken);
        var patientAmountByInvoice = calcRows.GroupBy(x => x.InvoiceId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.VersionNo).First().PatientAmount);

        // 6. Batch query Settlement & Tender SUCCEEDED
        var settlementRows = invoiceIds.Count == 0
            ? []
            : await _dbContext.BilSettlements.AsNoTracking()
                .Where(s => s.InvoiceId != null && invoiceIds.Contains(s.InvoiceId!.Value)
                    && s.Purpose == BillingSettlementPurposes.InvoicePayment)
                .Select(s => new { s.Id, InvoiceId = s.InvoiceId!.Value })
                .ToListAsync(cancellationToken);
        var settlementToInvoice = settlementRows.ToDictionary(s => s.Id, s => s.InvoiceId);
        var settlementIds = settlementRows.Select(s => s.Id).ToList();

        var tenderRows = settlementIds.Count == 0
            ? []
            : await _dbContext.BilTenders.AsNoTracking()
                .Where(t => settlementIds.Contains(t.SettlementId) && t.Status == BillingTenderStatuses.Succeeded)
                .Select(t => new { t.Id, t.SettlementId, t.Amount, t.AttemptedAt })
                .ToListAsync(cancellationToken);
        var tendersByInvoice = tenderRows
            .GroupBy(t => settlementToInvoice[t.SettlementId])
            .ToDictionary(g => g.Key, g => g.ToList());

        // 7. Batch query Deposit untuk RANAP
        var ranapEncounterIds = page
            .Where(x => IsRanap(x.ServiceType, x.EncounterType))
            .Select(x => x.EncounterId)
            .Distinct()
            .ToList();

        var depositAccounts = ranapEncounterIds.Count == 0
            ? []
            : await _dbContext.BilDepositAccounts.AsNoTracking()
                .Include(x => x.Movements)
                .Where(x => ranapEncounterIds.Contains(x.EncounterId) && !x.IsDelete)
                .ToListAsync(cancellationToken);
        var depositByEncounter = depositAccounts.ToDictionary(x => x.EncounterId, x => x);

        var episodes = ranapEncounterIds.Count == 0
            ? []
            : await _dbContext.Set<InpEpisode>().AsNoTracking()
                .Where(x => ranapEncounterIds.Contains(x.EncounterId) && !x.IsDelete)
                .ToListAsync(cancellationToken);
        var episodeByEncounter = episodes.ToDictionary(x => x.EncounterId, x => x);

        var nowOffset = DateTimeOffset.UtcNow;
        var activePolicies = await _dbContext.MstDepositPolicies.AsNoTracking()
            .Where(x => x.IsActive && !x.IsDelete && !x.IsCancel
                && x.EffectiveFrom <= nowOffset && (x.EffectiveTo == null || x.EffectiveTo > nowOffset))
            .ToListAsync(cancellationToken);

        // 8. Batch query Reminders (dengan safe fallback jika tabel belum dimigrasikan)
        var remindersByInvoice = new Dictionary<Guid, List<BilPaymentReminder>>();
        try
        {
            var reminderRows = await _dbContext.BilPaymentReminders.AsNoTracking()
                .Where(x => invoiceIds.Contains(x.InvoiceId) && !x.IsDelete)
                .ToListAsync(cancellationToken);
            remindersByInvoice = reminderRows.GroupBy(x => x.InvoiceId).ToDictionary(g => g.Key, g => g.ToList());
        }
        catch (Exception)
        {
            // Table BilPaymentReminder might not exist yet on remote DB
        }

        // 9. Susun Items Response
        var items = page.Select(x =>
        {
            guarantorByEncounter.TryGetValue(x.EncounterId, out var guarantor);
            var isRanap = IsRanap(x.ServiceType, x.EncounterType);

            string? claimMethod = guarantor?.InsuranceProviderId.HasValue == true
                && claimMethodByProvider.TryGetValue(guarantor.InsuranceProviderId.Value, out var cm)
                ? cm
                : null;

            var tenders = tendersByInvoice.TryGetValue(x.Id, out var invTenders)
                ? invTenders
                : [];
            var totalPaid = tenders.Sum(t => t.Amount);
            DateTimeOffset? lastPaymentAt = tenders.Count > 0 ? tenders.Max(t => (DateTimeOffset?)t.AttemptedAt) : null;
            var patientAmount = patientAmountByInvoice.TryGetValue(x.Id, out var pa) ? pa : 0m;
            var outstanding = Math.Max(0m, patientAmount - totalPaid);

            string paymentStatus;
            if (outstanding <= 0 && (patientAmount > 0 || totalPaid > 0))
                paymentStatus = CashierPaymentStatuses.Paid;
            else if (totalPaid > 0 && outstanding > 0)
                paymentStatus = CashierPaymentStatuses.Partial;
            else
                paymentStatus = CashierPaymentStatuses.Unpaid;

            // Deposit calculation
            bool hasDepositAccount = false;
            decimal depositAvailable = 0m;
            decimal depositReceived = 0m;
            decimal depositAllocated = 0m;
            decimal policyShortfall = 0m;
            decimal finalBillShortfall = 0m;
            decimal outstandingTopUp = 0m;
            string depositStatus = CashierDepositStatuses.NotApplicable;

            if (isRanap)
            {
                depositByEncounter.TryGetValue(x.EncounterId, out var account);
                episodeByEncounter.TryGetValue(x.EncounterId, out var ep);
                hasDepositAccount = account != null;

                if (account != null)
                {
                    var activeMovements = account.Movements.Where(m => !m.IsDelete).ToList();
                    var topUp = activeMovements.Where(m => m.MovementType == BillingDepositMovementTypes.TopUp).Sum(m => m.Amount);
                    var reversal = activeMovements.Where(m => m.MovementType == BillingDepositMovementTypes.Reversal).Sum(m => m.Amount);
                    depositReceived = Math.Max(0m, topUp - reversal);
                    depositAllocated = activeMovements.Where(m => m.MovementType == BillingDepositMovementTypes.Allocation).Sum(m => m.Amount);
                    depositAvailable = account.AvailableBalance;
                }

                var gId = guarantor?.InsuranceProviderId ?? guarantor?.CompanyGuarantorId;
                var classId = ep != null && ep.PatientClassId != Guid.Empty ? (Guid?)ep.PatientClassId : null;
                var policy = ResolveDepositPolicy(activePolicies, gId, classId);

                if (policy != null && policy.IsRequired && policy.MinimumAmount > 0)
                {
                    policyShortfall = Math.Max(0m, policy.MinimumAmount - depositReceived);
                }
                finalBillShortfall = Math.Max(0m, patientAmount - depositAllocated - depositAvailable);
                outstandingTopUp = policyShortfall;

                if (!hasDepositAccount || depositReceived == 0)
                    depositStatus = CashierDepositStatuses.NoDeposit;
                else if (depositAllocated > 0 && depositAvailable == 0)
                    depositStatus = CashierDepositStatuses.Exhausted;
                else if (depositAllocated > 0)
                    depositStatus = CashierDepositStatuses.Used;
                else if (depositReceived > 0 && depositAllocated == 0)
                    depositStatus = CashierDepositStatuses.AvailableNotUsed;
                else
                    depositStatus = CashierDepositStatuses.NoDeposit;
            }

            // Billing Age Days
            var startBillingDate = x.InvoiceDate?.Date ?? x.CreateDateTime.Date;
            var billingAgeDays = Math.Max(0, (DateTime.UtcNow.Date - startBillingDate).Days);

            // Reminders
            var reminders = remindersByInvoice.TryGetValue(x.Id, out var remList) ? remList : [];
            var reminderCount = reminders.Count;
            DateTime? lastReminderAt = reminderCount > 0 ? reminders.Max(r => r.SentAt) : null;
            string reminderStatus;
            if (paymentStatus == CashierPaymentStatuses.Paid)
                reminderStatus = CashierReminderStatuses.NotApplicable;
            else if (reminderCount > 0)
                reminderStatus = CashierReminderStatuses.Sent;
            else
                reminderStatus = CashierReminderStatuses.NeverSent;

            bool canSendReminder = outstanding > 0;
            bool hasInsurance = guarantor != null && (guarantor.PaymentType == EncounterPaymentType.Insurance || guarantor.InsuranceProviderId.HasValue);

            return new CashierBillingInvoiceListItemResponse
            {
                InvoiceId = x.Id,
                EncounterId = x.EncounterId,
                InvoiceNumber = x.InvoiceNumber,
                PatientId = x.PatientId,
                MedicalRecordNumber = x.MedicalRecordNumber,
                PatientName = x.PatientName,
                EncounterType = x.EncounterType,
                ServiceType = x.ServiceType,
                PatientType = guarantor != null ? MapPaymentTypeLabel(guarantor.PaymentType) : "Tunai",
                GuarantorName = guarantor?.PaymentSourceNameSnapshot ?? "Tunai",
                ClaimMethod = claimMethod,
                HasInsurancePayer = hasInsurance,
                VisitDate = x.VisitDate,
                TotalBillAmount = patientAmount,
                LastSuccessfulPaymentAt = lastPaymentAt,
                TotalPaidAmount = totalPaid,
                OutstandingAmount = outstanding,
                PaymentStatus = paymentStatus,
                HasDepositAccount = hasDepositAccount,
                DepositAvailableBalance = depositAvailable,
                DepositTotalReceived = depositReceived,
                DepositTotalAllocated = depositAllocated,
                DepositPolicyShortfallAmount = policyShortfall,
                DepositFinalBillShortfallAmount = finalBillShortfall,
                DepositOutstandingTopUp = outstandingTopUp,
                DepositStatus = depositStatus,
                BillingAgeDays = billingAgeDays,
                ReminderCount = reminderCount,
                LastReminderAt = lastReminderAt,
                ReminderStatus = reminderStatus,
                CanSendReminder = canSendReminder
            };
        }).ToList();

        return new PagedResult<CashierBillingInvoiceListItemResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    private static bool IsRanap(string serviceType, string encounterType)
    {
        return string.Equals(serviceType, "RANAP", StringComparison.OrdinalIgnoreCase)
            || string.Equals(encounterType, "Inpatient", StringComparison.OrdinalIgnoreCase)
            || string.Equals(encounterType, "RANAP", StringComparison.OrdinalIgnoreCase);
    }

    private static MstDepositPolicy? ResolveDepositPolicy(
        List<MstDepositPolicy> activePolicies, Guid? guarantorId, Guid? patientClassId)
    {
        return (guarantorId.HasValue && patientClassId.HasValue
                ? activePolicies.FirstOrDefault(x => x.GuarantorId == guarantorId.Value && x.PatientClassId == patientClassId.Value)
                : null)
            ?? (guarantorId.HasValue
                ? activePolicies.FirstOrDefault(x => x.GuarantorId == guarantorId.Value && x.PatientClassId == null)
                : null)
            ?? (patientClassId.HasValue
                ? activePolicies.FirstOrDefault(x => x.GuarantorId == null && x.PatientClassId == patientClassId.Value)
                : null)
            ?? activePolicies.FirstOrDefault(x => x.GuarantorId == null && x.PatientClassId == null);
    }

    public async Task<InvoiceDetailResponse> GetDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.BilInvoices.AsNoTracking()
            .Include(x => x.Items).ThenInclude(x => x.Category)
            .Include(x => x.Items).ThenInclude(x => x.Tariff).ThenInclude(x => x!.Drug).ThenInclude(x => x!.DispenseUnitMeasurement)
            .Include(x => x.DiscountApplications).ThenInclude(x => x.DiscountPolicy)
            .Include(x => x.CalculationVersions)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Invoice Billing tidak ditemukan.");
        var response = MapDetail(invoice, false);
        response.Patient = await LoadPatientSummaryAsync(invoice.EncounterId, cancellationToken);
        return response;
    }

    public async Task<EncounterChargeSummaryResponse> GetChargeSummaryByEncounterAsync(
        Guid encounterId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (encounterId == Guid.Empty)
            throw new BillingInvoiceValidationException("EncounterId wajib diisi.");

        var invoices = await _dbContext.BilInvoices.AsNoTracking()
            .Where(x => x.EncounterId == encounterId && !x.IsDelete)
            .OrderByDescending(x => x.CreateDateTime)
            .Select(x => new
            {
                x.Id,
                x.InvoiceNumber,
                x.ServiceType,
                x.Status,
                x.CurrentCalculationVersion
            })
            .ToListAsync(cancellationToken);

        if (invoices.Count == 0)
            throw new KeyNotFoundException("Belum ada invoice untuk kunjungan ini.");

        var invoice = invoices[0];

        // Sumber angkanya sama dengan yang dipakai Menu Pembayaran, sehingga rekap per kategori
        // dan total di layar tidak mungkin berbeda.
        var calculation = await _calculationService.PreviewCalculationAsync(
            invoice.Id, actorUserId, cancellationToken);
        var breakdown = calculation.Breakdown;

        var grouped = breakdown.Items
            .GroupBy(x => new { x.CategoryId, x.CategoryCode })
            .Select(group => new ChargeCategorySummaryResponse
            {
                CategoryId = group.Key.CategoryId,
                CategoryCode = group.Key.CategoryCode,
                Kind = ChargeSummaryKinds.Item,
                ItemCount = group.Count(),
                GrossAmount = group.Sum(x => x.GrossAmount),
                DiscountAmount = group.Sum(x => x.ItemDiscount),
                TaxAmount = group.Sum(x => x.TaxAmount),
                NetAmount = group.Sum(x => x.NetAmount)
            })
            .ToList();

        // Nama kategori tidak dibawa CalculationItemResponse (hanya kode), sedangkan layar
        // membutuhkannya - jadi diambil sekali untuk seluruh kategori yang muncul.
        var categoryIds = grouped.Where(x => x.CategoryId.HasValue)
            .Select(x => x.CategoryId!.Value).Distinct().ToList();
        var categoryNames = categoryIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _dbContext.Set<MstTariffCategory>().AsNoTracking()
                .Where(x => categoryIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.TariffCategoryName, cancellationToken);

        foreach (var row in grouped)
        {
            row.CategoryName = row.CategoryId.HasValue
                && categoryNames.TryGetValue(row.CategoryId.Value, out var name)
                    ? name
                    : row.CategoryCode;
        }

        var categories = grouped
            .OrderBy(x => x.CategoryName)
            .ToList();

        var administrationFee = breakdown.AdministrationFee.AppliedAmount;
        if (administrationFee != 0)
        {
            categories.Add(new ChargeCategorySummaryResponse
            {
                CategoryCode = "ADMINISTRATION_FEE",
                CategoryName = "Biaya Administrasi",
                Kind = ChargeSummaryKinds.AdministrationFee,
                ItemCount = 1,
                GrossAmount = administrationFee,
                NetAmount = administrationFee
            });
        }

        var roomCharge = breakdown.RoomCharge.AppliedAmount;
        if (roomCharge != 0)
        {
            categories.Add(new ChargeCategorySummaryResponse
            {
                CategoryCode = "ROOM_CHARGE",
                CategoryName = "Biaya Kamar",
                Kind = ChargeSummaryKinds.RoomCharge,
                ItemCount = breakdown.RoomCharge.Segments.Count,
                GrossAmount = roomCharge,
                NetAmount = roomCharge
            });
        }

        return new EncounterChargeSummaryResponse
        {
            EncounterId = encounterId,
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            ServiceType = invoice.ServiceType,
            Status = invoice.Status,
            CurrentCalculationVersion = invoice.CurrentCalculationVersion,
            InvoiceCount = invoices.Count,
            Categories = categories,
            Totals = new ChargeSummaryTotalResponse
            {
                GrossAmount = calculation.GrossAmount,
                AdministrationFeeAmount = calculation.AdministrationFeeAmount,
                RoomChargeAmount = calculation.RoomChargeAmount,
                ItemDiscount = calculation.ItemDiscount,
                PromoDiscount = calculation.TotalDiscount - calculation.ItemDiscount,
                TotalDiscount = calculation.TotalDiscount,
                TaxAmount = calculation.TaxAmount,
                PatientAmount = calculation.PatientAmount,
                PrimaryAmount = calculation.PrimaryAmount,
                ExcessAmount = calculation.ExcessAmount,
                UnresolvedCoverageAmount = calculation.UnresolvedCoverageAmount
            }
        };
    }

    // BKC-DEC-059: entri charge dari katalog tarif. Harga (NormalPrice), kategori
    // (TariffCategoryId), dan deskripsi (TariffName) seluruhnya diambil dari MstTariff milik
    // server - client cuma mengirim TariffId dan kuantitas, tidak bisa mendikte harga seperti pada
    // AddOtherChargeAsync (yang memang untuk biaya bebas kasir). SourceDomain "ADHOC_CATALOG"
    // (lihat BillingChargeSourceAdapter) supaya asal-usulnya bisa dibedakan dari entri bebas ADHOC
    // pada audit dan laporan, walau siklus hidupnya identik (completeOnEntry).
    public async Task<InvoiceDetailResponse> AddCatalogChargeAsync(
        AddCatalogChargeRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (idempotencyKey == Guid.Empty)
            throw new BillingInvoiceValidationException("Idempotency-Key wajib diisi.");

        // Cek Idempotency Replay terlebih dahulu agar replay tidak menambah QTY berulang
        var priorReceipt = await _dbContext.BilChargeReceipts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
        if (priorReceipt is not null)
        {
            var replayInvoice = await LoadInvoiceByItemAsync(priorReceipt.InvoiceItemId, cancellationToken);
            if (replayInvoice.EncounterId != request.EncounterId)
                throw new BillingInvoiceConflictException("Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");
            return MapDetail(replayInvoice, true);
        }

        var now = DateTime.UtcNow;
        var tariff = await _dbContext.MstTariffs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TariffId && !x.IsDelete && !x.IsCancel && x.IsActive
                && (x.EffectiveStartDate == null || x.EffectiveStartDate <= now)
                && (x.EffectiveEndDate == null || now < x.EffectiveEndDate), cancellationToken)
            ?? throw new BillingInvoiceValidationException(
                "Tarif tidak ditemukan, tidak aktif, atau sudah kedaluwarsa.");

        // Cek apakah sudah ada item aktif dengan nama yang sama (atau TariffId sama) pada invoice encounter ini
        var existingInvoice = await _dbContext.BilInvoices.AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.EncounterId == request.EncounterId && !x.IsDelete, cancellationToken);

        var tariffName = tariff.TariffName.Trim();
        var existingItem = existingInvoice?.Items.FirstOrDefault(x =>
            !x.IsDelete
            && x.Status == BillingInvoiceItemStatuses.Active
            && (string.Equals(x.DescriptionSnapshot.Trim(), tariffName, StringComparison.OrdinalIgnoreCase)
                || (x.TariffId.HasValue && x.TariffId == tariff.Id)));

        if (existingItem is not null)
        {
            var newQuantity = existingItem.Quantity + request.Quantity;

            return await UpsertChargeAsync(
                new UpsertChargeRequest
                {
                    EncounterId = request.EncounterId,
                    SourceDomain = existingItem.SourceDomain,
                    SourceDetailId = existingItem.SourceDetailId,
                    SourceVersion = existingItem.SourceVersion + 1,
                    SourceStatus = existingItem.SourceStatus,
                    OccurredAt = DateTimeOffset.UtcNow,
                    CategoryId = existingItem.CategoryId,
                    TariffId = existingItem.TariffId ?? tariff.Id,
                    DescriptionSnapshot = existingItem.DescriptionSnapshot,
                    Quantity = newQuantity,
                    UnitPrice = existingItem.UnitPrice,
                    DoctorShare = existingItem.DoctorShare,
                    ContractVersion = ContractBillingChargeSourceAdapter.ContractVersion,
                    CorrelationId = request.CorrelationId,
                    CausationId = request.CausationId
                },
                idempotencyKey,
                actorUserId,
                cancellationToken);
        }

        return await UpsertChargeAsync(
            new UpsertChargeRequest
            {
                EncounterId = request.EncounterId,
                SourceDomain = "ADHOC_CATALOG",
                // SourceDetailId diturunkan dari idempotencyKey (bukan Guid acak baru per panggilan)
                // supaya retry dengan Idempotency-Key yang sama benar-benar ter-replay lewat
                // BilChargeReceipt (UpsertChargeAsync mencocokkan SourceDetailId dengan receipt
                // tersimpan) - acak baru tiap panggilan akan selalu mismatch dan retry genuine akan
                // salah dianggap konflik.
                SourceDetailId = idempotencyKey.ToString("N"),
                SourceVersion = 1,
                SourceStatus = "ADDED",
                OccurredAt = DateTimeOffset.UtcNow,
                CategoryId = tariff.TariffCategoryId,
                TariffId = tariff.Id,
                DescriptionSnapshot = tariff.TariffName,
                Quantity = request.Quantity,
                UnitPrice = tariff.NormalPrice,
                DoctorShare = 0,
                ContractVersion = ContractBillingChargeSourceAdapter.ContractVersion,
                CorrelationId = request.CorrelationId,
                CausationId = request.CausationId
            },
            idempotencyKey,
            actorUserId,
            cancellationToken);
    }

    public static List<OtherChargeTypeOptionResponse> GetOtherChargeTypeOptions() =>
        BillingOtherChargeTypes.Labels
            .Select(x => new OtherChargeTypeOptionResponse { Value = x.Key, Label = x.Value })
            .ToList();

    // Menambah biaya lain-lain tanpa client perlu tahu Id kategori billing. Kategorinya ditetapkan
    // di sini - "Biaya Lain-Lain" - sehingga seluruh entri manual kasir konsisten mendarat di satu
    // kategori, dan yang dipilih kasir cukup jenis biayanya.
    public async Task<InvoiceDetailResponse> AddOtherChargeAsync(
        AddOtherChargeRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var chargeType = (request.ChargeType ?? string.Empty).Trim().ToUpperInvariant();
        if (!BillingOtherChargeTypes.Labels.TryGetValue(chargeType, out var chargeTypeLabel))
            throw new BillingInvoiceValidationException(
                "Jenis biaya lain-lain tidak dikenal.");

        var category = await _dbContext.Set<MstTariffCategory>().AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive)
            .FirstOrDefaultAsync(
                x => x.TariffCategoryCode == BillingOtherChargeTypes.CategoryCode
                    || x.TariffCategoryName == BillingOtherChargeTypes.CategoryName,
                cancellationToken)
            ?? throw new BillingInvoiceValidationException(
                $"Kategori tarif \"{BillingOtherChargeTypes.CategoryName}\" belum ada di master data. " +
                $"Buat kategori tarif dengan kode {BillingOtherChargeTypes.CategoryCode} lebih dulu.");

        // Jenis biaya ikut ditulis di deskripsi: kategori billing-nya sama untuk semua entri, jadi
        // tanpa ini jenisnya hilang dari tagihan dan audit.
        var description = $"{chargeTypeLabel} - {request.Description.Trim()}";
        if (description.Length > 250) description = description[..250];

        return await UpsertChargeAsync(
            new UpsertChargeRequest
            {
                EncounterId = request.EncounterId,
                SourceDomain = "ADHOC",
                SourceDetailId = Guid.NewGuid().ToString("N"),
                SourceVersion = 1,
                SourceStatus = "ADDED",
                OccurredAt = DateTimeOffset.UtcNow,
                CategoryId = category.Id,
                DescriptionSnapshot = description,
                Quantity = request.Quantity,
                UnitPrice = request.UnitPrice,
                DoctorShare = 0,
                ContractVersion = "BIL-INTEGRATION-0.4",
                CorrelationId = request.CorrelationId,
                CausationId = request.CausationId
            },
            idempotencyKey,
            actorUserId,
            cancellationToken);
    }

    // Kunjungan yang masih bisa ditagih. Draft dikecualikan karena pendaftarannya belum rampung,
    // Cancelled dan NoShow karena pelayanannya tidak pernah terjadi.
    //
    // Status inilah penentunya, bukan kolom IsActive. IsActive hanya dimatikan oleh pembatalan
    // lewat endpoint registrasi dan soft delete; penyelesaian kunjungan, Draft, dan NoShow tidak
    // menyentuhnya, dan pembatalan dari rawat inap (InpEpisodeService) melewatkannya sama sekali.
    // IsActive tetap ikut disyaratkan pada query sebagai penjaga tambahan.
    //
    // Completed SENGAJA disertakan: penagihan justru umum terjadi setelah pelayanan selesai, dan
    // kalau status itu dibuang, kunjungan yang paling sering perlu dibuatkan invoice malah tidak
    // muncul di daftar.
    private static readonly EncounterStatus[] BillableEncounterStatuses =
    [
        EncounterStatus.Registered,
        EncounterStatus.Queued,
        EncounterStatus.WaitingForNurse,
        EncounterStatus.InNurseScreening,
        EncounterStatus.WaitingForDoctor,
        EncounterStatus.InConsultation,
        EncounterStatus.ConsultationCompleted,
        EncounterStatus.Billing,
        EncounterStatus.Completed
    ];

    public async Task<List<ActiveEncounterOptionResponse>> GetActiveEncounterOptionsAsync(
        string? search,
        int limit,
        CancellationToken cancellationToken)
    {
        var safeLimit = Math.Clamp(limit, 1, 100);

        var query =
            from encounter in _dbContext.RegPatientEncounters.AsNoTracking()
            join patient in _dbContext.MstPatients.AsNoTracking()
                on encounter.PatientId equals patient.Id
            where !encounter.IsDelete
                && !encounter.IsCancel
                && encounter.IsActive
                && BillableEncounterStatuses.Contains(encounter.EncounterStatus)
            select new { encounter, patient };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToUpper();
            query = query.Where(x =>
                x.patient.FullName.ToUpper().Contains(keyword) ||
                x.patient.MedicalRecordNumber.ToUpper().Contains(keyword) ||
                x.encounter.EncounterNumber.ToUpper().Contains(keyword));
        }

        var rows = await query
            .OrderByDescending(x => x.encounter.EncounterDate)
            .Take(safeLimit)
            .Select(x => new
            {
                x.encounter.Id,
                x.encounter.EncounterNumber,
                x.encounter.EncounterType,
                x.encounter.EncounterStatus,
                x.encounter.EncounterDate,
                x.encounter.PaymentType,
                x.encounter.ServiceUnitId,
                x.encounter.ClinicId,
                x.encounter.PatientClassId,
                PatientName = x.patient.FullName,
                x.patient.MedicalRecordNumber
            })
            .ToListAsync(cancellationToken);

        var encounterIds = rows.Select(x => x.Id).ToList();
        var invoicedEncounterIds = encounterIds.Count == 0
            ? []
            : await _dbContext.BilInvoices.AsNoTracking()
                .Where(x => encounterIds.Contains(x.EncounterId) && !x.IsDelete)
                .Select(x => x.EncounterId)
                .Distinct()
                .ToListAsync(cancellationToken);

        // Nama penjamin diambil dari snapshot pada encounter, bukan master penjamin: yang relevan
        // bagi kasir adalah penjamin yang tercatat saat kunjungan itu didaftarkan.
        var guarantorNames = encounterIds.Count == 0
            ? new Dictionary<Guid, string?>()
            : await _dbContext.RegPatientEncounterGuarantors.AsNoTracking()
                .Where(x => encounterIds.Contains(x.EncounterId) && x.IsActive && !x.IsDelete)
                .GroupBy(x => x.EncounterId)
                .Select(group => new
                {
                    EncounterId = group.Key,
                    Name = group.Select(x => x.PaymentSourceNameSnapshot).FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.EncounterId, x => x.Name, cancellationToken);

        return rows.Select(x => new ActiveEncounterOptionResponse
        {
            Id = x.Id,
            EncounterNumber = x.EncounterNumber,
            PatientName = x.PatientName,
            MedicalRecordNumber = x.MedicalRecordNumber,
            EncounterType = x.EncounterType.ToString(),
            EncounterStatus = x.EncounterStatus.ToString(),
            EncounterDate = x.EncounterDate,
            HasInvoice = invoicedEncounterIds.Contains(x.Id),
            ServiceType = MapServiceType(x.EncounterType),
            PaymentType = x.PaymentType.ToString(),
            PaymentTypeLabel = MapPaymentTypeLabel(x.PaymentType),
            GuarantorName = guarantorNames.TryGetValue(x.Id, out var guarantorName)
                ? guarantorName
                : null,
            ServiceUnitId = x.ServiceUnitId,
            ClinicId = x.ClinicId,
            PatientClassId = x.PatientClassId
        }).ToList();
    }

    // Ringkasan konteks pasien/kunjungan untuk layar Menu Pembayaran (kasir). InvoiceDetailResponse
    // sendiri tidak menyimpan data ini (BilInvoice hanya punya EncounterId) - lihat catatan pada
    // InvoiceDetailResponse.Patient. Dokter DPJP sengaja tidak disertakan di sini karena butuh join
    // ke MstDoctor pada area Corporate/HumanResource; bisa ditambahkan pada task terpisah.
    private async Task<InvoicePatientSummaryResponse?> LoadPatientSummaryAsync(
        Guid encounterId, CancellationToken cancellationToken)
    {
        var row = await (
            from encounter in _dbContext.RegPatientEncounters.AsNoTracking()
            join patient in _dbContext.MstPatients.AsNoTracking()
                on encounter.PatientId equals patient.Id
            where encounter.Id == encounterId && !encounter.IsDelete
            select new { encounter, patient })
            .FirstOrDefaultAsync(cancellationToken);
        if (row is null) return null;

        var roomName = row.encounter.RoomId.HasValue
            ? await _dbContext.MstRooms.AsNoTracking()
                .Where(x => x.Id == row.encounter.RoomId.Value)
                .Select(x => (string?)x.RoomName)
                .FirstOrDefaultAsync(cancellationToken)
            : null;
        var serviceUnitName = await _dbContext.MstServiceUnits.AsNoTracking()
            .Where(x => x.Id == row.encounter.ServiceUnitId)
            .Select(x => (string?)x.ServiceUnitName)
            .FirstOrDefaultAsync(cancellationToken);
        var patientClassName = row.encounter.PatientClassId.HasValue
            ? await _dbContext.MstPatientClasses.AsNoTracking()
                .Where(x => x.Id == row.encounter.PatientClassId.Value)
                .Select(x => (string?)x.PatientClassName)
                .FirstOrDefaultAsync(cancellationToken)
            : null;
        var guarantorName = await _dbContext.RegPatientEncounterGuarantors.AsNoTracking()
            .Where(x => x.EncounterId == encounterId && x.IsActive)
            .Select(x => x.PaymentSourceNameSnapshot)
            .FirstOrDefaultAsync(cancellationToken);

        string? doctorName = null;
        if (row.encounter.DoctorId.HasValue)
        {
            doctorName = await _dbContext.MstDoctors.AsNoTracking()
                .Where(d => d.Id == row.encounter.DoctorId.Value && !d.IsDelete)
                .Select(d => d.FullName)
                .FirstOrDefaultAsync(cancellationToken);
        }

        string? bedName = null;
        string? bedNumber = null;
        DateTime? admissionDateTime = null;

        var episode = await _dbContext.Set<InpEpisode>().AsNoTracking()
            .Where(e => e.EncounterId == encounterId && !e.IsDelete)
            .OrderByDescending(e => e.CreateDateTime)
            .FirstOrDefaultAsync(cancellationToken);

        if (episode != null)
        {
            admissionDateTime = episode.AdmittedAt;

            var placement = await _dbContext.InpBedPlacements.AsNoTracking()
                .Where(p => p.EpisodeId == episode.Id && !p.IsDelete && p.IsActive)
                .OrderByDescending(p => p.StartDateTime)
                .Include(p => p.Bed)
                .FirstOrDefaultAsync(cancellationToken);

            if (placement?.Bed != null)
            {
                bedName = placement.Bed.BedName;
                bedNumber = placement.Bed.BedNumber;
            }
        }

        return new InvoicePatientSummaryResponse
        {
            PatientId = row.patient.Id,
            MedicalRecordNumber = row.patient.MedicalRecordNumber,
            FullName = row.patient.FullName,
            Gender = row.patient.Gender?.ToString(),
            AgeText = row.encounter.AgeTextAtEncounter,
            EncounterNumber = row.encounter.EncounterNumber,
            EncounterDate = row.encounter.EncounterDate,
            EncounterType = row.encounter.EncounterType.ToString(),
            PaymentType = row.encounter.PaymentType.ToString(),
            PaymentTypeLabel = MapPaymentTypeLabel(row.encounter.PaymentType),
            RoomName = roomName,
            ServiceUnitName = serviceUnitName,
            PatientClassName = patientClassName,
            GuarantorName = guarantorName,
            DoctorInChargeName = doctorName,
            BedName = bedName,
            BedNumber = bedNumber,
            AdmissionDateTime = admissionDateTime ?? row.encounter.CheckedInAt ?? row.encounter.EncounterDate
        };
    }

    public async Task<InvoiceDetailResponse> UpsertChargeAsync(
        UpsertChargeRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateRequest(request, idempotencyKey);
        var source = _sourceAdapter.ValidateAndNormalize(request);
        var payloadHash = ComputePayloadHash(request, source);
        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_SOURCE_{source.SourceDomain}_{source.SourceDetailId}", cancellationToken);
                await AcquireLockAsync($"BIL_ENCOUNTER_{request.EncounterId:N}", cancellationToken);
            }

            var priorReceipt = await _dbContext.BilChargeReceipts.AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
            if (priorReceipt is not null)
            {
                if (priorReceipt.PayloadHash != payloadHash
                    || priorReceipt.SourceDomain != source.SourceDomain
                    || priorReceipt.SourceDetailId != source.SourceDetailId)
                    throw new BillingInvoiceConflictException("Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");
                var replayInvoice = await LoadInvoiceByItemAsync(priorReceipt.InvoiceItemId, cancellationToken);
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                return MapDetail(replayInvoice, true);
            }

            var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.EncounterId && !x.IsDelete && !x.IsCancel, cancellationToken)
                ?? throw new KeyNotFoundException("Encounter tidak ditemukan.");
            var categoryExists = await _dbContext.MstTariffCategories.AsNoTracking()
                .AnyAsync(x => x.Id == request.CategoryId && !x.IsDelete && !x.IsCancel && x.IsActive, cancellationToken);
            if (!categoryExists) throw new BillingInvoiceValidationException("Kategori tarif tidak ditemukan atau tidak aktif.");

            // Category ikut dimuat karena MapDetail memakai namanya untuk pengelompokan tagihan.
            var invoice = await _dbContext.BilInvoices.Include(x => x.Items).ThenInclude(x => x.Category)
                .Include(x => x.Items).ThenInclude(x => x.Tariff).ThenInclude(x => x!.Drug).ThenInclude(x => x!.DispenseUnitMeasurement)
                .Include(x => x.DiscountApplications).ThenInclude(x => x.DiscountPolicy)
                .FirstOrDefaultAsync(x => x.EncounterId == request.EncounterId && !x.IsDelete, cancellationToken);
            var createdInvoice = invoice is null;
            if (invoice is null)
            {
                invoice = new BilInvoice
                {
                    EncounterId = request.EncounterId,
                    InvoiceNumber = await _numberSeries.AllocateInvoiceNumberAsync(actorUserId, DateTimeOffset.UtcNow, cancellationToken),
                    ServiceType = MapServiceType(encounter.EncounterType),
                    Status = BillingInvoiceStatuses.Open,
                    CurrentCalculationVersion = 0,
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                _dbContext.BilInvoices.Add(invoice);
            }
            if (invoice.Status != BillingInvoiceStatuses.Open)
                throw new BillingInvoiceValidationException("Invoice final tidak dapat diedit; ajukan adjustment.");

            var existingItem = await _dbContext.BilInvoiceItems.FirstOrDefaultAsync(
                x => x.SourceDomain == source.SourceDomain && x.SourceDetailId == source.SourceDetailId
                    && x.Status != BillingInvoiceItemStatuses.Voided && !x.IsDelete,
                cancellationToken);
            var isReplay = false;
            BilInvoiceItem item;
            if (existingItem is not null)
            {
                if (existingItem.InvoiceId != invoice.Id)
                    throw new BillingInvoiceConflictException("Item pelayanan ini sudah tercatat pada invoice lain.");
                if (request.SourceVersion < existingItem.SourceVersion)
                    throw new BillingInvoiceConflictException("Versi source lebih lama dari data Billing saat ini.");
                if (request.SourceVersion == existingItem.SourceVersion)
                {
                    if (existingItem.SourcePayloadHash != payloadHash)
                        throw new BillingInvoiceConflictException("Source version yang sama memiliki isi berbeda.");
                    isReplay = true;
                }
                else
                {
                    ApplySource(existingItem, request, source, payloadHash, idempotencyKey, actorUserId);
                    invoice.RowVersion = Guid.NewGuid();
                    invoice.UpdateDateTime = DateTime.UtcNow;
                    invoice.UpdateBy = actorUserId;
                }
                item = existingItem;
            }
            else
            {
                item = new BilInvoiceItem
                {
                    InvoiceId = invoice.Id,
                    Invoice = invoice,
                    Status = BillingInvoiceItemStatuses.Active,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                ApplySource(item, request, source, payloadHash, idempotencyKey, actorUserId, false);
                _dbContext.BilInvoiceItems.Add(item);
                invoice.RowVersion = Guid.NewGuid();
            }

            _dbContext.BilChargeReceipts.Add(new BilChargeReceipt
            {
                IdempotencyKey = idempotencyKey,
                InvoiceItemId = item.Id,
                SourceDomain = source.SourceDomain,
                SourceDetailId = source.SourceDetailId,
                PayloadHash = payloadHash,
                CorrelationId = request.CorrelationId,
                ReceivedAt = DateTimeOffset.UtcNow,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditAsync(createdInvoice ? "BillingInvoice.CreateCharge" : "BillingInvoice.UpsertCharge",
                invoice, item, request.CorrelationId, actorUserId, isReplay);
            return MapDetail(invoice, isReplay);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingInvoiceConflictException("Charge tidak dapat disimpan karena invoice, source, atau idempotency key sudah diproses.", exception);
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    public async Task<InvoiceDetailResponse> VoidItemAsync(
        Guid invoiceId,
        Guid itemId,
        VoidInvoiceItemRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateVoidRequest(invoiceId, itemId, request, idempotencyKey);
        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_ITEM_{itemId:N}", cancellationToken);
            }

            var invoice = await _dbContext.BilInvoices
                .Include(x => x.Items).ThenInclude(x => x.Category)
                .Include(x => x.Items).ThenInclude(x => x.Tariff).ThenInclude(x => x!.Drug).ThenInclude(x => x!.DispenseUnitMeasurement)
                .Include(x => x.DiscountApplications).ThenInclude(x => x.DiscountPolicy)
                .Include(x => x.CalculationVersions)
                .FirstOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Invoice Billing tidak ditemukan.");
            var item = invoice.Items.FirstOrDefault(x => x.Id == itemId && !x.IsDelete)
                ?? throw new KeyNotFoundException("Item invoice Billing tidak ditemukan.");

            if (_dbContext.Database.IsRelational())
                await AcquireLockAsync($"BIL_SOURCE_{item.SourceDomain}_{item.SourceDetailId}", cancellationToken);

            var voidPayloadHash = ComputeVoidPayloadHash(invoiceId, item, request);
            if (item.Status == BillingInvoiceItemStatuses.Voided)
            {
                if (item.SourcePayloadHash != voidPayloadHash)
                    throw new BillingInvoiceConflictException(
                        "Item sudah dibatalkan oleh permintaan yang berbeda.");
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                return MapDetail(invoice, true);
            }

            if (invoice.Status != BillingInvoiceStatuses.Open)
                throw new BillingInvoiceValidationException(
                    "Invoice final tidak dapat diedit; ajukan adjustment.");
            if (invoice.RowVersion != request.ExpectedRowVersion)
                throw new BillingInvoiceConflictException(
                    "Data telah berubah. Muat ulang sebelum melanjutkan.");
            if (invoice.CalculationVersions.Any(x => !x.IsDelete && x.IsLocked))
                throw new BillingInvoiceValidationException(
                    "Item tidak dapat dibatalkan karena pelayanan atau pembayaran sudah diproses.");

            var voidSource = _sourceAdapter.ValidateVoid(item, request);
            var previousSourceVersion = item.SourceVersion;
            var previousSourceStatus = item.SourceStatus;
            var previousCalculationVersion = invoice.CurrentCalculationVersion;
            var beforeGross = invoice.Items
                .Where(x => !x.IsDelete && x.Status != BillingInvoiceItemStatuses.Voided)
                .Sum(x => x.Quantity * x.UnitPrice);

            item.Status = BillingInvoiceItemStatuses.Voided;
            item.VoidReason = request.Reason.Trim();
            item.SourceVersion = voidSource.SourceVersion;
            item.SourceStatus = voidSource.SourceStatus;
            item.SourceContractVersion = voidSource.ContractVersion;
            item.LastIdempotencyKey = idempotencyKey;
            item.LastCorrelationId = request.CorrelationId;
            item.LastCausationId = request.CausationId;
            item.SourcePayloadHash = voidPayloadHash;
            item.UpdateDateTime = DateTime.UtcNow;
            item.UpdateBy = actorUserId;

            // Token sementara ini memastikan calculation memakai mutation yang sama. SaveChanges
            // dilakukan sekali oleh calculation service di dalam transaction yang sama.
            invoice.RowVersion = Guid.NewGuid();
            invoice.UpdateDateTime = DateTime.UtcNow;
            invoice.UpdateBy = actorUserId;
            try
            {
                await _calculationService.RecalculateAsync(
                    invoice.Id,
                    new RecalculateInvoiceRequest
                    {
                        ExpectedRowVersion = invoice.RowVersion,
                        Reason = $"Void item: {request.Reason.Trim()}"
                    },
                    actorUserId,
                    cancellationToken);
            }
            catch (BillingCalculationConflictException exception)
            {
                throw new BillingInvoiceConflictException(exception.Message, exception);
            }
            catch (BillingCalculationValidationException exception)
            {
                throw new BillingInvoiceValidationException(exception.Message);
            }

            if (transaction is not null) await transaction.CommitAsync(cancellationToken);

            var afterGross = invoice.Items
                .Where(x => !x.IsDelete && x.Status != BillingInvoiceItemStatuses.Voided)
                .Sum(x => x.Quantity * x.UnitPrice);
            await AuditVoidAsync(
                invoice,
                item,
                previousSourceVersion,
                previousSourceStatus,
                previousCalculationVersion,
                beforeGross,
                afterGross,
                request.CorrelationId,
                actorUserId);
            return MapDetail(invoice, false);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingInvoiceConflictException(
                "Item tidak dapat dibatalkan karena invoice telah berubah.", exception);
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    private async Task<BilInvoice> LoadInvoiceByItemAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var invoiceId = await _dbContext.BilInvoiceItems.AsNoTracking()
            .Where(x => x.Id == itemId).Select(x => x.InvoiceId).SingleOrDefaultAsync(cancellationToken);
        if (invoiceId == Guid.Empty) throw new BillingInvoiceConflictException("Receipt idempotency tidak memiliki item Billing yang valid.");
        return await _dbContext.BilInvoices.AsNoTracking()
            .Include(x => x.Items).ThenInclude(x => x.Category)
            .Include(x => x.Items).ThenInclude(x => x.Tariff).ThenInclude(x => x!.Drug).ThenInclude(x => x!.DispenseUnitMeasurement)
            .Include(x => x.DiscountApplications).ThenInclude(x => x.DiscountPolicy)
            .Include(x => x.CalculationVersions)
            .SingleAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken);
    }

    private async Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        await _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken);

    private Task AuditAsync(string action, BilInvoice invoice, BilInvoiceItem item, Guid correlationId, Guid actorUserId, bool replay) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan running invoice dari source pelayanan.", new
        {
            InvoiceId = invoice.Id,
            InvoiceItemId = item.Id,
            item.SourceDomain,
            item.SourceVersion,
            item.SourceStatus,
            item.Quantity,
            item.UnitPrice,
            item.DoctorShare,
            CorrelationId = correlationId,
            ActorUserId = actorUserId,
            IsReplay = replay
        });

    private Task AuditVoidAsync(
        BilInvoice invoice,
        BilInvoiceItem item,
        long previousSourceVersion,
        string previousSourceStatus,
        int previousCalculationVersion,
        decimal beforeGross,
        decimal afterGross,
        Guid correlationId,
        Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, "BillingInvoice.VoidItem", "Item invoice dibatalkan tanpa menghapus histori.", new
        {
            InvoiceId = invoice.Id,
            InvoiceItemId = item.Id,
            item.SourceDomain,
            PreviousSourceVersion = previousSourceVersion,
            CurrentSourceVersion = item.SourceVersion,
            PreviousSourceStatus = previousSourceStatus,
            CurrentSourceStatus = item.SourceStatus,
            PreviousCalculationVersion = previousCalculationVersion,
            CurrentCalculationVersion = invoice.CurrentCalculationVersion,
            BeforeGrossAmount = beforeGross,
            AfterGrossAmount = afterGross,
            Reason = item.VoidReason,
            CorrelationId = correlationId,
            UserId = actorUserId,
            ActorUserId = actorUserId
        });

    private static void ApplySource(BilInvoiceItem item, UpsertChargeRequest request, BillingChargeSourceSnapshot source,
        string payloadHash, Guid idempotencyKey, Guid actorUserId, bool markUpdate = true)
    {
        item.SourceDomain = source.SourceDomain;
        item.SourceDetailId = source.SourceDetailId;
        item.SourceVersion = request.SourceVersion;
        item.SourceContractVersion = request.ContractVersion.Trim();
        item.SourceStatus = source.SourceStatus;
        item.SourceOccurredAt = request.OccurredAt;
        item.CategoryId = request.CategoryId;
        item.TariffId = request.TariffId;
        item.DescriptionSnapshot = request.DescriptionSnapshot.Trim();
        item.Quantity = request.Quantity;
        item.UnitPrice = request.UnitPrice;
        item.DoctorShare = request.DoctorShare;
        item.LastIdempotencyKey = idempotencyKey;
        item.LastCorrelationId = request.CorrelationId;
        item.LastCausationId = request.CausationId;
        item.SourcePayloadHash = payloadHash;
        if (markUpdate)
        {
            item.UpdateDateTime = DateTime.UtcNow;
            item.UpdateBy = actorUserId;
        }
    }

    private static void ValidateRequest(UpsertChargeRequest request, Guid idempotencyKey)
    {
        if (idempotencyKey == Guid.Empty) throw new BillingInvoiceValidationException("Idempotency-Key wajib diisi.");
        if (request.EncounterId == Guid.Empty) throw new BillingInvoiceValidationException("EncounterId wajib diisi.");
        if (request.CategoryId == Guid.Empty) throw new BillingInvoiceValidationException("CategoryId wajib diisi.");
        if (request.SourceVersion <= 0) throw new BillingInvoiceValidationException("SourceVersion harus lebih besar dari nol.");
        if (request.OccurredAt == default) throw new BillingInvoiceValidationException("OccurredAt wajib diisi.");
        if (request.CorrelationId == Guid.Empty || request.CausationId == Guid.Empty)
            throw new BillingInvoiceValidationException("CorrelationId dan CausationId wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.DescriptionSnapshot))
            throw new BillingInvoiceValidationException("DescriptionSnapshot wajib diisi.");
        if (request.Quantity <= 0 || request.UnitPrice < 0 || request.DoctorShare < 0)
            throw new BillingInvoiceValidationException("Quantity harus positif dan nominal tidak boleh negatif.");
        if (request.DoctorShare > request.Quantity * request.UnitPrice)
            throw new BillingInvoiceValidationException("DoctorShare tidak boleh melebihi gross item.");
    }

    private static void ValidateVoidRequest(
        Guid invoiceId,
        Guid itemId,
        VoidInvoiceItemRequest request,
        Guid idempotencyKey)
    {
        if (invoiceId == Guid.Empty || itemId == Guid.Empty)
            throw new BillingInvoiceValidationException("InvoiceId dan ItemId wajib diisi.");
        if (idempotencyKey == Guid.Empty)
            throw new BillingInvoiceValidationException("Idempotency-Key wajib diisi.");
        if (request.ExpectedRowVersion == Guid.Empty)
            throw new BillingInvoiceValidationException("ExpectedRowVersion wajib diisi.");
        if (request.SourceVersion <= 0)
            throw new BillingInvoiceValidationException("SourceVersion harus lebih besar dari nol.");
        if (string.IsNullOrWhiteSpace(request.SourceStatus))
            throw new BillingInvoiceValidationException("SourceStatus wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.ContractVersion))
            throw new BillingInvoiceValidationException("ContractVersion wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new BillingInvoiceValidationException("Alasan pembatalan wajib diisi.");
        if (request.Reason.Trim().Length > 500)
            throw new BillingInvoiceValidationException("Alasan pembatalan maksimal 500 karakter.");
        if (request.CorrelationId == Guid.Empty || request.CausationId == Guid.Empty)
            throw new BillingInvoiceValidationException("CorrelationId dan CausationId wajib diisi.");
    }

    private static string ComputePayloadHash(
        UpsertChargeRequest request,
        BillingChargeSourceSnapshot source)
    {
        string canonical;

        if (string.Equals(
            source.SourceDomain,
            "ADHOC_CATALOG",
            StringComparison.OrdinalIgnoreCase))
        {
            canonical = string.Join('|',
                request.EncounterId.ToString("N"),
                source.SourceDomain,
                source.SourceDetailId,
                request.SourceVersion.ToString(CultureInfo.InvariantCulture),
                source.SourceStatus,
                request.CategoryId.ToString("N"),
                request.TariffId?.ToString("N") ?? string.Empty,
                request.DescriptionSnapshot.Trim(),
                request.Quantity.ToString(CultureInfo.InvariantCulture),
                request.UnitPrice.ToString(CultureInfo.InvariantCulture),
                request.DoctorShare.ToString(CultureInfo.InvariantCulture),
                request.ContractVersion.Trim(),
                request.CorrelationId.ToString("N"),
                request.CausationId.ToString("N"));
        }
        else
        {
            canonical = string.Join('|',
                request.EncounterId.ToString("N"),
                source.SourceDomain,
                source.SourceDetailId,
                request.SourceVersion.ToString(CultureInfo.InvariantCulture),
                source.SourceStatus,
                request.OccurredAt.ToUniversalTime()
                    .ToString("O", CultureInfo.InvariantCulture),
                request.CategoryId.ToString("N"),
                request.DescriptionSnapshot.Trim(),
                request.Quantity.ToString(CultureInfo.InvariantCulture),
                request.UnitPrice.ToString(CultureInfo.InvariantCulture),
                request.DoctorShare.ToString(CultureInfo.InvariantCulture),
                request.ContractVersion.Trim(),
                request.CorrelationId.ToString("N"),
                request.CausationId.ToString("N"));
        }

        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private static string ComputeVoidPayloadHash(
        Guid invoiceId,
        BilInvoiceItem item,
        VoidInvoiceItemRequest request)
    {
        var canonical = string.Join('|',
            invoiceId.ToString("N"),
            item.Id.ToString("N"),
            item.SourceDomain,
            item.SourceDetailId,
            request.ExpectedRowVersion.ToString("N"),
            request.SourceVersion.ToString(CultureInfo.InvariantCulture),
            request.SourceStatus.Trim().ToUpperInvariant(),
            request.ContractVersion.Trim(),
            request.Reason.Trim(),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private static string MapPaymentTypeLabel(EncounterPaymentType paymentType) => paymentType switch
    {
        EncounterPaymentType.Cash => "Tunai",
        EncounterPaymentType.Insurance => "Asuransi",
        EncounterPaymentType.CompanyGuarantor => "Penjamin Perusahaan",
        _ => paymentType.ToString()
    };

    private static string MapServiceType(EncounterType encounterType) => encounterType switch
    {
        EncounterType.Outpatient => "RAJAL",
        EncounterType.Emergency => "IGD",
        EncounterType.Inpatient => "RANAP",
        EncounterType.MedicalCheckup => "MCU",
        EncounterType.Telemedicine => "TELEMEDICINE",
        _ => throw new BillingInvoiceValidationException("Jenis encounter belum didukung untuk Billing.")
    };

    /// <summary>
    /// Pemetaan tunggal <see cref="BilInvoiceItem"/> -> <see cref="InvoiceItemResponse"/>, dipakai
    /// <see cref="MapDetail"/> (GET /{id}) maupun <see cref="BillingPayerEditService.GetEditContextAsync"/>
    /// (GET /{id}/edit-context, BE-BKC-FIX-009) supaya kedua layar membaca deskripsi/satuan/harga
    /// satuan/qty/kategori item yang identik dari satu rumus, bukan dua salinan yang bisa menyimpang.
    /// Pemanggil MUST memuat invoice dengan Include(Items).ThenInclude(Category) DAN
    /// Include(Items).ThenInclude(Tariff).ThenInclude(Drug).ThenInclude(DispenseUnitMeasurement) -
    /// tanpa include kedua, Unit selalu null untuk item farmasi.
    /// </summary>
    internal static List<InvoiceItemResponse> MapItems(IEnumerable<BilInvoiceItem> items) =>
        items.Where(x => !x.IsDelete)
            .OrderBy(x => x.CreateDateTime).Select(x => new InvoiceItemResponse
            {
                Id = x.Id,
                SourceDomain = x.SourceDomain,
                SourceDetailId = x.SourceDetailId,
                SourceVersion = x.SourceVersion,
                SourceContractVersion = x.SourceContractVersion,
                SourceStatus = x.SourceStatus,
                SourceOccurredAt = x.SourceOccurredAt,
                CategoryId = x.CategoryId,
                CategoryCode = x.Category != null ? x.Category.TariffCategoryCode : string.Empty,
                CategoryName = x.Category != null ? x.Category.TariffCategoryName : string.Empty,
                DescriptionSnapshot = x.DescriptionSnapshot,
                // Enhancement (di luar roadmap, permintaan langsung pengguna): satuan dispensing
                // obat/alkes untuk kolom "Satuan" Menu Pembayaran - hanya kategori Pharmacy/Drug/
                // Consumable-Alkes yang punya konsep satuan dispensing, kategori lain tetap null.
                Unit = x.Category != null && x.Category.IsPharmacy
                    ? x.Tariff?.Drug?.DispenseUnitMeasurement?.MeasurementName
                    : null,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                DoctorShare = x.DoctorShare,
                GrossAmount = x.Quantity * x.UnitPrice,
                Status = x.Status,
                VoidReason = x.VoidReason
            }).ToList();

    private static InvoiceDetailResponse MapDetail(BilInvoice invoice, bool isReplay)
    {
        var items = MapItems(invoice.Items);
        var activeItems = items.Where(x => x.Status != BillingInvoiceItemStatuses.Voided).ToList();
        return new InvoiceDetailResponse
        {
            Id = invoice.Id,
            EncounterId = invoice.EncounterId,
            InvoiceNumber = invoice.InvoiceNumber,
            ServiceType = invoice.ServiceType,
            Status = invoice.Status,
            CurrentCalculationVersion = invoice.CurrentCalculationVersion,
            RunningGrossAmount = activeItems.Sum(x => x.GrossAmount),
            ActiveItemCount = activeItems.Count,
            CreateDateTime = invoice.CreateDateTime,
            RowVersion = invoice.RowVersion,
            InvoiceDate = invoice.InvoiceDate,
            ClosedAt = invoice.ClosedAt,
            IsReplay = isReplay,
            Items = items,
            Discounts = invoice.DiscountApplications
                .Where(x => !x.IsDelete)
                .OrderByDescending(x => x.CreateDateTime)
                .Select(x => BillingDiscountService.Map(x, invoice.RowVersion))
                .ToList(),
            CalculationVersions = invoice.CalculationVersions
                .Where(x => !x.IsDelete)
                .OrderByDescending(x => x.VersionNo)
                .Select(x => BillingCalculationService.MapResponse(x, invoice.RowVersion))
                .ToList()
        };
    }
}

public sealed class BillingInvoiceValidationException(string message) : Exception(message);
public sealed class BillingInvoiceConflictException : Exception
{
    public BillingInvoiceConflictException(string message) : base(message) { }
    public BillingInvoiceConflictException(string message, Exception innerException) : base(message, innerException) { }
}
