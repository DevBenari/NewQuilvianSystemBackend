using System;
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
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public class HiringHandoffService : IHiringHandoffService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHrdOnboardingCaseService? _onboardingCaseService;

        public HiringHandoffService(
            ApplicationDbContext dbContext,
            IHrdOnboardingCaseService? onboardingCaseService = null)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _onboardingCaseService = onboardingCaseService;
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

            // Aturan Bisnis VR-REC-20: Cek status penawaran kerja
            if (offer.Status != JobOfferStatus.FinalAccepted && offer.Status != JobOfferStatus.ConditionalAccepted)
            {
                return RecruitmentResult<HiringHandoffResponse>.Unprocessable(
                    $"Serah terima onboarding hanya dapat diproses bila penawaran kerja telah disetujui kandidat. Status saat ini: {offer.Status} (VR-REC-20).");
            }

            // Aturan Bisnis VR-REC-20: Cek hasil kelaikan kerja MCU pra-kerja
            var mcu = await _dbContext.HrdPreEmploymentMedicalChecks
                .FirstOrDefaultAsync(m => m.OfferId == offer.Id && !m.IsDelete, cancellationToken);

            if (mcu == null || (mcu.ClearanceResult != McuClearanceResult.Fit && mcu.ClearanceResult != McuClearanceResult.FitWithRestrictions))
            {
                return RecruitmentResult<HiringHandoffResponse>.Unprocessable(
                    "Serah terima kandidat ke onboarding hanya dapat diproses bila hasil pemeriksaan kesehatan pra-kerja (MCU) dinyatakan Laik Kerja (FIT atau FIT WITH RESTRICTIONS) (VR-REC-20).");
            }

            var existingHandoff = await _dbContext.HrdCandidateHiringHandoffs
                .FirstOrDefaultAsync(h => h.OfferId == offer.Id && !h.IsDelete, cancellationToken);

            if (existingHandoff != null)
            {
                return RecruitmentResult<HiringHandoffResponse>.Conflict(
                    "Dokumen serah terima onboarding sudah pernah diterbitkan untuk penawaran ini.");
            }

            var today = DateTime.UtcNow;
            var todayStr = today.ToString("yyyyMMdd");

            // Aturan Bisnis AT-HRD-D2-11 & HRD-DEC-075: Pembentukan Draf WorkforceProfile Baru di Sub-modul 02
            var countProfilesToday = await _dbContext.MstWorkforceProfiles
                .CountAsync(p => p.ProfileCode.StartsWith($"WFP-{todayStr}-"), cancellationToken);

            var profileCode = $"WFP-{todayStr}-{(countProfilesToday + 1):D4}";

            var userType = vacancy.IsClinical ? UserType.PermanentDoctor : UserType.Employee;

            var newProfile = new MstWorkforceProfile
            {
                Id = Guid.NewGuid(),
                ProfileCode = profileCode,
                UserType = userType,
                DisplayName = candidate.FullName.Trim(),
                Email = candidate.Email.Trim(),
                PhoneNumber = candidate.PhoneNumber.Trim(),
                WhatsAppNumber = candidate.PhoneNumber.Trim(),
                PrimaryPositionId = vacancy.Requisition?.PositionId,
                IsActive = false, // Pending Onboarding (belum aktif login sebelum orientasi selesai)
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };
            await _dbContext.MstWorkforceProfiles.AddAsync(newProfile, cancellationToken);

            // Aturan Bisnis INT-HR-20, INT-HR-23: Sinkronisasi Kursi Formasi WFP
            Guid? seatId = request.HeadcountRequestSeatId;
            if (seatId.HasValue)
            {
                var seat = await _dbContext.HrdHeadcountRequestSeats
                    .FirstOrDefaultAsync(s => s.Id == seatId.Value && !s.IsDelete, cancellationToken);

                if (seat != null)
                {
                    seat.SeatStatus = HeadcountSeatStatus.Fulfilled;
                    seat.FulfilledAt = DateOnly.FromDateTime(DateTime.UtcNow);
                    seat.LinkedAt = DateTime.UtcNow;
                    seat.LinkedByUserId = userId;
                    seat.UpdateDateTime = DateTime.UtcNow;
                    seat.UpdateBy = userId;
                }
            }

            // Generate nomor serah terima HND-yyyyMMdd-XXXX
            var countHandoffsToday = await _dbContext.HrdCandidateHiringHandoffs
                .CountAsync(h => h.HandoffNumber.StartsWith($"HND-{todayStr}-"), cancellationToken);

            var handoffNumber = $"HND-{todayStr}-{(countHandoffsToday + 1):D4}";

            var handoff = new HrdCandidateHiringHandoff
            {
                Id = Guid.NewGuid(),
                HandoffNumber = handoffNumber,
                OfferId = offer.Id,
                CandidateId = candidate.Id,
                WorkforceProfileId = newProfile.Id,
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
            candidate.UpdateDateTime = DateTime.UtcNow;
            candidate.UpdateBy = userId;

            offer.Status = JobOfferStatus.FinalAccepted;
            offer.RowVersion = Guid.NewGuid();
            offer.UpdateDateTime = DateTime.UtcNow;
            offer.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Integrasi INT-HR-25 / BE-HRD-074: Pembentukan kasus onboarding otomatis dari serah terima
            if (_onboardingCaseService != null)
            {
                var caseResult = await _onboardingCaseService.CreateFromHandoffAsync(handoff.Id, userId, cancellationToken);
                if (!caseResult.IsSuccess)
                {
                    // Catat pesan peringatan jika pembentukan kasus mengalami hambatan
                    handoff.Notes = $"{handoff.Notes} | Catatan Onboarding: {caseResult.ErrorMessage}";
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
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
                CandidateNik = model.Candidate?.NationalIdNumber ?? "-",
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
