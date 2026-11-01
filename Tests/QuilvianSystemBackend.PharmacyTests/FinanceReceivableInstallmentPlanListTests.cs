using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Daftar, ringkasan, dan rincian perjanjian angsuran piutang pegawai (hanya baca): nama pemilik manfaat dari identitas
/// Workforce, agregat potongan berhasil / tunggakan per perjanjian, saringan tunggakan, dan empat angka ringkasan.
/// Basis data uji dalam memori. Jangan dijalankan otomatis (dotnet test menunggu permintaan eksplisit).
/// </summary>
public sealed class FinanceReceivableInstallmentPlanListTests : IDisposable
{
    private readonly TestDatabase _database = TestDatabase.Create();
    private readonly ApplicationDbContext _db;

    // Jalur baca tidak memakai LoggerService maupun sinkron payroll, jadi keduanya tidak diperlukan di sini.
    private readonly FinanceReceivableInstallmentPlanService _service;

    private readonly Guid _ownerAProfile = Guid.NewGuid();
    private readonly Guid _ownerBProfile = Guid.NewGuid();
    private Guid _arrearsPlanId;
    private Guid _pendingPlanId;
    private Guid _receivableOfArrearsPlan;

    // SQLite uji tidak mendukung ORDER BY pada DateTimeOffset; urutan bawaan produksi (requestedAt) dipakai di PostgreSQL.
    private static InstallmentPlanQuery Query() => new() { SortBy = "planNumber", SortDirection = "asc" };

    public FinanceReceivableInstallmentPlanListTests()
    {
        _db = _database.CreateContext();
        _service = new FinanceReceivableInstallmentPlanService(_db, null!, null!);

        AddEmployee(_ownerAProfile, "NIP-A-001", "Andi Pegawai");
        AddEmployee(_ownerBProfile, "NIP-B-002", "Bunga Pegawai");

        // P1: pemilik A, DISETUJUI, angsuran ke-2 TERTUNGGAK (potongan gagal), ke-3 belum jalan.
        var r1 = AddReceivable(_ownerAProfile, outstanding: 200_000m);
        _receivableOfArrearsPlan = r1.Id;
        var p1 = AddPlan("ANG-UJI-1", r1.Id, 3, 100_000m, FinReceivableInstallmentPlanStatuses.Disetujui, approved: true);
        _arrearsPlanId = p1.Id;
        AddInstallment(p1.Id, 1, 100_000m, paid: 100_000m, FinReceivableInstallmentStatuses.Terbayar);
        AddInstallment(p1.Id, 2, 100_000m, paid: 0m, FinReceivableInstallmentStatuses.Tertunggak);
        AddInstallment(p1.Id, 3, 100_000m, paid: 0m, FinReceivableInstallmentStatuses.Dijadwalkan);

        // P2: pemilik B, MENUNGGU persetujuan; belum ada jadwal.
        var r2 = AddReceivable(_ownerBProfile, outstanding: 80_000m);
        _pendingPlanId = AddPlan("ANG-UJI-2", r2.Id, 2, 40_000m, FinReceivableInstallmentPlanStatuses.Menunggu, approved: false).Id;

        // P3: pemilik B, SELESAI; semua terpotong.
        var r3 = AddReceivable(_ownerBProfile, outstanding: 0m, status: FinReceivableStatuses.Settled);
        var p3 = AddPlan("ANG-UJI-3", r3.Id, 2, 50_000m, FinReceivableInstallmentPlanStatuses.Selesai, approved: true);
        AddInstallment(p3.Id, 1, 50_000m, paid: 50_000m, FinReceivableInstallmentStatuses.Terbayar);
        AddInstallment(p3.Id, 2, 50_000m, paid: 50_000m, FinReceivableInstallmentStatuses.Terbayar);

        // P4: pemilik A, DISETUJUI tanpa tunggakan.
        var r4 = AddReceivable(_ownerAProfile, outstanding: 90_000m);
        var p4 = AddPlan("ANG-UJI-4", r4.Id, 3, 30_000m, FinReceivableInstallmentPlanStatuses.Disetujui, approved: true);
        AddInstallment(p4.Id, 1, 30_000m, paid: 0m, FinReceivableInstallmentStatuses.Dijadwalkan);
        AddInstallment(p4.Id, 2, 30_000m, paid: 0m, FinReceivableInstallmentStatuses.Dijadwalkan);
        AddInstallment(p4.Id, 3, 30_000m, paid: 0m, FinReceivableInstallmentStatuses.Dijadwalkan);

        // P5: dihapus lunak, tidak boleh ikut hitungan mana pun.
        var r5 = AddReceivable(_ownerAProfile, outstanding: 10_000m);
        var p5 = AddPlan("ANG-UJI-5", r5.Id, 2, 5_000m, FinReceivableInstallmentPlanStatuses.Disetujui, approved: true);
        p5.IsDelete = true;

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

    private FinReceivable AddReceivable(Guid ownerProfileId, decimal outstanding, string status = FinReceivableStatuses.Outstanding)
    {
        var receivable = new FinReceivable
        {
            ReceivableNumber = $"PIU-UJI-{Guid.NewGuid():N}"[..16],
            SourceHandoffKey = Guid.NewGuid(), SourceHandoffId = Guid.NewGuid(), InvoiceId = Guid.NewGuid(),
            DebtorType = FinReceivableDebtorTypes.EmployeeBenefit,
            BenefitOwnerWorkforceProfileId = ownerProfileId, BenefitRelationship = "SELF",
            OriginalAmount = outstanding, OutstandingAmount = outstanding, AllocatedAmount = 0m,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), Status = status,
            RecognizedAt = DateTimeOffset.UtcNow.AddDays(-45), CorrelationId = Guid.NewGuid(), CausationId = Guid.NewGuid()
        };
        _db.Set<FinReceivable>().Add(receivable);
        return receivable;
    }

    private FinReceivableInstallmentPlan AddPlan(
        string number, Guid receivableId, int count, decimal amount, string status, bool approved)
    {
        var plan = new FinReceivableInstallmentPlan
        {
            PlanNumber = number, ReceivableId = receivableId, InstallmentCount = count, InstallmentAmount = amount,
            TotalAgreedAmount = count * amount, FirstDeductionPeriod = "2026-11", Status = status,
            RequestedBy = Guid.NewGuid(), RequestedAt = DateTimeOffset.UtcNow.AddDays(-10),
            ApprovedBy = approved ? Guid.NewGuid() : null, ApprovedAt = approved ? DateTimeOffset.UtcNow.AddDays(-9) : null
        };
        _db.Set<FinReceivableInstallmentPlan>().Add(plan);
        return plan;
    }

    private void AddInstallment(Guid planId, int number, decimal scheduled, decimal paid, string status)
    {
        _db.Set<FinReceivableInstallment>().Add(new FinReceivableInstallment
        {
            PlanId = planId, InstallmentNumber = number, DeductionPeriod = $"2026-{10 + number}", ScheduledAmount = scheduled,
            CarriedOverAmount = 0m, PaidAmount = paid, OutstandingAmount = scheduled - paid, Status = status
        });
    }

    [Fact]
    public async Task List_CarriesOwnerIdentity_AndSuccessfulDeductionVersusArrears()
    {
        var page = await _service.GetPagedAsync(Query(), default);

        Assert.Equal(4, page.TotalData); // P5 terhapus lunak tidak ikut.
        var arrears = page.Items.Single(x => x.Id == _arrearsPlanId);
        Assert.Equal("Andi Pegawai", arrears.BenefitOwnerName);
        Assert.Equal("NIP-A-001", arrears.BenefitOwnerEmployeeNumber);
        Assert.Equal(100_000m, arrears.PaidAmount); // hanya potongan yang berhasil; yang gagal bukan terbayar
        Assert.Equal(1, arrears.ArrearsInstallmentCount);
        Assert.Equal(100_000m, arrears.ArrearsAmount);

        var pending = page.Items.Single(x => x.Id == _pendingPlanId);
        Assert.Equal("Bunga Pegawai", pending.BenefitOwnerName);
        Assert.Equal(0m, pending.PaidAmount);
        Assert.Equal(0, pending.ArrearsInstallmentCount);
        Assert.Equal(0m, pending.ArrearsAmount);
    }

    [Fact]
    public async Task List_FiltersByArrears()
    {
        var withArrears = Query();
        withArrears.HasArrears = true;
        var onlyArrears = await _service.GetPagedAsync(withArrears, default);
        Assert.Equal(_arrearsPlanId, Assert.Single(onlyArrears.Items).Id);

        var withoutArrears = Query();
        withoutArrears.HasArrears = false;
        var rest = await _service.GetPagedAsync(withoutArrears, default);
        Assert.Equal(3, rest.TotalData);
        Assert.DoesNotContain(rest.Items, x => x.Id == _arrearsPlanId);
    }

    [Fact]
    public async Task List_SearchMatchesPlanNumber_ReceivableNumber_OwnerNameAndStaffNumber()
    {
        var byOwnerName = Query();
        byOwnerName.Search = "bunga";
        Assert.Equal(2, (await _service.GetPagedAsync(byOwnerName, default)).TotalData);

        var byStaffNumber = Query();
        byStaffNumber.Search = "NIP-A-001";
        Assert.Equal(2, (await _service.GetPagedAsync(byStaffNumber, default)).TotalData);

        var byPlanNumber = Query();
        byPlanNumber.Search = "ang-uji-3";
        Assert.Equal(1, (await _service.GetPagedAsync(byPlanNumber, default)).TotalData);

        var nothing = Query();
        nothing.Search = "tidak-ada-yang-cocok";
        Assert.Equal(0, (await _service.GetPagedAsync(nothing, default)).TotalData);
    }

    [Fact]
    public async Task Summary_CountsPendingRunningArrearsAndCompleted_IgnoringSoftDeleted()
    {
        var summary = await _service.GetSummaryAsync(default);

        Assert.Equal(1, summary.PendingCount);
        Assert.Equal(2, summary.RunningCount); // P1 dan P4; P5 terhapus lunak
        Assert.Equal(1, summary.ArrearsCount); // hanya P1
        Assert.Equal(1, summary.CompletedCount);
    }

    [Fact]
    public async Task Detail_CarriesOwnerAndCurrentReceivableBalance()
    {
        var detail = await _service.GetDetailAsync(_arrearsPlanId, default);

        Assert.Equal("Andi Pegawai", detail.BenefitOwnerName);
        Assert.Equal("NIP-A-001", detail.BenefitOwnerEmployeeNumber);
        Assert.Equal("SELF", detail.BenefitRelationship);
        Assert.Equal(200_000m, detail.ReceivableOutstandingAmount);
        Assert.Equal(_receivableOfArrearsPlan, detail.ReceivableId);
        Assert.Equal(3, detail.Installments.Count);
        Assert.NotEqual(Guid.Empty, detail.RequestedByUserId);
    }

    public void Dispose()
    {
        _db.Dispose();
        _database.Dispose();
    }
}
