using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    public class HrdPersonnelActionOrderService : IHrdPersonnelActionOrderService
    {
        private readonly ApplicationDbContext _dbContext;
        private const string OrderNumberPrefix = "SK-RSMMC-";
        private const string AdvisoryLockName = "HR_PERSONNEL_ACTION_ORDER_NUMBER";

        public HrdPersonnelActionOrderService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<PersonnelActionOrderListResponse>> GetOrdersAsync(
            int pageNumber,
            int pageSize,
            string? actionCategory,
            string? orderStatus,
            Guid? organizationUnitId,
            DateTime? startDate,
            DateTime? endDate,
            string? search,
            CancellationToken ct = default)
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _dbContext.HrdPersonnelActionOrders
                .Include(x => x.WorkforceProfile)
                .Include(x => x.TargetOrganizationUnit)
                .Include(x => x.TargetDepartment)
                .Include(x => x.TargetPosition)
                .Include(x => x.TargetManagerProfile)
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(actionCategory))
            {
                query = query.Where(x => x.ActionCategory == actionCategory);
            }

            if (!string.IsNullOrWhiteSpace(orderStatus))
            {
                query = query.Where(x => x.OrderStatus == orderStatus);
            }

            if (organizationUnitId.HasValue && organizationUnitId.Value != Guid.Empty)
            {
                query = query.Where(x => x.TargetOrganizationUnitId == organizationUnitId.Value);
            }

            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                query = query.Where(x => x.EffectiveDate >= start);
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date;
                query = query.Where(x => x.EffectiveDate <= end);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.ActionOrderNumber.ToLower().Contains(keyword) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.DisplayName.ToLower().Contains(keyword)) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.ProfileCode.ToLower().Contains(keyword)) ||
                    x.ReasonText.ToLower().Contains(keyword));
            }

            var totalItems = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PersonnelActionOrderListResponse
                {
                    Id = x.Id,
                    ActionOrderNumber = x.ActionOrderNumber,
                    WorkforceProfileId = x.WorkforceProfileId,
                    EmployeeName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    ActionCategory = x.ActionCategory,
                    EffectiveDate = x.EffectiveDate,
                    PostingDate = x.PostingDate,
                    OrderStatus = x.OrderStatus,
                    ReasonText = x.ReasonText,
                    HasSignedDocument = !string.IsNullOrEmpty(x.SignedDocumentPath),
                    SignedDocumentFileName = x.SignedDocumentFileName,
                    TargetOrganizationUnitName = x.TargetOrganizationUnit != null ? x.TargetOrganizationUnit.UnitName : null,
                    TargetDepartmentName = x.TargetDepartment != null ? x.TargetDepartment.DepartmentName : null,
                    TargetPositionName = x.TargetPosition != null ? x.TargetPosition.PositionName : null,
                    TargetManagerName = x.TargetManagerProfile != null ? x.TargetManagerProfile.DisplayName : null,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(ct);

            return new PagedResult<PersonnelActionOrderListResponse>
            {
                Items = items,
                TotalData = totalItems,
                TotalPage = (int)Math.Ceiling(totalItems / (double)pageSize),
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PersonnelActionOrderDetailResponse?> GetOrderByIdAsync(Guid id, CancellationToken ct = default)
        {
            var order = await _dbContext.HrdPersonnelActionOrders
                .Include(x => x.WorkforceProfile)
                .Include(x => x.TargetOrganizationUnit)
                .Include(x => x.TargetDepartment)
                .Include(x => x.TargetPosition)
                .Include(x => x.TargetManagerProfile)
                .Include(x => x.Attachments.Where(a => !a.IsDelete))
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (order == null) return null;

            return MapToDetailResponse(order);
        }

        public async Task<PersonnelActionOrderSummaryResponse> GetSummaryAsync(CancellationToken ct = default)
        {
            var orders = await _dbContext.HrdPersonnelActionOrders
                .Where(x => !x.IsDelete)
                .Select(x => x.OrderStatus)
                .ToListAsync(ct);

            return new PersonnelActionOrderSummaryResponse
            {
                TotalOrders = orders.Count,
                DraftCount = orders.Count(x => x == "Draft"),
                PendingApprovalCount = orders.Count(x => x == "PendingApproval"),
                ScheduledCount = orders.Count(x => x == "Scheduled"),
                AppliedCount = orders.Count(x => x == "Applied"),
                RejectedCount = orders.Count(x => x == "Rejected"),
                CancelledCount = orders.Count(x => x == "Cancelled")
            };
        }

        public Task<PersonnelActionOrderFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken ct = default)
        {
            return Task.FromResult(new PersonnelActionOrderFilterMetadataResponse
            {
                ActionCategories = new List<string> { "Mutation", "Promotion", "Rotation", "Demotion", "TemporaryAssignment" },
                OrderStatuses = new List<string> { "Draft", "PendingApproval", "Scheduled", "Applied", "Rejected", "Cancelled" }
            });
        }

        public async Task<PersonnelActionOrderDetailResponse> CreateDraftOrderAsync(
            CreatePersonnelActionOrderRequest request,
            Guid userId,
            CancellationToken ct = default)
        {
            if (request.WorkforceProfileId == Guid.Empty)
                throw new ArgumentException("Pegawai target wajib dipilih.");

            var employee = await _dbContext.MstWorkforceProfiles
                .FirstOrDefaultAsync(x => x.Id == request.WorkforceProfileId && !x.IsDelete, ct);
            if (employee == null)
                throw new InvalidOperationException("Data pegawai target tidak ditemukan.");

            await ValidatePlacementCombinationAsync(
                request.WorkforceProfileId,
                request.TargetOrganizationUnitId,
                request.TargetDepartmentId,
                request.TargetPositionId,
                request.TargetManagerProfileId,
                request.EffectiveDate,
                ct);

            var orderNumber = await GenerateOrderNumberAsync(ct);

            var order = new HrdPersonnelActionOrder
            {
                ActionOrderNumber = orderNumber,
                WorkforceProfileId = request.WorkforceProfileId,
                ActionCategory = request.ActionCategory,
                EffectiveDate = request.EffectiveDate.Date,
                PostingDate = DateTime.UtcNow,
                OrderStatus = "Draft",
                ReasonText = request.ReasonText,
                TargetOrganizationUnitId = request.TargetOrganizationUnitId,
                TargetDepartmentId = request.TargetDepartmentId,
                TargetPositionId = request.TargetPositionId,
                TargetManagerProfileId = request.TargetManagerProfileId,
                TargetBaseSalary = request.TargetBaseSalary,
                Notes = request.Notes,
                CreateBy = userId,
                CreateDateTime = DateTime.UtcNow
            };

            _dbContext.HrdPersonnelActionOrders.Add(order);
            await _dbContext.SaveChangesAsync(ct);

            return (await GetOrderByIdAsync(order.Id, ct))!;
        }

        public async Task<PersonnelActionOrderDetailResponse> UpdateDraftOrderAsync(
            Guid id,
            UpdatePersonnelActionOrderRequest request,
            Guid userId,
            CancellationToken ct = default)
        {
            var order = await _dbContext.HrdPersonnelActionOrders
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (order == null)
                throw new InvalidOperationException("Dokumen SK tidak ditemukan.");

            if (order.OrderStatus != "Draft")
                throw new InvalidOperationException($"SK berstatus '{order.OrderStatus}' tidak dapat diubah lagi.");

            await ValidatePlacementCombinationAsync(
                order.WorkforceProfileId,
                request.TargetOrganizationUnitId,
                request.TargetDepartmentId,
                request.TargetPositionId,
                request.TargetManagerProfileId,
                request.EffectiveDate,
                ct);

            order.ActionCategory = request.ActionCategory;
            order.EffectiveDate = request.EffectiveDate.Date;
            order.ReasonText = request.ReasonText;
            order.TargetOrganizationUnitId = request.TargetOrganizationUnitId;
            order.TargetDepartmentId = request.TargetDepartmentId;
            order.TargetPositionId = request.TargetPositionId;
            order.TargetManagerProfileId = request.TargetManagerProfileId;
            order.TargetBaseSalary = request.TargetBaseSalary;
            order.Notes = request.Notes;
            order.UpdateBy = userId;
            order.UpdateDateTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(ct);
            return (await GetOrderByIdAsync(order.Id, ct))!;
        }

        public async Task<AttachmentResponse> UploadAttachmentAsync(
            Guid id,
            string fileName,
            string originalFileName,
            string contentType,
            long fileSize,
            string storagePath,
            Guid userId,
            CancellationToken ct = default)
        {
            var order = await _dbContext.HrdPersonnelActionOrders
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (order == null)
                throw new InvalidOperationException("Dokumen SK tidak ditemukan.");

            var attachment = new HrdPersonnelActionOrderAttachment
            {
                PersonnelActionOrderId = id,
                FileName = fileName,
                OriginalFileName = originalFileName,
                ContentType = contentType,
                FileSize = fileSize,
                StoragePath = storagePath,
                UploadedByUserId = userId,
                UploadedAt = DateTime.UtcNow,
                CreateBy = userId,
                CreateDateTime = DateTime.UtcNow
            };

            _dbContext.HrdPersonnelActionOrderAttachments.Add(attachment);

            order.SignedDocumentPath = storagePath;
            order.SignedDocumentFileName = originalFileName;
            order.UpdateBy = userId;
            order.UpdateDateTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(ct);

            return new AttachmentResponse
            {
                AttachmentId = attachment.Id,
                FileName = originalFileName,
                StoragePath = storagePath,
                FileSize = fileSize,
                UploadedAt = attachment.UploadedAt
            };
        }

        public async Task<(string PhysicalPath, string FileName, string ContentType)?> GetAttachmentFileAsync(
            Guid id,
            Guid attachmentId,
            CancellationToken ct = default)
        {
            var attachment = await _dbContext.HrdPersonnelActionOrderAttachments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PersonnelActionOrderId == id && x.Id == attachmentId && !x.IsDelete, ct);

            if (attachment == null) return null;

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", attachment.StoragePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
            {
                if (File.Exists(attachment.StoragePath))
                {
                    fullPath = attachment.StoragePath;
                }
                else
                {
                    return null;
                }
            }

            return (fullPath, attachment.OriginalFileName, attachment.ContentType);
        }

        public async Task<PersonnelActionOrderDetailResponse> SubmitOrderAsync(
            Guid id,
            Guid userId,
            CancellationToken ct = default)
        {
            var order = await _dbContext.HrdPersonnelActionOrders
                .Include(x => x.Attachments)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (order == null)
                throw new InvalidOperationException("Dokumen SK tidak ditemukan.");

            if (order.OrderStatus != "Draft")
                throw new InvalidOperationException($"SK berstatus '{order.OrderStatus}' tidak dapat diajukan.");

            // Invariant VR-WFC-04 / HRD-DEC-056: Mandatory physical signed scan upload
            if (string.IsNullOrWhiteSpace(order.SignedDocumentPath) && !order.Attachments.Any(a => !a.IsDelete))
            {
                throw new InvalidOperationException("Pengajuan persetujuan SK ditolak: Berkas fisik hasil pindai (scan) bertanda tangan wajib diunggah terlebih dahulu (HRD-DEC-056).");
            }

            order.OrderStatus = "PendingApproval";
            order.SubmittedByUserId = userId;
            order.SubmittedAt = DateTime.UtcNow;
            order.UpdateBy = userId;
            order.UpdateDateTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(ct);
            return (await GetOrderByIdAsync(order.Id, ct))!;
        }

        public async Task<PersonnelActionOrderDetailResponse> ApproveOrderAsync(
            Guid id,
            Guid userId,
            CancellationToken ct = default)
        {
            var order = await _dbContext.HrdPersonnelActionOrders
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (order == null)
                throw new InvalidOperationException("Dokumen SK tidak ditemukan.");

            if (order.OrderStatus != "PendingApproval")
                throw new InvalidOperationException($"Hanya SK berstatus 'PendingApproval' yang dapat disetujui (Status saat ini: '{order.OrderStatus}').");

            // Invariant VR-WFC-05 / HRD-DEC-031: Anti self-approval
            if (order.SubmittedByUserId.HasValue && order.SubmittedByUserId.Value == userId)
            {
                throw new InvalidOperationException("Persetujuan SK ditolak: Petugas pembuat draf SK dilarang menyetujui dokumen yang dibuatnya sendiri (Segregation of Duties / HRD-DEC-031).");
            }

            // Invariant VR-WFC-03 / HRD-DEC-050: Retroactive mutation cannot fall in a closed attendance period
            if (order.EffectiveDate.Date < DateTime.UtcNow.Date)
            {
                var effectiveDateOnly = DateOnly.FromDateTime(order.EffectiveDate);
                var closedPeriod = await _dbContext.Set<HrdAttendancePeriod>()
                    .Where(p => !p.IsDelete && p.PeriodStatus == "Closed")
                    .Where(p => p.StartDate <= effectiveDateOnly && p.EndDate >= effectiveDateOnly)
                    .FirstOrDefaultAsync(ct);

                if (closedPeriod != null)
                {
                    throw new InvalidOperationException($"Permohonan mutasi retroaktif ditolak: Tanggal efektif ({order.EffectiveDate:yyyy-MM-dd}) jatuh pada periode absensi yang sudah ditutup ({closedPeriod.PeriodCode}) (HRD-DEC-050).");
                }
            }

            await ValidatePlacementCombinationAsync(
                order.WorkforceProfileId,
                order.TargetOrganizationUnitId,
                order.TargetDepartmentId,
                order.TargetPositionId,
                order.TargetManagerProfileId,
                order.EffectiveDate,
                ct);

            using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                order.ApprovedByUserId = userId;
                order.ApprovedAt = DateTime.UtcNow;
                order.UpdateBy = userId;
                order.UpdateDateTime = DateTime.UtcNow;

                var isImmediateApply = order.EffectiveDate.Date <= DateTime.UtcNow.Date;

                if (isImmediateApply)
                {
                    order.OrderStatus = "Applied";
                    order.ExecutedAt = DateTime.UtcNow;

                    await ApplyOrderAssignmentsAsync(order, userId, ct);
                }
                else
                {
                    order.OrderStatus = "Scheduled";
                }

                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }

            return (await GetOrderByIdAsync(order.Id, ct))!;
        }

        public async Task<PersonnelActionOrderDetailResponse> RejectOrderAsync(
            Guid id,
            string reason,
            Guid userId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Alasan penolakan SK wajib diisi.");

            var order = await _dbContext.HrdPersonnelActionOrders
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (order == null)
                throw new InvalidOperationException("Dokumen SK tidak ditemukan.");

            if (order.OrderStatus != "PendingApproval")
                throw new InvalidOperationException($"Hanya SK berstatus 'PendingApproval' yang dapat ditolak.");

            order.OrderStatus = "Rejected";
            order.RejectedByUserId = userId;
            order.RejectedAt = DateTime.UtcNow;
            order.RejectionReason = reason;
            order.UpdateBy = userId;
            order.UpdateDateTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(ct);
            return (await GetOrderByIdAsync(order.Id, ct))!;
        }

        public async Task<PersonnelActionOrderDetailResponse> CancelOrderAsync(
            Guid id,
            string reason,
            string? counterOrderNumber,
            Guid userId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Alasan pembatalan SK wajib diisi.");

            var order = await _dbContext.HrdPersonnelActionOrders
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (order == null)
                throw new InvalidOperationException("Dokumen SK tidak ditemukan.");

            // Invariant VR-WFC-12 / HRD-DEC-057:
            // Scheduled: can be cancelled before effective date
            // Applied: requires CounterOrderNumber
            if (order.OrderStatus == "Applied")
            {
                if (string.IsNullOrWhiteSpace(counterOrderNumber))
                {
                    throw new InvalidOperationException("SK yang sudah berstatus 'Applied' hanya dapat dibatalkan melalui penerbitan SK Pembatalan / Kontra resmi (Nomor SK Kontra wajib diisi) (HRD-DEC-057).");
                }
                order.CounterOrderNumber = counterOrderNumber;
            }
            else if (order.OrderStatus != "Scheduled" && order.OrderStatus != "PendingApproval" && order.OrderStatus != "Draft")
            {
                throw new InvalidOperationException($"SK berstatus '{order.OrderStatus}' tidak dapat dibatalkan.");
            }

            order.OrderStatus = "Cancelled";
            order.CancellationRequestedByUserId = userId;
            order.CancellationRequestedAt = DateTime.UtcNow;
            order.CancellationReason = reason;
            order.UpdateBy = userId;
            order.UpdateDateTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(ct);
            return (await GetOrderByIdAsync(order.Id, ct))!;
        }

        private async Task ApplyOrderAssignmentsAsync(HrdPersonnelActionOrder order, Guid userId, CancellationToken ct)
        {
            var profileId = order.WorkforceProfileId;
            var effectiveDate = order.EffectiveDate.Date;

            // 1. Organization Assignment
            if (order.TargetOrganizationUnitId.HasValue || order.TargetDepartmentId.HasValue)
            {
                var activeOrg = await _dbContext.WfpOrganizationAssignments
                    .Where(x => x.WorkforceProfileId == profileId && x.IsPrimary && x.IsActive && !x.IsDelete)
                    .FirstOrDefaultAsync(ct);

                if (activeOrg != null)
                {
                    activeOrg.EffectiveEndDate = effectiveDate.AddDays(-1);
                    activeOrg.IsActive = false;
                    activeOrg.UpdateBy = userId;
                    activeOrg.UpdateDateTime = DateTime.UtcNow;
                }

                var newOrg = new WfpOrganizationAssignment
                {
                    WorkforceProfileId = profileId,
                    OrganizationUnitId = order.TargetOrganizationUnitId ?? activeOrg?.OrganizationUnitId,
                    DepartmentId = order.TargetDepartmentId ?? activeOrg?.DepartmentId ?? Guid.Empty,
                    PositionId = order.TargetPositionId ?? activeOrg?.PositionId ?? Guid.Empty,
                    AssignmentType = "Primary",
                    IsPrimary = true,
                    EffectiveStartDate = effectiveDate,
                    PersonnelActionOrderId = order.Id,
                    ApprovedByUserId = userId,
                    ApprovedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreateBy = userId,
                    CreateDateTime = DateTime.UtcNow
                };
                _dbContext.WfpOrganizationAssignments.Add(newOrg);
            }

            // 2. Position Assignment
            if (order.TargetPositionId.HasValue)
            {
                var activePos = await _dbContext.WfpPositionAssignments
                    .Where(x => x.WorkforceProfileId == profileId && x.IsPrimary && x.IsActive && !x.IsDelete)
                    .FirstOrDefaultAsync(ct);

                if (activePos != null)
                {
                    activePos.EffectiveEndDate = effectiveDate.AddDays(-1);
                    activePos.IsActive = false;
                    activePos.UpdateBy = userId;
                    activePos.UpdateDateTime = DateTime.UtcNow;
                }

                var newPos = new WfpPositionAssignment
                {
                    WorkforceProfileId = profileId,
                    PositionId = order.TargetPositionId.Value,
                    OrganizationUnitId = order.TargetOrganizationUnitId ?? activePos?.OrganizationUnitId,
                    DepartmentId = order.TargetDepartmentId ?? activePos?.DepartmentId,
                    AssignmentType = "Substantive",
                    IsPrimary = true,
                    EffectiveStartDate = effectiveDate,
                    PersonnelActionOrderId = order.Id,
                    ApprovedByUserId = userId,
                    ApprovedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreateBy = userId,
                    CreateDateTime = DateTime.UtcNow
                };
                _dbContext.WfpPositionAssignments.Add(newPos);
            }

            // 3. Manager Assignment
            if (order.TargetManagerProfileId.HasValue)
            {
                var activeMgr = await _dbContext.WfpManagerAssignments
                    .Where(x => x.WorkforceProfileId == profileId && x.IsPrimaryManager && x.IsActive && !x.IsDelete)
                    .FirstOrDefaultAsync(ct);

                if (activeMgr != null)
                {
                    activeMgr.EffectiveEndDate = effectiveDate.AddDays(-1);
                    activeMgr.IsActive = false;
                    activeMgr.UpdateBy = userId;
                    activeMgr.UpdateDateTime = DateTime.UtcNow;
                }

                var newMgr = new WfpManagerAssignment
                {
                    WorkforceProfileId = profileId,
                    ManagerWorkforceProfileId = order.TargetManagerProfileId.Value,
                    ManagerType = "Direct",
                    IsPrimaryManager = true,
                    CanApproveRequests = true,
                    EffectiveStartDate = effectiveDate,
                    PersonnelActionOrderId = order.Id,
                    ApprovedByUserId = userId,
                    ApprovedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreateBy = userId,
                    CreateDateTime = DateTime.UtcNow
                };
                _dbContext.WfpManagerAssignments.Add(newMgr);
            }

            // 4. Base Salary Assignment
            if (order.TargetBaseSalary.HasValue)
            {
                var activeSalary = await _dbContext.WfpSalaryAssignments
                    .Where(x => x.WorkforceProfileId == profileId && x.IsPrimary && x.IsActive && !x.IsDelete)
                    .FirstOrDefaultAsync(ct);

                if (activeSalary != null)
                {
                    activeSalary.EffectiveEndDate = effectiveDate.AddDays(-1);
                    activeSalary.IsActive = false;
                    activeSalary.UpdateBy = userId;
                    activeSalary.UpdateDateTime = DateTime.UtcNow;
                }

                var newSalary = new WfpSalaryAssignment
                {
                    WorkforceProfileId = profileId,
                    BaseSalary = order.TargetBaseSalary.Value,
                    SalaryStructureId = activeSalary?.SalaryStructureId ?? Guid.Empty,
                    SalaryGradeId = activeSalary?.SalaryGradeId ?? Guid.Empty,
                    EffectiveStartDate = effectiveDate,
                    IsPrimary = true,
                    IsActive = true,
                    PersonnelActionOrderId = order.Id,
                    ApprovedByUserId = userId,
                    ApprovedAt = DateTime.UtcNow,
                    CreateBy = userId,
                    CreateDateTime = DateTime.UtcNow
                };
                _dbContext.WfpSalaryAssignments.Add(newSalary);
            }
        }

        private async Task<string> GenerateOrderNumberAsync(CancellationToken ct)
        {
            var dateCode = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var prefix = $"{OrderNumberPrefix}{dateCode}-";

            var existingNumbers = await _dbContext.HrdPersonnelActionOrders
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.ActionOrderNumber.StartsWith(prefix))
                .Select(x => x.ActionOrderNumber)
                .ToListAsync(ct);

            var maxNumber = existingNumbers
                .Select(x => x[prefix.Length..])
                .Select(x => int.TryParse(x, out var value) ? value : 0)
                .DefaultIfEmpty(0)
                .Max();

            return $"{prefix}{(maxNumber + 1):D4}";
        }

        private async Task ValidatePlacementCombinationAsync(
            Guid workforceProfileId,
            Guid? targetOrgUnitId,
            Guid? targetDeptId,
            Guid? targetPositionId,
            Guid? targetManagerProfileId,
            DateTime effectiveDate,
            CancellationToken ct)
        {
            if (workforceProfileId == Guid.Empty)
                throw new ArgumentException("Pegawai target wajib dipilih.");

            var employee = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete, ct);
            if (employee == null)
                throw new InvalidOperationException("Data pegawai target tidak ditemukan.");

            // 1. Manager validation: anti self-management & active check
            if (targetManagerProfileId.HasValue && targetManagerProfileId.Value != Guid.Empty)
            {
                if (targetManagerProfileId.Value == workforceProfileId)
                    throw new InvalidOperationException("Pegawai tidak dapat menjadi atasan bagi dirinya sendiri.");

                var manager = await _dbContext.MstWorkforceProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == targetManagerProfileId.Value && !x.IsDelete, ct);
                if (manager == null || !manager.IsActive)
                    throw new InvalidOperationException("Atasan langsung yang dipilih tidak valid atau tidak aktif.");
            }

            // 2. Organization Unit validation
            MstOrganizationUnit? orgUnit = null;
            if (targetOrgUnitId.HasValue && targetOrgUnitId.Value != Guid.Empty)
            {
                orgUnit = await _dbContext.MstOrganizationUnits
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == targetOrgUnitId.Value && !x.IsDelete, ct);
                if (orgUnit == null || !orgUnit.IsActive)
                    throw new InvalidOperationException("Unit organisasi tujuan tidak ditemukan atau tidak aktif.");
            }

            // 3. Department validation
            MstDepartment? dept = null;
            if (targetDeptId.HasValue && targetDeptId.Value != Guid.Empty)
            {
                dept = await _dbContext.MstDepartments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == targetDeptId.Value && !x.IsDelete, ct);
                if (dept == null || !dept.IsActive)
                    throw new InvalidOperationException("Departemen tujuan tidak ditemukan atau tidak aktif.");
            }

            // 4. Cross validation: Organization Unit vs Department
            if (orgUnit != null && dept != null)
            {
                if (orgUnit.DepartmentId.HasValue && orgUnit.DepartmentId.Value != Guid.Empty && orgUnit.DepartmentId.Value != dept.Id)
                {
                    throw new InvalidOperationException($"Unit organisasi '{orgUnit.UnitName}' terikat dengan departemen lain dan tidak sesuai dengan departemen tujuan '{dept.DepartmentName}'.");
                }
            }

            // 5. Position validation
            if (targetPositionId.HasValue && targetPositionId.Value != Guid.Empty)
            {
                var position = await _dbContext.MstPositions
                    .Include(x => x.Department)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == targetPositionId.Value && !x.IsDelete, ct);
                if (position == null || !position.IsActive)
                    throw new InvalidOperationException("Jabatan tujuan tidak ditemukan atau tidak aktif.");

                if (targetDeptId.HasValue && targetDeptId.Value != Guid.Empty)
                {
                    if (position.DepartmentId != targetDeptId.Value)
                    {
                        var posDeptName = position.Department?.DepartmentName ?? "departemen lain";
                        var targetDeptName = dept?.DepartmentName ?? "departemen tujuan";
                        throw new InvalidOperationException($"Jabatan yang dipilih ({position.PositionName}) berada di bawah {posDeptName}, tidak sesuai dengan {targetDeptName}.");
                    }
                }
                else if (orgUnit != null && orgUnit.DepartmentId.HasValue && orgUnit.DepartmentId.Value != Guid.Empty)
                {
                    if (position.DepartmentId != orgUnit.DepartmentId.Value)
                    {
                        var posDeptName = position.Department?.DepartmentName ?? "departemen lain";
                        throw new InvalidOperationException($"Jabatan yang dipilih ({position.PositionName}) tidak sesuai dengan departemen dari unit organisasi tujuan ({orgUnit.UnitName}).");
                    }
                }
            }

            // 6. Retroactive closed attendance period validation
            if (effectiveDate.Date < DateTime.UtcNow.Date)
            {
                var effectiveDateOnly = DateOnly.FromDateTime(effectiveDate);
                var closedPeriod = await _dbContext.Set<HrdAttendancePeriod>()
                    .Where(p => !p.IsDelete && p.PeriodStatus == "Closed")
                    .Where(p => p.StartDate <= effectiveDateOnly && p.EndDate >= effectiveDateOnly)
                    .FirstOrDefaultAsync(ct);

                if (closedPeriod != null)
                {
                    throw new InvalidOperationException($"Tanggal efektif ({effectiveDate:yyyy-MM-dd}) jatuh pada periode absensi yang sudah ditutup ({closedPeriod.PeriodCode}) (HRD-DEC-050).");
                }
            }
        }

        private static PersonnelActionOrderDetailResponse MapToDetailResponse(HrdPersonnelActionOrder order)
        {
            return new PersonnelActionOrderDetailResponse
            {
                Id = order.Id,
                ActionOrderNumber = order.ActionOrderNumber,
                WorkforceProfileId = order.WorkforceProfileId,
                EmployeeName = order.WorkforceProfile != null ? order.WorkforceProfile.DisplayName : string.Empty,
                ProfileCode = order.WorkforceProfile != null ? order.WorkforceProfile.ProfileCode : string.Empty,
                ActionCategory = order.ActionCategory,
                EffectiveDate = order.EffectiveDate,
                PostingDate = order.PostingDate,
                OrderStatus = order.OrderStatus,
                ReasonText = order.ReasonText,
                SignedDocumentPath = order.SignedDocumentPath,
                SignedDocumentFileName = order.SignedDocumentFileName,
                TargetOrganizationUnitId = order.TargetOrganizationUnitId,
                TargetOrganizationUnitName = order.TargetOrganizationUnit != null ? order.TargetOrganizationUnit.UnitName : null,
                TargetDepartmentId = order.TargetDepartmentId,
                TargetDepartmentName = order.TargetDepartment != null ? order.TargetDepartment.DepartmentName : null,
                TargetPositionId = order.TargetPositionId,
                TargetPositionName = order.TargetPosition != null ? order.TargetPosition.PositionName : null,
                TargetManagerProfileId = order.TargetManagerProfileId,
                TargetManagerName = order.TargetManagerProfile != null ? order.TargetManagerProfile.DisplayName : null,
                LinkedSalaryAssignmentId = order.LinkedSalaryAssignmentId,
                TargetBaseSalary = order.TargetBaseSalary,
                WorkflowDefinitionId = order.WorkflowDefinitionId,
                WorkflowInstanceId = order.WorkflowInstanceId,
                SubmittedByUserId = order.SubmittedByUserId,
                SubmittedAt = order.SubmittedAt,
                ApprovedByUserId = order.ApprovedByUserId,
                ApprovedAt = order.ApprovedAt,
                RejectedByUserId = order.RejectedByUserId,
                RejectedAt = order.RejectedAt,
                RejectionReason = order.RejectionReason,
                CancellationRequestedByUserId = order.CancellationRequestedByUserId,
                CancellationRequestedAt = order.CancellationRequestedAt,
                CancellationReason = order.CancellationReason,
                CounterOrderNumber = order.CounterOrderNumber,
                ExecutedAt = order.ExecutedAt,
                Notes = order.Notes,
                IsActive = order.IsActive,
                Attachments = order.Attachments.Select(a => new PersonnelActionOrderAttachmentDto
                {
                    Id = a.Id,
                    PersonnelActionOrderId = a.PersonnelActionOrderId,
                    FileName = a.FileName,
                    OriginalFileName = a.OriginalFileName,
                    ContentType = a.ContentType,
                    FileSize = a.FileSize,
                    StoragePath = a.StoragePath,
                    UploadedAt = a.UploadedAt,
                    UploadedByUserId = a.UploadedByUserId
                }).ToList()
            };
        }
    }
}
