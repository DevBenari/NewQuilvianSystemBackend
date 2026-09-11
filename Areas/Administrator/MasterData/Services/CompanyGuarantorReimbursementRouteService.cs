using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Services
{
    public interface ICompanyGuarantorReimbursementRouteService
    {
        Task<CompanyGuarantorReimbursementRouteFilterMetadataResponse> GetFilterMetadataAsync();
        Task<CompanyGuarantorReimbursementRouteSummaryResponse> GetSummaryAsync();
        Task<PagedResult<CompanyGuarantorReimbursementRouteResponse>> GetRoutesPagedAsync(
            Guid? companyGuarantorId,
            Guid? insuranceProviderId,
            string? routeType,
            bool? isDefault,
            bool? isActive,
            string? search,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize);
        Task<List<CompanyGuarantorReimbursementRouteOptionResponse>> GetRouteOptionsAsync(
            Guid? companyGuarantorId,
            string? routeType,
            string? search,
            bool onlyActive,
            int pageNumber,
            int pageSize);
        Task<CompanyGuarantorReimbursementRouteDetailResponse?> GetRouteByIdAsync(Guid id);
        Task<CompanyGuarantorReimbursementRouteResponse> CreateRouteAsync(
            CreateCompanyGuarantorReimbursementRouteRequest request,
            Guid actorUserId);
        Task<CompanyGuarantorReimbursementRouteResponse> UpdateRouteAsync(
            Guid id,
            UpdateCompanyGuarantorReimbursementRouteRequest request,
            Guid actorUserId);
        Task<CompanyGuarantorReimbursementRouteResponse> UpdateRouteStatusAsync(
            Guid id,
            UpdateCompanyGuarantorReimbursementRouteStatusRequest request,
            Guid actorUserId);
        Task<bool> DeleteRouteAsync(Guid id, Guid actorUserId);
    }

    public sealed class CompanyGuarantorReimbursementRouteService : ICompanyGuarantorReimbursementRouteService
    {
        private const string LogCategory = "Administrator.MasterData.CompanyGuarantorReimbursementRoute";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public CompanyGuarantorReimbursementRouteService(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        public async Task<CompanyGuarantorReimbursementRouteFilterMetadataResponse> GetFilterMetadataAsync()
        {
            var result = new CompanyGuarantorReimbursementRouteFilterMetadataResponse
            {
                DefaultFilter = new CompanyGuarantorReimbursementRouteDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<CompanyGuarantorReimbursementRouteSortOptionResponse>
                {
                    new() { Value = "priority", Label = "Prioritas" },
                    new() { Value = "routeType", Label = "Tipe Rute" },
                    new() { Value = "companyGuarantorName", Label = "Perusahaan Penjamin" },
                    new() { Value = "insuranceProviderName", Label = "Asuransi Mitra" },
                    new() { Value = "isDefault", Label = "Rute Bawaan" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal Mulai Berlaku" },
                    new() { Value = "effectiveEndDate", Label = "Tanggal Akhir Berlaku" },
                    new() { Value = "isActive", Label = "Status Aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal Dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                RouteTypeOptions = new List<CompanyGuarantorReimbursementRouteOptionItemResponse>
                {
                    new() { Value = "SELF", Label = "Menanggung Sendiri (SELF)" },
                    new() { Value = "INSURANCE_PROVIDER", Label = "Asuransi Mitra (INSURANCE_PROVIDER)" }
                },
                QueryParameters = BuildQueryParameters(),
                CreateFields = BuildCreateFields(),
                UpdateFields = BuildUpdateFields(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "CompanyGuarantorReimbursementRoute.GetFilterMetadata",
                "Mengambil metadata filter rute reimbursement perusahaan penjamin.",
                result
            );

            return result;
        }

        public async Task<CompanyGuarantorReimbursementRouteSummaryResponse> GetSummaryAsync()
        {
            var query = _dbContext.MstCompanyGuarantorReimbursementRoutes
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var totalRoute = await query.CountAsync();
            var activeRoute = await query.CountAsync(x => x.IsActive);
            var inactiveRoute = await query.CountAsync(x => !x.IsActive);
            var selfRoute = await query.CountAsync(x => x.RouteType == "SELF");
            var insuranceProviderRoute = await query.CountAsync(x => x.RouteType == "INSURANCE_PROVIDER");
            var defaultRoute = await query.CountAsync(x => x.IsDefault && x.IsActive);

            var result = new CompanyGuarantorReimbursementRouteSummaryResponse
            {
                TotalRoute = totalRoute,
                ActiveRoute = activeRoute,
                InactiveRoute = inactiveRoute,
                SelfRoute = selfRoute,
                InsuranceProviderRoute = insuranceProviderRoute,
                DefaultRoute = defaultRoute
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "CompanyGuarantorReimbursementRoute.GetSummary",
                "Mengambil ringkasan rute reimbursement perusahaan penjamin.",
                result
            );

            return result;
        }

        public async Task<PagedResult<CompanyGuarantorReimbursementRouteResponse>> GetRoutesPagedAsync(
            Guid? companyGuarantorId,
            Guid? insuranceProviderId,
            string? routeType,
            bool? isDefault,
            bool? isActive,
            string? search,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            var query = _dbContext.MstCompanyGuarantorReimbursementRoutes
                .AsNoTracking()
                .Include(x => x.CompanyGuarantor)
                .Include(x => x.InsuranceProvider)
                .Where(x => !x.IsDelete);

            if (companyGuarantorId.HasValue && companyGuarantorId.Value != Guid.Empty)
            {
                query = query.Where(x => x.CompanyGuarantorId == companyGuarantorId.Value);
            }

            if (insuranceProviderId.HasValue && insuranceProviderId.Value != Guid.Empty)
            {
                query = query.Where(x => x.InsuranceProviderId == insuranceProviderId.Value);
            }

            if (!string.IsNullOrWhiteSpace(routeType))
            {
                var normalizedRoute = routeType.Trim().ToUpperInvariant();
                query = query.Where(x => x.RouteType == normalizedRoute);
            }

            if (isDefault.HasValue)
            {
                query = query.Where(x => x.IsDefault == isDefault.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.Trim().ToLower();
                query = query.Where(x =>
                    (x.CompanyGuarantor != null && (x.CompanyGuarantor.CompanyGuarantorName.ToLower().Contains(searchLower) ||
                                                    x.CompanyGuarantor.CompanyGuarantorCode.ToLower().Contains(searchLower))) ||
                    (x.InsuranceProvider != null && (x.InsuranceProvider.InsuranceProviderName.ToLower().Contains(searchLower) ||
                                                     x.InsuranceProvider.InsuranceProviderCode.ToLower().Contains(searchLower))) ||
                    (x.Description != null && x.Description.ToLower().Contains(searchLower))
                );
            }

            // Date filtering
            if (startDate.HasValue)
            {
                query = query.Where(x => x.CreateDateTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                var endInclusive = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.CreateDateTime <= endInclusive);
            }

            var totalData = await query.CountAsync();

            // Sorting
            query = ApplySorting(query, sortBy, sortDirection);

            var entities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorIds = entities
                .Select(x => x.CreateBy)
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            var actorNames = await GetActorNameMapAsync(actorIds);

            var items = entities
                .Select(x => MapToResponse(x, actorNames))
                .ToList();

            return new PagedResult<CompanyGuarantorReimbursementRouteResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<List<CompanyGuarantorReimbursementRouteOptionResponse>> GetRouteOptionsAsync(
            Guid? companyGuarantorId,
            string? routeType,
            string? search,
            bool onlyActive,
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 50;

            var query = _dbContext.MstCompanyGuarantorReimbursementRoutes
                .AsNoTracking()
                .Include(x => x.CompanyGuarantor)
                .Include(x => x.InsuranceProvider)
                .Where(x => !x.IsDelete);

            if (onlyActive)
            {
                query = query.Where(x => x.IsActive);
            }

            if (companyGuarantorId.HasValue && companyGuarantorId.Value != Guid.Empty)
            {
                query = query.Where(x => x.CompanyGuarantorId == companyGuarantorId.Value);
            }

            if (!string.IsNullOrWhiteSpace(routeType))
            {
                var normalizedRoute = routeType.Trim().ToUpperInvariant();
                query = query.Where(x => x.RouteType == normalizedRoute);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.Trim().ToLower();
                query = query.Where(x =>
                    (x.CompanyGuarantor != null && x.CompanyGuarantor.CompanyGuarantorName.ToLower().Contains(searchLower)) ||
                    (x.InsuranceProvider != null && x.InsuranceProvider.InsuranceProviderName.ToLower().Contains(searchLower))
                );
            }

            var entities = await query
                .OrderBy(x => x.Priority)
                .ThenBy(x => x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : string.Empty)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return entities.Select(x => new CompanyGuarantorReimbursementRouteOptionResponse
            {
                Id = x.Id,
                CompanyGuarantorId = x.CompanyGuarantorId,
                CompanyGuarantorName = x.CompanyGuarantor?.CompanyGuarantorName ?? string.Empty,
                RouteType = x.RouteType,
                RouteTypeName = BuildRouteTypeLabel(x.RouteType),
                InsuranceProviderId = x.InsuranceProviderId,
                InsuranceProviderName = x.InsuranceProvider?.InsuranceProviderName,
                Priority = x.Priority,
                IsDefault = x.IsDefault,
                IsActive = x.IsActive
            }).ToList();
        }

        public async Task<CompanyGuarantorReimbursementRouteDetailResponse?> GetRouteByIdAsync(Guid id)
        {
            var entity = await _dbContext.MstCompanyGuarantorReimbursementRoutes
                .AsNoTracking()
                .Include(x => x.CompanyGuarantor)
                .Include(x => x.InsuranceProvider)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return null;
            }

            var actorIds = new List<Guid>();
            if (entity.CreateBy != Guid.Empty)
                actorIds.Add(entity.CreateBy);
            if (entity.UpdateBy != Guid.Empty)
                actorIds.Add(entity.UpdateBy);

            var actorNames = await GetActorNameMapAsync(actorIds);

            return MapToDetailResponse(entity, actorNames);
        }

        public async Task<CompanyGuarantorReimbursementRouteResponse> CreateRouteAsync(
            CreateCompanyGuarantorReimbursementRouteRequest request,
            Guid actorUserId)
        {
            var normalizedRouteType = request.RouteType.Trim().ToUpperInvariant();

            // Validasi jenis rute
            if (normalizedRouteType != "SELF" && normalizedRouteType != "INSURANCE_PROVIDER")
            {
                throw new CompanyGuarantorReimbursementRouteValidationException(
                    "Tipe rute tidak valid. Pilih SELF atau INSURANCE_PROVIDER.",
                    400
                );
            }

            // BIL-VAL-087: RouteType = SELF tetapi perusahaan asuransi mitra ikut diisi
            if (normalizedRouteType == "SELF" && request.InsuranceProviderId.HasValue && request.InsuranceProviderId.Value != Guid.Empty)
            {
                throw new CompanyGuarantorReimbursementRouteValidationException(
                    "Perusahaan yang menanggung sendiri tidak memerlukan asuransi mitra.",
                    400
                );
            }

            // BIL-VAL-088: RouteType = INSURANCE_PROVIDER tetapi perusahaan asuransi mitra kosong
            if (normalizedRouteType == "INSURANCE_PROVIDER" && (!request.InsuranceProviderId.HasValue || request.InsuranceProviderId.Value == Guid.Empty))
            {
                throw new CompanyGuarantorReimbursementRouteValidationException(
                    "Pilih perusahaan asuransi mitra untuk rute ini.",
                    400
                );
            }

            // BIL-VAL-089: Perusahaan asuransi mitra yang dipilih sudah tidak aktif
            if (normalizedRouteType == "INSURANCE_PROVIDER")
            {
                var insurance = await _dbContext.MstInsuranceProviders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.InsuranceProviderId!.Value && !x.IsDelete);

                if (insurance == null)
                {
                    throw new CompanyGuarantorReimbursementRouteValidationException(
                        "Perusahaan asuransi yang dipilih tidak ditemukan.",
                        404
                    );
                }

                if (!insurance.IsActive)
                {
                    throw new CompanyGuarantorReimbursementRouteValidationException(
                        "Perusahaan asuransi yang dipilih sudah tidak aktif.",
                        422
                    );
                }
            }

            // Validasi keberadaan dan keaktifan Perusahaan Penjamin
            var company = await _dbContext.MstCompanyGuarantors
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.CompanyGuarantorId && !x.IsDelete);

            if (company == null)
            {
                throw new CompanyGuarantorReimbursementRouteValidationException(
                    "Perusahaan penjamin tidak ditemukan.",
                    404
                );
            }

            if (!company.IsActive)
            {
                throw new CompanyGuarantorReimbursementRouteValidationException(
                    "Perusahaan penjamin yang dipilih sudah tidak aktif.",
                    422
                );
            }

            // BIL-VAL-090: Menandai rute sebagai bawaan padahal perusahaan itu sudah punya rute bawaan aktif lain
            if (request.IsDefault && request.IsActive)
            {
                var existingDefault = await _dbContext.MstCompanyGuarantorReimbursementRoutes
                    .AsNoTracking()
                    .AnyAsync(x => x.CompanyGuarantorId == request.CompanyGuarantorId &&
                                   x.IsDefault &&
                                   x.IsActive &&
                                   !x.IsDelete);

                if (existingDefault)
                {
                    throw new CompanyGuarantorReimbursementRouteValidationException(
                        "Perusahaan ini sudah memiliki rute bawaan. Nonaktifkan yang lama lebih dulu.",
                        422
                    );
                }
            }

            // BIL-VAL-091: Tanggal akhir masa berlaku lebih awal dari tanggal mulai
            if (request.EffectiveStartDate.HasValue && request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                throw new CompanyGuarantorReimbursementRouteValidationException(
                    "Tanggal akhir masa berlaku tidak boleh mendahului tanggal mulai.",
                    400
                );
            }

            var entity = new MstCompanyGuarantorReimbursementRoute
            {
                Id = Guid.NewGuid(),
                CompanyGuarantorId = request.CompanyGuarantorId,
                RouteType = normalizedRouteType,
                InsuranceProviderId = normalizedRouteType == "INSURANCE_PROVIDER" ? request.InsuranceProviderId : null,
                Priority = request.Priority,
                IsDefault = request.IsDefault,
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                Description = NormalizeNullableText(request.Description),
                IsActive = request.IsActive,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId,
                IsDelete = false
            };

            _dbContext.MstCompanyGuarantorReimbursementRoutes.Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "CompanyGuarantorReimbursementRoute.Create",
                $"Rute reimbursement baru berhasil dibuat untuk perusahaan {company.CompanyGuarantorName}.",
                new { entity.Id, entity.RouteType, entity.CompanyGuarantorId, entity.InsuranceProviderId }
            );

            // Re-fetch entity with navigations for clean response
            var loaded = await _dbContext.MstCompanyGuarantorReimbursementRoutes
                .AsNoTracking()
                .Include(x => x.CompanyGuarantor)
                .Include(x => x.InsuranceProvider)
                .FirstAsync(x => x.Id == entity.Id);

            var actorNames = await GetActorNameMapAsync(new[] { actorUserId });
            return MapToResponse(loaded, actorNames);
        }

        public async Task<CompanyGuarantorReimbursementRouteResponse> UpdateRouteAsync(
            Guid id,
            UpdateCompanyGuarantorReimbursementRouteRequest request,
            Guid actorUserId)
        {
            var entity = await _dbContext.MstCompanyGuarantorReimbursementRoutes
                .Include(x => x.CompanyGuarantor)
                .Include(x => x.InsuranceProvider)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                throw new KeyNotFoundException("Data rute reimbursement tidak ditemukan atau sudah dihapus.");
            }

            var normalizedRouteType = request.RouteType.Trim().ToUpperInvariant();

            // Validasi jenis rute
            if (normalizedRouteType != "SELF" && normalizedRouteType != "INSURANCE_PROVIDER")
            {
                throw new CompanyGuarantorReimbursementRouteValidationException(
                    "Tipe rute tidak valid. Pilih SELF atau INSURANCE_PROVIDER.",
                    400
                );
            }

            // BIL-VAL-087: RouteType = SELF tetapi perusahaan asuransi mitra ikut diisi
            if (normalizedRouteType == "SELF" && request.InsuranceProviderId.HasValue && request.InsuranceProviderId.Value != Guid.Empty)
            {
                throw new CompanyGuarantorReimbursementRouteValidationException(
                    "Perusahaan yang menanggung sendiri tidak memerlukan asuransi mitra.",
                    400
                );
            }

            // BIL-VAL-088: RouteType = INSURANCE_PROVIDER tetapi perusahaan asuransi mitra kosong
            if (normalizedRouteType == "INSURANCE_PROVIDER" && (!request.InsuranceProviderId.HasValue || request.InsuranceProviderId.Value == Guid.Empty))
            {
                throw new CompanyGuarantorReimbursementRouteValidationException(
                    "Pilih perusahaan asuransi mitra untuk rute ini.",
                    400
                );
            }

            // BIL-VAL-089: Perusahaan asuransi mitra yang dipilih sudah tidak aktif
            if (normalizedRouteType == "INSURANCE_PROVIDER")
            {
                var insurance = await _dbContext.MstInsuranceProviders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.InsuranceProviderId!.Value && !x.IsDelete);

                if (insurance == null)
                {
                    throw new CompanyGuarantorReimbursementRouteValidationException(
                        "Perusahaan asuransi yang dipilih tidak ditemukan.",
                        404
                    );
                }

                if (!insurance.IsActive)
                {
                    throw new CompanyGuarantorReimbursementRouteValidationException(
                        "Perusahaan asuransi yang dipilih sudah tidak aktif.",
                        422
                    );
                }
            }

            // BIL-VAL-090: Menandai rute sebagai bawaan padahal perusahaan itu sudah punya rute bawaan aktif lain
            if (request.IsDefault && request.IsActive)
            {
                var existingDefault = await _dbContext.MstCompanyGuarantorReimbursementRoutes
                    .AsNoTracking()
                    .AnyAsync(x => x.Id != id &&
                                   x.CompanyGuarantorId == entity.CompanyGuarantorId &&
                                   x.IsDefault &&
                                   x.IsActive &&
                                   !x.IsDelete);

                if (existingDefault)
                {
                    throw new CompanyGuarantorReimbursementRouteValidationException(
                        "Perusahaan ini sudah memiliki rute bawaan. Nonaktifkan yang lama lebih dulu.",
                        422
                    );
                }
            }

            // BIL-VAL-091: Tanggal akhir masa berlaku lebih awal dari tanggal mulai
            if (request.EffectiveStartDate.HasValue && request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                throw new CompanyGuarantorReimbursementRouteValidationException(
                    "Tanggal akhir masa berlaku tidak boleh mendahului tanggal mulai.",
                    400
                );
            }

            entity.RouteType = normalizedRouteType;
            entity.InsuranceProviderId = normalizedRouteType == "INSURANCE_PROVIDER" ? request.InsuranceProviderId : null;
            entity.Priority = request.Priority;
            entity.IsDefault = request.IsDefault;
            entity.EffectiveStartDate = request.EffectiveStartDate;
            entity.EffectiveEndDate = request.EffectiveEndDate;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "CompanyGuarantorReimbursementRoute.Update",
                $"Rute reimbursement {id} berhasil diperbarui.",
                new { entity.Id, entity.RouteType, entity.CompanyGuarantorId, entity.InsuranceProviderId }
            );

            // Re-fetch entity with navigations
            var loaded = await _dbContext.MstCompanyGuarantorReimbursementRoutes
                .AsNoTracking()
                .Include(x => x.CompanyGuarantor)
                .Include(x => x.InsuranceProvider)
                .FirstAsync(x => x.Id == entity.Id);

            var actorIds = new List<Guid>();
            if (loaded.CreateBy != Guid.Empty) actorIds.Add(loaded.CreateBy);
            if (loaded.UpdateBy != Guid.Empty) actorIds.Add(loaded.UpdateBy);

            var actorNames = await GetActorNameMapAsync(actorIds);
            return MapToResponse(loaded, actorNames);
        }

        public async Task<CompanyGuarantorReimbursementRouteResponse> UpdateRouteStatusAsync(
            Guid id,
            UpdateCompanyGuarantorReimbursementRouteStatusRequest request,
            Guid actorUserId)
        {
            var entity = await _dbContext.MstCompanyGuarantorReimbursementRoutes
                .Include(x => x.CompanyGuarantor)
                .Include(x => x.InsuranceProvider)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                throw new KeyNotFoundException("Data rute reimbursement tidak ditemukan atau sudah dihapus.");
            }

            // Jika rute diaktifkan dan berstatus bawaan (default), periksa apakah ada default lain yang aktif
            if (request.IsActive && entity.IsDefault)
            {
                var existingDefault = await _dbContext.MstCompanyGuarantorReimbursementRoutes
                    .AsNoTracking()
                    .AnyAsync(x => x.Id != id &&
                                   x.CompanyGuarantorId == entity.CompanyGuarantorId &&
                                   x.IsDefault &&
                                   x.IsActive &&
                                   !x.IsDelete);

                if (existingDefault)
                {
                    throw new CompanyGuarantorReimbursementRouteValidationException(
                        "Perusahaan ini sudah memiliki rute bawaan. Nonaktifkan yang lama lebih dulu.",
                        422
                    );
                }
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "CompanyGuarantorReimbursementRoute.UpdateStatus",
                $"Status rute reimbursement {id} diubah menjadi {(request.IsActive ? "Aktif" : "Nonaktif")}.",
                new { entity.Id, entity.IsActive }
            );

            var actorIds = new List<Guid>();
            if (entity.CreateBy != Guid.Empty) actorIds.Add(entity.CreateBy);
            if (entity.UpdateBy != Guid.Empty) actorIds.Add(entity.UpdateBy);

            var actorNames = await GetActorNameMapAsync(actorIds);
            return MapToResponse(entity, actorNames);
        }

        public async Task<bool> DeleteRouteAsync(Guid id, Guid actorUserId)
        {
            var entity = await _dbContext.MstCompanyGuarantorReimbursementRoutes
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                throw new KeyNotFoundException("Data rute reimbursement tidak ditemukan atau sudah dihapus.");
            }

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = entity.DeleteDateTime;
            entity.UpdateBy = entity.DeleteBy;

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "CompanyGuarantorReimbursementRoute.Delete",
                $"Rute reimbursement {id} berhasil dihapus (soft-delete).",
                new { entity.Id, entity.CompanyGuarantorId }
            );

            return true;
        }

        private static IQueryable<MstCompanyGuarantorReimbursementRoute> ApplySorting(
            IQueryable<MstCompanyGuarantorReimbursementRoute> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy?.ToLower()) switch
            {
                "routetype" => isDescending ? query.OrderByDescending(x => x.RouteType) : query.OrderBy(x => x.RouteType),
                "companyguarantorname" => isDescending
                    ? query.OrderByDescending(x => x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : string.Empty)
                    : query.OrderBy(x => x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : string.Empty),
                "insuranceprovidername" => isDescending
                    ? query.OrderByDescending(x => x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty)
                    : query.OrderBy(x => x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty),
                "isdefault" => isDescending ? query.OrderByDescending(x => x.IsDefault) : query.OrderBy(x => x.IsDefault),
                "effectivestartdate" => isDescending ? query.OrderByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.EffectiveStartDate),
                "effectiveenddate" => isDescending ? query.OrderByDescending(x => x.EffectiveEndDate) : query.OrderBy(x => x.EffectiveEndDate),
                "isactive" => isDescending ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                "createdatetime" => isDescending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDescending ? query.OrderByDescending(x => x.Priority) : query.OrderBy(x => x.Priority)
            };
        }

        private async Task<IReadOnlyDictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> actorIds)
        {
            var ids = actorIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (!ids.Any()) return new Dictionary<Guid, string?>();

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => (string?)x.Name);
        }

        private static CompanyGuarantorReimbursementRouteResponse MapToResponse(
            MstCompanyGuarantorReimbursementRoute entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            string? createByName = null;
            if (entity.CreateBy != Guid.Empty && actorNames.TryGetValue(entity.CreateBy, out var cName))
            {
                createByName = cName;
            }

            return new CompanyGuarantorReimbursementRouteResponse
            {
                Id = entity.Id,
                CompanyGuarantorId = entity.CompanyGuarantorId,
                CompanyGuarantorCode = entity.CompanyGuarantor?.CompanyGuarantorCode ?? string.Empty,
                CompanyGuarantorName = entity.CompanyGuarantor?.CompanyGuarantorName ?? string.Empty,
                RouteType = entity.RouteType,
                RouteTypeName = BuildRouteTypeLabel(entity.RouteType),
                InsuranceProviderId = entity.InsuranceProviderId,
                InsuranceProviderCode = entity.InsuranceProvider?.InsuranceProviderCode,
                InsuranceProviderName = entity.InsuranceProvider?.InsuranceProviderName,
                Priority = entity.Priority,
                IsDefault = entity.IsDefault,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy,
                CreateByName = createByName
            };
        }

        private static CompanyGuarantorReimbursementRouteDetailResponse MapToDetailResponse(
            MstCompanyGuarantorReimbursementRoute entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            string? createByName = null;
            if (entity.CreateBy != Guid.Empty && actorNames.TryGetValue(entity.CreateBy, out var cName))
            {
                createByName = cName;
            }

            string? updateByName = null;
            if (entity.UpdateBy != Guid.Empty && actorNames.TryGetValue(entity.UpdateBy, out var uName))
            {
                updateByName = uName;
            }

            return new CompanyGuarantorReimbursementRouteDetailResponse
            {
                Id = entity.Id,
                CompanyGuarantorId = entity.CompanyGuarantorId,
                CompanyGuarantorCode = entity.CompanyGuarantor?.CompanyGuarantorCode ?? string.Empty,
                CompanyGuarantorName = entity.CompanyGuarantor?.CompanyGuarantorName ?? string.Empty,
                RouteType = entity.RouteType,
                RouteTypeName = BuildRouteTypeLabel(entity.RouteType),
                InsuranceProviderId = entity.InsuranceProviderId,
                InsuranceProviderCode = entity.InsuranceProvider?.InsuranceProviderCode,
                InsuranceProviderName = entity.InsuranceProvider?.InsuranceProviderName,
                Priority = entity.Priority,
                IsDefault = entity.IsDefault,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy,
                CreateByName = createByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy,
                UpdateByName = updateByName
            };
        }

        private static string BuildRouteTypeLabel(string routeType)
        {
            return routeType switch
            {
                "SELF" => "Menanggung Sendiri (SELF)",
                "INSURANCE_PROVIDER" => "Asuransi Mitra (INSURANCE_PROVIDER)",
                _ => routeType
            };
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static List<CompanyGuarantorReimbursementRouteCustomPeriodResponse> BuildCustomPeriodOptions()
        {
            return new List<CompanyGuarantorReimbursementRouteCustomPeriodResponse>
            {
                new() { Value = "today", Label = "Hari Ini", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 Hari Terakhir", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 Hari Terakhir", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thisMonth", Label = "Bulan Ini", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "custom", Label = "Kustom Tanggal", UsesStartDate = true, UsesEndDate = true }
            };
        }

        private static List<CompanyGuarantorReimbursementRouteQueryParameterResponse> BuildQueryParameters()
        {
            return new List<CompanyGuarantorReimbursementRouteQueryParameterResponse>
            {
                new() { Name = "companyGuarantorId", Type = "Guid", IsRequired = false, Description = "Saring rute berdasarkan id perusahaan penjamin.", Example = "a6c9c612-9c17-48f0-b0c4-904128f115a3" },
                new() { Name = "insuranceProviderId", Type = "Guid", IsRequired = false, Description = "Saring rute berdasarkan id asuransi mitra.", Example = "b1c2d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e" },
                new() { Name = "routeType", Type = "string", IsRequired = false, Description = "Saring berdasarkan tipe rute: SELF atau INSURANCE_PROVIDER.", Example = "SELF" },
                new() { Name = "isDefault", Type = "bool", IsRequired = false, Description = "Saring berdasarkan status rute bawaan (default).", Example = "true" },
                new() { Name = "isActive", Type = "bool", IsRequired = false, Description = "Saring berdasarkan status aktif.", Example = "true" },
                new() { Name = "search", Type = "string", IsRequired = false, Description = "Pencarian kata kunci nama/kode penjamin, mitra, atau keterangan.", Example = "Pertamina" },
                new() { Name = "startDate", Type = "DateTime", IsRequired = false, Description = "Awal rentang tanggal pembuatan rute.", Example = "2026-09-01" },
                new() { Name = "endDate", Type = "DateTime", IsRequired = false, Description = "Akhir rentang tanggal pembuatan rute.", Example = "2026-09-30" },
                new() { Name = "customPeriod", Type = "string", IsRequired = false, Description = "Periode siap pakai (today, last7days, last30days, thisMonth).", Example = "thisMonth" },
                new() { Name = "sortBy", Type = "string", IsRequired = false, Description = "Kolom pengurutan (priority, routeType, companyGuarantorName, insuranceProviderName, isDefault, isActive, createDateTime).", Example = "priority" },
                new() { Name = "sortDirection", Type = "string", IsRequired = false, Description = "Arah pengurutan (asc, desc).", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", IsRequired = false, Description = "Nomor halaman data (mulai dari 1).", Example = "1" },
                new() { Name = "pageSize", Type = "int", IsRequired = false, Description = "Jumlah data per halaman (10, 25, 50, 100).", Example = "25" }
            };
        }

        private static List<CompanyGuarantorReimbursementRouteFormFieldResponse> BuildCreateFields()
        {
            return new List<CompanyGuarantorReimbursementRouteFormFieldResponse>
            {
                new() { Name = "companyGuarantorId", Label = "Perusahaan Penjamin", Section = "Informasi Utama", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = false, RequiredType = "required", OptionsSource = "/api/v1/administrator/master-data/company-guarantors/options", Description = "Pilih perusahaan penjamin sasaran rute.", SortOrder = 1 },
                new() { Name = "routeType", Label = "Tipe Rute", Section = "Informasi Utama", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "required", OptionsSource = "RouteTypeOptions", Description = "Pilih SELF (menanggung sendiri) atau INSURANCE_PROVIDER (lewat asuransi mitra).", SortOrder = 2 },
                new() { Name = "insuranceProviderId", Label = "Asuransi Mitra", Section = "Informasi Utama", InputType = "select", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "conditional", OptionsSource = "/api/v1/administrator/master-data/insurance-providers/options", Description = "Wajib diisi jika tipe rute INSURANCE_PROVIDER; harus kosong jika SELF.", SortOrder = 3 },
                new() { Name = "priority", Label = "Prioritas", Section = "Konfigurasi Rute", InputType = "number", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "required", Description = "Urutan prioritas pencocokan rute (angka lebih kecil = prioritas lebih tinggi).", Example = "1", SortOrder = 4 },
                new() { Name = "isDefault", Label = "Rute Bawaan", Section = "Konfigurasi Rute", InputType = "switch", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", Description = "Tandai apakah rute ini adalah rute bawaan (maksimal 1 per perusahaan).", SortOrder = 5 },
                new() { Name = "effectiveStartDate", Label = "Tanggal Mulai Berlaku", Section = "Masa Berlaku", InputType = "date", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", Description = "Tanggal mulai berlakunya rute reimbursement.", SortOrder = 6 },
                new() { Name = "effectiveEndDate", Label = "Tanggal Akhir Berlaku", Section = "Masa Berlaku", InputType = "date", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", Description = "Tanggal berakhirnya masa berlaku rute reimbursement.", SortOrder = 7 },
                new() { Name = "description", Label = "Keterangan", Section = "Tambahan", InputType = "textarea", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", MaxLength = 500, Description = "Catatan atau keterangan tambahan rute reimbursement.", SortOrder = 8 },
                new() { Name = "isActive", Label = "Status Aktif", Section = "Tambahan", InputType = "switch", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", Description = "Status aktif atau nonaktif rute.", SortOrder = 9 }
            };
        }

        private static List<CompanyGuarantorReimbursementRouteFormFieldResponse> BuildUpdateFields()
        {
            return new List<CompanyGuarantorReimbursementRouteFormFieldResponse>
            {
                new() { Name = "routeType", Label = "Tipe Rute", Section = "Informasi Utama", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "required", OptionsSource = "RouteTypeOptions", Description = "Pilih SELF atau INSURANCE_PROVIDER.", SortOrder = 1 },
                new() { Name = "insuranceProviderId", Label = "Asuransi Mitra", Section = "Informasi Utama", InputType = "select", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "conditional", OptionsSource = "/api/v1/administrator/master-data/insurance-providers/options", Description = "Wajib jika INSURANCE_PROVIDER; harus kosong jika SELF.", SortOrder = 2 },
                new() { Name = "priority", Label = "Prioritas", Section = "Konfigurasi Rute", InputType = "number", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "required", Description = "Urutan prioritas pencocokan rute.", Example = "1", SortOrder = 3 },
                new() { Name = "isDefault", Label = "Rute Bawaan", Section = "Konfigurasi Rute", InputType = "switch", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", Description = "Tandai apakah rute ini rute bawaan perusahaan.", SortOrder = 4 },
                new() { Name = "effectiveStartDate", Label = "Tanggal Mulai Berlaku", Section = "Masa Berlaku", InputType = "date", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", Description = "Tanggal mulai berlakunya rute.", SortOrder = 5 },
                new() { Name = "effectiveEndDate", Label = "Tanggal Akhir Berlaku", Section = "Masa Berlaku", InputType = "date", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", Description = "Tanggal berakhirnya rute.", SortOrder = 6 },
                new() { Name = "description", Label = "Keterangan", Section = "Tambahan", InputType = "textarea", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", MaxLength = 500, Description = "Keterangan tambahan rute reimbursement.", SortOrder = 7 },
                new() { Name = "isActive", Label = "Status Aktif", Section = "Tambahan", InputType = "switch", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", Description = "Status aktif atau nonaktif rute.", SortOrder = 8 }
            };
        }
    }
}
