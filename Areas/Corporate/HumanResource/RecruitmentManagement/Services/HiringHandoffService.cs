using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public class HiringHandoffService : IHiringHandoffService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHrdOnboardingCaseService? _onboardingCaseService;
        private readonly NumberSeriesAllocator? _numberSeriesAllocator;

        public HiringHandoffService(
            ApplicationDbContext dbContext,
            IHrdOnboardingCaseService? onboardingCaseService = null,
            NumberSeriesAllocator? numberSeriesAllocator = null)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _onboardingCaseService = onboardingCaseService;
            _numberSeriesAllocator = numberSeriesAllocator;
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var guid) ? guid : Guid.Empty;
        }

        public async Task<RecruitmentResult<PagedResult<HiringHandoffListItemResponse>>> GetPagedAsync(
            HiringHandoffQuery query,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbContext.HrdCandidateHiringHandoffs
                .Include(h => h.Offer)
                .Include(h => h.Candidate)
                .Include(h => h.WorkforceProfile)
                .Include(h => h.Offer)
                    .ThenInclude(o => o!.Application)
                        .ThenInclude(a => a!.Vacancy)
                .Where(h => !h.IsDelete)
                .AsQueryable();

            if (query.Status.HasValue)
            {
                dbQuery = dbQuery.Where(h => h.Status == query.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.Trim().ToLower();
                dbQuery = dbQuery.Where(h =>
                    h.HandoffNumber.ToLower().Contains(term) ||
                    (h.Offer != null && h.Offer.OfferNumber.ToLower().Contains(term)) ||
                    (h.Candidate != null && (h.Candidate.FullName.ToLower().Contains(term) || h.Candidate.NationalIdNumber.Contains(term))) ||
                    (h.WorkforceProfile != null && h.WorkforceProfile.ProfileCode.ToLower().Contains(term)));
            }

            var totalCount = await dbQuery.CountAsync(cancellationToken);
            var items = await dbQuery
                .OrderByDescending(h => h.HandoffDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(h => new HiringHandoffListItemResponse
                {
                    Id = h.Id,
                    HandoffNumber = h.HandoffNumber,
                    OfferNumber = h.Offer != null ? h.Offer.OfferNumber : "-",
                    CandidateName = h.Candidate != null ? h.Candidate.FullName : "-",
                    VacancyTitle = h.Offer != null && h.Offer.Application != null && h.Offer.Application.Vacancy != null
                        ? h.Offer.Application.Vacancy.Title : "-",
                    WorkforceProfileCode = h.WorkforceProfile != null ? h.WorkforceProfile.ProfileCode : "-",
                    HandoffDate = h.HandoffDate,
                    Status = h.Status
                })
                .ToListAsync(cancellationToken);

            var totalPages = query.PageSize > 0 ? (int)Math.Ceiling((double)totalCount / query.PageSize) : 0;

            var result = new PagedResult<HiringHandoffListItemResponse>
            {
                Items = items,
                TotalData = totalCount,
                TotalPage = totalPages,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return RecruitmentResult<PagedResult<HiringHandoffListItemResponse>>.Ok(result, "Daftar berkas serah terima onboarding berhasil diambil.");
        }

        public async Task<RecruitmentResult<HiringHandoffResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var handoff = await _dbContext.HrdCandidateHiringHandoffs
                .Include(h => h.Offer)
                .Include(h => h.Candidate)
                .Include(h => h.WorkforceProfile)
                .Include(h => h.HeadcountRequestSeat)
                .Include(h => h.Offer)
                    .ThenInclude(o => o!.Application)
                        .ThenInclude(a => a!.Vacancy)
                .FirstOrDefaultAsync(h => h.Id == id && !h.IsDelete, cancellationToken);

            if (handoff == null)
            {
                return RecruitmentResult<HiringHandoffResponse>.NotFound("Dokumen serah terima hiring handoff tidak ditemukan.");
            }

            return RecruitmentResult<HiringHandoffResponse>.Ok(MapToResponse(handoff), "Detail dokumen serah terima onboarding berhasil diambil.");
        }

        public async Task<RecruitmentResult<HiringHandoffResponse>> GenerateHandoffAsync(
            GenerateHiringHandoffRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var offer = await _dbContext.HrdJobOffers
                .Include(o => o.Application)
                    .ThenInclude(a => a!.Candidate)
                .Include(o => o.Application)
                    .ThenInclude(a => a!.Vacancy)
                        .ThenInclude(v => v!.Requisition)
                .FirstOrDefaultAsync(o => o.Id == request.OfferId && !o.IsDelete, cancellationToken);

            if (offer == null)
            {
                return RecruitmentResult<HiringHandoffResponse>.NotFound("Surat penawaran kerja tidak ditemukan.");
            }

            var app = offer.Application;
            var candidate = app?.Candidate;
            var vacancy = app?.Vacancy;

            if (app == null || candidate == null || vacancy == null)
            {
                return RecruitmentResult<HiringHandoffResponse>.Unprocessable("Data lamaran, kandidat, atau lowongan tidak lengkap.");
            }

            var existingHandoff = await _dbContext.HrdCandidateHiringHandoffs
                .FirstOrDefaultAsync(h => h.OfferId == offer.Id && !h.IsDelete, cancellationToken);

            if (existingHandoff != null)
            {
                // Idempotent partial-handoff recovery: jika handoff sudah ada namun OnboardingCase belum terbentuk, pulihkan tanpa duplikasi profil/kursi
                if (existingHandoff.Status == HandoffStatus.Cancelled)
                {
                    return RecruitmentResult<HiringHandoffResponse>.Conflict(
                        "Dokumen serah terima onboarding ini sudah berstatus Cancelled.");
                }

                var hasOnboardingCase = await _dbContext.HrdOnboardingCases
                    .AnyAsync(c => c.HiringHandoffId == existingHandoff.Id && !c.IsDelete, cancellationToken);

                if (hasOnboardingCase)
                {
                    return RecruitmentResult<HiringHandoffResponse>.Conflict(
                        "Dokumen serah terima dan kasus onboarding sudah pernah diterbitkan untuk penawaran ini.");
                }

                if (_onboardingCaseService == null)
                {
                    return RecruitmentResult<HiringHandoffResponse>.Unprocessable(
                        "Layanan pembentukan kasus onboarding tidak tersedia untuk pemulihan partial handoff.");
                }

                await using var recoveryTx = _dbContext.Database.IsRelational()
                    ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                    : null;

                var recoverResult = await _onboardingCaseService.CreateFromHandoffAsync(existingHandoff.Id, userId, cancellationToken);
                if (!recoverResult.IsSuccess)
                {
                    if (recoveryTx != null)
                    {
                        await recoveryTx.RollbackAsync(cancellationToken);
                    }

                    return RecruitmentResult<HiringHandoffResponse>.Unprocessable(
                        $"Pemulihan kasus onboarding gagal: {recoverResult.ErrorMessage}");
                }

                existingHandoff.Status = HandoffStatus.TransferredToOnboarding;
                existingHandoff.Notes = $"{existingHandoff.Notes} | Pemulihan kasus onboarding berhasil.";
                existingHandoff.RowVersion = Guid.NewGuid();
                existingHandoff.UpdateDateTime = DateTime.UtcNow;
                existingHandoff.UpdateBy = userId;
                await _dbContext.SaveChangesAsync(cancellationToken);

                if (recoveryTx != null)
                {
                    await recoveryTx.CommitAsync(cancellationToken);
                }

                return await GetByIdAsync(existingHandoff.Id, cancellationToken);
            }

            // Validasi prasyarat penuh (FinalAccepted + MCU Fit + non-terminal + bebas FitWithRestrictions unconfirmed)
            var mcu = await _dbContext.HrdPreEmploymentMedicalChecks
                .FirstOrDefaultAsync(m => m.OfferId == offer.Id && !m.IsDelete, cancellationToken);

            var handoffPrereq = RecruitmentJourneyPolicy.EvaluateHandoffPrerequisites(app, offer, mcu, candidate);
            if (!handoffPrereq.IsEligible)
            {
                return RecruitmentResult<HiringHandoffResponse>.Unprocessable(handoffPrereq.BlockingMessage!);
            }

            // Validasi Kursi Formasi WFP sebelum menulis apa pun (INT-HR-20, INT-HR-23)
            Guid? seatId = request.HeadcountRequestSeatId;
            if (seatId.HasValue)
            {
                var seat = await _dbContext.HrdHeadcountRequestSeats
                    .FirstOrDefaultAsync(s => s.Id == seatId.Value && !s.IsDelete, cancellationToken);

                if (seat == null)
                {
                    return RecruitmentResult<HiringHandoffResponse>.NotFound("Kursi formasi WFP (HeadcountRequestSeatId) tidak ditemukan.");
                }

                if (vacancy.Requisition != null
                    && vacancy.Requisition.HeadcountRequestId != Guid.Empty
                    && seat.HeadcountRequestId != vacancy.Requisition.HeadcountRequestId)
                {
                    return RecruitmentResult<HiringHandoffResponse>.Unprocessable(
                        "Kursi formasi yang dipilih bukan milik Headcount Request yang terkait dengan lowongan ini.");
                }

                if (seat.SeatStatus != HeadcountSeatStatus.Open && seat.SeatStatus != HeadcountSeatStatus.Linked)
                {
                    return RecruitmentResult<HiringHandoffResponse>.Conflict(
                        $"Kursi formasi #{seat.SeatNumber} tidak tersedia untuk dipakai (status saat ini: '{seat.SeatStatus}').");
                }
            }

            // Rehire Dedup HRD-DEC-075: gunakan ulang WorkforceProfile lama bila kandidat adalah rehire / NIK sudah memiliki WorkforceProfile
            MstWorkforceProfile? profileToUse = null;
            var reusedProfileId = candidate.PreviousWorkforceProfileId;
            if (!reusedProfileId.HasValue)
            {
                var existingEmp = await _dbContext.MstEmployees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.IdentityNumber == candidate.NationalIdNumber && !e.IsDelete, cancellationToken);
                reusedProfileId = existingEmp?.WorkforceProfileId;
            }

            if (reusedProfileId.HasValue)
            {
                profileToUse = await _dbContext.MstWorkforceProfiles
                    .FirstOrDefaultAsync(p => p.Id == reusedProfileId.Value && !p.IsDelete, cancellationToken);
            }

            var dateCode = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, RecruitmentJourneyPolicy.JakartaTimeZone)
                .ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            string? allocatedProfileCode = null;
            if (profileToUse == null)
            {
                var wfpPrefix = $"WFP-{dateCode}-";
                var existingProfileCodes = await _dbContext.MstWorkforceProfiles
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(p => p.ProfileCode.StartsWith(wfpPrefix))
                    .Select(p => p.ProfileCode)
                    .ToListAsync(cancellationToken);

                allocatedProfileCode = await RecruitmentJourneyPolicy.AllocateSequenceAsync(
                    _numberSeriesAllocator,
                    existingProfileCodes,
                    "HRD_WORKFORCE_PROFILE_CODE",
                    "WFP",
                    userId,
                    cancellationToken);
            }

            var hndPrefix = $"HND-{dateCode}-";
            var existingHandoffNumbers = await _dbContext.HrdCandidateHiringHandoffs
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(h => h.HandoffNumber.StartsWith(hndPrefix))
                .Select(h => h.HandoffNumber)
                .ToListAsync(cancellationToken);

            var handoffNumber = await RecruitmentJourneyPolicy.AllocateSequenceAsync(
                _numberSeriesAllocator,
                existingHandoffNumbers,
                "HRD_HIRING_HANDOFF",
                "HND",
                userId,
                cancellationToken);

            await using var tx = _dbContext.Database.IsRelational()
                ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                : null;

            if (profileToUse == null)
            {
                var userType = vacancy.IsClinical ? UserType.PermanentDoctor : UserType.Employee;
                profileToUse = new MstWorkforceProfile
                {
                    Id = Guid.NewGuid(),
                    ProfileCode = allocatedProfileCode!,
                    UserType = userType,
                    DisplayName = candidate.FullName.Trim(),
                    Email = candidate.Email.Trim(),
                    PhoneNumber = candidate.PhoneNumber.Trim(),
                    WhatsAppNumber = candidate.PhoneNumber.Trim(),
                    PrimaryPositionId = vacancy.Requisition?.PositionId,
                    IsActive = false,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userId
                };
                await _dbContext.MstWorkforceProfiles.AddAsync(profileToUse, cancellationToken);
            }
            else
            {
                profileToUse.DisplayName = candidate.FullName.Trim();
                profileToUse.Email = candidate.Email.Trim();
                profileToUse.PhoneNumber = candidate.PhoneNumber.Trim();
                profileToUse.WhatsAppNumber = candidate.PhoneNumber.Trim();
                profileToUse.PrimaryPositionId = vacancy.Requisition?.PositionId ?? profileToUse.PrimaryPositionId;
                profileToUse.IsActive = false;
                profileToUse.UpdateDateTime = DateTime.UtcNow;
                profileToUse.UpdateBy = userId;
            }

            if (seatId.HasValue)
            {
                var seat = await _dbContext.HrdHeadcountRequestSeats
                    .FirstAsync(s => s.Id == seatId.Value && !s.IsDelete, cancellationToken);
                seat.SeatStatus = HeadcountSeatStatus.Fulfilled;
                seat.FulfilledAt = DateOnly.FromDateTime(DateTime.UtcNow);
                seat.LinkedAt = DateTime.UtcNow;
                seat.LinkedByUserId = userId;
                seat.UpdateDateTime = DateTime.UtcNow;
                seat.UpdateBy = userId;
            }

            var handoff = new HrdCandidateHiringHandoff
            {
                Id = Guid.NewGuid(),
                HandoffNumber = handoffNumber,
                OfferId = offer.Id,
                CandidateId = candidate.Id,
                WorkforceProfileId = profileToUse.Id,
                HeadcountRequestSeatId = seatId,
                HandoffDate = DateTime.UtcNow,
                Status = HandoffStatus.TransferredToOnboarding,
                Notes = request.Notes?.Trim() ?? "Serah terima kandidat berhasil ke draf onboarding karyawan baru.",
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };
            await _dbContext.HrdCandidateHiringHandoffs.AddAsync(handoff, cancellationToken);

            // Perbarui status lamaran dan kandidat
            app.CurrentStage = ApplicationStage.Handoff;
            app.StageStatus = "HandoffCompletedPendingOnboarding";
            app.UpdateDateTime = DateTime.UtcNow;
            app.UpdateBy = userId;

            candidate.Status = CandidateStatus.Hired;
            candidate.PreviousWorkforceProfileId ??= profileToUse.Id;
            candidate.UpdateDateTime = DateTime.UtcNow;
            candidate.UpdateBy = userId;

            offer.Status = JobOfferStatus.FinalAccepted;
            offer.RowVersion = Guid.NewGuid();
            offer.UpdateDateTime = DateTime.UtcNow;
            offer.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Integrasi INT-HR-25 / BE-HRD-074: Pembentukan kasus onboarding atomik dari serah terima
            if (_onboardingCaseService != null)
            {
                var caseResult = await _onboardingCaseService.CreateFromHandoffAsync(handoff.Id, userId, cancellationToken);
                if (!caseResult.IsSuccess)
                {
                    if (tx != null)
                    {
                        await tx.RollbackAsync(cancellationToken);
                    }

                    return RecruitmentResult<HiringHandoffResponse>.Unprocessable(
                        $"Serah terima onboarding dibatalkan secara atomik karena pembentukan kasus onboarding gagal: {caseResult.ErrorMessage}");
                }
            }

            if (tx != null)
            {
                await tx.CommitAsync(cancellationToken);
            }

            return await GetByIdAsync(handoff.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<HiringHandoffResponse>> ConfirmTransferAsync(
            Guid id,
            ConfirmHiringHandoffRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var handoff = await _dbContext.HrdCandidateHiringHandoffs
                .FirstOrDefaultAsync(h => h.Id == id && !h.IsDelete, cancellationToken);

            if (handoff == null)
            {
                return RecruitmentResult<HiringHandoffResponse>.NotFound("Dokumen serah terima hiring handoff tidak ditemukan.");
            }

            if (handoff.Status == HandoffStatus.Cancelled)
            {
                return RecruitmentResult<HiringHandoffResponse>.Unprocessable(
                    "Konfirmasi selesai ditolak karena dokumen serah terima berstatus Cancelled.");
            }

            var hasCase = await _dbContext.HrdOnboardingCases
                .AnyAsync(c => c.HiringHandoffId == handoff.Id && !c.IsDelete, cancellationToken);

            if (!hasCase)
            {
                if (_onboardingCaseService == null)
                {
                    return RecruitmentResult<HiringHandoffResponse>.Unprocessable(
                        "Konfirmasi selesai ditolak: kasus onboarding belum terbentuk.");
                }

                var recovery = await _onboardingCaseService.CreateFromHandoffAsync(handoff.Id, userId, cancellationToken);
                if (!recovery.IsSuccess)
                {
                    return RecruitmentResult<HiringHandoffResponse>.Unprocessable(
                        $"Konfirmasi selesai ditolak karena pembuatan kasus onboarding gagal: {recovery.ErrorMessage}");
                }
            }

            handoff.Status = HandoffStatus.Completed;
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                handoff.Notes = $"{handoff.Notes} | Konfirmasi: {request.Notes.Trim()}";
            }
            handoff.RowVersion = Guid.NewGuid();
            handoff.UpdateDateTime = DateTime.UtcNow;
            handoff.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(handoff.Id, cancellationToken);
        }

        private static HiringHandoffResponse MapToResponse(HrdCandidateHiringHandoff model)
        {
            return new HiringHandoffResponse
            {
                Id = model.Id,
                HandoffNumber = model.HandoffNumber,
                OfferId = model.OfferId,
                OfferNumber = model.Offer?.OfferNumber ?? "-",
                CandidateId = model.CandidateId,
                CandidateName = model.Candidate?.FullName ?? "-",
                CandidateNik = RecruitmentJourneyPolicy.MaskNationalIdNumber(model.Candidate?.NationalIdNumber),
                VacancyTitle = model.Offer?.Application?.Vacancy?.Title ?? "-",
                WorkforceProfileId = model.WorkforceProfileId,
                WorkforceProfileCode = model.WorkforceProfile?.ProfileCode,
                HeadcountRequestSeatId = model.HeadcountRequestSeatId,
                SeatNumber = model.HeadcountRequestSeat?.SeatNumber,
                HandoffDate = model.HandoffDate,
                Status = model.Status,
                Notes = model.Notes,
                CreateDateTime = model.CreateDateTime
            };
        }
    }
}
