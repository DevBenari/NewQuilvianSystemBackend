using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services
{
    /// <summary>
    /// Pemilik seluruh pembacaan dan perubahan daftar alasan terkendali Bank Darah. Controller
    /// <c>BloodBankReasonController</c> tidak menyentuh <c>ApplicationDbContext</c> sendiri,
    /// sesuai QBE-SVC-001.
    /// </summary>
    /// <remarks>
    /// Tiga aturan melekat di sini:
    ///
    /// 1. Kode alasan tunggal. Kode kembar ditolak sebelum menyentuh database, dan index unik
    ///    menjadi penjaga terakhirnya.
    /// 2. <b>Kategori wajib berasal dari daftar tertutup</b>
    ///    (<see cref="BloodBankReasonCategories"/>). Kolomnya bertipe teks mengikuti kamus data,
    ///    sehingga daftar tertutupnya dijaga di sini — tanpa penjagaan ini, satu salah ketik
    ///    menciptakan kategori baru yang tidak pernah dibaca layar mana pun, dan alasannya
    ///    hilang dari kotak pilihan tanpa ada yang menyadarinya.
    /// 3. Alasan yang tidak dipakai lagi dinonaktifkan, bukan dihapus, dan penonaktifannya
    ///    <b>tidak</b> mengubah makna riwayat lama — rekam tindakan menyimpan salinan teksnya.
    /// </remarks>
    public class BloodBankReasonService
    {
        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;

        private const string NotFoundMessage = "Alasan tidak ditemukan atau sudah dihapus.";

        private readonly ApplicationDbContext _dbContext;

        public BloodBankReasonService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<BloodBankReasonResponse>> GetPagedAsync(
            string? search,
            bool? isActive,
            string? reasonCategory,
            string? sortBy,
            string? sortDirection,
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
                    x.ReasonCode.ToLower().Contains(keyword) ||
                    x.ReasonText.ToLower().Contains(keyword));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            var category = BloodBankReasonCategories.Normalize(reasonCategory);

            if (category != null)
                query = query.Where(x => x.ReasonCategory == category);

            var totalData = await query.CountAsync(cancellationToken);

            query = ApplySort(query, sortBy, sortDirection);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BloodBankReasonResponse
                {
                    Id = x.Id,
                    ReasonCode = x.ReasonCode,
                    ReasonText = x.ReasonText,
                    ReasonCategory = x.ReasonCategory,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<BloodBankReasonResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>Ringkasan jumlah, termasuk kategori yang belum punya alasan aktif.</summary>
        public async Task<BloodBankReasonSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var rows = await BaseQuery()
                .Select(x => new { x.IsActive, x.ReasonCategory })
                .ToListAsync(cancellationToken);

            var kategoriBerisi = rows
                .Where(x => x.IsActive)
                .Select(x => x.ReasonCategory)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var kategoriKosong = BloodBankReasonCategories.All
                .Where(x => !kategoriBerisi.Contains(x))
                .ToList();

            return new BloodBankReasonSummaryResponse
            {
                TotalBloodBankReason = rows.Count,
                ActiveBloodBankReason = rows.Count(x => x.IsActive),
                InactiveBloodBankReason = rows.Count(x => !x.IsActive),
                CategoryWithoutActiveReasonCount = kategoriKosong.Count,
                CategoryWithoutActiveReason = kategoriKosong
            };
        }

        /// <summary>
        /// Pilihan alasan aktif, biasanya disaring per kategori.
        /// </summary>
        /// <remarks>
        /// Penyaring kategori ada di kontrak (<c>GET /options?category=</c>) karena layar
        /// pembatalan order hanya boleh menawarkan alasan pembatalan, bukan seluruh alasan Bank
        /// Darah. Kategori yang tidak dikenal memulangkan daftar kosong, bukan seluruh isi
        /// tabel — memulangkan semuanya akan menawarkan alasan yang salah konteks kepada petugas.
        /// </remarks>
        public Task<List<BloodBankReasonOptionResponse>> GetOptionsAsync(
            string? category,
            string? search,
            CancellationToken cancellationToken = default)
        {
            var query = BaseQuery().Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(category))
            {
                var normalized = BloodBankReasonCategories.Normalize(category);

                if (normalized == null)
                    return Task.FromResult(new List<BloodBankReasonOptionResponse>());

                query = query.Where(x => x.ReasonCategory == normalized);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ReasonCode.ToLower().Contains(keyword) ||
                    x.ReasonText.ToLower().Contains(keyword));
            }

            return query
                .OrderBy(x => x.ReasonCategory)
                .ThenBy(x => x.ReasonCode)
                .Select(x => new BloodBankReasonOptionResponse
                {
                    Id = x.Id,
                    ReasonCode = x.ReasonCode,
                    ReasonText = x.ReasonText,
                    ReasonCategory = x.ReasonCategory
                })
                .ToListAsync(cancellationToken);
        }

        public Task<MstBloodBankReason?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return BaseQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<BloodBankReasonResult> CreateAsync(
            CreateBloodBankReasonRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var (validationMessage, category) = ValidateRequest(request);

            if (validationMessage != null)
                return Failed(BloodBankReasonStatus.Invalid, validationMessage);

            var code = NormalizeCode(request.ReasonCode);

            if (await ReasonCodeIsUsedAsync(code, excludeId: null, cancellationToken))
                return Failed(BloodBankReasonStatus.DuplicateCode, DuplicateCodeMessage(code));

            var now = DateTime.UtcNow;

            var entity = new MstBloodBankReason
            {
                Id = Guid.NewGuid(),
                ReasonCode = code,
                ReasonText = NormalizeText(request.ReasonText) ?? string.Empty,
                ReasonCategory = category!,
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<MstBloodBankReason>().Add(entity);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Penjaga terakhir index unik kode. Barisnya dilepas dari pelacakan supaya
                // penyimpanan berikutnya tidak mencoba menyisipkannya sekali lagi.
                _dbContext.Entry(entity).State = EntityState.Detached;

                return Failed(BloodBankReasonStatus.DuplicateCode, DuplicateCodeMessage(code));
            }

            return new BloodBankReasonResult(
                BloodBankReasonStatus.Success,
                entity,
                "Alasan berhasil dibuat.");
        }

        public async Task<BloodBankReasonResult> UpdateAsync(
            Guid id,
            UpdateBloodBankReasonRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedAsync(id, cancellationToken);

            if (entity == null)
                return Failed(BloodBankReasonStatus.NotFound, NotFoundMessage);

            var (validationMessage, category) = ValidateRequest(request);

            if (validationMessage != null)
                return Failed(BloodBankReasonStatus.Invalid, validationMessage);

            var code = NormalizeCode(request.ReasonCode);

            if (await ReasonCodeIsUsedAsync(code, excludeId: id, cancellationToken))
                return Failed(BloodBankReasonStatus.DuplicateCode, DuplicateCodeMessage(code));

            entity.ReasonCode = code;
            entity.ReasonText = NormalizeText(request.ReasonText) ?? entity.ReasonText;
            entity.ReasonCategory = category!;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Failed(BloodBankReasonStatus.DuplicateCode, DuplicateCodeMessage(code));
            }

            return new BloodBankReasonResult(
                BloodBankReasonStatus.Success,
                entity,
                "Alasan berhasil diubah.");
        }

        /// <remarks>
        /// Menonaktifkan alasan TIDAK menyentuh satu pun rekam tindakan yang sudah memakainya.
        /// Yang berubah hanya ke depan: alasan itu tidak lagi muncul sebagai pilihan.
        ///
        /// <b>Contoh.</b> Alasan "Pasien menolak transfusi" sudah dipakai pada 80 pembatalan
        /// order tahun lalu. Hari ini BDRS menonaktifkannya karena rumusannya diganti. Kedelapan
        /// puluh rekam itu tetap menyebut teks yang sama apa adanya, sehingga tinjauan atas
        /// periode itu tetap dapat dibaca utuh.
        /// </remarks>
        public async Task<BloodBankReasonResult> UpdateStatusAsync(
            Guid id,
            bool isActive,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedAsync(id, cancellationToken);

            if (entity == null)
                return Failed(BloodBankReasonStatus.NotFound, NotFoundMessage);

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new BloodBankReasonResult(
                BloodBankReasonStatus.Success,
                entity,
                isActive
                    ? "Alasan berhasil diaktifkan."
                    : "Alasan berhasil dinonaktifkan. Rekam tindakan yang sudah memakainya tidak berubah.");
        }

        /// <summary>Menandai alasan terhapus. Tidak pernah menghapus baris secara fisik.</summary>
        /// <remarks>
        /// Untuk keadaan sehari-hari, <b>menonaktifkan lebih tepat daripada menghapus</b>. Rekam
        /// tindakan memang menyimpan salinan teks alasannya sendiri, sehingga menghapus baris
        /// tidak merusak riwayat — tetapi menonaktifkan menjaga jejak bahwa alasan itu pernah
        /// menjadi pilihan resmi.
        /// </remarks>
        public async Task<BloodBankReasonResult> DeleteAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedAsync(id, cancellationToken);

            if (entity == null)
                return Failed(BloodBankReasonStatus.NotFound, NotFoundMessage);

            var now = DateTime.UtcNow;

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new BloodBankReasonResult(
                BloodBankReasonStatus.Success,
                entity,
                "Alasan berhasil dihapus.");
        }

        public static BloodBankReasonFilterMetadataResponse BuildFilterMetadata()
            => new()
            {
                DefaultFilter = new BloodBankReasonDefaultFilterResponse(),
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ReasonCategoryOptions = BuildCategoryOptions(),
                SortOptions = new List<BloodBankReasonSortOptionResponse>
                {
                    new() { Value = "reasonCategory", Label = "Kategori" },
                    new() { Value = "reasonCode", Label = "Kode Alasan" },
                    new() { Value = "reasonText", Label = "Teks Alasan" },
                    new() { Value = "createDateTime", Label = "Tanggal Dibuat" }
                },
                QueryParameters = new List<BloodBankReasonQueryParameterInfoResponse>
                {
                    new()
                    {
                        Name = "search",
                        Type = "string",
                        Description = "Mencari pada kode dan teks alasan.",
                        Example = "salah input"
                    },
                    new()
                    {
                        Name = "isActive",
                        Type = "boolean",
                        Description = "Menyaring alasan aktif atau nonaktif.",
                        Example = "true"
                    },
                    new()
                    {
                        Name = "reasonCategory",
                        Type = "string",
                        Description =
                            "Menyaring per kategori. Nilainya dari daftar tertutup pada reasonCategoryOptions.",
                        Example = BloodBankReasonCategories.OrderCancellationClinical
                    },
                    new()
                    {
                        Name = "sortBy",
                        Type = "string",
                        Description = "Kolom pengurutan. Bawaannya reasonCategory.",
                        Example = "reasonCategory"
                    },
                    new()
                    {
                        Name = "sortDirection",
                        Type = "string",
                        Description = "Arah pengurutan, asc atau desc.",
                        Example = "asc"
                    },
                    new()
                    {
                        Name = "pageNumber",
                        Type = "integer",
                        Description = "Nomor halaman, dimulai dari 1.",
                        Example = "1"
                    },
                    new()
                    {
                        Name = "pageSize",
                        Type = "integer",
                        Description = "Jumlah baris per halaman, paling banyak 100.",
                        Example = "25"
                    }
                },
                CreateFields = BuildFormFields(),
                UpdateFields = BuildFormFields()
            };

        public static BloodBankReasonResponse ToResponse(MstBloodBankReason entity) => new()
        {
            Id = entity.Id,
            ReasonCode = entity.ReasonCode,
            ReasonText = entity.ReasonText,
            ReasonCategory = entity.ReasonCategory,
            IsActive = entity.IsActive,
            CreateDateTime = entity.CreateDateTime,
            UpdateDateTime = entity.UpdateDateTime
        };

        /// <summary>Label kategori yang dibaca petugas, bukan nama teknisnya.</summary>
        public static List<BloodBankReasonCategoryOptionResponse> BuildCategoryOptions()
            => new()
            {
                new()
                {
                    Value = BloodBankReasonCategories.OrderCancellationClinical,
                    Label = "Pembatalan order — klinis",
                    Description = "Dipakai dokter peminta ketika kebutuhan transfusi berubah atau dicabut."
                },
                new()
                {
                    Value = BloodBankReasonCategories.OrderCancellationOperational,
                    Label = "Pembatalan order — operasional",
                    Description = "Dipakai petugas BDRS untuk merapikan kekeliruan, misalnya order ganda atau salah input."
                },
                new()
                {
                    Value = BloodBankReasonCategories.Emergency,
                    Label = "Jalur darurat",
                    Description = "Menyertai pemberian darah lewat jalur darurat."
                },
                new()
                {
                    Value = BloodBankReasonCategories.PendingReviewResolution,
                    Label = "Penyelesaian kantong menunggu keputusan",
                    Description = "Menyertai penyelesaian kantong yang berstatus menunggu keputusan."
                },
                new()
                {
                    Value = BloodBankReasonCategories.Return,
                    Label = "Pengembalian ke PMI",
                    Description = "Menyertai pengembalian kantong kepada penyedia."
                },
                new()
                {
                    Value = BloodBankReasonCategories.NotUsable,
                    Label = "Penetapan tidak layak",
                    Description = "Menyertai penetapan kantong sebagai tidak layak pakai."
                },
                new()
                {
                    Value = BloodBankReasonCategories.OverDelivery,
                    Label = "Kiriman melebihi permintaan",
                    Description = "Menyertai pencatatan kantong yang datang melebihi jumlah yang diminta."
                },
                new()
                {
                    Value = BloodBankReasonCategories.AllocationCancellation,
                    Label = "Pembatalan alokasi",
                    Description = "Menyertai pembatalan alokasi kantong terhadap sebuah kebutuhan."
                },
                new()
                {
                    Value = BloodBankReasonCategories.IssuanceCorrection,
                    Label = "Koreksi pencatatan pemberian",
                    Description = "Menyertai pengajuan koreksi atas pencatatan pemberian."
                },
                new()
                {
                    Value = BloodBankReasonCategories.CorrectionRejection,
                    Label = "Penolakan koreksi",
                    Description = "Menyertai penolakan permintaan koreksi oleh Dokter BDRS."
                }
            };

        private static List<BloodBankReasonFormFieldMetadataResponse> BuildFormFields()
            => new()
            {
                new()
                {
                    Name = "reasonCode",
                    Label = "Kode Alasan",
                    Section = "Identitas",
                    InputType = "text",
                    IsRequiredOnCreate = true,
                    IsRequiredOnUpdate = true,
                    RequiredType = "Required",
                    MaxLength = 30,
                    Description = "Kode singkat yang dikenali petugas BDRS.",
                    Example = "CANCEL-KLINIS-01",
                    SortOrder = 1
                },
                new()
                {
                    Name = "reasonText",
                    Label = "Teks Alasan",
                    Section = "Identitas",
                    InputType = "text",
                    IsRequiredOnCreate = true,
                    IsRequiredOnUpdate = true,
                    RequiredType = "Required",
                    MaxLength = 200,
                    Description = "Kalimat yang dibaca petugas saat memilih alasan.",
                    Example = "Kebutuhan transfusi dibatalkan dokter",
                    SortOrder = 2
                },
                new()
                {
                    Name = "reasonCategory",
                    Label = "Kategori",
                    Section = "Identitas",
                    InputType = "select",
                    IsRequiredOnCreate = true,
                    IsRequiredOnUpdate = true,
                    RequiredType = "Required",
                    MaxLength = 40,
                    OptionsSource = "reasonCategoryOptions",
                    Description =
                        "Menentukan di layar mana alasan ini muncul. Pembatalan order klinis dan operasional sengaja dipisah supaya peninjau dapat membedakannya.",
                    Example = BloodBankReasonCategories.OrderCancellationClinical,
                    SortOrder = 3
                },
                new()
                {
                    Name = "isActive",
                    Label = "Aktif",
                    Section = "Status",
                    InputType = "switch",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    Description = "Alasan nonaktif tidak lagi muncul sebagai pilihan; rekam lama tidak berubah.",
                    Example = "true",
                    SortOrder = 4
                }
            };

        private static IQueryable<MstBloodBankReason> ApplySort(
            IQueryable<MstBloodBankReason> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy?.Trim().ToLowerInvariant()) switch
            {
                "reasoncode" => descending
                    ? query.OrderByDescending(x => x.ReasonCode)
                    : query.OrderBy(x => x.ReasonCode),
                "reasontext" => descending
                    ? query.OrderByDescending(x => x.ReasonText)
                    : query.OrderBy(x => x.ReasonText),
                "createdatetime" => descending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),
                _ => descending
                    ? query.OrderByDescending(x => x.ReasonCategory).ThenBy(x => x.ReasonCode)
                    : query.OrderBy(x => x.ReasonCategory).ThenBy(x => x.ReasonCode)
            };
        }

        private IQueryable<MstBloodBankReason> BaseQuery()
            => _dbContext.Set<MstBloodBankReason>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && !x.IsCancel);

        private Task<MstBloodBankReason?> TrackedAsync(Guid id, CancellationToken cancellationToken)
            => _dbContext.Set<MstBloodBankReason>()
                .FirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDelete && !x.IsCancel,
                    cancellationToken);

        private Task<bool> ReasonCodeIsUsedAsync(
            string code,
            Guid? excludeId,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<MstBloodBankReason>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.ReasonCode.ToLower() == code.ToLower());

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return query.AnyAsync(cancellationToken);
        }

        private static string DuplicateCodeMessage(string code)
            => $"Kode alasan {code} sudah dipakai alasan lain.";

        private static BloodBankReasonResult Failed(BloodBankReasonStatus status, string message)
            => new(status, null, message);

        /// <summary>
        /// Memulangkan pesan kesalahan bila ada, beserta kategori yang sudah dinormalkan ke
        /// bentuk bakunya.
        /// </summary>
        private static (string? Message, string? Category) ValidateRequest(
            CreateBloodBankReasonRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ReasonCode))
                return ("Kode alasan wajib diisi.", null);

            if (string.IsNullOrWhiteSpace(request.ReasonText))
                return ("Teks alasan wajib diisi.", null);

            var category = BloodBankReasonCategories.Normalize(request.ReasonCategory);

            if (category == null)
            {
                return (
                    "Kategori alasan tidak dikenal. Pilih salah satu dari: " +
                    string.Join(", ", BloodBankReasonCategories.All) + ".",
                    null);
            }

            return (null, category);
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

    public enum BloodBankReasonStatus
    {
        Success = 0,
        NotFound = 1,
        Invalid = 2,
        DuplicateCode = 3
    }

    public sealed record BloodBankReasonResult(
        BloodBankReasonStatus Status,
        MstBloodBankReason? Entity,
        string Message);
}
