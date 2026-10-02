using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services
{
    /// <summary>
    /// Pemilik seluruh pembacaan dan perubahan master keperluan akses rekam medis. Controller
    /// <c>MedicalRecordAccessPurposeController</c> tidak menyentuh <c>ApplicationDbContext</c>
    /// sendiri, sesuai QBE-SVC-001.
    /// </summary>
    /// <remarks>
    /// Master ini bukan daftar pilihan biasa. Selama isinya kosong, <b>pembukaan berkas rekam
    /// medis pasien di luar rawatan pengguna selalu ditolak</b> — penilaian akses menuntut
    /// keperluan yang sah, dan tidak ada satu pun yang dapat dipilih. Itulah sebabnya layar
    /// pengelolanya menentukan apakah modul rekam medis berguna atau tidak.
    ///
    /// Tiga aturan melekat di sini:
    ///
    /// 1. Satu keperluan tidak boleh terdaftar dua kali. Kode kembar ditolak sebelum menyentuh
    ///    database, dan index unik pada <c>PurposeCode</c> menjadi penjaga terakhirnya bila dua
    ///    petugas menyimpan pada saat hampir bersamaan.
    /// 2. Keperluan yang tidak berlaku lagi dinonaktifkan, bukan dihapus, dan penonaktifannya
    ///    TIDAK pernah menyentuh jejak akses yang sudah memakainya.
    /// 3. <c>IsFreeTextRequired</c> dan <c>RequiresReview</c> menentukan perilaku layar lain,
    ///    sehingga keduanya selalu bernilai tegas — tidak pernah disimpulkan dari nama.
    /// </remarks>
    public class MedicalRecordAccessPurposeService
    {
        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;

        private const string NotFoundMessage = "Keperluan akses tidak ditemukan.";

        private readonly ApplicationDbContext _dbContext;

        public MedicalRecordAccessPurposeService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<MedicalRecordAccessPurposeResponse>> GetPagedAsync(
            string? search,
            bool? isActive,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var query = BaseQuery();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PurposeCode.ToLower().Contains(keyword) ||
                    x.PurposeName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            var totalData = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.PurposeName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new MedicalRecordAccessPurposeResponse
                {
                    Id = x.Id,
                    PurposeCode = x.PurposeCode,
                    PurposeName = x.PurposeName,
                    Description = x.Description,
                    IsFreeTextRequired = x.IsFreeTextRequired,
                    RequiresReview = x.RequiresReview,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<MedicalRecordAccessPurposeResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>
        /// Pilihan keperluan yang aktif saja, untuk kotak isian.
        /// </summary>
        /// <remarks>
        /// Bentuknya SENGAJA memakai <see cref="MedicalRecordAccessPurposeOptionResponse"/>
        /// yang sudah dipakai <c>GET /medical-records/filters/metadata</c>. Satu konsep yang
        /// sama tidak boleh punya dua bentuk balasan yang berbeda: frontend yang berpindah dari
        /// satu sumber ke sumber lain tidak perlu menulis ulang pembacanya.
        /// </remarks>
        public Task<List<MedicalRecordAccessPurposeOptionResponse>> GetOptionsAsync(
            CancellationToken cancellationToken = default)
        {
            return BaseQuery()
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.PurposeName)
                .Select(x => new MedicalRecordAccessPurposeOptionResponse
                {
                    Id = x.Id,
                    PurposeCode = x.PurposeCode,
                    PurposeName = x.PurposeName,
                    IsFreeTextRequired = x.IsFreeTextRequired,
                    RequiresReview = x.RequiresReview,
                    Description = x.Description
                })
                .ToListAsync(cancellationToken);
        }

        public Task<MstMedicalRecordAccessPurpose?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return BaseQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<MedicalRecordAccessPurposeResult> CreateAsync(
            CreateMedicalRecordAccessPurposeRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var validationMessage = ValidateRequest(request);

            if (validationMessage != null)
                return Failed(MedicalRecordAccessPurposeStatus.Invalid, validationMessage);

            var purposeCode = NormalizeCode(request.PurposeCode);

            if (await PurposeCodeIsUsedAsync(purposeCode, excludeId: null, cancellationToken))
            {
                return Failed(
                    MedicalRecordAccessPurposeStatus.DuplicateCode,
                    DuplicateCodeMessage(purposeCode));
            }

            var now = DateTime.UtcNow;

            var entity = new MstMedicalRecordAccessPurpose
            {
                Id = Guid.NewGuid(),
                PurposeCode = purposeCode,
                PurposeName = NormalizeText(request.PurposeName) ?? string.Empty,
                Description = NormalizeText(request.Description),
                IsFreeTextRequired = request.IsFreeTextRequired,
                RequiresReview = request.RequiresReview,
                SortOrder = request.SortOrder,
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<MstMedicalRecordAccessPurpose>().Add(entity);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Penjaga terakhir. Dua petugas yang menyimpan kode yang sama pada saat hampir
                // bersamaan sama-sama lolos pemeriksaan di atas, dan index unik di database
                // yang menolak salah satunya.
                //
                // Barisnya dilepas dari pelacakan supaya penyimpanan berikutnya pada permintaan
                // yang sama tidak mencoba menyisipkannya sekali lagi.
                _dbContext.Entry(entity).State = EntityState.Detached;

                return Failed(
                    MedicalRecordAccessPurposeStatus.DuplicateCode,
                    DuplicateCodeMessage(purposeCode));
            }

            return new MedicalRecordAccessPurposeResult(
                MedicalRecordAccessPurposeStatus.Success,
                entity,
                "Keperluan akses berhasil dibuat.");
        }

        public async Task<MedicalRecordAccessPurposeResult> UpdateAsync(
            Guid id,
            UpdateMedicalRecordAccessPurposeRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedAsync(id, cancellationToken);

            if (entity == null)
                return Failed(MedicalRecordAccessPurposeStatus.NotFound, NotFoundMessage);

            var validationMessage = ValidateRequest(request);

            if (validationMessage != null)
                return Failed(MedicalRecordAccessPurposeStatus.Invalid, validationMessage);

            var purposeCode = NormalizeCode(request.PurposeCode);

            if (await PurposeCodeIsUsedAsync(purposeCode, excludeId: id, cancellationToken))
            {
                return Failed(
                    MedicalRecordAccessPurposeStatus.DuplicateCode,
                    DuplicateCodeMessage(purposeCode));
            }

            entity.PurposeCode = purposeCode;
            entity.PurposeName = NormalizeText(request.PurposeName) ?? entity.PurposeName;
            entity.Description = NormalizeText(request.Description);
            entity.IsFreeTextRequired = request.IsFreeTextRequired;
            entity.RequiresReview = request.RequiresReview;
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Failed(
                    MedicalRecordAccessPurposeStatus.DuplicateCode,
                    DuplicateCodeMessage(purposeCode));
            }

            return new MedicalRecordAccessPurposeResult(
                MedicalRecordAccessPurposeStatus.Success,
                entity,
                "Keperluan akses berhasil diubah.");
        }

        /// <remarks>
        /// Method ini mengubah <c>IsActive</c> saja. Ia tidak menyentuh satu pun baris
        /// <c>MrcAccessLog</c>.
        ///
        /// <b>Contoh.</b> Keperluan <c>RUJUKAN</c> sudah dipakai pada 400 pembukaan berkas
        /// sepanjang tahun lalu. Hari ini unit rekam medis menonaktifkannya karena rujukan
        /// pindah ke alur lain. Keempat ratus jejak itu tetap menyebut <c>RUJUKAN</c> apa
        /// adanya, sehingga tinjauan atas periode itu tetap dapat dibaca utuh.
        /// </remarks>
        public async Task<MedicalRecordAccessPurposeResult> UpdateStatusAsync(
            Guid id,
            bool isActive,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedAsync(id, cancellationToken);

            if (entity == null)
                return Failed(MedicalRecordAccessPurposeStatus.NotFound, NotFoundMessage);

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new MedicalRecordAccessPurposeResult(
                MedicalRecordAccessPurposeStatus.Success,
                entity,
                isActive
                    ? "Keperluan akses berhasil diaktifkan."
                    : "Keperluan akses berhasil dinonaktifkan.");
        }

        public static MedicalRecordAccessPurposeResponse ToResponse(
            MstMedicalRecordAccessPurpose entity) => new()
            {
                Id = entity.Id,
                PurposeCode = entity.PurposeCode,
                PurposeName = entity.PurposeName,
                Description = entity.Description,
                IsFreeTextRequired = entity.IsFreeTextRequired,
                RequiresReview = entity.RequiresReview,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime
            };

        private IQueryable<MstMedicalRecordAccessPurpose> BaseQuery()
            => _dbContext.Set<MstMedicalRecordAccessPurpose>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && !x.IsCancel);

        private Task<MstMedicalRecordAccessPurpose?> TrackedAsync(
            Guid id,
            CancellationToken cancellationToken)
            => _dbContext.Set<MstMedicalRecordAccessPurpose>()
                .FirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDelete && !x.IsCancel,
                    cancellationToken);

        private Task<bool> PurposeCodeIsUsedAsync(
            string purposeCode,
            Guid? excludeId,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<MstMedicalRecordAccessPurpose>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.PurposeCode.ToLower() == purposeCode.ToLower());

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return query.AnyAsync(cancellationToken);
        }

        private static string DuplicateCodeMessage(string purposeCode)
            => $"Kode keperluan {purposeCode} sudah dipakai keperluan akses lain.";

        private static MedicalRecordAccessPurposeResult Failed(
            MedicalRecordAccessPurposeStatus status,
            string message)
            => new(status, null, message);

        private static string? ValidateRequest(CreateMedicalRecordAccessPurposeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PurposeCode))
                return "Kode keperluan wajib diisi.";

            if (string.IsNullOrWhiteSpace(request.PurposeName))
                return "Nama keperluan wajib diisi.";

            if (request.SortOrder is < 0 or > 9999)
                return "Urutan tampil harus antara 0 dan 9999.";

            return null;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

            return (pageNumber, pageSize);
        }

        private static string NormalizeCode(string value)
            => value.Trim().ToUpperInvariant();

        private static string? NormalizeText(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public enum MedicalRecordAccessPurposeStatus
    {
        Success = 0,
        NotFound = 1,
        Invalid = 2,
        DuplicateCode = 3
    }

    public sealed record MedicalRecordAccessPurposeResult(
        MedicalRecordAccessPurposeStatus Status,
        MstMedicalRecordAccessPurpose? Entity,
        string Message);
}
