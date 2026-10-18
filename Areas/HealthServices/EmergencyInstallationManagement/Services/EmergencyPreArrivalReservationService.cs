using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    public class EmergencyPreArrivalReservationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly EmergencyDocumentNumberService _documentNumberService;
        private readonly EmergencyVisitService _emergencyVisitService;

        public EmergencyPreArrivalReservationService(
            ApplicationDbContext dbContext,
            EmergencyDocumentNumberService documentNumberService,
            EmergencyVisitService emergencyVisitService)
        {
            _dbContext = dbContext;
            _documentNumberService = documentNumberService;
            _emergencyVisitService = emergencyVisitService;
        }

        public sealed record Hasil<T>(T? Data, int StatusCode, string? Penolakan)
        {
            public bool Berhasil => Penolakan == null;
            public static Hasil<T> Ok(T data) => new(data, StatusCodes.Status200OK, null);
            public static Hasil<T> Dibuat(T data) => new(data, StatusCodes.Status201Created, null);
            public static Hasil<T> Gagal(int statusCode, string penolakan) => new(default, statusCode, penolakan);
        }

        public async Task<Hasil<EmergencyPreArrivalReservationResponse>> CreateReservationAsync(
            CreateEmergencyPreArrivalReservationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.CallerName))
                return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                    StatusCodes.Status400BadRequest, "Nama pemanggil wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.CallerPhoneNumber))
                return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                    StatusCodes.Status400BadRequest, "Nomor telepon pemanggil wajib diisi.");

            if (request.EstimatedArrivalDateTime == default)
                return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                    StatusCodes.Status400BadRequest, "Estimasi waktu tiba wajib diisi.");

            Guid? patientId = request.PatientId.HasValue && request.PatientId.Value != Guid.Empty
                ? request.PatientId.Value
                : null;

            Guid? patientMembershipId = request.PatientMembershipId.HasValue && request.PatientMembershipId.Value != Guid.Empty
                ? request.PatientMembershipId.Value
                : null;

            Guid? membershipTierId = request.MembershipTierId.HasValue && request.MembershipTierId.Value != Guid.Empty
                ? request.MembershipTierId.Value
                : null;

            if (patientId.HasValue)
            {
                var patientExists = await _dbContext.Set<MstPatient>()
                    .AsNoTracking()
                    .AnyAsync(p => p.Id == patientId.Value && !p.IsDelete, cancellationToken);

                if (!patientExists)
                    return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                        StatusCodes.Status404NotFound, "Pasien tidak ditemukan.");

                if (!patientMembershipId.HasValue)
                {
                    var activeMembership = await _dbContext.Set<MstPatientMembership>()
                        .AsNoTracking()
                        .Include(m => m.MembershipTier)
                        .Where(m => m.PatientId == patientId.Value &&
                                    !m.IsDelete &&
                                    m.MembershipStatus == MembershipStatus.Active)
                        .OrderByDescending(m => m.CreateDateTime)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (activeMembership != null)
                    {
                        patientMembershipId = activeMembership.Id;
                        membershipTierId ??= activeMembership.MembershipTierId;
                    }
                }
            }

            Guid? ambulanceArrivalModeId = request.AmbulanceArrivalModeId.HasValue && request.AmbulanceArrivalModeId.Value != Guid.Empty
                ? request.AmbulanceArrivalModeId.Value
                : null;

            if (request.RequiresAmbulance && ambulanceArrivalModeId.HasValue)
            {
                var arrivalModeExists = await _dbContext.Set<EmgArrivalMode>()
                    .AsNoTracking()
                    .AnyAsync(a => a.Id == ambulanceArrivalModeId.Value && !a.IsDelete, cancellationToken);

                if (!arrivalModeExists)
                    return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                        StatusCodes.Status400BadRequest, "Moda ambulans tidak valid.");
            }

            var now = DateTime.UtcNow;
            var etaUtc = request.EstimatedArrivalDateTime.Kind == DateTimeKind.Utc
                ? request.EstimatedArrivalDateTime
                : request.EstimatedArrivalDateTime.ToUniversalTime();

            var reservationNumber = _documentNumberService.Generate("RES-EMG", now);

            var entity = new EmgPreArrivalReservation
            {
                Id = Guid.NewGuid(),
                ReservationNumber = reservationNumber,
                PatientId = patientId,
                PatientMembershipId = patientMembershipId,
                MembershipTierId = membershipTierId,
                CallerName = request.CallerName.Trim(),
                CallerPhoneNumber = request.CallerPhoneNumber.Trim(),
                EstimatedArrivalDateTime = etaUtc,
                ChiefComplaint = string.IsNullOrWhiteSpace(request.ChiefComplaint) ? null : request.ChiefComplaint.Trim(),
                RequiresAmbulance = request.RequiresAmbulance,
                AmbulanceArrivalModeId = ambulanceArrivalModeId,
                AmbulancePickupAddress = string.IsNullOrWhiteSpace(request.AmbulancePickupAddress) ? null : request.AmbulancePickupAddress.Trim(),
                AssignedEscortStaffName = string.IsNullOrWhiteSpace(request.AssignedEscortStaffName) ? null : request.AssignedEscortStaffName.Trim(),
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                Status = EmergencyPreArrivalStatus.PendingArrival,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false,
                RowVersion = Guid.NewGuid()
            };

            _dbContext.Set<EmgPreArrivalReservation>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var response = await ProjectToResponseAsync(entity.Id, cancellationToken);
            return Hasil<EmergencyPreArrivalReservationResponse>.Dibuat(response!);
        }

        public async Task<PagedResult<EmergencyPreArrivalReservationResponse>> GetListAsync(
            string? search,
            EmergencyPreArrivalStatus? status,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _dbContext.Set<EmgPreArrivalReservation>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            if (fromDate.HasValue)
            {
                var fromUtc = fromDate.Value.Kind == DateTimeKind.Utc ? fromDate.Value : fromDate.Value.ToUniversalTime();
                query = query.Where(x => x.EstimatedArrivalDateTime >= fromUtc);
            }

            if (toDate.HasValue)
            {
                var toUtc = toDate.Value.Kind == DateTimeKind.Utc ? toDate.Value : toDate.Value.ToUniversalTime();
                query = query.Where(x => x.EstimatedArrivalDateTime <= toUtc);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(x =>
                    x.ReservationNumber.ToLower().Contains(term) ||
                    x.CallerName.ToLower().Contains(term) ||
                    x.CallerPhoneNumber.ToLower().Contains(term) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(term)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(term)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(x => x.Status == EmergencyPreArrivalStatus.PendingArrival ? 0 : 1)
                .ThenBy(x => x.EstimatedArrivalDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new EmergencyPreArrivalReservationResponse
                {
                    Id = x.Id,
                    ReservationNumber = x.ReservationNumber,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : null,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    PatientMembershipId = x.PatientMembershipId,
                    MemberNumber = x.PatientMembership != null ? x.PatientMembership.MemberNumber : null,
                    MembershipTierId = x.MembershipTierId,
                    TierCode = x.MembershipTier != null ? x.MembershipTier.TierCode : null,
                    TierName = x.MembershipTier != null ? x.MembershipTier.TierName : null,
                    PriorityLevel = x.MembershipTier != null ? x.MembershipTier.PriorityLevel : 0,
                    CallerName = x.CallerName,
                    CallerPhoneNumber = x.CallerPhoneNumber,
                    EstimatedArrivalDateTime = x.EstimatedArrivalDateTime,
                    ChiefComplaint = x.ChiefComplaint,
                    RequiresAmbulance = x.RequiresAmbulance,
                    AmbulanceArrivalModeId = x.AmbulanceArrivalModeId,
                    AmbulanceArrivalModeName = x.AmbulanceArrivalMode != null ? x.AmbulanceArrivalMode.Name : null,
                    AmbulancePickupAddress = x.AmbulancePickupAddress,
                    AssignedEscortStaffName = x.AssignedEscortStaffName,
                    Status = x.Status,
                    StatusName = x.Status.ToString(),
                    ResultEmergencyVisitId = x.ResultEmergencyVisitId,
                    EmergencyVisitNumber = x.ResultEmergencyVisit != null ? x.ResultEmergencyVisit.EmergencyVisitNumber : null,
                    ArrivedAt = x.ArrivedAt,
                    CancelledAt = x.CancelledAt,
                    CancelledByUserId = x.CancelledByUserId,
                    CancellationReason = x.CancellationReason,
                    Notes = x.Notes,
                    CreateDateTime = x.CreateDateTime,
                    RowVersion = x.RowVersion
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<EmergencyPreArrivalReservationResponse>
            {
                Items = items,
                TotalData = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPage = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<EmergencyPreArrivalReservationResponse?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await ProjectToResponseAsync(id, cancellationToken);
        }

        public async Task<Hasil<EmergencyPreArrivalReservationResponse>> UpdateReservationAsync(
            Guid id,
            UpdateEmergencyPreArrivalReservationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _dbContext.Set<EmgPreArrivalReservation>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                    StatusCodes.Status404NotFound, "Reservasi pra-kedatangan tidak ditemukan.");

            if (entity.Status != EmergencyPreArrivalStatus.PendingArrival)
                return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                    StatusCodes.Status400BadRequest, $"Reservasi tidak dapat diubah karena sudah berstatus {entity.Status}.");

            if (string.IsNullOrWhiteSpace(request.CallerName))
                return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                    StatusCodes.Status400BadRequest, "Nama pemanggil wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.CallerPhoneNumber))
                return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                    StatusCodes.Status400BadRequest, "Nomor telepon pemanggil wajib diisi.");

            Guid? ambulanceArrivalModeId = request.AmbulanceArrivalModeId.HasValue && request.AmbulanceArrivalModeId.Value != Guid.Empty
                ? request.AmbulanceArrivalModeId.Value
                : null;

            if (request.RequiresAmbulance && ambulanceArrivalModeId.HasValue)
            {
                var arrivalModeExists = await _dbContext.Set<EmgArrivalMode>()
                    .AsNoTracking()
                    .AnyAsync(a => a.Id == ambulanceArrivalModeId.Value && !a.IsDelete, cancellationToken);

                if (!arrivalModeExists)
                    return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                        StatusCodes.Status400BadRequest, "Moda ambulans tidak valid.");
            }

            var now = DateTime.UtcNow;
            var etaUtc = request.EstimatedArrivalDateTime.Kind == DateTimeKind.Utc
                ? request.EstimatedArrivalDateTime
                : request.EstimatedArrivalDateTime.ToUniversalTime();

            entity.CallerName = request.CallerName.Trim();
            entity.CallerPhoneNumber = request.CallerPhoneNumber.Trim();
            entity.EstimatedArrivalDateTime = etaUtc;
            entity.ChiefComplaint = string.IsNullOrWhiteSpace(request.ChiefComplaint) ? null : request.ChiefComplaint.Trim();
            entity.RequiresAmbulance = request.RequiresAmbulance;
            entity.AmbulanceArrivalModeId = ambulanceArrivalModeId;
            entity.AmbulancePickupAddress = string.IsNullOrWhiteSpace(request.AmbulancePickupAddress) ? null : request.AmbulancePickupAddress.Trim();
            entity.AssignedEscortStaffName = string.IsNullOrWhiteSpace(request.AssignedEscortStaffName) ? null : request.AssignedEscortStaffName.Trim();
            entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);

            var response = await ProjectToResponseAsync(entity.Id, cancellationToken);
            return Hasil<EmergencyPreArrivalReservationResponse>.Ok(response!);
        }

        public async Task<Hasil<EmergencyPreArrivalReservationResponse>> CancelReservationAsync(
            Guid id,
            CancelEmergencyPreArrivalReservationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.CancellationReason))
                return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                    StatusCodes.Status400BadRequest, "Alasan pembatalan wajib diisi.");

            var entity = await _dbContext.Set<EmgPreArrivalReservation>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                    StatusCodes.Status404NotFound, "Reservasi pra-kedatangan tidak ditemukan.");

            if (entity.Status != EmergencyPreArrivalStatus.PendingArrival)
                return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                    StatusCodes.Status400BadRequest, $"Reservasi tidak dapat dibatalkan karena sudah berstatus {entity.Status}.");

            var now = DateTime.UtcNow;
            entity.Status = EmergencyPreArrivalStatus.Cancelled;
            entity.CancelledAt = now;
            entity.CancelledByUserId = actorUserId;
            entity.CancellationReason = request.CancellationReason.Trim();
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);

            var response = await ProjectToResponseAsync(entity.Id, cancellationToken);
            return Hasil<EmergencyPreArrivalReservationResponse>.Ok(response!);
        }

        public const string PesanBelumTerdaftarDiLoket =
            "Pasien belum didaftarkan di loket IGD. Daftarkan di loket lebih dulu, lalu ulangi Konfirmasi Tiba.";

        private const string PesanKunjunganEncounterPernahDihapus =
            "Kunjungan IGD untuk encounter ini pernah dihapus, sehingga encounter ini tidak dapat dimulai lagi. Daftarkan ulang pasien.";

        public async Task<Hasil<EmergencyPreArrivalReservationResponse>> CheckInReservationAsync(
            Guid id,
            CheckInEmergencyPreArrivalReservationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _dbContext.Set<EmgPreArrivalReservation>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                    StatusCodes.Status404NotFound, "Reservasi pra-kedatangan tidak ditemukan.");

            if (entity.Status != EmergencyPreArrivalStatus.PendingArrival)
                return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                    StatusCodes.Status400BadRequest, $"Reservasi tidak dapat di-check-in karena sudah berstatus {entity.Status}.");

            if (!entity.PatientId.HasValue || entity.PatientId.Value == Guid.Empty)
                return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                    StatusCodes.Status400BadRequest, "Data rekam medis pasien wajib ditautkan sebelum melakukan konfirmasi kedatangan (check-in).");

            var now = DateTime.UtcNow;
            var arrivalDateTime = request.ArrivalDateTime.HasValue
                ? NormalizeUtc(request.ArrivalDateTime.Value)
                : now;

            if (arrivalDateTime > now)
                return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                    StatusCodes.Status400BadRequest, "Waktu tiba tidak boleh melewati waktu sekarang.");

            Guid? arrivalModeId = request.ArrivalModeId.HasValue && request.ArrivalModeId.Value != Guid.Empty
                ? request.ArrivalModeId.Value
                : entity.AmbulanceArrivalModeId;

            if (!arrivalModeId.HasValue)
            {
                var defaultArrivalMode = await _dbContext.Set<EmgArrivalMode>()
                    .AsNoTracking()
                    .Where(a => a.IsActive && !a.IsDelete)
                    .OrderBy(a => a.Sequence)
                    .FirstOrDefaultAsync(cancellationToken);

                arrivalModeId = defaultArrivalMode?.Id;
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await EmergencyEpisodeRule.LockPatientEpisodeAsync(_dbContext, entity.PatientId.Value, cancellationToken);

            var openEpisode = await EmergencyEpisodeRule.FindOpenEpisodeAsync(
                _dbContext,
                entity.PatientId.Value,
                null,
                cancellationToken);

            if (openEpisode is { Kind: EmergencyEpisodeRule.OpenEpisodeKind.Visit, Visit: { } activeVisit })
                return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                    StatusCodes.Status409Conflict, EmergencyVisitService.PesanKunjunganLainBerjalan(activeVisit));

            if (openEpisode is not { Kind: EmergencyEpisodeRule.OpenEpisodeKind.Encounter, Encounter: { } encounter })
                return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                    StatusCodes.Status409Conflict, PesanBelumTerdaftarDiLoket);

            var kunjunganEncounter = await _dbContext.Set<EmgVisit>()
                .AsNoTracking()
                .Where(x => x.EncounterId == encounter.Id)
                .Select(x => new { x.IsDelete, x.EmergencyVisitNumber })
                .FirstOrDefaultAsync(cancellationToken);

            if (kunjunganEncounter != null)
                return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                    StatusCodes.Status409Conflict,
                    kunjunganEncounter.IsDelete
                        ? PesanKunjunganEncounterPernahDihapus
                        : $"Encounter IGD pasien ini sudah memiliki kunjungan {kunjunganEncounter.EmergencyVisitNumber}. Buka kunjungan tersebut, jangan mendaftar ulang.");

            var visit = new EmgVisit
            {
                Id = Guid.NewGuid(),
                EmergencyVisitNumber = await _emergencyVisitService.GenerateVisitNumberAsync(now, cancellationToken),
                EncounterId = encounter.Id,
                PatientId = encounter.PatientId,
                ServiceUnitId = encounter.ServiceUnitId,
                ArrivalModeId = arrivalModeId,
                CaseTypeId = request.CaseTypeId.HasValue && request.CaseTypeId.Value != Guid.Empty ? request.CaseTypeId.Value : null,
                ArrivalDateTime = arrivalDateTime,
                ArrivalTimeSource = EmergencyArrivalTimeSource.Confirmed,
                ArrivalConfirmedAt = now,
                ArrivalConfirmedByUserId = actorUserId == Guid.Empty ? null : actorUserId,
                ChiefComplaint = !string.IsNullOrWhiteSpace(request.ChiefComplaint)
                    ? request.ChiefComplaint.Trim()
                    : entity.ChiefComplaint,
                ArrivalLocation = request.ArrivalLocation?.Trim(),
                IsImmediateCareAllowed = true,
                VisitStatus = EmergencyVisitStatus.WaitingForTriage,
                RegistrationStatus = EmergencyRegistrationStatus.Registered,
                RegistrationCompletedAt = NormalizeUtc(encounter.RegisteredAt),
                Notes = request.Notes?.Trim() ?? entity.Notes,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<EmgVisit>().Add(visit);

            entity.Status = EmergencyPreArrivalStatus.Arrived;
            entity.ArrivedAt = arrivalDateTime;
            entity.ResultEmergencyVisitId = visit.Id;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.RowVersion = Guid.NewGuid();

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                return Hasil<EmergencyPreArrivalReservationResponse>.Gagal(
                    StatusCodes.Status409Conflict,
                    "Pasien ini baru saja mulai ditangani lewat kunjungan IGD lain. Buka kunjungan tersebut, jangan mendaftar ulang.");
            }

            await transaction.CommitAsync(cancellationToken);

            var response = await ProjectToResponseAsync(entity.Id, cancellationToken);
            return Hasil<EmergencyPreArrivalReservationResponse>.Ok(response!);
        }

        private static DateTime NormalizeUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        private async Task<EmergencyPreArrivalReservationResponse?> ProjectToResponseAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<EmgPreArrivalReservation>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new EmergencyPreArrivalReservationResponse
                {
                    Id = x.Id,
                    ReservationNumber = x.ReservationNumber,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : null,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    PatientMembershipId = x.PatientMembershipId,
                    MemberNumber = x.PatientMembership != null ? x.PatientMembership.MemberNumber : null,
                    MembershipTierId = x.MembershipTierId,
                    TierCode = x.MembershipTier != null ? x.MembershipTier.TierCode : null,
                    TierName = x.MembershipTier != null ? x.MembershipTier.TierName : null,
                    PriorityLevel = x.MembershipTier != null ? x.MembershipTier.PriorityLevel : 0,
                    CallerName = x.CallerName,
                    CallerPhoneNumber = x.CallerPhoneNumber,
                    EstimatedArrivalDateTime = x.EstimatedArrivalDateTime,
                    ChiefComplaint = x.ChiefComplaint,
                    RequiresAmbulance = x.RequiresAmbulance,
                    AmbulanceArrivalModeId = x.AmbulanceArrivalModeId,
                    AmbulanceArrivalModeName = x.AmbulanceArrivalMode != null ? x.AmbulanceArrivalMode.Name : null,
                    AmbulancePickupAddress = x.AmbulancePickupAddress,
                    AssignedEscortStaffName = x.AssignedEscortStaffName,
                    Status = x.Status,
                    StatusName = x.Status.ToString(),
                    ResultEmergencyVisitId = x.ResultEmergencyVisitId,
                    EmergencyVisitNumber = x.ResultEmergencyVisit != null ? x.ResultEmergencyVisit.EmergencyVisitNumber : null,
                    ArrivedAt = x.ArrivedAt,
                    CancelledAt = x.CancelledAt,
                    CancelledByUserId = x.CancelledByUserId,
                    CancellationReason = x.CancellationReason,
                    Notes = x.Notes,
                    CreateDateTime = x.CreateDateTime,
                    RowVersion = x.RowVersion
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
