using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
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

            // Fetch active primary organization assignments (the baseline employees)
            var orgAssignments = await _dbContext.WfpOrganizationAssignments
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(p => p.PrimaryPosition)
                .Include(x => x.OrganizationUnit)
                .Include(x => x.Department)
                .Include(x => x.Position)
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

                // Check if already has an active primary position assignment
                var hasPosition = await _dbContext.WfpPositionAssignments
                    .AsNoTracking()
                    .AnyAsync(x => x.WorkforceProfileId == org.WorkforceProfileId &&
                                   x.IsPrimary && x.IsActive && !x.IsDelete &&
                                   (x.EffectiveEndDate == null || x.EffectiveEndDate >= today), ct);

                // Resolve prefilled position if available from org assignment or profile
                Guid? positionId = null;
                string? positionName = null;

                if (org.PositionId != Guid.Empty)
                {
                    positionId = org.PositionId;
                    positionName = org.Position?.PositionName;
                }
                else if (org.WorkforceProfile.PrimaryPositionId.HasValue && org.WorkforceProfile.PrimaryPositionId.Value != Guid.Empty)
                {
                    positionId = org.WorkforceProfile.PrimaryPositionId.Value;
                    positionName = org.WorkforceProfile.PrimaryPosition?.PositionName;
                }

                bool isValid = true;
                string? validationMessage = null;

                if (hasPosition)
                {
                    isValid = false;
                    validationMessage = "Sudah memiliki jabatan aktif. Tidak dapat diproses melalui batch inisialisasi awal.";
                }
                else if (!positionId.HasValue || positionId.Value == Guid.Empty)
                {
                    isValid = false;
                    validationMessage = "Jabatan belum ditentukan pada data baseline organisasi.";
                }

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
                    PositionId = positionId,
                    PositionName = positionName,
                    ManagerWorkforceProfileId = null,
                    ManagerName = null,
                    BaseSalary = null,
                    EffectiveStartDate = effectiveDate,
                    IsValid = isValid,
                    ValidationMessage = validationMessage
                });
            }

            return result;
        }

        public async Task<BatchValidationResultResponse> ValidateRowsAsync(List<CreateInitialBatchRowDto> rows, CancellationToken ct = default)
        {
            if (rows == null || rows.Count == 0)
            {
                return new BatchValidationResultResponse
                {
                    IsAllValid = false,
                    TotalCount = 0,
                    ValidCount = 0,
                    InvalidCount = 0,
                    ValidationErrors = new List<string> { "Baris data batch penempatan awal tidak boleh kosong." },
                    RowResults = new List<RowValidationItemResponse>()
                };
            }

            var errors = new List<string>();
            var rowResults = new List<RowValidationItemResponse>();
            var validCount = 0;
            var invalidCount = 0;
            var today = DateTime.UtcNow.Date;

            // Detect duplicates within the submitted batch
            var profileGroupCounts = rows
                .GroupBy(x => x.WorkforceProfileId)
                .ToDictionary(g => g.Key, g => g.Count());

            var profileIds = rows.Select(x => x.WorkforceProfileId).Distinct().ToList();
            var positionIds = rows.Select(x => x.PositionId).Where(id => id != Guid.Empty).Distinct().ToList();
            var managerIds = rows.Where(x => x.ManagerWorkforceProfileId.HasValue && x.ManagerWorkforceProfileId.Value != Guid.Empty)
                .Select(x => x.ManagerWorkforceProfileId!.Value).Distinct().ToList();
            var deptIds = rows.Select(x => x.DepartmentId).Where(id => id != Guid.Empty).Distinct().ToList();
            var orgUnitIds = rows.Select(x => x.OrganizationUnitId).Where(id => id != Guid.Empty).Distinct().ToList();

            var activeProfiles = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .Where(x => profileIds.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, ct);

            var activePositions = await _dbContext.MstPositions
                .Include(p => p.Department)
                .AsNoTracking()
                .Where(x => positionIds.Contains(x.Id) && !x.IsDelete && x.IsActive)
                .ToDictionaryAsync(x => x.Id, ct);

            var activeManagers = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .Where(x => managerIds.Contains(x.Id) && !x.IsDelete && x.IsActive)
                .ToDictionaryAsync(x => x.Id, ct);

            var activeDepartments = await _dbContext.MstDepartments
                .AsNoTracking()
                .Where(x => deptIds.Contains(x.Id) && !x.IsDelete && x.IsActive)
                .ToDictionaryAsync(x => x.Id, ct);

            var activeOrgUnits = await _dbContext.MstOrganizationUnits
                .AsNoTracking()
                .Where(x => orgUnitIds.Contains(x.Id) && !x.IsDelete && x.IsActive)
                .ToDictionaryAsync(x => x.Id, ct);

            // Check existing active primary position assignments (prevent illegal overwrite)
            var existingPositionProfileIds = await _dbContext.WfpPositionAssignments
                .AsNoTracking()
                .Where(x => profileIds.Contains(x.WorkforceProfileId) &&
                            x.IsPrimary && x.IsActive && !x.IsDelete &&
                            (x.EffectiveEndDate == null || x.EffectiveEndDate >= today))
                .Select(x => x.WorkforceProfileId)
                .Distinct()
                .ToListAsync(ct);
            var existingPositionSet = existingPositionProfileIds.ToHashSet();

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var rowNum = i + 1;
                var rowErrors = new List<string>();

                // 1. Duplicate check in batch
                if (profileGroupCounts.TryGetValue(row.WorkforceProfileId, out var count) && count > 1)
                {
                    rowErrors.Add("Pegawai tercantum lebih dari satu kali dalam batch yang sama.");
                }

                // 2. Profile existence & active check
                if (!activeProfiles.TryGetValue(row.WorkforceProfileId, out var profile))
                {
                    rowErrors.Add($"Pegawai dengan ID {row.WorkforceProfileId} tidak ditemukan atau sudah dihapus.");
                }
                else if (!profile.IsActive)
                {
                    rowErrors.Add($"Pegawai {profile.DisplayName} dalam status non-aktif.");
                }

                // 3. Conflict with existing placement check
                if (existingPositionSet.Contains(row.WorkforceProfileId))
                {
                    rowErrors.Add("Pegawai sudah memiliki penempatan jabatan primer aktif di sistem.");
                }

                // 4. Position validation & department consistency
                if (row.PositionId == Guid.Empty)
                {
                    rowErrors.Add("Jabatan wajib dipilih.");
                }
                else if (!activePositions.TryGetValue(row.PositionId, out var position))
                {
                    rowErrors.Add($"Jabatan dengan ID {row.PositionId} tidak ditemukan atau tidak aktif.");
                }
                else
                {
                    if (row.DepartmentId != Guid.Empty && position.DepartmentId != Guid.Empty && position.DepartmentId != row.DepartmentId)
                    {
                        rowErrors.Add($"Jabatan '{position.PositionName}' tidak sesuai dengan departemen yang dipilih.");
                    }
                }

                // 5. Department validation
                if (row.DepartmentId != Guid.Empty && !activeDepartments.ContainsKey(row.DepartmentId))
                {
                    rowErrors.Add($"Departemen dengan ID {row.DepartmentId} tidak ditemukan atau tidak aktif.");
                }

                // 6. Organization unit validation
                if (row.OrganizationUnitId != Guid.Empty && !activeOrgUnits.ContainsKey(row.OrganizationUnitId))
                {
                    rowErrors.Add($"Unit organisasi dengan ID {row.OrganizationUnitId} tidak ditemukan atau tidak aktif.");
                }
                else if (row.OrganizationUnitId != Guid.Empty &&
                         activeOrgUnits.TryGetValue(row.OrganizationUnitId, out var orgUnit) &&
                         orgUnit.DepartmentId.HasValue && orgUnit.DepartmentId.Value != Guid.Empty &&
                         row.DepartmentId != Guid.Empty && orgUnit.DepartmentId.Value != row.DepartmentId)
                {
                    rowErrors.Add("Departemen tidak selaras dengan unit organisasi yang dipilih.");
                }

                // 7. Manager validation
                if (row.ManagerWorkforceProfileId.HasValue && row.ManagerWorkforceProfileId.Value != Guid.Empty)
                {
                    if (row.ManagerWorkforceProfileId.Value == row.WorkforceProfileId)
                    {
                        rowErrors.Add("Pegawai tidak dapat menjadi atasan bagi dirinya sendiri.");
                    }
                    else if (!activeManagers.ContainsKey(row.ManagerWorkforceProfileId.Value))
                    {
                        rowErrors.Add($"Atasan langsung dengan ID {row.ManagerWorkforceProfileId.Value} tidak ditemukan atau tidak aktif.");
                    }
                }

                // 8. Effective date validation
                if (row.EffectiveStartDate == default)
                {
                    rowErrors.Add("Tanggal mulai berlaku penempatan wajib diisi.");
                }

                // 9. Base salary validation (if provided)
                if (row.BaseSalary.HasValue && row.BaseSalary.Value < 0)
                {
                    rowErrors.Add("Besaran gaji pokok tidak boleh bernilai negatif.");
                }

                var rowValid = rowErrors.Count == 0;
                if (rowValid)
                {
                    validCount++;
                }
                else
                {
                    invalidCount++;
                    foreach (var err in rowErrors)
                    {
                        errors.Add($"Baris {rowNum}: {err}");
                    }
                }

                rowResults.Add(new RowValidationItemResponse
                {
                    RowIndex = i,
                    WorkforceProfileId = row.WorkforceProfileId,
                    IsValid = rowValid,
                    ErrorMessage = rowValid ? null : string.Join("; ", rowErrors)
                });
            }

            return new BatchValidationResultResponse
            {
                IsAllValid = invalidCount == 0 && rows.Count > 0,
                TotalCount = rows.Count,
                ValidCount = validCount,
                InvalidCount = invalidCount,
                ValidationErrors = errors,
                RowResults = rowResults
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

            for (int i = 0; i < request.Rows.Count; i++)
            {
                var rowDto = request.Rows[i];
                var rowVal = validation.RowResults.FirstOrDefault(r => r.RowIndex == i);
                var isRowValid = rowVal?.IsValid ?? false;
                var valMsg = rowVal?.ErrorMessage;

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
                    ValidationMessage = valMsg,
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
                    BaseSalary = null, // Confidential: disembunyikan demi kepatuhan privasi data gaji HR non-payroll (AUD-HRD07-12 / HRD-DEC-033)
                    EffectiveStartDate = r.EffectiveStartDate,
                    IsValid = r.IsValid,
                    ValidationMessage = r.ValidationMessage,
                    ProcessedAt = r.ProcessedAt
                }).ToList()
            };
        }

        public async Task<InitialBatchDetailResponse?> ValidateBatchByIdAsync(Guid id, CancellationToken ct = default)
        {
            var batch = await _dbContext.HrdInitialPlacementBatches
                .Include(x => x.Rows)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (batch == null) return null;

            if (batch.BatchStatus == "Executed" || batch.BatchStatus == "PendingApproval" || batch.BatchStatus == "Approved")
            {
                throw new InvalidOperationException($"Batch penempatan awal dalam status '{batch.BatchStatus}' tidak dapat divalidasi ulang atau diubah statusnya (AUD-HRD07-02 / HRD-DEC-054).");
            }

            var rowsDto = batch.Rows.Select(r => new CreateInitialBatchRowDto
            {
                WorkforceProfileId = r.WorkforceProfileId,
                OrganizationUnitId = r.OrganizationUnitId ?? Guid.Empty,
                DepartmentId = r.DepartmentId ?? Guid.Empty,
                PositionId = r.PositionId ?? Guid.Empty,
                ManagerWorkforceProfileId = r.ManagerWorkforceProfileId,
                SalaryGradeId = r.SalaryGradeId,
                BaseSalary = r.BaseSalary,
                EffectiveStartDate = r.EffectiveStartDate
            }).ToList();

            var validation = await ValidateRowsAsync(rowsDto, ct);
            var now = DateTime.UtcNow;

            for (int i = 0; i < batch.Rows.Count; i++)
            {
                var row = batch.Rows.ElementAt(i);
                var rowVal = validation.RowResults.FirstOrDefault(r => r.RowIndex == i);
                row.IsValid = rowVal?.IsValid ?? false;
                row.ValidationMessage = rowVal?.ErrorMessage;
                row.UpdateDateTime = now;
            }

            batch.TotalRows = batch.Rows.Count;
            batch.ValidatedRows = validation.ValidCount;
            batch.FailedRows = validation.InvalidCount;
            batch.BatchStatus = validation.IsAllValid ? "Validated" : "Draft";
            batch.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(ct);
            return await GetBatchByIdAsync(id, ct);
        }

        public async Task<InitialBatchDetailResponse> SubmitBatchAsync(Guid id, Guid userId, CancellationToken ct = default)
        {
            var batch = await _dbContext.HrdInitialPlacementBatches
                .Include(x => x.Rows)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (batch == null)
                throw new InvalidOperationException("Batch penempatan awal tidak ditemukan.");

            if (batch.BatchStatus != "Validated")
                throw new InvalidOperationException($"Hanya batch berstatus 'Validated' yang dapat diajukan (Status saat ini: '{batch.BatchStatus}').");

            if (batch.FailedRows > 0 || batch.Rows.Any(r => !r.IsValid))
                throw new InvalidOperationException($"Batch memuat baris yang tidak valid. Semua baris harus valid sebelum pengajuan persetujuan.");

            var now = DateTime.UtcNow;
            batch.BatchStatus = "PendingApproval";
            batch.SubmittedByUserId = userId;
            batch.SubmittedAt = now;
            batch.UpdateDateTime = now;
            batch.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(ct);
            return (await GetBatchByIdAsync(id, ct))!;
        }

        private const string BatchApprovalLockKey = "HRD_INITIAL_PLACEMENT_BATCH_APPROVAL";

        /// <summary>Kunci penasihat PostgreSQL di dalam transaksi aktif; dilewati di luar PostgreSQL.</summary>
        private async Task AcquireTransactionLockAsync(string lockKey, CancellationToken ct)
        {
            if (!_dbContext.Database.IsNpgsql())
                return;

            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [lockKey],
                ct);
        }

        public async Task<InitialBatchDetailResponse> ApproveBatchAsync(Guid id, Guid userId, CancellationToken ct = default)
        {
            var batch = await _dbContext.HrdInitialPlacementBatches
                .Include(x => x.Rows)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (batch == null)
                throw new InvalidOperationException("Batch penempatan awal tidak ditemukan.");

            if (batch.BatchStatus != "PendingApproval")
                throw new InvalidOperationException($"Hanya batch berstatus 'PendingApproval' yang dapat disetujui (Status saat ini: '{batch.BatchStatus}').");

            // Anti self-approval guard (Segregation of Duties - HRD-DEC-054)
            if (batch.CreateBy == userId || (batch.SubmittedByUserId.HasValue && batch.SubmittedByUserId.Value == userId))
            {
                throw new InvalidOperationException("Persetujuan batch penempatan awal ditolak: Pembuat draf maupun pengaju batch dilarang menyetujui batch yang diajukannya sendiri (Segregation of Duties / HRD-DEC-054).");
            }

            // Guard once-per-employee approved (AUD-HRD07-15 / HRD-DEC-054)
            var profileIds = batch.Rows.Select(r => r.WorkforceProfileId).Distinct().ToList();
            var conflictingEmployee = await _dbContext.HrdInitialPlacementBatchRows
                .Include(r => r.Batch)
                .Where(r => r.BatchId != batch.Id && profileIds.Contains(r.WorkforceProfileId) && (r.Batch.BatchStatus == "Approved" || r.Batch.BatchStatus == "Executed") && !r.Batch.IsDelete)
                .Select(r => new { r.WorkforceProfileId, r.Batch.BatchNumber, r.Batch.BatchStatus })
                .FirstOrDefaultAsync(ct);

            if (conflictingEmployee != null)
            {
                throw new InvalidOperationException($"Persetujuan batch ditolak: Pegawai dengan ID '{conflictingEmployee.WorkforceProfileId}' telah tercatat dalam batch '{conflictingEmployee.BatchNumber}' yang berstatus '{conflictingEmployee.BatchStatus}'. Satu pegawai dilarang berada di dua batch penempatan yang sama-sama disetujui (AUD-HRD07-15 / HRD-DEC-054).");
            }

            // BE-HRD-412: persetujuan batch diantrekan (kunci global persetujuan batch) dan status dibaca ulang,
            // sehingga dua approver serentak tidak sama-sama menyetujui, dan dua batch berisi pegawai yang sama
            // tidak lolos pemeriksaan sekali-per-pegawai secara bersamaan.
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            await AcquireTransactionLockAsync(BatchApprovalLockKey, ct);

            var liveStatus = await _dbContext.HrdInitialPlacementBatches
                .AsNoTracking()
                .Where(x => x.Id == batch.Id)
                .Select(x => x.BatchStatus)
                .FirstOrDefaultAsync(ct);
            if (liveStatus != "PendingApproval")
            {
                throw new InvalidOperationException($"Batch sudah diproses oleh petugas lain (status saat ini: '{liveStatus}'). Muat ulang data.");
            }

            var conflictAfterLock = await _dbContext.HrdInitialPlacementBatchRows
                .Where(r => r.BatchId != batch.Id && profileIds.Contains(r.WorkforceProfileId) && (r.Batch!.BatchStatus == "Approved" || r.Batch.BatchStatus == "Executed") && !r.Batch.IsDelete)
                .Select(r => r.Batch!.BatchNumber)
                .FirstOrDefaultAsync(ct);
            if (conflictAfterLock != null)
            {
                throw new InvalidOperationException($"Persetujuan batch ditolak: sebagian pegawai baru saja disetujui pada batch '{conflictAfterLock}' (HRD-DEC-054).");
            }

            var now = DateTime.UtcNow;
            batch.BatchStatus = "Approved";
            batch.ApprovedByUserId = userId;
            batch.ApprovedAt = now;
            batch.UpdateDateTime = now;
            batch.UpdateBy = userId;
            await _dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return (await GetBatchByIdAsync(id, ct))!;
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

            if (batch.BatchStatus != "Approved")
            {
                throw new InvalidOperationException($"Batch dengan status '{batch.BatchStatus}' tidak dapat dieksekusi. Hanya batch berstatus 'Approved' yang dapat dieksekusi (HRD-DEC-054).");
            }

            if (batch.Rows == null || batch.Rows.Count == 0)
            {
                throw new InvalidOperationException("Batch penempatan awal tidak memiliki baris data untuk dieksekusi.");
            }

            if (batch.FailedRows > 0 || batch.Rows.Any(r => !r.IsValid))
            {
                throw new InvalidOperationException($"Batch memuat {batch.FailedRows} baris tidak valid. Seluruh baris harus berstatus valid sebelum eksekusi.");
            }

            // Once-per-employee approved validation (HRD-DEC-054)
            var profileIds = batch.Rows.Select(r => r.WorkforceProfileId).Distinct().ToList();
            var alreadyExecutedEmployeeId = await _dbContext.HrdInitialPlacementBatchRows
                .Include(r => r.Batch)
                .Where(r => r.BatchId != batch.Id && profileIds.Contains(r.WorkforceProfileId) && r.Batch.BatchStatus == "Executed" && !r.Batch.IsDelete)
                .Select(r => r.WorkforceProfileId)
                .FirstOrDefaultAsync(ct);

            if (alreadyExecutedEmployeeId != Guid.Empty)
            {
                throw new InvalidOperationException($"Pegawai dengan ID '{alreadyExecutedEmployeeId}' telah pernah diproses dalam batch inisialisasi awal yang sudah dieksekusi sebelumnya. Satu pegawai hanya boleh diproses satu kali melalui batch penempatan awal (HRD-DEC-054).");
            }

            // Re-validate against live master data right before transaction
            var rowsDto = batch.Rows.Select(r => new CreateInitialBatchRowDto
            {
                WorkforceProfileId = r.WorkforceProfileId,
                OrganizationUnitId = r.OrganizationUnitId ?? Guid.Empty,
                DepartmentId = r.DepartmentId ?? Guid.Empty,
                PositionId = r.PositionId ?? Guid.Empty,
                ManagerWorkforceProfileId = r.ManagerWorkforceProfileId,
                SalaryGradeId = r.SalaryGradeId,
                BaseSalary = r.BaseSalary,
                EffectiveStartDate = r.EffectiveStartDate
            }).ToList();

            var revalidation = await ValidateRowsAsync(rowsDto, ct);
            if (!revalidation.IsAllValid)
            {
                throw new InvalidOperationException($"Validasi integritas pra-eksekusi gagal: {string.Join("; ", revalidation.ValidationErrors.Take(3))}");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                // BE-HRD-412: eksekusi tepat sekali walau dua permintaan datang bersamaan.
                await AcquireTransactionLockAsync($"HRD_INITIAL_PLACEMENT_EXECUTE_{batch.Id}", ct);
                var liveStatus = await _dbContext.HrdInitialPlacementBatches
                    .AsNoTracking()
                    .Where(x => x.Id == batch.Id)
                    .Select(x => x.BatchStatus)
                    .FirstOrDefaultAsync(ct);
                if (liveStatus != "Approved")
                {
                    throw new InvalidOperationException($"Batch sudah diproses oleh petugas lain (status saat ini: '{liveStatus}').");
                }

                var now = DateTime.UtcNow;

                var profiles = await _dbContext.MstWorkforceProfiles
                    .Where(p => profileIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, ct);

                foreach (var row in batch.Rows)
                {
                    if (!row.PositionId.HasValue || row.PositionId.Value == Guid.Empty)
                    {
                        throw new InvalidOperationException($"Baris pegawai {row.WorkforceProfileId} tidak memiliki jabatan yang valid.");
                    }

                    // Strict protection: do not overwrite if employee already has active primary position
                    var hasExistingActivePosition = await _dbContext.WfpPositionAssignments
                        .AnyAsync(x => x.WorkforceProfileId == row.WorkforceProfileId &&
                                       x.IsPrimary && x.IsActive && !x.IsDelete &&
                                       (x.EffectiveEndDate == null || x.EffectiveEndDate >= now.Date), ct);

                    if (hasExistingActivePosition)
                    {
                        var employeeLabel = profiles.TryGetValue(row.WorkforceProfileId, out var p) ? p.DisplayName : row.WorkforceProfileId.ToString();
                        throw new InvalidOperationException($"Pegawai '{employeeLabel}' sudah memiliki penempatan jabatan primer aktif di sistem. Eksekusi dibatalkan untuk mencegah penimpaan tidak sah.");
                    }

                    // 1. Position Assignment (Substantive)
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
                        ApprovedByUserId = batch.ApprovedByUserId ?? userId,
                        ApprovedAt = now,
                        CreateBy = userId,
                        CreateDateTime = now
                    };
                    _dbContext.WfpPositionAssignments.Add(newPos);

                    // Update primary position & department on profile HANYA jika tanggal mulai sudah efektif hari ini (AUD-HRD07-10 / HRD-DEC-047)
                    if (profiles.TryGetValue(row.WorkforceProfileId, out var profile) && row.EffectiveStartDate.Date <= now.Date)
                    {
                        profile.PrimaryPositionId = row.PositionId.Value;
                        if (row.DepartmentId.HasValue && row.DepartmentId.Value != Guid.Empty)
                        {
                            profile.PrimaryDepartmentId = row.DepartmentId.Value;
                        }
                        profile.UpdateDateTime = now;
                        profile.UpdateBy = userId;
                    }

                    // 2. Manager Assignment (only if specified and valid)
                    if (row.ManagerWorkforceProfileId.HasValue &&
                        row.ManagerWorkforceProfileId.Value != Guid.Empty &&
                        row.ManagerWorkforceProfileId.Value != row.WorkforceProfileId)
                    {
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
                            ApprovedByUserId = batch.ApprovedByUserId ?? userId,
                            ApprovedAt = now,
                            CreateBy = userId,
                            CreateDateTime = now
                        };
                        _dbContext.WfpManagerAssignments.Add(newMgr);
                    }

                    // 3. Salary Assignment - STOPPED per HRD-DEC-036 & HRD-DEC-054
                    // Batch data awal penempatan pegawai tidak boleh langsung menetapkan gaji tanpa workflow approval payroll terpisah.

                    row.IsValid = true;
                    row.ProcessedAt = now;
                    row.UpdateDateTime = now;
                    row.UpdateBy = userId;
                }

                batch.BatchStatus = "Executed";
                batch.ExecutionSummary = $"Sukses menerapkan penempatan awal untuk {batch.Rows.Count} pegawai baseline.";
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
