using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public class DisciplinarySuspensionService : IDisciplinarySuspensionService
    {
        private readonly IEmployeeRelationUnitOfWork _uow;

        public DisciplinarySuspensionService(IEmployeeRelationUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
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

            var suspensionNumber = $"SKORS/HRD/{DateTime.UtcNow:yyyyMM}/{Random.Shared.Next(100, 999)}";

            var entity = new TrxDisciplinarySuspension
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

        private static SuspensionResponseDto MapToDto(TrxDisciplinarySuspension entity)
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
    }
}
