using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services
{
    public interface IBusinessTravelRequestService
    {
        Task<(List<BusinessTravelResponseDto> Items, int TotalCount)> GetRequestsAsync(
            Guid? workforceProfileId, string? status, DateOnly? startDate, DateOnly? endDate, Guid? departmentId, int pageNumber, int pageSize);
        Task<BusinessTravelDetailResponseDto> GetRequestByIdAsync(Guid id);
        Task<BusinessTravelDetailResponseDto> CreateRequestAsync(CreateBusinessTravelRequestDto dto, Guid currentUserId, Guid currentWorkforceProfileId);
        Task<BusinessTravelResponseDto> UpdateRequestAsync(Guid id, UpdateBusinessTravelRequestDto dto, Guid currentUserId);
        Task<BusinessTravelDetailResponseDto> SubmitRequestAsync(Guid id, Guid currentUserId, Guid currentWorkforceProfileId, CancellationToken cancellationToken = default);
        Task<BusinessTravelResponseDto> CancelRequestAsync(Guid id, CancelTravelRequestDto dto, Guid currentUserId, CancellationToken cancellationToken = default);
    }

    public class BusinessTravelRequestService : IBusinessTravelRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITravelBudgetAndConflictValidationService _conflictValidationService;
        private readonly IMstTravelAllowanceRateService _rateService;
        private readonly WorkflowService _workflowService;
        private readonly ITravelAttendanceAndRosterLockService _lockService;

        public BusinessTravelRequestService(
            ApplicationDbContext context,
            ITravelBudgetAndConflictValidationService conflictValidationService,
            IMstTravelAllowanceRateService rateService,
            WorkflowService workflowService,
            ITravelAttendanceAndRosterLockService lockService)
        {
            _context = context;
            _conflictValidationService = conflictValidationService;
            _rateService = rateService;
            _workflowService = workflowService;
            _lockService = lockService;
        }

        // VR-TRV-26/27 (kontrak v28): Draft, NeedRevision, atau SPPD lama "Submitted" yang belum punya workflow (HRD-DEC-227).
        private static bool IsOpenForApplicant(TrxBusinessTravelRequest r) =>
            r.TravelStatus == BusinessTravelRequestStatuses.Draft ||
            r.TravelStatus == BusinessTravelRequestStatuses.NeedRevision ||
            (r.TravelStatus == BusinessTravelRequestStatuses.Submitted && r.WorkflowInstanceId == null);

        private static bool IsApproved(TrxBusinessTravelRequest r) =>
            r.ApprovedAt.HasValue &&
            r.TravelStatus != BusinessTravelRequestStatuses.Cancelled &&
            r.TravelStatus != BusinessTravelRequestStatuses.Rejected;

        private static int CalculateInclusiveDays(DateOnly start, DateOnly end)
        {
            return end.DayNumber - start.DayNumber + 1;
        }

        public async Task<(List<BusinessTravelResponseDto> Items, int TotalCount)> GetRequestsAsync(
            Guid? workforceProfileId, string? status, DateOnly? startDate, DateOnly? endDate, Guid? departmentId, int pageNumber, int pageSize)
        {
            var query = _context.TrxBusinessTravelRequests
                .Include(r => r.WorkforceProfile)
                .Where(r => r.IsActive);

            if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty)
            {
                query = query.Where(r => r.WorkforceProfileId == workforceProfileId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.TravelStatus.ToLower() == status.ToLower());
            }

            if (startDate.HasValue)
            {
                query = query.Where(r => r.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.EndDate <= endDate.Value);
            }

            if (departmentId.HasValue && departmentId.Value != Guid.Empty)
            {
                query = query.Where(r => r.DepartmentId == departmentId.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new BusinessTravelResponseDto
                {
                    Id = r.Id,
                    TravelRequestNumber = r.TravelRequestNumber,
                    WorkforceProfileId = r.WorkforceProfileId,
                    EmployeeName = r.WorkforceProfile != null ? r.WorkforceProfile.DisplayName : "Pegawai Rumah Sakit",
                    TravelTitle = r.TravelTitle,
                    Origin = r.Origin,
                    Destination = r.Destination,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    TravelDayCount = r.TravelDayCount,
                    EstimatedTotalAmount = r.EstimatedTotalAmount,
                    ApprovedBudgetAmount = r.ApprovedBudgetAmount,
                    TravelStatus = r.TravelStatus,
                    CurrencyCode = r.CurrencyCode,
                    CreatedAt = r.CreateDateTime
                })
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<BusinessTravelDetailResponseDto> GetRequestByIdAsync(Guid id)
        {
            var r = await _context.TrxBusinessTravelRequests
                .Include(x => x.WorkforceProfile)
                .Include(x => x.CostCenter)
                .Include(x => x.DestinationZone)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (r == null) throw new KeyNotFoundException("Permohonan perjalanan dinas tidak ditemukan");

            return new BusinessTravelDetailResponseDto
            {
                Id = r.Id,
                TravelRequestNumber = r.TravelRequestNumber,
                WorkforceProfileId = r.WorkforceProfileId,
                EmployeeName = r.WorkforceProfile?.DisplayName ?? "Pegawai Rumah Sakit",
                TravelTitle = r.TravelTitle,
                TravelPurpose = r.TravelPurpose,
                ActivityDescription = r.ActivityDescription,
                Origin = r.Origin,
                Destination = r.Destination,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                TravelDayCount = r.TravelDayCount,
                EstimatedTransportationAmount = r.EstimatedTransportationAmount,
                EstimatedAccommodationAmount = r.EstimatedAccommodationAmount,
                EstimatedAllowanceAmount = r.EstimatedAllowanceAmount,
                EstimatedOtherAmount = r.EstimatedOtherAmount,
                EstimatedTotalAmount = r.EstimatedTotalAmount,
                ApprovedBudgetAmount = r.ApprovedBudgetAmount,
                TravelStatus = r.TravelStatus,
                CurrencyCode = r.CurrencyCode,
                IsDomestic = r.IsDomestic,
                IsInternational = r.IsInternational,
                IsUrgent = r.IsUrgent,
                CostCenterId = r.CostCenterId,
                CostCenterName = r.CostCenter?.CostCenterName,
                DestinationZoneId = r.DestinationZoneId,
                DestinationZoneName = r.DestinationZone?.DestinationZoneName,
                DocumentPdfUrl = $"/api/v1/hrd/travel/requests/{r.Id}/document-pdf",
                ApprovalNotes = r.ApprovalNotes,
                CancellationReason = r.CancellationReason,
                CreatedAt = r.CreateDateTime,
                WorkflowInstanceId = r.WorkflowInstanceId,
                CurrentApprovalStep = r.CurrentApprovalStep,
                SubmittedAt = r.SubmittedAt,
                ApprovedAt = r.ApprovedAt,
                RejectedAt = r.RejectedAt,
                IsApproved = IsApproved(r),
                CanSubmit = IsOpenForApplicant(r)
            };
        }

        public async Task<BusinessTravelDetailResponseDto> CreateRequestAsync(CreateBusinessTravelRequestDto dto, Guid currentUserId, Guid currentWorkforceProfileId)
        {
            var inclusiveDays = CalculateInclusiveDays(dto.StartDate, dto.EndDate);
            if (inclusiveDays <= 0)
            {
                throw new InvalidOperationException("Durasi hari perjalanan dinas minimal 1 hari (VR-TRV-19)");
            }

            // Hitung uang harian otomatis bila belum diisi (TRV-B3, VR-TRV-04)
            var allowanceAmount = dto.EstimatedAllowanceAmount;
            if (allowanceAmount <= 0)
            {
                allowanceAmount = await _rateService.CalculateTotalAllowanceAsync(dto.DestinationZoneId, null, inclusiveDays);
            }

            var totalEstimated = dto.EstimatedTransportationAmount + dto.EstimatedAccommodationAmount + allowanceAmount + dto.EstimatedOtherAmount;

            // Validasi konflik cuti, jadwal dinas lain & pagu anggaran (VR-TRV-10, 11, 12, 14)
            await _conflictValidationService.ValidateTravelRequestAsync(
                currentWorkforceProfileId, dto.StartDate, dto.EndDate, dto.CostCenterId, totalEstimated);

            var now = DateTime.UtcNow;
            var seq = await _context.TrxBusinessTravelRequests.CountAsync() + 1;
            var regNumber = $"SPPD/RSQ/{now:yyyy}/{now:MM}/{seq:D4}";

            var defaultTravelType = await _context.MstTravelTypes.FirstOrDefaultAsync();

            var entity = new TrxBusinessTravelRequest
            {
                Id = Guid.NewGuid(),
                TravelRequestNumber = regNumber,
                WorkforceProfileId = currentWorkforceProfileId,
                TravelTypeId = dto.TravelTypeId ?? defaultTravelType?.Id ?? Guid.NewGuid(),
                CostCenterId = dto.CostCenterId,
                DestinationZoneId = dto.DestinationZoneId,
                TravelTitle = dto.TravelTitle,
                TravelPurpose = dto.TravelPurpose,
                ActivityDescription = dto.ActivityDescription,
                Origin = dto.Origin,
                Destination = dto.Destination,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                TravelDayCount = inclusiveDays,
                EstimatedTransportationAmount = dto.EstimatedTransportationAmount,
                EstimatedAccommodationAmount = dto.EstimatedAccommodationAmount,
                EstimatedAllowanceAmount = allowanceAmount,
                EstimatedOtherAmount = dto.EstimatedOtherAmount,
                EstimatedTotalAmount = totalEstimated,
                // Kontrak v28 (HRD-DEC-226): anggaran disetujui baru diisi saat workflow menyetujui SPPD.
                ApprovedBudgetAmount = 0m,
                CurrencyCode = string.IsNullOrWhiteSpace(dto.CurrencyCode) ? "IDR" : dto.CurrencyCode,
                IsDomestic = dto.IsDomestic,
                IsInternational = dto.IsInternational,
                IsUrgent = dto.IsUrgent,
                // Kontrak v28 (HRD-DEC-224): SPPD disimpan sebagai Draf; persetujuan dimulai lewat Ajukan.
                TravelStatus = BusinessTravelRequestStatuses.Draft,
                IsActive = true,
                CreateBy = currentUserId,
                CreateDateTime = now
            };

            _context.TrxBusinessTravelRequests.Add(entity);
            await _context.SaveChangesAsync();

            return await GetRequestByIdAsync(entity.Id);
        }

        public async Task<BusinessTravelResponseDto> UpdateRequestAsync(Guid id, UpdateBusinessTravelRequestDto dto, Guid currentUserId)
        {
            var entity = await _context.TrxBusinessTravelRequests.FindAsync(id);
            if (entity == null) throw new KeyNotFoundException("Permohonan perjalanan dinas tidak ditemukan");

            if (!IsOpenForApplicant(entity))
            {
                throw new BusinessTravelRequestException(
                    StatusCodes.Status409Conflict,
                    "SPPD yang sedang menunggu persetujuan atau sudah diputuskan tidak dapat diubah.");
            }

            var inclusiveDays = CalculateInclusiveDays(dto.StartDate, dto.EndDate);
            if (inclusiveDays <= 0)
            {
                throw new InvalidOperationException("Durasi hari perjalanan dinas minimal 1 hari (VR-TRV-19)");
            }

            var totalEstimated = dto.EstimatedTransportationAmount + dto.EstimatedAccommodationAmount + dto.EstimatedAllowanceAmount + dto.EstimatedOtherAmount;

            await _conflictValidationService.ValidateTravelRequestAsync(
                entity.WorkforceProfileId, dto.StartDate, dto.EndDate, entity.CostCenterId, totalEstimated, entity.Id);

            entity.TravelTitle = dto.TravelTitle;
            entity.TravelPurpose = dto.TravelPurpose;
            entity.ActivityDescription = dto.ActivityDescription;
            entity.Origin = dto.Origin;
            entity.Destination = dto.Destination;
            entity.StartDate = dto.StartDate;
            entity.EndDate = dto.EndDate;
            entity.TravelDayCount = inclusiveDays;
            entity.EstimatedTransportationAmount = dto.EstimatedTransportationAmount;
            entity.EstimatedAccommodationAmount = dto.EstimatedAccommodationAmount;
            entity.EstimatedAllowanceAmount = dto.EstimatedAllowanceAmount;
            entity.EstimatedOtherAmount = dto.EstimatedOtherAmount;
            entity.EstimatedTotalAmount = totalEstimated;
            entity.UpdateBy = currentUserId;
            entity.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new BusinessTravelResponseDto
            {
                Id = entity.Id,
                TravelRequestNumber = entity.TravelRequestNumber,
                WorkforceProfileId = entity.WorkforceProfileId,
                EmployeeName = "Pegawai Pemohon",
                TravelTitle = entity.TravelTitle,
                Origin = entity.Origin,
                Destination = entity.Destination,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                TravelDayCount = entity.TravelDayCount,
                EstimatedTotalAmount = entity.EstimatedTotalAmount,
                ApprovedBudgetAmount = entity.ApprovedBudgetAmount,
                TravelStatus = entity.TravelStatus,
                CurrencyCode = entity.CurrencyCode,
                CreatedAt = entity.CreateDateTime
            };
        }

        public async Task<BusinessTravelDetailResponseDto> SubmitRequestAsync(
            Guid id, Guid currentUserId, Guid currentWorkforceProfileId, CancellationToken cancellationToken = default)
        {
            var entity = await _context.TrxBusinessTravelRequests
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive && !x.IsDelete, cancellationToken);
            if (entity == null) throw new KeyNotFoundException("Permohonan perjalanan dinas tidak ditemukan");

            // Permission 32.1 (kontrak v28): hanya pemohon SPPD yang boleh mengajukan.
            if (currentWorkforceProfileId == Guid.Empty || entity.WorkforceProfileId != currentWorkforceProfileId)
            {
                throw new BusinessTravelRequestException(
                    StatusCodes.Status403Forbidden,
                    "Hanya pemohon SPPD yang dapat mengajukan SPPD ini.");
            }

            if (entity.TravelStatus == BusinessTravelRequestStatuses.Rejected)
            {
                throw new BusinessTravelRequestException(
                    StatusCodes.Status409Conflict,
                    "SPPD yang ditolak tidak dapat diajukan ulang. Buat SPPD baru.");
            }

            if (!IsOpenForApplicant(entity))
            {
                throw new BusinessTravelRequestException(
                    StatusCodes.Status409Conflict,
                    "SPPD hanya dapat diajukan dari status Draft atau Dikembalikan untuk revisi.");
            }

            // SPPD yang dikembalikan memakai ulang instance-nya (Workflow V2 menerima submit dari Draft,
            // RevisionRequested, dan Returned); selain itu dibuat instance baru.
            Guid? reusableWorkflowId = null;
            if (entity.WorkflowInstanceId.HasValue)
            {
                var existingStatus = await _context.Set<TrxWorkflowInstance>()
                    .AsNoTracking()
                    .Where(x => x.Id == entity.WorkflowInstanceId.Value && !x.IsDelete)
                    .Select(x => x.WorkflowStatus)
                    .FirstOrDefaultAsync(cancellationToken);
                if (existingStatus == WorkflowValueConstants.WorkflowStatus.Draft ||
                    existingStatus == WorkflowValueConstants.WorkflowStatus.RevisionRequested ||
                    existingStatus == WorkflowValueConstants.WorkflowStatus.Returned)
                {
                    reusableWorkflowId = entity.WorkflowInstanceId.Value;
                }
            }

            var previousDefinitionId = entity.WorkflowDefinitionId;
            var previousInstanceId = entity.WorkflowInstanceId;
            var workflowId = reusableWorkflowId;
            var createdNow = false;

            if (workflowId == null)
            {
                var createResult = await _workflowService.CreateAsync(
                    new CreateWorkflowInstanceRequest
                    {
                        WorkflowDefinitionCode = BusinessTravelWorkflowCodes.DefinitionCode,
                        ReferenceType = BusinessTravelWorkflowCodes.ReferenceType,
                        ReferenceId = entity.Id,
                        ExternalReferenceNumber = entity.TravelRequestNumber,
                        RequestContext = JsonSerializer.SerializeToElement(new
                        {
                            businessTravelRequestId = entity.Id,
                            travelRequestNumber = entity.TravelRequestNumber,
                            entity.TravelTitle,
                            entity.Destination,
                            startDate = entity.StartDate,
                            endDate = entity.EndDate,
                            amount = entity.EstimatedTotalAmount,
                            entity.CurrencyCode
                        })
                    },
                    cancellationToken);

                if (!createResult.Success || createResult.Data == null)
                {
                    throw new BusinessTravelRequestException(ToTravelStatusCode(createResult.StatusCode), createResult.Message);
                }

                createdNow = true;
                workflowId = createResult.Data.Id;
                entity.WorkflowDefinitionId = createResult.Data.WorkflowDefinitionId;
                entity.WorkflowInstanceId = createResult.Data.Id;
                entity.UpdateBy = currentUserId;
                entity.UpdateDateTime = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }

            var submitResult = await _workflowService.SubmitAsync(workflowId.Value, new WorkflowSubmitRequest(), cancellationToken);
            if (!submitResult.Success || submitResult.Data == null)
            {
                // Submit yang gagal sudah di-rollback Workflow V2, tetapi perubahannya masih di change tracker yang sama
                // (pola BE-HRD-FIX-002). Kembalikan SPPD ke keadaan sebelum Ajukan dan hapus draf workflow yang baru dibuat.
                _context.ChangeTracker.Clear();
                if (createdNow)
                {
                    var restored = await _context.TrxBusinessTravelRequests.FirstAsync(x => x.Id == id, cancellationToken);
                    restored.WorkflowDefinitionId = previousDefinitionId;
                    restored.WorkflowInstanceId = previousInstanceId;
                    restored.UpdateBy = currentUserId;
                    restored.UpdateDateTime = DateTime.UtcNow;

                    var draftWorkflow = await _context.Set<TrxWorkflowInstance>()
                        .FirstOrDefaultAsync(x =>
                            x.Id == workflowId.Value &&
                            x.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Draft &&
                            !x.IsDelete,
                            cancellationToken);
                    if (draftWorkflow != null)
                    {
                        var deletedAt = DateTime.UtcNow;
                        draftWorkflow.IsDelete = true;
                        draftWorkflow.DeleteDateTime = deletedAt;
                        draftWorkflow.DeleteBy = currentUserId;
                        draftWorkflow.UpdateDateTime = deletedAt;
                        draftWorkflow.UpdateBy = currentUserId;
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                }

                throw new BusinessTravelRequestException(ToTravelStatusCode(submitResult.StatusCode), submitResult.Message);
            }

            var submitted = await _context.TrxBusinessTravelRequests.FirstAsync(x => x.Id == id, cancellationToken);
            var now = DateTime.UtcNow;
            submitted.TravelStatus = BusinessTravelRequestStatuses.Submitted;
            submitted.SubmittedAt = now;
            submitted.SubmittedByUserId = currentUserId;
            submitted.CurrentApprovalStep = submitResult.Data.CurrentStepOrder;
            submitted.UpdateBy = currentUserId;
            submitted.UpdateDateTime = now;
            await _context.SaveChangesAsync(cancellationToken);

            return await GetRequestByIdAsync(id);
        }

        // VR-TRV-29: galat Workflow V2 diteruskan apa adanya. Definisi yang tidak aktif/tidak berlaku (404 di engine)
        // adalah konflik konfigurasi bagi SPPD yang ada (409); identitas pelaku yang tidak lengkap (401) menjadi 400.
        private static int ToTravelStatusCode(int workflowStatusCode) => workflowStatusCode switch
        {
            StatusCodes.Status404NotFound => StatusCodes.Status409Conflict,
            StatusCodes.Status401Unauthorized => StatusCodes.Status400BadRequest,
            _ => workflowStatusCode
        };

        public async Task<BusinessTravelResponseDto> CancelRequestAsync(Guid id, CancelTravelRequestDto dto, Guid currentUserId, CancellationToken cancellationToken = default)
        {
            var entity = await _context.TrxBusinessTravelRequests.FindAsync(new object[] { id }, cancellationToken);
            if (entity == null) throw new KeyNotFoundException("Permohonan perjalanan dinas tidak ditemukan");

            // State 32.2 (kontrak v28): SPPD yang ditolak bersifat final, sama dengan yang sudah selesai atau dibatalkan.
            if (entity.TravelStatus == BusinessTravelRequestStatuses.Completed ||
                entity.TravelStatus == BusinessTravelRequestStatuses.Cancelled ||
                entity.TravelStatus == BusinessTravelRequestStatuses.Rejected)
            {
                throw new InvalidOperationException("Perjalanan dinas yang sudah selesai, ditolak, atau dibatalkan tidak dapat diubah");
            }

            if (string.IsNullOrWhiteSpace(dto.CancellationReason) || dto.CancellationReason.Trim().Length < 15)
            {
                throw new InvalidOperationException("Alasan pembatalan dinas wajib diisi tertulis minimal 15 karakter (VR-TRV-20)");
            }

            // VR-TRV-33: workflow persetujuan yang masih hidup ditutup lebih dulu (pemohon). Bila Workflow V2 menolak,
            // SPPD tidak berubah. Sedang diproses → ditarik (Withdraw); belum dikirim atau dikembalikan → dibatalkan.
            if (entity.WorkflowInstanceId.HasValue)
            {
                var workflowStatus = await _context.Set<TrxWorkflowInstance>()
                    .AsNoTracking()
                    .Where(x => x.Id == entity.WorkflowInstanceId.Value && !x.IsDelete)
                    .Select(x => x.WorkflowStatus)
                    .FirstOrDefaultAsync(cancellationToken);

                WorkflowServiceResult<WorkflowInstanceDetailResponse>? closed = null;
                if (workflowStatus == WorkflowValueConstants.WorkflowStatus.InProgress)
                {
                    closed = await _workflowService.WithdrawAsync(entity.WorkflowInstanceId.Value,
                        new WorkflowWithdrawRequest { Reason = dto.CancellationReason.Trim() }, cancellationToken);
                }
                else if (workflowStatus == WorkflowValueConstants.WorkflowStatus.Draft ||
                         workflowStatus == WorkflowValueConstants.WorkflowStatus.Submitted ||
                         workflowStatus == WorkflowValueConstants.WorkflowStatus.RevisionRequested ||
                         workflowStatus == WorkflowValueConstants.WorkflowStatus.Returned)
                {
                    closed = await _workflowService.CancelAsync(entity.WorkflowInstanceId.Value,
                        new WorkflowCancelRequest { Reason = dto.CancellationReason.Trim() }, cancellationToken);
                }

                if (closed != null && !closed.Success)
                {
                    _context.ChangeTracker.Clear();
                    throw new BusinessTravelRequestException(StatusCodes.Status409Conflict, closed.Message);
                }

                // Penangan SPPD (BE-HRD-376) sudah dapat menyetel status lewat entitas yang sama; muat ulang nilai terbaru.
                await _context.Entry(entity).ReloadAsync(cancellationToken);
            }

            var wasApproved = entity.ApprovedAt.HasValue;
            var now = DateTime.UtcNow;
            entity.TravelStatus = BusinessTravelRequestStatuses.Cancelled;
            entity.CancellationReason = dto.CancellationReason;
            entity.CancelledAt ??= now;
            entity.CancelledByUserId ??= currentUserId;
            entity.UpdateBy = currentUserId;
            entity.UpdateDateTime = now;

            if (wasApproved)
            {
                // INT-HR-110: SPPD yang sudah disetujui melepas gembok roster dan presensi Dinas Luar; disimpan bersama pembatalan.
                await _lockService.ReleaseRosterLockAsync(entity.Id, currentUserId);
            }
            else
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return new BusinessTravelResponseDto
            {
                Id = entity.Id,
                TravelRequestNumber = entity.TravelRequestNumber,
                WorkforceProfileId = entity.WorkforceProfileId,
                EmployeeName = "Pegawai Pemohon",
                TravelTitle = entity.TravelTitle,
                Origin = entity.Origin,
                Destination = entity.Destination,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                TravelDayCount = entity.TravelDayCount,
                EstimatedTotalAmount = entity.EstimatedTotalAmount,
                ApprovedBudgetAmount = entity.ApprovedBudgetAmount,
                TravelStatus = entity.TravelStatus,
                CurrencyCode = entity.CurrencyCode,
                CreatedAt = entity.CreateDateTime
            };
        }
    }
}
