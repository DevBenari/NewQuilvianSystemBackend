using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Pengaturan footer cetak per disiplin (<c>LAB-DEC-119</c>, <c>LAB-DEC-127</c>,
    /// <c>BE-LAB-63</c>).
    ///
    /// <b>Nol pembuatan dan nol penghapusan.</b> Barisnya tetap tiga, di-seed dari
    /// <c>LAB-EVD-005</c>, dan hanya isinya yang berubah.
    /// </summary>
    public class LabDisciplineSettingService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabDisciplineSettingService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        public async Task<List<LabDisciplineSettingResponse>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var rows = await _dbContext.LabDisciplineSettings
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.Discipline)
                .ToListAsync(cancellationToken);

            return [.. rows.Select(Map)];
        }

        public async Task<LabDisciplineSettingResponse> GetByDisciplineAsync(
            LabDiscipline discipline,
            CancellationToken cancellationToken = default)
        {
            var row = await _dbContext.LabDisciplineSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Discipline == discipline && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pengaturan disiplin itu tidak ditemukan.");

            return Map(row);
        }

        /// <summary>
        /// Mengubah isi satu baris.
        ///
        /// <b>Membuat barisnya bila belum ada, dan itu bukan <c>POST</c> terselubung.</b>
        /// Disiplinnya hanya tiga dan seluruhnya sudah tertentu oleh enum; yang dapat terjadi
        /// hanyalah seeder belum sempat berjalan pada satu lingkungan. Menjawab <c>404</c>
        /// di situ memaksa kepala instalasi menunggu penempatan ulang aplikasi hanya untuk
        /// mengetik satu nama.
        /// </summary>
        public async Task<LabDisciplineSettingResponse> UpdateAsync(
            LabDiscipline discipline,
            LabDisciplineSettingUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var row = await _dbContext.LabDisciplineSettings
                .FirstOrDefaultAsync(x => x.Discipline == discipline && !x.IsDelete, cancellationToken);

            var baru = row is null;

            if (row is null)
            {
                row = new LabDisciplineSetting
                {
                    Discipline = discipline,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                _dbContext.LabDisciplineSettings.Add(row);
            }
            else
            {
                row.UpdateDateTime = now;
                row.UpdateBy = actorUserId;
            }

            row.ConsultantLabel = request.ConsultantLabel.Trim();
            row.ConsultantName = Normalize(request.ConsultantName);
            row.StandingNote = Normalize(request.StandingNote);
            row.ReportNumberPrefix = Normalize(request.ReportNumberPrefix);

            // SENGAJA TIDAK memakai Normalize pada pemisah. Normalize memetakan teks kosong
            // menjadi null, sedangkan di sini keduanya BERBEDA ARTI: null berarti belum pernah
            // disetel dan jatuh ke bawaan, teks kosong berarti sengaja tanpa pemisah —
            // Patologi Klinik memang menempelkan tahun langsung pada nomornya (25039254).
            row.ReportNumberSeparator = request.ReportNumberSeparator;

            row.ReportNumberLength = request.ReportNumberLength;
            row.IsActive = request.IsActive;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "LabDisciplineSetting.Update",
                baru
                    ? "Membuat pengaturan disiplin yang belum ada."
                    : "Mengubah pengaturan disiplin.",
                new { row.Id, Discipline = discipline.ToString(), row.ConsultantLabel });

            return Map(row);
        }

        /// <summary>
        /// Ketiga disiplin beserta keterangan sudah atau belum diatur.
        ///
        /// <b>Ruas <c>isConfigured</c> ada supaya layar dapat membedakan dua keadaan yang
        /// terlihat sama:</b> pengaturan yang memang dikosongkan, dan pengaturan yang belum
        /// pernah ada. Alasannya sekerabat dengan <c>criticalRuleAvailable</c> pada <c>r26</c>.
        /// </summary>
        public async Task<List<LabDisciplineOptionResponse>> GetOptionsAsync(
            CancellationToken cancellationToken = default)
        {
            var terpakai = await _dbContext.LabDisciplineSettings
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .Select(x => x.Discipline)
                .ToListAsync(cancellationToken);

            return [.. Enum.GetValues<LabDiscipline>().Select(x => new LabDisciplineOptionResponse
            {
                Value = (int)x,
                Name = x.ToString(),
                Label = ResolveLabel(x),
                IsConfigured = terpakai.Contains(x)
            })];
        }

        private static LabDisciplineSettingResponse Map(LabDisciplineSetting row) => new()
        {
            Id = row.Id,
            Discipline = (int)row.Discipline,
            DisciplineName = ResolveLabel(row.Discipline),
            ConsultantLabel = row.ConsultantLabel,
            ConsultantName = row.ConsultantName,
            StandingNote = row.StandingNote,
            ReportNumberPrefix = row.ReportNumberPrefix,
            ReportNumberSeparator = row.ReportNumberSeparator,
            ReportNumberLength = row.ReportNumberLength,
            ReportNumberExample = LabReportNumberService.Format(
                new LabReportNumberService.LabReportNumberShape(
                    row.ReportNumberPrefix ?? string.Empty,
                    row.ReportNumberSeparator ?? LabReportNumberService.DefaultYearSeparator,
                    Math.Clamp(
                        row.ReportNumberLength,
                        LabReportNumberService.MinSequenceLength,
                        LabReportNumberService.MaxSequenceLength)),
                DateTime.UtcNow.Year,
                1),
            IsActive = row.IsActive
        };

        private static string ResolveLabel(LabDiscipline discipline) => discipline switch
        {
            LabDiscipline.ClinicalPathology => "Patologi Klinik",
            LabDiscipline.AnatomicalPathology => "Patologi Anatomi",
            LabDiscipline.Microbiology => "Mikrobiologi",
            _ => discipline.ToString()
        };

        private static string? Normalize(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }
}
