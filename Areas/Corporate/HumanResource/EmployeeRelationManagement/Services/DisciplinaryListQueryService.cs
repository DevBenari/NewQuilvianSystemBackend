using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    /// <summary>
    /// Daftar sanksi dan skorsing seluruh pegawai (register masalah SDM MSL-14, DEC-FER-05 opsi A, 18 November 2026).
    /// </summary>
    /// <remarks>
    /// Kolom <c>employee_id</c> modul ini bisa berisi <c>MstEmployee.Id</c> atau <c>MstWorkforceProfile.Id</c>, sehingga nama
    /// pegawai dicari dari keduanya. Uraian pelanggaran dan pernyataan banding tidak ikut; isinya tetap lewat detail per pegawai.
    /// </remarks>
    public interface IDisciplinaryListQueryService
    {
        Task<PagedResult<SanctionListItemDto>> GetSanctionsAsync(DisciplinaryListQuery query, CancellationToken cancellationToken = default);
        Task<PagedResult<SuspensionListItemDto>> GetSuspensionsAsync(DisciplinaryListQuery query, CancellationToken cancellationToken = default);
    }

    public class DisciplinaryListQueryService : IDisciplinaryListQueryService
    {
        /// <summary>Status sanksi yang sah untuk filter <c>statuses</c> (state-transition v30 bagian 34.2, <c>VR-PKB-04</c>).</summary>
        public static readonly IReadOnlyList<string> SanctionStatusValues = new[] { "Active", "Expired", "RevokedOnAppeal" };

        public const string UnknownSanctionStatusMessage = "Status sanksi tidak dikenal.";
        public const string StatusAndStatusesMessage = "Gunakan salah satu: status atau statuses.";

        /// <summary>
        /// Validasi filter status daftar sanksi (<c>VR-PKB-04</c>, <c>VR-PKB-05</c>). Mengembalikan pesan galat, atau
        /// <c>null</c> bila filter sah.
        /// </summary>
        public static string? ValidateSanctionStatusFilter(DisciplinaryListQuery q)
        {
            var requested = (q.Statuses ?? new List<string>()).Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
            if (requested.Count == 0)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(q.Status))
            {
                return StatusAndStatusesMessage;
            }

            return requested.All(v => SanctionStatusValues.Any(a => string.Equals(a, v.Trim(), StringComparison.OrdinalIgnoreCase)))
                ? null
                : UnknownSanctionStatusMessage;
        }

        private static List<string> NormalizeSanctionStatuses(IEnumerable<string>? values) =>
            (values ?? Enumerable.Empty<string>())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => SanctionStatusValues.First(a => string.Equals(a, v.Trim(), StringComparison.OrdinalIgnoreCase)))
                .Distinct()
                .ToList();

        private readonly ApplicationDbContext _context;

        public DisciplinaryListQueryService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PagedResult<SanctionListItemDto>> GetSanctionsAsync(DisciplinaryListQuery q, CancellationToken cancellationToken = default)
        {
            var (page, size) = HrCrossEmployeeLookup.NormalizePage(q.PageNumber, q.PageSize);
            var pattern = HrCrossEmployeeLookup.LikePattern(q.Search);
            var query =
                from s in _context.HrdDisciplinarySanctions.AsNoTracking()
                where !s.IsDelete
                join e in _context.MstEmployees.AsNoTracking() on s.EmployeeId equals e.Id into ej
                from e in ej.DefaultIfEmpty()
                join p in _context.MstWorkforceProfiles.AsNoTracking() on s.EmployeeId equals p.Id into pj
                from p in pj.DefaultIfEmpty()
                select new
                {
                    s,
                    Name = e != null ? e.FullName : (p != null ? p.DisplayName : null),
                    Code = e != null ? e.EmployeeNumber : (p != null ? p.ProfileCode : null)
                };

            if (!string.IsNullOrWhiteSpace(q.Status)) query = query.Where(x => x.s.Status == q.Status);
            var statuses = NormalizeSanctionStatuses(q.Statuses);
            if (statuses.Count > 0) query = query.Where(x => statuses.Contains(x.s.Status));
            if (!string.IsNullOrWhiteSpace(q.Level)) query = query.Where(x => x.s.SanctionLevel == q.Level);
            if (q.ActiveOnly == true)
            {
                var today = DateTime.UtcNow.Date;
                query = query.Where(x => x.s.ValidUntil >= today);
            }

            if (pattern != null)
            {
                query = query.Where(x => EF.Functions.ILike(x.s.SanctionNumber, pattern)
                    || (x.Name != null && EF.Functions.ILike(x.Name, pattern))
                    || (x.Code != null && EF.Functions.ILike(x.Code, pattern)));
            }

            var total = await query.CountAsync(cancellationToken);
            var rows = await query
                .OrderByDescending(x => x.s.IssuedDate)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new SanctionListItemDto
                {
                    SanctionId = x.s.SanctionId,
                    SanctionNumber = x.s.SanctionNumber,
                    EmployeeId = x.s.EmployeeId,
                    EmployeeName = x.Name ?? "Pegawai tidak ditemukan",
                    EmployeeCode = x.Code,
                    SanctionLevel = x.s.SanctionLevel,
                    IssuedDate = x.s.IssuedDate,
                    ValidUntil = x.s.ValidUntil,
                    Status = x.s.Status,
                    IsAppealed = x.s.IsAppealed,
                    AppealedAt = x.s.AppealedAt
                })
                .ToListAsync(cancellationToken);

            return HrCrossEmployeeLookup.ToPaged(rows, total, page, size);
        }

        public async Task<PagedResult<SuspensionListItemDto>> GetSuspensionsAsync(DisciplinaryListQuery q, CancellationToken cancellationToken = default)
        {
            var (page, size) = HrCrossEmployeeLookup.NormalizePage(q.PageNumber, q.PageSize);
            var pattern = HrCrossEmployeeLookup.LikePattern(q.Search);
            var query =
                from s in _context.HrdDisciplinarySuspensions.AsNoTracking()
                where !s.IsDelete
                join e in _context.MstEmployees.AsNoTracking() on s.EmployeeId equals e.Id into ej
                from e in ej.DefaultIfEmpty()
                join p in _context.MstWorkforceProfiles.AsNoTracking() on s.EmployeeId equals p.Id into pj
                from p in pj.DefaultIfEmpty()
                select new
                {
                    s,
                    Name = e != null ? e.FullName : (p != null ? p.DisplayName : null),
                    Code = e != null ? e.EmployeeNumber : (p != null ? p.ProfileCode : null)
                };

            if (!string.IsNullOrWhiteSpace(q.Status)) query = query.Where(x => x.s.Status == q.Status);
            if (q.ActiveOnly == true) query = query.Where(x => x.s.ActualEndDate == null);
            if (pattern != null)
            {
                query = query.Where(x => EF.Functions.ILike(x.s.SuspensionNumber, pattern)
                    || (x.Name != null && EF.Functions.ILike(x.Name, pattern))
                    || (x.Code != null && EF.Functions.ILike(x.Code, pattern)));
            }

            var total = await query.CountAsync(cancellationToken);
            var rows = await query
                .OrderByDescending(x => x.s.StartDate)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new SuspensionListItemDto
                {
                    SuspensionId = x.s.SuspensionId,
                    SuspensionNumber = x.s.SuspensionNumber,
                    EmployeeId = x.s.EmployeeId,
                    EmployeeName = x.Name ?? "Pegawai tidak ditemukan",
                    EmployeeCode = x.Code,
                    StartDate = x.s.StartDate,
                    ExpectedEndDate = x.s.ExpectedEndDate,
                    ActualEndDate = x.s.ActualEndDate,
                    WagePaymentStatus = x.s.WagePaymentStatus,
                    IsSystemAccessRevoked = x.s.IsSystemAccessRevoked,
                    IsPhysicalAccessRevoked = x.s.IsPhysicalAccessRevoked,
                    Status = x.s.Status
                })
                .ToListAsync(cancellationToken);

            return HrCrossEmployeeLookup.ToPaged(rows, total, page, size);
        }
    }
}
