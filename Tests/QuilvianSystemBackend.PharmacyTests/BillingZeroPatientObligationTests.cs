using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Pengujian komprehensif untuk skenario penjaminan penuh di mana kewajiban pasien bernilai nol:
/// PatientAmount = 0 (Full Insurance / Coverage Penuh).
///
/// Memastikan bahwa:
/// 1. Sisa tagihan pasien (Outstanding) dihitung murni dari PatientAmount dan bernilai 0.
/// 2. Tidak memerlukan tender/settlement pasien atau kuitansi bernilai Rp0.
/// 3. Piutang penjamin (BilArHandoff tipe PAYER) tetap terbentuk sebesar PrimaryAmount + ExcessAmount.
/// 4. Invoice berstatus FINAL dengan sisa kewajiban pasien = 0 otomatis ditutup (CLOSED).
/// 5. Tidak memerlukan departure exception saat finalisasi.
/// </summary>
public class BillingZeroPatientObligationTests : IDisposable
{
    private readonly TestDatabase _database;
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingInvoiceClosureService _closureService;
    private readonly BillingArApHandoffService _arApHandoffService;
    private readonly LoggerService _loggerService;
    private readonly BillingFinalizationService _finalizationService;

    private readonly Guid _patientId = Guid.NewGuid();
    private readonly Guid _actorUserId = Guid.NewGuid();
    private readonly Guid _insuranceProviderId = Guid.NewGuid();

    public BillingZeroPatientObligationTests()
    {
        _database = TestDatabase.Create();
        _dbContext = _database.CreateContext();

        var httpContextAccessor = new HttpContextAccessor();
        _loggerService = new LoggerService(NullLogger<LoggerService>.Instance, httpContextAccessor);

        _closureService = new BillingInvoiceClosureService(_dbContext);
        _arApHandoffService = new BillingArApHandoffService(_dbContext, _loggerService);
        _finalizationService = new BillingFinalizationService(
            _dbContext,
            new ContractBillingChargeSourceAdapter(),
            _arApHandoffService,
            _closureService,
            new BilConsumerHandoffService(_dbContext, _loggerService),
            _loggerService,
            calculationService: null);

        SeedBaselineData();
    }

    private void SeedBaselineData()
    {
        _dbContext.MstPatients.Add(new MstPatient
        {
            Id = _patientId,
            MedicalRecordNumber = "MRN-INS-001",
            FullName = "Pasien Asuransi Penuh",
            BirthDate = DateTime.UtcNow.AddYears(-35),
            IsActive = true,
            IsDelete = false
        });

        _dbContext.SaveChanges();
    }

    private (BilInvoice invoice, BilCalculationVersion calculation) SetupInvoiceWithCoverage(
        decimal totalTagihan,
        decimal primaryPayerAmount,
        decimal patientAmount,
        string status = BillingInvoiceStatuses.Open)
    {
        var encounter = new RegPatientEncounter
        {
            Id = Guid.NewGuid(),
            EncounterNumber = "ENC-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
            PatientId = _patientId,
            EncounterStatus = EncounterStatus.Billing,
            EncounterDate = DateTime.UtcNow,
            IsActive = true,
            IsDelete = false
        };
        _dbContext.RegPatientEncounters.Add(encounter);

        var guarantor = new RegPatientEncounterGuarantor
        {
            Id = Guid.NewGuid(),
            EncounterId = encounter.Id,
            InsuranceProviderId = _insuranceProviderId,
            IsPrimary = true,
            Priority = 1,
            IsActive = true,
            IsDelete = false,
            IsCancel = false
        };
        _dbContext.RegPatientEncounterGuarantors.Add(guarantor);

        var invoice = new BilInvoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = "INV-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
            EncounterId = encounter.Id,
            PatientId = _patientId,
            Status = status,
            CurrentCalculationVersion = 1,
            InvoiceDate = DateTimeOffset.UtcNow,
            RowVersion = Guid.NewGuid(),
            IsActive = true,
            IsDelete = false
        };
        _dbContext.BilInvoices.Add(invoice);

        var calculation = new BilCalculationVersion
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            VersionNo = 1,
            GrossAmount = totalTagihan,
            TaxAmount = 0m,
            PatientAmount = patientAmount,
            PrimaryAmount = primaryPayerAmount,
            ExcessAmount = 0m,
            IsLocked = false,
            CalculatedAt = DateTimeOffset.UtcNow,
            Reason = "Kalkulasi pengujian penjaminan penuh.",
            BreakdownSnapshot = "{}"
        };
        _dbContext.BilCalculationVersions.Add(calculation);
        _dbContext.SaveChanges();

        return (invoice, calculation);
    }

    [Fact]
    public async Task TEST_A_Full_Insurance_Outstanding_Adalah_Nol()
    {
        // Total tagihan Rp1.649.382, ditanggung asuransi penuh (PrimaryAmount = 1.649.382), PatientAmount = 0
        var (invoice, calculation) = SetupInvoiceWithCoverage(
            totalTagihan: 1649382m,
            primaryPayerAmount: 1649382m,
            patientAmount: 0m);

        var outstanding = await _closureService.CalculateOutstandingAsync(invoice, calculation, CancellationToken.None);

        Assert.Equal(0m, outstanding);
    }

    [Fact]
    public async Task TEST_B_Partial_Insurance_Outstanding_Sesuai_Porsi_Pasien()
    {
        // Total tagihan Rp1.649.382, asuransi menanggung Rp1.000.000, pasien membayar Rp649.382
        var (invoice, calculation) = SetupInvoiceWithCoverage(
            totalTagihan: 1649382m,
            primaryPayerAmount: 1000000m,
            patientAmount: 649382m);

        var outstanding = await _closureService.CalculateOutstandingAsync(invoice, calculation, CancellationToken.None);

        Assert.Equal(649382m, outstanding);
    }

    [Fact]
    public async Task TEST_C_Handoff_Piutang_Penjamin_Terbentuk_Saat_Coverage_Penuh()
    {
        // Menjamin bahwa porsi penjamin tetap diteruskan ke Finance/AR melalui BilArHandoff
        var (invoice, calculation) = SetupInvoiceWithCoverage(
            totalTagihan: 1649382m,
            primaryPayerAmount: 1649382m,
            patientAmount: 0m);

        var finalizationRecord = new BilFinalizationRecord
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            FinalizedAt = DateTimeOffset.UtcNow,
            FinalizedBy = _actorUserId,
            Reason = "Finalisasi tagihan penjamin penuh.",
            CorrelationId = Guid.NewGuid()
        };
        _dbContext.BilFinalizationRecords.Add(finalizationRecord);
        await _dbContext.SaveChangesAsync();

        await _arApHandoffService.StageHandoffsForFinalizationAsync(
            invoice,
            calculation,
            finalizationRecord,
            outstandingAtFinalization: 0m,
            isDepartureException: false,
            actorUserId: _actorUserId,
            cancellationToken: CancellationToken.None);

        await _dbContext.SaveChangesAsync();

        var arHandoffs = await _dbContext.BilArHandoffs
            .Where(x => x.InvoiceId == invoice.Id)
            .ToListAsync();

        // 1 handoff untuk penjamin (PAYER), tidak ada handoff piutang pasien (PATIENT_GUARANTOR)
        Assert.Single(arHandoffs);
        var payerHandoff = arHandoffs.First();
        Assert.Equal(BillingArDebtorTypes.Payer, payerHandoff.DebtorType);
        Assert.Equal(1649382m, payerHandoff.Amount);
        Assert.Equal(_insuranceProviderId, payerHandoff.DebtorReferenceId);
    }

    [Fact]
    public async Task TEST_D_Invoice_Final_Dengan_PatientAmount_Nol_Otomatis_Closed()
    {
        // Saat status invoice FINAL dan sisa kewajiban pasien = 0, SyncClosureAsync mengubahnya menjadi CLOSED
        var (invoice, _) = SetupInvoiceWithCoverage(
            totalTagihan: 1649382m,
            primaryPayerAmount: 1649382m,
            patientAmount: 0m,
            status: BillingInvoiceStatuses.Final);

        await _closureService.SyncClosureAsync(invoice, _actorUserId, CancellationToken.None);
        await _dbContext.SaveChangesAsync();

        var updatedInvoice = await _dbContext.BilInvoices.FindAsync(invoice.Id);
        Assert.NotNull(updatedInvoice);
        Assert.Equal(BillingInvoiceStatuses.Closed, updatedInvoice!.Status);
        Assert.NotNull(updatedInvoice.ClosedAt);
    }

    [Fact]
    public async Task TEST_E_Diskon_Total_100_Persen_Outstanding_Nol_Tanpa_Payer_Handoff()
    {
        // Kasus diskon/pembebasan 100%: TotalTagihan = 500.000, PatientAmount = 0, PrimaryAmount = 0
        var (invoice, calculation) = SetupInvoiceWithCoverage(
            totalTagihan: 500000m,
            primaryPayerAmount: 0m,
            patientAmount: 0m);

        var outstanding = await _closureService.CalculateOutstandingAsync(invoice, calculation, CancellationToken.None);
        Assert.Equal(0m, outstanding);

        var finalizationRecord = new BilFinalizationRecord
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            FinalizedAt = DateTimeOffset.UtcNow,
            FinalizedBy = _actorUserId,
            Reason = "Finalisasi pembebasan biaya.",
            CorrelationId = Guid.NewGuid()
        };
        _dbContext.BilFinalizationRecords.Add(finalizationRecord);
        await _dbContext.SaveChangesAsync();

        await _arApHandoffService.StageHandoffsForFinalizationAsync(
            invoice,
            calculation,
            finalizationRecord,
            outstandingAtFinalization: 0m,
            isDepartureException: false,
            actorUserId: _actorUserId,
            cancellationToken: CancellationToken.None);

        await _dbContext.SaveChangesAsync();

        var handoffs = await _dbContext.BilArHandoffs.Where(x => x.InvoiceId == invoice.Id).ToListAsync();
        Assert.Empty(handoffs); // Tidak ada piutang penjamin maupun pasien karena diskon 100%
    }

    [Fact]
    public async Task TEST_F_Full_Insurance_Complete_Without_Patient_Payment()
    {
        // Tagihan Rp1.649.382 ditanggung asuransi penuh (PatientAmount = 0).
        // Menjamin: no settlement, no tender, finalization record created, payer AR handoff created, invoice CLOSED.
        var (invoice, _) = SetupInvoiceWithCoverage(
            totalTagihan: 1649382m,
            primaryPayerAmount: 1649382m,
            patientAmount: 0m);

        var request = new CompleteInvoiceRequest
        {
            ExpectedRowVersion = invoice.RowVersion,
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

        var response = await _finalizationService.CompleteInvoiceWithoutPatientPaymentAsync(
            invoice.Id, request, Guid.NewGuid(), _actorUserId, CancellationToken.None);

        Assert.Equal(BillingInvoiceStatuses.Closed, response.Status);
        Assert.Equal(0m, response.PatientAmount);
        Assert.Equal(0m, response.PatientOutstanding);
        Assert.False(response.PaymentRequired);
        Assert.True(response.AutoCompleted);
        Assert.NotNull(response.ClosedAt);
        Assert.NotNull(response.FinalizationRecordId);

        // Pastikan tidak ada settlement dan tidak ada tender Rp0
        Assert.Empty(await _dbContext.BilSettlements.Where(x => x.InvoiceId == invoice.Id).ToListAsync());
        Assert.Empty(await _dbContext.BilTenders.Where(x => x.Settlement.InvoiceId == invoice.Id).ToListAsync());

        // Pastikan satu BilFinalizationRecord internal dibuat
        var records = await _dbContext.BilFinalizationRecords.Where(x => x.InvoiceId == invoice.Id).ToListAsync();
        Assert.Single(records);
        Assert.Equal(BillingFinalizationReasons.AutoNoPatientPayment, records.First().Reason);
        Assert.False(records.First().IsDepartureException);

        // Pastikan BilArHandoff untuk penjamin terbentuk
        Assert.Single(response.PayerHandoffs);
        var payerHandoff = response.PayerHandoffs.First();
        Assert.Equal(BillingArDebtorTypes.Payer, payerHandoff.DebtorType);
        Assert.Equal(1649382m, payerHandoff.Amount);
        Assert.Equal(BillingHandoffStatuses.Created, payerHandoff.Status);
    }

    [Fact]
    public async Task TEST_G_Duplicate_Completion_Retry_Is_Idempotent()
    {
        var (invoice, _) = SetupInvoiceWithCoverage(
            totalTagihan: 1649382m,
            primaryPayerAmount: 1649382m,
            patientAmount: 0m);

        var idempotencyKey = Guid.NewGuid();
        var request = new CompleteInvoiceRequest
        {
            ExpectedRowVersion = invoice.RowVersion,
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

        var response1 = await _finalizationService.CompleteInvoiceWithoutPatientPaymentAsync(
            invoice.Id, request, idempotencyKey, _actorUserId, CancellationToken.None);
        Assert.False(response1.IsReplay);

        // Retry kedua dengan IdempotencyKey yang sama
        var response2 = await _finalizationService.CompleteInvoiceWithoutPatientPaymentAsync(
            invoice.Id, request, idempotencyKey, _actorUserId, CancellationToken.None);
        Assert.True(response2.IsReplay);
        Assert.Equal(response1.InvoiceId, response2.InvoiceId);
        Assert.Equal(response1.FinalizationRecordId, response2.FinalizationRecordId);

        // Pastikan tetap tepat 1 finalization record dan 1 AR handoff
        Assert.Single(await _dbContext.BilFinalizationRecords.Where(x => x.InvoiceId == invoice.Id).ToListAsync());
        Assert.Single(await _dbContext.BilArHandoffs.Where(x => x.InvoiceId == invoice.Id).ToListAsync());
    }

    [Fact]
    public async Task TEST_H_Payer_Ar_Handoff_Status_Created_Not_Paid()
    {
        // AR penjamin wajib tetap berstatus CREATED (belum dibayar), Finance intake yang mengelola koleksi
        var (invoice, _) = SetupInvoiceWithCoverage(
            totalTagihan: 1649382m,
            primaryPayerAmount: 1649382m,
            patientAmount: 0m);

        var request = new CompleteInvoiceRequest
        {
            ExpectedRowVersion = invoice.RowVersion,
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

        var response = await _finalizationService.CompleteInvoiceWithoutPatientPaymentAsync(
            invoice.Id, request, Guid.NewGuid(), _actorUserId, CancellationToken.None);

        var arHandoff = await _dbContext.BilArHandoffs.SingleAsync(x => x.InvoiceId == invoice.Id);
        Assert.Equal(BillingHandoffStatuses.Created, arHandoff.Status);
        Assert.NotEqual("PAID", arHandoff.Status);
        Assert.NotEqual("SETTLED", arHandoff.Status);
    }

    [Fact]
    public async Task TEST_I_Incomplete_Order_Blocks_Completion_And_Leaves_Invoice_Open()
    {
        var (invoice, _) = SetupInvoiceWithCoverage(
            totalTagihan: 1649382m,
            primaryPayerAmount: 1649382m,
            patientAmount: 0m);

        // Tambah order aktif yang belum selesai (REQUESTED)
        _dbContext.BilInvoiceItems.Add(new BilInvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            SourceDomain = "PROCEDURE",
            SourceStatus = "REQUESTED",
            Status = BillingInvoiceItemStatuses.Active,
            Amount = 100000m,
            IsActive = true,
            IsDelete = false
        });
        await _dbContext.SaveChangesAsync();

        var request = new CompleteInvoiceRequest
        {
            ExpectedRowVersion = invoice.RowVersion,
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

        var exception = await Assert.ThrowsAsync<BillingFinalizationBlockedException>(
            () => _finalizationService.CompleteInvoiceWithoutPatientPaymentAsync(
                invoice.Id, request, Guid.NewGuid(), _actorUserId, CancellationToken.None));

        Assert.Contains("belum selesai", exception.Message);

        // Invoice harus tetap berstatus OPEN, dan tidak ada AR handoff
        var updatedInvoice = await _dbContext.BilInvoices.FindAsync(invoice.Id);
        Assert.Equal(BillingInvoiceStatuses.Open, updatedInvoice!.Status);
        Assert.Empty(await _dbContext.BilArHandoffs.Where(x => x.InvoiceId == invoice.Id).ToListAsync());
    }

    [Fact]
    public async Task TEST_J_Positive_Patient_Amount_Blocks_Direct_Completion()
    {
        // Tagihan memiliki kewajiban pasien (PatientAmount > 0)
        var (invoice, _) = SetupInvoiceWithCoverage(
            totalTagihan: 1649382m,
            primaryPayerAmount: 1000000m,
            patientAmount: 649382m);

        var request = new CompleteInvoiceRequest
        {
            ExpectedRowVersion = invoice.RowVersion,
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

        await Assert.ThrowsAsync<BillingFinalizationBlockedException>(
            () => _finalizationService.CompleteInvoiceWithoutPatientPaymentAsync(
                invoice.Id, request, Guid.NewGuid(), _actorUserId, CancellationToken.None));

        var updatedInvoice = await _dbContext.BilInvoices.FindAsync(invoice.Id);
        Assert.Equal(BillingInvoiceStatuses.Open, updatedInvoice!.Status);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _database.Dispose();
    }
}
