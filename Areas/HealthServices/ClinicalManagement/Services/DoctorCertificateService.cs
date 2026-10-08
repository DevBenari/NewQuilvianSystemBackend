using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// RJ-DOC-REV-BE-004 — surat dokter rawat jalan. Pemilik validasi, nomor surat, snapshot,
    /// riwayat, dan pembatalan. Dokter pada surat selalu Dokter Penanggung Jawab kunjungan.
    /// </summary>
    public class DoctorCertificateService
    {
        private const string LogCategory = "DoctorCertificate";
        private const int SequenceDigits = 6;

        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly LoggerService _loggerService;

        public DoctorCertificateService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator numberSeriesAllocator,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _numberSeriesAllocator = numberSeriesAllocator;
            _loggerService = loggerService;
        }

        public sealed record Result(int StatusCode, string Message, DoctorCertificateResponse? Data = null)
        {
            public bool Success => StatusCode is >= 200 and < 300;
        }

        public static string ToTypeKey(DoctorCertificateType type) => type switch
        {
            DoctorCertificateType.Health => "health",
            DoctorCertificateType.InpatientReferral => "inpatientReferral",
            _ => "sickLeave"
        };

        public static DoctorCertificateType? ParseType(string? value) => value?.Trim().ToLowerInvariant() switch
        {
            "sickleave" or "sick_leave" or "1" => DoctorCertificateType.SickLeave,
            "health" or "2" => DoctorCertificateType.Health,
            "inpatientreferral" or "referral" or "3" => DoctorCertificateType.InpatientReferral,
            _ => null
        };

        private static (string Key, string Prefix) SequenceOf(DoctorCertificateType type) => type switch
        {
            DoctorCertificateType.Health => ("CLI_DOCTOR_CERT_HEALTH", "SKH"),
            DoctorCertificateType.InpatientReferral => ("CLI_DOCTOR_CERT_REFERRAL", "SRJ"),
            _ => ("CLI_DOCTOR_CERT_SICK", "SKS")
        };

        public async Task<PagedResult<DoctorCertificateResponse>> GetListAsync(
            Guid? patientId,
            Guid? encounterId,
            string? certificateType,
            bool includeCancelled,
            int pageNumber,
            int pageSize,
            CancellationToken ct)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

            var query = _dbContext.CliDoctorCertificates.AsNoTracking().Where(x => !x.IsDelete);

            if (patientId.HasValue) query = query.Where(x => x.PatientId == patientId.Value);
            if (encounterId.HasValue) query = query.Where(x => x.EncounterId == encounterId.Value);
            if (ParseType(certificateType) is { } type) query = query.Where(x => x.CertificateType == type);
            if (!includeCancelled) query = query.Where(x => x.CertificateStatus != DoctorCertificateStatus.Cancelled);

            var total = await query.CountAsync(ct);
            var rows = await query
                .OrderByDescending(x => x.IssuedDate)
                .ThenByDescending(x => x.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var encounterNumbers = await LoadEncounterNumbersAsync(rows.Select(x => x.EncounterId), ct);

            return new PagedResult<DoctorCertificateResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = rows.Select(x => ToResponse(x, encounterNumbers, includeSignature: false)).ToList()
            };
        }

        public async Task<DoctorCertificateResponse?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var entity = await _dbContext.CliDoctorCertificates.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (entity == null) return null;

            var numbers = await LoadEncounterNumbersAsync(new[] { entity.EncounterId }, ct);
            return ToResponse(entity, numbers, includeSignature: true);
        }

        public async Task<DoctorCertificateResponse?> GetActiveByQueueAsync(Guid queueId, CancellationToken ct)
        {
            var entity = await _dbContext.CliDoctorCertificates.AsNoTracking()
                .Where(x => x.QueueId == queueId && !x.IsDelete && x.CertificateStatus == DoctorCertificateStatus.Issued)
                .OrderByDescending(x => x.UpdateDateTime ?? x.CreateDateTime)
                .FirstOrDefaultAsync(ct);
            if (entity == null) return null;

            var numbers = await LoadEncounterNumbersAsync(new[] { entity.EncounterId }, ct);
            return ToResponse(entity, numbers, includeSignature: true);
        }

        public async Task<List<DoctorCertificateReferralTargetOption>> GetReferralTargetsAsync(
            string? search,
            int take,
            CancellationToken ct)
        {
            take = take is < 1 or > 100 ? 30 : take;
            var keyword = search?.Trim().ToLower();

            var units = _dbContext.MstServiceUnits.AsNoTracking().Where(x => !x.IsDelete && x.IsActive);
            var clinics = _dbContext.MstClinics.AsNoTracking().Where(x => !x.IsDelete && x.IsActive);

            if (!string.IsNullOrEmpty(keyword))
            {
                units = units.Where(x => x.ServiceUnitName.ToLower().Contains(keyword) || x.ServiceUnitCode.ToLower().Contains(keyword));
                clinics = clinics.Where(x => x.ClinicName.ToLower().Contains(keyword) || x.ClinicCode.ToLower().Contains(keyword)
                    || (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitName.ToLower().Contains(keyword)));
            }

            var unitRows = await units.OrderBy(x => x.ServiceUnitName).Take(take)
                .Select(x => new DoctorCertificateReferralTargetOption
                {
                    Kind = "serviceUnit",
                    Id = x.Id,
                    Code = x.ServiceUnitCode,
                    Name = x.ServiceUnitName
                })
                .ToListAsync(ct);

            var clinicRows = await clinics.OrderBy(x => x.ClinicName).Take(take)
                .Select(x => new DoctorCertificateReferralTargetOption
                {
                    Kind = "clinic",
                    Id = x.Id,
                    Code = x.ClinicCode,
                    Name = x.ClinicName,
                    ParentName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null
                })
                .ToListAsync(ct);

            return unitRows.Concat(clinicRows).Take(take).ToList();
        }

        public async Task<Result> CreateAsync(SaveDoctorCertificateRequest request, Guid actorUserId, CancellationToken ct)
        {
            var validation = await ValidateAsync(request, ct);
            if (validation.Error != null) return validation.Error;

            var type = validation.Type!.Value;
            var encounter = validation.Encounter!;

            var entity = new CliDoctorCertificate
            {
                CertificateType = type,
                CertificateStatus = DoctorCertificateStatus.Issued,
                PatientId = encounter.PatientId,
                EncounterId = encounter.Id,
                QueueId = validation.QueueId,
                ConsultationId = request.ConsultationId,
                ServiceUnitId = encounter.ServiceUnitId,
                ClinicId = encounter.ClinicId,
                IsActive = true,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };

            await ApplyAsync(entity, request, validation, ct);

            // Nomor diminta paling akhir supaya deret tidak berlubang oleh permintaan yang ditolak.
            var (sequenceKey, prefix) = SequenceOf(type);
            try
            {
                entity.CertificateNumber = await _numberSeriesAllocator.AllocateAsync(
                    new NumberAllocationRequest(
                        SequenceKey: sequenceKey,
                        Prefix: prefix,
                        ResetPolicy: NumberSeriesResetPolicies.Never,
                        SequenceDigits: SequenceDigits,
                        ActorUserId: actorUserId,
                        Instant: DateTimeOffset.UtcNow),
                    ct);
            }
            catch (NumberSeriesAllocationException)
            {
                return new Result(StatusCodes.Status500InternalServerError,
                    "Nomor surat dokter gagal diterbitkan. Coba simpan ulang beberapa saat lagi.");
            }

            _dbContext.CliDoctorCertificates.Add(entity);
            await _dbContext.SaveChangesAsync(ct);

            await _loggerService.AuditAsync(LogCategory, "DoctorCertificate.Create", "Menerbitkan surat dokter.", new
            {
                entity.Id,
                entity.CertificateNumber,
                CertificateType = entity.CertificateType.ToString(),
                entity.EncounterId,
                ActorUserId = actorUserId
            });

            return new Result(StatusCodes.Status200OK, "Surat dokter berhasil diterbitkan.",
                await GetByIdAsync(entity.Id, ct));
        }

        public async Task<Result> UpdateAsync(Guid id, SaveDoctorCertificateRequest request, Guid actorUserId, CancellationToken ct)
        {
            var entity = await _dbContext.CliDoctorCertificates.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (entity == null)
                return new Result(StatusCodes.Status404NotFound, "Surat dokter tidak ditemukan.");

            if (entity.CertificateStatus == DoctorCertificateStatus.Cancelled)
                return new Result(StatusCodes.Status400BadRequest, "Surat dokter yang sudah dibatalkan tidak dapat diubah.");

            var validation = await ValidateAsync(request, ct);
            if (validation.Error != null) return validation.Error;

            if (validation.Encounter!.Id != entity.EncounterId)
                return new Result(StatusCodes.Status400BadRequest, "Surat dokter tidak boleh dipindah ke kunjungan lain.");

            if (validation.Type != entity.CertificateType)
                return new Result(StatusCodes.Status400BadRequest,
                    "Jenis surat tidak dapat diubah. Batalkan surat ini lalu terbitkan surat baru.");

            await ApplyAsync(entity, request, validation, ct);
            entity.ConsultationId = request.ConsultationId ?? entity.ConsultationId;
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(ct);

            await _loggerService.AuditAsync(LogCategory, "DoctorCertificate.Update", "Mengubah surat dokter.", new
            {
                entity.Id,
                entity.CertificateNumber,
                ActorUserId = actorUserId
            });

            return new Result(StatusCodes.Status200OK, "Surat dokter berhasil diubah.", await GetByIdAsync(entity.Id, ct));
        }

        public async Task<Result> CancelAsync(Guid id, CancelDoctorCertificateRequest request, Guid actorUserId, CancellationToken ct)
        {
            var entity = await _dbContext.CliDoctorCertificates.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (entity == null)
                return new Result(StatusCodes.Status404NotFound, "Surat dokter tidak ditemukan.");

            if (entity.CertificateStatus == DoctorCertificateStatus.Cancelled)
                return new Result(StatusCodes.Status400BadRequest, "Surat dokter sudah dibatalkan.");

            var reason = request.Reason?.Trim();
            if (string.IsNullOrEmpty(reason))
                return new Result(StatusCodes.Status400BadRequest, "Alasan pembatalan wajib diisi.");

            var now = DateTime.UtcNow;
            entity.CertificateStatus = DoctorCertificateStatus.Cancelled;
            entity.CancelReason = reason;
            entity.IsCancel = true;
            entity.CancelBy = actorUserId;
            entity.CancelDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(ct);

            await _loggerService.AuditAsync(LogCategory, "DoctorCertificate.Cancel", "Membatalkan surat dokter.", new
            {
                entity.Id,
                entity.CertificateNumber,
                ActorUserId = actorUserId
            });

            return new Result(StatusCodes.Status200OK, "Surat dokter berhasil dibatalkan.", await GetByIdAsync(entity.Id, ct));
        }

        private sealed class Validation
        {
            public Result? Error { get; init; }
            public DoctorCertificateType? Type { get; init; }
            public QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models.RegPatientEncounter? Encounter { get; init; }
            public Guid? QueueId { get; init; }
            public string? ReferralTargetName { get; init; }
        }

        private async Task<Validation> ValidateAsync(SaveDoctorCertificateRequest request, CancellationToken ct)
        {
            static Validation Fail(string message) =>
                new() { Error = new Result(StatusCodes.Status400BadRequest, message) };

            var type = ParseType(request.CertificateType);
            if (type == null) return Fail("Jenis surat dokter tidak dikenali.");

            if (!request.EncounterId.HasValue || request.EncounterId == Guid.Empty)
                return Fail("Kunjungan wajib diisi.");

            var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.EncounterId.Value && !x.IsDelete, ct);
            if (encounter == null)
                return new Validation { Error = new Result(StatusCodes.Status404NotFound, "Kunjungan tidak ditemukan.") };

            Guid? queueId = null;
            if (request.QueueId.HasValue && request.QueueId != Guid.Empty)
            {
                var queueMatches = await _dbContext.RegQueues.AsNoTracking()
                    .AnyAsync(x => x.Id == request.QueueId.Value && x.EncounterId == encounter.Id && !x.IsDelete, ct);
                if (!queueMatches) return Fail("Antrean tidak sesuai dengan kunjungan.");
                queueId = request.QueueId;
            }

            string? referralTargetName = null;
            switch (type)
            {
                case DoctorCertificateType.SickLeave:
                {
                    var sick = request.SickLeave;
                    if (sick?.StartDate == null || sick.EndDate == null)
                        return Fail("Tanggal mulai dan selesai istirahat wajib diisi.");
                    if (sick.EndDate.Value.Date < sick.StartDate.Value.Date)
                        return Fail("Tanggal selesai istirahat tidak boleh sebelum tanggal mulai.");
                    break;
                }
                case DoctorCertificateType.Health:
                    if (request.Health == null)
                        return Fail("Data pemeriksaan surat sehat wajib diisi.");
                    break;
                case DoctorCertificateType.InpatientReferral:
                {
                    var referral = request.InpatientReferral;
                    if (referral == null) return Fail("Data rujukan wajib diisi.");

                    if (referral.TargetClinicId is { } clinicId && clinicId != Guid.Empty)
                    {
                        referralTargetName = await _dbContext.MstClinics.AsNoTracking()
                            .Where(x => x.Id == clinicId && !x.IsDelete && x.IsActive)
                            .Select(x => x.ClinicName)
                            .FirstOrDefaultAsync(ct);
                        if (referralTargetName == null) return Fail("Klinik tujuan rujukan tidak ditemukan atau tidak aktif.");
                    }
                    else if (referral.TargetServiceUnitId is { } unitId && unitId != Guid.Empty)
                    {
                        referralTargetName = await _dbContext.MstServiceUnits.AsNoTracking()
                            .Where(x => x.Id == unitId && !x.IsDelete && x.IsActive)
                            .Select(x => x.ServiceUnitName)
                            .FirstOrDefaultAsync(ct);
                        if (referralTargetName == null) return Fail("Unit tujuan rujukan tidak ditemukan atau tidak aktif.");
                    }
                    else
                    {
                        return Fail("Unit/Tujuan rujukan wajib dipilih dari daftar.");
                    }
                    break;
                }
            }

            return new Validation
            {
                Type = type,
                Encounter = encounter,
                QueueId = queueId,
                ReferralTargetName = referralTargetName
            };
        }

        private async Task ApplyAsync(
            CliDoctorCertificate entity,
            SaveDoctorCertificateRequest request,
            Validation validation,
            CancellationToken ct)
        {
            var encounter = validation.Encounter!;

            var patient = await _dbContext.MstPatients.AsNoTracking()
                .Where(x => x.Id == encounter.PatientId)
                .Select(x => new { x.FullName, x.MedicalRecordNumber, x.BirthDate, x.Address })
                .FirstOrDefaultAsync(ct);

            // Dokter pada surat = Dokter Penanggung Jawab kunjungan, bukan nilai kiriman klien.
            var doctor = encounter.DoctorId.HasValue
                ? await _dbContext.MstDoctors.AsNoTracking()
                    .Where(x => x.Id == encounter.DoctorId.Value)
                    .Select(x => new { x.Id, x.FullName })
                    .FirstOrDefaultAsync(ct)
                : null;

            var clinicName = encounter.ClinicId.HasValue
                ? await _dbContext.MstClinics.AsNoTracking()
                    .Where(x => x.Id == encounter.ClinicId.Value)
                    .Select(x => x.ClinicName)
                    .FirstOrDefaultAsync(ct)
                : null;

            entity.DoctorId = doctor?.Id;
            entity.DoctorNameSnapshot = doctor?.FullName;
            entity.ClinicNameSnapshot = clinicName;
            entity.DoctorSipSnapshot = Clean(request.DoctorSip);
            entity.DoctorSignatureDataUrl = Clean(request.DoctorSignatureDataUrl);

            entity.PatientNameSnapshot = Clean(request.PatientName) ?? patient?.FullName ?? string.Empty;
            entity.MedicalRecordNumberSnapshot = patient?.MedicalRecordNumber;
            entity.BirthDateSnapshot = AsUtcDate(request.BirthDate) ?? AsUtcDate(patient?.BirthDate);
            entity.GenderSnapshot = Clean(request.Gender);
            entity.AddressSnapshot = Clean(request.Address) ?? Clean(patient?.Address);
            entity.OccupationSnapshot = Clean(request.Occupation);

            entity.IssuedDate = AsUtcDate(request.IssuedDate) ?? AsUtcDate(DateTime.UtcNow)!.Value;
            entity.Purpose = Clean(request.Purpose);
            entity.Diagnosis = Clean(request.Diagnosis);
            entity.ClinicalSummary = Clean(request.ClinicalSummary);
            entity.AdditionalNote = Clean(request.AdditionalNote);

            var sick = entity.CertificateType == DoctorCertificateType.SickLeave ? request.SickLeave : null;
            entity.SickStartDate = AsUtcDate(sick?.StartDate);
            entity.SickEndDate = AsUtcDate(sick?.EndDate);
            entity.SickDurationDays = sick?.StartDate != null && sick.EndDate != null
                ? (int)(sick.EndDate.Value.Date - sick.StartDate.Value.Date).TotalDays + 1
                : null;
            entity.ActivityRestriction = Clean(sick?.ActivityRestriction);

            var health = entity.CertificateType == DoctorCertificateType.Health ? request.Health : null;
            entity.ExaminationDate = AsUtcDate(health?.ExaminationDate);
            entity.HealthConclusion = Clean(health?.Conclusion);
            entity.BloodPressure = Clean(health?.BloodPressure);
            entity.Pulse = Clean(health?.Pulse);
            entity.Temperature = Clean(health?.Temperature);
            entity.Weight = Clean(health?.Weight);
            entity.Height = Clean(health?.Height);
            entity.ColorBlindResult = Clean(health?.ColorBlindResult);
            entity.HealthRecommendation = Clean(health?.Recommendation);

            var referral = entity.CertificateType == DoctorCertificateType.InpatientReferral ? request.InpatientReferral : null;
            entity.ReferralAdmissionDate = AsUtcDate(referral?.AdmissionDate);
            entity.ReferralDiagnosis = Clean(referral?.Diagnosis);
            entity.ReferralReason = Clean(referral?.Reason);
            entity.ReferralTargetClinicId = referral?.TargetClinicId is { } c && c != Guid.Empty ? c : null;
            entity.ReferralTargetServiceUnitId = entity.ReferralTargetClinicId == null && referral?.TargetServiceUnitId is { } u && u != Guid.Empty ? u : null;
            entity.ReferralTargetNameSnapshot = validation.ReferralTargetName;
            entity.RequestedRoomClass = Clean(referral?.RequestedRoomClass);
            entity.SpecialInstruction = Clean(referral?.SpecialInstruction);
        }

        private async Task<Dictionary<Guid, string>> LoadEncounterNumbersAsync(IEnumerable<Guid> encounterIds, CancellationToken ct)
        {
            var ids = encounterIds.Distinct().ToList();
            return await _dbContext.RegPatientEncounters.AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.EncounterNumber, ct);
        }

        private static DoctorCertificateResponse ToResponse(
            CliDoctorCertificate x,
            IReadOnlyDictionary<Guid, string> encounterNumbers,
            bool includeSignature) => new()
        {
            Id = x.Id,
            CertificateNumber = x.CertificateNumber,
            CertificateType = ToTypeKey(x.CertificateType),
            CertificateTypeName = x.CertificateType switch
            {
                DoctorCertificateType.Health => "Surat Keterangan Sehat",
                DoctorCertificateType.InpatientReferral => "Surat Rujukan",
                _ => "Surat Keterangan Sakit"
            },
            CertificateStatus = (int)x.CertificateStatus,
            CertificateStatusName = x.CertificateStatus == DoctorCertificateStatus.Cancelled ? "Dibatalkan" : "Terbit",
            PatientId = x.PatientId,
            EncounterId = x.EncounterId,
            EncounterNumber = encounterNumbers.TryGetValue(x.EncounterId, out var number) ? number : null,
            QueueId = x.QueueId,
            ConsultationId = x.ConsultationId,
            DoctorId = x.DoctorId,
            ClinicId = x.ClinicId,
            IssuedDate = x.IssuedDate,
            Purpose = x.Purpose,
            Diagnosis = x.Diagnosis,
            ClinicalSummary = x.ClinicalSummary,
            AdditionalNote = x.AdditionalNote,
            PatientName = x.PatientNameSnapshot,
            MedicalRecordNumber = x.MedicalRecordNumberSnapshot,
            BirthDate = x.BirthDateSnapshot,
            Gender = x.GenderSnapshot,
            Address = x.AddressSnapshot,
            Occupation = x.OccupationSnapshot,
            DoctorName = x.DoctorNameSnapshot,
            DoctorSip = x.DoctorSipSnapshot,
            ClinicName = x.ClinicNameSnapshot,
            DoctorSignatureDataUrl = includeSignature ? x.DoctorSignatureDataUrl : null,
            SickLeave = x.CertificateType == DoctorCertificateType.SickLeave
                ? new DoctorCertificateSickLeaveRequest
                {
                    StartDate = x.SickStartDate,
                    EndDate = x.SickEndDate,
                    DurationDays = x.SickDurationDays,
                    ActivityRestriction = x.ActivityRestriction
                }
                : null,
            Health = x.CertificateType == DoctorCertificateType.Health
                ? new DoctorCertificateHealthRequest
                {
                    ExaminationDate = x.ExaminationDate,
                    Conclusion = x.HealthConclusion,
                    BloodPressure = x.BloodPressure,
                    Pulse = x.Pulse,
                    Temperature = x.Temperature,
                    Weight = x.Weight,
                    Height = x.Height,
                    ColorBlindResult = x.ColorBlindResult,
                    Recommendation = x.HealthRecommendation
                }
                : null,
            InpatientReferral = x.CertificateType == DoctorCertificateType.InpatientReferral
                ? new DoctorCertificateReferralResponse
                {
                    AdmissionDate = x.ReferralAdmissionDate,
                    Diagnosis = x.ReferralDiagnosis,
                    Reason = x.ReferralReason,
                    TargetServiceUnitId = x.ReferralTargetServiceUnitId,
                    TargetClinicId = x.ReferralTargetClinicId,
                    TargetServiceUnit = x.ReferralTargetNameSnapshot,
                    RequestedRoomClass = x.RequestedRoomClass,
                    SpecialInstruction = x.SpecialInstruction
                }
                : null,
            CancelReason = x.CancelReason,
            CreateDateTime = x.CreateDateTime,
            UpdateDateTime = x.UpdateDateTime
        };

        /// <summary>Tanggal kalender disimpan sebagai tengah malam UTC (kolom timestamptz).</summary>
        private static DateTime? AsUtcDate(DateTime? value) =>
            value.HasValue ? DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Utc) : null;

        private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
