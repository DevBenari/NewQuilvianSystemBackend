using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Penerimaan benefit kesehatan berbasis WorkforceProfileId untuk karyawan dan dokter tetap
/// (AC-01, AC-02, AC-05, AC-06, AC-08 sisi data, AC-09, AC-10, AC-13, AC-14, AC-16).
/// Basis data uji dalam memori; tidak ada nomor RM, MstPatient, atau MstEmployee yang dibuat untuk dokter.
/// Jangan dijalankan otomatis (dotnet test menunggu permintaan eksplisit).
/// </summary>
public sealed class WorkforceBenefitOwnerAcceptanceTests : IDisposable
{
    private readonly TestDatabase _database = TestDatabase.Create();
    private readonly ApplicationDbContext _db;
    private readonly LoggerService _logger;
    private readonly EmployeeBenefitEligibilityService _eligibility;
    private readonly EmployeeBenefitBalanceService _balance;

    private readonly Guid _planId = Guid.NewGuid();
    private readonly Guid _typeId = Guid.NewGuid();

    public WorkforceBenefitOwnerAcceptanceTests()
    {
        _db = _database.CreateContext();
        _logger = new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor());
        _eligibility = new EmployeeBenefitEligibilityService(_db);
        _balance = new EmployeeBenefitBalanceService(_db, _logger, _eligibility);

        _db.Set<MstBenefitType>().Add(new MstBenefitType
        {
            Id = _typeId,
            BenefitTypeCode = "UJI-MED",
            BenefitTypeName = "Layanan RS (Uji)",
            BenefitCategory = BenefitServiceCategories.Medical,
            IsClaimBased = false,
            CreateDateTime = DateTime.UtcNow
        });
        _db.Set<MstBenefitPlan>().Add(new MstBenefitPlan
        {
            Id = _planId,
            BenefitTypeId = _typeId,
            BenefitPlanCode = "UJI-PLAN",
            BenefitPlanName = "Plan Uji",
            CoverageType = BenefitCoverageTypes.Individual,
            CoverageLimitAmount = 10_000_000m,
            CreateDateTime = DateTime.UtcNow
        });
        _db.SaveChanges();
    }

    // ------------------------------------------------------------------ fixture

    private (Guid ProfileId, Guid DoctorId) AddDoctor(UserType type = UserType.PermanentDoctor, bool withEnrollment = true, decimal limit = 10_000_000m)
    {
        var profileId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        _db.Set<MstWorkforceProfile>().Add(new MstWorkforceProfile
        {
            Id = profileId, ProfileCode = $"WP-{profileId:N}"[..20], UserType = type, DisplayName = "dr. Uji", IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });
        _db.Set<MstDoctor>().Add(new MstDoctor
        {
            Id = doctorId, WorkforceProfileId = profileId, DoctorCode = $"DC-{doctorId:N}"[..16], DoctorNumber = $"DR-{doctorId:N}"[..12],
            FullName = "dr. Uji Sp.A", IdentityNumber = "3174000000000001", IsActive = true, CreateDateTime = DateTime.UtcNow
        });
        if (withEnrollment) AddEnrollment(profileId, limit);
        _db.SaveChanges();
        return (profileId, doctorId);
    }

    private (Guid ProfileId, Guid EmployeeId) AddEmployee(bool withEnrollment = true)
    {
        var profileId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        _db.Set<MstWorkforceProfile>().Add(new MstWorkforceProfile
        {
            Id = profileId, ProfileCode = $"WP-{profileId:N}"[..20], UserType = UserType.Employee, DisplayName = "Karyawan Uji", IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });
        _db.Set<MstEmployee>().Add(new MstEmployee
        {
            Id = employeeId, WorkforceProfileId = profileId, EmployeeCode = $"EC-{employeeId:N}"[..16], EmployeeNumber = $"NIP-{employeeId:N}"[..12],
            FullName = "Karyawan Uji", IdentityType = "KTP", IdentityNumber = "3174000000000002", Email = "uji@example.test",
            BirthDate = new DateTime(1990, 1, 1), JoinDate = new DateTime(2020, 1, 1), IsActive = true, CreateDateTime = DateTime.UtcNow
        });
        if (withEnrollment) AddEnrollment(profileId, 10_000_000m, employeeId);
        _db.SaveChanges();
        return (profileId, employeeId);
    }

    private TrxEmployeeBenefitEnrollment AddEnrollment(Guid profileId, decimal limit, Guid? employeeId = null)
    {
        var enrollment = new TrxEmployeeBenefitEnrollment
        {
            WorkforceProfileId = profileId,
            EmployeeId = employeeId,
            BenefitPlanId = _planId,
            EnrollmentNumber = $"BEN-{Guid.NewGuid():N}"[..14],
            EnrollmentStatus = EmployeeBenefitEnrollmentStatuses.Active,
            CoverageLevel = BenefitCoverageTypes.Individual,
            EnrollmentDate = DateTime.UtcNow.Date.AddDays(-30),
            EffectiveStartDate = DateTime.UtcNow.Date.AddDays(-30),
            CoverageLimitAmount = limit,
            RemainingAmount = limit,
            CreateDateTime = DateTime.UtcNow
        };
        _db.Set<TrxEmployeeBenefitEnrollment>().Add(enrollment);
        return enrollment;
    }

    // ------------------------------------------------------------------ AC-02: resolver

    [Fact]
    public async Task Resolver_PermanentDoctor_ResolvesAsDoctorOwner_WithoutEmployeeId()
    {
        var (profileId, doctorId) = AddDoctor();

        var result = await _eligibility.OwnerResolver.ResolveByWorkforceProfileAsync(profileId, default);

        Assert.True(result.IsResolved);
        Assert.Equal(WorkforceBenefitOwnerTypes.Doctor, result.Owner!.OwnerType);
        Assert.Equal(profileId, result.Owner.WorkforceProfileId);
        Assert.Equal(doctorId, result.Owner.DoctorId);
        Assert.Null(result.Owner.EmployeeId); // DoctorId tidak pernah masuk ke EmployeeId
        Assert.False(string.IsNullOrWhiteSpace(result.Owner.StaffNumber));
    }

    [Fact]
    public async Task Resolver_GuestDoctor_IsNotAllowedByPolicy()
    {
        var (profileId, _) = AddDoctor(UserType.GuestDoctor);

        var result = await _eligibility.OwnerResolver.ResolveByWorkforceProfileAsync(profileId, default);

        Assert.False(result.IsResolved);
        Assert.Equal(BenefitEligibilityReasonCodes.OwnerTypeNotAllowed, result.ReasonCode);
    }

    [Fact]
    public async Task Resolver_EmployeeProfileWithoutEmployeeRow_IsIdentityConflict()
    {
        var profileId = Guid.NewGuid();
        _db.Set<MstWorkforceProfile>().Add(new MstWorkforceProfile
        {
            Id = profileId, ProfileCode = "WP-KONFLIK", UserType = UserType.Employee, DisplayName = "Tanpa Karyawan", IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var result = await _eligibility.OwnerResolver.ResolveByWorkforceProfileAsync(profileId, default);

        Assert.False(result.IsResolved);
        Assert.Equal(BenefitEligibilityReasonCodes.OwnerIdentityConflict, result.ReasonCode);
    }

    [Fact]
    public async Task Resolver_EmployeeIdAndWorkforceProfileOfDifferentPeople_IsMismatch()
    {
        var (_, employeeId) = AddEmployee(withEnrollment: false);
        var (otherProfile, _) = AddDoctor(withEnrollment: false);

        var result = await _eligibility.OwnerResolver.ResolveRequestAsync(otherProfile, employeeId, default);

        Assert.False(result.IsResolved);
        Assert.Equal(BenefitEligibilityReasonCodes.OwnerMismatch, result.ReasonCode);
    }

    // ------------------------------------------------------------------ AC-01, AC-05, AC-06: kelayakan

    [Fact]
    public async Task Eligibility_DoctorWithActiveMedicalEnrollment_IsEligible()
    {
        var (profileId, _) = AddDoctor();

        var result = await _eligibility.CheckAsync(new BenefitEligibilityRequest
        {
            WorkforceProfileId = profileId,
            ServiceDate = DateTime.UtcNow.Date
        }, default);

        Assert.True(result.IsEligible);
        Assert.Equal(profileId, result.WorkforceProfileId);
        Assert.Equal(WorkforceBenefitOwnerTypes.Doctor, result.OwnerType);
        Assert.Null(result.EmployeeId);
        Assert.Equal(BenefitPersonKinds.Enrollment, result.BenefitPersonKind);
    }

    [Fact]
    public async Task Eligibility_DoctorWithoutEnrollment_IsNotActiveNotEmployeeNotFound()
    {
        var (profileId, _) = AddDoctor(withEnrollment: false);

        var result = await _eligibility.CheckAsync(new BenefitEligibilityRequest
        {
            WorkforceProfileId = profileId,
            ServiceDate = DateTime.UtcNow.Date
        }, default);

        Assert.False(result.IsEligible);
        Assert.Equal(BenefitEligibilityReasonCodes.EnrollmentNotActive, result.ReasonCode);
    }

    [Fact]
    public async Task Eligibility_LegacyEmployeeId_StillWorks_AndMatchesWorkforcePath()
    {
        var (profileId, employeeId) = AddEmployee();
        var day = DateTime.UtcNow.Date;

        var legacy = await _eligibility.CheckAsync(new BenefitEligibilityRequest { EmployeeId = employeeId, ServiceDate = day }, default);
        var canonical = await _eligibility.CheckAsync(new BenefitEligibilityRequest { WorkforceProfileId = profileId, ServiceDate = day }, default);

        Assert.True(legacy.IsEligible);
        Assert.True(canonical.IsEligible);
        Assert.Equal(legacy.BenefitPersonId, canonical.BenefitPersonId); // satu saldo, bukan dua
        Assert.Equal(employeeId, legacy.EmployeeId);
    }

    [Fact]
    public async Task Search_ReturnsEmployeesAndPermanentDoctors_ButNotGuestDoctors_WithMaskedNik()
    {
        AddEmployee();
        AddDoctor();
        AddDoctor(UserType.GuestDoctor);

        var result = await _eligibility.SearchWorkforceAsync("uji", default);

        Assert.Contains(result.Items, x => x.OwnerType == WorkforceBenefitOwnerTypes.Employee);
        Assert.Contains(result.Items, x => x.OwnerType == WorkforceBenefitOwnerTypes.Doctor && x.EmployeeId is null);
        Assert.Equal(2, result.Items.Count); // dokter tamu tidak ikut
        Assert.All(result.Items, x => Assert.DoesNotContain("3174000000000001", x.NationalIdMasked));
        Assert.All(result.Items, x => Assert.True(x.HasActiveMedicalEnrollment));
    }

    // ------------------------------------------------------------------ AC-09, AC-10, AC-13: saldo

    [Fact]
    public async Task Balance_Use_DeductsOnlyDoctorsOwnEnrollment_IsIdempotent_AndReturnRestores()
    {
        var (profileId, _) = AddDoctor(limit: 5_000_000m);
        var key = Guid.NewGuid();

        var read = await _balance.ReadAsync(new BenefitBalanceReadRequest
        {
            WorkforceProfileId = profileId,
            AsOfDate = DateTime.UtcNow.Date
        }, default);
        Assert.True(read.IsUsable);
        Assert.Equal(5_000_000m, read.RemainingAmount);
        Assert.Equal(profileId, read.WorkforceProfileId);

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var use = new BenefitBalanceUseRequest
        {
            SourceHandoffKey = key,
            InvoiceId = Guid.NewGuid(),
            BenefitPersonKind = read.BenefitPersonKind!,
            BenefitPersonId = read.BenefitPersonId!.Value,
            Amount = 1_200_000m
        };
        var first = await _balance.UseAsync(use, default);
        await _db.SaveChangesAsync();
        var replay = await _balance.UseAsync(use, default); // pengiriman ulang tidak memotong dua kali
        await _db.SaveChangesAsync();

        Assert.Equal(BenefitUseResults.Applied, first.Result);
        Assert.Equal(BenefitUseResults.AlreadyApplied, replay.Result);
        Assert.Equal(3_800_000m, replay.RemainingAmount);

        var returned = await _balance.ReturnAsync(new BenefitBalanceReturnRequest { SourceHandoffKey = key, Reason = "Uji reversal" }, default);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        Assert.Equal(BenefitReturnResults.Returned, returned.Result);
        Assert.Equal(5_000_000m, returned.RemainingAmount);
        // Tidak ada saldo terpisah yang dibuat hanya karena profesi dokter: tetap satu enrollment.
        Assert.Equal(1, await _db.Set<TrxEmployeeBenefitEnrollment>().CountAsync(x => x.WorkforceProfileId == profileId));
    }

    [Fact]
    public async Task Balance_Movements_ShowDoctorNameAndNumber_WhenEnrollmentOwnerIsDoctor()
    {
        var (profileId, _) = AddDoctor();
        var enrollment = await _db.Set<TrxEmployeeBenefitEnrollment>().SingleAsync(x => x.WorkforceProfileId == profileId);
        await using var transaction = await _db.Database.BeginTransactionAsync();
        await _balance.UseAsync(new BenefitBalanceUseRequest
        {
            SourceHandoffKey = Guid.NewGuid(), InvoiceId = Guid.NewGuid(), BenefitPersonKind = BenefitPersonKinds.Enrollment,
            BenefitPersonId = enrollment.Id, Amount = 100_000m
        }, default);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        var movements = await _balance.GetMovementsAsync(enrollment.Id, new BenefitMovementPagedQuery(), default);

        var row = Assert.Single(movements.Items);
        Assert.Equal("dr. Uji Sp.A", row.PersonName);
        Assert.Equal(WorkforceBenefitOwnerTypes.Doctor, row.OwnerType);
        Assert.False(string.IsNullOrWhiteSpace(row.OwnerStaffNumber));
    }

    // ------------------------------------------------------------------ AC-09: split Billing untuk dokter

    [Fact]
    public async Task BillingSplit_DoctorSnapshotWithoutEmployeeId_UsesDoctorsAllowanceAndCarriesWorkforceIdentity()
    {
        var (profileId, _) = AddDoctor(limit: 600_000m);
        var encounterId = Guid.NewGuid();
        var invoice = new BilInvoice
        {
            Id = Guid.NewGuid(), EncounterId = encounterId, InvoiceNumber = "UJI-DOK-1", ServiceType = "OUTPATIENT",
            Status = "OPEN", RowVersion = Guid.NewGuid(), CreateDateTime = DateTime.UtcNow
        };
        _db.Set<BilInvoice>().Add(invoice);
        _db.Set<RegPatientEncounterGuarantor>().Add(new RegPatientEncounterGuarantor
        {
            PaymentSourceNumber = "UJI-PS-1", EncounterId = encounterId, PatientId = Guid.NewGuid(),
            PaymentType = EncounterPaymentType.CompanyGuarantor, IsActive = true, IsPrimary = true, Priority = 1,
            EmployeeIdSnapshot = null,                 // dokter: tanpa EmployeeId palsu
            WorkforceProfileIdSnapshot = profileId,    // identitas canonical
            BenefitRelationshipSnapshot = BenefitRelationshipCodes.Self,
            CreateDateTime = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var split = new BillingEmployeeBenefitSplitService(_db, _balance, _logger, _eligibility.OwnerResolver);
        var calculation = new BilCalculationVersion
        {
            InvoiceId = invoice.Id, GrossAmount = 1_000_000m, PrimaryAmount = 1_000_000m, ExcessAmount = 0m,
            PatientAmount = 100_000m, Reason = "uji", BreakdownSnapshot = "{}", CalculatedAt = DateTimeOffset.UtcNow
        };

        var result = await split.EvaluateSplitAsync(
            invoice, calculation, patientOutstanding: 100_000m, choiceAmount: 0m, consentGivenBy: null, isFinalizing: false,
            cancellationToken: default);

        Assert.NotNull(result);
        Assert.True(result!.Response.IsUsable);
        Assert.Equal(600_000m, result.PayerAmount);           // dibatasi sisa plafon dokter
        Assert.Equal(400_000m, result.ExcessAmount);          // kelebihan plafon menjadi kewajiban pasien
        Assert.Equal(500_000m, result.DirectPayAmount);       // E + K tanpa penangguhan
        Assert.Null(result.EmployeeId);
        Assert.Equal(profileId, result.WorkforceProfileId);
        Assert.Equal(WorkforceBenefitOwnerTypes.Doctor, result.Response.BenefitOwnerType);
        Assert.False(string.IsNullOrWhiteSpace(result.Response.BenefitOwnerName));
    }

    // ------------------------------------------------------------------ AC-16: tidak ada pasien/No. RM/karyawan palsu

    [Fact]
    public async Task DoctorSetup_NeverCreatesPatientOrEmployeeRows()
    {
        var before = (Patients: await _db.MstPatients.CountAsync(), Employees: await _db.MstEmployees.CountAsync());

        var (profileId, _) = AddDoctor();
        await _eligibility.CheckAsync(new BenefitEligibilityRequest { WorkforceProfileId = profileId, ServiceDate = DateTime.UtcNow.Date }, default);

        Assert.Equal(before.Patients, await _db.MstPatients.CountAsync());
        Assert.Equal(before.Employees, await _db.MstEmployees.CountAsync());
    }

    public void Dispose()
    {
        _db.Dispose();
        _database.Dispose();
    }
}
