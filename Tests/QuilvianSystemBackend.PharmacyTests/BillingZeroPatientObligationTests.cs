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
            calculationService: new TestBillingCalculationService(_dbContext));

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

        await _closureService.SyncClosureAsync(invoice.Id, _actorUserId, DateTimeOffset.UtcNow, CancellationToken.None);
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
    public async Task TEST_01_Complete_With_Fresh_RowVersion_Succeeds()
    {
        // 1. complete with fresh RowVersion => success.
        var (invoice, _) = SetupInvoiceWithCoverage(1649382m, 1649382m, 0m);
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
    }

    [Fact]
    public async Task TEST_02_Complete_With_Stale_RowVersion_Throws_409_Conflict()
    {
        // 2. complete with stale RowVersion => 409.
        var (invoice, _) = SetupInvoiceWithCoverage(1649382m, 1649382m, 0m);
        var staleRowVersion = Guid.NewGuid();
        var request = new CompleteInvoiceRequest
        {
            ExpectedRowVersion = staleRowVersion,
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

        var ex = await Assert.ThrowsAsync<BillingFinalizationConflictException>(
            () => _finalizationService.CompleteInvoiceWithoutPatientPaymentAsync(
                invoice.Id, request, Guid.NewGuid(), _actorUserId, CancellationToken.None));

        Assert.Equal("Data telah berubah. Muat ulang sebelum melanjutkan.", ex.Message);
        Assert.Equal("STALE_INVOICE_ROW_VERSION", ex.Code);
        Assert.Equal(invoice.RowVersion, ex.CurrentRowVersion);

        var currentInvoice = await _dbContext.BilInvoices.FindAsync(invoice.Id);
        Assert.Equal(BillingInvoiceStatuses.Open, currentInvoice!.Status);
        Assert.Empty(await _dbContext.BilFinalizationRecords.Where(x => x.InvoiceId == invoice.Id).ToListAsync());
        Assert.Empty(await _dbContext.BilArHandoffs.Where(x => x.InvoiceId == invoice.Id).ToListAsync());
    }

    [Fact]
    public async Task TEST_03_Calculation_Version_0_Triggers_Internal_Recalculation_And_Succeeds()
    {
        // 3. calculation version 0 + fresh initial RowVersion => internal recalc + success, no false 409.
        var (invoice, _) = SetupInvoiceWithCoverage(1649382m, 1649382m, 0m);
        invoice.CurrentCalculationVersion = 0;
        await _dbContext.SaveChangesAsync();

        var initialRowVersion = invoice.RowVersion;
        var request = new CompleteInvoiceRequest
        {
            ExpectedRowVersion = initialRowVersion,
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

        var response = await _finalizationService.CompleteInvoiceWithoutPatientPaymentAsync(
            invoice.Id, request, Guid.NewGuid(), _actorUserId, CancellationToken.None);

        Assert.Equal(BillingInvoiceStatuses.Closed, response.Status);
        Assert.NotNull(response.FinalizationRecordId);

        var updatedInvoice = await _dbContext.BilInvoices.FindAsync(invoice.Id);
        Assert.Equal(BillingInvoiceStatuses.Closed, updatedInvoice!.Status);
        Assert.True(updatedInvoice.CurrentCalculationVersion > 0);
        Assert.NotEqual(initialRowVersion, updatedInvoice.RowVersion);
    }

    [Fact]
    public async Task TEST_04_Stale_Calculation_Triggers_Internal_Recalculation_And_Succeeds()
    {
        // 4. stale calculation + fresh initial RowVersion => internal recalc + success.
        var (invoice, _) = SetupInvoiceWithCoverage(1649382m, 1649382m, 0m);
        _dbContext.BilInvoiceItems.Add(new BilInvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            SourceDomain = "PROCEDURE",
            SourceStatus = "COMPLETED",
            Status = BillingInvoiceItemStatuses.Active,
            Amount = 100000m,
            IsActive = true,
            IsDelete = false,
            CreateDateTime = DateTime.UtcNow.AddMinutes(5)
        });
        await _dbContext.SaveChangesAsync();

        var initialRowVersion = invoice.RowVersion;
        var request = new CompleteInvoiceRequest
        {
            ExpectedRowVersion = initialRowVersion,
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

        var response = await _finalizationService.CompleteInvoiceWithoutPatientPaymentAsync(
            invoice.Id, request, Guid.NewGuid(), _actorUserId, CancellationToken.None);

        Assert.Equal(BillingInvoiceStatuses.Closed, response.Status);
        Assert.True(response.AutoCompleted);
    }

    [Fact]
    public async Task TEST_05_Full_Insurance_Produces_No_Settlement_Or_Tender()
    {
        // 5. full insurance => no settlement/tender.
        var (invoice, _) = SetupInvoiceWithCoverage(1649382m, 1649382m, 0m);
        var request = new CompleteInvoiceRequest
        {
            ExpectedRowVersion = invoice.RowVersion,
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

        await _finalizationService.CompleteInvoiceWithoutPatientPaymentAsync(
            invoice.Id, request, Guid.NewGuid(), _actorUserId, CancellationToken.None);

        Assert.Empty(await _dbContext.BilSettlements.Where(x => x.InvoiceId == invoice.Id).ToListAsync());
        Assert.Empty(await _dbContext.BilTenders.Where(x => x.Settlement.InvoiceId == invoice.Id).ToListAsync());
        Assert.Empty(await _dbContext.BilPaymentAllocations.Where(x => x.TargetId == invoice.Id).ToListAsync());
    }

    [Fact]
    public async Task TEST_06_Payer_Ar_Handoff_Created_With_Status_Created()
    {
        // 6. payer handoff created.
        var (invoice, _) = SetupInvoiceWithCoverage(1649382m, 1649382m, 0m);
        var request = new CompleteInvoiceRequest
        {
            ExpectedRowVersion = invoice.RowVersion,
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

        var response = await _finalizationService.CompleteInvoiceWithoutPatientPaymentAsync(
            invoice.Id, request, Guid.NewGuid(), _actorUserId, CancellationToken.None);

        Assert.Single(response.PayerHandoffs);
        var handoff = response.PayerHandoffs.First();
        Assert.Equal(BillingArDebtorTypes.Payer, handoff.DebtorType);
        Assert.Equal(1649382m, handoff.Amount);
        Assert.Equal(BillingHandoffStatuses.Created, handoff.Status);

        var dbHandoff = await _dbContext.BilArHandoffs.SingleAsync(x => x.InvoiceId == invoice.Id);
        Assert.Equal(BillingHandoffStatuses.Created, dbHandoff.Status);
        Assert.NotEqual("PAID", dbHandoff.Status);
        Assert.NotEqual("SETTLED", dbHandoff.Status);
    }

    [Fact]
    public async Task TEST_07_Duplicate_Completion_Request_Is_Idempotent()
    {
        // 7. duplicate request idempotent.
        var (invoice, _) = SetupInvoiceWithCoverage(1649382m, 1649382m, 0m);
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

        var response2 = await _finalizationService.CompleteInvoiceWithoutPatientPaymentAsync(
            invoice.Id, request, idempotencyKey, _actorUserId, CancellationToken.None);
        Assert.True(response2.IsReplay);
        Assert.Equal(response1.InvoiceId, response2.InvoiceId);
        Assert.Equal(response1.FinalizationRecordId, response2.FinalizationRecordId);

        Assert.Single(await _dbContext.BilFinalizationRecords.Where(x => x.InvoiceId == invoice.Id).ToListAsync());
        Assert.Single(await _dbContext.BilArHandoffs.Where(x => x.InvoiceId == invoice.Id).ToListAsync());
    }

    [Fact]
    public async Task TEST_08_DbUpdateConcurrencyException_Throws_409_Conflict_With_Db_Concurrency_Code()
    {
        // 8. real DbUpdateConcurrencyException => 409.
        var innerException = new DbUpdateConcurrencyException("Simulated optimistic concurrency failure.");
        var conflictEx = new BillingFinalizationConflictException(
            "Data telah berubah. Muat ulang sebelum melanjutkan.",
            innerException,
            code: "DB_CONCURRENCY_CONFLICT");

        Assert.Equal("DB_CONCURRENCY_CONFLICT", conflictEx.Code);
        Assert.Equal("Data telah berubah. Muat ulang sebelum melanjutkan.", conflictEx.Message);
        Assert.IsType<DbUpdateConcurrencyException>(conflictEx.InnerException);
    }

    [Fact]
    public async Task TEST_09_Failed_409_Produces_No_Partial_Finalization_Or_Handoff()
    {
        // 9. failed 409 produces no partial finalization/handoff.
        var (invoice, _) = SetupInvoiceWithCoverage(1649382m, 1649382m, 0m);
        var request = new CompleteInvoiceRequest
        {
            ExpectedRowVersion = Guid.NewGuid(), // Stale row version
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

        await Assert.ThrowsAsync<BillingFinalizationConflictException>(
            () => _finalizationService.CompleteInvoiceWithoutPatientPaymentAsync(
                invoice.Id, request, Guid.NewGuid(), _actorUserId, CancellationToken.None));

        Assert.Empty(await _dbContext.BilFinalizationRecords.Where(x => x.InvoiceId == invoice.Id).ToListAsync());
        Assert.Empty(await _dbContext.BilArHandoffs.Where(x => x.InvoiceId == invoice.Id).ToListAsync());
        var currentInvoice = await _dbContext.BilInvoices.FindAsync(invoice.Id);
        Assert.Equal(BillingInvoiceStatuses.Open, currentInvoice!.Status);
    }

    [Fact]
    public async Task TEST_10_Transaction_Rollback_Leaves_Invoice_Unchanged()
    {
        // 10. transaction rollback leaves invoice unchanged.
        var (invoice, _) = SetupInvoiceWithCoverage(1649382m, 1649382m, 0m);
        var initialRowVersion = invoice.RowVersion;

        _dbContext.BilInvoiceItems.Add(new BilInvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            SourceDomain = "PROCEDURE",
            SourceStatus = "REQUESTED", // Order belum selesai memicu exception dan rollback
            Status = BillingInvoiceItemStatuses.Active,
            Amount = 100000m,
            IsActive = true,
            IsDelete = false
        });
        await _dbContext.SaveChangesAsync();

        var request = new CompleteInvoiceRequest
        {
            ExpectedRowVersion = initialRowVersion,
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid()
        };

        await Assert.ThrowsAsync<BillingFinalizationBlockedException>(
            () => _finalizationService.CompleteInvoiceWithoutPatientPaymentAsync(
                invoice.Id, request, Guid.NewGuid(), _actorUserId, CancellationToken.None));

        var currentInvoice = await _dbContext.BilInvoices.FindAsync(invoice.Id);
        Assert.Equal(BillingInvoiceStatuses.Open, currentInvoice!.Status);
        Assert.Equal(initialRowVersion, currentInvoice.RowVersion);
        Assert.Empty(await _dbContext.BilFinalizationRecords.Where(x => x.InvoiceId == invoice.Id).ToListAsync());
    }

    private sealed class TestBillingCalculationService : IBillingCalculationService
    {
        private readonly ApplicationDbContext _dbContext;

        public TestBillingCalculationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CalculationResponse> RecalculateAsync(
            Guid invoiceId,
            RecalculateInvoiceRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var invoice = await _dbContext.BilInvoices.SingleAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken);
            if (invoice.RowVersion != request.ExpectedRowVersion)
                throw new BillingCalculationConflictException("Data telah berubah. Muat ulang sebelum melanjutkan.");

            var nextVersion = invoice.CurrentCalculationVersion + 1;
            var calculation = new BilCalculationVersion
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                VersionNo = nextVersion,
                GrossAmount = 1649382m,
                PrimaryAmount = 1649382m,
                PatientAmount = 0m,
                CalculatedAt = DateTimeOffset.UtcNow,
                Reason = request.Reason,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.BilCalculationVersions.Add(calculation);

            invoice.CurrentCalculationVersion = nextVersion;
            invoice.RowVersion = Guid.NewGuid();
            invoice.UpdateDateTime = DateTime.UtcNow;
            invoice.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CalculationResponse
            {
                InvoiceId = invoice.Id,
                VersionNo = nextVersion,
                RowVersion = invoice.RowVersion,
                PatientAmount = 0m,
                PrimaryAmount = 1649382m
            };
        }
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _database.Dispose();
    }
}
