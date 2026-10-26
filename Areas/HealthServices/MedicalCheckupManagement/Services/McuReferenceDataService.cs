using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using E = QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services.McuErrorCodes;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services
{
    /// <summary>
    /// Data referensi MCU: jenis Pemeriksaan MCU beserta isian formulirnya, dan kategori kelayakan
    /// (<c>BE-MCU-006</c>, <c>MCU-API-v1</c> grup Mcu Reference Data, <c>VAL-MCU-09</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Urutan isian adalah nomor butir formulir</b> (<c>FieldNumber</c>), bukan kolom urutan
    /// tampilan generik. Daftar isian selalu disajikan urut nomor itu.
    /// </para>
    /// <para>
    /// <b>Tidak ada yang dihapus fisik.</b> Jenis pemeriksaan dan kategori kelayakan dinonaktifkan;
    /// isian yang dibuang dari formulir ditandai terhapus, karena
    /// <c>McuExaminationResultValue.ExaminationFieldId</c> hasil lama masih menunjuknya.
    /// </para>
    /// <para>
    /// <b>Menukar nomor butir disimpan dua tahap.</b> Index unik <c>(jenis, nomor butir)</c>
    /// PostgreSQL diperiksa per pernyataan, sehingga menukar butir 1 dan 2 dalam satu langkah ditolak
    /// di tengah jalan. Butir yang berpindah diparkir dulu di nomor negatif, lalu diberi nomor akhirnya
    /// — keduanya di dalam satu transaksi.
    /// </para>
    /// </remarks>
    public class McuReferenceDataService
    {
        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;

        private readonly ApplicationDbContext _dbContext;

        public McuReferenceDataService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // =====================================================================
        // Jenis Pemeriksaan MCU
        // =====================================================================

        public async Task<PagedResult<McuExaminationTypeResponse>> GetExaminationTypesAsync(
            McuExaminationTypePagedQuery query,
            CancellationToken cancellationToken)
        {
            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);

            var rows = _dbContext.McuExaminationTypes.AsNoTracking().Where(x => !x.IsDelete);

            var search = Normalize(query.Search)?.ToLower();
            if (search != null)
            {
                rows = rows.Where(x => x.ExaminationTypeCode.ToLower().Contains(search)
                    || x.ExaminationTypeName.ToLower().Contains(search));
            }

            if (query.IsActive.HasValue)
            {
                rows = rows.Where(x => x.IsActive == query.IsActive.Value);
            }

            var totalData = await rows.CountAsync(cancellationToken);

            var items = await rows
                .OrderBy(x => x.ExaminationTypeName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new McuExaminationTypeResponse
                {
                    Id = x.Id,
                    ExaminationTypeCode = x.ExaminationTypeCode,
                    ExaminationTypeName = x.ExaminationTypeName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    FieldCount = x.Fields.Count(f => !f.IsDelete)
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<McuExaminationTypeResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<McuExaminationTypeDetailResponse?> GetExaminationTypeAsync(Guid id, CancellationToken cancellationToken)
        {
            var type = await _dbContext.McuExaminationTypes.AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new McuExaminationTypeDetailResponse
                {
                    Id = x.Id,
                    ExaminationTypeCode = x.ExaminationTypeCode,
                    ExaminationTypeName = x.ExaminationTypeName,
                    Description = x.Description,
                    IsActive = x.IsActive
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (type == null)
                return null;

            var fields = await _dbContext.McuExaminationFields.AsNoTracking()
                .Where(x => x.ExaminationTypeId == id && !x.IsDelete)
                .OrderBy(x => x.FieldNumber)
                .ToListAsync(cancellationToken);

            type.Fields = fields.Select(x => new McuExaminationFieldResponse
            {
                Id = x.Id,
                FieldNumber = x.FieldNumber,
                FieldCode = x.FieldCode,
                FieldLabel = x.FieldLabel,
                FieldType = x.FieldType,
                FieldTypeLabel = FieldTypeLabel(x.FieldType),
                UnitName = x.UnitName,
                OptionValues = x.OptionValues,
                Options = SplitOptions(x.OptionValues),
                IsRequired = x.IsRequired
            }).ToList();
            type.FieldCount = type.Fields.Count;

            return type;
        }

        public async Task<McuResult<McuExaminationTypeDetailResponse>> CreateExaminationTypeAsync(
            CreateMcuExaminationTypeRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var validation = await ValidateExaminationTypeAsync(null, request, cancellationToken);
            if (validation.Error != null)
                return McuResult<McuExaminationTypeDetailResponse>.Invalid(validation.Error.Code, validation.Error.Message);

            var now = DateTime.UtcNow;
            var type = new McuExaminationType
            {
                ExaminationTypeCode = validation.Code,
                ExaminationTypeName = request.ExaminationTypeName.Trim(),
                Description = Normalize(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.McuExaminationTypes.Add(type);
            foreach (var field in validation.Fields)
            {
                _dbContext.McuExaminationFields.Add(NewField(type.Id, field, actorUserId, now));
            }

            var saveError = await SaveAsync(cancellationToken);
            if (saveError != null)
                return McuResult<McuExaminationTypeDetailResponse>.Invalid(saveError.Code, saveError.Message);

            return McuResult<McuExaminationTypeDetailResponse>.Ok((await GetExaminationTypeAsync(type.Id, cancellationToken))!);
        }

        public async Task<McuResult<McuExaminationTypeDetailResponse>> UpdateExaminationTypeAsync(
            Guid id,
            UpdateMcuExaminationTypeRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var type = await _dbContext.McuExaminationTypes
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (type == null)
                return McuResult<McuExaminationTypeDetailResponse>.NotFound("Jenis pemeriksaan MCU tidak ditemukan.");

            var validation = await ValidateExaminationTypeAsync(id, request, cancellationToken);
            if (validation.Error != null)
                return McuResult<McuExaminationTypeDetailResponse>.Invalid(validation.Error.Code, validation.Error.Message);

            var existing = await _dbContext.McuExaminationFields
                .Where(x => x.ExaminationTypeId == id && !x.IsDelete)
                .ToListAsync(cancellationToken);

            // Pasangkan menurut Id, lalu sisanya menurut kode isian.
            var unmatched = existing.ToList();
            var pairs = new List<(ValidField Request, McuExaminationField? Existing)>();
            foreach (var field in validation.Fields)
            {
                var byId = field.Id.HasValue ? unmatched.FirstOrDefault(x => x.Id == field.Id.Value) : null;
                if (byId != null)
                    unmatched.Remove(byId);
                pairs.Add((field, byId));
            }

            for (var i = 0; i < pairs.Count; i++)
            {
                if (pairs[i].Existing != null)
                    continue;

                var byCode = unmatched.FirstOrDefault(x => x.FieldCode == pairs[i].Request.Code);
                if (byCode == null)
                    continue;

                unmatched.Remove(byCode);
                pairs[i] = (pairs[i].Request, byCode);
            }

            // Isian yang sudah dipakai hasil pemeriksaan tidak boleh berganti jenis: nilai "Normal"
            // yang tersimpan tidak bermakna pada isian Angka.
            var retyped = pairs
                .Where(x => x.Existing != null && x.Existing.FieldType != x.Request.Type)
                .Select(x => x.Existing!)
                .ToList();

            if (retyped.Count > 0)
            {
                var retypedIds = retyped.Select(x => x.Id).ToList();
                var inUse = await _dbContext.McuExaminationResultValues.AsNoTracking()
                    .Where(x => retypedIds.Contains(x.ExaminationFieldId) && !x.IsDelete)
                    .Select(x => x.ExaminationFieldId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (inUse != Guid.Empty)
                {
                    var label = retyped.Single(x => x.Id == inUse).FieldLabel;
                    return McuResult<McuExaminationTypeDetailResponse>.Conflict(
                        E.FieldInUse,
                        $"Isian {label} sudah dipakai pada hasil pemeriksaan; jenis isiannya tidak dapat diganti.");
                }
            }

            var now = DateTime.UtcNow;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            // Tahap 1: lepaskan nomor dan kode yang akan berpindah, dan tandai isian yang dibuang.
            var parked = 0;
            foreach (var (field, row) in pairs)
            {
                if (row == null || (row.FieldNumber == field.Number && row.FieldCode == field.Code))
                    continue;

                row.FieldNumber = -(++parked);
                row.FieldCode = $"~{row.Id:N}";
            }

            foreach (var removed in unmatched)
            {
                removed.IsDelete = true;
                removed.DeleteDateTime = now;
                removed.DeleteBy = actorUserId;
            }

            var saveError = await SaveAsync(cancellationToken);
            if (saveError != null)
                return McuResult<McuExaminationTypeDetailResponse>.Invalid(saveError.Code, saveError.Message);

            // Tahap 2: nilai akhir.
            type.ExaminationTypeCode = validation.Code;
            type.ExaminationTypeName = request.ExaminationTypeName.Trim();
            type.Description = Normalize(request.Description);
            type.IsActive = request.IsActive;
            type.UpdateDateTime = now;
            type.UpdateBy = actorUserId;

            foreach (var (field, row) in pairs)
            {
                if (row == null)
                {
                    _dbContext.McuExaminationFields.Add(NewField(id, field, actorUserId, now));
                    continue;
                }

                row.FieldNumber = field.Number;
                row.FieldCode = field.Code;
                row.FieldLabel = field.Label;
                row.FieldType = field.Type;
                row.UnitName = field.Unit;
                row.OptionValues = field.Options;
                row.IsRequired = field.IsRequired;
                row.UpdateDateTime = now;
                row.UpdateBy = actorUserId;
            }

            saveError = await SaveAsync(cancellationToken);
            if (saveError != null)
                return McuResult<McuExaminationTypeDetailResponse>.Invalid(saveError.Code, saveError.Message);

            await transaction.CommitAsync(cancellationToken);

            return McuResult<McuExaminationTypeDetailResponse>.Ok((await GetExaminationTypeAsync(id, cancellationToken))!);
        }

        // =====================================================================
        // Kategori kelayakan
        // =====================================================================

        public async Task<List<McuFitnessCategoryResponse>> GetFitnessCategoriesAsync(
            McuFitnessCategoryQuery query,
            CancellationToken cancellationToken)
        {
            var rows = _dbContext.McuFitnessCategories.AsNoTracking().Where(x => !x.IsDelete);

            if (query.IsActive.HasValue)
            {
                rows = rows.Where(x => x.IsActive == query.IsActive.Value);
            }

            return await rows
                .OrderBy(x => x.FitnessCategoryName)
                .Select(x => ToResponse(x))
                .ToListAsync(cancellationToken);
        }

        public async Task<McuResult<McuFitnessCategoryResponse>> CreateFitnessCategoryAsync(
            CreateMcuFitnessCategoryRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var validation = await ValidateFitnessCategoryAsync(null, request, cancellationToken);
            if (validation.Error != null)
                return McuResult<McuFitnessCategoryResponse>.Invalid(validation.Error.Code, validation.Error.Message);

            var category = new McuFitnessCategory
            {
                FitnessCategoryCode = validation.Code,
                FitnessCategoryName = request.FitnessCategoryName.Trim(),
                Description = Normalize(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.McuFitnessCategories.Add(category);

            var saveError = await SaveAsync(cancellationToken);
            if (saveError != null)
                return McuResult<McuFitnessCategoryResponse>.Invalid(saveError.Code, saveError.Message);

            return McuResult<McuFitnessCategoryResponse>.Ok(ToResponse(category));
        }

        /// <summary>Mengubah kategori, termasuk menonaktifkannya. Kategori tidak pernah dihapus.</summary>
        public async Task<McuResult<McuFitnessCategoryResponse>> UpdateFitnessCategoryAsync(
            Guid id,
            UpdateMcuFitnessCategoryRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var category = await _dbContext.McuFitnessCategories
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (category == null)
                return McuResult<McuFitnessCategoryResponse>.NotFound("Kategori kelayakan tidak ditemukan.");

            var validation = await ValidateFitnessCategoryAsync(id, request, cancellationToken);
            if (validation.Error != null)
                return McuResult<McuFitnessCategoryResponse>.Invalid(validation.Error.Code, validation.Error.Message);

            category.FitnessCategoryCode = validation.Code;
            category.FitnessCategoryName = request.FitnessCategoryName.Trim();
            category.Description = Normalize(request.Description);
            category.IsActive = request.IsActive;
            category.UpdateDateTime = DateTime.UtcNow;
            category.UpdateBy = actorUserId;

            var saveError = await SaveAsync(cancellationToken);
            if (saveError != null)
                return McuResult<McuFitnessCategoryResponse>.Invalid(saveError.Code, saveError.Message);

            return McuResult<McuFitnessCategoryResponse>.Ok(ToResponse(category));
        }

        // =====================================================================
        // Validasi
        // =====================================================================

        private sealed record Failure(string Code, string Message);

        private sealed record ValidField(
            Guid? Id,
            int Number,
            string Code,
            string Label,
            McuExaminationFieldType Type,
            string? Unit,
            string? Options,
            bool IsRequired);

        private sealed record TypeValidation(string Code, List<ValidField> Fields, Failure? Error);

        private sealed record CategoryValidation(string Code, Failure? Error);

        private async Task<TypeValidation> ValidateExaminationTypeAsync(
            Guid? typeId,
            CreateMcuExaminationTypeRequest request,
            CancellationToken cancellationToken)
        {
            static TypeValidation Reject(string code, string message) => new(string.Empty, [], new Failure(code, message));

            var code = request.ExaminationTypeCode?.Trim().ToUpperInvariant() ?? string.Empty;
            var codeError = CheckCode(code, "Kode jenis pemeriksaan", 30);
            if (codeError != null)
                return Reject(E.InvalidRequest, codeError);

            var name = request.ExaminationTypeName?.Trim() ?? string.Empty;
            if (name.Length == 0)
                return Reject(E.InvalidRequest, "Nama jenis pemeriksaan wajib diisi.");
            if (name.Length > 150)
                return Reject(E.InvalidRequest, "Nama jenis pemeriksaan paling banyak 150 karakter.");
            if (Normalize(request.Description)?.Length > 500)
                return Reject(E.InvalidRequest, "Keterangan jenis pemeriksaan paling banyak 500 karakter.");

            var requested = request.Fields ?? [];
            if (requested.Count == 0)
                return Reject(E.InvalidRequest, "Jenis pemeriksaan harus berisi sekurang-kurangnya satu isian.");

            var fields = new List<ValidField>();
            foreach (var field in requested)
            {
                if (field.FieldNumber < 1)
                    return Reject(E.InvalidRequest, "Nomor butir isian harus 1 atau lebih.");

                var fieldCode = field.FieldCode?.Trim().ToUpperInvariant() ?? string.Empty;
                var fieldCodeError = CheckCode(fieldCode, "Kode isian", 50);
                if (fieldCodeError != null)
                    return Reject(E.InvalidRequest, fieldCodeError);

                var label = field.FieldLabel?.Trim() ?? string.Empty;
                if (label.Length == 0)
                    return Reject(E.InvalidRequest, $"Label isian butir {field.FieldNumber} wajib diisi.");
                if (label.Length > 150)
                    return Reject(E.InvalidRequest, $"Label isian butir {field.FieldNumber} paling banyak 150 karakter.");

                if (!Enum.IsDefined(field.FieldType))
                    return Reject(E.InvalidRequest, $"Jenis isian butir {field.FieldNumber} tidak dikenal.");

                var unit = Normalize(field.UnitName);
                if (unit?.Length > 30)
                    return Reject(E.InvalidRequest, $"Satuan isian butir {field.FieldNumber} paling banyak 30 karakter.");

                string? options = null;
                if (field.FieldType == McuExaminationFieldType.Option)
                {
                    var parsed = SplitOptions(field.OptionValues);
                    if (parsed.Count == 0)
                        return Reject(E.Val09, "Isian pilihan wajib berisi daftar pilihan.");

                    options = string.Join('|', parsed);
                    if (options.Length > 1000)
                        return Reject(E.InvalidRequest, $"Daftar pilihan butir {field.FieldNumber} paling banyak 1000 karakter.");
                }

                fields.Add(new ValidField(field.Id, field.FieldNumber, fieldCode, label, field.FieldType, unit, options, field.IsRequired));
            }

            var duplicateNumber = fields.GroupBy(x => x.Number).FirstOrDefault(x => x.Count() > 1);
            if (duplicateNumber != null)
                return Reject(E.InvalidRequest, $"Nomor butir {duplicateNumber.Key} dipakai lebih dari satu isian.");

            var duplicateCode = fields.GroupBy(x => x.Code).FirstOrDefault(x => x.Count() > 1);
            if (duplicateCode != null)
                return Reject(E.InvalidRequest, $"Kode isian {duplicateCode.Key} dipakai lebih dari satu isian.");

            var codeTaken = await _dbContext.McuExaminationTypes.AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.ExaminationTypeCode == code && x.Id != typeId, cancellationToken);
            if (codeTaken)
                return Reject(E.InvalidRequest, $"Kode jenis pemeriksaan {code} sudah dipakai.");

            return new TypeValidation(code, fields.OrderBy(x => x.Number).ToList(), null);
        }

        private async Task<CategoryValidation> ValidateFitnessCategoryAsync(
            Guid? categoryId,
            CreateMcuFitnessCategoryRequest request,
            CancellationToken cancellationToken)
        {
            static CategoryValidation Reject(string message) => new(string.Empty, new Failure(E.InvalidRequest, message));

            var code = request.FitnessCategoryCode?.Trim().ToUpperInvariant() ?? string.Empty;
            var codeError = CheckCode(code, "Kode kategori kelayakan", 30);
            if (codeError != null)
                return Reject(codeError);

            var name = request.FitnessCategoryName?.Trim() ?? string.Empty;
            if (name.Length == 0)
                return Reject("Nama kategori kelayakan wajib diisi.");
            if (name.Length > 100)
                return Reject("Nama kategori kelayakan paling banyak 100 karakter.");
            if (Normalize(request.Description)?.Length > 500)
                return Reject("Keterangan kategori kelayakan paling banyak 500 karakter.");

            var codeTaken = await _dbContext.McuFitnessCategories.AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.FitnessCategoryCode == code && x.Id != categoryId, cancellationToken);
            if (codeTaken)
                return Reject($"Kode kategori kelayakan {code} sudah dipakai.");

            return new CategoryValidation(code, null);
        }

        // =====================================================================
        // Penolong
        // =====================================================================

        private static string? CheckCode(string code, string label, int maxLength)
        {
            if (code.Length == 0)
                return $"{label} wajib diisi.";
            if (code.Length > maxLength)
                return $"{label} paling banyak {maxLength} karakter.";
            if (code.Any(char.IsWhiteSpace))
                return $"{label} tidak boleh mengandung spasi.";
            return null;
        }

        private static McuExaminationField NewField(Guid typeId, ValidField field, Guid actorUserId, DateTime now) => new()
        {
            ExaminationTypeId = typeId,
            FieldNumber = field.Number,
            FieldCode = field.Code,
            FieldLabel = field.Label,
            FieldType = field.Type,
            UnitName = field.Unit,
            OptionValues = field.Options,
            IsRequired = field.IsRequired,
            CreateDateTime = now,
            CreateBy = actorUserId
        };

        private static McuFitnessCategoryResponse ToResponse(McuFitnessCategory x) => new()
        {
            Id = x.Id,
            FitnessCategoryCode = x.FitnessCategoryCode,
            FitnessCategoryName = x.FitnessCategoryName,
            Description = x.Description,
            IsActive = x.IsActive
        };

        /// <summary>Kode atau nomor butir yang sama disimpan serentak oleh dua admin — index unik menolak yang kedua.</summary>
        private async Task<Failure?> SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return null;
            }
            catch (DbUpdateException exception)
                when ((exception.InnerException as PostgresException)?.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return new Failure(E.InvalidRequest, "Kode atau nomor butir yang sama baru saja disimpan pengguna lain. Muat ulang lalu coba lagi.");
            }
        }

        public static List<string> SplitOptions(string? optionValues) =>
            (optionValues ?? string.Empty)
                .Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .ToList();

        public static string FieldTypeLabel(McuExaminationFieldType type) => type switch
        {
            McuExaminationFieldType.Text => "Teks",
            McuExaminationFieldType.Number => "Angka",
            McuExaminationFieldType.Option => "Pilihan",
            McuExaminationFieldType.YesNo => "Ya/Tidak",
            _ => type.ToString()
        };

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
