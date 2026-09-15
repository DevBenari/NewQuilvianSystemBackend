using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services
{
    public interface ICompanyGuarantorCoverageRuleService
    {
        Task<CompanyGuarantorCoverageRuleFilterMetadataResponse> GetFilterMetadataAsync();
        Task<CompanyGuarantorCoverageRuleSummaryResponse> GetSummaryAsync();
        Task<PagedResult<CompanyGuarantorCoverageRuleResponse>> GetRulesPagedAsync(
            Guid? companyGuarantorId,
            string? itemType,
            string? coverageStatus,
            string? benefitPlanCode,
            string? employeeGrade,
            bool? isActive,
            string? search,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize);
        Task<List<CompanyGuarantorCoverageRuleOptionResponse>> GetRuleOptionsAsync(
            Guid? companyGuarantorId,
            string? itemType,
            string? search,
            bool onlyActive,
            int pageNumber,
            int pageSize);
        Task<CompanyGuarantorCoverageRuleDetailResponse?> GetRuleByIdAsync(Guid id);
        Task<CompanyGuarantorCoverageRuleResponse> CreateRuleAsync(
            CreateCompanyGuarantorCoverageRuleRequest request,
            Guid actorUserId);
        Task<CompanyGuarantorCoverageRuleResponse> UpdateRuleAsync(
            Guid id,
            UpdateCompanyGuarantorCoverageRuleRequest request,
            Guid actorUserId);
        Task<CompanyGuarantorCoverageRuleResponse> UpdateRuleStatusAsync(
            Guid id,
            UpdateCompanyGuarantorCoverageRuleStatusRequest request,
            Guid actorUserId);
        Task<bool> DeleteRuleAsync(
            Guid id,
            string? deleteReason,
            Guid actorUserId);
    }

    public sealed class CompanyGuarantorCoverageRuleService : ICompanyGuarantorCoverageRuleService
    {
        private const string LogCategory = "HealthServices.MasterData.CompanyGuarantorCoverageRule";
        private const string RuleCodePrefix = "CCR-RSMMC-";
        private const int RuleCodeDigitLength = 5;

        private static readonly HashSet<string> AllowedItemTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Tariff",
            "Drug",
            "DrugCategory",
            "Procedure",
            "TariffCategory",
            "ServiceCategory"
        };

        private static readonly HashSet<string> AllowedCoverageStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Covered",
            "NotCovered",
            "PartialCovered",
            "NeedApproval"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public CompanyGuarantorCoverageRuleService(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        public async Task<CompanyGuarantorCoverageRuleFilterMetadataResponse> GetFilterMetadataAsync()
        {
            var result = new CompanyGuarantorCoverageRuleFilterMetadataResponse
            {
                DefaultFilter = new CompanyGuarantorCoverageRuleDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<CompanyGuarantorCoverageRuleSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "priority", Label = "Prioritas" },
                    new() { Value = "ruleCode", Label = "Kode Aturan" },
                    new() { Value = "ruleName", Label = "Nama Aturan" },
                    new() { Value = "companyGuarantorName", Label = "Perusahaan Penjamin" },
                    new() { Value = "itemType", Label = "Tipe Item" },
                    new() { Value = "coverageStatus", Label = "Status Tanggungan" },
                    new() { Value = "coveragePercent", Label = "Persentase Tanggungan" },
                    new() { Value = "employeeGrade", Label = "Golongan Karyawan" },
                    new() { Value = "benefitPlanCode", Label = "Paket Manfaat" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal Mulai Berlaku" },
                    new() { Value = "effectiveEndDate", Label = "Tanggal Akhir Berlaku" },
                    new() { Value = "isActive", Label = "Status Aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal Dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ItemTypeOptions = new List<CompanyGuarantorCoverageRuleOptionItemResponse>
                {
                    new() { Value = "Tariff", Label = "Tarif / Tindakan Spesifik (Tariff)" },
                    new() { Value = "Drug", Label = "Obat / Farmasi Spesifik (Drug)" },
                    new() { Value = "DrugCategory", Label = "Kategori Obat (DrugCategory)" },
                    new() { Value = "Procedure", Label = "Prosedur Medis (Procedure)" },
                    new() { Value = "TariffCategory", Label = "Kategori Layanan / Tarif (TariffCategory)" }
                },
                CoverageStatusOptions = new List<CompanyGuarantorCoverageRuleOptionItemResponse>
                {
                    new() { Value = "Covered", Label = "Ditanggung Penuh / Sesuai Porsi (Covered)" },
                    new() { Value = "NotCovered", Label = "Tidak Ditanggung (NotCovered)" },
                    new() { Value = "PartialCovered", Label = "Ditanggung Sebagian (PartialCovered)" },
                    new() { Value = "NeedApproval", Label = "Perlu Persetujuan Khusus (NeedApproval)" }
                },
                QueryParameters = BuildQueryParameters(),
                CreateFields = BuildCreateFields(),
                UpdateFields = BuildUpdateFields(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "CompanyGuarantorCoverageRule.GetFilterMetadata",
                "Mengambil metadata filter aturan tanggungan perusahaan penjamin.",
                result
            );

            return result;
        }

        public async Task<CompanyGuarantorCoverageRuleSummaryResponse> GetSummaryAsync()
        {
            var query = _dbContext.MstCompanyGuarantorCoverageRules
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var totalRule = await query.CountAsync();
            var activeRule = await query.CountAsync(x => x.IsActive);
            var inactiveRule = await query.CountAsync(x => !x.IsActive);
            var coveredRule = await query.CountAsync(x => x.CoverageStatus == "Covered");
            var notCoveredRule = await query.CountAsync(x => x.CoverageStatus == "NotCovered");
            var partialCoveredRule = await query.CountAsync(x => x.CoverageStatus == "PartialCovered");
            var needApprovalRule = await query.CountAsync(x => x.CoverageStatus == "NeedApproval");

            var result = new CompanyGuarantorCoverageRuleSummaryResponse
            {
                TotalRule = totalRule,
                ActiveRule = activeRule,
                InactiveRule = inactiveRule,
                CoveredRule = coveredRule,
                NotCoveredRule = notCoveredRule,
                PartialCoveredRule = partialCoveredRule,
                NeedApprovalRule = needApprovalRule
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "CompanyGuarantorCoverageRule.GetSummary",
                "Mengambil ringkasan aturan tanggungan perusahaan penjamin.",
                result
            );

            return result;
        }

        public async Task<PagedResult<CompanyGuarantorCoverageRuleResponse>> GetRulesPagedAsync(
            Guid? companyGuarantorId,
            string? itemType,
            string? coverageStatus,
            string? benefitPlanCode,
            string? employeeGrade,
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
            var (normalizedPageNumber, normalizedPageSize) = NormalizePaging(pageNumber, pageSize);

            var query = _dbContext.MstCompanyGuarantorCoverageRules
                .AsNoTracking()
                .Include(x => x.CompanyGuarantor)
                .Include(x => x.Tariff)
                .Include(x => x.Drug)
                .Include(x => x.DrugCategory)
                .Include(x => x.Procedure)
                .Include(x => x.TariffCategory)
                .Include(x => x.PatientClass)
                .Where(x => !x.IsDelete);

            if (companyGuarantorId.HasValue && companyGuarantorId.Value != Guid.Empty)
                query = query.Where(x => x.CompanyGuarantorId == companyGuarantorId.Value);

            if (!string.IsNullOrWhiteSpace(itemType))
            {
                var normalizedItemType = itemType.Trim().ToLower();
                query = query.Where(x => x.ItemType.ToLower() == normalizedItemType);
            }

            if (!string.IsNullOrWhiteSpace(coverageStatus))
            {
                var normalizedCoverageStatus = coverageStatus.Trim().ToLower();
                query = query.Where(x => x.CoverageStatus.ToLower() == normalizedCoverageStatus);
            }

            if (!string.IsNullOrWhiteSpace(benefitPlanCode))
            {
                var normalizedPlan = benefitPlanCode.Trim().ToLower();
                query = query.Where(x => x.BenefitPlanCode != null && x.BenefitPlanCode.ToLower().Contains(normalizedPlan));
            }

            if (!string.IsNullOrWhiteSpace(employeeGrade))
            {
                var normalizedGrade = employeeGrade.Trim().ToLower();
                query = query.Where(x => x.EmployeeGrade != null && x.EmployeeGrade.ToLower().Contains(normalizedGrade));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.RuleCode.ToLower().Contains(keyword) ||
                    x.RuleName.ToLower().Contains(keyword) ||
                    x.ItemType.ToLower().Contains(keyword) ||
                    x.CoverageStatus.ToLower().Contains(keyword) ||
                    (x.EmployeeGrade != null && x.EmployeeGrade.ToLower().Contains(keyword)) ||
                    (x.BenefitPlanCode != null && x.BenefitPlanCode.ToLower().Contains(keyword)) ||
                    (x.BenefitPlanName != null && x.BenefitPlanName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.CompanyGuarantor != null && (x.CompanyGuarantor.CompanyGuarantorCode.ToLower().Contains(keyword) || x.CompanyGuarantor.CompanyGuarantorName.ToLower().Contains(keyword))) ||
                    (x.Tariff != null && (x.Tariff.TariffCode.ToLower().Contains(keyword) || x.Tariff.TariffName.ToLower().Contains(keyword))) ||
                    (x.Drug != null && (x.Drug.DrugCode.ToLower().Contains(keyword) || x.Drug.DrugName.ToLower().Contains(keyword))) ||
                    (x.DrugCategory != null && (x.DrugCategory.DrugCategoryCode.ToLower().Contains(keyword) || x.DrugCategory.DrugCategoryName.ToLower().Contains(keyword))) ||
                    (x.Procedure != null && (x.Procedure.ProcedureCode.ToLower().Contains(keyword) || x.Procedure.ProcedureName.ToLower().Contains(keyword))) ||
                    (x.TariffCategory != null && (x.TariffCategory.TariffCategoryCode.ToLower().Contains(keyword) || x.TariffCategory.TariffCategoryName.ToLower().Contains(keyword))) ||
                    (x.PatientClass != null && (x.PatientClass.PatientClassCode.ToLower().Contains(keyword) || x.PatientClass.PatientClassName.ToLower().Contains(keyword)))
                );
            }

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);

            var totalCount = await query.CountAsync();

            query = ApplySorting(query, sortBy, sortDirection);

            var entities = await query
                .Skip((normalizedPageNumber - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToListAsync();

            var userIds = entities
                .Select(x => x.CreateBy)
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            var userNames = await _dbContext.Users
                .AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => !string.IsNullOrEmpty(x.DisplayName) ? x.DisplayName : (x.UserName ?? x.Id.ToString()));

            var items = entities.Select(x => MapToResponse(x, userNames)).ToList();

            return new PagedResult<CompanyGuarantorCoverageRuleResponse>
            {
                Items = items,
                TotalData = totalCount,
                TotalPage = (int)Math.Ceiling((double)totalCount / normalizedPageSize),
                PageNumber = normalizedPageNumber,
                PageSize = normalizedPageSize
            };
        }

        public async Task<List<CompanyGuarantorCoverageRuleOptionResponse>> GetRuleOptionsAsync(
            Guid? companyGuarantorId,
            string? itemType,
            string? search,
            bool onlyActive,
            int pageNumber,
            int pageSize)
        {
            var (normalizedPageNumber, normalizedPageSize) = NormalizePaging(pageNumber, pageSize);

            var query = _dbContext.MstCompanyGuarantorCoverageRules
                .AsNoTracking()
                .Include(x => x.CompanyGuarantor)
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (companyGuarantorId.HasValue && companyGuarantorId.Value != Guid.Empty)
                query = query.Where(x => x.CompanyGuarantorId == companyGuarantorId.Value);

            if (!string.IsNullOrWhiteSpace(itemType))
            {
                var normalizedItemType = itemType.Trim().ToLower();
                query = query.Where(x => x.ItemType.ToLower() == normalizedItemType);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.RuleCode.ToLower().Contains(keyword) ||
                    x.RuleName.ToLower().Contains(keyword) ||
                    (x.EmployeeGrade != null && x.EmployeeGrade.ToLower().Contains(keyword)) ||
                    (x.BenefitPlanCode != null && x.BenefitPlanCode.ToLower().Contains(keyword)) ||
                    (x.CompanyGuarantor != null && x.CompanyGuarantor.CompanyGuarantorName.ToLower().Contains(keyword))
                );
            }

            return await query
                .OrderBy(x => x.Priority)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.RuleCode)
                .Skip((normalizedPageNumber - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .Select(x => new CompanyGuarantorCoverageRuleOptionResponse
                {
                    Id = x.Id,
                    CompanyGuarantorId = x.CompanyGuarantorId,
                    CompanyGuarantorName = x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : string.Empty,
                    RuleCode = x.RuleCode,
                    RuleName = x.RuleName,
                    ItemType = x.ItemType,
                    CoverageStatus = x.CoverageStatus,
                    CoveragePercent = x.CoveragePercent,
                    CoPaymentPercent = x.CoPaymentPercent,
                    BenefitPlanCode = x.BenefitPlanCode,
                    EmployeeGrade = x.EmployeeGrade,
                    Priority = x.Priority,
                    IsActive = x.IsActive
                })
                .ToListAsync();
        }

        public async Task<CompanyGuarantorCoverageRuleDetailResponse?> GetRuleByIdAsync(Guid id)
        {
            var entity = await _dbContext.MstCompanyGuarantorCoverageRules
                .AsNoTracking()
                .Include(x => x.CompanyGuarantor)
                .Include(x => x.Tariff)
                .Include(x => x.Drug)
                .Include(x => x.DrugCategory)
                .Include(x => x.Procedure)
                .Include(x => x.TariffCategory)
                .Include(x => x.PatientClass)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return null;

            var userIds = new List<Guid>();
            if (entity.CreateBy != Guid.Empty) userIds.Add(entity.CreateBy);
            if (entity.UpdateBy != Guid.Empty) userIds.Add(entity.UpdateBy);

            var userNames = await _dbContext.Users
                .AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => !string.IsNullOrEmpty(x.DisplayName) ? x.DisplayName : (x.UserName ?? x.Id.ToString()));

            return MapToDetailResponse(entity, userNames);
        }

        public async Task<CompanyGuarantorCoverageRuleResponse> CreateRuleAsync(
            CreateCompanyGuarantorCoverageRuleRequest request,
            Guid actorUserId)
        {
            await ValidateBusinessRulesAsync(null, request);

            var now = DateTime.UtcNow;
            var itemType = NormalizeItemType(request.ItemType);
            var ruleCode = string.IsNullOrWhiteSpace(request.RuleCode)
                ? await GenerateRuleCodeAsync()
                : request.RuleCode.Trim();

            // Authoritative server-side derivation untuk urun biaya (BIL-VAL-092 & MPY-DEC-008)
            var coveragePercent = Math.Clamp(request.CoveragePercent, 0m, 100m);
            var coPaymentPercent = Math.Clamp(100m - coveragePercent, 0m, 100m);

            var entity = new MstCompanyGuarantorCoverageRule
            {
                Id = Guid.NewGuid(),
                CompanyGuarantorId = request.CompanyGuarantorId,
                RuleCode = ruleCode,
                RuleName = request.RuleName.Trim(),
                ItemType = itemType,
                TariffId = itemType == "Tariff" ? request.TariffId : null,
                DrugId = itemType == "Drug" ? request.DrugId : null,
                DrugCategoryId = itemType == "DrugCategory" ? request.DrugCategoryId : null,
                ProcedureId = itemType == "Procedure" ? request.ProcedureId : null,
                TariffCategoryId = (itemType == "TariffCategory" || itemType == "ServiceCategory") ? request.TariffCategoryId : null,
                BenefitPlanCode = NormalizeNullableText(request.BenefitPlanCode)?.ToUpperInvariant(),
                BenefitPlanName = NormalizeNullableText(request.BenefitPlanName),
                EmployeeGrade = NormalizeNullableText(request.EmployeeGrade)?.ToUpperInvariant(),
                PatientClassId = request.PatientClassId,
                CoverageStatus = NormalizeCoverageStatus(request.CoverageStatus),
                CoveragePercent = coveragePercent,
                MaxCoverageAmount = request.MaxCoverageAmount,
                CoPaymentPercent = coPaymentPercent,
                CoPaymentAmount = request.CoPaymentAmount,
                IsNeedApproval = request.IsNeedApproval,
                IsNeedGuaranteeLetter = request.IsNeedGuaranteeLetter,
                IsAllowExcessPaymentByPatient = request.IsAllowExcessPaymentByPatient,
                MaxQuantityPerVisit = request.MaxQuantityPerVisit,
                MaxQuantityPerMonth = request.MaxQuantityPerMonth,
                MaxAmountPerVisit = request.MaxAmountPerVisit,
                MaxAmountPerMonth = request.MaxAmountPerMonth,
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                Priority = request.Priority,
                ApprovalInstruction = NormalizeNullableText(request.ApprovalInstruction),
                BillingInstruction = NormalizeNullableText(request.BillingInstruction),
                Description = NormalizeNullableText(request.Description),
                SortOrder = request.SortOrder,
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstCompanyGuarantorCoverageRules.Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "CompanyGuarantorCoverageRule.Create",
                $"Aturan tanggungan perusahaan baru berhasil dibuat dengan kode {entity.RuleCode}.",
                new { entity.Id, entity.RuleCode, entity.CompanyGuarantorId, entity.ItemType, entity.CoveragePercent, entity.CoPaymentPercent }
            );

            var loaded = await _dbContext.MstCompanyGuarantorCoverageRules
                .AsNoTracking()
                .Include(x => x.CompanyGuarantor)
                .Include(x => x.Tariff)
                .Include(x => x.Drug)
                .Include(x => x.DrugCategory)
                .Include(x => x.Procedure)
                .Include(x => x.TariffCategory)
                .Include(x => x.PatientClass)
                .FirstAsync(x => x.Id == entity.Id);

            return MapToResponse(loaded, new Dictionary<Guid, string>());
        }

        public async Task<CompanyGuarantorCoverageRuleResponse> UpdateRuleAsync(
            Guid id,
            UpdateCompanyGuarantorCoverageRuleRequest request,
            Guid actorUserId)
        {
            var entity = await _dbContext.MstCompanyGuarantorCoverageRules
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                throw new CompanyGuarantorCoverageRuleValidationException(
                    "Aturan tanggungan perusahaan penjamin tidak ditemukan.",
                    StatusCodes.Status404NotFound
                );
            }

            await ValidateBusinessRulesAsync(id, request);

            var now = DateTime.UtcNow;
            var itemType = NormalizeItemType(request.ItemType);
            var coveragePercent = Math.Clamp(request.CoveragePercent, 0m, 100m);
            var coPaymentPercent = Math.Clamp(100m - coveragePercent, 0m, 100m);

            if (!string.IsNullOrWhiteSpace(request.RuleCode))
            {
                entity.RuleCode = request.RuleCode.Trim();
            }

            entity.CompanyGuarantorId = request.CompanyGuarantorId;
            entity.RuleName = request.RuleName.Trim();
            entity.ItemType = itemType;
            entity.TariffId = itemType == "Tariff" ? request.TariffId : null;
            entity.DrugId = itemType == "Drug" ? request.DrugId : null;
            entity.DrugCategoryId = itemType == "DrugCategory" ? request.DrugCategoryId : null;
            entity.ProcedureId = itemType == "Procedure" ? request.ProcedureId : null;
            entity.TariffCategoryId = (itemType == "TariffCategory" || itemType == "ServiceCategory") ? request.TariffCategoryId : null;
            entity.BenefitPlanCode = NormalizeNullableText(request.BenefitPlanCode)?.ToUpperInvariant();
            entity.BenefitPlanName = NormalizeNullableText(request.BenefitPlanName);
            entity.EmployeeGrade = NormalizeNullableText(request.EmployeeGrade)?.ToUpperInvariant();
            entity.PatientClassId = request.PatientClassId;
            entity.CoverageStatus = NormalizeCoverageStatus(request.CoverageStatus);
            entity.CoveragePercent = coveragePercent;
            entity.MaxCoverageAmount = request.MaxCoverageAmount;
            entity.CoPaymentPercent = coPaymentPercent;
            entity.CoPaymentAmount = request.CoPaymentAmount;
            entity.IsNeedApproval = request.IsNeedApproval;
            entity.IsNeedGuaranteeLetter = request.IsNeedGuaranteeLetter;
            entity.IsAllowExcessPaymentByPatient = request.IsAllowExcessPaymentByPatient;
            entity.MaxQuantityPerVisit = request.MaxQuantityPerVisit;
            entity.MaxQuantityPerMonth = request.MaxQuantityPerMonth;
            entity.MaxAmountPerVisit = request.MaxAmountPerVisit;
            entity.MaxAmountPerMonth = request.MaxAmountPerMonth;
            entity.EffectiveStartDate = request.EffectiveStartDate;
            entity.EffectiveEndDate = request.EffectiveEndDate;
            entity.Priority = request.Priority;
            entity.ApprovalInstruction = NormalizeNullableText(request.ApprovalInstruction);
            entity.BillingInstruction = NormalizeNullableText(request.BillingInstruction);
            entity.Description = NormalizeNullableText(request.Description);
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "CompanyGuarantorCoverageRule.Update",
                $"Aturan tanggungan perusahaan {entity.RuleCode} berhasil diperbarui.",
                new { entity.Id, entity.RuleCode, entity.CoveragePercent, entity.CoPaymentPercent }
            );

            var loaded = await _dbContext.MstCompanyGuarantorCoverageRules
                .AsNoTracking()
                .Include(x => x.CompanyGuarantor)
                .Include(x => x.Tariff)
                .Include(x => x.Drug)
                .Include(x => x.DrugCategory)
                .Include(x => x.Procedure)
                .Include(x => x.TariffCategory)
                .Include(x => x.PatientClass)
                .FirstAsync(x => x.Id == entity.Id);

            return MapToResponse(loaded, new Dictionary<Guid, string>());
        }

        public async Task<CompanyGuarantorCoverageRuleResponse> UpdateRuleStatusAsync(
            Guid id,
            UpdateCompanyGuarantorCoverageRuleStatusRequest request,
            Guid actorUserId)
        {
            var entity = await _dbContext.MstCompanyGuarantorCoverageRules
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                throw new CompanyGuarantorCoverageRuleValidationException(
                    "Aturan tanggungan perusahaan penjamin tidak ditemukan.",
                    StatusCodes.Status404NotFound
                );
            }

            var now = DateTime.UtcNow;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "CompanyGuarantorCoverageRule.UpdateStatus",
                $"Status aturan tanggungan perusahaan {entity.RuleCode} diubah menjadi {(entity.IsActive ? "Aktif" : "Nonaktif")}.",
                new { entity.Id, entity.RuleCode, entity.IsActive }
            );

            var loaded = await _dbContext.MstCompanyGuarantorCoverageRules
                .AsNoTracking()
                .Include(x => x.CompanyGuarantor)
                .Include(x => x.Tariff)
                .Include(x => x.Drug)
                .Include(x => x.DrugCategory)
                .Include(x => x.Procedure)
                .Include(x => x.TariffCategory)
                .Include(x => x.PatientClass)
                .FirstAsync(x => x.Id == entity.Id);

            return MapToResponse(loaded, new Dictionary<Guid, string>());
        }

        public async Task<bool> DeleteRuleAsync(
            Guid id,
            string? deleteReason,
            Guid actorUserId)
        {
            var entity = await _dbContext.MstCompanyGuarantorCoverageRules
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return false;

            // BIL-VAL-097: Menghapus aturan yang sedang dipakai versi perhitungan yang tersimpan
            var idString = id.ToString();
            var ruleCode = entity.RuleCode;
            var isUsedInCalculations = await _dbContext.BilCalculationVersions
                .AsNoTracking()
                .AnyAsync(x => !x.IsDelete && (
                    EF.Functions.Like(x.BreakdownSnapshot, $"%{idString}%") ||
                    (!string.IsNullOrEmpty(ruleCode) && EF.Functions.Like(x.BreakdownSnapshot, $"%{ruleCode}%"))
                ));

            if (isUsedInCalculations)
            {
                throw new CompanyGuarantorCoverageRuleValidationException(
                    "Aturan ini sudah dipakai pada tagihan yang tersimpan. Nonaktifkan saja, jangan dihapus.",
                    StatusCodes.Status422UnprocessableEntity
                );
            }

            var now = DateTime.UtcNow;
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (!string.IsNullOrWhiteSpace(deleteReason))
                entity.Description = NormalizeNullableText(deleteReason);

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "CompanyGuarantorCoverageRule.Delete",
                $"Aturan tanggungan perusahaan {entity.RuleCode} berhasil dihapus (soft-delete).",
                new { entity.Id, entity.RuleCode, entity.DeleteDateTime }
            );

            return true;
        }

        private async Task ValidateBusinessRulesAsync(
            Guid? excludeId,
            CreateCompanyGuarantorCoverageRuleRequest request)
        {
            if (request.CompanyGuarantorId == Guid.Empty)
            {
                throw new CompanyGuarantorCoverageRuleValidationException(
                    "Perusahaan penjamin wajib dipilih.",
                    StatusCodes.Status400BadRequest
                );
            }

            if (string.IsNullOrWhiteSpace(request.RuleName))
            {
                throw new CompanyGuarantorCoverageRuleValidationException(
                    "Nama aturan wajib diisi.",
                    StatusCodes.Status400BadRequest
                );
            }

            if (string.IsNullOrWhiteSpace(request.ItemType))
            {
                throw new CompanyGuarantorCoverageRuleValidationException(
                    "Tipe item wajib diisi.",
                    StatusCodes.Status400BadRequest
                );
            }

            var itemType = NormalizeItemType(request.ItemType);
            if (!AllowedItemTypes.Contains(itemType))
            {
                throw new CompanyGuarantorCoverageRuleValidationException(
                    "Tipe item tidak valid. Gunakan salah satu: Tariff, Drug, DrugCategory, Procedure, TariffCategory.",
                    StatusCodes.Status400BadRequest
                );
            }

            if (string.IsNullOrWhiteSpace(request.CoverageStatus))
            {
                throw new CompanyGuarantorCoverageRuleValidationException(
                    "Status tanggungan wajib diisi.",
                    StatusCodes.Status400BadRequest
                );
            }

            var coverageStatus = NormalizeCoverageStatus(request.CoverageStatus);
            if (!AllowedCoverageStatuses.Contains(coverageStatus))
            {
                throw new CompanyGuarantorCoverageRuleValidationException(
                    "Status tanggungan tidak valid. Gunakan salah satu: Covered, NotCovered, PartialCovered, NeedApproval.",
                    StatusCodes.Status400BadRequest
                );
            }

            // BIL-VAL-092: Persentase tanggungan di luar rentang 0 sampai 100
            if (request.CoveragePercent < 0 || request.CoveragePercent > 100)
            {
                throw new CompanyGuarantorCoverageRuleValidationException(
                    "Persentase tanggungan harus berada di antara 0 dan 100.",
                    StatusCodes.Status400BadRequest
                );
            }

            // BIL-VAL-096: Tanggal akhir masa berlaku lebih awal dari tanggal mulai
            if (request.EffectiveStartDate.HasValue && request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                throw new CompanyGuarantorCoverageRuleValidationException(
                    "Tanggal akhir masa berlaku tidak boleh mendahului tanggal mulai.",
                    StatusCodes.Status400BadRequest
                );
            }

            // BIL-VAL-095: Lebih dari satu rujukan item diisi sekaligus sehingga sasarannya ambigu
            var filledItemCount = (request.TariffId.HasValue ? 1 : 0)
                + (request.DrugId.HasValue ? 1 : 0)
                + (request.DrugCategoryId.HasValue ? 1 : 0)
                + (request.ProcedureId.HasValue ? 1 : 0)
                + (request.TariffCategoryId.HasValue ? 1 : 0);

            if (filledItemCount > 1)
            {
                throw new CompanyGuarantorCoverageRuleValidationException(
                    "Aturan hanya boleh menyasar satu jenis item.",
                    StatusCodes.Status400BadRequest
                );
            }

            // BIL-VAL-094: Jenis item menuntut rujukan tertentu tetapi rujukannya kosong
            var hasTargetItem = itemType switch
            {
                "Tariff" => request.TariffId.HasValue,
                "Drug" => request.DrugId.HasValue,
                "DrugCategory" => request.DrugCategoryId.HasValue,
                "Procedure" => request.ProcedureId.HasValue,
                "TariffCategory" or "ServiceCategory" => request.TariffCategoryId.HasValue,
                _ => false
            };

            if (!hasTargetItem)
            {
                throw new CompanyGuarantorCoverageRuleValidationException(
                    "Lengkapi item yang menjadi sasaran aturan ini.",
                    StatusCodes.Status400BadRequest
                );
            }

            // Validasi keberadaan perusahaan penjamin
            var company = await _dbContext.MstCompanyGuarantors
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.CompanyGuarantorId && !x.IsDelete);

            if (company == null)
            {
                throw new CompanyGuarantorCoverageRuleValidationException(
                    "Perusahaan penjamin tidak ditemukan.",
                    StatusCodes.Status404NotFound
                );
            }

            if (!company.IsActive)
            {
                throw new CompanyGuarantorCoverageRuleValidationException(
                    "Perusahaan penjamin yang dipilih sudah tidak aktif.",
                    StatusCodes.Status422UnprocessableEntity
                );
            }

            // BIL-VAL-093: Kode aturan sudah dipakai aturan lain pada perusahaan yang sama
            if (!string.IsNullOrWhiteSpace(request.RuleCode))
            {
                var normalizedCode = request.RuleCode.Trim().ToLower();
                var duplicateCode = await _dbContext.MstCompanyGuarantorCoverageRules
                    .AsNoTracking()
                    .AnyAsync(x => !x.IsDelete &&
                                   x.CompanyGuarantorId == request.CompanyGuarantorId &&
                                   x.RuleCode.ToLower() == normalizedCode &&
                                   (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateCode)
                {
                    throw new CompanyGuarantorCoverageRuleValidationException(
                        "Kode aturan ini sudah dipakai pada perusahaan penjamin tersebut.",
                        StatusCodes.Status422UnprocessableEntity
                    );
                }
            }

            // Validasi keberadaan master item sasaran
            if (itemType == "Tariff")
            {
                var exists = await _dbContext.MstTariffs
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.TariffId!.Value && !x.IsDelete && x.IsActive);
                if (!exists)
                {
                    throw new CompanyGuarantorCoverageRuleValidationException(
                        "Tarif sasaran tidak ditemukan atau tidak aktif.",
                        StatusCodes.Status422UnprocessableEntity
                    );
                }
            }
            else if (itemType == "Drug")
            {
                var exists = await _dbContext.MstDrugs
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.DrugId!.Value && !x.IsDelete && x.IsActive);
                if (!exists)
                {
                    throw new CompanyGuarantorCoverageRuleValidationException(
                        "Obat sasaran tidak ditemukan atau tidak aktif.",
                        StatusCodes.Status422UnprocessableEntity
                    );
                }
            }
            else if (itemType == "DrugCategory")
            {
                var exists = await _dbContext.MstDrugCategories
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.DrugCategoryId!.Value && !x.IsDelete && x.IsActive);
                if (!exists)
                {
                    throw new CompanyGuarantorCoverageRuleValidationException(
                        "Kategori obat sasaran tidak ditemukan atau tidak aktif.",
                        StatusCodes.Status422UnprocessableEntity
                    );
                }
            }
            else if (itemType == "Procedure")
            {
                var exists = await _dbContext.MstProcedures
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.ProcedureId!.Value && !x.IsDelete && x.IsActive);
                if (!exists)
                {
                    throw new CompanyGuarantorCoverageRuleValidationException(
                        "Tindakan/prosedur sasaran tidak ditemukan atau tidak aktif.",
                        StatusCodes.Status422UnprocessableEntity
                    );
                }
            }
            else if (itemType == "TariffCategory" || itemType == "ServiceCategory")
            {
                var exists = await _dbContext.MstTariffCategories
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.TariffCategoryId!.Value && !x.IsDelete && x.IsActive);
                if (!exists)
                {
                    throw new CompanyGuarantorCoverageRuleValidationException(
                        "Kategori tarif sasaran tidak ditemukan atau tidak aktif.",
                        StatusCodes.Status422UnprocessableEntity
                    );
                }
            }

            // Validasi kelas pasien jika diisi
            if (request.PatientClassId.HasValue)
            {
                var exists = await _dbContext.MstPatientClasses
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.PatientClassId.Value && !x.IsDelete && x.IsActive);
                if (!exists)
                {
                    throw new CompanyGuarantorCoverageRuleValidationException(
                        "Kelas pasien sasaran tidak ditemukan atau tidak aktif.",
                        StatusCodes.Status422UnprocessableEntity
                    );
                }
            }
        }

        private async Task<string> GenerateRuleCodeAsync()
        {
            var existingCodes = await _dbContext.MstCompanyGuarantorCoverageRules
                .AsNoTracking()
                .Where(x => x.RuleCode.StartsWith(RuleCodePrefix))
                .Select(x => x.RuleCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(TryExtractRuleSequenceNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToHashSet();

            var nextNumber = 1;
            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return $"{RuleCodePrefix}{nextNumber.ToString().PadLeft(RuleCodeDigitLength, '0')}";
        }

        private static int? TryExtractRuleSequenceNumber(string ruleCode)
        {
            if (string.IsNullOrWhiteSpace(ruleCode))
                return null;

            if (!ruleCode.StartsWith(RuleCodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var numberText = ruleCode[RuleCodePrefix.Length..];
            return int.TryParse(numberText, out var number) ? number : null;
        }

        private static string NormalizeItemType(string itemType)
        {
            var trimmed = itemType.Trim();
            return trimmed.Equals("ServiceCategory", StringComparison.OrdinalIgnoreCase)
                ? "TariffCategory"
                : trimmed;
        }

        private static string NormalizeCoverageStatus(string status)
        {
            return status.Trim();
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            var normalizedPageNumber = pageNumber < 1 ? 1 : pageNumber;
            var normalizedPageSize = pageSize switch
            {
                <= 0 => 25,
                > 100 => 100,
                _ => pageSize
            };

            return (normalizedPageNumber, normalizedPageSize);
        }

        private static IQueryable<MstCompanyGuarantorCoverageRule> ApplyDateFilter(
            IQueryable<MstCompanyGuarantorCoverageRule> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var now = AppDateTimeHelper.OperationalDate();

            if (!string.IsNullOrWhiteSpace(customPeriod))
            {
                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        startDate = now;
                        endDate = now;
                        break;
                    case "last7days":
                        startDate = now.AddDays(-6);
                        endDate = now;
                        break;
                    case "last30days":
                        startDate = now.AddDays(-29);
                        endDate = now;
                        break;
                    case "thismonth":
                        startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        endDate = startDate.Value.AddMonths(1).AddDays(-1);
                        break;
                }
            }

            if (startDate.HasValue)
                query = query.Where(x => x.CreateDateTime >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(x => x.CreateDateTime < endDate.Value.Date.AddDays(1));

            return query;
        }

        private static IOrderedQueryable<MstCompanyGuarantorCoverageRule> ApplySorting(
            IQueryable<MstCompanyGuarantorCoverageRule> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDescending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "rulecode" => isDescending ? query.OrderByDescending(x => x.RuleCode) : query.OrderBy(x => x.RuleCode),
                "rulename" => isDescending ? query.OrderByDescending(x => x.RuleName) : query.OrderBy(x => x.RuleName),
                "companyguarantorname" => isDescending ? query.OrderByDescending(x => x.CompanyGuarantor!.CompanyGuarantorName) : query.OrderBy(x => x.CompanyGuarantor!.CompanyGuarantorName),
                "itemtype" => isDescending ? query.OrderByDescending(x => x.ItemType) : query.OrderBy(x => x.ItemType),
                "coveragestatus" => isDescending ? query.OrderByDescending(x => x.CoverageStatus) : query.OrderBy(x => x.CoverageStatus),
                "coveragepercent" => isDescending ? query.OrderByDescending(x => x.CoveragePercent) : query.OrderBy(x => x.CoveragePercent),
                "employeegrade" => isDescending ? query.OrderByDescending(x => x.EmployeeGrade) : query.OrderBy(x => x.EmployeeGrade),
                "benefitplancode" => isDescending ? query.OrderByDescending(x => x.BenefitPlanCode) : query.OrderBy(x => x.BenefitPlanCode),
                "effectivestartdate" => isDescending ? query.OrderByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.EffectiveStartDate),
                "effectiveenddate" => isDescending ? query.OrderByDescending(x => x.EffectiveEndDate) : query.OrderBy(x => x.EffectiveEndDate),
                "priority" => isDescending ? query.OrderByDescending(x => x.Priority) : query.OrderBy(x => x.Priority),
                "isactive" => isDescending ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDescending ? query.OrderByDescending(x => x.SortOrder) : query.OrderBy(x => x.SortOrder)
            };
        }

        private static CompanyGuarantorCoverageRuleResponse MapToResponse(
            MstCompanyGuarantorCoverageRule entity,
            Dictionary<Guid, string> userNames)
        {
            string? createByName = null;
            if (entity.CreateBy != Guid.Empty)
            {
                if (userNames.TryGetValue(entity.CreateBy, out var cName))
                    createByName = cName;
                else
                    createByName = entity.CreateBy.ToString();
            }

            return new CompanyGuarantorCoverageRuleResponse
            {
                Id = entity.Id,
                CompanyGuarantorId = entity.CompanyGuarantorId,
                CompanyGuarantorCode = entity.CompanyGuarantor?.CompanyGuarantorCode ?? string.Empty,
                CompanyGuarantorName = entity.CompanyGuarantor?.CompanyGuarantorName ?? string.Empty,
                CompanyGroupName = entity.CompanyGuarantor?.CompanyGroupName,
                RuleCode = entity.RuleCode,
                RuleName = entity.RuleName,
                ItemType = entity.ItemType,
                TariffId = entity.TariffId,
                TariffCode = entity.Tariff?.TariffCode,
                TariffName = entity.Tariff?.TariffName,
                DrugId = entity.DrugId,
                DrugCode = entity.Drug?.DrugCode,
                DrugName = entity.Drug?.DrugName,
                DrugCategoryId = entity.DrugCategoryId,
                DrugCategoryCode = entity.DrugCategory?.DrugCategoryCode,
                DrugCategoryName = entity.DrugCategory?.DrugCategoryName,
                ProcedureId = entity.ProcedureId,
                ProcedureCode = entity.Procedure?.ProcedureCode,
                ProcedureName = entity.Procedure?.ProcedureName,
                TariffCategoryId = entity.TariffCategoryId,
                TariffCategoryCode = entity.TariffCategory?.TariffCategoryCode,
                TariffCategoryName = entity.TariffCategory?.TariffCategoryName,
                BenefitPlanCode = entity.BenefitPlanCode,
                BenefitPlanName = entity.BenefitPlanName,
                EmployeeGrade = entity.EmployeeGrade,
                PatientClassId = entity.PatientClassId,
                PatientClassCode = entity.PatientClass?.PatientClassCode,
                PatientClassName = entity.PatientClass?.PatientClassName,
                CoverageStatus = entity.CoverageStatus,
                CoveragePercent = entity.CoveragePercent,
                MaxCoverageAmount = entity.MaxCoverageAmount,
                CoPaymentPercent = entity.CoPaymentPercent,
                CoPaymentAmount = entity.CoPaymentAmount,
                IsNeedApproval = entity.IsNeedApproval,
                IsNeedGuaranteeLetter = entity.IsNeedGuaranteeLetter,
                IsAllowExcessPaymentByPatient = entity.IsAllowExcessPaymentByPatient,
                MaxQuantityPerVisit = entity.MaxQuantityPerVisit,
                MaxQuantityPerMonth = entity.MaxQuantityPerMonth,
                MaxAmountPerVisit = entity.MaxAmountPerVisit,
                MaxAmountPerMonth = entity.MaxAmountPerMonth,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                Priority = entity.Priority,
                ApprovalInstruction = entity.ApprovalInstruction,
                BillingInstruction = entity.BillingInstruction,
                Description = entity.Description,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy,
                CreateByName = createByName
            };
        }

        private static CompanyGuarantorCoverageRuleDetailResponse MapToDetailResponse(
            MstCompanyGuarantorCoverageRule entity,
            Dictionary<Guid, string> userNames)
        {
            var baseRes = MapToResponse(entity, userNames);
            string? updateByName = null;
            if (entity.UpdateBy != Guid.Empty)
            {
                if (userNames.TryGetValue(entity.UpdateBy, out var uName))
                    updateByName = uName;
                else
                    updateByName = entity.UpdateBy.ToString();
            }

            return new CompanyGuarantorCoverageRuleDetailResponse
            {
                Id = baseRes.Id,
                CompanyGuarantorId = baseRes.CompanyGuarantorId,
                CompanyGuarantorCode = baseRes.CompanyGuarantorCode,
                CompanyGuarantorName = baseRes.CompanyGuarantorName,
                CompanyGroupName = baseRes.CompanyGroupName,
                RuleCode = baseRes.RuleCode,
                RuleName = baseRes.RuleName,
                ItemType = baseRes.ItemType,
                TariffId = baseRes.TariffId,
                TariffCode = baseRes.TariffCode,
                TariffName = baseRes.TariffName,
                DrugId = baseRes.DrugId,
                DrugCode = baseRes.DrugCode,
                DrugName = baseRes.DrugName,
                DrugCategoryId = baseRes.DrugCategoryId,
                DrugCategoryCode = baseRes.DrugCategoryCode,
                DrugCategoryName = baseRes.DrugCategoryName,
                ProcedureId = baseRes.ProcedureId,
                ProcedureCode = baseRes.ProcedureCode,
                ProcedureName = baseRes.ProcedureName,
                TariffCategoryId = baseRes.TariffCategoryId,
                TariffCategoryCode = baseRes.TariffCategoryCode,
                TariffCategoryName = baseRes.TariffCategoryName,
                BenefitPlanCode = baseRes.BenefitPlanCode,
                BenefitPlanName = baseRes.BenefitPlanName,
                EmployeeGrade = baseRes.EmployeeGrade,
                PatientClassId = baseRes.PatientClassId,
                PatientClassCode = baseRes.PatientClassCode,
                PatientClassName = baseRes.PatientClassName,
                CoverageStatus = baseRes.CoverageStatus,
                CoveragePercent = baseRes.CoveragePercent,
                MaxCoverageAmount = baseRes.MaxCoverageAmount,
                CoPaymentPercent = baseRes.CoPaymentPercent,
                CoPaymentAmount = baseRes.CoPaymentAmount,
                IsNeedApproval = baseRes.IsNeedApproval,
                IsNeedGuaranteeLetter = baseRes.IsNeedGuaranteeLetter,
                IsAllowExcessPaymentByPatient = baseRes.IsAllowExcessPaymentByPatient,
                MaxQuantityPerVisit = baseRes.MaxQuantityPerVisit,
                MaxQuantityPerMonth = baseRes.MaxQuantityPerMonth,
                MaxAmountPerVisit = baseRes.MaxAmountPerVisit,
                MaxAmountPerMonth = baseRes.MaxAmountPerMonth,
                EffectiveStartDate = baseRes.EffectiveStartDate,
                EffectiveEndDate = baseRes.EffectiveEndDate,
                Priority = baseRes.Priority,
                ApprovalInstruction = baseRes.ApprovalInstruction,
                BillingInstruction = baseRes.BillingInstruction,
                Description = baseRes.Description,
                SortOrder = baseRes.SortOrder,
                IsActive = baseRes.IsActive,
                CreateDateTime = baseRes.CreateDateTime,
                CreateBy = baseRes.CreateBy,
                CreateByName = baseRes.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy,
                UpdateByName = updateByName
            };
        }

        private static List<CompanyGuarantorCoverageRuleCustomPeriodResponse> BuildCustomPeriodOptions()
        {
            return new List<CompanyGuarantorCoverageRuleCustomPeriodResponse>
            {
                new() { Value = "today", Label = "Hari Ini", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 Hari Terakhir", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 Hari Terakhir", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thisMonth", Label = "Bulan Ini", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "custom", Label = "Kustom Tanggal", UsesStartDate = true, UsesEndDate = true }
            };
        }

        private static List<CompanyGuarantorCoverageRuleQueryParameterResponse> BuildQueryParameters()
        {
            return new List<CompanyGuarantorCoverageRuleQueryParameterResponse>
            {
                new() { Name = "companyGuarantorId", Type = "Guid", IsRequired = false, Description = "Saring berdasarkan id perusahaan penjamin.", Example = "a6c9c612-9c17-48f0-b0c4-904128f115a3" },
                new() { Name = "itemType", Type = "string", IsRequired = false, Description = "Saring berdasarkan tipe item (Tariff, Drug, DrugCategory, Procedure, TariffCategory).", Example = "Tariff" },
                new() { Name = "coverageStatus", Type = "string", IsRequired = false, Description = "Saring berdasarkan status tanggungan (Covered, NotCovered, PartialCovered, NeedApproval).", Example = "Covered" },
                new() { Name = "benefitPlanCode", Type = "string", IsRequired = false, Description = "Saring berdasarkan kode paket manfaat.", Example = "GOLD" },
                new() { Name = "employeeGrade", Type = "string", IsRequired = false, Description = "Saring berdasarkan golongan karyawan.", Example = "MANAGER" },
                new() { Name = "isActive", Type = "bool", IsRequired = false, Description = "Saring berdasarkan status aktif.", Example = "true" },
                new() { Name = "search", Type = "string", IsRequired = false, Description = "Pencarian kata kunci kode/nama aturan, penjamin, item, grade, atau keterangan.", Example = "Konsultasi" },
                new() { Name = "startDate", Type = "DateTime", IsRequired = false, Description = "Awal rentang tanggal pembuatan aturan.", Example = "2026-09-01" },
                new() { Name = "endDate", Type = "DateTime", IsRequired = false, Description = "Akhir rentang tanggal pembuatan aturan.", Example = "2026-09-30" },
                new() { Name = "customPeriod", Type = "string", IsRequired = false, Description = "Periode siap pakai (today, last7days, last30days, thisMonth).", Example = "thisMonth" },
                new() { Name = "sortBy", Type = "string", IsRequired = false, Description = "Kolom pengurutan data.", Example = "priority" },
                new() { Name = "sortDirection", Type = "string", IsRequired = false, Description = "Arah pengurutan (asc, desc).", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", IsRequired = false, Description = "Nomor halaman data (mulai dari 1).", Example = "1" },
                new() { Name = "pageSize", Type = "int", IsRequired = false, Description = "Jumlah baris per halaman (10, 25, 50, 100).", Example = "25" }
            };
        }

        private static List<CompanyGuarantorCoverageRuleFormFieldResponse> BuildCreateFields()
        {
            return new List<CompanyGuarantorCoverageRuleFormFieldResponse>
            {
                new() { Name = "companyGuarantorId", Label = "Perusahaan Penjamin", Section = "Informasi Penjamin", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "required", OptionsSource = "/api/v1/administrator/master-data/company-guarantors/options", Description = "Pilih perusahaan penjamin sasaran aturan.", SortOrder = 1 },
                new() { Name = "ruleCode", Label = "Kode Aturan", Section = "Informasi Utama", InputType = "text", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", MaxLength = 50, Description = "Kode aturan unik per penjamin. Dibuat otomatis jika dikosongkan.", SortOrder = 2 },
                new() { Name = "ruleName", Label = "Nama Aturan", Section = "Informasi Utama", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "required", MaxLength = 200, Description = "Nama deskriptif aturan tanggungan.", SortOrder = 3 },
                new() { Name = "itemType", Label = "Tipe Item Sasaran", Section = "Sasaran Aturan", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "required", OptionsSource = "ItemTypeOptions", Description = "Tipe item yang diatur (Tariff, Drug, DrugCategory, Procedure, TariffCategory).", SortOrder = 4 },
                new() { Name = "tariffId", Label = "Tarif / Layanan", Section = "Sasaran Aturan", InputType = "select", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "conditional", OptionsSource = "/api/v1/health-services/master-data/tariffs/options", Description = "Wajib diisi jika tipe item Tariff.", SortOrder = 5 },
                new() { Name = "drugId", Label = "Obat / Farmasi", Section = "Sasaran Aturan", InputType = "select", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "conditional", OptionsSource = "/api/v1/health-services/master-data/drugs/options", Description = "Wajib diisi jika tipe item Drug.", SortOrder = 6 },
                new() { Name = "drugCategoryId", Label = "Kategori Obat", Section = "Sasaran Aturan", InputType = "select", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "conditional", OptionsSource = "/api/v1/health-services/master-data/drug-categories/options", Description = "Wajib diisi jika tipe item DrugCategory.", SortOrder = 7 },
                new() { Name = "procedureId", Label = "Tindakan Medis", Section = "Sasaran Aturan", InputType = "select", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "conditional", OptionsSource = "/api/v1/health-services/master-data/procedures/options", Description = "Wajib diisi jika tipe item Procedure.", SortOrder = 8 },
                new() { Name = "tariffCategoryId", Label = "Kategori Tarif", Section = "Sasaran Aturan", InputType = "select", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "conditional", OptionsSource = "/api/v1/health-services/master-data/tariff-categories/options", Description = "Wajib diisi jika tipe item TariffCategory.", SortOrder = 9 },
                new() { Name = "employeeGrade", Label = "Golongan Karyawan", Section = "Kriteria Peserta", InputType = "text", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", MaxLength = 50, Description = "Golongan atau tingkatan karyawan sasaran tanggungan.", SortOrder = 10 },
                new() { Name = "benefitPlanCode", Label = "Kode Paket Manfaat", Section = "Kriteria Peserta", InputType = "text", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", MaxLength = 100, Description = "Kode paket manfaat penjamin.", SortOrder = 11 },
                new() { Name = "patientClassId", Label = "Kelas Perawatan", Section = "Kriteria Peserta", InputType = "select", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", OptionsSource = "/api/v1/health-services/master-data/patient-classes/options", Description = "Batasan kelas perawatan yang berlaku.", SortOrder = 12 },
                new() { Name = "coverageStatus", Label = "Status Tanggungan", Section = "Ketentuan Tanggungan", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "required", OptionsSource = "CoverageStatusOptions", Description = "Covered, NotCovered, PartialCovered, atau NeedApproval.", SortOrder = 13 },
                new() { Name = "coveragePercent", Label = "Persentase Tanggungan (%)", Section = "Ketentuan Tanggungan", InputType = "number", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "required", Description = "Nilai 0 s.d. 100. Urun biaya (Co-Payment) diturunkan server otomatis.", Example = "100", SortOrder = 14 },
                new() { Name = "maxCoverageAmount", Label = "Plafon Maksimal Biaya", Section = "Ketentuan Tanggungan", InputType = "number", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", Description = "Batas maksimal biaya yang ditanggung penjamin per transaksi.", SortOrder = 15 },
                new() { Name = "priority", Label = "Prioritas Aturan", Section = "Pengaturan", InputType = "number", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "required", Description = "Urutan evaluasi aturan (angka lebih kecil dinilai lebih dulu).", Example = "1", SortOrder = 16 },
                new() { Name = "isNeedApproval", Label = "Butuh Persetujuan", Section = "Pengaturan", InputType = "switch", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", Description = "Tandai jika memerlukan approval sebelum ditagihkan.", SortOrder = 17 },
                new() { Name = "isNeedGuaranteeLetter", Label = "Butuh Surat Jaminan (GL)", Section = "Pengaturan", InputType = "switch", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", Description = "Tandai jika butuh surat jaminan dari penjamin.", SortOrder = 18 },
                new() { Name = "isAllowExcessPaymentByPatient", Label = "Izinkan Pasien Bayar Selisih (Excess)", Section = "Pengaturan", InputType = "switch", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", Description = "Apakah selisih biaya boleh dibebankan ke pasien.", SortOrder = 19 },
                new() { Name = "effectiveStartDate", Label = "Tanggal Mulai Berlaku", Section = "Masa Berlaku", InputType = "date", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", Description = "Awal masa berlaku aturan.", SortOrder = 20 },
                new() { Name = "effectiveEndDate", Label = "Tanggal Akhir Berlaku", Section = "Masa Berlaku", InputType = "date", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", Description = "Akhir masa berlaku aturan.", SortOrder = 21 },
                new() { Name = "approvalInstruction", Label = "Instruksi Approval", Section = "Instruksi & Catatan", InputType = "textarea", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", MaxLength = 500, Description = "Instruksi bagi staf saat meminta persetujuan penjamin.", SortOrder = 22 },
                new() { Name = "billingInstruction", Label = "Instruksi Penagihan", Section = "Instruksi & Catatan", InputType = "textarea", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", MaxLength = 500, Description = "Instruksi bagi kasir/billing saat mencetak atau menagihkan.", SortOrder = 23 },
                new() { Name = "description", Label = "Keterangan", Section = "Instruksi & Catatan", InputType = "textarea", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", MaxLength = 500, Description = "Catatan internal aturan tanggungan.", SortOrder = 24 },
                new() { Name = "isActive", Label = "Status Aktif", Section = "Instruksi & Catatan", InputType = "switch", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "optional", Description = "Status aktif aturan tanggungan.", SortOrder = 25 }
            };
        }

        private static List<CompanyGuarantorCoverageRuleFormFieldResponse> BuildUpdateFields()
        {
            return BuildCreateFields();
        }
    }
}
