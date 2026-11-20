using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Shared.HumanResource.Helpers
{
    /// <summary>
    /// Bantuan bersama daftar lintas pegawai modul SDM (MSL-14, DEC-FER-05 opsi A, 18 November 2026):
    /// pembatasan halaman yang seragam dan pencarian nama/kode pegawai untuk baris yang sudah dipaging.
    /// </summary>
    public static class HrCrossEmployeeLookup
    {
        public const int MaxPageSize = 100;

        public sealed record EmployeeLabel(string Name, string? Code);

        public static (int PageNumber, int PageSize) NormalizePage(int pageNumber, int pageSize) =>
            (Math.Max(1, pageNumber), Math.Clamp(pageSize <= 0 ? 20 : pageSize, 1, MaxPageSize));

        public static PagedResult<T> ToPaged<T>(IReadOnlyList<T> items, int totalData, int pageNumber, int pageSize) => new()
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = totalData,
            TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
            Items = items.ToList()
        };

        /// <summary>Nama tampilan per <c>MstWorkforceProfile.Id</c>.</summary>
        public static async Task<Dictionary<Guid, EmployeeLabel>> LoadProfileLabelsAsync(
            ApplicationDbContext context, IEnumerable<Guid> workforceProfileIds, CancellationToken cancellationToken)
        {
            var ids = workforceProfileIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<Guid, EmployeeLabel>();
            return await context.MstWorkforceProfiles.AsNoTracking()
                .Where(p => ids.Contains(p.Id))
                .Select(p => new { p.Id, p.DisplayName, p.ProfileCode })
                .ToDictionaryAsync(p => p.Id, p => new EmployeeLabel(p.DisplayName, p.ProfileCode), cancellationToken);
        }

        /// <summary>
        /// Nama per kunci pegawai yang bisa berupa <c>MstEmployee.Id</c> atau <c>MstWorkforceProfile.Id</c>
        /// (modul Hubungan Industrial menyimpan salah satunya di kolom <c>employee_id</c>).
        /// </summary>
        public static async Task<Dictionary<Guid, EmployeeLabel>> LoadEmployeeLabelsAsync(
            ApplicationDbContext context, IEnumerable<Guid> employeeKeys, CancellationToken cancellationToken)
        {
            var ids = employeeKeys.Where(x => x != Guid.Empty).Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<Guid, EmployeeLabel>();

            var result = await context.MstEmployees.AsNoTracking()
                .Where(e => ids.Contains(e.Id))
                .Select(e => new { e.Id, e.FullName, e.EmployeeNumber })
                .ToDictionaryAsync(e => e.Id, e => new EmployeeLabel(e.FullName, e.EmployeeNumber), cancellationToken);

            var missing = ids.Where(id => !result.ContainsKey(id)).ToList();
            if (missing.Count > 0)
            {
                foreach (var (id, label) in await LoadProfileLabelsAsync(context, missing, cancellationToken))
                {
                    result[id] = label;
                }
            }

            return result;
        }

        public static string NameOf(Dictionary<Guid, EmployeeLabel> labels, Guid key) =>
            labels.TryGetValue(key, out var label) && !string.IsNullOrWhiteSpace(label.Name) ? label.Name : "Pegawai tidak ditemukan";

        public static string? CodeOf(Dictionary<Guid, EmployeeLabel> labels, Guid key) =>
            labels.TryGetValue(key, out var label) ? label.Code : null;

        /// <summary>Pola ILIKE yang aman untuk pencarian teks bebas (karakter wildcard diloloskan).</summary>
        public static string? LikePattern(string? search)
        {
            if (string.IsNullOrWhiteSpace(search)) return null;
            var escaped = search.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            return $"%{escaped}%";
        }
    }
}
