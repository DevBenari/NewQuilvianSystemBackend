using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Pengelolaan daftar alasan <b>mengubah hasil</b> — dipakai <i>Kembalikan ke analis</i>
    /// (<c>LAB-DEC-138</c>) dan kelak koreksi sesudah rilis (<c>LAB-DEC-082</c>). <c>r34</c>
    /// bagian 29.6, <c>BE-LAB-71</c>.
    ///
    /// <b>Kembar dengan <see cref="LabFourEyesExceptionReasonService"/>, dan kembarannya dijaga
    /// eksplisit</b> — sejajar <see cref="LabOrganismService"/> dan <see cref="LabAntibioticService"/>.
    /// Aturannya tidak disalin: seluruhnya tinggal di <see cref="LabResultReasonRules"/>.
    ///
    /// <b>Nol penghapusan.</b> Riwayat menyimpan kode alasan yang dipilih saat itu; alasan yang
    /// tidak lagi dipakai dinonaktifkan.
    /// </summary>
    public class LabResultCorrectionReasonService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";
        private const string NotFoundMessage = "Alasan pengembalian hasil tidak ditemukan.";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabResultCorrectionReasonService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        public async Task<PagedResult<LabResultReasonResponse>> GetListAsync(
            LabResultReasonPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var (pageNumber, pageSize) = LabResultReasonRules.Paging(query.PageNumber, query.PageSize);

            var source = _dbContext.LabResultCorrectionReasons.AsNoTracking().Where(x => !x.IsDelete);

            if (query.IsActive.HasValue)
                source = source.Where(x => x.IsActive == query.IsActive.Value);

            var search = LabMicrobiologyMasterDataText.Normalize(query.Search);

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = $"%{search}%";
                source = source.Where(x =>
                    EF.Functions.ILike(x.ReasonCode, pattern) ||
                    EF.Functions.ILike(x.ReasonName, pattern) ||
                    (x.Description != null && EF.Functions.ILike(x.Description, pattern)));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ReasonName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LabResultReasonResponse
                {
                    Id = x.Id,
                    ReasonCode = x.ReasonCode,
                    ReasonName = x.ReasonName,
                    Description = x.Description,
                    RequiresNote = x.RequiresNote,
                    IsActive = x.IsActive,
                    SortOrder = x.SortOrder
                })
                .ToListAsync(cancellationToken);

            return LabMicrobiologyMasterDataText.Page(pageNumber, pageSize, totalData, items);
        }

        /// <summary>Hanya alasan <b>aktif</b> — alasan nonaktif tidak dapat dipilih pada tindakan baru.</summary>
        public async Task<PagedResult<LabResultReasonOptionResponse>> GetOptionsAsync(
            CancellationToken cancellationToken = default)
        {
            var items = await _dbContext.LabResultCorrectionReasons
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ReasonName)
                .Select(x => new LabResultReasonOptionResponse
                {
                    Id = x.Id,
                    ReasonCode = x.ReasonCode,
                    ReasonName = x.ReasonName,
                    RequiresNote = x.RequiresNote,
                    SortOrder = x.SortOrder
                })
                .ToListAsync(cancellationToken);

            return LabResultReasonRules.SinglePage(items);
        }

        public async Task<LabResultReasonSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var source = _dbContext.LabResultCorrectionReasons.AsNoTracking().Where(x => !x.IsDelete);

            var total = await source.CountAsync(cancellationToken);
            var aktif = await source.CountAsync(x => x.IsActive, cancellationToken);
            var wajibCatatan = await source.CountAsync(x => x.RequiresNote, cancellationToken);

            return new LabResultReasonSummaryResponse
            {
                TotalReason = total,
                ActiveReason = aktif,
                InactiveReason = total - aktif,
                RequiresNoteReason = wajibCatatan
            };
        }

        public async Task<LabResultReasonResponse> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabResultCorrectionReasons
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException(NotFoundMessage);

            return Map(entity);
        }

        public async Task<LabResultReasonResponse> CreateAsync(
            LabResultReasonCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var code = LabResultReasonRules.ValidateCode(request.ReasonCode);
            var name = LabResultReasonRules.ValidateName(request.ReasonName);
            var description = LabResultReasonRules.ValidateDescription(request.Description);

            // VAL-140. Diperiksa di sini supaya pemanggil menerima pesan yang berarti; index unik
            // parsial tetap menjadi penjaga terakhir bila dua permintaan datang bersamaan.
            var duplicate = await _dbContext.LabResultCorrectionReasons
                .AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.ReasonCode == code, cancellationToken);

            if (duplicate)
                throw new LabResultReasonConflictException(LabResultReasonRules.DuplicateMessage);

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            // RequiresNote sengaja tidak dibaca dari permintaan: hanya system-flags yang
            // menyetelnya.
            var entity = new LabResultCorrectionReason
            {
                ReasonCode = code,
                ReasonName = name,
                Description = description,
                SortOrder = request.SortOrder,
                RequiresNote = false,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.LabResultCorrectionReasons.Add(entity);
            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabResultCorrectionReason.Create",
                "Menambah alasan pengembalian hasil.",
                new { entity.Id, entity.ReasonCode, entity.ReasonName, entity.SortOrder, ActorUserId = actorUserId });

            return Map(entity);
        }

        /// <summary>Mengubah nama, keterangan, dan urutan. Kode dan status tidak berubah di sini.</summary>
        public async Task<LabResultReasonResponse> UpdateAsync(
            Guid id,
            LabResultReasonUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _dbContext.LabResultCorrectionReasons
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException(NotFoundMessage);

            LabResultReasonRules.EnsureCodeUnchanged(entity.ReasonCode, request.ReasonCode);

            var name = LabResultReasonRules.ValidateName(request.ReasonName);
            var description = LabResultReasonRules.ValidateDescription(request.Description);
            var actorUserId = GetCurrentUserId();

            entity.ReasonName = name;
            entity.Description = description;
            entity.SortOrder = request.SortOrder;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabResultCorrectionReason.Update",
                "Mengubah alasan pengembalian hasil.",
                new { entity.Id, entity.ReasonCode, entity.ReasonName, entity.SortOrder, ActorUserId = actorUserId });

            return Map(entity);
        }

        /// <summary>Penonaktifan, <b>bukan</b> penghapusan. Riwayat lama tetap terbaca.</summary>
        public async Task<LabResultReasonResponse> SetStatusAsync(
            Guid id,
            bool isActive,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabResultCorrectionReasons
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException(NotFoundMessage);

            var actorUserId = GetCurrentUserId();

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabResultCorrectionReason.SetStatus",
                isActive ? "Mengaktifkan alasan pengembalian hasil." : "Menonaktifkan alasan pengembalian hasil.",
                new { entity.Id, entity.ReasonCode, entity.IsActive, ActorUserId = actorUserId });

            return Map(entity);
        }

        /// <summary>
        /// Menyetel <c>RequiresNote</c>. Wewenangnya ditegakkan
        /// <c>[AccessPermission("LabResultCorrectionReason", "SystemFlag")]</c> — admin sistem saja
        /// (pola <c>LAB-DEC-019</c>, diadopsi <c>LAB-DEC-082</c>).
        /// </summary>
        public async Task<LabResultReasonResponse> SetSystemFlagsAsync(
            Guid id,
            LabResultReasonSystemFlagsRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _dbContext.LabResultCorrectionReasons
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException(NotFoundMessage);

            var sebelumnya = entity.RequiresNote;
            var actorUserId = GetCurrentUserId();

            entity.RequiresNote = request.RequiresNote;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabResultCorrectionReason.SetSystemFlags",
                "Menyetel penanda wajib catatan pada alasan pengembalian hasil.",
                new { entity.Id, entity.ReasonCode, PreviousRequiresNote = sebelumnya, entity.RequiresNote, ActorUserId = actorUserId });

            return Map(entity);
        }

        private async Task SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (LabMicrobiologyMasterDataText.IsUniqueViolation(exception))
            {
                throw new LabResultReasonConflictException(LabResultReasonRules.DuplicateMessage);
            }
        }

        private Guid GetCurrentUserId() =>
            LabMicrobiologyMasterDataText.ResolveActor(_httpContextAccessor);

        private static LabResultReasonResponse Map(LabResultCorrectionReason x) =>
            new()
            {
                Id = x.Id,
                ReasonCode = x.ReasonCode,
                ReasonName = x.ReasonName,
                Description = x.Description,
                RequiresNote = x.RequiresNote,
                IsActive = x.IsActive,
                SortOrder = x.SortOrder
            };
    }

    /// <summary>
    /// Pengelolaan daftar alasan <b>merangkap peran</b> pada hasil yang sama — pemvalidasi yang
    /// juga pengisi, atau perilis yang juga pemvalidasi (<c>LAB-DEC-003</c>, <c>INV-42</c>,
    /// <c>INV-43</c>). <c>r34</c> bagian 29.6, <c>BE-LAB-71</c>.
    ///
    /// Kembar dengan <see cref="LabResultCorrectionReasonService"/>; tabelnya sengaja terpisah
    /// (<c>LAB-DA-001</c> A5.4). Nama alasan ikut tercetak pada penanda pengecualian, dan hasil
    /// yang memakainya menyalin namanya saat itu — mengubah nama di sini tidak mengubah hasil lama.
    /// </summary>
    public class LabFourEyesExceptionReasonService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";
        private const string NotFoundMessage = "Alasan pengecualian empat mata tidak ditemukan.";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabFourEyesExceptionReasonService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        public async Task<PagedResult<LabResultReasonResponse>> GetListAsync(
            LabResultReasonPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var (pageNumber, pageSize) = LabResultReasonRules.Paging(query.PageNumber, query.PageSize);

            var source = _dbContext.LabFourEyesExceptionReasons.AsNoTracking().Where(x => !x.IsDelete);

            if (query.IsActive.HasValue)
                source = source.Where(x => x.IsActive == query.IsActive.Value);

            var search = LabMicrobiologyMasterDataText.Normalize(query.Search);

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = $"%{search}%";
                source = source.Where(x =>
                    EF.Functions.ILike(x.ReasonCode, pattern) ||
                    EF.Functions.ILike(x.ReasonName, pattern) ||
                    (x.Description != null && EF.Functions.ILike(x.Description, pattern)));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ReasonName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LabResultReasonResponse
                {
                    Id = x.Id,
                    ReasonCode = x.ReasonCode,
                    ReasonName = x.ReasonName,
                    Description = x.Description,
                    RequiresNote = x.RequiresNote,
                    IsActive = x.IsActive,
                    SortOrder = x.SortOrder
                })
                .ToListAsync(cancellationToken);

            return LabMicrobiologyMasterDataText.Page(pageNumber, pageSize, totalData, items);
        }

        /// <summary>Hanya alasan <b>aktif</b>. Dipakai layar Validasi dan Rilis ketika pelaku merangkap peran.</summary>
        public async Task<PagedResult<LabResultReasonOptionResponse>> GetOptionsAsync(
            CancellationToken cancellationToken = default)
        {
            var items = await _dbContext.LabFourEyesExceptionReasons
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ReasonName)
                .Select(x => new LabResultReasonOptionResponse
                {
                    Id = x.Id,
                    ReasonCode = x.ReasonCode,
                    ReasonName = x.ReasonName,
                    RequiresNote = x.RequiresNote,
                    SortOrder = x.SortOrder
                })
                .ToListAsync(cancellationToken);

            return LabResultReasonRules.SinglePage(items);
        }

        public async Task<LabResultReasonSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var source = _dbContext.LabFourEyesExceptionReasons.AsNoTracking().Where(x => !x.IsDelete);

            var total = await source.CountAsync(cancellationToken);
            var aktif = await source.CountAsync(x => x.IsActive, cancellationToken);
            var wajibCatatan = await source.CountAsync(x => x.RequiresNote, cancellationToken);

            return new LabResultReasonSummaryResponse
            {
                TotalReason = total,
                ActiveReason = aktif,
                InactiveReason = total - aktif,
                RequiresNoteReason = wajibCatatan
            };
        }

        public async Task<LabResultReasonResponse> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabFourEyesExceptionReasons
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException(NotFoundMessage);

            return Map(entity);
        }

        public async Task<LabResultReasonResponse> CreateAsync(
            LabResultReasonCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var code = LabResultReasonRules.ValidateCode(request.ReasonCode);
            var name = LabResultReasonRules.ValidateName(request.ReasonName);
            var description = LabResultReasonRules.ValidateDescription(request.Description);

            // VAL-140.
            var duplicate = await _dbContext.LabFourEyesExceptionReasons
                .AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.ReasonCode == code, cancellationToken);

            if (duplicate)
                throw new LabResultReasonConflictException(LabResultReasonRules.DuplicateMessage);

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new LabFourEyesExceptionReason
            {
                ReasonCode = code,
                ReasonName = name,
                Description = description,
                SortOrder = request.SortOrder,
                RequiresNote = false,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.LabFourEyesExceptionReasons.Add(entity);
            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabFourEyesExceptionReason.Create",
                "Menambah alasan pengecualian empat mata.",
                new { entity.Id, entity.ReasonCode, entity.ReasonName, entity.SortOrder, ActorUserId = actorUserId });

            return Map(entity);
        }

        /// <summary>Mengubah nama, keterangan, dan urutan. Kode dan status tidak berubah di sini.</summary>
        public async Task<LabResultReasonResponse> UpdateAsync(
            Guid id,
            LabResultReasonUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _dbContext.LabFourEyesExceptionReasons
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException(NotFoundMessage);

            LabResultReasonRules.EnsureCodeUnchanged(entity.ReasonCode, request.ReasonCode);

            var name = LabResultReasonRules.ValidateName(request.ReasonName);
            var description = LabResultReasonRules.ValidateDescription(request.Description);
            var actorUserId = GetCurrentUserId();

            entity.ReasonName = name;
            entity.Description = description;
            entity.SortOrder = request.SortOrder;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabFourEyesExceptionReason.Update",
                "Mengubah alasan pengecualian empat mata.",
                new { entity.Id, entity.ReasonCode, entity.ReasonName, entity.SortOrder, ActorUserId = actorUserId });

            return Map(entity);
        }

        /// <summary>Penonaktifan, <b>bukan</b> penghapusan. Hasil yang sudah memakainya tidak tersentuh.</summary>
        public async Task<LabResultReasonResponse> SetStatusAsync(
            Guid id,
            bool isActive,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabFourEyesExceptionReasons
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException(NotFoundMessage);

            var actorUserId = GetCurrentUserId();

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabFourEyesExceptionReason.SetStatus",
                isActive ? "Mengaktifkan alasan pengecualian empat mata." : "Menonaktifkan alasan pengecualian empat mata.",
                new { entity.Id, entity.ReasonCode, entity.IsActive, ActorUserId = actorUserId });

            return Map(entity);
        }

        /// <summary>
        /// Menyetel <c>RequiresNote</c> — admin sistem saja, lewat
        /// <c>[AccessPermission("LabFourEyesExceptionReason", "SystemFlag")]</c>. Bagi alasan
        /// pengecualian, polanya masih <b>usulan</b> (<c>ARCH-GAP-LAB-08</c> butir 4).
        /// </summary>
        public async Task<LabResultReasonResponse> SetSystemFlagsAsync(
            Guid id,
            LabResultReasonSystemFlagsRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _dbContext.LabFourEyesExceptionReasons
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException(NotFoundMessage);

            var sebelumnya = entity.RequiresNote;
            var actorUserId = GetCurrentUserId();

            entity.RequiresNote = request.RequiresNote;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabFourEyesExceptionReason.SetSystemFlags",
                "Menyetel penanda wajib catatan pada alasan pengecualian empat mata.",
                new { entity.Id, entity.ReasonCode, PreviousRequiresNote = sebelumnya, entity.RequiresNote, ActorUserId = actorUserId });

            return Map(entity);
        }

        private async Task SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (LabMicrobiologyMasterDataText.IsUniqueViolation(exception))
            {
                throw new LabResultReasonConflictException(LabResultReasonRules.DuplicateMessage);
            }
        }

        private Guid GetCurrentUserId() =>
            LabMicrobiologyMasterDataText.ResolveActor(_httpContextAccessor);

        private static LabResultReasonResponse Map(LabFourEyesExceptionReason x) =>
            new()
            {
                Id = x.Id,
                ReasonCode = x.ReasonCode,
                ReasonName = x.ReasonName,
                Description = x.Description,
                RequiresNote = x.RequiresNote,
                IsActive = x.IsActive,
                SortOrder = x.SortOrder
            };
    }

    /// <summary>
    /// Aturan kedua data induk alasan, ditulis <b>satu kali</b> (<c>LAB-VAL-v1</c> <c>r12</c>
    /// bagian 14.4). Dua salinan aturan yang sama pasti bercabang.
    /// </summary>
    internal static class LabResultReasonRules
    {
        /// <summary><c>VAL-140</c>.</summary>
        public const string DuplicateMessage = "Kode alasan ini sudah dipakai.";

        private const int MaxCodeLength = 32;
        private const int MaxNameLength = 200;
        private const int MaxDescriptionLength = 256;

        private static readonly Regex CodePattern = new("^[A-Z0-9-]+$", RegexOptions.Compiled);

        /// <summary>
        /// <c>VAL-141</c>. Kode berhuruf kecil <b>ditolak</b>, tidak dinormalkan diam-diam: kode
        /// adalah kunci laporan mutu, dan petugas harus melihat persis kode yang tersimpan.
        /// </summary>
        public static string ValidateCode(string? value)
        {
            var code = value?.Trim() ?? string.Empty;

            if (code.Length == 0)
                throw new LabResultReasonValidationException("Kode alasan wajib diisi.");

            if (code.Length > MaxCodeLength)
                throw new LabResultReasonValidationException($"Kode alasan paling panjang {MaxCodeLength} karakter.");

            if (!CodePattern.IsMatch(code))
                throw new LabResultReasonValidationException(
                    "Kode alasan hanya boleh berisi huruf besar, angka, dan tanda hubung.");

            return code;
        }

        /// <summary><c>VAL-141</c>.</summary>
        public static string ValidateName(string? value)
        {
            var name = LabMicrobiologyMasterDataText.Normalize(value);

            if (string.IsNullOrEmpty(name))
                throw new LabResultReasonValidationException("Nama alasan wajib diisi.");

            if (name.Length > MaxNameLength)
                throw new LabResultReasonValidationException($"Nama alasan paling panjang {MaxNameLength} karakter.");

            return name;
        }

        /// <summary><c>VAL-141</c>.</summary>
        public static string? ValidateDescription(string? value)
        {
            var description = LabMicrobiologyMasterDataText.Normalize(value);

            if (description is { Length: > MaxDescriptionLength })
                throw new LabResultReasonValidationException(
                    $"Keterangan alasan paling panjang {MaxDescriptionLength} karakter.");

            return description;
        }

        /// <summary>
        /// <c>VAL-142</c>. Laporan mutu menghitung per kode — mengubah <c>SAMPEL-TERTUKAR</c>
        /// menjadi <c>TERTUKAR</c> di tengah tahun membuat dua paruh tahun tidak dapat dijumlahkan.
        /// Kode yang tidak dikirim atau sama persis dengan yang tersimpan diterima.
        /// </summary>
        public static void EnsureCodeUnchanged(string storedCode, string? requestedCode)
        {
            var requested = requestedCode?.Trim();

            if (!string.IsNullOrEmpty(requested) && !string.Equals(requested, storedCode, StringComparison.Ordinal))
                throw new LabResultReasonValidationException(
                    "Kode alasan tidak dapat diubah. Buat alasan baru bila perlu.");
        }

        public static (int PageNumber, int PageSize) Paging(int pageNumber, int pageSize) =>
            (pageNumber < 1 ? 1 : pageNumber, pageSize is < 1 or > 100 ? 20 : pageSize);

        /// <summary>
        /// Pilihan alasan dikirim utuh dalam satu halaman: daftarnya pendek dan dipakai sebagai
        /// isi kotak pilihan, sehingga memotongnya berarti menyembunyikan alasan yang sah.
        /// </summary>
        public static PagedResult<T> SinglePage<T>(List<T> items) =>
            LabMicrobiologyMasterDataText.Page(1, Math.Max(items.Count, 1), items.Count, items);
    }

    /// <summary>Pelanggaran <c>VAL-141</c> atau <c>VAL-142</c>. Dipetakan menjadi <c>422</c>.</summary>
    public sealed class LabResultReasonValidationException(string message) : Exception(message);

    /// <summary>Pelanggaran <c>VAL-140</c>. Dipetakan menjadi <c>409</c>.</summary>
    public sealed class LabResultReasonConflictException(string message) : Exception(message);
}
