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
    public static class InitialPlacementDiagnosticCodes
    {
        // Kategori Kelayakan (Diagnostic Categories)
        public const string SiapDiproses = "SIAP_DIPROSES";
        public const string DilewatiSudahDitempatkan = "DILEWATI_SUDAH_DITEMPATKAN";
        public const string PerluPerbaikan = "PERLU_PERBAIKAN";
        public const string PerluRekonsiliasi = "PERLU_REKONSILIASI";

        // Kode Alasan (Reason Codes)
        public const string EligibleInitialPlacement = "ELIGIBLE_INITIAL_PLACEMENT";
        public const string AlreadyHasActivePrimaryPosition = "ALREADY_HAS_ACTIVE_PRIMARY_POSITION";
        public const string AlreadyInApprovedInitialBatch = "ALREADY_IN_APPROVED_INITIAL_BATCH";
        public const string PositionNotAssigned = "POSITION_NOT_ASSIGNED";
        public const string PositionDepartmentMismatch = "POSITION_DEPARTMENT_MISMATCH";
        public const string OrganizationUnitDepartmentMismatch = "ORGANIZATION_UNIT_DEPARTMENT_MISMATCH";
        public const string EmployeeInactiveOrMissing = "EMPLOYEE_INACTIVE_OR_MISSING";
        public const string ManagerInvalidOrSelf = "MANAGER_INVALID_OR_SELF";
        public const string EffectiveDateUnverified = "EFFECTIVE_DATE_UNVERIFIED";
        public const string DuplicateInSelectionOrConcurrentChange = "DUPLICATE_IN_SELECTION_OR_CONCURRENT_CHANGE";

        // Kunci Entitas Tujuan Navigasi Aman
        public const string MasterPosition = "MASTER_POSITION";
        public const string MasterDepartment = "MASTER_DEPARTMENT";
        public const string MasterOrganizationUnit = "MASTER_ORGANIZATION_UNIT";
        public const string OrganizationAssignment = "ORGANIZATION_ASSIGNMENT";
        public const string EmployeeProfile = "EMPLOYEE_PROFILE";
        public const string PositionAssignment = "POSITION_ASSIGNMENT";
        public const string EmployeeManager = "EMPLOYEE_MANAGER";
        public const string PersonnelActionOrder = "PERSONNEL_ACTION_ORDER";
    }

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

            var profileIds = orgAssignments.Select(x => x.WorkforceProfileId).Distinct().ToList();

            // Bulk fetch active primary position assignments (eliminates N+1)
            var activePositions = await _dbContext.WfpPositionAssignments
                .AsNoTracking()
                .Where(x => profileIds.Contains(x.WorkforceProfileId) &&
                            x.IsPrimary && x.IsActive && !x.IsDelete &&
                            (x.EffectiveEndDate == null || x.EffectiveEndDate >= today))
                .ToDictionaryAsync(x => x.WorkforceProfileId, ct);

            // Bulk fetch check if already in an approved/executed initial batch (HRD-DEC-054)
            var approvedBatchRows = await _dbContext.HrdInitialPlacementBatchRows
                .Include(r => r.Batch)
                .AsNoTracking()
                .Where(r => profileIds.Contains(r.WorkforceProfileId) &&
                            (r.Batch.BatchStatus == "Approved" || r.Batch.BatchStatus == "Executed") &&
                            !r.Batch.IsDelete)
                .ToDictionaryAsync(r => r.WorkforceProfileId, r => r.Batch.BatchNumber, ct);

            // Collect position IDs for validation & department checking
            var positionIds = orgAssignments
                .Select(x => x.PositionId != Guid.Empty ? x.PositionId : (x.WorkforceProfile?.PrimaryPositionId ?? Guid.Empty))
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            var masterPositions = await _dbContext.MstPositions
                .Include(p => p.Department)
                .AsNoTracking()
                .Where(p => positionIds.Contains(p.Id) && !p.IsDelete && p.IsActive)
                .ToDictionaryAsync(p => p.Id, ct);

            var result = new List<InitialBatchRowResponse>();

            foreach (var org in orgAssignments)
            {
                if (org.WorkforceProfile == null) continue;

                var profile = org.WorkforceProfile;

                // Resolve prefilled position if available from org assignment or profile
                Guid? positionId = null;
                string? positionName = null;

                if (org.PositionId != Guid.Empty)
                {
                    positionId = org.PositionId;
                    positionName = org.Position?.PositionName;
                }
                else if (profile.PrimaryPositionId.HasValue && profile.PrimaryPositionId.Value != Guid.Empty)
                {
                    positionId = profile.PrimaryPositionId.Value;
                    positionName = profile.PrimaryPosition?.PositionName;
                }

                // Unified evaluation conforming to diagnostic categories & rules
                bool isValid = true;
                string? validationMessage = null;
                string eligibilityStatus = InitialPlacementDiagnosticCodes.SiapDiproses;
                string reasonCode = InitialPlacementDiagnosticCodes.EligibleInitialPlacement;
                string? targetResourceKey = null;
                Guid? targetRecordId = null;

                if (approvedBatchRows.TryGetValue(org.WorkforceProfileId, out var existingBatchNo))
                {
                    isValid = false;
                    eligibilityStatus = InitialPlacementDiagnosticCodes.DilewatiSudahDitempatkan;
                    reasonCode = InitialPlacementDiagnosticCodes.AlreadyInApprovedInitialBatch;
                    validationMessage = $"Pegawai sudah tercatat dalam batch '{existingBatchNo}' yang telah disetujui atau dieksekusi sebelumnya.";
                    targetResourceKey = InitialPlacementDiagnosticCodes.PositionAssignment;
                }
                else if (activePositions.TryGetValue(org.WorkforceProfileId, out var activePos))
                {
                    isValid = false;
                    eligibilityStatus = InitialPlacementDiagnosticCodes.DilewatiSudahDitempatkan;
                    reasonCode = InitialPlacementDiagnosticCodes.AlreadyHasActivePrimaryPosition;
                    validationMessage = "Sudah memiliki jabatan aktif. Tidak dapat diproses melalui batch inisialisasi awal.";
                    targetResourceKey = InitialPlacementDiagnosticCodes.PositionAssignment;
                    targetRecordId = activePos.Id;
                }
                else if (!profile.IsActive)
                {
                    isValid = false;
                    eligibilityStatus = InitialPlacementDiagnosticCodes.PerluPerbaikan;
                    reasonCode = InitialPlacementDiagnosticCodes.EmployeeInactiveOrMissing;
                    validationMessage = $"Pegawai {profile.DisplayName} dalam status non-aktif.";
                    targetResourceKey = InitialPlacementDiagnosticCodes.EmployeeProfile;
                    targetRecordId = profile.Id;
                }
                else if (!positionId.HasValue || positionId.Value == Guid.Empty)
                {
                    isValid = false;
                    eligibilityStatus = InitialPlacementDiagnosticCodes.PerluPerbaikan;
                    reasonCode = InitialPlacementDiagnosticCodes.PositionNotAssigned;
                    validationMessage = "Jabatan belum ditentukan pada data baseline organisasi.";
                    targetResourceKey = InitialPlacementDiagnosticCodes.MasterPosition;
                }
                else if (!masterPositions.TryGetValue(positionId.Value, out var positionRecord))
                {
                    isValid = false;
                    eligibilityStatus = InitialPlacementDiagnosticCodes.PerluPerbaikan;
                    reasonCode = InitialPlacementDiagnosticCodes.PositionNotAssigned;
                    validationMessage = $"Jabatan dengan ID {positionId.Value} tidak ditemukan atau tidak aktif.";
                    targetResourceKey = InitialPlacementDiagnosticCodes.MasterPosition;
                    targetRecordId = positionId.Value;
                }
                else if (org.DepartmentId != Guid.Empty && positionRecord.DepartmentId != Guid.Empty && positionRecord.DepartmentId != org.DepartmentId)
                {
                    isValid = false;
                    eligibilityStatus = InitialPlacementDiagnosticCodes.PerluPerbaikan;
                    reasonCode = InitialPlacementDiagnosticCodes.PositionDepartmentMismatch;
                    validationMessage = $"Jabatan '{positionRecord.PositionName}' tidak sesuai dengan departemen '{org.Department?.DepartmentName}'.";
                    targetResourceKey = InitialPlacementDiagnosticCodes.MasterPosition;
                    targetRecordId = positionRecord.Id;
                }
                else if (org.OrganizationUnitId != Guid.Empty && org.OrganizationUnit?.DepartmentId.HasValue == true &&
                         org.OrganizationUnit.DepartmentId.Value != Guid.Empty &&
                         org.DepartmentId != Guid.Empty && org.OrganizationUnit.DepartmentId.Value != org.DepartmentId)
                {
                    isValid = false;
                    eligibilityStatus = InitialPlacementDiagnosticCodes.PerluPerbaikan;
                    reasonCode = InitialPlacementDiagnosticCodes.OrganizationUnitDepartmentMismatch;
                    validationMessage = "Departemen tidak selaras dengan unit organisasi yang dipilih.";
                    targetResourceKey = InitialPlacementDiagnosticCodes.MasterOrganizationUnit;
                    targetRecordId = org.OrganizationUnitId;
                }
                else
                {
                    isValid = true;
                    eligibilityStatus = InitialPlacementDiagnosticCodes.SiapDiproses;
                    reasonCode = InitialPlacementDiagnosticCodes.EligibleInitialPlacement;
                    validationMessage = null;
                }

                // Default suggested effective date: join date or today
                var effectiveDate = org.EffectiveStartDate != default ? org.EffectiveStartDate : today;

                result.Add(new InitialBatchRowResponse
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = org.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    EmployeeName = profile.DisplayName,
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
                    ValidationMessage = validationMessage,
                    EligibilityStatus = eligibilityStatus,
                    ReasonCode = reasonCode,
                    ReasonMessage = validationMessage ?? "Kandidat memenuhi syarat integritas penempatan awal.",
                    TargetResourceKey = targetResourceKey,
                    TargetRecordId = targetRecordId
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
            var existingPositions = await _dbContext.WfpPositionAssignments
                .AsNoTracking()
                .Where(x => profileIds.Contains(x.WorkforceProfileId) &&
                            x.IsPrimary && x.IsActive && !x.IsDelete &&
                            (x.EffectiveEndDate == null || x.EffectiveEndDate >= today))
                .ToDictionaryAsync(x => x.WorkforceProfileId, ct);

            // Check approved/executed initial placement batch rows (AUD-HRD07-15 / HRD-DEC-054)
            var approvedBatchRows = await _dbContext.HrdInitialPlacementBatchRows
                .Include(r => r.Batch)
                .AsNoTracking()
                .Where(r => profileIds.Contains(r.WorkforceProfileId) &&
                            (r.Batch.BatchStatus == "Approved" || r.Batch.BatchStatus == "Executed") &&
                            !r.Batch.IsDelete)
                .ToDictionaryAsync(r => r.WorkforceProfileId, r => r.Batch.BatchNumber, ct);

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var rowNum = i + 1;
                var rowErrors = new List<string>();

                string eligibilityStatus = InitialPlacementDiagnosticCodes.SiapDiproses;
                string reasonCode = InitialPlacementDiagnosticCodes.EligibleInitialPlacement;
                string? targetResourceKey = null;
                Guid? targetRecordId = null;

                // 1. Duplicate check in batch
                if (profileGroupCounts.TryGetValue(row.WorkforceProfileId, out var count) && count > 1)
                {
                    rowErrors.Add("Pegawai tercantum lebih dari satu kali dalam batch yang sama.");
                    eligibilityStatus = InitialPlacementDiagnosticCodes.PerluPerbaikan;
                    reasonCode = InitialPlacementDiagnosticCodes.DuplicateInSelectionOrConcurrentChange;
                    targetResourceKey = InitialPlacementDiagnosticCodes.EmployeeProfile;
                    targetRecordId = row.WorkforceProfileId;
                }

                // 2. Profile existence & active check
                if (!activeProfiles.TryGetValue(row.WorkforceProfileId, out var profile))
                {
                    rowErrors.Add($"Pegawai dengan ID {row.WorkforceProfileId} tidak ditemukan atau sudah dihapus.");
                    eligibilityStatus = InitialPlacementDiagnosticCodes.PerluPerbaikan;
                    reasonCode = InitialPlacementDiagnosticCodes.EmployeeInactiveOrMissing;
                    targetResourceKey = InitialPlacementDiagnosticCodes.EmployeeProfile;
                    targetRecordId = row.WorkforceProfileId;
                }
                else if (!profile.IsActive)
                {
                    rowErrors.Add($"Pegawai {profile.DisplayName} dalam status non-aktif.");
                    eligibilityStatus = InitialPlacementDiagnosticCodes.PerluPerbaikan;
                    reasonCode = InitialPlacementDiagnosticCodes.EmployeeInactiveOrMissing;
                    targetResourceKey = InitialPlacementDiagnosticCodes.EmployeeProfile;
                    targetRecordId = profile.Id;
                }

                // 3. Already in approved/executed batch
                if (approvedBatchRows.TryGetValue(row.WorkforceProfileId, out var existingBatchNo))
                {
                    rowErrors.Add($"Pegawai sudah tercatat dalam batch '{existingBatchNo}' yang telah disetujui atau dieksekusi sebelumnya (HRD-DEC-054).");
                    eligibilityStatus = InitialPlacementDiagnosticCodes.DilewatiSudahDitempatkan;
                    reasonCode = InitialPlacementDiagnosticCodes.AlreadyInApprovedInitialBatch;
                    targetResourceKey = InitialPlacementDiagnosticCodes.PositionAssignment;
                }

                // 4. Conflict with existing placement check
                if (existingPositions.TryGetValue(row.WorkforceProfileId, out var existingPos))
                {
                    rowErrors.Add("Pegawai sudah memiliki penempatan jabatan primer aktif di sistem.");
                    eligibilityStatus = InitialPlacementDiagnosticCodes.DilewatiSudahDitempatkan;
                    reasonCode = InitialPlacementDiagnosticCodes.AlreadyHasActivePrimaryPosition;
                    targetResourceKey = InitialPlacementDiagnosticCodes.PositionAssignment;
                    targetRecordId = existingPos.Id;
                }

                // 5. Position validation & department consistency
                if (row.PositionId == Guid.Empty)
                {
                    rowErrors.Add("Jabatan wajib dipilih.");
                    if (eligibilityStatus == InitialPlacementDiagnosticCodes.SiapDiproses)
                    {
                        eligibilityStatus = InitialPlacementDiagnosticCodes.PerluPerbaikan;
                        reasonCode = InitialPlacementDiagnosticCodes.PositionNotAssigned;
                        targetResourceKey = InitialPlacementDiagnosticCodes.MasterPosition;
                    }
                }
                else if (!activePositions.TryGetValue(row.PositionId, out var position))
                {
                    rowErrors.Add($"Jabatan dengan ID {row.PositionId} tidak ditemukan atau tidak aktif.");
                    if (eligibilityStatus == InitialPlacementDiagnosticCodes.SiapDiproses)
                    {
                        eligibilityStatus = InitialPlacementDiagnosticCodes.PerluPerbaikan;
                        reasonCode = InitialPlacementDiagnosticCodes.PositionNotAssigned;
                        targetResourceKey = InitialPlacementDiagnosticCodes.MasterPosition;
                        targetRecordId = row.PositionId;
                    }
                }
                else
                {
                    if (row.DepartmentId != Guid.Empty && position.DepartmentId != Guid.Empty && position.DepartmentId != row.DepartmentId)
                    {
                        rowErrors.Add($"Jabatan '{position.PositionName}' tidak sesuai dengan departemen yang dipilih.");
                        if (eligibilityStatus == InitialPlacementDiagnosticCodes.SiapDiproses)
                        {
                            eligibilityStatus = InitialPlacementDiagnosticCodes.PerluPerbaikan;
                            reasonCode = InitialPlacementDiagnosticCodes.PositionDepartmentMismatch;
                            targetResourceKey = InitialPlacementDiagnosticCodes.MasterPosition;
                            targetRecordId = position.Id;
                        }
                    }
                }

                // 6. Department validation
                if (row.DepartmentId != Guid.Empty && !activeDepartments.ContainsKey(row.DepartmentId))
                {
                    rowErrors.Add($"Departemen dengan ID {row.DepartmentId} tidak ditemukan atau tidak aktif.");
                    if (eligibilityStatus == InitialPlacementDiagnosticCodes.SiapDiproses)
                    {
                        eligibilityStatus = InitialPlacementDiagnosticCodes.PerluPerbaikan;
                        reasonCode = InitialPlacementDiagnosticCodes.PositionDepartmentMismatch;
                        targetResourceKey = InitialPlacementDiagnosticCodes.MasterDepartment;
                        targetRecordId = row.DepartmentId;
                    }
                }

                // 7. Organization unit validation
                if (row.OrganizationUnitId != Guid.Empty && !activeOrgUnits.ContainsKey(row.OrganizationUnitId))
                {
                    rowErrors.Add($"Unit organisasi dengan ID {row.OrganizationUnitId} tidak ditemukan atau tidak aktif.");
                    if (eligibilityStatus == InitialPlacementDiagnosticCodes.SiapDiproses)
                    {
                        eligibilityStatus = InitialPlacementDiagnosticCodes.PerluPerbaikan;
                        reasonCode = InitialPlacementDiagnosticCodes.OrganizationUnitDepartmentMismatch;
                        targetResourceKey = InitialPlacementDiagnosticCodes.MasterOrganizationUnit;
                        targetRecordId = row.OrganizationUnitId;
                    }
                }
                else if (row.OrganizationUnitId != Guid.Empty &&
                         activeOrgUnits.TryGetValue(row.OrganizationUnitId, out var orgUnit) &&
                         orgUnit.DepartmentId.HasValue && orgUnit.DepartmentId.Value != Guid.Empty &&
                         row.DepartmentId != Guid.Empty && orgUnit.DepartmentId.Value != row.DepartmentId)
                {
                    rowErrors.Add("Departemen tidak selaras dengan unit organisasi yang dipilih.");
                    if (eligibilityStatus == InitialPlacementDiagnosticCodes.SiapDiproses)
                    {
                        eligibilityStatus = InitialPlacementDiagnosticCodes.PerluPerbaikan;
                        reasonCode = InitialPlacementDiagnosticCodes.OrganizationUnitDepartmentMismatch;
                        targetResourceKey = InitialPlacementDiagnosticCodes.MasterOrganizationUnit;
                        targetRecordId = row.OrganizationUnitId;
                    }
                }

                // 8. Manager validation
                if (row.ManagerWorkforceProfileId.HasValue && row.ManagerWorkforceProfileId.Value != Guid.Empty)
                {
                    if (row.ManagerWorkforceProfileId.Value == row.WorkforceProfileId)
                    {
                        rowErrors.Add("Pegawai tidak dapat menjadi atasan bagi dirinya sendiri.");
                        if (eligibilityStatus == InitialPlacementDiagnosticCodes.SiapDiproses)
                        {
                            eligibilityStatus = InitialPlacementDiagnosticCodes.PerluPerbaikan;
                            reasonCode = InitialPlacementDiagnosticCodes.ManagerInvalidOrSelf;
                            targetResourceKey = InitialPlacementDiagnosticCodes.EmployeeManager;
                        }
                    }
                    else if (!activeManagers.ContainsKey(row.ManagerWorkforceProfileId.Value))
                    {
                        rowErrors.Add($"Atasan langsung dengan ID {row.ManagerWorkforceProfileId.Value} tidak ditemukan atau tidak aktif.");
                        if (eligibilityStatus == InitialPlacementDiagnosticCodes.SiapDiproses)
                        {
                            eligibilityStatus = InitialPlacementDiagnosticCodes.PerluPerbaikan;
                            reasonCode = InitialPlacementDiagnosticCodes.ManagerInvalidOrSelf;
                            targetResourceKey = InitialPlacementDiagnosticCodes.EmployeeManager;
                            targetRecordId = row.ManagerWorkforceProfileId.Value;
                        }
                    }
                }

                // 9. Effective date validation
                if (row.EffectiveStartDate == default)
                {
                    rowErrors.Add("Tanggal mulai berlaku penempatan wajib diisi.");
                    if (eligibilityStatus == InitialPlacementDiagnosticCodes.SiapDiproses)
                    {
                        eligibilityStatus = InitialPlacementDiagnosticCodes.PerluRekonsiliasi;
                        reasonCode = InitialPlacementDiagnosticCodes.EffectiveDateUnverified;
                        targetResourceKey = InitialPlacementDiagnosticCodes.PersonnelActionOrder;
                    }
                }

                // 10. Base salary validation (if provided)
                if (row.BaseSalary.HasValue && row.BaseSalary.Value < 0)
                {
                    rowErrors.Add("Besaran gaji pokok tidak boleh bernilai negatif.");
                }

                var rowValid = rowErrors.Count == 0;
                if (rowValid)
                {
                    validCount++;
                    eligibilityStatus = InitialPlacementDiagnosticCodes.SiapDiproses;
                    reasonCode = InitialPlacementDiagnosticCodes.EligibleInitialPlacement;
                }
                else
                {
                    invalidCount++;
                    foreach (var err in rowErrors)
                    {
                        errors.Add($"Baris {rowNum}: {err}");
                    }
                }

                var combinedError = rowValid ? null : string.Join("; ", rowErrors);

                rowResults.Add(new RowValidationItemResponse
                {
                    RowIndex = i,
                    WorkforceProfileId = row.WorkforceProfileId,
                    IsValid = rowValid,
                    ErrorMessage = combinedError,
                    EligibilityStatus = eligibilityStatus,
                    ReasonCode = reasonCode,
                    ReasonMessage = combinedError ?? "Kandidat memenuhi syarat integritas penempatan awal.",
                    TargetResourceKey = targetResourceKey,
                    TargetRecordId = targetRecordId
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

            // Proteksi ketat: Tolak pembentukan batch jika memuat kandidat invalid (AUD-HRD07-06 / HRD-DEC-054)
            if (!validation.IsAllValid)
            {
                var sampleError = validation.ValidationErrors.FirstOrDefault() ?? "Kandidat terpilih tidak memenuhi syarat.";
                throw new InvalidOperationException($"Pembuatan draft batch ditolak: Terdapat {validation.InvalidCount} baris pegawai yang tidak valid ({sampleError}). Harap pastikan hanya kandidat yang valid dan memenuhi syarat yang diajukan.");
            }

            // Guard once-per-employee across approved/executed batches (AUD-HRD07-15 / HRD-DEC-054)
            var profileIds = request.Rows.Select(r => r.WorkforceProfileId).Distinct().ToList();
            var conflictingEmployee = await _dbContext.HrdInitialPlacementBatchRows
                .Include(r => r.Batch)
                .AsNoTracking()
                .Where(r => profileIds.Contains(r.WorkforceProfileId) &&
                            (r.Batch.BatchStatus == "Approved" || r.Batch.BatchStatus == "Executed") &&
                            !r.Batch.IsDelete)
                .Select(r => new { r.WorkforceProfileId, r.Batch.BatchNumber, r.Batch.BatchStatus })
                .FirstOrDefaultAsync(ct);

            if (conflictingEmployee != null)
            {
                throw new InvalidOperationException($"Pembuatan draft batch ditolak: Pegawai dengan ID '{conflictingEmployee.WorkforceProfileId}' telah tercatat dalam batch '{conflictingEmployee.BatchNumber}' yang berstatus '{conflictingEmployee.BatchStatus}'. Satu pegawai dilarang berada di dua batch penempatan yang sama-sama diproses (AUD-HRD07-15 / HRD-DEC-054).");
            }

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
                BatchStatus = "Validated",
                TotalRows = request.Rows.Count,
                ValidatedRows = validation.ValidCount,
                FailedRows = 0,
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
                    ProcessedAt = r.ProcessedAt,
                    EligibilityStatus = r.IsValid ? InitialPlacementDiagnosticCodes.SiapDiproses : InitialPlacementDiagnosticCodes.PerluPerbaikan,
                    ReasonCode = r.IsValid ? InitialPlacementDiagnosticCodes.EligibleInitialPlacement : InitialPlacementDiagnosticCodes.PositionDepartmentMismatch,
                    ReasonMessage = r.ValidationMessage ?? (r.IsValid ? "Kandidat memenuhi syarat penempatan awal." : null)
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
