using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Globalization;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Pengelolaan batas nilai rujukan laboratorium (<c>LAB-DEC-006</c>, <c>LAB-DEC-018</c>,
    /// <c>LAB-DEC-021</c>, <c>LAB-DEC-023</c>).
    ///
    /// Satu aturan membentuk hampir seluruh isi berkas ini: <b>batas normal boleh diubah kepala
    /// instalasi dan langsung berlaku, batas kritis tidak.</b> Batas normal adalah penilaian
    /// teknis laboratorium — ia wajar bergeser ketika alat atau metode berganti. Batas kritis
    /// adalah penilaian klinis tentang pada angka berapa seorang pasien dianggap terancam, dan
    /// karena itu hanya berubah lewat pengajuan yang disetujui pihak klinis (<c>BE-LAB-05</c>).
    ///
    /// Karena itu <c>VAL-28</c> ditegakkan di sini, bukan sekadar dengan tidak menyediakan
    /// ruasnya: permintaan ubah tetap menerima batas kritis, lalu menolaknya bila berbeda dari
    /// yang berlaku. Menolak secara terbuka lebih jujur daripada mengabaikan diam-diam, karena
    /// pemanggil yang mengira perubahannya tersimpan adalah keadaan yang justru berbahaya.
    /// </summary>
    public class LabValueBoundService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabValueBoundService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        // =================================================================
        // Baca
        // =================================================================

        public async Task<PagedResult<LabValueBoundListResponse>> GetListAsync(
            LabValueBoundPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var source = _dbContext.LabValueBounds
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (query.ProcedureId.HasValue && query.ProcedureId.Value != Guid.Empty)
                source = source.Where(x => x.ProcedureId == query.ProcedureId.Value);

            if (query.IsActive.HasValue)
                source = source.Where(x => x.IsActive == query.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                source = source.Where(x =>
                    x.Procedure != null &&
                    (EF.Functions.ILike(x.Procedure.ProcedureCode, $"%{search}%") ||
                     EF.Functions.ILike(x.Procedure.ProcedureName, $"%{search}%")));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.Procedure != null ? x.Procedure.ProcedureName : string.Empty)
                .ThenBy(x => x.GenderScope)
                .ThenBy(x => x.AgeCategoryId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => MapListProjection(x))
                .ToListAsync(cancellationToken);

            return new PagedResult<LabValueBoundListResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<LabValueBoundDetailResponse?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabValueBounds
                .AsNoTracking()
                .Include(x => x.Procedure)
                .Include(x => x.AgeCategory)
                .Include(x => x.Options)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null) return null;

            var hasPending = await _dbContext.LabValueBoundChangeRequests
                .AsNoTracking()
                .AnyAsync(x =>
                    x.ValueBoundId == id &&
                    !x.IsDelete &&
                    x.RequestStatus == LabBoundChangeStatus.Submitted,
                    cancellationToken);

            return MapDetail(entity, hasPending);
        }

        public async Task<List<LabValueBoundHistoryResponse>> GetHistoryAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var exists = await _dbContext.LabValueBounds
                .AsNoTracking()
                .AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (!exists)
                throw new KeyNotFoundException("Batas nilai tidak ditemukan.");

            return await _dbContext.LabValueBoundHistories
                .AsNoTracking()
                .Where(x => x.ValueBoundId == id && !x.IsDelete)
                .OrderByDescending(x => x.OccurredAt)
                .Select(x => new LabValueBoundHistoryResponse
                {
                    Id = x.Id,
                    ValueBoundId = x.ValueBoundId,
                    ChangedField = x.ChangedField,
                    OldValue = x.OldValue,
                    NewValue = x.NewValue,
                    ActorUserId = x.ActorUserId,
                    ApprovedByUserId = x.ApprovedByUserId,
                    ChangeReason = x.ChangeReason,
                    OccurredAt = x.OccurredAt
                })
                .ToListAsync(cancellationToken);
        }

        // =================================================================
        // Membuat
        // =================================================================

        public async Task<LabValueBoundDetailResponse> CreateAsync(
            CreateLabValueBoundRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.ProcedureId == Guid.Empty)
                throw new ArgumentException("ProcedureId wajib diisi.");

            if (!Enum.IsDefined(request.ResultForm))
                throw new ArgumentException("Bentuk hasil tidak dikenal.");

            if (!Enum.IsDefined(request.GenderScope))
                throw new ArgumentException("Pembatas jenis kelamin tidak dikenal.");

            var procedure = await _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.ProcedureId && x.IsLaboratory && x.IsActive && !x.IsDelete,
                    cancellationToken);

            if (procedure == null)
                throw new ArgumentException("Procedure tidak ditemukan, tidak aktif, atau bukan procedure laboratorium.");

            if (request.AgeCategoryId.HasValue)
            {
                var ageExists = await _dbContext.Set<MstAgeCategory>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.AgeCategoryId.Value && !x.IsDelete, cancellationToken);

                if (!ageExists)
                    throw new ArgumentException("Kelompok umur tidak ditemukan.");
            }

            ValidateShape(
                request.ResultForm,
                request.Unit,
                request.NormalLow,
                request.NormalHigh,
                request.CriticalLow,
                request.CriticalHigh,
                request.CitoTurnaroundMinutes,
                request.Options);

            // VAL-21. Diperiksa di sini supaya pemanggil menerima pesan yang berarti, sementara
            // index unik database tetap menjadi penjaga terakhir bila dua permintaan datang
            // bersamaan.
            var duplicate = await _dbContext.LabValueBounds
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.ProcedureId == request.ProcedureId &&
                    x.GenderScope == request.GenderScope &&
                    x.AgeCategoryId == request.AgeCategoryId,
                    cancellationToken);

            if (duplicate)
                throw new LabValueBoundConflictException(
                    "Batas nilai untuk kelompok pasien ini sudah ada. Ubah yang sudah ada, jangan membuat baru.");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new LabValueBound
            {
                ProcedureId = request.ProcedureId,
                ResultForm = request.ResultForm,
                Unit = Normalize(request.Unit),
                NormalLow = request.NormalLow,
                NormalHigh = request.NormalHigh,
                CriticalLow = request.CriticalLow,
                CriticalHigh = request.CriticalHigh,
                GenderScope = request.GenderScope,
                AgeCategoryId = request.AgeCategoryId,
                CitoTurnaroundMinutes = request.CitoTurnaroundMinutes,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            foreach (var option in request.Options)
            {
                entity.Options.Add(new LabValueOption
                {
                    OptionCode = option.OptionCode.Trim(),
                    OptionName = option.OptionName.Trim(),
                    IsOutOfReference = option.IsOutOfReference,
                    IsCritical = option.IsCritical,
                    SortOrder = option.SortOrder,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }

            _dbContext.LabValueBounds.Add(entity);

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabValueBound.Create",
                "Membuat batas nilai laboratorium.",
                new
                {
                    entity.Id,
                    entity.ProcedureId,
                    ResultForm = entity.ResultForm.ToString(),
                    GenderScope = entity.GenderScope.ToString(),
                    entity.AgeCategoryId,
                    OptionCount = entity.Options.Count,
                    ActorUserId = actorUserId
                });

            return (await GetDetailAsync(entity.Id, cancellationToken))!;
        }

        // =================================================================
        // Mengubah
        // =================================================================

        public async Task<LabValueBoundDetailResponse> UpdateAsync(
            Guid id,
            UpdateLabValueBoundRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabValueBounds
                .Include(x => x.Options)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                throw new KeyNotFoundException("Batas nilai tidak ditemukan.");

            // VAL-28 — pengaman keselamatan. Diperiksa sebelum apa pun disentuh, supaya
            // permintaan yang menyelipkan perubahan batas kritis ditolak seluruhnya dan tidak
            // menyisakan sebagian perubahan yang terlanjur tersimpan.
            EnsureNoCriticalBoundChange(entity, request);

            var options = request.Options;

            ValidateShape(
                entity.ResultForm,
                request.Unit,
                request.NormalLow,
                request.NormalHigh,
                entity.CriticalLow,
                entity.CriticalHigh,
                request.CitoTurnaroundMinutes,
                options ?? entity.Options
                    .Where(x => !x.IsDelete)
                    .Select(x => new LabValueOptionRequest
                    {
                        OptionCode = x.OptionCode,
                        OptionName = x.OptionName,
                        IsOutOfReference = x.IsOutOfReference,
                        IsCritical = x.IsCritical,
                        SortOrder = x.SortOrder
                    })
                    .ToList());

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var reason = Normalize(request.ChangeReason);

            // AC-34: setiap kolom yang berubah menghasilkan satu baris riwayat tersendiri,
            // lengkap dengan nilai lama dan nilai barunya.
            RecordChange(entity, nameof(LabValueBound.Unit), entity.Unit, Normalize(request.Unit), actorUserId, reason, now);
            RecordChange(entity, nameof(LabValueBound.NormalLow), Format(entity.NormalLow), Format(request.NormalLow), actorUserId, reason, now);
            RecordChange(entity, nameof(LabValueBound.NormalHigh), Format(entity.NormalHigh), Format(request.NormalHigh), actorUserId, reason, now);
            RecordChange(entity, nameof(LabValueBound.CitoTurnaroundMinutes), Format(entity.CitoTurnaroundMinutes), Format(request.CitoTurnaroundMinutes), actorUserId, reason, now);

            entity.Unit = Normalize(request.Unit);
            entity.NormalLow = request.NormalLow;
            entity.NormalHigh = request.NormalHigh;
            entity.CitoTurnaroundMinutes = request.CitoTurnaroundMinutes;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (options != null)
            {
                ReplaceOptions(entity, options, actorUserId, reason, now);
            }

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabValueBound.Update",
                "Mengubah batas nilai laboratorium.",
                new { entity.Id, entity.ProcedureId, ActorUserId = actorUserId });

            return (await GetDetailAsync(entity.Id, cancellationToken))!;
        }

        // =================================================================
        // Menonaktifkan
        // =================================================================

        public async Task<LabValueBoundDetailResponse> DeactivateAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabValueBounds
                .Include(x => x.Options)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                throw new KeyNotFoundException("Batas nilai tidak ditemukan.");

            if (!entity.IsActive)
                return (await GetDetailAsync(entity.Id, cancellationToken))!;

            // VAL-30. Pemeriksaan tanpa satu pun batas nilai yang aktif membuat hasilnya tidak
            // dapat dinilai sama sekali — termasuk tidak dapat dikenali sebagai nilai kritis.
            var otherActive = await _dbContext.LabValueBounds
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.Id != entity.Id &&
                    x.ProcedureId == entity.ProcedureId,
                    cancellationToken);

            if (!otherActive)
                throw new LabValueBoundValidationException(
                    "Ini satu-satunya batas nilai untuk pemeriksaan tersebut. Menonaktifkannya membuat hasil tidak dapat dinilai.");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            RecordChange(entity, nameof(LabValueBound.IsActive), "true", "false", actorUserId, changeReason: null, now);

            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabValueBound.Deactivate",
                "Menonaktifkan batas nilai laboratorium.",
                new { entity.Id, entity.ProcedureId, ActorUserId = actorUserId });

            return (await GetDetailAsync(entity.Id, cancellationToken))!;
        }

        // =================================================================
        // Validasi bentuk — VAL-22 sampai VAL-27 dan VAL-29
        // =================================================================

        private static void ValidateShape(
            LabResultForm resultForm,
            string? unit,
            decimal? normalLow,
            decimal? normalHigh,
            decimal? criticalLow,
            decimal? criticalHigh,
            int? citoTurnaroundMinutes,
            IReadOnlyCollection<LabValueOptionRequest> options)
        {
            if (resultForm == LabResultForm.Numeric)
            {
                // VAL-22
                if (string.IsNullOrWhiteSpace(unit))
                    throw new LabValueBoundValidationException(
                        "Pemeriksaan berhasil angka wajib punya satuan, misalnya g/dL.");

                // VAL-24
                if (options.Count > 0)
                    throw new LabValueBoundValidationException(
                        "Pemeriksaan berhasil angka tidak boleh punya daftar pilihan.");
            }
            else
            {
                // VAL-23
                if (options.Count == 0)
                    throw new LabValueBoundValidationException(
                        "Pemeriksaan berhasil pilihan wajib punya sekurang-kurangnya satu pilihan.");

                var duplicateCode = options
                    .GroupBy(x => x.OptionCode.Trim(), StringComparer.OrdinalIgnoreCase)
                    .Any(x => x.Count() > 1);

                if (duplicateCode)
                    throw new LabValueBoundValidationException(
                        "Kode pilihan tidak boleh kembar dalam satu batas nilai.");

                if (options.Any(x => string.IsNullOrWhiteSpace(x.OptionCode) || string.IsNullOrWhiteSpace(x.OptionName)))
                    throw new LabValueBoundValidationException(
                        "Setiap pilihan wajib punya kode dan nama.");
            }

            // VAL-25
            if (normalLow.HasValue && normalHigh.HasValue && normalLow > normalHigh)
                throw new LabValueBoundValidationException(
                    "Batas normal bawah tidak boleh lebih besar daripada batas atas.");

            // VAL-26
            if (criticalLow.HasValue && normalLow.HasValue && criticalLow > normalLow)
                throw new LabValueBoundValidationException(
                    "Batas kritis bawah harus lebih rendah daripada batas normal bawah.");

            // VAL-27
            if (criticalHigh.HasValue && normalHigh.HasValue && criticalHigh < normalHigh)
                throw new LabValueBoundValidationException(
                    "Batas kritis atas harus lebih tinggi daripada batas normal atas.");

            // VAL-29
            if (citoTurnaroundMinutes.HasValue && citoTurnaroundMinutes.Value <= 0)
                throw new LabValueBoundValidationException(
                    "Batas waktu cito harus lebih dari nol menit.");
        }

        /// <summary>
        /// <c>VAL-28</c>. Batas kritis hanya berubah lewat pengajuan yang disetujui pihak
        /// klinis, dan itu berlaku untuk kedua bentuk hasil: angka lewat
        /// <c>CriticalLow</c>/<c>CriticalHigh</c>, dan pilihan lewat penanda
        /// <c>IsCritical</c> pada daftar pilihannya.
        /// </summary>
        private static void EnsureNoCriticalBoundChange(
            LabValueBound entity,
            UpdateLabValueBoundRequest request)
        {
            const string pesan =
                "Perubahan batas kritis harus lewat pengajuan yang disetujui pihak klinis.";

            if (request.CriticalLow != entity.CriticalLow || request.CriticalHigh != entity.CriticalHigh)
                throw new LabValueBoundValidationException(pesan);

            if (request.Options == null) return;

            var sekarang = entity.Options
                .Where(x => !x.IsDelete)
                .ToDictionary(x => x.OptionCode.Trim(), x => x.IsCritical, StringComparer.OrdinalIgnoreCase);

            foreach (var option in request.Options)
            {
                var kode = option.OptionCode.Trim();

                // Pilihan baru yang langsung ditandai kritis juga merupakan perubahan batas
                // kritis, bukan sekadar penambahan daftar.
                if (!sekarang.TryGetValue(kode, out var kritisSekarang))
                {
                    if (option.IsCritical) throw new LabValueBoundValidationException(pesan);
                    continue;
                }

                if (option.IsCritical != kritisSekarang)
                    throw new LabValueBoundValidationException(pesan);
            }

            // Menghapus pilihan yang bertanda kritis sama dengan mencabut batas kritis.
            var kodeDikirim = request.Options
                .Select(x => x.OptionCode.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (sekarang.Any(x => x.Value && !kodeDikirim.Contains(x.Key)))
                throw new LabValueBoundValidationException(pesan);
        }

        // =================================================================
        // Pembantu
        // =================================================================

        private void ReplaceOptions(
            LabValueBound entity,
            List<LabValueOptionRequest> options,
            Guid actorUserId,
            string? changeReason,
            DateTime now)
        {
            var sebelumnya = entity.Options.Where(x => !x.IsDelete).ToList();

            var lama = string.Join(
                ", ",
                sebelumnya.OrderBy(x => x.SortOrder).Select(x => x.OptionCode));

            var baru = string.Join(
                ", ",
                options.OrderBy(x => x.SortOrder).Select(x => x.OptionCode.Trim()));

            RecordChange(entity, nameof(LabValueBound.Options), lama, baru, actorUserId, changeReason, now);

            // Baris lama dihapus lewat DbSet, dan baris baru ditambahkan lewat DbSet pula
            // dengan ValueBoundId yang ditulis eksplisit.
            //
            // Koleksi navigasi sengaja tidak disentuh. Mengosongkannya sesudah RemoveRange
            // memicu fixup relasi terhadap baris yang sudah ditandai hapus, dan EF kemudian
            // mencoba memperbarui baris yang tidak lagi ada.
            _dbContext.LabValueOptions.RemoveRange(sebelumnya);

            foreach (var option in options)
            {
                _dbContext.LabValueOptions.Add(new LabValueOption
                {
                    ValueBoundId = entity.Id,
                    OptionCode = option.OptionCode.Trim(),
                    OptionName = option.OptionName.Trim(),
                    IsOutOfReference = option.IsOutOfReference,
                    IsCritical = option.IsCritical,
                    SortOrder = option.SortOrder,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }
        }

        /// <summary>
        /// Menerbitkan satu baris riwayat bila nilainya memang berubah. Nilai yang tidak berubah
        /// sengaja tidak menghasilkan baris, supaya riwayat tetap dapat dibaca sebagai daftar
        /// perubahan yang sesungguhnya terjadi.
        /// </summary>
        private void RecordChange(
            LabValueBound entity,
            string changedField,
            string? oldValue,
            string? newValue,
            Guid actorUserId,
            string? changeReason,
            DateTime now)
        {
            if (string.Equals(oldValue, newValue, StringComparison.Ordinal)) return;

            _dbContext.LabValueBoundHistories.Add(new LabValueBoundHistory
            {
                ValueBoundId = entity.Id,
                ChangedField = changedField,
                OldValue = Truncate(oldValue),
                NewValue = Truncate(newValue),
                ActorUserId = actorUserId,
                // Kosong di sini dan itu memang benar: yang menempuh persetujuan klinis hanya
                // perubahan batas kritis, dan jalur itu bukan di service ini.
                ApprovedByUserId = null,
                ChangeReason = changeReason,
                OccurredAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }

        private async Task SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (IsUniqueViolation(exception))
            {
                // Penjaga terakhir VAL-21 bila dua permintaan datang bersamaan dan keduanya
                // lolos pemeriksaan awal.
                throw new LabValueBoundConflictException(
                    "Batas nilai untuk kelompok pasien ini sudah ada. Ubah yang sudah ada, jangan membuat baru.");
            }
        }

        private static bool IsUniqueViolation(DbUpdateException exception)
        {
            return exception.InnerException?.GetType().Name == "PostgresException" &&
                   exception.InnerException.Message.Contains("duplicate key value", StringComparison.OrdinalIgnoreCase);
        }

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? Format(decimal? value) =>
            value?.ToString(CultureInfo.InvariantCulture);

        private static string? Format(int? value) =>
            value?.ToString(CultureInfo.InvariantCulture);

        private static string? Truncate(string? value) =>
            value != null && value.Length > 200 ? value[..200] : value;

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }

        private static LabValueBoundListResponse MapListProjection(LabValueBound x) =>
            new()
            {
                Id = x.Id,
                ProcedureId = x.ProcedureId,
                ProcedureCode = x.Procedure != null ? x.Procedure.ProcedureCode : string.Empty,
                ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : string.Empty,
                ResultForm = x.ResultForm.ToString(),
                Unit = x.Unit,
                GenderScope = x.GenderScope.ToString(),
                AgeCategoryId = x.AgeCategoryId,
                AgeCategoryName = x.AgeCategory != null ? x.AgeCategory.AgeCategoryName : null,
                NormalLow = x.NormalLow,
                NormalHigh = x.NormalHigh,
                CriticalLow = x.CriticalLow,
                CriticalHigh = x.CriticalHigh,
                CitoTurnaroundMinutes = x.CitoTurnaroundMinutes,
                IsActive = x.IsActive,
                OptionCount = x.Options.Count(o => !o.IsDelete)
            };

        private static LabValueBoundDetailResponse MapDetail(LabValueBound entity, bool hasPending)
        {
            return new LabValueBoundDetailResponse
            {
                Id = entity.Id,
                ProcedureId = entity.ProcedureId,
                ProcedureCode = entity.Procedure?.ProcedureCode ?? string.Empty,
                ProcedureName = entity.Procedure?.ProcedureName ?? string.Empty,
                ResultForm = entity.ResultForm.ToString(),
                Unit = entity.Unit,
                GenderScope = entity.GenderScope.ToString(),
                AgeCategoryId = entity.AgeCategoryId,
                AgeCategoryName = entity.AgeCategory?.AgeCategoryName,
                NormalLow = entity.NormalLow,
                NormalHigh = entity.NormalHigh,
                CriticalLow = entity.CriticalLow,
                CriticalHigh = entity.CriticalHigh,
                CitoTurnaroundMinutes = entity.CitoTurnaroundMinutes,
                IsActive = entity.IsActive,
                OptionCount = entity.Options.Count(x => !x.IsDelete),
                HasPendingCriticalChangeRequest = hasPending,
                Options = entity.Options
                    .Where(x => !x.IsDelete)
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new LabValueOptionResponse
                    {
                        Id = x.Id,
                        OptionCode = x.OptionCode,
                        OptionName = x.OptionName,
                        IsOutOfReference = x.IsOutOfReference,
                        IsCritical = x.IsCritical,
                        SortOrder = x.SortOrder
                    })
                    .ToList()
            };
        }
    }

    /// <summary>Pelanggaran aturan isi batas nilai. Dipetakan menjadi <c>422</c>.</summary>
    public sealed class LabValueBoundValidationException(string message) : Exception(message);

    /// <summary>Bentrokan dengan baris yang sudah ada. Dipetakan menjadi <c>409</c>.</summary>
    public sealed class LabValueBoundConflictException(string message) : Exception(message);
}
