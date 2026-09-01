using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Menjaga lifecycle awal konsultasi dokter agar satu antrean/encounter
    /// selalu menggunakan transaksi TrxDoctorConsultation yang sama.
    ///
    /// Service ini tidak melakukan SaveChanges atau Commit. Pemanggil wajib
    /// menjalankannya di dalam transaction yang sama dengan perubahan status
    /// TrxQueue dan TrxPatientEncounter.
    /// </summary>
    public class DoctorConsultationLifecycleService
    {
        private readonly ApplicationDbContext _dbContext;

        public DoctorConsultationLifecycleService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TrxDoctorConsultation> GetOrCreateForQueueAsync(
            TrxQueue queue,
            Guid actorUserId,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(queue);

            if (queue.Id == Guid.Empty)
                throw new InvalidOperationException("QueueId konsultasi dokter tidak valid.");

            if (queue.EncounterId == Guid.Empty)
                throw new InvalidOperationException("EncounterId konsultasi dokter tidak valid.");

            if (!queue.IsDoctorRequired)
                throw new InvalidOperationException("Antrean ini tidak membutuhkan pemeriksaan dokter.");

            if (!queue.DoctorId.HasValue || queue.DoctorId.Value == Guid.Empty)
                throw new InvalidOperationException("Dokter pada antrean belum ditentukan.");

            if (_dbContext.Database.CurrentTransaction == null)
            {
                throw new InvalidOperationException(
                    "DoctorConsultationLifecycleService harus dijalankan di dalam database transaction.");
            }

            await AcquireLifecycleLockAsync(queue.EncounterId, cancellationToken);

            var existingRows = await _dbContext.Set<TrxDoctorConsultation>()
                .Where(x =>
                    !x.IsDelete &&
                    (x.QueueId == queue.Id || x.EncounterId == queue.EncounterId))
                .ToListAsync(cancellationToken);

            var existing = existingRows
                .OrderByDescending(x => x.QueueId == queue.Id)
                .ThenByDescending(x => x.IsActive)
                .ThenByDescending(x => x.UpdateDateTime ?? x.CreateDateTime)
                .FirstOrDefault();

            if (existing != null)
            {
                if (existing.ConsultationStatus == DoctorConsultationStatus.Cancelled ||
                    existing.IsCancel)
                {
                    throw new InvalidOperationException(
                        "Konsultasi dokter pada encounter ini sudah dibatalkan dan tidak dapat dimulai kembali.");
                }

                if (existing.ConsultationStatus == DoctorConsultationStatus.Completed ||
                    existing.CompletedAt.HasValue)
                {
                    throw new InvalidOperationException(
                        "Konsultasi dokter pada encounter ini sudah selesai.");
                }

                existing.ConsultationStatus = DoctorConsultationStatus.InProgress;
                existing.StartedAt ??= nowUtc;
                existing.StartedByUserId ??= actorUserId == Guid.Empty ? null : actorUserId;
                existing.IsActive = true;
                existing.UpdateDateTime = nowUtc;
                existing.UpdateBy = actorUserId;

                return existing;
            }

            var assessment = await ResolveAssessmentAsync(
                queue.EncounterId,
                cancellationToken);

            var entity = new TrxDoctorConsultation
            {
                Id = Guid.NewGuid(),
                ConsultationNumber = await GenerateConsultationNumberAsync(
                    nowUtc,
                    cancellationToken),
                EncounterId = queue.EncounterId,
                QueueId = queue.Id,
                AssessmentId = assessment?.Id,
                PatientId = queue.PatientId,
                DoctorId = queue.DoctorId.Value,
                ServiceUnitId = queue.ServiceUnitId,
                ClinicId = queue.ClinicId,
                ConsultationDateTime = nowUtc,
                ConsultationStatus = DoctorConsultationStatus.InProgress,

                IsVitalSignCopiedFromAssessment = assessment != null,
                BloodPressureSystolic = assessment?.BloodPressureSystolic,
                BloodPressureDiastolic = assessment?.BloodPressureDiastolic,
                PulseRate = assessment?.PulseRate,
                RespiratoryRate = assessment?.RespiratoryRate,
                Temperature = assessment?.Temperature,
                OxygenSaturation = assessment?.OxygenSaturation,
                Weight = assessment?.Weight,
                Height = assessment?.Height,
                BMI = assessment?.BMI,

                ChiefComplaint = string.IsNullOrWhiteSpace(queue.Encounter?.ChiefComplaint)
                    ? null
                    : queue.Encounter!.ChiefComplaint!.Trim(),

                DiagnosisCount = 0,
                HasPrimaryDiagnosis = false,
                ProcedureCount = 0,
                HasProcedure = false,
                PrescriptionCount = 0,
                HasPrescription = false,
                SupportingOrderCount = 0,
                HasSupportingOrder = false,
                MedicalCertificateCount = 0,
                ClinicalDocumentCount = 0,
                ConsentCount = 0,

                StartedAt = nowUtc,
                StartedByUserId = actorUserId == Guid.Empty ? null : actorUserId,
                IsActive = true,
                CreateDateTime = nowUtc,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<TrxDoctorConsultation>().Add(entity);
            return entity;
        }

        /// <summary>
        /// Mencari konsultasi dokter yang masih dapat difinalisasi untuk sebuah antrean.
        ///
        /// Dipakai jalur kompatibilitas <c>POST /doctor-queues/{id}/finish-consultation</c>
        /// (<c>RJ-DOC-BE-001</c>) supaya jalur itu tidak lagi memiliki logika finalisasi klinis
        /// sendiri. Identitas konsultasi diresolusi server dari antrean, bukan diterima dari
        /// client, sesuai <c>RJ-DOC-COMPLETION-001@1.0.0</c> bagian 1.2.
        ///
        /// Kecocokan encounter ikut diperiksa supaya konsultasi milik antrean lain tidak pernah
        /// terambil. Mengembalikan <c>null</c> bila tidak ada kandidat yang sah; pemanggil yang
        /// memutuskan bentuk responsnya.
        /// </summary>
        public async Task<TrxDoctorConsultation?> ResolveFinalizableForQueueAsync(
            Guid queueId,
            Guid encounterId,
            CancellationToken cancellationToken = default)
        {
            if (queueId == Guid.Empty || encounterId == Guid.Empty)
                return null;

            return await _dbContext.Set<TrxDoctorConsultation>()
                .Where(x =>
                    x.QueueId == queueId &&
                    x.EncounterId == encounterId &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.ConsultationStatus != DoctorConsultationStatus.Cancelled &&
                    x.ConsultationStatus != DoctorConsultationStatus.Completed)
                .OrderByDescending(x => x.ConsultationStatus == DoctorConsultationStatus.InProgress)
                .ThenByDescending(x => x.UpdateDateTime ?? x.CreateDateTime)
                .ThenByDescending(x => x.ConsultationDateTime)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<TrxPatientAssessment?> ResolveAssessmentAsync(
            Guid encounterId,
            CancellationToken cancellationToken)
        {
            var completedAssessment = await _dbContext.Set<TrxPatientAssessment>()
                .AsNoTracking()
                .Where(x =>
                    x.EncounterId == encounterId &&
                    x.AssessmentStatus == PatientAssessmentStatus.Completed &&
                    !x.IsDelete)
                .OrderByDescending(x => x.AssessmentDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            if (completedAssessment != null)
                return completedAssessment;

            return await _dbContext.Set<TrxPatientAssessment>()
                .AsNoTracking()
                .Where(x => x.EncounterId == encounterId && !x.IsDelete)
                .OrderByDescending(x => x.AssessmentDateTime)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<string> GenerateConsultationNumberAsync(
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            await AcquireConsultationNumberLockAsync(nowUtc, cancellationToken);

            var prefix = $"CON-{nowUtc:yyyyMMdd}";
            var lastNumber = await _dbContext.Set<TrxDoctorConsultation>()
                .AsNoTracking()
                .Where(x => x.ConsultationNumber.StartsWith(prefix))
                .OrderByDescending(x => x.ConsultationNumber)
                .Select(x => x.ConsultationNumber)
                .FirstOrDefaultAsync(cancellationToken);

            var nextSequence = 1;
            if (!string.IsNullOrWhiteSpace(lastNumber))
            {
                var separatorIndex = lastNumber.LastIndexOf('-');
                if (separatorIndex >= 0 &&
                    int.TryParse(lastNumber[(separatorIndex + 1)..], out var currentSequence))
                {
                    nextSequence = currentSequence + 1;
                }
            }

            return $"{prefix}-{nextSequence:D5}";
        }

        private async Task AcquireLifecycleLockAsync(
            Guid encounterId,
            CancellationToken cancellationToken)
        {
            var lockKey = BuildGuidAdvisoryLockKey(
                encounterId,
                0x444F4354434F4E53UL);

            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock({0});",
                new object[] { lockKey },
                cancellationToken);
        }

        private async Task AcquireConsultationNumberLockAsync(
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            const long numberNamespace = 0x434F4E0000000000L;
            var dateNumber =
                (long)nowUtc.Year * 10000L +
                (long)nowUtc.Month * 100L +
                nowUtc.Day;
            var lockKey = numberNamespace + dateNumber;

            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock({0});",
                new object[] { lockKey },
                cancellationToken);
        }

        private static long BuildGuidAdvisoryLockKey(Guid value, ulong lockNamespace)
        {
            const ulong fnvOffsetBasis = 14695981039346656037UL;
            const ulong fnvPrime = 1099511628211UL;

            var hash = fnvOffsetBasis ^ lockNamespace;
            foreach (var item in value.ToByteArray())
            {
                hash ^= item;
                hash *= fnvPrime;
            }

            return unchecked((long)hash);
        }
    }
}
