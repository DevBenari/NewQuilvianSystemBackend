using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Controllers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Tests.HumanResource;

public sealed class RecruitmentJourneyWorkflowTests : IDisposable
{
    private readonly JourneyTestDb _db = JourneyTestDb.Create();
    private readonly Guid _siteId = Guid.NewGuid();
    private readonly Guid _unitId = Guid.NewGuid();
    private readonly Guid _positionId = Guid.NewGuid();
    private readonly Guid _salaryGradeId = Guid.NewGuid();
    private readonly Guid _contractTypePkwtId = Guid.NewGuid();
    private readonly Guid _headcountId = Guid.NewGuid();
    private readonly Guid _seatOpenId = Guid.NewGuid();
    private readonly Guid _seatFulfilledId = Guid.NewGuid();
    private readonly Guid _reqId = Guid.NewGuid();
    private readonly Guid _vacClinicalId = Guid.NewGuid();
    private readonly Guid _vacNonClinicalId = Guid.NewGuid();
    private readonly Guid _superUserId = Guid.NewGuid();
    private readonly Guid _recruiterRestrictedUserId = Guid.NewGuid();
    private readonly Guid _hrNoMcuNotesUserId = Guid.NewGuid();
    private readonly Guid _medicalDoctorUserId = Guid.NewGuid();
    private readonly Guid _clinicalAssessorUserId = Guid.NewGuid();
    private readonly Guid _otherAssessorUserId = Guid.NewGuid();
    private readonly Guid _approverUserId = Guid.NewGuid();

    public RecruitmentJourneyWorkflowTests()
    {
        using var ctx = _db.CreateContext();
        SeedBaseData(ctx);
    }

    public void Dispose() => _db.Dispose();

    private void SeedBaseData(ApplicationDbContext ctx)
    {
        ctx.MstHospitalSites.Add(new MstHospitalSite { Id = _siteId, SiteCode = "RS-UJI", SiteName = "RS Uji", IsActive = true });
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _unitId, UnitCode = "IGD", UnitName = "Instalasi Gawat Darurat", IsActive = true });
        ctx.MstPositions.Add(new MstPosition { Id = _positionId, PositionCode = "DR-UMUM", PositionName = "Dokter Umum", IsActive = true });
        ctx.MstSalaryGrades.Add(new MstSalaryGrade
        {
            Id = _salaryGradeId,
            SalaryGradeCode = "SG-MED-1",
            SalaryGradeName = "Grade Medis 1",
            CurrencyCode = "IDR",
            MinimumSalary = 8000000m,
            MidpointSalary = 10000000m,
            MaximumSalary = 12000000m,
            EffectiveStartDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsActive = true
        });
        ctx.MstContractTypes.Add(new MstContractType
        {
            Id = _contractTypePkwtId,
            ContractTypeCode = "PKWT",
            ContractTypeName = "Perjanjian Kerja Waktu Tertentu",
            RequiresEndDate = true,
            IsActive = true
        });

        ctx.HrdHeadcountRequests.Add(new HrdHeadcountRequest
        {
            Id = _headcountId,
            RequestNumber = "HCR-001",
            HospitalSiteId = _siteId,
            OrganizationUnitId = _unitId,
            PositionId = _positionId,
            SalaryGradeId = _salaryGradeId,
            RequestedQuantity = 2m,
            ExternalSeatCount = 2,
            RequestStatus = HeadcountRequestStatus.ReadyForRecruitment
        });
        ctx.HrdHeadcountRequestSeats.AddRange(
            new HrdHeadcountRequestSeat
            {
                Id = _seatOpenId,
                HeadcountRequestId = _headcountId,
                SeatNumber = 1,
                FulfilmentSource = HeadcountFulfilmentSource.External,
                SeatStatus = HeadcountSeatStatus.Open
            },
            new HrdHeadcountRequestSeat
            {
                Id = _seatFulfilledId,
                HeadcountRequestId = _headcountId,
                SeatNumber = 2,
                FulfilmentSource = HeadcountFulfilmentSource.External,
                SeatStatus = HeadcountSeatStatus.Fulfilled
            });

        ctx.HrdJobRequisitions.Add(new HrdJobRequisition
        {
            Id = _reqId,
            RequisitionNumber = "REQ-001",
            HeadcountRequestId = _headcountId,
            HospitalSiteId = _siteId,
            OrganizationUnitId = _unitId,
            PositionId = _positionId,
            RequestedSeats = 2,
            ApprovedSeats = 2,
            Status = JobRequisitionStatus.Approved
        });

        ctx.HrdJobVacancies.AddRange(
            new HrdJobVacancy
            {
                Id = _vacClinicalId,
                VacancyNumber = "VAC-CLN-001",
                RequisitionId = _reqId,
                Title = "Dokter Jaga IGD",
                QuotaSeats = 2,
                IsClinical = true,
                Status = JobVacancyStatus.Published
            },
            new HrdJobVacancy
            {
                Id = _vacNonClinicalId,
                VacancyNumber = "VAC-NCL-001",
                RequisitionId = _reqId,
                Title = "Staf Administrasi Medis",
                QuotaSeats = 2,
                IsClinical = false,
                Status = JobVacancyStatus.Published
            });

        ApplicationUser MakeUser(Guid id, UserType type, string name) => new()
        {
            Id = id,
            UserName = name,
            UserCode = name.ToUpperInvariant().PadRight(8, 'X')[..8],
            DisplayName = name,
            UserType = type,
            IsActive = true
        };

        ctx.Users.AddRange(
            MakeUser(_superUserId, UserType.SuperAdmin, "super"),
            MakeUser(_recruiterRestrictedUserId, UserType.Employee, "recres"),
            MakeUser(_hrNoMcuNotesUserId, UserType.Employee, "hrnomcu"),
            MakeUser(_medicalDoctorUserId, UserType.Employee, "k3rsdoc"),
            MakeUser(_clinicalAssessorUserId, UserType.Employee, "asesora"),
            MakeUser(_otherAssessorUserId, UserType.Employee, "asesorb"),
            MakeUser(_approverUserId, UserType.Employee, "approver")
        );

        void Grant(Guid userId, params (string Res, string Act)[] pairs)
        {
            var deptId = Guid.NewGuid();
            var posId = Guid.NewGuid();
            ctx.ApplicationUserOrganizations.Add(new ApplicationUserOrganization
            {
                UserId = userId,
                DepartmentId = deptId,
                PositionId = posId,
                IsActive = true
            });
            foreach (var (res, act) in pairs)
            {
                var c = ctx.SysControllerAccesses.Local.FirstOrDefault(x => x.ControllerName == res)
                        ?? ctx.SysControllerAccesses.FirstOrDefault(x => x.ControllerName == res);
                if (c == null)
                {
                    c = new SysControllerAccess { ModuleId = Guid.NewGuid(), ControllerName = res, DisplayName = res };
                    ctx.SysControllerAccesses.Add(c);
                }

                var a = ctx.SysActionAccesses.Local.FirstOrDefault(x => x.ControllerAccessId == c.Id && x.ActionName == act)
                        ?? ctx.SysActionAccesses.FirstOrDefault(x => x.ControllerAccessId == c.Id && x.ActionName == act);
                if (a == null)
                {
                    a = new SysActionAccess { ControllerAccessId = c.Id, ActionName = act, DisplayName = act, AccessType = "Read" };
                    ctx.SysActionAccesses.Add(a);
                }

                ctx.SysAccessPolicies.Add(new SysAccessPolicy
                {
                    DepartmentId = deptId,
                    PositionId = posId,
                    ControllerAccessId = c.Id,
                    ActionAccessId = a.Id,
                    IsAllowed = true,
                    IsActive = true
                });
            }
        }

        Grant(_recruiterRestrictedUserId, ("Candidate", "Read"), ("CandidateScreening", "Read"));
        Grant(_hrNoMcuNotesUserId,
            ("Candidate", "Read"), ("CandidateScreening", "Read"), ("CandidateEvaluation", "Read"),
            ("JobOffer", "Read"), ("OccupationalHealthClearance", "Read"), ("HiringHandoff", "Read"),
            ("WorkforceOnboardingCase", "Read"));
        Grant(_medicalDoctorUserId,
            ("Candidate", "Read"), ("OccupationalHealthClearance", "Read"), ("OccupationalHealthClearance", "Evaluate"));
        Grant(_clinicalAssessorUserId,
            ("Candidate", "Read"), ("CandidateScreening", "Read"), ("CandidateEvaluation", "Read"));

        ctx.SaveChanges();
    }

    private static ClaimsPrincipal MakePrincipal(Guid userId, string name = "Tester")
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("sub", userId.ToString()),
            new Claim("user_id", userId.ToString()),
            new Claim(ClaimTypes.Name, name)
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    private (Guid CandidateId, Guid ApplicationId) SeedApplication(
        ApplicationDbContext ctx,
        bool clinical,
        string nik = "3174010101900001",
        Guid? prevProfileId = null)
    {
        var candId = Guid.NewGuid();
        var appId = Guid.NewGuid();
        ctx.HrdCandidates.Add(new HrdCandidate
        {
            Id = candId,
            CandidateCode = "CAND-" + candId.ToString("N")[..6],
            NationalIdNumber = nik,
            FullName = "Kandidat Uji " + candId.ToString("N")[..4],
            Gender = "Laki-laki",
            BirthDate = new DateOnly(1992, 5, 10),
            BirthPlace = "Jakarta",
            Email = $"cand.{candId:N}@uji.local",
            PhoneNumber = "0812000111",
            IsRehire = prevProfileId.HasValue,
            PreviousWorkforceProfileId = prevProfileId,
            Status = CandidateStatus.Active
        });
        ctx.HrdCandidateApplications.Add(new HrdCandidateApplication
        {
            Id = appId,
            ApplicationNumber = "APP-" + appId.ToString("N")[..6],
            CandidateId = candId,
            VacancyId = clinical ? _vacClinicalId : _vacNonClinicalId,
            CurrentStage = ApplicationStage.Screening,
            StageStatus = "Registered"
        });
        ctx.SaveChanges();
        return (candId, appId);
    }

    private sealed class StubOnboardingService(ApplicationDbContext ctx, bool shouldFail = false) : IHrdOnboardingCaseService
    {
        public Task<QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs.RecruitmentResult<OnboardingCaseDetailResponse>> CreateFromHandoffAsync(
            Guid handoffId, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            if (shouldFail)
            {
                return Task.FromResult(QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs.RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable("Simulasi gagal buat kasus onboarding."));
            }

            var handoff = ctx.HrdCandidateHiringHandoffs.First(h => h.Id == handoffId);
            var existing = ctx.HrdOnboardingCases.FirstOrDefault(c => c.HiringHandoffId == handoffId && !c.IsDelete);
            if (existing != null)
            {
                return Task.FromResult(QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs.RecruitmentResult<OnboardingCaseDetailResponse>.Ok(new OnboardingCaseDetailResponse
                {
                    Id = existing.Id,
                    CaseNumber = existing.CaseNumber,
                    HiringHandoffId = existing.HiringHandoffId,
                    WorkforceProfileId = existing.WorkforceProfileId,
                    CaseStatus = existing.CaseStatus
                }));
            }

            var onb = new HrdOnboardingCase
            {
                Id = Guid.NewGuid(),
                CaseNumber = "ONB-" + Guid.NewGuid().ToString("N")[..6],
                HiringHandoffId = handoff.Id,
                WorkforceProfileId = handoff.WorkforceProfileId ?? Guid.NewGuid(),
                HospitalSiteId = Guid.NewGuid(),
                OrganizationUnitId = Guid.NewGuid(),
                PositionId = Guid.NewGuid(),
                CoordinatorUserId = actorUserId,
                CaseStatus = OnboardingCaseStatus.InProgress,
                PlannedStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
            };
            ctx.HrdOnboardingCases.Add(onb);
            ctx.SaveChanges();

            return Task.FromResult(QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs.RecruitmentResult<OnboardingCaseDetailResponse>.Ok(new OnboardingCaseDetailResponse
            {
                Id = onb.Id,
                CaseNumber = onb.CaseNumber,
                HiringHandoffId = onb.HiringHandoffId,
                WorkforceProfileId = onb.WorkforceProfileId,
                CaseStatus = onb.CaseStatus
            }));
        }

        public Task<QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs.RecruitmentResult<PagedResult<OnboardingCaseListItemResponse>>> GetPagedAsync(OnboardingCaseQuery q, ClaimsPrincipal u, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs.RecruitmentResult<OnboardingCaseSummaryResponse>> GetSummaryAsync(ClaimsPrincipal u, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs.RecruitmentResult<OnboardingCaseDetailResponse>> GetByIdAsync(Guid id, ClaimsPrincipal u, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs.RecruitmentResult<OnboardingCaseDetailResponse>> RecordFirstDayAttendanceAsync(Guid id, RecordFirstDayAttendanceRequest r, ClaimsPrincipal u, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs.RecruitmentResult<OnboardingCaseDetailResponse>> CancelFirstDayAttendanceAsync(Guid id, ClaimsPrincipal u, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs.RecruitmentResult<OnboardingCaseDetailResponse>> CompleteItemAsync(Guid c, Guid i, CompleteChecklistItemRequest r, ClaimsPrincipal u, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs.RecruitmentResult<OnboardingCaseDetailResponse>> UncompleteItemAsync(Guid c, Guid i, ClaimsPrincipal u, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs.RecruitmentResult<OnboardingCaseDetailResponse>> CheckAutomaticItemsAsync(Guid c, ClaimsPrincipal u, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs.RecruitmentResult<OnboardingCaseDetailResponse>> ReassignCoordinatorAsync(Guid c, ReassignCoordinatorRequest r, ClaimsPrincipal u, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs.RecruitmentResult<OnboardingCaseDetailResponse>> DeclareReadyForWorkAsync(Guid c, DeclareReadyForWorkRequest r, ClaimsPrincipal u, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs.RecruitmentResult<OnboardingCaseDetailResponse>> CancelJoinAsync(Guid c, CancelOnboardingJoinRequest r, ClaimsPrincipal u, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<(bool Exists, Stream? Stream, string? ContentType, string? FileName)> DownloadItemEvidenceAsync(Guid c, Guid i, CancellationToken ct = default) => throw new NotImplementedException();
    }

    [Fact]
    public void CandidateController_Journey_Endpoint_Memenuhi_Registri_Permission()
    {
        var method = typeof(CandidateController).GetMethod(nameof(CandidateController.GetApplicationJourney))!;
        var actionAttr = method.GetCustomAttribute<AccessActionAttribute>();
        var permAttr = method.GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(actionAttr);
        Assert.NotNull(permAttr);
        Assert.Equal("Candidate", (string)permAttr!.Arguments![0]);
        Assert.Equal("Read", (string)permAttr.Arguments[1]);
        Assert.Equal("Read", actionAttr!.ActionName);
    }

    [Fact]
    public async Task Scheduled_Evaluation_Belum_Submit_Dilarang_Naik_Stage_Atau_Buat_Offer()
    {
        using var ctx = _db.CreateContext();
        var (candId, appId) = SeedApplication(ctx, clinical: false);
        ctx.HrdCandidateScreenings.Add(new HrdCandidateScreening
        {
            Id = Guid.NewGuid(),
            ApplicationId = appId,
            Status = ScreeningStatus.Passed,
            ClinicalScreeningResult = ClinicalScreeningResult.NotApplicable
        });
        ctx.SaveChanges();

        var evalService = new CandidateEvaluationService(ctx);
        var offerService = new JobOfferService(ctx);
        var super = MakePrincipal(_superUserId, "Super");

        // Jadwalkan HR Interview (UpdateDateTime masih null, score=0, Recommendation default=Considered)
        var scheduled = await evalService.ScheduleAsync(new ScheduleEvaluationRequest
        {
            ApplicationId = appId,
            EvaluationType = EvaluationType.HrInterview,
            EvaluatorUserId = _superUserId,
            EvaluatorName = "HR Admin",
            EvaluatorRole = "HR",
            EvaluationDate = DateTime.UtcNow.AddDays(1)
        }, super);
        Assert.True(scheduled.Success);

        // Penjadwalan tanpa submit HR dilarang langsung membuat offer (sebelumnya lolos jika tidak ada UpdateDateTime check!)
        var premOffer = await offerService.CreateAsync(new CreateJobOfferRequest
        {
            ApplicationId = appId,
            SalaryGradeId = _salaryGradeId,
            ContractTypeId = _contractTypePkwtId,
            ContractDurationMonths = 12,
            OfferedBasicSalary = 9500000m,
            OfferedPositionTitle = "Staf Admin",
            ProposedJoinDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        }, super);

        Assert.False(premOffer.Success);
        Assert.Equal(422, premOffer.StatusCode);

        // Submit HR evaluasi → sekarang barulah stage naik ke Offering dan pembuatan offer diizinkan
        var submitted = await evalService.SubmitHrEvaluationAsync(scheduled.Data!.Id, new SubmitHrEvaluationRequest
        {
            Score = 88m,
            Recommendation = EvaluationRecommendation.Recommended,
            Notes = "Baik"
        }, super);
        Assert.True(submitted.Success);

        var appAfter = ctx.HrdCandidateApplications.AsNoTracking().First(a => a.Id == appId);
        Assert.Equal(ApplicationStage.Offering, appAfter.CurrentStage);

        var validOffer = await offerService.CreateAsync(new CreateJobOfferRequest
        {
            ApplicationId = appId,
            SalaryGradeId = _salaryGradeId,
            ContractTypeId = _contractTypePkwtId,
            ContractDurationMonths = 12,
            OfferedBasicSalary = 9500000m,
            OfferedPositionTitle = "Staf Admin",
            ProposedJoinDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        }, super);
        Assert.True(validOffer.Success);
    }

    [Fact]
    public async Task Evaluasi_Sebelum_Screening_Passed_Ditolak_422_Dan_Veto_Terisolasi_Per_Lamaran()
    {
        using var ctx = _db.CreateContext();
        var (candId, appA) = SeedApplication(ctx, clinical: true, nik: "3174010101900099");

        // Lamaran B untuk kandidat yang sama pada lowongan non-klinis
        var appB = Guid.NewGuid();
        ctx.HrdCandidateApplications.Add(new HrdCandidateApplication
        {
            Id = appB,
            ApplicationNumber = "APP-B001",
            CandidateId = candId,
            VacancyId = _vacNonClinicalId,
            CurrentStage = ApplicationStage.Offering,
            StageStatus = "EvaluationPassedReadyForOffer"
        });
        ctx.SaveChanges();

        var evalService = new CandidateEvaluationService(ctx);
        var super = MakePrincipal(_superUserId, "Super");

        // Jadwal evaluasi saat belum screening Passed → 422
        var premature = await evalService.ScheduleAsync(new ScheduleEvaluationRequest
        {
            ApplicationId = appA,
            EvaluationType = EvaluationType.ClinicalAssessment,
            EvaluatorUserId = _clinicalAssessorUserId,
            EvaluatorName = "Asesor",
            EvaluatorRole = "Dokter",
            EvaluationDate = DateTime.UtcNow
        }, super);
        Assert.False(premature.Success);
        Assert.Equal(422, premature.StatusCode);

        // Screening A lulus
        ctx.HrdCandidateScreenings.Add(new HrdCandidateScreening
        {
            Id = Guid.NewGuid(),
            ApplicationId = appA,
            Status = ScreeningStatus.Passed,
            IsStrRequired = true,
            ClinicalScreeningResult = ClinicalScreeningResult.StrValid
        });
        ctx.SaveChanges();

        var schedClin = await evalService.ScheduleAsync(new ScheduleEvaluationRequest
        {
            ApplicationId = appA,
            EvaluationType = EvaluationType.ClinicalAssessment,
            EvaluatorUserId = _clinicalAssessorUserId,
            EvaluatorName = "Asesor A",
            EvaluatorRole = "Dokter",
            EvaluationDate = DateTime.UtcNow
        }, super);
        Assert.True(schedClin.Success);

        // Penilai salah (403)
        var wrongUserRes = await evalService.SubmitClinicalEvaluationAsync(schedClin.Data!.Id, new SubmitClinicalEvaluationRequest
        {
            Score = 50m,
            Recommendation = EvaluationRecommendation.NotRecommended
        }, MakePrincipal(_otherAssessorUserId, "Wrong"));
        Assert.Equal(403, wrongUserRes.StatusCode);

        // Asesor asli menerapkan Veto Klinis pada App A
        var vetoRes = await evalService.SubmitClinicalEvaluationAsync(schedClin.Data.Id, new SubmitClinicalEvaluationRequest
        {
            Score = 40m,
            Recommendation = EvaluationRecommendation.NotRecommended,
            VetoReason = "Kompetensi tindakan gawat darurat belum memenuhi."
        }, MakePrincipal(_clinicalAssessorUserId, "Asesor A"));
        Assert.True(vetoRes.Success);

        // Lamaran A gugur permanen, tetapi kandidat punya Lamaran B aktif sehingga Candidate.Status TIDAK clobbered jadi Rejected
        var cand = ctx.HrdCandidates.AsNoTracking().First(c => c.Id == candId);
        var loadedA = ctx.HrdCandidateApplications.AsNoTracking().First(a => a.Id == appA);
        var loadedB = ctx.HrdCandidateApplications.AsNoTracking().First(a => a.Id == appB);
        Assert.Equal("ASSESSMENT_FAILED", loadedA.StageStatus);
        Assert.Equal(CandidateStatus.Active, cand.Status);
        Assert.Equal(ApplicationStage.Offering, loadedB.CurrentStage);

        // Journey App A berstatus REJECTED, sementara Journey App B berstatus ACTIVE
        var journeyService = new RecruitmentJourneyService(ctx, Pelaku.Izin(ctx));
        var jA = await journeyService.GetApplicationJourneyAsync(appA, MakePrincipal(_superUserId));
        var jB = await journeyService.GetApplicationJourneyAsync(appB, MakePrincipal(_superUserId));
        Assert.Equal(RecruitmentJourneyOutcomeStatuses.Rejected, jA.Data!.JourneyOutcome);
        Assert.Equal(RecruitmentJourneyOutcomeStatuses.Active, jB.Data!.JourneyOutcome);
    }

    [Fact]
    public async Task Offer_Mcu_Handoff_Guards_Dan_Rehire_Dedup_Atomik_Recovery()
    {
        using var ctx = _db.CreateContext();
        var oldProfileId = Guid.NewGuid();
        ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile
        {
            Id = oldProfileId,
            ProfileCode = "WFP-OLD-01",
            UserType = UserType.Employee,
            DisplayName = "Rehire Lama",
            IsActive = false
        });
        ctx.SaveChanges();

        var (_, appId) = SeedApplication(ctx, clinical: false, nik: "3174010101900088", prevProfileId: oldProfileId);
        ctx.HrdCandidateScreenings.Add(new HrdCandidateScreening
        {
            Id = Guid.NewGuid(),
            ApplicationId = appId,
            Status = ScreeningStatus.Passed,
            ClinicalScreeningResult = ClinicalScreeningResult.NotApplicable
        });
        ctx.HrdCandidateEvaluations.Add(new HrdCandidateEvaluation
        {
            Id = Guid.NewGuid(),
            ApplicationId = appId,
            EvaluationType = EvaluationType.HrInterview,
            EvaluatorUserId = _superUserId,
            EvaluatorName = "HR",
            EvaluatorRole = "HR",
            EvaluationDate = DateTime.UtcNow,
            Score = 90m,
            Recommendation = EvaluationRecommendation.Recommended,
            UpdateDateTime = DateTime.UtcNow
        });
        ctx.SaveChanges();

        var super = MakePrincipal(_superUserId, "Super");
        var approver = MakePrincipal(_approverUserId, "Approver");
        var hrCreator = MakePrincipal(_recruiterRestrictedUserId, "Creator");

        var offerSvc = new JobOfferService(ctx);
        var approvalSvc = new JobOfferApprovalService(ctx);
        var mcuSvc = new McuClearanceService(ctx, Pelaku.Izin(ctx));

        var offerRes = await offerSvc.CreateAsync(new CreateJobOfferRequest
        {
            ApplicationId = appId,
            SalaryGradeId = _salaryGradeId,
            ContractTypeId = _contractTypePkwtId,
            ContractDurationMonths = 12,
            OfferedBasicSalary = 9500000m,
            OfferedPositionTitle = "Staf Medis",
            ProposedJoinDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        }, hrCreator);
        Assert.True(offerRes.Success);
        var offerId = offerRes.Data!.Id;

        // Self-approval ditolak (403)
        var selfApprove = await approvalSvc.ApproveOfferStepAsync(offerId, new ApproveJobOfferRequest(), hrCreator);
        Assert.Equal(403, selfApprove.StatusCode);

        // Setujui seluruh step T13 (2 step) oleh approver berbeda
        Assert.True((await approvalSvc.ApproveOfferStepAsync(offerId, new ApproveJobOfferRequest(), approver)).Success);
        Assert.True((await approvalSvc.ApproveOfferStepAsync(offerId, new ApproveJobOfferRequest(), approver)).Success);

        // Bug fix: Approved tapi belum SentConditional → CandidateRespond MUST FAIL 422!
        var prematureResp = await offerSvc.CandidateRespondAsync(offerId, new CandidateOfferResponseRequest
        {
            ResponseStatus = JobOfferStatus.ConditionalAccepted
        }, super);
        Assert.Equal(422, prematureResp.StatusCode);

        // Kirim offer resmi lalu coba bypass RegisterMcu saat SentConditional → HRD-DEC-078 MUST FAIL 422!
        Assert.True((await offerSvc.SendConditionalOfferAsync(offerId, new SendConditionalOfferRequest(), super)).Success);
        var prematureMcu = await mcuSvc.RegisterMcuAsync(new RegisterMcuRequest
        {
            OfferId = offerId,
            ClinicOrFacility = "RS Klinik Uji"
        }, super);
        Assert.Equal(422, prematureMcu.StatusCode);

        // Bug fix: candidate respond tidak boleh langsung loncat ke FinalAccepted dari SentConditional!
        var jumpFinal = await offerSvc.CandidateRespondAsync(offerId, new CandidateOfferResponseRequest
        {
            ResponseStatus = JobOfferStatus.FinalAccepted
        }, super);
        Assert.Equal(422, jumpFinal.StatusCode);

        // Respons ConditionalAccepted → Register MCU → FitWithRestrictions tidak langsung FinalAccepted & tahan handoff!
        Assert.True((await offerSvc.CandidateRespondAsync(offerId, new CandidateOfferResponseRequest
        {
            ResponseStatus = JobOfferStatus.ConditionalAccepted
        }, super)).Success);

        var mcuReg = await mcuSvc.RegisterMcuAsync(new RegisterMcuRequest
        {
            OfferId = offerId,
            ClinicOrFacility = "Klinik K3RS"
        }, super);
        Assert.True(mcuReg.Success);

        var fwrRes = await mcuSvc.RecordResultAsync(mcuReg.Data!.Id, new RecordMcuResultRequest
        {
            ClearanceResult = McuClearanceResult.FitWithRestrictions,
            ExaminingDoctorName = "dr. K3RS",
            CheckDate = DateTime.UtcNow,
            DoctorNotes = "Hindari shift malam 3 bulan",
            CertificateFilePath = "/mcu/cert-1.pdf"
        }, MakePrincipal(_medicalDoctorUserId, "K3RS"));
        Assert.True(fwrRes.Success);

        var handoffFailSvc = new HiringHandoffService(ctx, new StubOnboardingService(ctx, shouldFail: false));
        var handoffBlockedByFwr = await handoffFailSvc.GenerateHandoffAsync(new GenerateHiringHandoffRequest
        {
            OfferId = offerId,
            HeadcountRequestSeatId = _seatOpenId
        }, super);
        Assert.Equal(422, handoffBlockedByFwr.StatusCode);

        // Catat ulang hasil MCU menjadi Fit penuh
        var fitRes = await mcuSvc.RecordResultAsync(mcuReg.Data.Id, new RecordMcuResultRequest
        {
            ClearanceResult = McuClearanceResult.Fit,
            ExaminingDoctorName = "dr. K3RS",
            CheckDate = DateTime.UtcNow,
            DoctorNotes = "RAHASIA_KLINIS_DIAGNOSA",
            CertificateFilePath = "/mcu/cert-fit.pdf"
        }, MakePrincipal(_medicalDoctorUserId, "K3RS"));
        Assert.True(fitRes.Success);

        // Privasi medis: HR tanpa OccupationalHealthClearance:Evaluate → DoctorNotes & CertificateFilePath termasking
        var journeySvc = new RecruitmentJourneyService(ctx, Pelaku.Izin(ctx));
        var jHr = await journeySvc.GetApplicationJourneyAsync(appId, MakePrincipal(_hrNoMcuNotesUserId, "HR"));
        var jDoc = await journeySvc.GetApplicationJourneyAsync(appId, MakePrincipal(_medicalDoctorUserId, "Doc"));
        var mcuHrDetail = jHr.Data!.Stages.Single(s => s.StageKey == RecruitmentJourneyStageKeys.PreEmploymentMcu).McuDetail!;
        var mcuDocDetail = jDoc.Data!.Stages.Single(s => s.StageKey == RecruitmentJourneyStageKeys.PreEmploymentMcu).McuDetail!;
        Assert.True(mcuHrDetail.IsMedicalDetailsMasked);
        Assert.Null(mcuHrDetail.DoctorNotes);
        Assert.Null(mcuHrDetail.CertificateFilePath);
        Assert.False(mcuDocDetail.IsMedicalDetailsMasked);
        Assert.Equal("RAHASIA_KLINIS_DIAGNOSA", mcuDocDetail.DoctorNotes);

        // Kursi formasi sudah Fulfilled → 409 Conflict
        var badSeat = await handoffFailSvc.GenerateHandoffAsync(new GenerateHiringHandoffRequest
        {
            OfferId = offerId,
            HeadcountRequestSeatId = _seatFulfilledId
        }, super);
        Assert.Equal(409, badSeat.StatusCode);

        // Eksekusi Handoff sukses dengan Seat Open → rehire profile LAMA harus dipakai ulang (jumlah profile TIDAK bertambah)
        var profilesBefore = ctx.MstWorkforceProfiles.Count();
        var okHandoff = await handoffFailSvc.GenerateHandoffAsync(new GenerateHiringHandoffRequest
        {
            OfferId = offerId,
            HeadcountRequestSeatId = _seatOpenId
        }, super);
        Assert.True(okHandoff.Success);
        Assert.Equal(oldProfileId, okHandoff.Data!.WorkforceProfileId);
        Assert.Equal(profilesBefore, ctx.MstWorkforceProfiles.Count());
        Assert.Equal(HeadcountSeatStatus.Fulfilled, ctx.HrdHeadcountRequestSeats.AsNoTracking().First(s => s.Id == _seatOpenId).SeatStatus);

        // Partial recovery test: simulasi HrdOnboardingCase terhapus / belum terbuat lalu Generate lagi memulihkan case tanpa duplikasi!
        var existingOnb = ctx.HrdOnboardingCases.First(c => c.HiringHandoffId == okHandoff.Data.Id);
        ctx.HrdOnboardingCases.Remove(existingOnb);
        ctx.SaveChanges();

        var jPartial = await journeySvc.GetApplicationJourneyAsync(appId, super);
        var stage6Partial = jPartial.Data!.Stages.Single(s => s.StageKey == RecruitmentJourneyStageKeys.HandoffOnboarding);
        Assert.Equal(RecruitmentJourneyStageStatuses.Blocked, stage6Partial.Status);
        Assert.True(stage6Partial.HandoffDetail!.NeedsOnboardingRecovery);

        var recoveredHandoff = await handoffFailSvc.GenerateHandoffAsync(new GenerateHiringHandoffRequest
        {
            OfferId = offerId,
            HeadcountRequestSeatId = _seatOpenId
        }, super);
        Assert.True(recoveredHandoff.Success);
        Assert.Equal(1, ctx.HrdOnboardingCases.Count(c => c.HiringHandoffId == okHandoff.Data.Id && !c.IsDelete));
        Assert.Equal(profilesBefore, ctx.MstWorkforceProfiles.Count());
    }

    [Fact]
    public async Task Restricted_Viewer_Menampilkan_Restricted_Stage_Dan_Tanpa_Kebocoran_Data()
    {
        using var ctx = _db.CreateContext();
        var (_, appId) = SeedApplication(ctx, clinical: true);
        ctx.HrdCandidateScreenings.Add(new HrdCandidateScreening
        {
            Id = Guid.NewGuid(),
            ApplicationId = appId,
            Status = ScreeningStatus.Passed,
            ClinicalScreeningResult = ClinicalScreeningResult.StrValid,
            StrNumber = "STR-SECRET-99"
        });
        ctx.HrdCandidateEvaluations.Add(new HrdCandidateEvaluation
        {
            Id = Guid.NewGuid(),
            ApplicationId = appId,
            EvaluationType = EvaluationType.ClinicalAssessment,
            EvaluatorUserId = _otherAssessorUserId,
            EvaluatorName = "Asesor Lain",
            EvaluatorRole = "Dokter",
            EvaluationDate = DateTime.UtcNow,
            Score = 85m,
            Recommendation = EvaluationRecommendation.Recommended,
            UpdateDateTime = DateTime.UtcNow
        });
        ctx.SaveChanges();

        var svc = new RecruitmentJourneyService(ctx, Pelaku.Izin(ctx));

        // Recruiter restricted hanya punya Candidate:Read & CandidateScreening:Read → Stage 3..6 RESTRICTED
        var restrictedRes = await svc.GetApplicationJourneyAsync(appId, MakePrincipal(_recruiterRestrictedUserId, "Restricted"));
        Assert.True(restrictedRes.Success);
        var data = restrictedRes.Data!;
        Assert.Equal("RESTRICTED", data.DataCompleteness.CompletenessStatus);
        Assert.Equal(6, data.Stages.Count);
        Assert.Equal(RecruitmentJourneyStageStatuses.Completed, data.Stages[1].Status);
        Assert.Equal(RecruitmentJourneyStageStatuses.Restricted, data.Stages[2].Status);
        Assert.Null(data.Stages[2].EvaluationDetail);
        Assert.Equal(RecruitmentJourneyStageStatuses.Restricted, data.Stages[3].Status);
        Assert.Equal(RecruitmentJourneyStageStatuses.Restricted, data.Stages[4].Status);
        Assert.Equal(RecruitmentJourneyStageStatuses.Restricted, data.Stages[5].Status);
        Assert.DoesNotContain("STR-SECRET-99", data.CandidateSummary.MaskedNationalIdNumber);

        // Clinical assessor tanpa ViewAll melihat Stage 3 tetapi sesi penilai lain tersembunyi (VR-PKB-07)
        var assessorRes = await svc.GetApplicationJourneyAsync(appId, MakePrincipal(_clinicalAssessorUserId, "Assessor"));
        Assert.True(assessorRes.Success);
        var evalStage = assessorRes.Data!.Stages.Single(s => s.StageKey == RecruitmentJourneyStageKeys.Evaluation);
        Assert.True(evalStage.EvaluationDetail!.HasHiddenOtherEvaluatorSessions);
        Assert.Empty(evalStage.EvaluationDetail.Sessions);
    }

    [Fact]
    public async Task ApplyVacancy_AutoCreates_Pending_Screening_Dan_Anti_Duplikasi_Serta_Reconcile_Idempoten()
    {
        using var ctx = _db.CreateContext();
        var super = MakePrincipal(_superUserId, "Super");
        var allocator = new NumberSeriesAllocator(new JourneyDbFactory(_db));
        var storage = new HrAttachmentStorage(null!, new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        var candSvc = new CandidateService(ctx, allocator, storage);
        var screeningSvc = new CandidateScreeningService(ctx);

        // Register profil saja -> tidak membuat lamaran atau screening
        var reg = await candSvc.RegisterAsync(new RegisterCandidateRequest
        {
            NationalIdNumber = "3174010101900777",
            FullName = "Kandidat Auto Screening",
            Gender = "Perempuan",
            BirthDate = new DateOnly(1996, 3, 15),
            BirthPlace = "Bandung",
            Email = "auto.screen@uji.local",
            PhoneNumber = "0812333444"
        }, super);
        Assert.True(reg.Success, reg.Message);
        Assert.Equal(0, ctx.HrdCandidateScreenings.Count(s => !s.IsDelete));

        // Apply lowongan klinis -> atomik menghasilkan 1 application + 1 screening Pending (IsStrRequired=true)
        var applyClin = await candSvc.ApplyVacancyAsync(reg.Data!.Id, new ApplyVacancyRequest
        {
            VacancyId = _vacClinicalId,
            Notes = "Lamaran IGD"
        }, super);
        Assert.True(applyClin.Success, applyClin.Message);

        var createdScreening = await screeningSvc.GetByApplicationIdAsync(applyClin.Data!.Id);
        Assert.True(createdScreening.Success, createdScreening.Message);
        Assert.Equal(ScreeningStatus.Pending, createdScreening.Data!.Status);
        Assert.True(createdScreening.Data.IsStrRequired);
        Assert.Equal("Menunggu Pemeriksaan STR", createdScreening.Data.ClinicalScreeningResultLabel);

        // Double-click / apply ulang ke lowongan yang sama -> 409 Conflict, tidak ada duplikasi screening
        var dupApply = await candSvc.ApplyVacancyAsync(reg.Data.Id, new ApplyVacancyRequest
        {
            VacancyId = _vacClinicalId
        }, super);
        Assert.Equal(409, dupApply.StatusCode);
        Assert.Equal(1, ctx.HrdCandidateScreenings.Count(s => s.ApplicationId == applyClin.Data.Id && !s.IsDelete));

        // Seed lamaran lama (Screening/Submitted) tanpa screening + lamaran FastTrack (Offering) -> reconcile hanya buat 1
        var (_, legacyAppId) = SeedApplication(ctx, clinical: false, nik: "3174010101900778");
        var legacyApp = ctx.HrdCandidateApplications.First(a => a.Id == legacyAppId);
        legacyApp.StageStatus = "Submitted";
        var (_, ftAppId) = SeedApplication(ctx, clinical: false, nik: "3174010101900779");
        var ftApp = ctx.HrdCandidateApplications.First(a => a.Id == ftAppId);
        ftApp.CurrentStage = ApplicationStage.Offering;
        ftApp.StageStatus = "FastTrackPromotedFromTalentPool";
        ctx.SaveChanges();

        var preview = await screeningSvc.GetReconcilePreviewAsync();
        Assert.True(preview.Success);
        Assert.Equal(1, preview.Data!.EligibleCount);
        Assert.Equal(legacyAppId, preview.Data.EligibleItems[0].ApplicationId);
        Assert.Contains(preview.Data.SkippedItems, s => s.ApplicationId == ftAppId);

        var reconcile = await screeningSvc.ReconcileMissingScreeningsAsync(super);
        Assert.True(reconcile.Success, reconcile.Message);
        Assert.Equal(1, reconcile.Data!.CreatedCount);

        // Rerun idempoten -> 0 baru
        var rerun = await screeningSvc.ReconcileMissingScreeningsAsync(super);
        Assert.True(rerun.Success);
        Assert.Equal(0, rerun.Data!.CreatedCount);
    }

    [Fact]
    public async Task Screening_HardGate_Str_Dan_Dokumen_Wajib_Umum_Serta_Eligible_Worklist()
    {
        using var ctx = _db.CreateContext();
        var super = MakePrincipal(_superUserId, "Super");
        var allocator = new NumberSeriesAllocator(new JourneyDbFactory(_db));
        var storage = new HrAttachmentStorage(null!, new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        var candSvc = new CandidateService(ctx, allocator, storage);
        var screeningSvc = new CandidateScreeningService(ctx);
        var evalSvc = new CandidateEvaluationService(ctx);

        var (candId, appId) = SeedApplication(ctx, clinical: true, nik: "3174010101900888");
        var app = ctx.HrdCandidateApplications.First(a => a.Id == appId);
        app.StageStatus = "Submitted";
        ctx.SaveChanges();
        var initRes = await screeningSvc.InitiateScreeningAsync(new InitiateScreeningRequest
        {
            ApplicationId = appId,
            IsStrRequired = true
        }, super);
        Assert.True(initRes.Success);

        // Hanya punya STR valid tanpa KTP/CV/Ijazah -> Decision=Passed wajib ditolak 422
        ctx.HrdCandidateDocuments.Add(new HrdCandidateDocument
        {
            Id = Guid.NewGuid(),
            CandidateId = candId,
            ApplicationId = appId,
            DocumentType = DocumentType.StrCertificate,
            DocumentTitle = "STR Aktif",
            DocumentNumber = "STR-001",
            FilePath = "recruitment-candidate-document/2026/11/str.pdf",
            MimeType = "application/pdf",
            ValidUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))
        });
        ctx.SaveChanges();

        var missingDocsFail = await screeningSvc.EvaluateScreeningAsync(new EvaluateScreeningRequest
        {
            ApplicationId = appId,
            Decision = ScreeningStatus.Passed
        }, super);
        Assert.False(missingDocsFail.Success);
        Assert.Equal(422, missingDocsFail.StatusCode);

        // Simpan catatan kelengkapan (Decision = Pending) -> tetap Pending
        var draftSave = await screeningSvc.EvaluateScreeningAsync(new EvaluateScreeningRequest
        {
            ApplicationId = appId,
            Decision = ScreeningStatus.Pending,
            Notes = "Menunggu unggah KTP, CV, dan Ijazah."
        }, super);
        Assert.True(draftSave.Success);
        Assert.Equal(ScreeningStatus.Pending, draftSave.Data!.Status);

        // Lengkapi KTP, CV, Ijazah -> sekarang lulus dan langsung muncul di eligible-applications evaluasi meski 0 sesi
        ctx.HrdCandidateDocuments.AddRange(
            new HrdCandidateDocument { Id = Guid.NewGuid(), CandidateId = candId, ApplicationId = appId, DocumentType = DocumentType.IdentityCard, DocumentTitle = "KTP", FilePath = "ktp.pdf", MimeType = "application/pdf" },
            new HrdCandidateDocument { Id = Guid.NewGuid(), CandidateId = candId, ApplicationId = appId, DocumentType = DocumentType.CurriculumVitae, DocumentTitle = "CV", FilePath = "cv.pdf", MimeType = "application/pdf" },
            new HrdCandidateDocument { Id = Guid.NewGuid(), CandidateId = candId, ApplicationId = appId, DocumentType = DocumentType.DiplomaCertificate, DocumentTitle = "Ijazah", FilePath = "ijazah.pdf", MimeType = "application/pdf" }
        );
        ctx.SaveChanges();

        var passed = await screeningSvc.EvaluateScreeningAsync(new EvaluateScreeningRequest
        {
            ApplicationId = appId,
            Decision = ScreeningStatus.Passed,
            Notes = "Lengkap & sesuai"
        }, super);
        Assert.True(passed.Success, passed.Message);
        Assert.Equal(ScreeningStatus.Passed, passed.Data!.Status);
        Assert.Equal(ClinicalScreeningResult.StrValid, passed.Data.ClinicalScreeningResult);

        var eligibleEvals = await evalSvc.GetEligibleApplicationsAsync(new EvaluationEligibleQuery { PendingOnly = true }, super, canViewAll: true);
        Assert.True(eligibleEvals.Success);
        var row = Assert.Single(eligibleEvals.Data!.Items, x => x.ApplicationId == appId);
        Assert.False(row.HasHrSession);
        Assert.False(row.HasClinicalSession);

        // Pastikan Journey API menghasilkan route /offers/ & /handoffs/ yang bersih
        var journeySvc = new RecruitmentJourneyService(ctx, Pelaku.Izin(ctx));
        var j = await journeySvc.GetApplicationJourneyAsync(appId, super);
        Assert.True(j.Success);
        Assert.Equal(6, j.Data!.Stages.Count);
    }

    [Fact]
    public async Task TalentPool_Eligibility_Dan_FastTrack_Clinical_Str_Guards()
    {
        using var ctx = _db.CreateContext();
        var super = MakePrincipal(_superUserId, "Super");
        var poolSvc = new TalentPoolService(ctx);

        // Kasus 1: kandidat veto klinis tidak boleh masuk Talent Pool
        var (candVetoId, appVetoId) = SeedApplication(ctx, clinical: true, nik: "3174010101900901");
        ctx.HrdCandidateScreenings.Add(new HrdCandidateScreening { Id = Guid.NewGuid(), ApplicationId = appVetoId, Status = ScreeningStatus.Passed, ClinicalScreeningResult = ClinicalScreeningResult.StrValid });
        ctx.HrdCandidateEvaluations.Add(new HrdCandidateEvaluation { Id = Guid.NewGuid(), ApplicationId = appVetoId, EvaluationType = EvaluationType.HrInterview, EvaluatorUserId = _superUserId, Score = 80m, Recommendation = EvaluationRecommendation.Recommended, UpdateDateTime = DateTime.UtcNow });
        ctx.HrdCandidateEvaluations.Add(new HrdCandidateEvaluation { Id = Guid.NewGuid(), ApplicationId = appVetoId, EvaluationType = EvaluationType.ClinicalAssessment, EvaluatorUserId = _clinicalAssessorUserId, Score = 45m, Recommendation = EvaluationRecommendation.NotRecommended, IsClinicalVetoApplied = true, VetoReason = "Gagal asesmen", UpdateDateTime = DateTime.UtcNow });
        ctx.SaveChanges();

        var vetoEnroll = await poolSvc.EnrollAsync(new EnrollTalentPoolRequest
        {
            CandidateId = candVetoId,
            OriginalVacancyId = _vacClinicalId,
            QualificationSummary = "Test veto",
            RankingScore = 80m,
            RankingPosition = 1
        }, super);
        Assert.Equal(422, vetoEnroll.StatusCode);

        // Kasus 2: download sertifikat MCU membutuhkan hak Evaluate, bukan hanya Read
        var downloadMethod = typeof(McuClearanceController).GetMethod(nameof(McuClearanceController.DownloadHrAttachment))!;
        var permAttr = downloadMethod.GetCustomAttribute<AccessPermissionAttribute>();
        Assert.Equal("OccupationalHealthClearance", (string)permAttr!.Arguments![0]);
        Assert.Equal("Evaluate", (string)permAttr.Arguments[1]);
    }

    private sealed class JourneyDbFactory(JourneyTestDb db) : IDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext() => db.CreateContext();
    }

    private sealed class JourneySqliteModelCustomizer(ModelCustomizerDependencies dependencies) : RelationalModelCustomizer(dependencies)
    {
        private readonly ModelUjiSqlite _inner = new(dependencies);

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            _inner.Customize(modelBuilder, context);
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var constraintName in entityType.GetDeclaredCheckConstraints()
                             .Where(x => x.Sql is not null && x.Sql.Contains("btrim", StringComparison.OrdinalIgnoreCase))
                             .Select(x => x.ModelName)
                             .ToList())
                {
                    entityType.RemoveCheckConstraint(constraintName);
                }
            }
        }
    }

    private sealed class JourneyTestDb : IDisposable
    {
        private readonly SqliteConnection _connection;
        private JourneyTestDb(SqliteConnection connection) => _connection = connection;

        public static JourneyTestDb Create()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            connection.CreateFunction<string?, string?>("btrim", s => s?.Trim(), isDeterministic: true);
            var db = new JourneyTestDb(connection);
            using (var ctx = db.CreateContext()) ctx.Database.EnsureCreated();
            using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = OFF;";
                pragma.ExecuteNonQuery();
            }
            return db;
        }

        public ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .ReplaceService<IModelCustomizer, JourneySqliteModelCustomizer>()
                .Options;
            return new ApplicationDbContext(options);
        }

        public void Dispose() => _connection.Dispose();
    }
}
