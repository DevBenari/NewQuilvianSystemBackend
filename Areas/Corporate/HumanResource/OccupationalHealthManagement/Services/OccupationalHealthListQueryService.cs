using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Daftar lintas pegawai K3RS (register masalah SDM MSL-14, keputusan DEC-FER-05 opsi A, 18 November 2026).
    /// </summary>
    /// <remarks>
    /// Contoh kebutuhan: Kepala K3RS menjawab "siapa saja perawat yang sedang dibatasi tugasnya?" tanpa membuka pegawai
    /// satu per satu. Hanya kolom administratif yang dikirim; dasar klinis (<c>ClinicalBasisRestricted</c>,
    /// <c>ConfidentialClinicalNotes</c>) tetap hanya lewat endpoint per pegawai dengan hak <c>ViewClinicalDetail</c>.
    /// </remarks>
    public interface IOccupationalHealthListQueryService
    {
        Task<PagedResult<WorkRestrictionListItemDto>> GetRestrictionsAsync(OccupationalHealthListQuery query, CancellationToken cancellationToken = default);
        Task<PagedResult<FitnessSummaryListItemDto>> GetFitnessSummariesAsync(OccupationalHealthListQuery query, CancellationToken cancellationToken = default);
        Task<PagedResult<InjuryListItemDto>> GetInjuriesAsync(OccupationalHealthListQuery query, CancellationToken cancellationToken = default);
        Task<PagedResult<VaccinationListItemDto>> GetVaccinationsAsync(OccupationalHealthListQuery query, CancellationToken cancellationToken = default);
    }

    public class OccupationalHealthListQueryService : IOccupationalHealthListQueryService
    {
        private readonly ApplicationDbContext _context;

        public OccupationalHealthListQueryService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PagedResult<WorkRestrictionListItemDto>> GetRestrictionsAsync(OccupationalHealthListQuery q, CancellationToken cancellationToken = default)
        {
            var (page, size) = HrCrossEmployeeLookup.NormalizePage(q.PageNumber, q.PageSize);
            var pattern = HrCrossEmployeeLookup.LikePattern(q.Search);
            var query =
                from r in _context.TrxWorkRestrictions.AsNoTracking()
                where !r.IsDelete
                join p in _context.MstWorkforceProfiles.AsNoTracking() on r.WorkforceProfileId equals p.Id into pj
                from p in pj.DefaultIfEmpty()
                let employeeId = r.EmployeeId ?? _context.MstEmployees.Where(e => e.WorkforceProfileId == r.WorkforceProfileId).Select(e => (Guid?)e.Id).FirstOrDefault()
                select new { r, EmployeeId = employeeId, Name = p != null ? p.DisplayName : null, Code = p != null ? p.ProfileCode : null };

            if (q.ActiveOnly == true) query = query.Where(x => x.r.IsActive && x.r.RestrictionStatus == "Active");
            if (!string.IsNullOrWhiteSpace(q.Status)) query = query.Where(x => x.r.RestrictionStatus == q.Status);
            if (pattern != null)
            {
                query = query.Where(x => EF.Functions.ILike(x.r.RestrictionNumber, pattern)
                    || (x.Name != null && EF.Functions.ILike(x.Name, pattern))
                    || (x.Code != null && EF.Functions.ILike(x.Code, pattern)));
            }

            var total = await query.CountAsync(cancellationToken);
            var rows = await query
                .OrderByDescending(x => x.r.EffectiveStartDate)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new WorkRestrictionListItemDto
                {
                    Id = x.r.Id,
                    RestrictionNumber = x.r.RestrictionNumber,
                    WorkforceProfileId = x.r.WorkforceProfileId,
                    EmployeeId = x.EmployeeId,
                    EmployeeName = x.Name ?? "Pegawai tidak ditemukan",
                    ProfileCode = x.Code,
                    RestrictionType = x.r.RestrictionType,
                    RestrictionStatus = x.r.RestrictionStatus,
                    EffectiveStartDate = x.r.EffectiveStartDate,
                    EffectiveEndDate = x.r.EffectiveEndDate,
                    ReviewDate = x.r.ReviewDate,
                    AdministrativeInstruction = x.r.AdministrativeInstruction,
                    MaximumWorkHoursPerDay = x.r.MaximumWorkHoursPerDay,
                    NoNightShift = x.r.NoNightShift,
                    NoOnCall = x.r.NoOnCall,
                    NoHeavyLifting = x.r.NoHeavyLifting,
                    IsSchedulingBlocked = x.r.IsSchedulingBlocked,
                    IsClinicalServiceBlocked = x.r.IsClinicalServiceBlocked
                })
                .ToListAsync(cancellationToken);

            return HrCrossEmployeeLookup.ToPaged(rows, total, page, size);
        }

        public async Task<PagedResult<FitnessSummaryListItemDto>> GetFitnessSummariesAsync(OccupationalHealthListQuery q, CancellationToken cancellationToken = default)
        {
            var (page, size) = HrCrossEmployeeLookup.NormalizePage(q.PageNumber, q.PageSize);
            var pattern = HrCrossEmployeeLookup.LikePattern(q.Search);
            var query =
                from f in _context.TrxEmployeeFitnessToWorks.AsNoTracking()
                where !f.IsDelete
                join p in _context.MstWorkforceProfiles.AsNoTracking() on f.WorkforceProfileId equals p.Id into pj
                from p in pj.DefaultIfEmpty()
                let employeeId = f.EmployeeId ?? _context.MstEmployees.Where(e => e.WorkforceProfileId == f.WorkforceProfileId).Select(e => (Guid?)e.Id).FirstOrDefault()
                select new { f, EmployeeId = employeeId, Name = p != null ? p.DisplayName : null, Code = p != null ? p.ProfileCode : null };

            if (q.ActiveOnly == true) query = query.Where(x => x.f.IsActive);
            if (!string.IsNullOrWhiteSpace(q.Status)) query = query.Where(x => x.f.FitnessStatus == q.Status);
            if (pattern != null)
            {
                query = query.Where(x => EF.Functions.ILike(x.f.AssessmentNumber, pattern)
                    || (x.Name != null && EF.Functions.ILike(x.Name, pattern))
                    || (x.Code != null && EF.Functions.ILike(x.Code, pattern)));
            }

            var total = await query.CountAsync(cancellationToken);
            var rows = await query
                .OrderByDescending(x => x.f.AssessmentDate)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new FitnessSummaryListItemDto
                {
                    Id = x.f.Id,
                    AssessmentNumber = x.f.AssessmentNumber,
                    WorkforceProfileId = x.f.WorkforceProfileId,
                    EmployeeId = x.EmployeeId,
                    EmployeeName = x.Name ?? "Pegawai tidak ditemukan",
                    ProfileCode = x.Code,
                    AssessmentDate = x.f.AssessmentDate,
                    FitnessStatus = x.f.FitnessStatus,
                    EffectiveStartDate = x.f.EffectiveStartDate,
                    EffectiveEndDate = x.f.EffectiveEndDate,
                    WorkRestrictionRequired = x.f.WorkRestrictionRequired,
                    IsSchedulingAllowed = x.f.IsSchedulingAllowed,
                    IsClinicalDutyAllowed = x.f.IsClinicalDutyAllowed,
                    ReviewDate = x.f.ReviewDate
                })
                .ToListAsync(cancellationToken);

            return HrCrossEmployeeLookup.ToPaged(rows, total, page, size);
        }

        public async Task<PagedResult<InjuryListItemDto>> GetInjuriesAsync(OccupationalHealthListQuery q, CancellationToken cancellationToken = default)
        {
            var (page, size) = HrCrossEmployeeLookup.NormalizePage(q.PageNumber, q.PageSize);
            var pattern = HrCrossEmployeeLookup.LikePattern(q.Search);
            var query =
                from i in _context.TrxEmployeeInjuries.AsNoTracking()
                where !i.IsDelete
                join p in _context.MstWorkforceProfiles.AsNoTracking() on i.WorkforceProfileId equals p.Id into pj
                from p in pj.DefaultIfEmpty()
                select new { i, Name = p != null ? p.DisplayName : null, Code = p != null ? p.ProfileCode : null };

            if (q.ActiveOnly == true) query = query.Where(x => x.i.IsActive);
            if (!string.IsNullOrWhiteSpace(q.Status)) query = query.Where(x => x.i.InjuryStatus == q.Status);
            if (pattern != null)
            {
                query = query.Where(x => EF.Functions.ILike(x.i.InjuryNumber, pattern)
                    || (x.Name != null && EF.Functions.ILike(x.Name, pattern))
                    || (x.Code != null && EF.Functions.ILike(x.Code, pattern)));
            }

            var total = await query.CountAsync(cancellationToken);
            var rows = await query
                .OrderByDescending(x => x.i.InjuryDateTime)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new InjuryListItemDto
                {
                    Id = x.i.Id,
                    InjuryNumber = x.i.InjuryNumber,
                    WorkforceProfileId = x.i.WorkforceProfileId,
                    EmployeeName = x.Name ?? "Pegawai tidak ditemukan",
                    ProfileCode = x.Code,
                    InjuryDateTime = x.i.InjuryDateTime,
                    InjuryType = x.i.InjuryType,
                    BodyPart = x.i.BodyPart,
                    SeverityLevel = x.i.SeverityLevel,
                    IsLostTimeInjury = x.i.IsLostTimeInjury,
                    LostWorkDays = x.i.LostWorkDays,
                    IsReportableIncident = x.i.IsReportableIncident,
                    WorkCompensationClaimNumber = x.i.WorkCompensationClaimNumber,
                    ReturnToWorkDate = x.i.ReturnToWorkDate,
                    InjuryStatus = x.i.InjuryStatus,
                    BpjsStage1ReportDeadline = x.i.InjuryDateTime.AddHours(48),
                    BpjsReportedWithin48Hours = x.i.CreateDateTime <= x.i.InjuryDateTime.AddHours(48)
                })
                .ToListAsync(cancellationToken);

            return HrCrossEmployeeLookup.ToPaged(rows, total, page, size);
        }

        public async Task<PagedResult<VaccinationListItemDto>> GetVaccinationsAsync(OccupationalHealthListQuery q, CancellationToken cancellationToken = default)
        {
            var (page, size) = HrCrossEmployeeLookup.NormalizePage(q.PageNumber, q.PageSize);
            var pattern = HrCrossEmployeeLookup.LikePattern(q.Search);
            var query =
                from v in _context.TrxEmployeeVaccinations.AsNoTracking()
                where !v.IsDelete
                join p in _context.MstWorkforceProfiles.AsNoTracking() on v.WorkforceProfileId equals p.Id into pj
                from p in pj.DefaultIfEmpty()
                select new { v, Name = p != null ? p.DisplayName : null, Code = p != null ? p.ProfileCode : null };

            if (q.ActiveOnly == true) query = query.Where(x => x.v.IsActive);
            if (!string.IsNullOrWhiteSpace(q.Status)) query = query.Where(x => x.v.VaccinationStatus == q.Status);
            if (pattern != null)
            {
                query = query.Where(x => EF.Functions.ILike(x.v.VaccinationNumber, pattern)
                    || EF.Functions.ILike(x.v.VaccineName, pattern)
                    || (x.Name != null && EF.Functions.ILike(x.Name, pattern))
                    || (x.Code != null && EF.Functions.ILike(x.Code, pattern)));
            }

            var total = await query.CountAsync(cancellationToken);
            var rows = await query
                .OrderByDescending(x => x.v.AdministrationDate ?? x.v.ScheduledDate)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new VaccinationListItemDto
                {
                    Id = x.v.Id,
                    VaccinationNumber = x.v.VaccinationNumber,
                    WorkforceProfileId = x.v.WorkforceProfileId,
                    EmployeeName = x.Name ?? "Pegawai tidak ditemukan",
                    ProfileCode = x.Code,
                    VaccineType = x.v.VaccineType,
                    VaccineName = x.v.VaccineName,
                    DoseNumber = x.v.DoseNumber,
                    AdministrationDate = x.v.AdministrationDate,
                    NextDoseDate = x.v.NextDoseDate,
                    ExpiryDate = x.v.ExpiryDate,
                    VaccinationStatus = x.v.VaccinationStatus,
                    IsMandatory = x.v.IsMandatory,
                    IsCompliant = x.v.IsCompliant,
                    IsVerified = x.v.IsVerified
                })
                .ToListAsync(cancellationToken);

            return HrCrossEmployeeLookup.ToPaged(rows, total, page, size);
        }
    }
}
