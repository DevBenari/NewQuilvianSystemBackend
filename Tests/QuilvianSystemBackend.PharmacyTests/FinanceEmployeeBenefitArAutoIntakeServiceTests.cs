using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Intake otomatis AR Karyawan: hanya handoff EMPLOYEE_BENEFIT yang melewati masa tunggu, tanpa handoff pengganti koreksi,
/// dan hanya baris intake NEW yang diolah. Pengolah sungguhan (FinanceBillingIntakeService.ProcessAsync) diganti pengolah
/// tiruan agar yang diuji adalah PEMILIHAN dan batasnya, bukan pembuatan piutang yang sudah punya jalur sendiri.
/// Basis data uji dalam memori. Jangan dijalankan otomatis (dotnet test menunggu permintaan eksplisit).
/// </summary>
public sealed class FinanceEmployeeBenefitArAutoIntakeServiceTests : IDisposable
{
    private static readonly TimeSpan Grace = TimeSpan.FromMinutes(15);

    private readonly TestDatabase _database = TestDatabase.Create();
    private readonly ApplicationDbContext _db;
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    private BilArHandoff _eligible = null!;
    private BilArHandoff _withinGrace = null!;
    private BilArHandoff _payer = null!;
    private BilArHandoff _alreadyAcknowledged = null!;
    private BilArHandoff _replaced = null!;
    private BilArHandoff _discoveredByManualSync = null!;
    private BilArHandoff _alreadyErrored = null!;

    public FinanceEmployeeBenefitArAutoIntakeServiceTests()
    {
        _db = _database.CreateContext();

        _eligible = AddHandoff(BillingArDebtorTypes.EmployeeBenefit, BillingHandoffStatuses.Created, ageMinutes: 30);
        _withinGrace = AddHandoff(BillingArDebtorTypes.EmployeeBenefit, BillingHandoffStatuses.Created, ageMinutes: 5);
        _payer = AddHandoff(BillingArDebtorTypes.Payer, BillingHandoffStatuses.Created, ageMinutes: 60);
        _alreadyAcknowledged = AddHandoff(BillingArDebtorTypes.EmployeeBenefit, BillingHandoffStatuses.Acknowledged, ageMinutes: 60);
        _replaced = AddHandoff(BillingArDebtorTypes.EmployeeBenefit, BillingHandoffStatuses.Created, ageMinutes: 60);
        _discoveredByManualSync = AddHandoff(BillingArDebtorTypes.EmployeeBenefit, BillingHandoffStatuses.Created, ageMinutes: 45);
        _alreadyErrored = AddHandoff(BillingArDebtorTypes.EmployeeBenefit, BillingHandoffStatuses.Created, ageMinutes: 45);

        // Koreksi salah orang: handoff pengganti diolah lewat intake ADJUSTMENT, bukan intake AR (mengikuti SyncNewFactsAsync).
        _db.Set<BilHandoffAdjustment>().Add(new BilHandoffAdjustment
        {
            ArHandoffId = _replaced.Id, Amount = 10_000m, Reason = "Koreksi pemilik manfaat", CorrelationId = _replaced.CorrelationId,
            CausationId = Guid.NewGuid(), CreatedAt = _now.AddMinutes(-50)
        });

        // Sudah ditemukan lewat sinkron manual tetapi belum diolah siapa pun.
        AddIntake(_discoveredByManualSync, FinBillingHandoffIntakeStatuses.New);
        // Pernah gagal: tetap ERROR sampai petugas menanganinya.
        AddIntake(_alreadyErrored, FinBillingHandoffIntakeStatuses.Error, "Pemilik manfaat tidak ditemukan.");

        _db.SaveChanges();
    }

    private BilArHandoff AddHandoff(string debtorType, string status, int ageMinutes)
    {
        var isEmployee = debtorType == BillingArDebtorTypes.EmployeeBenefit;
        var handoff = new BilArHandoff
        {
            InvoiceId = Guid.NewGuid(), FinalizationRecordId = Guid.NewGuid(), DebtorType = debtorType,
            DebtorReferenceId = isEmployee ? null : Guid.NewGuid(), Amount = 100_000m,
            Status = status, HandoffKey = Guid.NewGuid(), CorrelationId = Guid.NewGuid(), CausationId = Guid.NewGuid(),
            CreatedAt = _now.AddMinutes(-ageMinutes),
            BenefitOwnerWorkforceProfileId = isEmployee ? Guid.NewGuid() : null,
            BenefitRelationship = isEmployee ? "SELF" : null,
            EmployeeArOrigin = isEmployee ? BillingEmployeeArOrigins.OverAllowance : null
        };
        _db.Set<BilArHandoff>().Add(handoff);
        return handoff;
    }

    private void AddIntake(BilArHandoff handoff, string status, string? error = null) =>
        _db.Set<FinBillingHandoffIntake>().Add(new FinBillingHandoffIntake
        {
            HandoffType = FinBillingHandoffTypes.Ar, SourceHandoffId = handoff.Id, SourceHandoffKey = handoff.HandoffKey,
            Status = status, ErrorMessage = error, CorrelationId = handoff.CorrelationId, CreateDateTime = DateTime.UtcNow
        });

    private FinanceEmployeeBenefitArAutoIntakeService CreateService(RecordingProcessor processor) => new(_db, processor);

    private Task<List<FinBillingHandoffIntake>> IntakeRowsAsync() =>
        _db.Set<FinBillingHandoffIntake>().AsNoTracking().ToListAsync();

    [Fact]
    public async Task OnlyEmployeeBenefitHandoffs_PastGrace_AreDiscoveredAndProcessed()
    {
        var processor = new RecordingProcessor(_db);

        var result = await CreateService(processor).RunAsync(Guid.NewGuid(), Grace, 50, default, _now);

        // Ditemukan: hanya _eligible (dua lainnya sudah punya baris intake). Diolah: _eligible + yang ditemukan sinkron manual.
        Assert.Equal(1, result.Discovered);
        Assert.Equal(2, result.Processed);
        Assert.Equal(0, result.Failed);
        Assert.Equal(0, result.Skipped);

        var rows = await IntakeRowsAsync();
        Assert.Contains(rows, x => x.SourceHandoffKey == _eligible.HandoffKey && x.Status == FinBillingHandoffIntakeStatuses.Acknowledged);
        Assert.Contains(rows, x => x.SourceHandoffKey == _discoveredByManualSync.HandoffKey && x.Status == FinBillingHandoffIntakeStatuses.Acknowledged);

        // Tidak disentuh: di dalam masa tunggu, bukan karyawan, sudah diakui, pengganti koreksi.
        foreach (var untouched in new[] { _withinGrace, _payer, _alreadyAcknowledged, _replaced })
            Assert.DoesNotContain(rows, x => x.SourceHandoffKey == untouched.HandoffKey);

        // Pernah gagal: tidak dicoba ulang otomatis.
        var errored = Assert.Single(rows, x => x.SourceHandoffKey == _alreadyErrored.HandoffKey);
        Assert.Equal(FinBillingHandoffIntakeStatuses.Error, errored.Status);
        Assert.DoesNotContain(errored.Id, processor.Calls);
    }

    [Fact]
    public async Task SecondRun_IsIdempotent()
    {
        var first = new RecordingProcessor(_db);
        await CreateService(first).RunAsync(Guid.NewGuid(), Grace, 50, default, _now);

        var second = new RecordingProcessor(_db);
        var result = await CreateService(second).RunAsync(Guid.NewGuid(), Grace, 50, default, _now);

        Assert.Equal(0, result.Total);
        Assert.Empty(second.Calls);
        Assert.Equal(1, (await IntakeRowsAsync()).Count(x => x.SourceHandoffKey == _eligible.HandoffKey));
    }

    [Fact]
    public async Task ZeroGrace_IncludesFreshHandoffs_ButNeverOtherDebtorTypes()
    {
        var result = await CreateService(new RecordingProcessor(_db)).RunAsync(Guid.NewGuid(), TimeSpan.Zero, 50, default, _now);

        Assert.Equal(2, result.Discovered); // _eligible dan _withinGrace
        var rows = await IntakeRowsAsync();
        Assert.Contains(rows, x => x.SourceHandoffKey == _withinGrace.HandoffKey);
        Assert.DoesNotContain(rows, x => x.SourceHandoffKey == _payer.HandoffKey);
    }

    [Fact]
    public async Task ProcessingFailure_IsCountedAndStaysVisibleAsError()
    {
        var processor = new RecordingProcessor(_db) { Outcome = FinBillingHandoffIntakeStatuses.Error };

        var result = await CreateService(processor).RunAsync(Guid.NewGuid(), Grace, 50, default, _now);

        Assert.Equal(2, result.Failed);
        Assert.Equal(0, result.Processed);
        Assert.All(
            (await IntakeRowsAsync()).Where(x => x.SourceHandoffKey == _eligible.HandoffKey || x.SourceHandoffKey == _discoveredByManualSync.HandoffKey),
            row => Assert.Equal(FinBillingHandoffIntakeStatuses.Error, row.Status));
    }

    [Fact]
    public async Task AlreadyProcessedElsewhere_IsSkipped_NotAnError()
    {
        var processor = new RecordingProcessor(_db) { ThrowValidation = true };

        var result = await CreateService(processor).RunAsync(Guid.NewGuid(), Grace, 50, default, _now);

        Assert.Equal(2, result.Skipped);
        Assert.Equal(0, result.Failed);
        Assert.Equal(0, result.Processed);
    }

    [Fact]
    public async Task MaxPerCycle_BoundsDiscoveryAndProcessing()
    {
        for (var i = 0; i < 5; i++) AddHandoff(BillingArDebtorTypes.EmployeeBenefit, BillingHandoffStatuses.Created, ageMinutes: 100 + i);
        _db.SaveChanges();

        var processor = new RecordingProcessor(_db);
        var result = await CreateService(processor).RunAsync(Guid.NewGuid(), Grace, 2, default, _now);

        Assert.True(result.Discovered <= 2);
        Assert.True(result.Processed <= 2);
        Assert.True(processor.Calls.Count <= 2);
    }

    [Fact]
    public async Task NoEmployeeBenefitHandoffs_DoesNothing()
    {
        _db.Set<BilArHandoff>().RemoveRange(_db.Set<BilArHandoff>().Where(x => x.DebtorType == BillingArDebtorTypes.EmployeeBenefit));
        _db.SaveChanges();

        var processor = new RecordingProcessor(_db);
        var result = await CreateService(processor).RunAsync(Guid.NewGuid(), Grace, 50, default, _now);

        Assert.Equal(0, result.Total);
        Assert.Empty(processor.Calls);
    }

    public void Dispose()
    {
        _db.Dispose();
        _database.Dispose();
    }

    /// <summary>Meniru efek ProcessAsync pada baris intake tanpa membuat piutang.</summary>
    private sealed class RecordingProcessor(ApplicationDbContext db) : IBillingIntakeProcessor
    {
        public List<Guid> Calls { get; } = new();
        public string Outcome { get; set; } = FinBillingHandoffIntakeStatuses.Acknowledged;
        public bool ThrowValidation { get; set; }

        public async Task<BillingIntakeResponse> ProcessAsync(Guid intakeId, Guid actorUserId, CancellationToken cancellationToken)
        {
            Calls.Add(intakeId);
            if (ThrowValidation) throw new BillingIntakeValidationException("Fakta ini sudah berhasil diolah dan tidak dapat diulang.");

            var row = await db.Set<FinBillingHandoffIntake>().SingleAsync(x => x.Id == intakeId, cancellationToken);
            row.Status = Outcome;
            if (Outcome == FinBillingHandoffIntakeStatuses.Error) row.ErrorMessage = "Galat uji.";
            await db.SaveChangesAsync(cancellationToken);
            return new BillingIntakeResponse { Id = intakeId, Status = Outcome };
        }
    }
}
