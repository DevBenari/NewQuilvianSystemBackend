using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Riwayat pembayaran gabungan satu kunjungan (tab "Riwayat Pembayaran" Menu Pembayaran): tender pada
/// seluruh invoice kunjungan, top-up deposit, dan pemakaian deposit.
/// Skenario acuan: tagihan Rp22.571.734, deposit Rp20.000.000 dipakai, sisa Rp2.571.734 dibayar QRIS
/// Rp1.500.000 lalu Tunai Rp1.071.734.
/// CATATAN: pengujian ini disiapkan sebagai sumber verifikasi source-level dan belum dijalankan.
/// </summary>
public class BillingEncounterPaymentHistoryTests : IDisposable
{
    private const decimal DepositAmount = 20_000_000m;
    private const decimal QrisAmount = 1_500_000m;
    private const decimal CashAmount = 1_071_734m;
    private const decimal RemainingAfterDeposit = QrisAmount + CashAmount;

    private readonly TestDatabase _database;
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingEncounterPaymentHistoryService _service;
    private readonly Guid _actorUserId = Guid.NewGuid();
    private readonly Guid _patientId = Guid.NewGuid();
    private readonly Guid _qrisMethodId = Guid.NewGuid();
    private readonly Guid _cashMethodId = Guid.NewGuid();
    private readonly Guid _transferMethodId = Guid.NewGuid();
    private readonly DateTimeOffset _t0 = new(2026, 10, 22, 8, 0, 0, TimeSpan.Zero);

    public BillingEncounterPaymentHistoryTests()
    {
        _database = TestDatabase.Create();
        _dbContext = _database.CreateContext();
        _service = new BillingEncounterPaymentHistoryService(_dbContext);

        _dbContext.MstPaymentMethods.AddRange(
            NewMethod(_qrisMethodId, "PM-QRIS", "QRIS"),
            NewMethod(_cashMethodId, "PM-CASH", "Tunai Kasir", isCash: true),
            NewMethod(_transferMethodId, "PM-TRF", "Transfer Bank"));
        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _database.Dispose();
    }

    private MstPaymentMethod NewMethod(Guid id, string code, string name, bool isCash = false) => new()
    {
        Id = id,
        PaymentMethodCode = code,
        PaymentMethodName = name,
        PaymentMethodType = isCash ? "Cash" : "Transfer",
        PaymentGroupName = isCash ? "Cash" : "Non-Cash",
        IsActive = true,
        IsAvailableForBilling = true,
        IsCash = isCash,
        CreateDateTime = DateTime.UtcNow,
        CreateBy = _actorUserId
    };

    private RegPatientEncounter NewEncounter(string number)
    {
        var encounter = new RegPatientEncounter
        {
            Id = Guid.NewGuid(),
            EncounterNumber = number,
            PatientId = _patientId,
            EncounterType = EncounterType.Inpatient,
            EncounterStatus = EncounterStatus.InProgress,
            EncounterDate = DateTime.UtcNow.Date,
            RegisteredAt = DateTime.UtcNow,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.RegPatientEncounters.Add(encounter);
        _dbContext.SaveChanges();
        return encounter;
    }

    private BilInvoice NewInvoice(Guid encounterId, string number)
    {
        var invoice = new BilInvoice
        {
            Id = Guid.NewGuid(),
            EncounterId = encounterId,
            InvoiceNumber = number,
            ServiceType = "RANAP",
            Status = BillingInvoiceStatuses.Open,
            CurrentCalculationVersion = 1,
            RowVersion = Guid.NewGuid(),
            VisitDate = DateTime.UtcNow.Date,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilInvoices.Add(invoice);
        _dbContext.SaveChanges();
        return invoice;
    }

    private BilSettlement NewSettlement(Guid invoiceId, decimal requested, decimal successful, string status)
    {
        var settlement = new BilSettlement
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            Purpose = BillingSettlementPurposes.InvoicePayment,
            RequestedAmount = requested,
            SuccessfulAmount = successful,
            AllocatedAmount = successful,
            Status = status,
            IdempotencyKey = Guid.NewGuid(),
            PayloadHash = "hash",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid(),
            StartedAt = _t0,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilSettlements.Add(settlement);
        _dbContext.SaveChanges();
        return settlement;
    }

    private BilTender NewTender(
        Guid settlementId,
        Guid methodId,
        decimal amount,
        DateTimeOffset attemptedAt,
        string kwitansi,
        string status = BillingTenderStatuses.Succeeded)
    {
        var tender = new BilTender
        {
            Id = Guid.NewGuid(),
            SettlementId = settlementId,
            PaymentMethodId = methodId,
            Amount = amount,
            Status = status,
            KwitansiNumber = kwitansi,
            IdempotencyKey = Guid.NewGuid(),
            PayloadHash = "hash",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid(),
            AttemptedAt = attemptedAt,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilTenders.Add(tender);
        _dbContext.SaveChanges();
        return tender;
    }

    private BilDepositAccount NewDepositAccount(Guid encounterId)
    {
        var account = new BilDepositAccount
        {
            Id = Guid.NewGuid(),
            EncounterId = encounterId,
            AccountNumber = "DEP-ACC-" + encounterId.ToString("N")[..6],
            AvailableBalance = 0m,
            Status = BillingDepositAccountStatuses.Active,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilDepositAccounts.Add(account);
        _dbContext.SaveChanges();
        return account;
    }

    private BilDepositMovement NewMovement(
        Guid accountId,
        string type,
        decimal amount,
        DateTimeOffset occurredAt,
        Guid? settlementId = null,
        Guid? methodId = null,
        Guid? reverses = null)
    {
        var movement = new BilDepositMovement
        {
            Id = Guid.NewGuid(),
            DepositAccountId = accountId,
            MovementType = type,
            Amount = amount,
            SettlementId = settlementId,
            PaymentMethodId = methodId,
            ReversesMovementId = reverses,
            IdempotencyKey = Guid.NewGuid(),
            PayloadHash = "hash",
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid(),
            OccurredAt = occurredAt,
            Reason = "Uji",
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.BilDepositMovements.Add(movement);
        _dbContext.SaveChanges();
        return movement;
    }

    /// <summary>Kunjungan RANAP: top-up 20 juta, deposit dipakai 20 juta, lalu QRIS dan Tunai untuk sisanya.</summary>
    private (RegPatientEncounter Encounter, BilInvoice Invoice, BilDepositMovement TopUp, BilDepositMovement Allocation)
        SeedFullScenario()
    {
        var encounter = NewEncounter("ENC-HIST-001");
        var invoice = NewInvoice(encounter.Id, "INV-HIST-001");
        var account = NewDepositAccount(encounter.Id);

        var topUp = NewMovement(account.Id, BillingDepositMovementTypes.TopUp, DepositAmount, _t0, methodId: _transferMethodId);

        // Pemakaian deposit: settlement SETTLED TANPA tender (BillingAllocationService).
        var depositSettlement = NewSettlement(invoice.Id, DepositAmount, DepositAmount, BillingSettlementStatuses.Settled);
        var allocation = NewMovement(
            account.Id, BillingDepositMovementTypes.Allocation, DepositAmount, _t0.AddHours(1),
            settlementId: depositSettlement.Id);

        var tenderSettlement = NewSettlement(invoice.Id, RemainingAfterDeposit, RemainingAfterDeposit, BillingSettlementStatuses.Settled);
        NewTender(tenderSettlement.Id, _qrisMethodId, QrisAmount, _t0.AddHours(2), "KW-001");
        NewTender(tenderSettlement.Id, _cashMethodId, CashAmount, _t0.AddHours(3), "KW-002");

        return (encounter, invoice, topUp, allocation);
    }

    [Fact]
    public async Task Semua_Sumber_Tampil_Kronologis_Dan_Ringkasan_Benar()
    {
        var (encounter, _, _, _) = SeedFullScenario();

        var result = await _service.GetAsync(encounter.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(
            new[]
            {
                EncounterPaymentHistoryEntryTypes.DepositTopUp,
                EncounterPaymentHistoryEntryTypes.DepositAllocation,
                EncounterPaymentHistoryEntryTypes.TenderPayment,
                EncounterPaymentHistoryEntryTypes.TenderPayment
            },
            result!.Entries.Select(x => x.EntryType).ToArray());
        Assert.Equal(4, result.Summary.EntryCount);
        Assert.Equal(RemainingAfterDeposit, result.Summary.TotalTenderReceived);
        Assert.Equal(DepositAmount, result.Summary.TotalDepositTopUp);
        Assert.Equal(DepositAmount, result.Summary.TotalDepositUsed);
    }

    [Fact]
    public async Task Pemakaian_Deposit_Tanpa_Tender_Tetap_Tampil_Dengan_Invoice_Dan_Tanpa_Kwitansi()
    {
        var (encounter, invoice, _, allocation) = SeedFullScenario();

        var result = await _service.GetAsync(encounter.Id, CancellationToken.None);

        var entry = Assert.Single(result!.Entries, x => x.EntryType == EncounterPaymentHistoryEntryTypes.DepositAllocation);
        Assert.Equal(allocation.Id, entry.Id);
        Assert.Equal("Pemakaian Deposit", entry.EntryTypeName);
        Assert.Equal("Deposit Pasien", entry.PaymentMethodName);
        Assert.Equal(invoice.Id, entry.InvoiceId);
        Assert.Equal("INV-HIST-001", entry.InvoiceNumber);
        Assert.Equal(DepositAmount, entry.Amount);
        Assert.False(entry.HasKwitansi);
        Assert.Equal("SUCCEEDED", entry.Status);
        Assert.Null(entry.TransactionClassification);
    }

    [Fact]
    public async Task Top_Up_Deposit_Membawa_Metode_Dan_Tidak_Terkait_Invoice()
    {
        var (encounter, _, topUp, _) = SeedFullScenario();

        var result = await _service.GetAsync(encounter.Id, CancellationToken.None);

        var entry = Assert.Single(result!.Entries, x => x.EntryType == EncounterPaymentHistoryEntryTypes.DepositTopUp);
        Assert.Equal(topUp.Id, entry.Id);
        Assert.Equal("Transfer Bank", entry.PaymentMethodName);
        Assert.Null(entry.InvoiceId);
        Assert.StartsWith("DEP-", entry.DocumentNumber);
    }

    [Fact]
    public async Task Tender_Pertama_Cicilan_Dan_Tender_Yang_Memenuhi_Target_Pelunasan()
    {
        var (encounter, _, _, _) = SeedFullScenario();

        var result = await _service.GetAsync(encounter.Id, CancellationToken.None);

        var tenders = result!.Entries.Where(x => x.EntryType == EncounterPaymentHistoryEntryTypes.TenderPayment).ToList();
        Assert.Equal(2, tenders.Count);

        Assert.Equal("QRIS", tenders[0].PaymentMethodName);
        Assert.Equal("CICILAN", tenders[0].TransactionClassification);
        Assert.Equal("SEBAGIAN", tenders[0].PostTransactionStatus);
        Assert.Equal(CashAmount, tenders[0].RemainingOutstanding);
        Assert.True(tenders[0].HasKwitansi);

        Assert.Equal("Tunai Kasir", tenders[1].PaymentMethodName);
        Assert.Equal("PELUNASAN", tenders[1].TransactionClassification);
        Assert.Equal("LUNAS", tenders[1].PostTransactionStatus);
        Assert.Equal(0m, tenders[1].RemainingOutstanding);
    }

    [Fact]
    public async Task Top_Up_Yang_Dibalik_Ditandai_REVERSED_Dan_Tidak_Dihitung_Pada_Ringkasan()
    {
        var (encounter, _, topUp, _) = SeedFullScenario();
        var account = _dbContext.BilDepositAccounts.Single(x => x.EncounterId == encounter.Id);
        NewMovement(account.Id, BillingDepositMovementTypes.Reversal, DepositAmount, _t0.AddHours(4), reverses: topUp.Id);

        var result = await _service.GetAsync(encounter.Id, CancellationToken.None);

        var entry = Assert.Single(result!.Entries, x => x.Id == topUp.Id);
        Assert.Equal("REVERSED", entry.Status);
        Assert.Equal("Dibalik", entry.StatusName);
        Assert.Equal(0m, result.Summary.TotalDepositTopUp);
        // Pembalikan itu sendiri adalah koreksi ledger, bukan pembayaran, jadi tidak menjadi baris.
        Assert.Equal(4, result.Entries.Count);
    }

    [Fact]
    public async Task Alokasi_Yang_Dipulihkan_Ditandai_REVERSED_Dan_Tidak_Dihitung_Sebagai_Terpakai()
    {
        var (encounter, _, _, allocation) = SeedFullScenario();
        var account = _dbContext.BilDepositAccounts.Single(x => x.EncounterId == encounter.Id);
        NewMovement(account.Id, BillingDepositMovementTypes.Release, DepositAmount, _t0.AddHours(4), reverses: allocation.Id);

        var result = await _service.GetAsync(encounter.Id, CancellationToken.None);

        Assert.Equal("REVERSED", Assert.Single(result!.Entries, x => x.Id == allocation.Id).Status);
        Assert.Equal(0m, result.Summary.TotalDepositUsed);
    }

    [Fact]
    public async Task Tender_Gagal_Tampil_Tetapi_Tidak_Dihitung_Sebagai_Diterima()
    {
        var encounter = NewEncounter("ENC-HIST-002");
        var invoice = NewInvoice(encounter.Id, "INV-HIST-002");
        var settlement = NewSettlement(invoice.Id, 500_000m, 0m, BillingSettlementStatuses.InProgress);
        NewTender(settlement.Id, _qrisMethodId, 500_000m, _t0, "", BillingTenderStatuses.Failed);

        var result = await _service.GetAsync(encounter.Id, CancellationToken.None);

        var entry = Assert.Single(result!.Entries);
        Assert.Equal("FAILED", entry.Status);
        Assert.Equal("PENDING", entry.TransactionClassification);
        Assert.False(entry.HasKwitansi);
        Assert.Equal(0m, result.Summary.TotalTenderReceived);
    }

    [Fact]
    public async Task Pembayaran_Kunjungan_Lain_Tidak_Ikut_Tampil()
    {
        var (encounter, _, _, _) = SeedFullScenario();

        var other = NewEncounter("ENC-HIST-OTHER");
        var otherInvoice = NewInvoice(other.Id, "INV-HIST-OTHER");
        var otherSettlement = NewSettlement(otherInvoice.Id, 900_000m, 900_000m, BillingSettlementStatuses.Settled);
        NewTender(otherSettlement.Id, _cashMethodId, 900_000m, _t0.AddHours(5), "KW-OTHER");

        var result = await _service.GetAsync(encounter.Id, CancellationToken.None);

        Assert.Equal(4, result!.Entries.Count);
        Assert.DoesNotContain(result.Entries, x => x.InvoiceNumber == "INV-HIST-OTHER");
        Assert.DoesNotContain(result.Entries, x => x.DocumentNumber == "KW-OTHER");
    }

    [Fact]
    public async Task Tender_Pada_Semua_Invoice_Satu_Kunjungan_Ikut_Tampil()
    {
        var (encounter, _, _, _) = SeedFullScenario();

        var secondInvoice = NewInvoice(encounter.Id, "INV-HIST-001B");
        var settlement = NewSettlement(secondInvoice.Id, 250_000m, 250_000m, BillingSettlementStatuses.Settled);
        NewTender(settlement.Id, _cashMethodId, 250_000m, _t0.AddHours(6), "KW-003");

        var result = await _service.GetAsync(encounter.Id, CancellationToken.None);

        Assert.Equal(5, result!.Entries.Count);
        Assert.Contains(result.Entries, x => x.InvoiceNumber == "INV-HIST-001B" && x.DocumentNumber == "KW-003");
    }

    [Fact]
    public async Task Kunjungan_Tanpa_Pembayaran_Mengembalikan_Daftar_Kosong()
    {
        var encounter = NewEncounter("ENC-HIST-EMPTY");

        var result = await _service.GetAsync(encounter.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result!.Entries);
        Assert.Equal(0, result.Summary.EntryCount);
    }

    [Fact]
    public async Task Kunjungan_Tidak_Ada_Mengembalikan_Null_Dan_Id_Kosong_Ditolak()
    {
        Assert.Null(await _service.GetAsync(Guid.NewGuid(), CancellationToken.None));
        await Assert.ThrowsAsync<BillingInvoiceValidationException>(
            () => _service.GetAsync(Guid.Empty, CancellationToken.None));
    }
}
