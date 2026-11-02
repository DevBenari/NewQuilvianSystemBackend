using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    public class HrdInitialPlacementBatchService : IHrdInitialPlacementBatchService
    {
        private readonly ApplicationDbContext _dbContext;

        public HrdInitialPlacementBatchService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<InitialBatchRowResponse>> GetTemplateRowsAsync(CancellationToken ct = default)
        {
            var today = DateTime.UtcNow.Date;

            // Fetch active primary organization assignments (the 55 baseline employees)
            var orgAssignments = await _dbContext.WfpOrganizationAssignments
                .Include(x => x.WorkforceProfile)
                .Include(x => x.OrganizationUnit)
                .Include(x => x.Department)
                .AsNoTracking()
                .Where(x => x.IsPrimary && x.IsActive && !x.IsDelete &&
                            x.EffectiveStartDate <= today &&
                            (x.EffectiveEndDate == null || x.EffectiveEndDate >= today))
                .OrderBy(x => x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty)
                .ToListAsync(ct);

            var result = new List<InitialBatchRowResponse>();

            foreach (var org in orgAssignments)
            {
                if (org.WorkforceProfile == null) continue;

                // Check if already has an active position assignment
                var hasPosition = await _dbContext.WfpPositionAssignments
                    .AsNoTracking()
                    .AnyAsync(x => x.WorkforceProfileId == org.WorkforceProfileId &&
                                   x.IsPrimary && x.IsActive && !x.IsDelete &&
                                   x.EffectiveStartDate <= today &&
                                   (x.EffectiveEndDate == null || x.EffectiveEndDate >= today), ct);

                // Default suggested effective date: join date or today
                var effectiveDate = org.EffectiveStartDate != default ? org.EffectiveStartDate : today;

                result.Add(new InitialBatchRowResponse
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = org.WorkforceProfileId,
                    ProfileCode = org.WorkforceProfile.ProfileCode,
                    EmployeeName = org.WorkforceProfile.DisplayName,
                    OrganizationUnitId = org.OrganizationUnitId,
                    OrganizationUnitName = org.OrganizationUnit?.UnitName,
                    DepartmentId = org.DepartmentId,
                    DepartmentName = org.Department?.DepartmentName,
                    PositionId = null,
                    PositionName = null,
                    ManagerWorkforceProfileId = null,
                    ManagerName = null,
                    BaseSalary = null,
                    EffectiveStartDate = effectiveDate,
                    IsValid = !hasPosition,
                    ValidationMessage = hasPosition ? "Sudah memiliki jabatan aktif." : null
                });
            }

            return result;
        }

        public async Task<BatchValidationResultResponse> ValidateRowsAsync(List<CreateInitialBatchRowDto> rows, CancellationToken ct = default)
        {
            var errors = new List<string>();
            var validCount = 0;
            var invalidCount = 0;

            var profileIds = rows.Select(x => x.WorkforceProfileId).Distinct().ToList();
            var positionIds = rows.Select(x => x.PositionId).Distinct().ToList();
            var managerIds = rows.Where(x => x.ManagerWorkforceProfileId.HasValue).Select(x => x.ManagerWorkforceProfileId!.Value).Distinct().ToList();

            var activeProfiles = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .Where(x => profileIds.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, ct);

            var activePositions = await _dbContext.MstPositions
                .AsNoTracking()
                .Where(x => positionIds.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, ct);

            var activeManagers = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .Where(x => managerIds.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, ct);

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var rowNum = i + 1;
                var rowValid = true;

                if (!activeProfiles.TryGetValue(row.WorkforceProfileId, out var profile))
                {
                    errors.Add($"Baris {rowNum}: Pegawai dengan ID {row.WorkforceProfileId} tidak ditemukan.");
                    rowValid = false;
                }

                if (!activePositions.ContainsKey(row.PositionId))
                {
                    errors.Add($"Baris {rowNum}: Jabatan dengan ID {row.PositionId} tidak ditemukan.");
                    rowValid = false;
                }

                if (row.ManagerWorkforceProfileId.HasValue)
                {
                    if (row.ManagerWorkforceProfileId.Value == row.WorkforceProfileId)
                    {
                        errors.Add($"Baris {rowNum}: Pegawai tidak dapat menjadi atasan bagi dirinya sendiri.");
                        rowValid = false;
                    }
                    else if (!activeManagers.ContainsKey(row.ManagerWorkforceProfileId.Value))
                    {
                        errors.Add($"Baris {rowNum}: Atasan dengan ID {row.ManagerWorkforceProfileId.Value} tidak ditemukan.");
                        rowValid = false;
                    }
                }

                if (row.EffectiveStartDate == default)
                {
                    errors.Add($"Baris {rowNum}: Tanggal mulai berlaku wajib diisi.");
                    rowValid = false;
                }

                if (rowValid)
                {
                    validCount++;
                }
                else
                {
                    invalidCount++;
                }
            }

            return new BatchValidationResultResponse
            {
                IsAllValid = invalidCount == 0 && rows.Count > 0,
                TotalCount = rows.Count,
                ValidCount = validCount,
                InvalidCount = invalidCount,
                ValidationErrors = errors
            };
        }

        public async Task<InitialBatchDetailResponse> CreateBatchDraftAsync(CreateInitialBatchRequest request, Guid userId, CancellationToken ct = default)
        {
            if (request.Rows == null || request.Rows.Count == 0)
            {
                throw new ArgumentException("Baris data batch penempatan awal tidak boleh kosong.");
            }

            var validation = await ValidateRowsAsync(request.Rows, ct);

            // Generate Batch Number
            var today = DateTime.UtcNow.Date;
            var prefix = $"BATCH-INIT-{today:yyyyMMdd}-";
            var existingCount = await _dbContext.HrdInitialPlacementBatches
                .CountAsync(x => x.BatchNumber.StartsWith(prefix), ct);
            var batchNumber = $"{prefix}{(existingCount + 1):D4}";

            var batch = new HrdInitialPlacementBatch
            {
                BatchNumber = batchNumber,
                BatchName = string.IsNullOrWhiteSpace(request.BatchName) ? "Initial Placement Baseline Batch" : request.BatchName.Trim(),
                BatchStatus = validation.IsAllValid ? "Validated" : "Draft",
                TotalRows = request.Rows.Count,
                ValidatedRows = validation.ValidCount,
                FailedRows = validation.InvalidCount,
                SubmittedByUserId = userId,
                SubmittedAt = DateTime.UtcNow,
                CreateBy = userId,
                CreateDateTime = DateTime.UtcNow
            };

            var profileIds = request.Rows.Select(x => x.WorkforceProfileId).Distinct().ToList();
            var profiles = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .Where(x => profileIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, ct);

            foreach (var rowDto in request.Rows)
            {
                var isRowValid = profiles.ContainsKey(rowDto.WorkforceProfileId) &&
                                 rowDto.PositionId != Guid.Empty &&
                                 rowDto.EffectiveStartDate != default &&
                                 (!rowDto.ManagerWorkforceProfileId.HasValue || rowDto.ManagerWorkforceProfileId.Value != rowDto.WorkforceProfileId);

                var rowEntity = new HrdInitialPlacementBatchRow
                {
                    BatchId = batch.Id,
                    WorkforceProfileId = rowDto.WorkforceProfileId,
                    OrganizationUnitId = rowDto.OrganizationUnitId != Guid.Empty ? rowDto.OrganizationUnitId : null,
                    DepartmentId = rowDto.DepartmentId != Guid.Empty ? rowDto.DepartmentId : null,
                    PositionId = rowDto.PositionId != Guid.Empty ? rowDto.PositionId : null,
                    ManagerWorkforceProfileId = rowDto.ManagerWorkforceProfileId,
                    SalaryGradeId = rowDto.SalaryGradeId,
                    BaseSalary = rowDto.BaseSalary,
                    EffectiveStartDate = rowDto.EffectiveStartDate,
                    IsValid = isRowValid,
                    ValidationMessage = isRowValid ? null : "Data baris belum memenuhi validasi integritas",
                    CreateBy = userId,
                    CreateDateTime = DateTime.UtcNow
                };

                batch.Rows.Add(rowEntity);
            }

            _dbContext.HrdInitialPlacementBatches.Add(batch);
            await _dbContext.SaveChangesAsync(ct);

            return await GetBatchByIdAsync(batch.Id, ct) ?? throw new InvalidOperationException("Gagal mengambil data batch yang baru dibuat.");
        }

        public async Task<PagedResult<InitialBatchListResponse>> GetBatchesAsync(int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var query = _dbContext.HrdInitialPlacementBatches
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.CreateDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new InitialBatchListResponse
                {
                    Id = x.Id,
                    BatchNumber = x.BatchNumber,
                    BatchName = x.BatchName,
                    BatchStatus = x.BatchStatus,
                    TotalRows = x.TotalRows,
                    ValidatedRows = x.ValidatedRows,
                    FailedRows = x.FailedRows,
                    CreateDateTime = x.CreateDateTime,
                    ApprovedAt = x.ApprovedAt
                })
                .ToListAsync(ct);

            return new PagedResult<InitialBatchListResponse>
            {
                Items = items,
                TotalData = totalCount,
                TotalPage = (int)Math.Ceiling(totalCount / (double)pageSize),
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<InitialBatchDetailResponse?> GetBatchByIdAsync(Guid id, CancellationToken ct = default)
        {
            var batch = await _dbContext.HrdInitialPlacementBatches
                .Include(x => x.Rows)
                    .ThenInclude(r => r.WorkforceProfile)
                .Include(x => x.Rows)
                    .ThenInclude(r => r.OrganizationUnit)
                .Include(x => x.Rows)
                    .ThenInclude(r => r.Department)
                .Include(x => x.Rows)
                    .ThenInclude(r => r.Position)
                .Include(x => x.Rows)
                    .ThenInclude(r => r.ManagerWorkforceProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (batch == null) return null;

            return new InitialBatchDetailResponse
            {
                Id = batch.Id,
                BatchNumber = batch.BatchNumber,
                BatchName = batch.BatchName,
                BatchStatus = batch.BatchStatus,
                TotalRows = batch.TotalRows,
                ValidatedRows = batch.ValidatedRows,
                FailedRows = batch.FailedRows,
                CreateDateTime = batch.CreateDateTime,
                SubmittedByUserId = batch.SubmittedByUserId,
                SubmittedAt = batch.SubmittedAt,
                ApprovedByUserId = batch.ApprovedByUserId,
                ApprovedAt = batch.ApprovedAt,
                ExecutionSummary = batch.ExecutionSummary,
                Rows = batch.Rows.Select(r => new InitialBatchRowResponse
                {
                    Id = r.Id,
                    WorkforceProfileId = r.WorkforceProfileId,
                    ProfileCode = r.WorkforceProfile?.ProfileCode ?? string.Empty,
                    EmployeeName = r.WorkforceProfile?.DisplayName ?? string.Empty,
                    OrganizationUnitId = r.OrganizationUnitId,
                    OrganizationUnitName = r.OrganizationUnit?.UnitName,
                    DepartmentId = r.DepartmentId,
                    DepartmentName = r.Department?.DepartmentName,
                    PositionId = r.PositionId,
                    PositionName = r.Position?.PositionName,
                    ManagerWorkforceProfileId = r.ManagerWorkforceProfileId,
                    ManagerName = r.ManagerWorkforceProfile?.DisplayName,
                    BaseSalary = r.BaseSalary,
                    EffectiveStartDate = r.EffectiveStartDate,
                    IsValid = r.IsValid,
                    ValidationMessage = r.ValidationMessage,
                    ProcessedAt = r.ProcessedAt
                }).ToList()
            };
        }

        public async Task<InitialBatchDetailResponse> ExecuteBatchAsync(Guid id, Guid userId, CancellationToken ct = default)
        {
            var batch = await _dbContext.HrdInitialPlacementBatches
                .Include(x => x.Rows)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (batch == null)
            {
                throw new InvalidOperationException("Batch penempatan awal tidak ditemukan.");
            }

            if (batch.BatchStatus == "Executed")
            {
                throw new InvalidOperationException("Batch penempatan awal ini sudah dieksekusi sebelumnya.");
            }

            // Look up a default salary structure & grade if salary is specified but IDs missing
            var defaultSalaryStructure = await _dbContext.MstSalaryStructures
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IsActive && !x.IsDelete, ct);

            var defaultSalaryGrade = await _dbContext.MstSalaryGrades
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IsActive && !x.IsDelete, ct);

            using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                var now = DateTime.UtcNow;

                foreach (var row in batch.Rows)
                {
                    // 1. Process Position Assignment
                    if (row.PositionId.HasValue && row.PositionId.Value != Guid.Empty)
                    {
                        var existingPrimaryPos = await _dbContext.WfpPositionAssignments
                            .Where(x => x.WorkforceProfileId == row.WorkforceProfileId &&
                                        x.IsPrimary && x.IsActive && !x.IsDelete)
                            .ToListAsync(ct);

                        foreach (var p in existingPrimaryPos)
                        {
                            p.IsPrimary = false;
                            p.EffectiveEndDate = row.EffectiveStartDate.AddDays(-1);
                            p.UpdateDateTime = now;
                            p.UpdateBy = userId;
                        }

                        var newPos = new WfpPositionAssignment
                        {
                            WorkforceProfileId = row.WorkforceProfileId,
                            PositionId = row.PositionId.Value,
                            DepartmentId = row.DepartmentId,
                            OrganizationUnitId = row.OrganizationUnitId,
                            AssignmentType = "Substantive",
                            EffectiveStartDate = row.EffectiveStartDate,
                            IsPrimary = true,
                            IsActive = true,
                            ApprovedByUserId = userId,
                            ApprovedAt = now,
                            CreateBy = userId,
                            CreateDateTime = now
                        };

                        _dbContext.WfpPositionAssignments.Add(newPos);
                    }

                    // 2. Process Manager Assignment
                    if (row.ManagerWorkforceProfileId.HasValue && row.ManagerWorkforceProfileId.Value != Guid.Empty &&
                        row.ManagerWorkforceProfileId.Value != row.WorkforceProfileId)
                    {
                        var existingPrimaryMgr = await _dbContext.WfpManagerAssignments
                            .Where(x => x.WorkforceProfileId == row.WorkforceProfileId &&
                                        x.IsPrimaryManager && x.IsActive && !x.IsDelete)
                            .ToListAsync(ct);

                        foreach (var m in existingPrimaryMgr)
                        {
                            m.IsPrimaryManager = false;
                            m.EffectiveEndDate = row.EffectiveStartDate.AddDays(-1);
                            m.UpdateDateTime = now;
                            m.UpdateBy = userId;
                        }

                        var newMgr = new WfpManagerAssignment
                        {
                            WorkforceProfileId = row.WorkforceProfileId,
                            ManagerWorkforceProfileId = row.ManagerWorkforceProfileId.Value,
                            OrganizationUnitId = row.OrganizationUnitId,
                            DepartmentId = row.DepartmentId,
                            ManagerType = "Direct",
                            EffectiveStartDate = row.EffectiveStartDate,
                            IsPrimaryManager = true,
                            CanApproveRequests = true,
                            IsActive = true,
                            ApprovedByUserId = userId,
                            ApprovedAt = now,
                            CreateBy = userId,
                            CreateDateTime = now
                        };

                        _dbContext.WfpManagerAssignments.Add(newMgr);
                    }

                    // 3. Process Salary Assignment (if specified)
                    if (row.BaseSalary.HasValue && row.BaseSalary.Value > 0 &&
                        (row.SalaryGradeId.HasValue || defaultSalaryGrade != null) &&
                        defaultSalaryStructure != null)
                    {
                        var targetGradeId = row.SalaryGradeId ?? defaultSalaryGrade!.Id;

                        var existingPrimarySalary = await _dbContext.WfpSalaryAssignments
                            .Where(x => x.WorkforceProfileId == row.WorkforceProfileId &&
                                        x.IsPrimary && x.IsActive && !x.IsDelete)
                            .ToListAsync(ct);

                        foreach (var s in existingPrimarySalary)
                        {
                            s.IsPrimary = false;
                            s.EffectiveEndDate = row.EffectiveStartDate.AddDays(-1);
                            s.UpdateDateTime = now;
                            s.UpdateBy = userId;
                        }

                        var newSalary = new WfpSalaryAssignment
                        {
                            WorkforceProfileId = row.WorkforceProfileId,
                            SalaryStructureId = defaultSalaryStructure.Id,
                            SalaryGradeId = targetGradeId,
                            BaseSalary = row.BaseSalary.Value,
                            CurrencyCode = "IDR",
                            PaymentFrequency = "Monthly",
                            EffectiveStartDate = row.EffectiveStartDate,
                            IsPrimary = true,
                            IsConfidential = true,
                            IsActive = true,
                            CreateBy = userId,
                            CreateDateTime = now
                        };

                        _dbContext.WfpSalaryAssignments.Add(newSalary);
                    }

                    row.IsValid = true;
                    row.ProcessedAt = now;
                    row.UpdateDateTime = now;
                    row.UpdateBy = userId;
                }

                batch.BatchStatus = "Executed";
                batch.ApprovedByUserId = userId;
                batch.ApprovedAt = now;
                batch.ExecutionSummary = $"Sukses menerapkan penempatan awal untuk {batch.Rows.Count} pegawai.";
                batch.UpdateDateTime = now;
                batch.UpdateBy = userId;

                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }

            return await GetBatchByIdAsync(id, ct) ?? throw new InvalidOperationException("Gagal mengambil data batch setelah eksekusi.");
        }
    }
}
