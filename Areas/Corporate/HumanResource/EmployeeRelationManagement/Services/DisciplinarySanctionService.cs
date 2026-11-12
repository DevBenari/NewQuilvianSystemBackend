using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public class DisciplinarySanctionService : IDisciplinarySanctionService
    {
        private readonly IEmployeeRelationUnitOfWork _uow;
        private readonly NumberSeriesAllocator _numberAllocator;

        public DisciplinarySanctionService(IEmployeeRelationUnitOfWork uow, NumberSeriesAllocator numberAllocator)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _numberAllocator = numberAllocator ?? throw new ArgumentNullException(nameof(numberAllocator));
        }

        public async Task<SanctionResponseDto> IssueSanctionAsync(IssueSanctionRequestDto request, string actorEmail, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Jika ada investigationId, validasi Just Culture classification
            if (request.InvestigationId.HasValue && request.InvestigationId.Value != Guid.Empty)
            {
                var investigation = await _uow.InvestigationRecords
                    .Include(x => x.IncidentReport)
                    .FirstOrDefaultAsync(x => x.InvestigationId == request.InvestigationId.Value, cancellationToken);

                // VR-EMP-03: Dilarang menerbitkan sanksi SP jika just_culture_classification == "HumanError"
                if (investigation?.IncidentReport != null &&
                    string.Equals(investigation.IncidentReport.JustCultureClassification, "HumanError", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Prinsip Just Culture: Insiden kategori Human Error murni dialihkan ke pembinaan/diklat Sub-16, dilarang diterbitkan sanksi SP sesuai aturan VR-EMP-03.");
                }
            }

            // Hitung masa berlaku
            DateTime validUntil;
            if (string.Equals(request.SanctionLevel, "VerbalWarning", StringComparison.OrdinalIgnoreCase))
            {
                // VR-EMP-07: Masa berlaku Teguran Lisan maksimal 90 hari kalender (3 bulan)
                validUntil = request.IssuedDate.AddMonths(3);
            }
            else
            {
                // VR-EMP-06: Masa berlaku SP 1, 2, 3 wajib tepat 6 bulan kalender (PP 35/2021)
                validUntil = request.IssuedDate.AddMonths(6);
            }

            // Ambil riwayat sanksi aktif
            var activeSanctions = await _uow.Sanctions
                .Where(x => x.EmployeeId == request.EmployeeId && x.Status == "Active" && x.ValidUntil > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            // VR-EMP-08: SP 2 mensyaratkan adanya SP 1 aktif, kecuali bypass pelanggaran berat
            if (string.Equals(request.SanctionLevel, "SP2", StringComparison.OrdinalIgnoreCase))
            {
                var hasActiveSp1 = activeSanctions.Any(x => string.Equals(x.SanctionLevel, "SP1", StringComparison.OrdinalIgnoreCase));
                if (!hasActiveSp1 && !request.IsDirectorApprovedBypass)
                {
                    throw new InvalidOperationException("Penerbitan SP 2 mensyaratkan adanya SP 1 yang masih aktif sesuai aturan VR-EMP-08, kecuali disetujui sebagai pelanggaran berat mendesak.");
                }
            }

            // VR-EMP-09: Direct SP 3 bypass wajib melalui persetujuan Direktur SDM
            if (string.Equals(request.SanctionLevel, "SP3", StringComparison.OrdinalIgnoreCase))
            {
                var hasActiveSp2 = activeSanctions.Any(x => string.Equals(x.SanctionLevel, "SP2", StringComparison.OrdinalIgnoreCase));
                if (!hasActiveSp2 && !request.IsDirectorApprovedBypass)
                {
                    throw new InvalidOperationException("Penerbitan langsung SP 3 tanpa SP 2 aktif wajib melalui persetujuan Direktur SDM dengan bukti pelanggaran berat sesuai aturan VR-EMP-09.");
                }
            }

            // Satu register SP per bulan untuk seluruh tingkat; bagian acak diganti nomor urut (QBE-CODE-003). Bentuk surat tetap: SP1/HRD/11/2026/0001
            var allocatedSanction = await _numberAllocator.AllocateAsync(new NumberAllocationRequest(
                SequenceKey: EmployeeRelationNumberSeries.Keys.Sanction,
                Prefix: EmployeeRelationNumberSeries.Prefixes.Sanction,
                ResetPolicy: EmployeeRelationNumberSeries.ResetPolicies.Sanction,
                SequenceDigits: EmployeeRelationNumberSeries.SequenceDigits,
                ActorUserId: await ResolveActorUserIdAsync(actorEmail, cancellationToken),
                Instant: DateTimeOffset.UtcNow), cancellationToken);
            var sanctionNumber = $"{request.SanctionLevel}/HRD/{DateTime.UtcNow:MM/yyyy}/{EmployeeRelationNumberSeries.TrailingSequence(allocatedSanction)}";

            var entity = new TrxDisciplinarySanction
            {
                SanctionId = Guid.NewGuid(),
                InvestigationId = request.InvestigationId,
                EmployeeId = request.EmployeeId,
                SanctionNumber = sanctionNumber,
                SanctionLevel = request.SanctionLevel.Trim(),
                IssuedDate = request.IssuedDate,
                ValidUntil = validUntil,
                ViolationDescription = request.ViolationDescription.Trim(),
                Status = "Active",
                IsAppealed = false,
                CreateDateTime = DateTime.UtcNow
            };

            await _uow.AddSanctionAsync(entity, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return MapToDto(entity);
        }

        public async Task<List<SanctionResponseDto>> GetSanctionsByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
        {
            var sanctions = await _uow.Sanctions
                .AsNoTracking()
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.IssuedDate)
                .Select(x => MapToDto(x))
                .ToListAsync(cancellationToken);

            return sanctions;
        }

        public async Task<SanctionAppealResponseDto> AppealSanctionAsync(Guid sanctionId, AppealSanctionRequestDto request, string actorEmail, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var sanction = await _uow.Sanctions.FirstOrDefaultAsync(x => x.SanctionId == sanctionId, cancellationToken);
            if (sanction == null)
            {
                throw new KeyNotFoundException($"Surat sanksi dengan ID {sanctionId} tidak ditemukan.");
            }

            // VR-EMP-10: Pengajuan banding atas sanksi SP dibatasi maksimal 7 hari kerja sejak IssuedDate
            var daysSinceIssued = (DateTime.UtcNow.Date - sanction.IssuedDate.Date).TotalDays;
            if (daysSinceIssued > 10) // Memberikan toleransi hari kerja (7 hari kerja ~ 9-10 hari kalender)
            {
                throw new InvalidOperationException("Batas waktu pengajuan sanggah banding sanksi disiplin telah kedaluwarsa (maksimal 7 hari kerja) sesuai aturan VR-EMP-10.");
            }

            sanction.IsAppealed = true;
            sanction.AppealStatement = request.AppealStatement.Trim();
            sanction.AppealEvidenceUrls = request.SupportingEvidenceUrls?.Trim();
            sanction.AppealedAt = DateTime.UtcNow;
            sanction.UpdateDateTime = DateTime.UtcNow;

            await _uow.SaveChangesAsync(cancellationToken);

            return new SanctionAppealResponseDto
            {
                SanctionId = sanction.SanctionId,
                SanctionNumber = sanction.SanctionNumber,
                IsAppealed = true,
                AppealedAt = sanction.AppealedAt.Value,
                Status = sanction.Status,
                Message = "Sanggah banding sanksi berhasil didaftarkan untuk proses investigasi ulang Majelis Disiplin."
            };
        }

        private static SanctionResponseDto MapToDto(TrxDisciplinarySanction entity)
        {
            return new SanctionResponseDto
            {
                SanctionId = entity.SanctionId,
                InvestigationId = entity.InvestigationId,
                EmployeeId = entity.EmployeeId,
                SanctionNumber = entity.SanctionNumber,
                SanctionLevel = entity.SanctionLevel,
                IssuedDate = entity.IssuedDate,
                ValidUntil = entity.ValidUntil,
                ViolationDescription = entity.ViolationDescription,
                Status = entity.Status,
                IsAppealed = entity.IsAppealed,
                AppealResolution = entity.AppealResolution,
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
