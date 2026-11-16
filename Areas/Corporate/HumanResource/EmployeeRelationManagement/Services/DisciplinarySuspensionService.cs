using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public class DisciplinarySuspensionService : IDisciplinarySuspensionService
    {
        private readonly IEmployeeRelationUnitOfWork _uow;
        private readonly NumberSeriesAllocator _numberAllocator;

        public DisciplinarySuspensionService(IEmployeeRelationUnitOfWork uow, NumberSeriesAllocator numberAllocator)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _numberAllocator = numberAllocator ?? throw new ArgumentNullException(nameof(numberAllocator));
        }

        public async Task<SuspensionResponseDto> IssueSuspensionAsync(IssueSuspensionRequestDto request, string actorEmail, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // VR-EMP-11: Durasi skorsing investigasi maksimal 180 hari kalender (6 bulan)
            var durationDays = (request.ExpectedEndDate.Date - request.StartDate.Date).TotalDays;
            if (durationDays <= 0 || durationDays > 180)
            {
                throw new InvalidOperationException("Masa skorsing investigasi tidak boleh melampaui batas maksimal 6 bulan kalender (180 hari) sesuai aturan VR-EMP-11.");
            }

            // Bagian acak diganti nomor urut bulanan dari provider bersama (QBE-CODE-003). Bentuk surat tetap: SKORS/HRD/202611/0001
            var allocatedSuspension = await _numberAllocator.AllocateAsync(new NumberAllocationRequest(
                SequenceKey: EmployeeRelationNumberSeries.Keys.Suspension,
                Prefix: EmployeeRelationNumberSeries.Prefixes.Suspension,
                ResetPolicy: EmployeeRelationNumberSeries.ResetPolicies.Suspension,
                SequenceDigits: EmployeeRelationNumberSeries.SequenceDigits,
                ActorUserId: await ResolveActorUserIdAsync(actorEmail, cancellationToken),
                Instant: DateTimeOffset.UtcNow), cancellationToken);
            var suspensionNumber = $"SKORS/HRD/{DateTime.UtcNow:yyyyMM}/{EmployeeRelationNumberSeries.TrailingSequence(allocatedSuspension)}";

            var entity = new HrdDisciplinarySuspension
            {
                SuspensionId = Guid.NewGuid(),
                InvestigationId = request.InvestigationId,
                EmployeeId = request.EmployeeId,
                SuspensionNumber = suspensionNumber,
                StartDate = request.StartDate,
                ExpectedEndDate = request.ExpectedEndDate,
                // VR-EMP-12: Hak upah skorsing wajib PaidFull 100% Pasal 157A UU Ketenagakerjaan
                WagePaymentStatus = "PaidFull",
                // VR-EMP-13: Akses SIMRS dan fisik wajib dibekukan instan (INT-HR-88)
                IsSystemAccessRevoked = true,
                IsPhysicalAccessRevoked = true,
                Status = "Active",
                CreateDateTime = DateTime.UtcNow
            };

            await _uow.AddSuspensionAsync(entity, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return MapToDto(entity);
        }

        public async Task<SuspensionResponseDto> ReinstateEmployeeAsync(Guid suspensionId, ReinstateEmployeeRequestDto request, string actorEmail, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var suspension = await _uow.Suspensions.FirstOrDefaultAsync(x => x.SuspensionId == suspensionId, cancellationToken);
            if (suspension == null)
            {
                throw new KeyNotFoundException($"Berkas skorsing dengan ID {suspensionId} tidak ditemukan.");
            }

            suspension.ActualEndDate = request.ActualEndDate;
            suspension.ReinstatementNotes = request.ReinstatementNotes.Trim();
            suspension.Status = "CompletedReinstated";
            suspension.IsSystemAccessRevoked = false;
            suspension.IsPhysicalAccessRevoked = false;
            suspension.UpdateDateTime = DateTime.UtcNow;

            await _uow.SaveChangesAsync(cancellationToken);
            return MapToDto(suspension);
        }

        public async Task<SuspensionResponseDto?> GetSuspensionByIdAsync(Guid suspensionId, CancellationToken cancellationToken = default)
        {
            var suspension = await _uow.Suspensions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SuspensionId == suspensionId, cancellationToken);

            return suspension == null ? null : MapToDto(suspension);
        }

        private static SuspensionResponseDto MapToDto(HrdDisciplinarySuspension entity)
        {
            return new SuspensionResponseDto
            {
                SuspensionId = entity.SuspensionId,
                InvestigationId = entity.InvestigationId,
                EmployeeId = entity.EmployeeId,
                SuspensionNumber = entity.SuspensionNumber,
                StartDate = entity.StartDate,
                ExpectedEndDate = entity.ExpectedEndDate,
                ActualEndDate = entity.ActualEndDate,
                WagePaymentStatus = entity.WagePaymentStatus,
                IsSystemAccessRevoked = entity.IsSystemAccessRevoked,
                IsPhysicalAccessRevoked = entity.IsPhysicalAccessRevoked,
                Status = entity.Status,
                ReinstatementNotes = entity.ReinstatementNotes,
                CreatedAt = entity.CreateDateTime
            };
        }

        private async Task<Guid> ResolveActorUserIdAsync(string actorEmail, CancellationToken cancellationToken)
        {
            var userId = await _uow.FindUserIdByEmailAsync(actorEmail, cancellationToken);
            return userId ?? EmployeeRelationNumberSeries.SystemFallbackUserId;
        }
    }
}
