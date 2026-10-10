using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Service pengelola antrean dan penyelesaian admisi ranap dari transfer pasien IGD & Poliklinik (BE-RWI-206, BE-RWI-207).
    /// </summary>
    public class InpAdmissionTransferService
    {
        private const string LogCategory = "HealthServices.InPatient.AdmissionTransfer";

        private readonly ApplicationDbContext _dbContext;
        private readonly InpEpisodeService _episodeService;
        private readonly InpBedOccupancyService _bedOccupancyService;
        private readonly LoggerService _loggerService;

        public InpAdmissionTransferService(
            ApplicationDbContext dbContext,
            InpEpisodeService episodeService,
            InpBedOccupancyService bedOccupancyService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _episodeService = episodeService;
            _bedOccupancyService = bedOccupancyService;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Mengambil daftar antrean pasien transfer dari IGD yang menunggu tempat tidur (BE-RWI-206).
        /// </summary>
        public async Task<PagedResult<InpatientAdmissionTransferItemResponse>> GetPagedTransfersAsync(
            AdmissionTransferQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

            var baseQuery = _dbContext.Set<EmgDisposition>()
                .AsNoTracking()
                .Include(x => x.EmergencyVisit)
                    .ThenInclude(v => v!.Patient)
                .Include(x => x.DispositionType)
                .Include(x => x.DecidedByDoctor)
                .Include(x => x.DestinationServiceUnit)
                .Where(x => !x.IsDelete && x.EmergencyVisit != null && !x.EmergencyVisit.IsDelete);

            // Saring jenis disposisi yang mengarah ke rawat inap
            baseQuery = baseQuery.Where(x =>
                x.DispositionType != null &&
                (x.DispositionType.RequiresDestinationServiceUnit ||
                 x.DestinationServiceUnitId != null ||
                 x.DispositionType.Name.Contains("Inap") ||
                 x.DispositionType.Code.Contains("INPATIENT") ||
                 x.DispositionType.Code.Contains("RANAP")));

            // Saring status
            if (query.Status.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.DispositionStatus == query.Status.Value);
            }
            else
            {
                // Bawaan: status Confirmed dan belum dieksekusi
                baseQuery = baseQuery.Where(x =>
                    x.DispositionStatus == EmergencyDispositionStatus.Confirmed &&
                    x.ExecutedAt == null);
            }

            // Saring overdue (> 60 menit)
            var now = DateTime.UtcNow;
            if (query.OverdueOnly == true)
            {
                var overdueThreshold = now.AddMinutes(-60);
                baseQuery = baseQuery.Where(x => x.DecidedAt <= overdueThreshold);
            }

            // Pencarian nama, nomor RM, atau nomor kunjungan IGD
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                baseQuery = baseQuery.Where(x =>
                    (x.EmergencyVisit!.Patient != null && (
                        x.EmergencyVisit.Patient.FullName.ToLower().Contains(term) ||
                        x.EmergencyVisit.Patient.MedicalRecordNumber.ToLower().Contains(term))) ||
                    x.EmergencyVisit.EmergencyVisitNumber.ToLower().Contains(term));
            }

            var totalCount = await baseQuery.CountAsync(cancellationToken);

            var items = await baseQuery
                .OrderBy(x => x.DecidedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    Disposition = x,
                    Visit = x.EmergencyVisit!,
                    Patient = x.EmergencyVisit!.Patient,
                    Doctor = x.DecidedByDoctor,
                    Unit = x.DestinationServiceUnit
                })
                .ToListAsync(cancellationToken);

            var resultItems = items.Select(x =>
            {
                var waitingMinutes = (int)Math.Max(0, (now - x.Disposition.DecidedAt).TotalMinutes);
                return new InpatientAdmissionTransferItemResponse
                {
                    TransferId = x.Disposition.Id,
                    SourceType = "Emergency",
                    EmergencyVisitId = x.Visit.Id,
                    EmergencyVisitNumber = x.Visit.EmergencyVisitNumber,
                    SourceEncounterId = x.Visit.EncounterId,
                    PatientId = x.Patient != null ? x.Patient.Id : Guid.Empty,
                    PatientName = x.Patient?.FullName ?? "Pasien Tidak Diketahui",
                    MedicalRecordNumber = x.Patient?.MedicalRecordNumber ?? "-",
                    Gender = x.Patient?.Gender?.ToString(),
                    BirthDate = x.Patient?.BirthDate,
                    DecidedByDoctorId = x.Disposition.DecidedByDoctorId,
                    DecidedByDoctorName = x.Doctor?.FullName,
                    DispositionReason = x.Disposition.DispositionReason,
                    PatientCondition = x.Disposition.PatientConditionAtDisposition,
                    FollowUpInstruction = x.Disposition.FollowUpInstruction,
                    RecommendedServiceUnitId = x.Disposition.DestinationServiceUnitId,
                    RecommendedServiceUnitName = x.Unit?.ServiceUnitName,
                    DecidedAt = x.Disposition.DecidedAt,
                    WaitingMinutes = waitingMinutes,
                    IsOverdue = waitingMinutes > 60,
                    DispositionStatus = x.Disposition.DispositionStatus.ToString()
                };
            }).ToList();

            return new PagedResult<InpatientAdmissionTransferItemResponse>(
                resultItems, totalCount, pageNumber, pageSize);
        }

        /// <summary>
        /// Mengambil detail satu rujukan transfer IGD untuk mengisi awal stepper admisi (BE-RWI-206).
        /// </summary>
        public async Task<InpatientAdmissionTransferItemResponse?> GetTransferByIdAsync(
            Guid transferId,
            CancellationToken cancellationToken = default)
        {
            var disposition = await _dbContext.Set<EmgDisposition>()
                .AsNoTracking()
                .Include(x => x.EmergencyVisit)
                    .ThenInclude(v => v!.Patient)
                .Include(x => x.DecidedByDoctor)
                .Include(x => x.DestinationServiceUnit)
                .FirstOrDefaultAsync(x => x.Id == transferId && !x.IsDelete, cancellationToken);

            if (disposition == null || disposition.EmergencyVisit == null)
            {
                return null;
            }

            var now = DateTime.UtcNow;
            var waitingMinutes = (int)Math.Max(0, (now - disposition.DecidedAt).TotalMinutes);
            var patient = disposition.EmergencyVisit.Patient;

            return new InpatientAdmissionTransferItemResponse
            {
                TransferId = disposition.Id,
                SourceType = "Emergency",
                EmergencyVisitId = disposition.EmergencyVisit.Id,
                EmergencyVisitNumber = disposition.EmergencyVisit.EmergencyVisitNumber,
                SourceEncounterId = disposition.EmergencyVisit.EncounterId,
                PatientId = patient?.Id ?? Guid.Empty,
                PatientName = patient?.FullName ?? string.Empty,
                MedicalRecordNumber = patient?.MedicalRecordNumber ?? string.Empty,
                Gender = patient?.Gender?.ToString(),
                BirthDate = patient?.BirthDate,
                DecidedByDoctorId = disposition.DecidedByDoctorId,
                DecidedByDoctorName = disposition.DecidedByDoctor?.FullName,
                DispositionReason = disposition.DispositionReason,
                PatientCondition = disposition.PatientConditionAtDisposition,
                FollowUpInstruction = disposition.FollowUpInstruction,
                RecommendedServiceUnitId = disposition.DestinationServiceUnitId,
                RecommendedServiceUnitName = disposition.DestinationServiceUnit?.ServiceUnitName,
                DecidedAt = disposition.DecidedAt,
                WaitingMinutes = waitingMinutes,
                IsOverdue = waitingMinutes > 60,
                DispositionStatus = disposition.DispositionStatus.ToString()
            };
        }

        /// <summary>
        /// Mengeksekusi pendaftaran rawat inap dari transfer IGD secara atomik (BE-RWI-207).
        /// </summary>
        public async Task<InpEpisodeOperationResult> ExecuteTransferAdmissionAsync(
            OpenAdmissionFromTransferRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return InpEpisodeOperationResult.Invalid("Data pendaftaran transfer belum dikirim.");
            }

            // 1. Verifikasi Disposisi IGD
            var disposition = await _dbContext.Set<EmgDisposition>()
                .Include(x => x.EmergencyVisit)
                .FirstOrDefaultAsync(
                    x => x.Id == request.TransferDispositionId && !x.IsDelete,
                    cancellationToken);

            if (disposition == null || disposition.EmergencyVisit == null)
            {
                return InpEpisodeOperationResult.NotFound("Disposisi transfer IGD tidak ditemukan.");
            }

            if (disposition.DispositionStatus != EmergencyDispositionStatus.Confirmed ||
                disposition.ExecutedAt.HasValue)
            {
                return InpEpisodeOperationResult.BusinessRuleRejected(
                    "Disposisi transfer IGD ini sudah pernah dieksekusi atau tidak berstatus menunggu kamar.");
            }

            if (disposition.EmergencyVisit.PatientId != request.PatientId)
            {
                return InpEpisodeOperationResult.BusinessRuleRejected(
                    "Pasien yang didaftarkan tidak cocok dengan data rujukan transfer IGD.");
            }

            // 2. Buka episode rawat inap
            var openRequest = new OpenAdmissionRequest
            {
                PatientId = request.PatientId,
                EncounterId = null, // Admisi membuat encounter rawat inap sendiri
                ServiceUnitId = request.ServiceUnitId,
                PatientClassId = request.PatientClassId,
                DoctorId = request.DoctorId,
                Notes = string.IsNullOrWhiteSpace(request.Notes)
                    ? $"Transfer dari IGD {disposition.EmergencyVisit.EmergencyVisitNumber}"
                    : request.Notes
            };

            var openResult = await _episodeService.OpenAdmissionAsync(
                openRequest,
                actorUserId,
                cancellationToken);

            if (!openResult.IsSuccess || openResult.Data == null)
            {
                return openResult;
            }

            var episodeId = openResult.Data.Id;

            // 3. Reservasi / Booking Tempat Tidur
            if (request.BedId != Guid.Empty)
            {
                var reserveBedRequest = new ReserveBedRequest
                {
                    EpisodeId = episodeId,
                    BedId = request.BedId,
                    Notes = $"Alokasi kamar untuk transfer IGD ({disposition.EmergencyVisit.EmergencyVisitNumber})"
                };

                var bedResult = await _bedOccupancyService.ReserveBedAsync(
                    reserveBedRequest,
                    actorUserId,
                    cancellationToken);

                if (!bedResult.IsSuccess)
                {
                    // Rollback pembukaan admisi jika booking bed gagal
                    await _episodeService.CancelAdmissionAsync(
                        episodeId,
                        new CancelAdmissionRequest { CancellationReason = "Gagal alokasi tempat tidur transfer: " + bedResult.Message },
                        actorUserId,
                        cancellationToken);

                    return InpEpisodeOperationResult.BusinessRuleRejected(
                        $"Alokasi tempat tidur gagal: {bedResult.Message}");
                }
            }

            // 4. Update status disposisi IGD menjadi Executed
            var now = DateTime.UtcNow;
            disposition.DispositionStatus = EmergencyDispositionStatus.Executed;
            disposition.ExecutedAt = now;
            disposition.ConfirmedByUserId = actorUserId;
            disposition.UpdateDateTime = now;
            disposition.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            // 5. Catat audit log
            await _loggerService.AuditAsync(
                LogCategory,
                "InpAdmissionTransfer.Executed",
                $"Admisi rawat inap berhasil diterbitkan untuk transfer IGD {disposition.EmergencyVisit.EmergencyVisitNumber}. EpisodeId={episodeId}",
                actorUserId,
                cancellationToken);

            return InpEpisodeOperationResult.Success(
                openResult.Data,
                "Pendaftaran rawat inap dari transfer IGD berhasil diselesaikan.");
        }
    }
}
