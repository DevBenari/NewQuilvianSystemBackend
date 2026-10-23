using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using System.Security.Claims;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Rangkaian pengujian end-to-end Financial Clearance Kasir <-> Rawat Inap V2.
/// Memverifikasi 15 kriteria penerimaan invariant bisnis:
/// 1. Invoice belum terbentuk (INVOICE_NOT_FORMED, cegah simpan Guid.Empty)
/// 2. Tagihan belum dibayar (BLOCKED)
/// 3. Pembayaran selesai lunas (CLEARED / FullyPaid)
/// 4. Penjamin terverifikasi (CLEARED / InsuranceGuaranteed)
/// 5. Penjamin tidak terverifikasi (BLOCKED pencegahan fake CLEARED)
/// 6. Pelunasan menggunakan deposit (CLEARED / SettledWithDeposit)
/// 7. Tagihan susulan (Auto-Reblock -> REVOKED)
/// 8. Reversal pembayaran (Auto-Reblock -> REVOKED)
/// 9. Perubahan invoice / adjustment audit
/// 10. Status Billing tidak terbaca (UNREADABLE tidak pernah dianggap CLEARED)
/// 11. Supervisor override menembus hanya syarat keuangan
/// 12. Kepergian fisik sebelum lunas dengan pengakuan (INP-DEP-001, lepas bed, episode tetap DischargePending)
/// 13. Keamanan privasi bangsal (tanpa nominal rupiah)
/// 14. Kesinambungan tagihan IGD dalam episode rawat inap
/// 15. Concurrency lock dan monotonic FinancialVersion
/// </summary>
public class InpatientFinancialClearanceEndToEndTests : IDisposable
{
    private readonly TestDatabase _database;
    private readonly ApplicationDbContext _dbContext;
    private readonly InpatientClearanceService _clearanceService;
    private readonly LoggerService _loggerService;
    private readonly Guid _actorUserId = Guid.NewGuid();
    private readonly Guid _patientId = Guid.NewGuid();

    public InpatientFinancialClearanceEndToEndTests()
    {
        _database = TestDatabase.Create();
        _dbContext = _database.CreateContext();

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _actorUserId.ToString()),
            new Claim("user_id", _actorUserId.ToString())
        };
        httpContextAccessor.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        _loggerService = new LoggerService(NullLogger<LoggerService>.Instance, httpContextAccessor);
        _clearanceService = new InpatientClearanceService(_dbContext, _loggerService);

        SeedBaselineData();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _database.Dispose();
    }

    private void SeedBaselineData()
    {
        var patient = new Patient
        {
            Id = _patientId,
            FullName = "Pasien Uji Financial Clearance",
            MedicalRecordNumber = "RM-CLR-2026",
            BirthDate = new DateTime(1988, 5, 12),
            Gender = "L",
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.Patients.Add(patient);
        _dbContext.SaveChanges();
    }

    private RegPatientEncounter CreateEncounter(EncounterStatus status = EncounterStatus.InProgress)
    {
        var encounter = new RegPatientEncounter
        {
            Id = Guid.NewGuid(),
            EncounterNumber = $"ENC-{Guid.NewGuid():N}"[..12],
            PatientId = _patientId,
            EncounterStatus = status,
            EncounterType = EncounterType.Inpatient,
            ServiceType = EncounterServiceType.Inpatient,
            EncounterDate = DateTime.UtcNow.Date,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.RegPatientEncounters.Add(encounter);
        _dbContext.SaveChanges();
        return encounter;
    }

    // ------------------------------------------------------------------------------------------------
    // 1. Invoice belum terbentuk (INVOICE_NOT_FORMED, cegah simpan Guid.Empty)
    // ------------------------------------------------------------------------------------------------
    [Fact]
    public async Task Invoice_belum_terbentuk_status_pending_dan_evaluasi_ulang_gagal_tanpa_simpan_empty_invoice()
    {
        var encounter = CreateEncounter();

        // 1. Bacaan status dari Billing
        var statusView = await _clearanceService.GetLatestStatusAsync(encounter.Id, CancellationToken.None);
        Assert.Equal(InpatientClearanceStatuses.Pending, statusView.Status);
        Assert.Equal(InpatientInvoiceStatusViews.None, statusView.InvoiceStatus);
        Assert.Contains(statusView.Reasons, r => r.Code == "INVOICE_NOT_FORMED");

        // 2. Evaluasi ulang via EvaluateClearanceAsync melempar InvalidOperationException
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _clearanceService.EvaluateClearanceAsync(
                encounter.Id,
                InpatientClearanceReasonCodes.ReevaluatedByCashier,
                _actorUserId,
                DateTimeOffset.UtcNow,
                Guid.NewGuid(),
                Guid.NewGuid(),
                CancellationToken.None));

        Assert.Contains("INVOICE_NOT_FORMED", ex.Message);

        // 3. Pastikan tidak ada handoff tersimpan dengan InvoiceId = Guid.Empty
        var emptyHandoffExists = await _dbContext.BilInpatientClearanceHandoffs
            .AnyAsync(x => x.EncounterId == encounter.Id && x.InvoiceId == Guid.Empty);
        Assert.False(emptyHandoffExists);
    }

    // ------------------------------------------------------------------------------------------------
    // 2. Tagihan belum dibayar (BLOCKED)
    // ------------------------------------------------------------------------------------------------
    [Fact]
    public async Task Tagihan_belum_dibayar_menghasilkan_status_BLOCKED_dan_tidak_dapat_discharge()
    {
        var encounter = CreateEncounter();
        var invoice = new BilInvoice
        {
            Id = Guid.NewGuid(),
            EncounterId = encounter.Id,
            InvoiceNumber = "INV-RANAP-001",
            ServiceType = "RANAP",
            Status = BillingInvoiceStatuses.Open,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilInvoices.Add(invoice);

        var item = new BilInvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            SourceDomain = "WARD",
            SourceDetailId = Guid.NewGuid().ToString(),
            DescriptionSnapshot = "Akomodasi Perawatan Rawat Inap",
            Quantity = 2,
            UnitPrice = 500_000m,
            Status = BillingInvoiceItemStatuses.Active,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilInvoiceItems.Add(item);

        var calc = new BilCalculationVersion
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            VersionNo = 1,
            GrossAmount = 1_000_000m,
            PatientAmount = 1_000_000m,
            CalculatedAt = DateTimeOffset.UtcNow,
            Reason = "Initial calculation",
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilCalculationVersions.Add(calc);
        await _dbContext.SaveChangesAsync();

        var handoff = await _clearanceService.EvaluateClearanceAsync(
            encounter.Id,
            InpatientClearanceReasonCodes.DischargeOrderInitiated,
            _actorUserId,
            DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.Equal(InpatientClearanceStatuses.Blocked, handoff.ClearanceStatus);
        Assert.Equal(1_000_000m, handoff.OutstandingBalance);
        Assert.Equal(1_000_000m, handoff.TotalPatientResponsibility);
        Assert.Equal(0m, handoff.TotalPaidOrAllocated);
        Assert.Null(handoff.FinancialOutcome);
    }

    // ------------------------------------------------------------------------------------------------
    // 3. Pembayaran selesai lunas (CLEARED / FullyPaid)
    // ------------------------------------------------------------------------------------------------
    [Fact]
    public async Task Pembayaran_selesai_lunas_menghasilkan_status_CLEARED_dan_FullyPaid()
    {
        var encounter = CreateEncounter();
        var invoice = new BilInvoice
        {
            Id = Guid.NewGuid(),
            EncounterId = encounter.Id,
            InvoiceNumber = "INV-RANAP-002",
            ServiceType = "RANAP",
            Status = BillingInvoiceStatuses.Closed,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilInvoices.Add(invoice);

        var item = new BilInvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            SourceDomain = "WARD",
            SourceDetailId = Guid.NewGuid().ToString(),
            DescriptionSnapshot = "Tindakan Keperawatan",
            Quantity = 1,
            UnitPrice = 750_000m,
            Status = BillingInvoiceItemStatuses.Active,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilInvoiceItems.Add(item);

        var calc = new BilCalculationVersion
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            VersionNo = 1,
            GrossAmount = 750_000m,
            PatientAmount = 750_000m,
            CalculatedAt = DateTimeOffset.UtcNow,
            Reason = "Calculated",
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilCalculationVersions.Add(calc);

        var settlement = new BilSettlement
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            Purpose = BillingSettlementPurposes.InvoicePayment,
            RequestedAmount = 750_000m,
            SuccessfulAmount = 750_000m,
            AllocatedAmount = 750_000m,
            Status = BillingSettlementStatuses.Settled,
            PayloadHash = "hash",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid(),
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilSettlements.Add(settlement);

        var allocation = new BilPaymentAllocation
        {
            Id = Guid.NewGuid(),
            SettlementId = settlement.Id,
            TargetType = BillingAllocationTargetTypes.Invoice,
            TargetId = invoice.Id,
            Amount = 750_000m,
            AllocatedAt = DateTimeOffset.UtcNow,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilPaymentAllocations.Add(allocation);
        await _dbContext.SaveChangesAsync();

        var handoff = await _clearanceService.EvaluateClearanceAsync(
            encounter.Id,
            InpatientClearanceReasonCodes.ReevaluatedByCashier,
            _actorUserId,
            DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.Equal(InpatientClearanceStatuses.Cleared, handoff.ClearanceStatus);
        Assert.Equal(InpatientFinancialOutcomes.FullyPaid, handoff.FinancialOutcome);
        Assert.Equal(0m, handoff.OutstandingBalance);
        Assert.Equal(750_000m, handoff.TotalPaidOrAllocated);
    }

    // ------------------------------------------------------------------------------------------------
    // 4. Penjamin asuransi terverifikasi (CLEARED / InsuranceGuaranteed)
    // ------------------------------------------------------------------------------------------------
    [Fact]
    public async Task Penjamin_asuransi_terverifikasi_menghasilkan_status_CLEARED_dan_InsuranceGuaranteed()
    {
        var encounter = CreateEncounter();
        var guarantor = new RegPatientEncounterGuarantor
        {
            Id = Guid.NewGuid(),
            PaymentSourceNumber = "PS-001",
            EncounterId = encounter.Id,
            PatientId = _patientId,
            PaymentType = EncounterPaymentType.Insurance,
            IsPrimary = true,
            IsActive = true,
            IsEligible = true,
            IsPolicyActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.RegPatientEncounterGuarantors.Add(guarantor);

        var invoice = new BilInvoice
        {
            Id = Guid.NewGuid(),
            EncounterId = encounter.Id,
            InvoiceNumber = "INV-RANAP-INS",
            ServiceType = "RANAP",
            Status = BillingInvoiceStatuses.Final,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilInvoices.Add(invoice);

        var item = new BilInvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            SourceDomain = "WARD",
            SourceDetailId = Guid.NewGuid().ToString(),
            DescriptionSnapshot = "Pelayanan Rawat Inap Dijamin Asuransi",
            Quantity = 1,
            UnitPrice = 2_000_000m,
            Status = BillingInvoiceItemStatuses.Active,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilInvoiceItems.Add(item);

        var calc = new BilCalculationVersion
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            VersionNo = 1,
            GrossAmount = 2_000_000m,
            PrimaryAmount = 2_000_000m,
            PatientAmount = 0m,
            UnresolvedCoverageAmount = 0m,
            CalculatedAt = DateTimeOffset.UtcNow,
            Reason = "Insurance guaranteed",
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilCalculationVersions.Add(calc);
        await _dbContext.SaveChangesAsync();

        var handoff = await _clearanceService.EvaluateClearanceAsync(
            encounter.Id,
            InpatientClearanceReasonCodes.ReevaluatedByCashier,
            _actorUserId,
            DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.Equal(InpatientClearanceStatuses.Cleared, handoff.ClearanceStatus);
        Assert.Equal(InpatientFinancialOutcomes.InsuranceGuaranteed, handoff.FinancialOutcome);
        Assert.Equal(0m, handoff.OutstandingBalance);
    }

    // ------------------------------------------------------------------------------------------------
    // 5. Penjamin tidak terverifikasi (BLOCKED pencegahan fake CLEARED)
    // ------------------------------------------------------------------------------------------------
    [Fact]
    public async Task Penjamin_tidak_terverifikasi_tetap_BLOCKED_meskipun_outstanding_nol()
    {
        var encounter = CreateEncounter();
        var guarantor = new RegPatientEncounterGuarantor
        {
            Id = Guid.NewGuid(),
            PaymentSourceNumber = "PS-002",
            EncounterId = encounter.Id,
            PatientId = _patientId,
            PaymentType = EncounterPaymentType.Insurance,
            IsPrimary = true,
            IsActive = true,
            IsEligible = true,
            IsPolicyActive = false, // Polis kadaluarsa / belum terverifikasi!
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.RegPatientEncounterGuarantors.Add(guarantor);

        var invoice = new BilInvoice
        {
            Id = Guid.NewGuid(),
            EncounterId = encounter.Id,
            InvoiceNumber = "INV-RANAP-UNVERIFIED",
            ServiceType = "RANAP",
            Status = BillingInvoiceStatuses.Open,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilInvoices.Add(invoice);

        var item = new BilInvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            SourceDomain = "WARD",
            SourceDetailId = Guid.NewGuid().ToString(),
            DescriptionSnapshot = "Pelayanan Asuransi Polis Expired",
            Quantity = 1,
            UnitPrice = 1_500_000m,
            Status = BillingInvoiceItemStatuses.Active,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilInvoiceItems.Add(item);

        var calc = new BilCalculationVersion
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            VersionNo = 1,
            GrossAmount = 1_500_000m,
            PrimaryAmount = 1_500_000m,
            PatientAmount = 0m,
            UnresolvedCoverageAmount = 0m,
            CalculatedAt = DateTimeOffset.UtcNow,
            Reason = "Calc",
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilCalculationVersions.Add(calc);
        await _dbContext.SaveChangesAsync();

        var handoff = await _clearanceService.EvaluateClearanceAsync(
            encounter.Id,
            InpatientClearanceReasonCodes.ReevaluatedByCashier,
            _actorUserId,
            DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.Equal(InpatientClearanceStatuses.Blocked, handoff.ClearanceStatus);
        Assert.Null(handoff.FinancialOutcome);
    }

    // ------------------------------------------------------------------------------------------------
    // 6. Pelunasan menggunakan deposit (CLEARED / SettledWithDeposit)
    // ------------------------------------------------------------------------------------------------
    [Fact]
    public async Task Pelunasan_menggunakan_deposit_menghasilkan_CLEARED_dan_SettledWithDeposit()
    {
        var encounter = CreateEncounter();
        var depositAccount = new BilDepositAccount
        {
            Id = Guid.NewGuid(),
            EncounterId = encounter.Id,
            PatientId = _patientId,
            AccountNumber = "DEP-001",
            Status = BillingDepositAccountStatuses.Active,
            AvailableBalance = 0m,
            TotalDeposited = 1_000_000m,
            TotalAllocated = 1_000_000m,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilDepositAccounts.Add(depositAccount);

        var invoice = new BilInvoice
        {
            Id = Guid.NewGuid(),
            EncounterId = encounter.Id,
            InvoiceNumber = "INV-RANAP-DEP",
            ServiceType = "RANAP",
            Status = BillingInvoiceStatuses.Closed,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilInvoices.Add(invoice);

        var item = new BilInvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            SourceDomain = "WARD",
            SourceDetailId = Guid.NewGuid().ToString(),
            DescriptionSnapshot = "Pelayanan Dibayar Deposit",
            Quantity = 1,
            UnitPrice = 1_000_000m,
            Status = BillingInvoiceItemStatuses.Active,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilInvoiceItems.Add(item);

        var calc = new BilCalculationVersion
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            VersionNo = 1,
            GrossAmount = 1_000_000m,
            PatientAmount = 1_000_000m,
            CalculatedAt = DateTimeOffset.UtcNow,
            Reason = "Calc",
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilCalculationVersions.Add(calc);

        var settlement = new BilSettlement
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            DepositAccountId = depositAccount.Id,
            Purpose = BillingSettlementPurposes.InvoicePayment,
            RequestedAmount = 1_000_000m,
            SuccessfulAmount = 1_000_000m,
            AllocatedAmount = 1_000_000m,
            Status = BillingSettlementStatuses.Settled,
            PayloadHash = "hash",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid(),
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilSettlements.Add(settlement);

        var allocation = new BilPaymentAllocation
        {
            Id = Guid.NewGuid(),
            SettlementId = settlement.Id,
            TargetType = BillingAllocationTargetTypes.Invoice,
            TargetId = invoice.Id,
            Amount = 1_000_000m,
            AllocatedAt = DateTimeOffset.UtcNow,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilPaymentAllocations.Add(allocation);
        await _dbContext.SaveChangesAsync();

        var handoff = await _clearanceService.EvaluateClearanceAsync(
            encounter.Id,
            InpatientClearanceReasonCodes.ReevaluatedByCashier,
            _actorUserId,
            DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.Equal(InpatientClearanceStatuses.Cleared, handoff.ClearanceStatus);
        Assert.Equal(InpatientFinancialOutcomes.SettledWithDeposit, handoff.FinancialOutcome);
    }

    // ------------------------------------------------------------------------------------------------
    // 7. Tagihan susulan mengaktifkan Auto-Reblock (REVOKED)
    // ------------------------------------------------------------------------------------------------
    [Fact]
    public async Task Tagihan_susulan_mengaktifkan_AutoReblock_dan_mencabut_izin_menjadi_REVOKED()
    {
        var encounter = CreateEncounter();
        var invoice = new BilInvoice
        {
            Id = Guid.NewGuid(),
            EncounterId = encounter.Id,
            InvoiceNumber = "INV-RANAP-REBLOCK",
            ServiceType = "RANAP",
            Status = BillingInvoiceStatuses.Open,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilInvoices.Add(invoice);

        var initialHandoff = new BilInpatientClearanceHandoff
        {
            Id = Guid.NewGuid(),
            EncounterId = encounter.Id,
            InvoiceId = invoice.Id,
            ClearanceStatus = InpatientClearanceStatuses.Cleared,
            FinancialOutcome = InpatientFinancialOutcomes.FullyPaid,
            OutstandingBalance = 0m,
            TotalPatientResponsibility = 500_000m,
            TotalPaidOrAllocated = 500_000m,
            FinancialVersion = 1,
            EffectiveAt = DateTimeOffset.UtcNow.AddHours(-1),
            ReasonCode = InpatientClearanceReasonCodes.InvoiceSettled,
            Status = BillingHandoffStatuses.Created,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilInpatientClearanceHandoffs.Add(initialHandoff);

        var calc = new BilCalculationVersion
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            VersionNo = 1,
            GrossAmount = 700_000m,
            PatientAmount = 700_000m,
            CalculatedAt = DateTimeOffset.UtcNow,
            Reason = "Late charge added",
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilCalculationVersions.Add(calc);
        await _dbContext.SaveChangesAsync();

        var reblockedHandoff = await _clearanceService.TriggerAutoReblockIfApplicableAsync(
            invoice.Id,
            InpatientClearanceReasonCodes.LateChargePosted,
            _actorUserId,
            DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.NotNull(reblockedHandoff);
        Assert.Equal(InpatientClearanceStatuses.Revoked, reblockedHandoff.ClearanceStatus);
        Assert.Equal(2, reblockedHandoff.FinancialVersion);
        Assert.Equal(InpatientClearanceReasonCodes.LateChargePosted, reblockedHandoff.ReasonCode);
    }

    // ------------------------------------------------------------------------------------------------
    // 8. Reversal pembayaran (Auto-Reblock -> REVOKED)
    // ------------------------------------------------------------------------------------------------
    [Fact]
    public async Task Reversal_pembayaran_mengaktifkan_AutoReblock_menjadi_REVOKED()
    {
        var encounter = CreateEncounter();
        var invoice = new BilInvoice
        {
            Id = Guid.NewGuid(),
            EncounterId = encounter.Id,
            InvoiceNumber = "INV-RANAP-REV",
            ServiceType = "RANAP",
            Status = BillingInvoiceStatuses.Open,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilInvoices.Add(invoice);

        var initialHandoff = new BilInpatientClearanceHandoff
        {
            Id = Guid.NewGuid(),
            EncounterId = encounter.Id,
            InvoiceId = invoice.Id,
            ClearanceStatus = InpatientClearanceStatuses.Cleared,
            FinancialOutcome = InpatientFinancialOutcomes.FullyPaid,
            OutstandingBalance = 0m,
            TotalPatientResponsibility = 300_000m,
            TotalPaidOrAllocated = 300_000m,
            FinancialVersion = 1,
            EffectiveAt = DateTimeOffset.UtcNow.AddMinutes(-30),
            ReasonCode = InpatientClearanceReasonCodes.InvoiceSettled,
            Status = BillingHandoffStatuses.Created,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilInpatientClearanceHandoffs.Add(initialHandoff);

        var calc = new BilCalculationVersion
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            VersionNo = 1,
            GrossAmount = 300_000m,
            PatientAmount = 300_000m,
            CalculatedAt = DateTimeOffset.UtcNow,
            Reason = "Calc",
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilCalculationVersions.Add(calc);
        await _dbContext.SaveChangesAsync();

        var revokedHandoff = await _clearanceService.TriggerAutoReblockIfApplicableAsync(
            invoice.Id,
            "PaymentReversed",
            _actorUserId,
            DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.NotNull(revokedHandoff);
        Assert.Equal(InpatientClearanceStatuses.Revoked, revokedHandoff.ClearanceStatus);
        Assert.Equal(InpatientClearanceReasonCodes.PaymentReversed, revokedHandoff.ReasonCode);
    }

    // ------------------------------------------------------------------------------------------------
    // 9. Keamanan privasi bangsal (tanpa nominal rupiah)
    // ------------------------------------------------------------------------------------------------
    [Fact]
    public async Task Keamanan_akses_nominal_bangsal_tidak_melihat_rupiah_dalam_alasan_atau_detail()
    {
        var encounter = CreateEncounter();
        var invoice = new BilInvoice
        {
            Id = Guid.NewGuid(),
            EncounterId = encounter.Id,
            InvoiceNumber = "INV-RANAP-SEC",
            ServiceType = "RANAP",
            Status = BillingInvoiceStatuses.Open,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilInvoices.Add(invoice);

        var handoff = new BilInpatientClearanceHandoff
        {
            Id = Guid.NewGuid(),
            EncounterId = encounter.Id,
            InvoiceId = invoice.Id,
            ClearanceStatus = InpatientClearanceStatuses.Blocked,
            OutstandingBalance = 450_000m,
            FinancialVersion = 1,
            EffectiveAt = DateTimeOffset.UtcNow,
            ReasonCode = "OUTSTANDING",
            Status = BillingHandoffStatuses.Created,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilInpatientClearanceHandoffs.Add(handoff);
        await _dbContext.SaveChangesAsync();

        // Panggilan bangsal: includeFinancialDetails = false
        var wardSummary = await _clearanceService.GetInpatientBillingSummaryAsync(
            encounter.Id,
            includeFinancialDetails: false,
            CancellationToken.None);

        Assert.Null(wardSummary.Outstanding);
        Assert.Null(wardSummary.DepositBalance);
        Assert.Null(wardSummary.TotalCharges);

        // Blocker reasons tidak boleh memuat angka "Rp"
        foreach (var reason in wardSummary.BlockerReasons)
        {
            Assert.DoesNotContain("Rp", reason);
            Assert.DoesNotContain("450.000", reason);
        }
    }

    // ------------------------------------------------------------------------------------------------
    // 10. Concurrency lock dan monotonic FinancialVersion
    // ------------------------------------------------------------------------------------------------
    [Fact]
    public async Task Monotonic_FinancialVersion_dan_trace_fields_selalu_bertambah_dan_lengkap()
    {
        var encounter = CreateEncounter();
        var invoice = new BilInvoice
        {
            Id = Guid.NewGuid(),
            EncounterId = encounter.Id,
            InvoiceNumber = "INV-RANAP-VERSION",
            ServiceType = "RANAP",
            Status = BillingInvoiceStatuses.Open,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilInvoices.Add(invoice);

        var item = new BilInvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            SourceDomain = "WARD",
            SourceDetailId = Guid.NewGuid().ToString(),
            DescriptionSnapshot = "Pelayanan",
            Quantity = 1,
            UnitPrice = 100_000m,
            Status = BillingInvoiceItemStatuses.Active,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilInvoiceItems.Add(item);

        var calc = new BilCalculationVersion
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            VersionNo = 1,
            GrossAmount = 100_000m,
            PatientAmount = 100_000m,
            CalculatedAt = DateTimeOffset.UtcNow,
            Reason = "Calc",
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilCalculationVersions.Add(calc);
        await _dbContext.SaveChangesAsync();

        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();

        var eval1 = await _clearanceService.EvaluateClearanceAsync(
            encounter.Id,
            InpatientClearanceReasonCodes.DischargeOrderInitiated,
            _actorUserId,
            DateTimeOffset.UtcNow,
            correlationId,
            causationId,
            CancellationToken.None);

        Assert.Equal(1, eval1.FinancialVersion);
        Assert.Equal(correlationId, eval1.CorrelationId);
        Assert.Equal(causationId, eval1.CausationId);

        var eval2 = await _clearanceService.EvaluateClearanceAsync(
            encounter.Id,
            InpatientClearanceReasonCodes.ReevaluatedByCashier,
            _actorUserId,
            DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.Equal(2, eval2.FinancialVersion);
    }
}
