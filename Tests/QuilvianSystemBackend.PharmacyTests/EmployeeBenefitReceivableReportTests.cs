using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Laporan HR "Piutang Benefit Pegawai" (hanya baca): hanya piutang EMPLOYEE_BENEFIT, nilai dari Finance, potongan
/// gaji yang berhasil = PaidAmount angsuran pada perjanjian yang sah. Basis data uji dalam memori.
/// Jangan dijalankan otomatis (dotnet test menunggu permintaan eksplisit).
/// </summary>
public sealed class EmployeeBenefitReceivableReportTests : IDisposable
{
    private readonly TestDatabase _database = TestDatabase.Create();
    private readonly ApplicationDbContext _db;
    private readonly EmployeeBenefitReceivableReportService _service;

    // SQLite uji tidak mendukung ORDER BY pada DateTimeOffset; urutan bawaan produksi (recognizedAt) dipakai di PostgreSQL.
    // Uji memakai dueDate (DateOnly) supaya urutan tetap sah di basis data uji.
    private static EmployeeBenefitReceivableReportQuery Query() => new() { SortBy = "dueDate" };

    private readonly Guid _ownerAProfile = Guid.NewGuid();
    private readonly Guid _ownerBProfile = Guid.NewGuid();
    private Guid _receivableWithPlan;
    private Guid _receivableWithoutPlan;

    public EmployeeBenefitReceivableReportTests()
    {
        _db = _database.CreateContext();
        _service = new EmployeeBenefitReceivableReportService(_db);

        AddEmployee(_ownerAProfile, "NIP-A-001", "Andi Pegawai");
        AddEmployee(_ownerBProfile, "NIP-B-002", "Bunga Pegawai");

        var invoice = new BilInvoice
        {
            Id = Guid.NewGuid(), EncounterId = Guid.NewGuid(), InvoiceNumber = "INV-UJI-HR-001", ServiceType = "OUTPATIENT",
            Status = "FINAL", RowVersion = Guid.NewGuid(), CreateDateTime = DateTime.UtcNow
        };
        _db.Set<BilInvoice>().Add(invoice);

        // R1: sebagian sudah dipotong lewat payroll (perjanjian DISETUJUI).
        var r1 = AddReceivable(_ownerAProfile, original: 400_000m, outstanding: 150_000m, allocated: 250_000m,
            origin: BillingEmployeeArOrigins.OverAllowance, status: FinReceivableStatuses.Partial, invoiceId: invoice.Id);
        _receivableWithPlan = r1.Id;
        var plan = new FinReceivableInstallmentPlan
        {
            PlanNumber = "ANG-UJI-001", ReceivableId = r1.Id, InstallmentCount = 4, InstallmentAmount = 100_000m,
            TotalAgreedAmount = 400_000m, FirstDeductionPeriod = "2026-11", Status = FinReceivableInstallmentPlanStatuses.Disetujui,
            RequestedBy = Guid.NewGuid(), RequestedAt = DateTimeOffset.UtcNow.AddDays(-40),
            ApprovedBy = Guid.NewGuid(), ApprovedAt = DateTimeOffset.UtcNow.AddDays(-39)
        };
        _db.Set<FinReceivableInstallmentPlan>().Add(plan);
        AddInstallment(plan.Id, 1, "2026-11", scheduled: 100_000m, paid: 100_000m, FinReceivableInstallmentStatuses.Terbayar, DateTimeOffset.UtcNow.AddDays(-20));
        AddInstallment(plan.Id, 2, "2026-12", scheduled: 100_000m, paid: 50_000m, FinReceivableInstallmentStatuses.TerbayarSebagian, DateTimeOffset.UtcNow.AddDays(-5));
        AddInstallment(plan.Id, 3, "2027-01", scheduled: 100_000m, paid: 0m, FinReceivableInstallmentStatuses.Dijadwalkan, null);
        AddInstallment(plan.Id, 4, "2027-02", scheduled: 100_000m, paid: 0m, FinReceivableInstallmentStatuses.Dijadwalkan, null);

        // R2: belum ada perjanjian, asal campuran.
        _receivableWithoutPlan = AddReceivable(_ownerBProfile, original: 80_000m, outstanding: 80_000m, allocated: 0m,
            origin: BillingEmployeeArOrigins.Mixed, status: FinReceivableStatuses.Outstanding).Id;

        // R3: piutang penjamin (bukan karyawan) tidak boleh ikut laporan HR.
        AddReceivable(null, original: 1_000_000m, outstanding: 1_000_000m, allocated: 0m, origin: null,
            status: FinReceivableStatuses.Outstanding, debtorType: FinReceivableDebtorTypes.Payer);

        // R4: dibatalkan (mis. koreksi pemilik) tidak tampil secara bawaan.
        AddReceivable(_ownerAProfile, original: 10_000m, outstanding: 10_000m, allocated: 0m,
            origin: BillingEmployeeArOrigins.OverAllowance, status: FinReceivableStatuses.Cancelled);

        _db.SaveChanges();
    }

    private void AddEmployee(Guid profileId, string number, string name)
    {
        var employeeId = Guid.NewGuid();
        _db.Set<MstWorkforceProfile>().Add(new MstWorkforceProfile
        {
            Id = profileId, ProfileCode = $"WP-{profileId:N}"[..20], UserType = UserType.Employee, DisplayName = name,
            IsActive = true, CreateDateTime = DateTime.UtcNow
        });
        _db.Set<MstEmployee>().Add(new MstEmployee
        {
            Id = employeeId, WorkforceProfileId = profileId, EmployeeCode = $"EC-{employeeId:N}"[..16], EmployeeNumber = number,
            FullName = name, IdentityType = "KTP", IdentityNumber = $"31740000{Random.Shared.Next(10000000, 99999999)}",
            Email = $"{employeeId:N}@example.test", BirthDate = new DateTime(1990, 1, 1), JoinDate = new DateTime(2020, 1, 1),
            IsActive = true, CreateDateTime = DateTime.UtcNow
        });
    }

    private FinReceivable AddReceivable(
        Guid? ownerProfileId, decimal original, decimal outstanding, decimal allocated, string? origin, string status,
        Guid? invoiceId = null, string debtorType = FinReceivableDebtorTypes.EmployeeBenefit)
    {
        var receivable = new FinReceivable
        {
            ReceivableNumber = $"PIU-UJI-{Guid.NewGuid():N}"[..16],
            SourceHandoffKey = Guid.NewGuid(), SourceHandoffId = Guid.NewGuid(), InvoiceId = invoiceId ?? Guid.NewGuid(),
            DebtorType = debtorType,
            BenefitOwnerWorkforceProfileId = ownerProfileId,
            BenefitRelationship = ownerProfileId.HasValue ? "SELF" : null,
            EmployeeArOrigin = origin,
            OriginalAmount = original, OutstandingAmount = outstanding, AllocatedAmount = allocated,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), Status = status,
            RecognizedAt = DateTimeOffset.UtcNow.AddDays(-45), CorrelationId = Guid.NewGuid(), CausationId = Guid.NewGuid()
        };
        _db.Set<FinReceivable>().Add(receivable);
        return receivable;
    }

    private void AddInstallment(
        Guid planId, int number, string period, decimal scheduled, decimal paid, string status, DateTimeOffset? resultAt)
    {
        _db.Set<FinReceivableInstallment>().Add(new FinReceivableInstallment
        {
            PlanId = planId, InstallmentNumber = number, DeductionPeriod = period, ScheduledAmount = scheduled,
            CarriedOverAmount = 0m, PaidAmount = paid, OutstandingAmount = scheduled - paid, Status = status, LastResultAt = resultAt
        });
    }

    [Fact]
    public async Task List_OnlyEmployeeBenefitReceivables_ExcludesPayerAndCancelled()
    {
        var page = await _service.GetPagedAsync(Query(), default);

        Assert.Equal(2, page.TotalData);
        Assert.All(page.Items, row => Assert.NotNull(row.BenefitOwnerWorkforceProfileId));
        Assert.DoesNotContain(page.Items, row => row.OriginalAmount == 1_000_000m);
        Assert.DoesNotContain(page.Items, row => row.Status == FinReceivableStatuses.Cancelled);
    }

    [Fact]
    public async Task Row_CarriesFinanceValues_OwnerIdentity_AndSuccessfulPayrollDeductionsOnly()
    {
        var query = Query();
        query.Search = "NIP-A";
        var page = await _service.GetPagedAsync(query, default);

        var row = Assert.Single(page.Items);
        Assert.Equal(_receivableWithPlan, row.ReceivableId);
        Assert.Equal("Andi Pegawai", row.BenefitOwnerName);
        Assert.Equal("NIP-A-001", row.BenefitOwnerStaffNumber);
        Assert.Equal("INV-UJI-HR-001", row.InvoiceNumber);
        Assert.Equal(400_000m, row.OriginalAmount);
        Assert.Equal(250_000m, row.AllocatedAmount);
        Assert.Equal(150_000m, row.OutstandingAmount);
        // Potongan berhasil hanya PaidAmount (100.000 + 50.000); jadwal yang belum jalan tidak dihitung.
        Assert.Equal(150_000m, row.PayrollDeductedAmount);
        Assert.Equal(FinReceivableInstallmentPlanStatuses.Disetujui, row.InstallmentPlanStatus);
        Assert.Equal("2026-12", row.LatestDeductionPeriod);
        Assert.Equal(FinReceivableInstallmentStatuses.TerbayarSebagian, row.LatestDeductionStatus);
    }

    [Fact]
    public async Task Filters_ByOrigin_AndByInstallmentPlanPresence()
    {
        var byOrigin = Query();
        byOrigin.EmployeeArOrigin = BillingEmployeeArOrigins.Mixed;
        var mixed = await _service.GetPagedAsync(byOrigin, default);
        Assert.Equal(_receivableWithoutPlan, Assert.Single(mixed.Items).ReceivableId);

        var byPlan = Query();
        byPlan.HasApprovedInstallmentPlan = false;
        var withoutPlan = await _service.GetPagedAsync(byPlan, default);
        Assert.Equal(_receivableWithoutPlan, Assert.Single(withoutPlan.Items).ReceivableId);

        var byOwner = Query();
        byOwner.BenefitOwnerWorkforceProfileId = _ownerBProfile;
        var owner = await _service.GetPagedAsync(byOwner, default);
        Assert.Equal(_receivableWithoutPlan, Assert.Single(owner.Items).ReceivableId);
    }

    [Fact]
    public async Task Summary_SumsFinanceValues_AndCountsReceivablesUnderInstallment()
    {
        var summary = await _service.GetSummaryAsync(new EmployeeBenefitReceivableReportQuery(), default);

        Assert.Equal(2, summary.TotalReceivable);
        Assert.Equal(480_000m, summary.TotalOriginalAmount);
        Assert.Equal(230_000m, summary.TotalOutstandingAmount);
        Assert.Equal(250_000m, summary.TotalAllocatedAmount);
        Assert.Equal(1, summary.UnderInstallmentCount);
        Assert.Equal(150_000m, summary.UnderInstallmentOutstandingAmount);
        Assert.Equal(150_000m, summary.TotalPayrollDeductedAmount);
        Assert.Equal(0, summary.SettledCount);
    }

    [Fact]
    public async Task Detail_ReturnsInstallmentSchedule_AndRejectsNonEmployeeBenefitReceivable()
    {
        var detail = await _service.GetDetailAsync(_receivableWithPlan, default);

        var plan = Assert.Single(detail.Plans);
        Assert.Equal(4, plan.Installments.Count);
        Assert.Equal(new[] { 1, 2, 3, 4 }, plan.Installments.Select(i => i.InstallmentNumber).ToArray());

        var payerId = _db.Set<FinReceivable>().Single(x => x.DebtorType == FinReceivableDebtorTypes.Payer).Id;
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetDetailAsync(payerId, default));
    }

    public void Dispose()
    {
        _db.Dispose();
        _database.Dispose();
    }
}
