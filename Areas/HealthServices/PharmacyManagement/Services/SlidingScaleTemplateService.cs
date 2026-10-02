using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    /// <summary>
    /// Hasil operasi sliding scale beserta kode status yang sudah ditetapkan kontrak.
    /// </summary>
    public sealed class SlidingScaleResult<T>
    {
        public bool IsSuccess { get; private init; }
        public int StatusCode { get; private init; }
        public string Message { get; private init; } = string.Empty;
        public T? Data { get; private init; }

        public static SlidingScaleResult<T> Ok(T data, string message, int statusCode = StatusCodes.Status200OK)
            => new() { IsSuccess = true, StatusCode = statusCode, Data = data, Message = message };

        public static SlidingScaleResult<T> Fail(int statusCode, string message)
            => new() { IsSuccess = false, StatusCode = statusCode, Message = message };
    }

    /// <summary>
    /// Template dan versi protokol sliding scale — <c>BE-RWI-102</c>, <c>FR-DOK-094</c>,
    /// <c>FR-DOK-095</c>, <c>RWI-DEC-146</c>, <c>RWI-DEC-147</c>, state matrix 0.6.0 bagian 8.6.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Proses bisnisnya.</b> Pengubah konfigurasi farmasi-klinis menyimpan versi <c>Draft</c> berisi
    /// rentang gula darah dan dosis insulin. Pengguna lain yang memegang hak <c>Approve</c> mengesahkan
    /// versi itu. Pengesahan memensiunkan versi sah sebelumnya pada transaksi yang sama, sehingga pada
    /// setiap saat tepat satu versi dapat dipesan.
    /// </para>
    /// <para>
    /// <b>Pengesah bukan pengubah terakhir</b> dijaga dari data versi, bukan dari nama jabatan:
    /// siapa pengubah dan siapa pengesah diberikan admin lewat layar Akses Role
    /// (<c>SlidingScaleTemplate : Update</c> dan <c>: Approve</c>).
    /// </para>
    /// <para>
    /// <b>Gerbang produksi yang masih terbuka — <c>RWI-OQ-097</c>.</b> Nama pengesah isi protokol belum
    /// ditunjuk. Mesin ini dapat dibangun dan diuji, tetapi di lingkungan produksi nol versi dapat
    /// dinaikkan menjadi <c>Approved</c> sampai hak <c>Approve</c> diberikan kepada orang yang ditunjuk.
    /// </para>
    /// <para>
    /// <b>Contoh.</b> Senin 09.00 Andi menyimpan draft v2 "SSI-DEWASA". Senin 09.30 Andi mencoba
    /// mengesahkan → <c>403</c> "Versi ini terakhir diubah oleh Anda…". Selasa 10.00 Ns. Wati mengesahkan
    /// → v2 <c>Approved</c>, v1 <c>Retired</c> pada transaksi yang sama. Rabu Andi mencoba mengubah v2 →
    /// <c>409</c> "Versi yang sudah disahkan tidak dapat diubah. Buat versi baru."
    /// </para>
    /// </remarks>
    public class SlidingScaleTemplateService
    {
        private const string LogCategory = "HealthServices.Pharmacy.SlidingScaleTemplate";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public SlidingScaleTemplateService(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        // =====================================================================
        // Baca
        // =====================================================================

        public async Task<List<SlidingScaleTemplateListItem>> ListAsync(
            bool? isActive,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<PhmSlidingScaleTemplate>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            return await query
                .OrderBy(x => x.TemplateName)
                .Select(x => new SlidingScaleTemplateListItem
                {
                    Id = x.Id,
                    TemplateCode = x.TemplateCode,
                    TemplateName = x.TemplateName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    HasApprovedVersion = x.Versions.Any(v => !v.IsDelete && v.VersionStatus == SlidingScaleVersionStatus.Approved),
                    ApprovedVersionId = x.Versions
                        .Where(v => !v.IsDelete && v.VersionStatus == SlidingScaleVersionStatus.Approved)
                        .Select(v => (Guid?)v.Id)
                        .FirstOrDefault(),
                    ApprovedVersionNumber = x.Versions
                        .Where(v => !v.IsDelete && v.VersionStatus == SlidingScaleVersionStatus.Approved)
                        .Select(v => (int?)v.VersionNumber)
                        .FirstOrDefault(),
                    ApprovedGlucoseUnit = x.Versions
                        .Where(v => !v.IsDelete && v.VersionStatus == SlidingScaleVersionStatus.Approved)
                        .Select(v => (BloodGlucoseUnit?)v.GlucoseUnit)
                        .FirstOrDefault(),
                    ApprovedAt = x.Versions
                        .Where(v => !v.IsDelete && v.VersionStatus == SlidingScaleVersionStatus.Approved)
                        .Select(v => v.ApprovedAt)
                        .FirstOrDefault(),
                    LatestVersionNumber = x.Versions.Where(v => !v.IsDelete).Select(v => (int?)v.VersionNumber).Max() ?? 0
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<SlidingScaleResult<SlidingScaleTemplateResponse>> GetAsync(
            Guid templateId,
            CancellationToken cancellationToken = default)
        {
            var template = await _dbContext.Set<PhmSlidingScaleTemplate>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == templateId && !x.IsDelete, cancellationToken);

            if (template == null)
            {
                return SlidingScaleResult<SlidingScaleTemplateResponse>.Fail(
                    StatusCodes.Status404NotFound, "Template sliding scale tidak ditemukan.");
            }

            var versions = await _dbContext.Set<PhmSlidingScaleTemplateVersion>()
                .AsNoTracking()
                .Include(x => x.Ranges)
                .Include(x => x.LastModifiedByUser)
                .Include(x => x.ApprovedByUser)
                .Where(x => x.TemplateId == templateId && !x.IsDelete)
                .OrderByDescending(x => x.VersionNumber)
                .ToListAsync(cancellationToken);

            return SlidingScaleResult<SlidingScaleTemplateResponse>.Ok(new SlidingScaleTemplateResponse
            {
                Id = template.Id,
                TemplateCode = template.TemplateCode,
                TemplateName = template.TemplateName,
                Description = template.Description,
                IsActive = template.IsActive,
                CreateDateTime = template.CreateDateTime,
                Versions = versions.Select(ToVersionResponse).ToList()
            }, "Template sliding scale berhasil diambil.");
        }

        // =====================================================================
        // Tulis
        // =====================================================================

        public async Task<SlidingScaleResult<SlidingScaleTemplateResponse>> CreateTemplateAsync(
            CreateSlidingScaleTemplateRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var kode = request.TemplateCode?.Trim().ToUpperInvariant();
            var nama = request.TemplateName?.Trim();

            if (string.IsNullOrWhiteSpace(kode) || string.IsNullOrWhiteSpace(nama))
            {
                return SlidingScaleResult<SlidingScaleTemplateResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Kode dan nama template wajib diisi.");
            }

            var kodeDipakai = await _dbContext.Set<PhmSlidingScaleTemplate>()
                .AsNoTracking()
                .AnyAsync(x => x.TemplateCode == kode, cancellationToken);

            if (kodeDipakai)
            {
                return SlidingScaleResult<SlidingScaleTemplateResponse>.Fail(
                    StatusCodes.Status409Conflict, $"Kode template {kode} sudah dipakai.");
            }

            var now = DateTime.UtcNow;
            var entity = new PhmSlidingScaleTemplate
            {
                Id = Guid.NewGuid(),
                TemplateCode = kode,
                TemplateName = nama,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<PhmSlidingScaleTemplate>().Add(entity);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return SlidingScaleResult<SlidingScaleTemplateResponse>.Fail(
                    StatusCodes.Status409Conflict, $"Kode template {kode} sudah dipakai.");
            }

            await _loggerService.InfoAsync(LogCategory, "SlidingScaleTemplate.Create",
                "Membuat template sliding scale.", new { entity.Id, entity.TemplateCode });

            return SlidingScaleResult<SlidingScaleTemplateResponse>.Ok(new SlidingScaleTemplateResponse
            {
                Id = entity.Id,
                TemplateCode = entity.TemplateCode,
                TemplateName = entity.TemplateName,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime
            }, "Template sliding scale berhasil dibuat.", StatusCodes.Status201Created);
        }

        /// <summary>
        /// Membuat versi <c>Draft</c> baru beserta rentangnya.
        /// </summary>
        public async Task<SlidingScaleResult<SlidingScaleVersionResponse>> CreateVersionAsync(
            Guid templateId,
            SaveSlidingScaleVersionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var templateAda = await _dbContext.Set<PhmSlidingScaleTemplate>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == templateId && !x.IsDelete, cancellationToken);

            if (!templateAda)
            {
                return SlidingScaleResult<SlidingScaleVersionResponse>.Fail(
                    StatusCodes.Status404NotFound, "Template sliding scale tidak ditemukan.");
            }

            var validasi = SlidingScaleRangeValidator.Validate(request.Ranges, request.GlucoseUnit);

            if (!validasi.IsValid)
            {
                return SlidingScaleResult<SlidingScaleVersionResponse>.Fail(
                    StatusCodes.Status400BadRequest, validasi.ErrorMessage!);
            }

            // Nomor versi berikutnya pada satu template. Bukan nomor bisnis lintas modul; dua draft yang
            // lahir bersamaan dijaga index unik (TemplateId, VersionNumber) dan dijawab 409, bukan
            // diam-diam menjadi dua versi bernomor sama.
            var nomorTerakhir = await _dbContext.Set<PhmSlidingScaleTemplateVersion>()
                .AsNoTracking()
                .Where(x => x.TemplateId == templateId)
                .Select(x => (int?)x.VersionNumber)
                .MaxAsync(cancellationToken) ?? 0;

            var now = DateTime.UtcNow;
            var version = new PhmSlidingScaleTemplateVersion
            {
                Id = Guid.NewGuid(),
                TemplateId = templateId,
                VersionNumber = nomorTerakhir + 1,
                VersionStatus = SlidingScaleVersionStatus.Draft,
                GlucoseUnit = request.GlucoseUnit!.Value,
                LastModifiedByUserId = actorUserId,
                LastModifiedAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<PhmSlidingScaleTemplateVersion>().Add(version);
            _dbContext.Set<PhmSlidingScaleRange>().AddRange(
                SlidingScaleRangeValidator.BuildRows(validasi.OrderedRanges, version.Id, null, actorUserId, now));

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return SlidingScaleResult<SlidingScaleVersionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Versi baru template ini baru saja dibuat pengguna lain. Muat ulang lalu ulangi.");
            }

            await _loggerService.InfoAsync(LogCategory, "SlidingScaleTemplate.CreateVersion",
                "Membuat versi draft protokol sliding scale.",
                new { version.Id, version.TemplateId, version.VersionNumber, LastModifiedBy = actorUserId });

            return await ReloadVersionAsync(version.Id, "Versi draft berhasil dibuat.", StatusCodes.Status201Created, cancellationToken);
        }

        /// <summary>
        /// Mengubah versi yang masih <c>Draft</c>; pengubah terakhir dicatat ulang.
        /// </summary>
        public async Task<SlidingScaleResult<SlidingScaleVersionResponse>> UpdateVersionAsync(
            Guid versionId,
            SaveSlidingScaleVersionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var version = await _dbContext.Set<PhmSlidingScaleTemplateVersion>()
                .Include(x => x.Ranges)
                .FirstOrDefaultAsync(x => x.Id == versionId && !x.IsDelete, cancellationToken);

            if (version == null)
            {
                return SlidingScaleResult<SlidingScaleVersionResponse>.Fail(
                    StatusCodes.Status404NotFound, "Versi sliding scale tidak ditemukan.");
            }

            // VAL-DOK-54d.
            if (version.VersionStatus != SlidingScaleVersionStatus.Draft)
            {
                return SlidingScaleResult<SlidingScaleVersionResponse>.Fail(
                    StatusCodes.Status409Conflict, "Versi yang sudah disahkan tidak dapat diubah. Buat versi baru.");
            }

            var validasi = SlidingScaleRangeValidator.Validate(request.Ranges, request.GlucoseUnit);

            if (!validasi.IsValid)
            {
                return SlidingScaleResult<SlidingScaleVersionResponse>.Fail(
                    StatusCodes.Status400BadRequest, validasi.ErrorMessage!);
            }

            var now = DateTime.UtcNow;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            // Rentang draft lama ditandai terhapus, bukan dihapus fisik; hanya draft yang boleh begini,
            // karena rentang versi draft belum pernah dipakai order mana pun.
            foreach (var lama in version.Ranges.Where(x => !x.IsDelete))
            {
                lama.IsDelete = true;
                lama.DeleteDateTime = now;
                lama.DeleteBy = actorUserId;
            }

            version.GlucoseUnit = request.GlucoseUnit!.Value;
            version.LastModifiedByUserId = actorUserId;
            version.LastModifiedAt = now;
            version.UpdateDateTime = now;
            version.UpdateBy = actorUserId;

            _dbContext.Set<PhmSlidingScaleRange>().AddRange(
                SlidingScaleRangeValidator.BuildRows(validasi.OrderedRanges, version.Id, null, actorUserId, now));

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await _loggerService.InfoAsync(LogCategory, "SlidingScaleTemplate.UpdateVersion",
                "Mengubah versi draft protokol sliding scale.",
                new { version.Id, version.TemplateId, version.VersionNumber, LastModifiedBy = actorUserId });

            return await ReloadVersionAsync(version.Id, "Versi draft berhasil diubah.", StatusCodes.Status200OK, cancellationToken);
        }

        /// <summary>
        /// Mengesahkan versi <c>Draft</c> — <c>FR-DOK-095</c>, <c>VAL-DOK-54c</c>. Versi sah sebelumnya
        /// menjadi <c>Retired</c> pada transaksi yang sama (kriteria 5).
        /// </summary>
        public async Task<SlidingScaleResult<SlidingScaleVersionResponse>> ApproveVersionAsync(
            Guid versionId,
            ApproveSlidingScaleVersionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var version = await _dbContext.Set<PhmSlidingScaleTemplateVersion>()
                .Include(x => x.Ranges)
                .FirstOrDefaultAsync(x => x.Id == versionId && !x.IsDelete, cancellationToken);

            if (version == null)
            {
                return SlidingScaleResult<SlidingScaleVersionResponse>.Fail(
                    StatusCodes.Status404NotFound, "Versi sliding scale tidak ditemukan.");
            }

            if (version.VersionStatus != SlidingScaleVersionStatus.Draft)
            {
                return SlidingScaleResult<SlidingScaleVersionResponse>.Fail(
                    StatusCodes.Status409Conflict, "Versi ini sudah disahkan atau sudah diganti sehingga tidak dapat disahkan lagi.");
            }

            // VAL-DOK-54c. Kewenangan yang melekat pada data versi, bukan pada nama jabatan.
            if (version.LastModifiedByUserId == actorUserId)
            {
                return SlidingScaleResult<SlidingScaleVersionResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Versi ini terakhir diubah oleh Anda. Pengesahan harus dilakukan pengguna lain.");
            }

            var rentang = SlidingScaleRangeValidator.ToRequests(version.Ranges);
            var validasi = SlidingScaleRangeValidator.Validate(rentang, version.GlucoseUnit);

            if (!validasi.IsValid)
            {
                return SlidingScaleResult<SlidingScaleVersionResponse>.Fail(
                    StatusCodes.Status400BadRequest, validasi.ErrorMessage!);
            }

            var now = DateTime.UtcNow;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var versiSahLama = await _dbContext.Set<PhmSlidingScaleTemplateVersion>()
                .Where(x => x.TemplateId == version.TemplateId &&
                            x.Id != version.Id &&
                            !x.IsDelete &&
                            x.VersionStatus == SlidingScaleVersionStatus.Approved)
                .ToListAsync(cancellationToken);

            foreach (var lama in versiSahLama)
            {
                lama.VersionStatus = SlidingScaleVersionStatus.Retired;
                lama.RetiredAt = now;
                lama.UpdateDateTime = now;
                lama.UpdateBy = actorUserId;
            }

            // Disimpan lebih dulu: index unik parsial "satu Approved per template" diperiksa per
            // pernyataan, sehingga versi lama wajib sudah Retired sebelum versi baru menjadi Approved.
            await _dbContext.SaveChangesAsync(cancellationToken);

            version.VersionStatus = SlidingScaleVersionStatus.Approved;
            version.ApprovedByUserId = actorUserId;
            version.ApprovedAt = now;
            version.ApprovalNote = string.IsNullOrWhiteSpace(request.ApprovalNote) ? null : request.ApprovalNote.Trim();
            version.DefinitionHash = SlidingScaleRangeValidator.ComputeDefinitionHash(version.GlucoseUnit, validasi.OrderedRanges);
            version.UpdateDateTime = now;
            version.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);
                return SlidingScaleResult<SlidingScaleVersionResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Versi lain template ini baru saja disahkan. Muat ulang lalu periksa kembali.");
            }

            await _loggerService.AuditAsync(LogCategory, "SlidingScaleTemplate.ApproveVersion",
                "Mengesahkan versi protokol sliding scale.",
                new
                {
                    version.Id,
                    version.TemplateId,
                    version.VersionNumber,
                    version.DefinitionHash,
                    ApprovedBy = actorUserId,
                    version.LastModifiedByUserId,
                    RetiredVersionIds = versiSahLama.Select(x => x.Id).ToList()
                });

            return await ReloadVersionAsync(version.Id, "Versi sliding scale berhasil disahkan.", StatusCodes.Status200OK, cancellationToken);
        }

        private async Task<SlidingScaleResult<SlidingScaleVersionResponse>> ReloadVersionAsync(
            Guid versionId,
            string message,
            int statusCode,
            CancellationToken cancellationToken)
        {
            var version = await _dbContext.Set<PhmSlidingScaleTemplateVersion>()
                .AsNoTracking()
                .Include(x => x.Ranges)
                .Include(x => x.LastModifiedByUser)
                .Include(x => x.ApprovedByUser)
                .FirstAsync(x => x.Id == versionId, cancellationToken);

            return SlidingScaleResult<SlidingScaleVersionResponse>.Ok(ToVersionResponse(version), message, statusCode);
        }

        internal static SlidingScaleRangeResponse ToRangeResponse(PhmSlidingScaleRange x) => new()
        {
            Id = x.Id,
            LowerBoundInclusive = x.LowerBoundInclusive,
            UpperBoundExclusive = x.UpperBoundExclusive,
            DoseUnits = x.DoseUnits,
            InstructionText = x.InstructionText,
            RequiresPhysicianNotification = x.RequiresPhysicianNotification,
            SortOrder = x.SortOrder
        };

        private static SlidingScaleVersionResponse ToVersionResponse(PhmSlidingScaleTemplateVersion x)
        {
            var response = new SlidingScaleVersionResponse
            {
                Id = x.Id,
                TemplateId = x.TemplateId,
                VersionNumber = x.VersionNumber,
                VersionStatus = x.VersionStatus,
                GlucoseUnit = x.GlucoseUnit,
                DefinitionHash = x.DefinitionHash,
                LastModifiedByUserId = x.LastModifiedByUserId,
                LastModifiedByName = x.LastModifiedByUser?.DisplayName,
                LastModifiedAt = x.LastModifiedAt,
                ApprovedByUserId = x.ApprovedByUserId,
                ApprovedByName = x.ApprovedByUser?.DisplayName,
                ApprovedAt = x.ApprovedAt,
                RetiredAt = x.RetiredAt,
                ApprovalNote = x.ApprovalNote,
                Ranges = x.Ranges.Where(r => !r.IsDelete).OrderBy(r => r.SortOrder).Select(ToRangeResponse).ToList()
            };

            if (x.VersionStatus == SlidingScaleVersionStatus.Draft)
                response.AvailableActions.AddRange(new[] { "Update", "Approve" });

            return response;
        }
    }
}
