using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
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
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private const string OrderNumberPrefix = "SK-RSMMC-";
        private const string OrderNumberSeriesPrefix = "SK-RSMMC";
        private const string OrderNumberSequenceKey = "HRD_PERSONNEL_ACTION_ORDER";
        private const int OrderNumberDigits = 4;

        /// <summary>
        /// BE-HRD-414 / HRD-DEC-036: perubahan penetapan atasan adalah jenis transaksi tersendiri, bukan
        /// Mutasi atau Rotasi. Alur persetujuannya memakai pola MVP yang sama (pembuat ≠ penyetuju, scan wajib).
        /// </summary>
        public const string ManagerChangeCategory = "ManagerChange";

        private static void ValidateManagerChangeShape(
            string? actionCategory,
            Guid? targetOrganizationUnitId,
            Guid? targetDepartmentId,
            Guid? targetPositionId,
            Guid? targetManagerProfileId)
        {
            if (!string.Equals(actionCategory, ManagerChangeCategory, StringComparison.Ordinal))
                return;

            if (!targetManagerProfileId.HasValue || targetManagerProfileId.Value == Guid.Empty)
                throw new ArgumentException("Perubahan atasan wajib memilih atasan langsung baru.");

            if (targetOrganizationUnitId.HasValue || targetDepartmentId.HasValue || targetPositionId.HasValue)
                throw new ArgumentException("Perubahan atasan hanya mengubah atasan langsung. Perubahan unit atau jabatan diajukan sebagai Mutasi/Rotasi/Promosi.");
        }

        public HrdPersonnelActionOrderService(ApplicationDbContext dbContext, NumberSeriesAllocator numberSeriesAllocator)
        {
            _dbContext = dbContext;
            _numberSeriesAllocator = numberSeriesAllocator;
        }

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
                ActionCategories = new List<string> { "Mutation", "Promotion", "Rotation", "Demotion", "TemporaryAssignment", ManagerChangeCategory },
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

            ValidateManagerChangeShape(request.ActionCategory, request.TargetOrganizationUnitId, request.TargetDepartmentId, request.TargetPositionId, request.TargetManagerProfileId);

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

            HrdPersonnelActionOrder? order = null;
            for (int attempt = 0; attempt < 3; attempt++)
            {
                var orderNumber = await GenerateOrderNumberAsync(userId, ct);
                order = new HrdPersonnelActionOrder
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
                    TargetBaseSalary = null, // Protected: gaji wajib melalui workflow payroll tersendiri (HRD-DEC-036/054)
                    Notes = request.Notes,
                    CreateBy = userId,
                    CreateDateTime = DateTime.UtcNow
                };

                _dbContext.HrdPersonnelActionOrders.Add(order);
                try
                {
                    await _dbContext.SaveChangesAsync(ct);
                    break;
                }
                catch (DbUpdateException) when (attempt < 2)
                {
                    _dbContext.Entry(order).State = EntityState.Detached;
                    await Task.Delay(50 * (attempt + 1), ct);
                }
            }

            if (order == null || order.Id == Guid.Empty)
            {
                throw new InvalidOperationException("Gagal menyimpan dokumen SK kepegawaian.");
            }

            return (await GetOrderByIdAsync(order.Id, ct))!;
        }

        public async Task<PersonnelActionOrderDetailResponse> UpdateDraftOrderAsync(
            Guid id,
            UpdatePersonnelActionOrderRequest request,
            Guid userId,
            CancellationToken ct = default)
        {
            ValidateManagerChangeShape(request.ActionCategory, request.TargetOrganizationUnitId, request.TargetDepartmentId, request.TargetPositionId, request.TargetManagerProfileId);

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
            order.TargetBaseSalary = null; // Protected: gaji tidak dimutasi via SK langsung (HRD-DEC-036/054)
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
            if (order.CreateBy == userId || (order.SubmittedByUserId.HasValue && order.SubmittedByUserId.Value == userId))
            {
                throw new InvalidOperationException("Persetujuan SK ditolak: Petugas pembuat draf SK maupun pengaju dilarang menyetujui dokumen yang dibuat atau diajukannya sendiri (Segregation of Duties / HRD-DEC-031).");
            }

            // Invariant VR-WFC-04 / HRD-DEC-056: Mandatory physical signed scan upload verification
            if (string.IsNullOrWhiteSpace(order.SignedDocumentPath) && !await _dbContext.HrdPersonnelActionOrderAttachments.AnyAsync(a => a.PersonnelActionOrderId == order.Id && !a.IsDelete, ct))
            {
                throw new InvalidOperationException("Persetujuan SK ditolak: Berkas fisik hasil pindai (scan) bertanda tangan wajib dilampirkan terlebih dahulu (HRD-DEC-056).");
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
                // BE-HRD-407: dua persetujuan paralel berlaku sekali. Persetujuan kedua menunggu kunci per SK,
                // membaca ulang status sesudah yang pertama commit, lalu ditolak.
                await AcquireTransactionLockAsync($"HRD_PAO_{order.Id}", ct);
                var currentStatus = await _dbContext.HrdPersonnelActionOrders
                    .AsNoTracking()
                    .Where(x => x.Id == order.Id)
                    .Select(x => x.OrderStatus)
                    .FirstOrDefaultAsync(ct);
                if (currentStatus != "PendingApproval")
                {
                    throw new InvalidOperationException($"SK sudah diproses oleh petugas lain (status saat ini: '{currentStatus}'). Muat ulang data.");
                }

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
            // SK yang sudah berstatus 'Applied' TIDAK DAPAT dibatalkan langsung.
            // Untuk membalik kondisi, wajib menerbitkan peristiwa/SK baru.
            if (order.OrderStatus == "Applied")
            {
                throw new InvalidOperationException("SK yang sudah berstatus 'Applied' tidak dapat dibatalkan secara langsung. Untuk membalik kondisi atau membatalkan penempatan yang sudah efektif, wajib menerbitkan peristiwa/SK baru (Counter-Event) dengan referensi ke SK semula (HRD-DEC-057).");
            }
            else if (order.OrderStatus == "Scheduled")
            {
                if (order.EffectiveDate.Date <= GetHospitalBusinessDate())
                {
                    throw new InvalidOperationException("SK yang telah memasuki atau melewati tanggal efektif tidak dapat dibatalkan (HRD-DEC-057).");
                }

                // Invariant AUD-HRD07-05 / HRD-DEC-057: Wajib memiliki permohonan pembatalan terdahulu
                if (!order.CancellationRequestedByUserId.HasValue || order.CancellationRequestedAt == null)
                {
                    throw new InvalidOperationException("Pembatalan SK berstatus 'Scheduled' tidak dapat dilakukan secara langsung; wajib diajukan terlebih dahulu melalui permohonan pembatalan resmi (Cancel Request) sebelum disetujui (AUD-HRD07-05 / HRD-DEC-057).");
                }

                // Segregation of duties for cancellation (pemohon dilarang menyetujui sendiri)
                if (order.CancellationRequestedByUserId.Value == userId)
                {
                    throw new InvalidOperationException("Persetujuan pembatalan SK ditolak: Petugas yang mengajukan permohonan pembatalan dilarang menyetujui pembatalan sendiri (Segregation of Duties / HRD-DEC-057).");
                }
            }
            else if (order.OrderStatus != "PendingApproval" && order.OrderStatus != "Draft")
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

        public async Task<PersonnelActionOrderDetailResponse> RequestCancelOrderAsync(
            Guid id,
            string reason,
            Guid userId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Alasan permohonan pembatalan SK wajib diisi.");

            var order = await _dbContext.HrdPersonnelActionOrders
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (order == null)
                throw new InvalidOperationException("Dokumen SK tidak ditemukan.");

            if (order.OrderStatus == "Applied")
            {
                throw new InvalidOperationException("SK yang sudah berstatus 'Applied' tidak dapat dibatalkan secara langsung. Terbitkan peristiwa/SK baru (HRD-DEC-057).");
            }

            if (order.OrderStatus == "Draft" || order.OrderStatus == "PendingApproval")
            {
                order.OrderStatus = "Cancelled";
                order.CancellationRequestedByUserId = userId;
                order.CancellationRequestedAt = DateTime.UtcNow;
                order.CancellationReason = reason;
                order.UpdateBy = userId;
                order.UpdateDateTime = DateTime.UtcNow;
            }
            else if (order.OrderStatus == "Scheduled")
            {
                if (order.EffectiveDate.Date <= GetHospitalBusinessDate())
                {
                    throw new InvalidOperationException("SK yang telah memasuki atau melewati tanggal efektif tidak dapat diajukan pembatalan (HRD-DEC-057).");
                }

                order.CancellationRequestedByUserId = userId;
                order.CancellationRequestedAt = DateTime.UtcNow;
                order.CancellationReason = reason;
                order.UpdateBy = userId;
                order.UpdateDateTime = DateTime.UtcNow;
            }
            else
            {
                throw new InvalidOperationException($"SK berstatus '{order.OrderStatus}' tidak dapat diajukan pembatalan.");
            }

            await _dbContext.SaveChangesAsync(ct);
            return (await GetOrderByIdAsync(order.Id, ct))!;
        }

        private async Task ApplyOrderAssignmentsAsync(HrdPersonnelActionOrder order, Guid userId, CancellationToken ct)
        {
            var profileId = order.WorkforceProfileId;
            var effectiveDate = order.EffectiveDate.Date;

            var activeOrg = await _dbContext.WfpOrganizationAssignments
                .Where(x => x.WorkforceProfileId == profileId && x.IsPrimary && x.IsActive && !x.IsDelete)
                .FirstOrDefaultAsync(ct);

            var activePos = await _dbContext.WfpPositionAssignments
                .Where(x => x.WorkforceProfileId == profileId && x.IsPrimary && x.IsActive && !x.IsDelete)
                .FirstOrDefaultAsync(ct);

            // 1. Organization Assignment
            if (order.TargetOrganizationUnitId.HasValue || order.TargetDepartmentId.HasValue)
            {
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

            // 4. Base Salary Assignment - STOPPED per HRD-DEC-036 & HRD-DEC-054
            // Perubahan gaji wajib melalui alur persetujuan gaji terpisah (Payroll Workflow).
            // SK Kepegawaian tidak boleh memutasi nominal kompensasi secara langsung.
            // Referensi ke penetapan gaji yang sah disimpan pada LinkedSalaryAssignmentId jika ada.

            // 5. Employment History (Pencatatan riwayat transisi dari SK - ISSUE 06)
            var existingHistory = await _dbContext.WfpEmploymentHistories
                .FirstOrDefaultAsync(x => x.ReferenceType == "PersonnelActionOrder" && x.ReferenceId == order.Id && !x.IsDelete, ct);

            if (existingHistory == null)
            {
                var history = new WfpEmploymentHistory
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = profileId,
                    HistoryType = order.ActionCategory ?? "StatusChange",
                    OldPositionId = activePos?.PositionId,
                    NewPositionId = order.TargetPositionId ?? activePos?.PositionId,
                    OldDepartmentId = activeOrg?.DepartmentId,
                    NewDepartmentId = order.TargetDepartmentId ?? activeOrg?.DepartmentId,
                    OldOrganizationUnitId = activeOrg?.OrganizationUnitId,
                    NewOrganizationUnitId = order.TargetOrganizationUnitId ?? activeOrg?.OrganizationUnitId,
                    EffectiveDate = effectiveDate,
                    Reason = order.ReasonText,
                    ReferenceType = "PersonnelActionOrder",
                    ReferenceId = order.Id,
                    ApprovedByUserId = order.ApprovedByUserId ?? userId,
                    ApprovedAt = order.ApprovedAt ?? DateTime.UtcNow,
                    IsActive = true,
                    CreateBy = userId,
                    CreateDateTime = DateTime.UtcNow
                };
                _dbContext.WfpEmploymentHistories.Add(history);
            }

            // 6. Sinkronisasi mirror pada MstWorkforceProfile saat penempatan berlaku (HRD-DEC-047)
            var profile = await _dbContext.MstWorkforceProfiles
                .FirstOrDefaultAsync(x => x.Id == profileId && !x.IsDelete, ct);
            if (profile != null)
            {
                if (order.TargetPositionId.HasValue)
                {
                    profile.PrimaryPositionId = order.TargetPositionId.Value;
                }
                if (order.TargetDepartmentId.HasValue)
                {
                    profile.PrimaryDepartmentId = order.TargetDepartmentId.Value;
                }
                else if (order.TargetOrganizationUnitId.HasValue)
                {
                    var targetUnit = await _dbContext.MstOrganizationUnits
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == order.TargetOrganizationUnitId.Value && !u.IsDelete, ct);
                    if (targetUnit?.DepartmentId != null)
                    {
                        profile.PrimaryDepartmentId = targetUnit.DepartmentId;
                    }
                }
                profile.UpdateDateTime = DateTime.UtcNow;
                profile.UpdateBy = userId;
            }
        }

        private static DateTime GetHospitalBusinessDate()
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
            }
            catch
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
                }
                catch
                {
                    return DateTime.UtcNow.AddHours(7).Date;
                }
            }
        }

        public async Task<int> ProcessScheduledOrdersAsync(CancellationToken ct = default)
        {
            var businessToday = GetHospitalBusinessDate();
            // AUD-HRD07-06 & AUD-HRD07-11: Saring SK 'Scheduled' per tanggal bisnis RS dan kecualikan yang pending cancellation
            var scheduledOrders = await _dbContext.HrdPersonnelActionOrders
                .Where(x => x.OrderStatus == "Scheduled" && x.EffectiveDate <= businessToday && !x.IsDelete)
                .Where(x => x.CancellationRequestedByUserId == null)
                .OrderBy(x => x.EffectiveDate)
                .ToListAsync(ct);

            int appliedCount = 0;
            foreach (var order in scheduledOrders)
            {
                // Invariant VR-WFC-03 / HRD-DEC-050: attendance period must still be open
                var effectiveDateOnly = DateOnly.FromDateTime(order.EffectiveDate);
                var closedPeriod = await _dbContext.Set<HrdAttendancePeriod>()
                    .Where(p => !p.IsDelete && p.PeriodStatus == "Closed")
                    .Where(p => p.StartDate <= effectiveDateOnly && p.EndDate >= effectiveDateOnly)
                    .FirstOrDefaultAsync(ct);

                if (closedPeriod != null)
                {
                    // Periode kehadiran telah ditutup, tunda penerapan dan catat catatan
                    order.Notes = $"[AUTO-APPLY DEFERRED] Periode absensi {closedPeriod.PeriodCode} telah ditutup saat jatuh tempo tanggal efektif.";
                    order.UpdateDateTime = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync(ct);
                    continue;
                }

                using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
                try
                {
                    // BE-HRD-411: dua tick atau dua instance worker tidak menerapkan SK yang sama dua kali, dan
                    // permohonan pembatalan yang masuk sesudah daftar dibaca tetap dihormati.
                    await AcquireTransactionLockAsync($"HRD_PAO_{order.Id}", ct);
                    var live = await _dbContext.HrdPersonnelActionOrders
                        .AsNoTracking()
                        .Where(x => x.Id == order.Id)
                        .Select(x => new { x.OrderStatus, x.CancellationRequestedByUserId })
                        .FirstOrDefaultAsync(ct);
                    if (live == null || live.OrderStatus != "Scheduled" || live.CancellationRequestedByUserId.HasValue)
                    {
                        await transaction.RollbackAsync(ct);
                        continue;
                    }

                    order.OrderStatus = "Applied";
                    order.ExecutedAt = DateTime.UtcNow;
                    order.UpdateDateTime = DateTime.UtcNow;

                    await ApplyOrderAssignmentsAsync(order, order.ApprovedByUserId ?? Guid.Empty, ct);
                    await _dbContext.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);
                    appliedCount++;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(ct);
                    // AUD-HRD07-16: Logging observabilitas kegagalan per SK tanpa menggagalkan SK lain
                    order.Notes = $"[AUTO-APPLY FAILED] Gagal menerapkan SK: {ex.Message}";
                    order.UpdateDateTime = DateTime.UtcNow;
                    try { await _dbContext.SaveChangesAsync(ct); } catch { /* ignore secondary log save failure */ }
                }
            }

            return appliedCount;
        }

        /// <summary>
        /// BE-HRD-409: nomor SK <c>SK-RSMMC-YYYYMMDD-NNNN</c> dari number series bersama yang atomik di
        /// database (aman lintas instance), bukan baca-nomor-tertinggi + lock lokal. Nomor tertinggi hari ini
        /// yang sudah terbit dipakai sebagai batas bawah agar tidak bentrok dengan nomor lama.
        /// </summary>
        private async Task<string> GenerateOrderNumberAsync(Guid actorUserId, CancellationToken ct)
        {
            var dateCode = GetHospitalBusinessDate().ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var prefix = $"{OrderNumberPrefix}{dateCode}-";

            var todayNumbers = await _dbContext.HrdPersonnelActionOrders
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.ActionOrderNumber.StartsWith(prefix))
                .Select(x => x.ActionOrderNumber)
                .ToListAsync(ct);

            var legacyHighWatermark = todayNumbers
                .Select(n => long.TryParse(n[prefix.Length..], out var parsed) ? parsed : 0L)
                .DefaultIfEmpty(0L)
                .Max();

            return await _numberSeriesAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: OrderNumberSequenceKey,
                    Prefix: OrderNumberSeriesPrefix,
                    ResetPolicy: NumberSeriesResetPolicies.Daily,
                    SequenceDigits: OrderNumberDigits,
                    ActorUserId: actorUserId,
                    Instant: DateTimeOffset.UtcNow,
                    MinimumValue: legacyHighWatermark),
                ct);
        }

        private async Task<bool> WouldCreateCycleAsync(Guid workforceProfileId, Guid managerWorkforceProfileId, DateTime activeDate, CancellationToken ct)
        {
            var current = managerWorkforceProfileId;
            var visited = new HashSet<Guid>();
            for (var depth = 0; depth < 100; depth++)
            {
                if (current == workforceProfileId) return true;
                if (!visited.Add(current)) return true;
                var next = await _dbContext.Set<WfpManagerAssignment>().AsNoTracking()
                    .Where(x => x.WorkforceProfileId == current && x.IsPrimaryManager && x.IsActive && !x.IsDelete &&
                                x.EffectiveStartDate <= activeDate && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= activeDate))
                    .Select(x => (Guid?)x.ManagerWorkforceProfileId).FirstOrDefaultAsync(ct);
                if (!next.HasValue || next.Value == Guid.Empty) return false;
                current = next.Value;
            }
            return true;
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

                // AUD-HRD07-08: Deteksi siklus hierarki komando melingkar (Cycle detection)
                if (await WouldCreateCycleAsync(workforceProfileId, targetManagerProfileId.Value, effectiveDate.Date, ct))
                {
                    throw new InvalidOperationException("Penetapan atasan langsung ditolak: Relasi atasan akan membentuk siklus hierarki komando melingkar (Circular Cycle / AUD-HRD07-08).");
                }
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
                TargetBaseSalary = null, // Confidential: disembunyikan untuk menjaga kerahasiaan kompensasi (HRD-DEC-033/054)
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
