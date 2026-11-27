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
    /// Setelan penyerahan hasil kepada pasien (<c>BE-LAB-104</c>, <c>LAB-API-v1</c> <c>r51</c> bagian
    /// 46.3): aktivasi per disiplin dan daftar pemeriksaan sensitif (<c>LAB-DEC-294</c>..<c>296</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Aktivasi</b> adalah tabel peristiwa: setiap perubahan satu baris baru, keadaan kini = baris
    /// terbaru per disiplin, tanpa baris = nonaktif (<c>INV-80</c>). Bawaan nonaktif adalah keadaan
    /// yang benar sampai Kepala Instalasi mengonfirmasi (<c>LAB-SIGN-002</c>).
    /// </para>
    /// <para>
    /// Hanya Patologi Klinik dan Mikrobiologi (<c>LAB-DEC-288</c>). Gerbang penyerahan yang membaca
    /// setelan ini menyusul pada <c>BE-LAB-106</c>.
    /// </para>
    /// </remarks>
    public class LabPatientDeliverySettingService
    {
        public const string VerificationBaseUrlKey = "LabPatientResult:VerificationBaseUrl";

        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private static readonly LabDiscipline[] DeliverableDisciplines =
        {
            LabDiscipline.ClinicalPathology,
            LabDiscipline.Microbiology
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly LoggerService _loggerService;

        public LabPatientDeliverySettingService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _loggerService = loggerService;
        }

        /// <summary>Keadaan aktivasi PK dan Mikrobiologi, daftar sensitif aktif, dan kesiapan alamat verifikasi.</summary>
        public async Task<LabPatientDeliverySettingResponse> GetAsync(CancellationToken cancellationToken = default)
        {
            var latest = await ReadLatestActivationsAsync(cancellationToken);

            var disciplines = DeliverableDisciplines
                .Select(discipline =>
                {
                    latest.TryGetValue(discipline, out var row);
                    return new LabPatientDeliveryDisciplineResponse
                    {
                        Discipline = discipline.ToString(),
                        DisciplineName = LabFilterMetadataFactory.LabelDisiplin(discipline),
                        Enabled = row?.Enabled ?? false,
                        ChangedAt = row?.ChangedAt,
                        ChangedByName = row?.ChangedByName,
                        Reason = row?.Reason
                    };
                })
                .ToList();

            var sensitive = await _dbContext.LabSensitiveProcedures
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.Procedure!.ProcedureName)
                .Select(x => new LabSensitiveProcedureResponse
                {
                    Id = x.Id,
                    ProcedureId = x.ProcedureId,
                    ProcedureCode = x.Procedure!.ProcedureCode,
                    ProcedureName = x.Procedure!.ProcedureName,
                    Note = x.Note,
                    MarkedByName = _dbContext.Users
                        .Where(u => u.Id == x.CreateBy)
                        .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                        .FirstOrDefault(),
                    MarkedAt = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            return new LabPatientDeliverySettingResponse
            {
                Disciplines = disciplines,
                SensitiveProcedures = sensitive,
                VerificationBaseUrlConfigured = IsVerificationBaseUrlConfigured()
            };
        }

        /// <summary>
        /// Mengaktifkan atau menonaktifkan penyerahan untuk satu disiplin (<c>VAL-178</c>). Urutan
        /// penilaian mengikuti kontrak: disiplin → alasan → penegasan daftar sensitif → keadaan sama →
        /// alamat verifikasi. Menonaktifkan tidak menyentuh dokumen, persetujuan, maupun catatan kirim.
        /// </summary>
        public async Task<LabPatientDeliverySettingResponse> ChangeActivationAsync(
            LabDiscipline discipline,
            ChangeLabPatientDeliveryActivationRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            EnsureDeliverableDiscipline(discipline);

            var reason = request.Reason?.Trim();
            if (string.IsNullOrEmpty(reason))
                throw new LabPatientResultValidationException("Tuliskan alasan perubahan.");

            if (request.Enabled && !request.SensitiveListConfirmed)
                throw new LabPatientResultValidationException(
                    "Tegaskan bahwa daftar pemeriksaan sensitif sudah ditetapkan.");

            var latest = await ReadLatestActivationsAsync(cancellationToken);
            var currentlyEnabled = latest.TryGetValue(discipline, out var current) && current.Enabled;
            var label = LabFilterMetadataFactory.LabelDisiplin(discipline);

            if (currentlyEnabled == request.Enabled)
                throw new LabPatientResultConflictException(
                    "NoChange",
                    $"Penyerahan {label} sudah {(request.Enabled ? "aktif" : "nonaktif")}.");

            if (request.Enabled && !IsVerificationBaseUrlConfigured())
                throw new LabPatientResultConflictException(
                    "VerificationUrlMissing",
                    "Alamat verifikasi QR belum dikonfigurasi; hubungi admin platform.");

            var actorUserId = RequireCurrentUserId();
            var now = DateTime.UtcNow;

            _dbContext.LabPatientDeliveryActivations.Add(new LabPatientDeliveryActivation
            {
                Discipline = discipline,
                Enabled = request.Enabled,
                Reason = reason,
                SensitiveListConfirmed = request.Enabled && request.SensitiveListConfirmed,
                ChangedByUserId = actorUserId,
                ChangedAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId
            });

            await _dbContext.SaveChangesAsync(cancellationToken);

            // LoggerService tidak menulis payload — isi di teks pesan. Alasan tidak dicatat.
            await _loggerService.AuditAsync(
                LogCategory,
                "LabPatientDelivery.ChangeActivation",
                $"Penyerahan hasil {label} {(request.Enabled ? "diaktifkan" : "dinonaktifkan")} oleh pengguna {actorUserId}.");

            return await GetAsync(cancellationToken);
        }

        /// <summary>Jejak perubahan aktivasi satu disiplin, terbaru di atas.</summary>
        public async Task<List<LabPatientDeliveryActivationResponse>> GetActivationHistoryAsync(
            LabDiscipline discipline,
            CancellationToken cancellationToken = default)
        {
            EnsureDeliverableDiscipline(discipline);

            return await _dbContext.LabPatientDeliveryActivations
                .AsNoTracking()
                .Where(x => x.Discipline == discipline && !x.IsDelete)
                .OrderByDescending(x => x.ChangedAt)
                .Select(x => new LabPatientDeliveryActivationResponse
                {
                    Id = x.Id,
                    Discipline = x.Discipline.ToString(),
                    Enabled = x.Enabled,
                    Reason = x.Reason,
                    SensitiveListConfirmed = x.SensitiveListConfirmed,
                    ChangedByUserId = x.ChangedByUserId,
                    ChangedByName = _dbContext.Users
                        .Where(u => u.Id == x.ChangedByUserId)
                        .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                        .FirstOrDefault(),
                    ChangedAt = x.ChangedAt
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>Menandai satu jenis pemeriksaan sebagai sensitif (<c>VAL-179</c>).</summary>
        public async Task<LabSensitiveProcedureResponse> AddSensitiveProcedureAsync(
            AddLabSensitiveProcedureRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var procedureExists = await _dbContext.MstProcedures
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.ProcedureId && !x.IsDelete, cancellationToken);
            if (!procedureExists)
                throw new KeyNotFoundException("Jenis pemeriksaan tidak ditemukan.");

            var alreadyMarked = await _dbContext.LabSensitiveProcedures
                .AsNoTracking()
                .AnyAsync(x => x.ProcedureId == request.ProcedureId && !x.IsDelete, cancellationToken);
            if (alreadyMarked)
                throw new LabPatientResultConflictException("AlreadyMarked", AlreadyMarkedMessage);

            var actorUserId = RequireCurrentUserId();
            var row = new LabSensitiveProcedure
            {
                ProcedureId = request.ProcedureId,
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.LabSensitiveProcedures.Add(row);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Dua penandaan bersamaan: index unik tersaring menolak yang kedua.
                _dbContext.Entry(row).State = EntityState.Detached;
                if (await IsMarkedAsync(request.ProcedureId, row.Id, cancellationToken))
                    throw new LabPatientResultConflictException("AlreadyMarked", AlreadyMarkedMessage);
                throw;
            }

            await _loggerService.AuditAsync(
                LogCategory,
                "LabPatientDelivery.AddSensitiveProcedure",
                $"Jenis pemeriksaan {request.ProcedureId} ditandai sensitif oleh pengguna {actorUserId}.");

            return await ReadSensitiveAsync(row.Id, cancellationToken);
        }

        /// <summary>Mencabut penanda sensitif, beralasan (<c>VAL-179</c>).</summary>
        public async Task RemoveSensitiveProcedureAsync(
            Guid id,
            RemoveLabSensitiveProcedureRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var row = await _dbContext.LabSensitiveProcedures
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Penanda pemeriksaan sensitif tidak ditemukan.");

            var reason = request.Reason?.Trim();
            if (string.IsNullOrEmpty(reason))
                throw new LabPatientResultValidationException("Tuliskan alasan pencabutan.");

            var actorUserId = RequireCurrentUserId();
            row.IsDelete = true;
            row.DeleteDateTime = DateTime.UtcNow;
            row.DeleteBy = actorUserId;
            row.RemovalReason = reason;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "LabPatientDelivery.RemoveSensitiveProcedure",
                $"Penanda sensitif jenis pemeriksaan {row.ProcedureId} dicabut oleh pengguna {actorUserId}.");
        }

        private const string AlreadyMarkedMessage = "Pemeriksaan ini sudah ditandai sensitif.";

        private bool IsVerificationBaseUrlConfigured() =>
            !string.IsNullOrWhiteSpace(_configuration[VerificationBaseUrlKey]);

        private static void EnsureDeliverableDiscipline(LabDiscipline discipline)
        {
            if (discipline == LabDiscipline.AnatomicalPathology)
                throw new LabPatientResultValidationException("Penyerahan hasil Patologi Anatomi belum tersedia.");

            if (!DeliverableDisciplines.Contains(discipline))
                throw new LabPatientResultValidationException("Disiplin itu tidak dikenal.");
        }

        private async Task<Dictionary<LabDiscipline, ActivationRow>> ReadLatestActivationsAsync(
            CancellationToken cancellationToken)
        {
            // Dua disiplin, sedikit baris per disiplin: baca seluruh jejak lalu ambil yang terbaru.
            var rows = await _dbContext.LabPatientDeliveryActivations
                .AsNoTracking()
                .Where(x => !x.IsDelete && DeliverableDisciplines.Contains(x.Discipline))
                .OrderByDescending(x => x.ChangedAt)
                .Select(x => new ActivationRow(
                    x.Discipline,
                    x.Enabled,
                    x.ChangedAt,
                    x.Reason,
                    _dbContext.Users
                        .Where(u => u.Id == x.ChangedByUserId)
                        .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                        .FirstOrDefault()))
                .ToListAsync(cancellationToken);

            return rows
                .GroupBy(x => x.Discipline)
                .ToDictionary(g => g.Key, g => g.First());
        }

        private async Task<bool> IsMarkedAsync(Guid procedureId, Guid exceptId, CancellationToken cancellationToken) =>
            await _dbContext.LabSensitiveProcedures
                .AsNoTracking()
                .AnyAsync(x => x.ProcedureId == procedureId && x.Id != exceptId && !x.IsDelete, cancellationToken);

        private async Task<LabSensitiveProcedureResponse> ReadSensitiveAsync(Guid id, CancellationToken cancellationToken) =>
            await _dbContext.LabSensitiveProcedures
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new LabSensitiveProcedureResponse
                {
                    Id = x.Id,
                    ProcedureId = x.ProcedureId,
                    ProcedureCode = x.Procedure!.ProcedureCode,
                    ProcedureName = x.Procedure!.ProcedureName,
                    Note = x.Note,
                    MarkedByName = _dbContext.Users
                        .Where(u => u.Id == x.CreateBy)
                        .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                        .FirstOrDefault(),
                    MarkedAt = x.CreateDateTime
                })
                .FirstAsync(cancellationToken);

        private Guid RequireCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            // Pelaku wajib orang nyata — FK ke pengguna (02-backend-architecture.md 13.7).
            return Guid.TryParse(value, out var userId) && userId != Guid.Empty
                ? userId
                : throw new UnauthorizedAccessException("Pengguna tidak dikenali.");
        }

        private sealed record ActivationRow(
            LabDiscipline Discipline,
            bool Enabled,
            DateTime ChangedAt,
            string Reason,
            string? ChangedByName);
    }

    /// <summary>Keadaan tidak mengizinkan tindakan penyerahan hasil → <c>409</c> dengan <see cref="Code"/>.</summary>
    public sealed class LabPatientResultConflictException(string code, string message) : Exception(message)
    {
        public string Code { get; } = code;
    }

    /// <summary>Isian tidak sah pada tindakan penyerahan hasil → <c>422</c>.</summary>
    public sealed class LabPatientResultValidationException(string message) : Exception(message);
}
