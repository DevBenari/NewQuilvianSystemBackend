using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Services
{
    /// <summary>
    /// Service pengelola data master peran pelaksana jasa medis (MstMedicalFeeRole)
    /// sesuai spesifikasi MDF-DES-003, MDF-DES-011, MF-DEC-016, dan matriks validasi MDF-VAL-1.0.
    /// </summary>
    public class MedicalFeeRoleService
    {
        private readonly ApplicationDbContext _dbContext;

        public MedicalFeeRoleService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // =====================================================================
        // Query Operations
        // =====================================================================

        /// <summary>
        /// Mengambil daftar peran terpaginasi dengan penyaring dan pengurutan.
        /// </summary>
        public async Task<PagedResult<MedicalFeeRoleResponse>> GetPagedAsync(
            MedicalFeeRolePagedQuery query,
            CancellationToken ct = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 25 : (query.PageSize > 200 ? 200 : query.PageSize);

            var q = _dbContext.MstMedicalFeeRoles
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();
                q = q.Where(x =>
                    x.RoleCode.ToLower().Contains(keyword) ||
                    x.RoleName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.OprTeamRoleMapping != null && x.OprTeamRoleMapping.ToLower().Contains(keyword)));
            }

            if (query.IsActive.HasValue)
            {
                q = q.Where(x => x.IsActive == query.IsActive.Value);
            }

            if (query.IsPrimaryRole.HasValue)
            {
                q = q.Where(x => x.IsPrimaryRole == query.IsPrimaryRole.Value);
            }

            if (query.HasOprTeamRoleMapping.HasValue)
            {
                q = query.HasOprTeamRoleMapping.Value
                    ? q.Where(x => x.OprTeamRoleMapping != null)
                    : q.Where(x => x.OprTeamRoleMapping == null);
            }

            var totalData = await q.CountAsync(ct);

            var sortDirDesc = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            q = (query.SortBy?.ToLowerInvariant()) switch
            {
                "rolename" => sortDirDesc ? q.OrderByDescending(x => x.RoleName) : q.OrderBy(x => x.RoleName),
                "oprteamrolemapping" => sortDirDesc ? q.OrderByDescending(x => x.OprTeamRoleMapping) : q.OrderBy(x => x.OprTeamRoleMapping),
                "isprimaryrole" => sortDirDesc ? q.OrderByDescending(x => x.IsPrimaryRole) : q.OrderBy(x => x.IsPrimaryRole),
                "isactive" => sortDirDesc ? q.OrderByDescending(x => x.IsActive) : q.OrderBy(x => x.IsActive),
                "createdatetime" => sortDirDesc ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime),
                _ => sortDirDesc ? q.OrderByDescending(x => x.RoleCode) : q.OrderBy(x => x.RoleCode),
            };

            var items = await q
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new MedicalFeeRoleResponse
                {
                    Id = x.Id,
                    RoleCode = x.RoleCode,
                    RoleName = x.RoleName,
                    OprTeamRoleMapping = x.OprTeamRoleMapping,
                    IsPrimaryRole = x.IsPrimaryRole,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    SharingRulesCount = x.SharingRules.Count(r => !r.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy,
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy
                })
                .ToListAsync(ct);

            return new PagedResult<MedicalFeeRoleResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>
        /// Mengambil ringkasan statistik master peran jasa medis.
        /// </summary>
        public async Task<MedicalFeeRoleSummaryResponse> GetSummaryAsync(CancellationToken ct = default)
        {
            var rows = await _dbContext.MstMedicalFeeRoles
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .Select(x => new
                {
                    x.IsActive,
                    x.IsPrimaryRole,
                    HasOprMapping = x.OprTeamRoleMapping != null
                })
                .ToListAsync(ct);

            return new MedicalFeeRoleSummaryResponse
            {
                TotalFeeRole = rows.Count,
                ActiveFeeRole = rows.Count(x => x.IsActive),
                InactiveFeeRole = rows.Count(x => !x.IsActive),
                PrimaryFeeRole = rows.Count(x => x.IsPrimaryRole),
                MappedToOprFeeRole = rows.Count(x => x.HasOprMapping)
            };
        }

        /// <summary>
        /// Opsi ringan peran untuk dropdown, lookup, atau filter pada modul lain.
        /// </summary>
        public async Task<List<MedicalFeeRoleOptionResponse>> GetOptionsAsync(
            bool onlyActive = true,
            string? search = null,
            CancellationToken ct = default)
        {
            var q = _dbContext.MstMedicalFeeRoles
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
            {
                q = q.Where(x => x.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                q = q.Where(x =>
                    x.RoleCode.ToLower().Contains(keyword) ||
                    x.RoleName.ToLower().Contains(keyword));
            }

            return await q
                .OrderBy(x => x.RoleCode)
                .Select(x => new MedicalFeeRoleOptionResponse
                {
                    Id = x.Id,
                    RoleCode = x.RoleCode,
                    RoleName = x.RoleName,
                    OprTeamRoleMapping = x.OprTeamRoleMapping,
                    IsPrimaryRole = x.IsPrimaryRole,
                    IsActive = x.IsActive
                })
                .ToListAsync(ct);
        }

        /// <summary>
        /// Mengambil detail satu data peran berdasarkan ID.
        /// </summary>
        public async Task<MedicalFeeRoleResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContext.MstMedicalFeeRoles
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new MedicalFeeRoleResponse
                {
                    Id = x.Id,
                    RoleCode = x.RoleCode,
                    RoleName = x.RoleName,
                    OprTeamRoleMapping = x.OprTeamRoleMapping,
                    IsPrimaryRole = x.IsPrimaryRole,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    SharingRulesCount = x.SharingRules.Count(r => !r.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy,
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy
                })
                .FirstOrDefaultAsync(ct);
        }

        /// <summary>
        /// Mengembalikan konfigurasi filter, opsi sort, enum, dan form field metadata.
        /// </summary>
        public MedicalFeeRoleFilterMetadataResponse GetFilterMetadata()
        {
            var oprRoleNames = Enum.GetNames<OprTeamRole>().ToList();

            return new MedicalFeeRoleFilterMetadataResponse
            {
                DefaultFilter = new MedicalFeeRoleDefaultFilterResponse(),
                SortOptions = new List<MedicalFeeRoleSortOptionResponse>
                {
                    new() { Value = "roleCode", Label = "Kode Peran" },
                    new() { Value = "roleName", Label = "Nama Peran" },
                    new() { Value = "oprTeamRoleMapping", Label = "Pemetaan Kamar Operasi" },
                    new() { Value = "isPrimaryRole", Label = "Peran Utama" },
                    new() { Value = "isActive", Label = "Status Aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal Dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                OprTeamRoleOptions = oprRoleNames,
                QueryParameters = new List<MedicalFeeRoleQueryParamResponse>
                {
                    new() { Name = "search", Type = "string", Required = false, Description = "Pencarian pada kode, nama, deskripsi, atau pemetaan", Example = "OPERATOR" },
                    new() { Name = "isActive", Type = "boolean", Required = false, Description = "Filter berdasarkan status aktif peran", Example = "true" },
                    new() { Name = "isPrimaryRole", Type = "boolean", Required = false, Description = "Filter peran utama pelaksana tunggal", Example = "true" },
                    new() { Name = "hasOprTeamRoleMapping", Type = "boolean", Required = false, Description = "Filter peran yang memiliki tautan OprTeamRole", Example = "true" },
                    new() { Name = "sortBy", Type = "string", Required = false, Description = "Kolom pengurutan", Example = "roleCode" },
                    new() { Name = "sortDirection", Type = "string", Required = false, Description = "Arah pengurutan (asc/desc)", Example = "asc" },
                    new() { Name = "pageNumber", Type = "integer", Required = false, Description = "Nomor halaman (mulai dari 1)", Example = "1" },
                    new() { Name = "pageSize", Type = "integer", Required = false, Description = "Jumlah data per halaman (1-200)", Example = "25" }
                },
                CreateFields = BuildCreateFieldMetadata(oprRoleNames),
                UpdateFields = BuildUpdateFieldMetadata(oprRoleNames),
                ResetButtonLabel = "Reset"
            };
        }

        // =====================================================================
        // Command Operations
        // =====================================================================

        /// <summary>
        /// Membuat peran pelaksana jasa medis baru — aturan MDF-VAL-1.0 §1.1-1.5.
        /// </summary>
        public async Task<MedicalFeeServiceResult<MedicalFeeRoleResponse>> CreateAsync(
            CreateMedicalFeeRoleRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var roleCode = request.RoleCode.Trim().ToUpperInvariant();

            // Aturan 1.3: RoleCode unik di antara baris yang belum dihapus
            var isDuplicateCode = await _dbContext.MstMedicalFeeRoles
                .AnyAsync(x => !x.IsDelete && x.RoleCode == roleCode, ct);

            if (isDuplicateCode)
            {
                return MedicalFeeServiceResult<MedicalFeeRoleResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Kode peran '{roleCode}' sudah digunakan.",
                    "MDF_ROLE_CODE_DUPLICATE");
            }

            // Aturan 1.4 & 1.5: Validasi pemetaan OprTeamRole
            string? normalizedMapping = null;
            if (!string.IsNullOrWhiteSpace(request.OprTeamRoleMapping))
            {
                normalizedMapping = request.OprTeamRoleMapping.Trim();

                if (!Enum.TryParse<OprTeamRole>(normalizedMapping, ignoreCase: false, out _))
                {
                    return MedicalFeeServiceResult<MedicalFeeRoleResponse>.Fail(
                        StatusCodes.Status400BadRequest,
                        $"Nilai pemetaan peran kamar operasi '{normalizedMapping}' tidak valid. Nilai yang diizinkan: {string.Join(", ", Enum.GetNames<OprTeamRole>())}.");
                }

                var isDuplicateMapping = await _dbContext.MstMedicalFeeRoles
                    .AnyAsync(x => !x.IsDelete && x.OprTeamRoleMapping == normalizedMapping, ct);

                if (isDuplicateMapping)
                {
                    return MedicalFeeServiceResult<MedicalFeeRoleResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        $"Pemetaan peran kamar operasi '{normalizedMapping}' sudah dipetakan ke peran lain.",
                        "MDF_ROLE_MAPPING_DUPLICATE");
                }
            }

            var entity = new MstMedicalFeeRole
            {
                Id = Guid.NewGuid(),
                RoleCode = roleCode,
                RoleName = request.RoleName.Trim(),
                OprTeamRoleMapping = normalizedMapping,
                IsPrimaryRole = request.IsPrimaryRole,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                IsActive = request.IsActive ?? true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.MstMedicalFeeRoles.Add(entity);
            await _dbContext.SaveChangesAsync(ct);

            return MedicalFeeServiceResult<MedicalFeeRoleResponse>.Ok(
                MapToResponse(entity, 0),
                "Peran jasa medis berhasil dibuat.",
                StatusCodes.Status201Created);
        }

        /// <summary>
        /// Mengubah data peran pelaksana jasa medis yang ada.
        /// </summary>
        public async Task<MedicalFeeServiceResult<MedicalFeeRoleResponse>> UpdateAsync(
            Guid id,
            UpdateMedicalFeeRoleRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var entity = await _dbContext.MstMedicalFeeRoles
                .Include(x => x.SharingRules)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (entity == null)
            {
                return MedicalFeeServiceResult<MedicalFeeRoleResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Peran jasa medis tidak ditemukan atau sudah dihapus.");
            }

            var roleCode = request.RoleCode.Trim().ToUpperInvariant();

            // Aturan 1.3: RoleCode unik di antara baris yang belum dihapus (kecuali dirinya sendiri)
            var isDuplicateCode = await _dbContext.MstMedicalFeeRoles
                .AnyAsync(x => x.Id != id && !x.IsDelete && x.RoleCode == roleCode, ct);

            if (isDuplicateCode)
            {
                return MedicalFeeServiceResult<MedicalFeeRoleResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Kode peran '{roleCode}' sudah digunakan.",
                    "MDF_ROLE_CODE_DUPLICATE");
            }

            // Aturan 1.4 & 1.5: Validasi pemetaan OprTeamRole
            string? normalizedMapping = null;
            if (!string.IsNullOrWhiteSpace(request.OprTeamRoleMapping))
            {
                normalizedMapping = request.OprTeamRoleMapping.Trim();

                if (!Enum.TryParse<OprTeamRole>(normalizedMapping, ignoreCase: false, out _))
                {
                    return MedicalFeeServiceResult<MedicalFeeRoleResponse>.Fail(
                        StatusCodes.Status400BadRequest,
                        $"Nilai pemetaan peran kamar operasi '{normalizedMapping}' tidak valid. Nilai yang diizinkan: {string.Join(", ", Enum.GetNames<OprTeamRole>())}.");
                }

                var isDuplicateMapping = await _dbContext.MstMedicalFeeRoles
                    .AnyAsync(x => x.Id != id && !x.IsDelete && x.OprTeamRoleMapping == normalizedMapping, ct);

                if (isDuplicateMapping)
                {
                    return MedicalFeeServiceResult<MedicalFeeRoleResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        $"Pemetaan peran kamar operasi '{normalizedMapping}' sudah dipetakan ke peran lain.",
                        "MDF_ROLE_MAPPING_DUPLICATE");
                }
            }

            entity.RoleCode = roleCode;
            entity.RoleName = request.RoleName.Trim();
            entity.OprTeamRoleMapping = normalizedMapping;
            entity.IsPrimaryRole = request.IsPrimaryRole;
            entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(ct);

            return MedicalFeeServiceResult<MedicalFeeRoleResponse>.Ok(
                MapToResponse(entity, entity.SharingRules.Count(r => !r.IsDelete)),
                "Peran jasa medis berhasil diperbarui.");
        }

        /// <summary>
        /// Mengubah status aktif / nonaktif peran pelaksana jasa medis.
        /// </summary>
        public async Task<MedicalFeeServiceResult<MedicalFeeRoleResponse>> ToggleActivationAsync(
            Guid id,
            bool isActive,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var entity = await _dbContext.MstMedicalFeeRoles
                .Include(x => x.SharingRules)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (entity == null)
            {
                return MedicalFeeServiceResult<MedicalFeeRoleResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Peran jasa medis tidak ditemukan atau sudah dihapus.");
            }

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(ct);

            var statusMsg = isActive ? "diaktifkan" : "dinonaktifkan";
            return MedicalFeeServiceResult<MedicalFeeRoleResponse>.Ok(
                MapToResponse(entity, entity.SharingRules.Count(r => !r.IsDelete)),
                $"Peran jasa medis berhasil {statusMsg}.");
        }

        /// <summary>
        /// Menghapus data peran secara soft delete.
        /// Aturan 1.6: Peran yang dipakai baris tarif (MdfSharingRule) tidak dapat dihapus, ditolak dengan kode 422.
        /// </summary>
        public async Task<MedicalFeeServiceResult<bool>> DeleteAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var entity = await _dbContext.MstMedicalFeeRoles
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (entity == null)
            {
                return MedicalFeeServiceResult<bool>.Fail(
                    StatusCodes.Status404NotFound,
                    "Peran jasa medis tidak ditemukan atau sudah dihapus.");
            }

            // Aturan 1.6: Ditolak 422 bila sudah dipakai baris tarif
            var isUsedInRules = await _dbContext.MdfSharingRules
                .AnyAsync(r => r.RoleId == id && !r.IsDelete, ct);

            if (isUsedInRules)
            {
                return MedicalFeeServiceResult<bool>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Peran tidak dapat dihapus karena sudah dipakai dalam baris aturan tarif sharing. Nonaktifkan peran sebagai gantinya.",
                    "MDF_ROLE_IN_USE");
            }

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = actorUserId;

            await _dbContext.SaveChangesAsync(ct);

            return MedicalFeeServiceResult<bool>.Ok(true, "Peran jasa medis berhasil dihapus.");
        }

        // =====================================================================
        // Helpers
        // =====================================================================

        private static MedicalFeeRoleResponse MapToResponse(MstMedicalFeeRole entity, int sharingRulesCount = 0)
        {
            return new MedicalFeeRoleResponse
            {
                Id = entity.Id,
                RoleCode = entity.RoleCode,
                RoleName = entity.RoleName,
                OprTeamRoleMapping = entity.OprTeamRoleMapping,
                IsPrimaryRole = entity.IsPrimaryRole,
                Description = entity.Description,
                IsActive = entity.IsActive,
                SharingRulesCount = sharingRulesCount,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy
            };
        }

        private static List<MedicalFeeRoleFieldMetadataResponse> BuildCreateFieldMetadata(List<string> oprOptions)
        {
            return new List<MedicalFeeRoleFieldMetadataResponse>
            {
                new()
                {
                    Name = "roleCode",
                    Label = "Kode Peran",
                    Section = "Informasi Utama",
                    InputType = "text",
                    IsRequiredOnCreate = true,
                    IsRequiredOnUpdate = true,
                    MaxLength = 30,
                    Description = "Kode peran unik huruf besar (2-30 karakter)",
                    Example = "OPERATOR",
                    SortOrder = 1
                },
                new()
                {
                    Name = "roleName",
                    Label = "Nama Peran",
                    Section = "Informasi Utama",
                    InputType = "text",
                    IsRequiredOnCreate = true,
                    IsRequiredOnUpdate = true,
                    MaxLength = 100,
                    Description = "Nama lengkap peran pelaksana tindakan",
                    Example = "Operator Utama Bedah",
                    SortOrder = 2
                },
                new()
                {
                    Name = "oprTeamRoleMapping",
                    Label = "Pemetaan Kamar Operasi",
                    Section = "Pemetaan Modul",
                    InputType = "select",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    MaxLength = 30,
                    OptionsSource = string.Join(",", oprOptions),
                    Description = "Padanan peran pada tim kamar operasi (OprTeamRole)",
                    Example = "PrimarySurgeon",
                    SortOrder = 3
                },
                new()
                {
                    Name = "isPrimaryRole",
                    Label = "Peran Utama",
                    Section = "Konfigurasi",
                    InputType = "checkbox",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    Description = "Tandai jika peran ini pelaksana utama default",
                    Example = "true",
                    SortOrder = 4
                },
                new()
                {
                    Name = "description",
                    Label = "Deskripsi",
                    Section = "Keterangan",
                    InputType = "textarea",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    MaxLength = 500,
                    Description = "Penjelasan lingkup tanggung jawab peran",
                    Example = "Pelaksana bedah utama penanggung jawab tindakan",
                    SortOrder = 5
                },
                new()
                {
                    Name = "isActive",
                    Label = "Status Aktif",
                    Section = "Konfigurasi",
                    InputType = "checkbox",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = true,
                    Description = "Menandai apakah peran siap digunakan pada tarif sharing",
                    Example = "true",
                    SortOrder = 6
                }
            };
        }

        private static List<MedicalFeeRoleFieldMetadataResponse> BuildUpdateFieldMetadata(List<string> oprOptions)
            => BuildCreateFieldMetadata(oprOptions);
    }
}

