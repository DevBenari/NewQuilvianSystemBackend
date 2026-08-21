using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    /// <summary>
    /// Aturan bisnis kunjungan IGD: OP sebagai jenis kunjungan, IGD sebagai asal/unit,
    /// pasien sementara, registrasi provisional, dan transisi status kunjungan.
    /// </summary>
    public class EmergencyVisitService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly EmergencyDocumentNumberService _documentNumberService;

        public EmergencyVisitService(
            ApplicationDbContext dbContext,
            EmergencyDocumentNumberService documentNumberService)
        {
            _dbContext = dbContext;
            _documentNumberService = documentNumberService;
        }

        public async Task<MstEmergencySetting?> GetActiveSettingAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<MstEmergencySetting>()
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<string?> ValidateRequestAsync(
            CreateEmergencyVisitRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.ServiceUnitId == Guid.Empty)
                return "ServiceUnitId wajib diisi.";

            if (!Enum.IsDefined(typeof(EmergencyRegistrationStatus), request.RegistrationStatus))
                return "Nilai RegistrationStatus tidak valid.";

            if (!Enum.IsDefined(typeof(EmergencyVisitStatus), request.VisitStatus))
                return "Nilai VisitStatus tidak valid.";

            var setting = await GetActiveSettingAsync(cancellationToken);
            if (setting == null)
                return "Setting IGD aktif belum tersedia.";

            if (request.ServiceUnitId != setting.DefaultEmergencyServiceUnitId)
                return "Asal kunjungan harus IGD. ServiceUnitId harus sama dengan DefaultEmergencyServiceUnitId pada setting IGD aktif.";

            if (request.IsUnknownPatient && !setting.AllowUnknownPatient)
                return "Setting IGD tidak mengizinkan pendaftaran pasien tanpa identitas.";

            if (request.RegistrationStatus == EmergencyRegistrationStatus.Provisional &&
                !setting.AllowProvisionalRegistration)
                return "Setting IGD tidak mengizinkan registrasi provisional.";

            if (!request.IsUnknownPatient &&
                (!request.PatientId.HasValue || request.PatientId.Value == Guid.Empty))
                return "PatientId wajib diisi untuk pasien yang sudah dikenal.";

            if (request.IsUnknownPatient && string.IsNullOrWhiteSpace(request.TemporaryPatientAlias))
                return "TemporaryPatientAlias wajib diisi untuk pasien yang belum diketahui identitasnya.";

            if ((request.RegistrationStatus == EmergencyRegistrationStatus.Registered ||
                 request.RegistrationStatus == EmergencyRegistrationStatus.Completed) &&
                (!request.EncounterId.HasValue || request.EncounterId.Value == Guid.Empty))
                return "EncounterId wajib tersedia ketika registrasi IGD sudah terdaftar atau selesai.";

            if (!await _dbContext.Set<MstServiceUnit>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.ServiceUnitId && !x.IsDelete, cancellationToken))
                return "ServiceUnitId tidak ditemukan.";

            if (request.EncounterId.HasValue && request.EncounterId.Value != Guid.Empty)
            {
                var encounter = await _dbContext.Set<TrxPatientEncounter>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == request.EncounterId.Value && !x.IsDelete,
                        cancellationToken);

                if (encounter == null)
                    return "EncounterId tidak ditemukan.";

                if (encounter.EncounterType != EncounterType.Outpatient)
                    return "Jenis kunjungan pasien IGD harus OP (EncounterType.Outpatient).";

                if (encounter.ServiceUnitId != request.ServiceUnitId)
                    return "ServiceUnitId kunjungan IGD harus sama dengan ServiceUnitId pada encounter.";

                if (request.PatientId.HasValue &&
                    request.PatientId.Value != Guid.Empty &&
                    encounter.PatientId != request.PatientId.Value)
                    return "PatientId tidak sesuai dengan pasien pada encounter.";
            }

            if (request.PatientId.HasValue &&
                request.PatientId.Value != Guid.Empty &&
                !await _dbContext.Set<MstPatient>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.PatientId.Value && !x.IsDelete, cancellationToken))
                return "PatientId tidak ditemukan.";

            if (request.ArrivalModeId.HasValue &&
                request.ArrivalModeId.Value != Guid.Empty &&
                !await _dbContext.Set<MstEmergencyArrivalMode>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.ArrivalModeId.Value && !x.IsDelete, cancellationToken))
                return "ArrivalModeId tidak ditemukan.";

            if (request.CaseTypeId.HasValue &&
                request.CaseTypeId.Value != Guid.Empty &&
                !await _dbContext.Set<MstEmergencyCaseType>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.CaseTypeId.Value && !x.IsDelete, cancellationToken))
                return "CaseTypeId tidak ditemukan.";

            return null;
        }

        public Task<string?> ValidateRequestAsync(
            UpdateEmergencyVisitRequest request,
            CancellationToken cancellationToken = default)
            => ValidateRequestAsync((CreateEmergencyVisitRequest)request, cancellationToken);

        public bool CanTransition(
            EmergencyRegistrationStatus current,
            EmergencyRegistrationStatus target)
        {
            if (current == target)
                return true;

            return current switch
            {
                EmergencyRegistrationStatus.Pending => target is EmergencyRegistrationStatus.Provisional
                    or EmergencyRegistrationStatus.Registered
                    or EmergencyRegistrationStatus.Cancelled,
                EmergencyRegistrationStatus.Provisional => target is EmergencyRegistrationStatus.Registered
                    or EmergencyRegistrationStatus.Completed
                    or EmergencyRegistrationStatus.Cancelled,
                EmergencyRegistrationStatus.Registered => target is EmergencyRegistrationStatus.Completed
                    or EmergencyRegistrationStatus.Cancelled,
                _ => false
            };
        }

        public bool CanTransition(EmergencyVisitStatus current, EmergencyVisitStatus target)
        {
            // Penyelesaian klinis bersifat final. Diperiksa sebelum jalan pintas status-sama
            // di bawah, supaya Completed ke Completed pun ikut tertolak.
            if (current == EmergencyVisitStatus.Completed)
                return false;

            if (current == target)
                return true;

            return current switch
            {
                EmergencyVisitStatus.Arrived => target is EmergencyVisitStatus.WaitingForTriage
                    or EmergencyVisitStatus.InTreatment
                    or EmergencyVisitStatus.Cancelled,
                EmergencyVisitStatus.WaitingForTriage => target is EmergencyVisitStatus.Triaged
                    or EmergencyVisitStatus.InTreatment
                    or EmergencyVisitStatus.Cancelled,
                EmergencyVisitStatus.Triaged => target is EmergencyVisitStatus.InTreatment
                    or EmergencyVisitStatus.UnderObservation
                    or EmergencyVisitStatus.AwaitingDisposition
                    or EmergencyVisitStatus.Cancelled,
                EmergencyVisitStatus.InTreatment => target is EmergencyVisitStatus.UnderObservation
                    or EmergencyVisitStatus.AwaitingDisposition
                    or EmergencyVisitStatus.Cancelled,
                EmergencyVisitStatus.UnderObservation => target is EmergencyVisitStatus.InTreatment
                    or EmergencyVisitStatus.AwaitingDisposition
                    or EmergencyVisitStatus.Cancelled,
                EmergencyVisitStatus.AwaitingDisposition => target is EmergencyVisitStatus.Disposed
                    or EmergencyVisitStatus.InTreatment
                    or EmergencyVisitStatus.Cancelled,
                // Sah menurut state matrix, tetapi closure gate-nya hanya ditegakkan oleh
                // PATCH /{id}/complete. UpdateStatus menolak target ini secara terpisah.
                EmergencyVisitStatus.Disposed => target is EmergencyVisitStatus.Completed,
                _ => false
            };
        }

        public async Task<string> GenerateVisitNumberAsync(
            DateTime now,
            CancellationToken cancellationToken = default)
        {
            var setting = await GetActiveSettingAsync(cancellationToken);
            var prefix = setting?.EmergencyVisitNumberPrefix ?? "IGD";

            for (var attempt = 0; attempt < 10; attempt++)
            {
                var number = _documentNumberService.Generate(prefix, now);
                var alreadyExists = await _dbContext.Set<TrxEmergencyVisit>()
                    .AsNoTracking()
                    .AnyAsync(
                        x => !x.IsDelete && x.EmergencyVisitNumber == number,
                        cancellationToken);

                if (!alreadyExists)
                    return number;
            }

            throw new InvalidOperationException("Nomor kunjungan IGD unik gagal dibentuk.");
        }
    }
}
