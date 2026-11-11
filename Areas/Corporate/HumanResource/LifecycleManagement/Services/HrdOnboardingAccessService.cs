using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class HrdOnboardingAccessService : IHrdOnboardingAccessService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmployeeUserAccountService _userAccountService;

        public HrdOnboardingAccessService(
            ApplicationDbContext dbContext,
            IEmployeeUserAccountService userAccountService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _userAccountService = userAccountService ?? throw new ArgumentNullException(nameof(userAccountService));
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var guid) ? guid : Guid.Empty;
        }

        public async Task<RecruitmentResult<PagedResult<OnboardingAccessListItemResponse>>> GetPagedAsync(
            OnboardingAccessQuery query,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            query ??= new OnboardingAccessQuery();
            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? 10 : (query.PageSize > 100 ? 100 : query.PageSize);

            var dbQuery = _dbContext.HrdOnboardingAccessRequests
                .Include(r => r.OnboardingCase)
                .Include(r => r.WorkforceProfile)
                .Include(r => r.CompletedByUser)
                .AsNoTracking()
                .AsQueryable();

            if (query.RequestType.HasValue)
            {
                dbQuery = dbQuery.Where(r => r.RequestType == query.RequestType.Value);
            }

            if (query.RequestStatus.HasValue)
            {
                dbQuery = dbQuery.Where(r => r.RequestStatus == query.RequestStatus.Value);
            }

            if (query.HospitalSiteId.HasValue)
            {
                dbQuery = dbQuery.Where(r => r.OnboardingCase != null && r.OnboardingCase.HospitalSiteId == query.HospitalSiteId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(r =>
                    (r.OnboardingCase != null && r.OnboardingCase.CaseNumber.ToLower().Contains(s)) ||
                    (r.WorkforceProfile != null && (
                        r.WorkforceProfile.ProfileCode.ToLower().Contains(s) ||
                        r.WorkforceProfile.DisplayName.ToLower().Contains(s)
                    )));
            }

            var totalCount = await dbQuery.CountAsync(cancellationToken);

            var items = await dbQuery
                .OrderByDescending(r => r.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new OnboardingAccessListItemResponse
                {
                    Id = r.Id,
                    OnboardingCaseId = r.OnboardingCaseId,
                    CaseNumber = r.OnboardingCase != null ? r.OnboardingCase.CaseNumber : string.Empty,
                    WorkforceProfileId = r.WorkforceProfileId,
                    WorkforceCode = r.WorkforceProfile != null ? r.WorkforceProfile.ProfileCode : string.Empty,
                    FullName = r.WorkforceProfile != null ? r.WorkforceProfile.DisplayName : string.Empty,
                    HospitalSiteId = r.OnboardingCase != null ? r.OnboardingCase.HospitalSiteId : Guid.Empty,
                    HospitalSiteName = r.OnboardingCase != null && r.OnboardingCase.HospitalSite != null ? r.OnboardingCase.HospitalSite.SiteName : string.Empty,
                    OrganizationUnitId = r.OnboardingCase != null ? r.OnboardingCase.OrganizationUnitId : Guid.Empty,
                    OrganizationUnitName = r.OnboardingCase != null && r.OnboardingCase.OrganizationUnit != null ? r.OnboardingCase.OrganizationUnit.UnitName : string.Empty,
                    PositionId = r.OnboardingCase != null ? r.OnboardingCase.PositionId : Guid.Empty,
                    PositionTitle = r.OnboardingCase != null && r.OnboardingCase.Position != null ? r.OnboardingCase.Position.PositionName : string.Empty,
                    RequestType = r.RequestType,
                    RequestStatus = r.RequestStatus,
                    ApplicationUserId = r.ApplicationUserId,
                    FailureMessage = r.FailureMessage,
                    SelfServiceActivatedAt = r.SelfServiceActivatedAt,
                    RequestedAt = r.RequestedAt,
                    CompletedAt = r.CompletedAt,
                    CompletedByUserId = r.CompletedByUserId,
                    CompletedByUserName = r.CompletedByUser != null ? r.CompletedByUser.DisplayName ?? r.CompletedByUser.UserName : null,
                    CompletionNote = r.CompletionNote
                })
                .ToListAsync(cancellationToken);

            var pagedResult = new PagedResult<OnboardingAccessListItemResponse>
            {
                Items = items,
                TotalData = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPage = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return RecruitmentResult<PagedResult<OnboardingAccessListItemResponse>>.Ok(pagedResult);
        }

        public async Task<RecruitmentResult<OnboardingAccessListItemResponse>> GetByIdAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var r = await _dbContext.HrdOnboardingAccessRequests
                .Include(x => x.OnboardingCase)
                    .ThenInclude(c => c!.HospitalSite)
                .Include(x => x.OnboardingCase)
                    .ThenInclude(c => c!.OrganizationUnit)
                .Include(x => x.OnboardingCase)
                    .ThenInclude(c => c!.Position)
                .Include(x => x.WorkforceProfile)
                .Include(x => x.CompletedByUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (r == null)
            {
                return RecruitmentResult<OnboardingAccessListItemResponse>.NotFound("Permintaan akses onboarding tidak ditemukan.");
            }

            var response = MapToResponse(r);
            return RecruitmentResult<OnboardingAccessListItemResponse>.Ok(response);
        }

        public async Task<RecruitmentResult<OnboardingAccessListItemResponse>> MarkCompletedAsync(
            Guid id,
            CompleteAccessRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actorId = GetUserId(user);

            var accessRequest = await _dbContext.HrdOnboardingAccessRequests
                .Include(x => x.OnboardingCase)
                    .ThenInclude(c => c!.HospitalSite)
                .Include(x => x.OnboardingCase)
                    .ThenInclude(c => c!.OrganizationUnit)
                .Include(x => x.OnboardingCase)
                    .ThenInclude(c => c!.Position)
                .Include(x => x.WorkforceProfile)
                .Include(x => x.CompletedByUser)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (accessRequest == null)
            {
                return RecruitmentResult<OnboardingAccessListItemResponse>.NotFound("Permintaan akses onboarding tidak ditemukan.");
            }

            // VR-ONB-76: Tandai selesai hanya untuk WorkAccess berstatus Requested
            if (accessRequest.RequestType != OnboardingAccessRequestType.WorkAccess)
            {
                return RecruitmentResult<OnboardingAccessListItemResponse>.BadRequest(
                    "VR-ONB-76: Hanya permintaan tiket akses kerja (WorkAccess) yang dapat ditandai selesai.");
            }

            if (accessRequest.RequestStatus != OnboardingAccessRequestStatus.Requested)
            {
                return RecruitmentResult<OnboardingAccessListItemResponse>.BadRequest(
                    $"VR-ONB-76: Permintaan akses kerja tidak dapat diselesaikan karena status saat ini adalah {accessRequest.RequestStatus}. Hanya status Requested yang diperbolehkan.");
            }

            var note = request?.CompletionNote?.Trim();
            accessRequest.RequestStatus = OnboardingAccessRequestStatus.Completed;
            accessRequest.CompletedAt = DateTime.UtcNow;
            accessRequest.CompletedByUserId = actorId != Guid.Empty ? actorId : null;
            accessRequest.CompletionNote = string.IsNullOrWhiteSpace(note) ? "Akses kerja telah selesai diberikan oleh IT/Administrator." : note;
            accessRequest.UpdateDateTime = DateTime.UtcNow;
            accessRequest.UpdateBy = actorId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return RecruitmentResult<OnboardingAccessListItemResponse>.Ok(MapToResponse(accessRequest));
        }

        public async Task<RecruitmentResult<OnboardingAccessListItemResponse>> RetryAccountCreationAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actorId = GetUserId(user);

            var accessRequest = await _dbContext.HrdOnboardingAccessRequests
                .Include(x => x.OnboardingCase)
                    .ThenInclude(c => c!.HospitalSite)
                .Include(x => x.OnboardingCase)
                    .ThenInclude(c => c!.OrganizationUnit)
                .Include(x => x.OnboardingCase)
                    .ThenInclude(c => c!.Position)
                .Include(x => x.WorkforceProfile)
                .Include(x => x.CompletedByUser)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (accessRequest == null)
            {
                return RecruitmentResult<OnboardingAccessListItemResponse>.NotFound("Permintaan akses onboarding tidak ditemukan.");
            }

            // VR-ONB-77: Ulang hanya untuk AccountCreation berstatus Failed
            if (accessRequest.RequestType != OnboardingAccessRequestType.AccountCreation)
            {
                return RecruitmentResult<OnboardingAccessListItemResponse>.BadRequest(
                    "VR-ONB-77: Hanya permintaan pembuatan akun (AccountCreation) yang dapat diulang.");
            }

            if (accessRequest.RequestStatus != OnboardingAccessRequestStatus.Failed)
            {
                return RecruitmentResult<OnboardingAccessListItemResponse>.BadRequest(
                    $"VR-ONB-77: Pembuatan akun hanya dapat diulang jika berstatus Failed. Status saat ini: {accessRequest.RequestStatus}.");
            }

            var profile = accessRequest.WorkforceProfile ?? await _dbContext.MstWorkforceProfiles
                .FirstOrDefaultAsync(p => p.Id == accessRequest.WorkforceProfileId, cancellationToken);

            if (profile == null || string.IsNullOrWhiteSpace(profile.Email))
            {
                return RecruitmentResult<OnboardingAccessListItemResponse>.BadRequest(
                    "VR-ONB-77: Profil pegawai belum memiliki alamat email yang valid. Lengkapi email profil terlebih dahulu sebelum mengulang pembuatan akun (AT-HRD-ONB-26).");
            }

            var accountResult = await _userAccountService.CreateForWorkforceProfileAsync(
                profile.Id,
                actorId,
                isFingerprintRegistrationEnabled: false,
                fingerprintRegistrationReason: null,
                cancellationToken: cancellationToken);

            if (accountResult.Success && accountResult.User != null)
            {
                accessRequest.RequestStatus = OnboardingAccessRequestStatus.Completed;
                accessRequest.ApplicationUserId = accountResult.User.Id;
                accessRequest.FailureMessage = null;
                accessRequest.CompletedAt = DateTime.UtcNow;
                accessRequest.CompletedByUserId = actorId != Guid.Empty ? actorId : null;
                accessRequest.CompletionNote = "Akun portal berhasil dibuat ulang.";
                accessRequest.UpdateDateTime = DateTime.UtcNow;
                accessRequest.UpdateBy = actorId;

                // Jika onboarding case sudah hadir hari pertama, aktifkan akun portal
                if (accessRequest.OnboardingCase?.FirstDayAttendedOn != null)
                {
                    await _userAccountService.ActivateAsync(accountResult.User.Id, actorId, cancellationToken);
                    accessRequest.SelfServiceActivatedAt = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                return RecruitmentResult<OnboardingAccessListItemResponse>.Ok(MapToResponse(accessRequest));
            }
            else
            {
                accessRequest.FailureMessage = accountResult.Message;
                accessRequest.UpdateDateTime = DateTime.UtcNow;
                accessRequest.UpdateBy = actorId;

                await _dbContext.SaveChangesAsync(cancellationToken);

                return RecruitmentResult<OnboardingAccessListItemResponse>.BadRequest(
                    $"Gagal membuat ulang akun pengguna: {accountResult.Message}");
            }
        }

        private static OnboardingAccessListItemResponse MapToResponse(HrdOnboardingAccessRequest r)
        {
            return new OnboardingAccessListItemResponse
            {
                Id = r.Id,
                OnboardingCaseId = r.OnboardingCaseId,
                CaseNumber = r.OnboardingCase?.CaseNumber ?? string.Empty,
                WorkforceProfileId = r.WorkforceProfileId,
                WorkforceCode = r.WorkforceProfile?.ProfileCode ?? string.Empty,
                FullName = r.WorkforceProfile?.DisplayName ?? string.Empty,
                HospitalSiteId = r.OnboardingCase?.HospitalSiteId ?? Guid.Empty,
                HospitalSiteName = r.OnboardingCase?.HospitalSite?.SiteName ?? string.Empty,
                OrganizationUnitId = r.OnboardingCase?.OrganizationUnitId ?? Guid.Empty,
                OrganizationUnitName = r.OnboardingCase?.OrganizationUnit?.UnitName ?? string.Empty,
                PositionId = r.OnboardingCase?.PositionId ?? Guid.Empty,
                PositionTitle = r.OnboardingCase?.Position?.PositionName ?? string.Empty,
                RequestType = r.RequestType,
                RequestStatus = r.RequestStatus,
                ApplicationUserId = r.ApplicationUserId,
                FailureMessage = r.FailureMessage,
                SelfServiceActivatedAt = r.SelfServiceActivatedAt,
                RequestedAt = r.RequestedAt,
                CompletedAt = r.CompletedAt,
                CompletedByUserId = r.CompletedByUserId,
                CompletedByUserName = r.CompletedByUser != null ? r.CompletedByUser.DisplayName ?? r.CompletedByUser.UserName : null,
                CompletionNote = r.CompletionNote
            };
        }
    }
}
