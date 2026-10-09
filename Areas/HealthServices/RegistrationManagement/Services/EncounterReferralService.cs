using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Pemilik aturan rincian rujukan kunjungan (<c>RJ-DOC-REV-BE-018</c>,
    /// <c>RJ-DOC-REFERRAL-001@1.0.0</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Identitas rujukan tidak diduplikasi.</b> Nomor rujukan, instansi, dan dokter perujuk
    /// tetap disimpan di <see cref="RegPatientEncounter"/>; service ini menulis keduanya bersama
    /// supaya tidak pernah berselisih.
    /// </para>
    /// <para>
    /// <b>Kelengkapan dihitung, bukan diisi.</b> <see cref="RegEncounterReferral.IsComplete"/>
    /// benar bila nomor, tanggal, instansi, unit tujuan, diagnosa, alasan, dan minimal satu surat
    /// aktif ada (<c>RJ-DOC-DEC-075</c>, <c>077</c>). Contoh: rujukan Kiosk tanpa diagnosa
    /// tersimpan <c>false</c>, lalu menjadi <c>true</c> saat petugas melengkapinya.
    /// </para>
    /// <para>
    /// <b>Koreksi terkunci</b> begitu konsultasi dokter dimulai (status kunjungan
    /// <c>InConsultation</c> ke atas), dan unit tujuan tidak pernah berubah (<c>RJ-DOC-DEC-078</c>).
    /// </para>
    /// <para>Diagnosa, catatan, dan alasan adalah data kesehatan: tidak pernah dikirim ke log.</para>
    /// </remarks>
    public class EncounterReferralService
    {
        public const string Pm03 = "RJ-VAL-PM-03: Nomor rujukan dan fasilitas perujuk wajib diisi.";
        public const string Pm04 = "RJ-VAL-PM-04: Dokter perujuk tidak terdaftar pada fasilitas perujuk ini, atau fasilitas/dokter perujuk tidak aktif.";
        public const string Pm05 = "RJ-VAL-PM-05: Unit tujuan belum tersedia.";
        public const string Pm06 = "RJ-VAL-PM-06: Diagnosa dan alasan rujukan wajib diisi.";
        public const string Pm07 = "RJ-VAL-PM-07: Tanggal rujukan tidak boleh melewati waktu sekarang.";
        public const string Pm09 = "RJ-VAL-PM-09: Rujukan tidak dapat diubah karena konsultasi dokter sudah dimulai atau kunjungan sudah ditutup.";
        public const string Pm10 = "RJ-VAL-PM-10: Unit tujuan tidak dapat diubah. Batalkan kunjungan bila salah unit.";
        public const string Pm11 = "RJ-VAL-PM-11: Data rujukan sudah diubah pengguna lain. Muat ulang.";
        public const string Pm17 = "RJ-VAL-PM-17: Fasilitas perujuk bukan mitra aktif atau perjanjian kerja samanya tidak berlaku pada tanggal kunjungan. Pilih ulang fasilitas perujuk.";

        /// <summary>Toleransi jam perangkat terhadap jam server untuk tanggal rujukan.</summary>
        private static readonly TimeSpan ClockSkew = TimeSpan.FromMinutes(5);

        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public EncounterReferralService(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        /// <summary>
        /// Tanggal kunjungan mulai kapan rujukan tanpa rincian dianggap belum lengkap
        /// (<c>HealthServices:Registration:ReferralDetailRequiredFrom</c>, bawaan 2026-10-08). Kunjungan lama
        /// tidak pernah punya rincian dan tidak ditandai.
        /// </summary>
        public DateTime ReferralDetailRequiredFrom
        {
            get
            {
                var raw = _configuration["HealthServices:Registration:ReferralDetailRequiredFrom"];
                var date = DateTime.TryParse(raw, out var parsed) ? parsed.Date : new DateTime(2026, 10, 8);
                return DateTime.SpecifyKind(date, DateTimeKind.Utc);
            }
        }

        // =================================================================
        // Create kunjungan (dipanggil di dalam transaksi CreateEncounterCoreAsync)
        // =================================================================

        /// <summary>
        /// Validasi identitas dan rincian rujukan pada create kunjungan. <c>null</c> berarti sah.
        /// </summary>
        /// <remarks>
        /// <c>serviceDate</c> adalah tanggal kunjungan (kalender WIB). Fasilitas wajib mitra layak
        /// pada tanggal ini (<c>DEC-FRJ-001</c>), bukan hanya saat daftar pilihan ditampilkan.
        /// </remarks>
        public async Task<string?> ValidateForCreateAsync(
            PatientEncounterCreateRequest request,
            bool isKiosk,
            DateOnly serviceDate,
            CancellationToken cancellationToken = default)
        {
            if (!request.IsReferral)
                return null;

            // Identitas perujuk boleh dikirim tanpa blok rincian (kontrak lama Kiosk), tetapi
            // bila dikirim wajib sah.
            var identityError = await ValidateInstitutionAndDoctorAsync(
                request.ReferralInstitutionId,
                request.ReferralDoctorId,
                previousInstitutionId: null,
                previousDoctorId: null,
                serviceDate,
                cancellationToken);

            if (identityError != null)
                return identityError;

            var referral = request.Referral;

            if (referral == null)
                return null;

            if (string.IsNullOrWhiteSpace(request.ReferralNumber) ||
                !request.ReferralInstitutionId.HasValue ||
                request.ReferralInstitutionId.Value == Guid.Empty)
            {
                return Pm03;
            }

            if (referral.TargetUnitType != ReferralTargetUnitType.Clinic ||
                !request.ClinicId.HasValue ||
                request.ClinicId.Value == Guid.Empty)
            {
                return Pm05;
            }

            if (referral.ReferralDateTime.HasValue &&
                ToUtc(referral.ReferralDateTime.Value) > DateTime.UtcNow.Add(ClockSkew))
            {
                return Pm07;
            }

            if (!isKiosk &&
                (!referral.DiagnosisId.HasValue ||
                 referral.DiagnosisId.Value == Guid.Empty ||
                 string.IsNullOrWhiteSpace(referral.ReferralReason)))
            {
                return Pm06;
            }

            return await ValidateDiagnosisAsync(referral.DiagnosisId, cancellationToken);
        }

        /// <summary>
        /// Mengisi identitas perujuk pada kunjungan baru dan menambahkan rincian rujukan ke context.
        /// Tidak memanggil <c>SaveChanges</c>; penyimpanan ikut transaksi pemanggil.
        /// </summary>
        public async Task<RegEncounterReferral?> AttachToNewEncounterAsync(
            RegPatientEncounter encounter,
            PatientEncounterCreateRequest request,
            bool isKiosk,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken = default)
        {
            if (!request.IsReferral)
                return null;

            encounter.ReferralInstitutionId = NormalizeGuid(request.ReferralInstitutionId);
            encounter.ReferralDoctorId = NormalizeGuid(request.ReferralDoctorId);

            if (request.Referral == null)
                return null;

            var referral = new RegEncounterReferral
            {
                Id = Guid.NewGuid(),
                PatientEncounterId = encounter.Id,
                ReferralDateTime = request.Referral.ReferralDateTime.HasValue
                    ? ToUtc(request.Referral.ReferralDateTime.Value)
                    : now,
                TargetUnitType = ReferralTargetUnitType.Clinic,
                TargetServiceUnitId = encounter.ServiceUnitId,
                TargetClinicId = encounter.ClinicId,
                DiagnosisId = NormalizeGuid(request.Referral.DiagnosisId),
                DiagnosisNote = NormalizeText(request.Referral.DiagnosisNote),
                ReferralReason = NormalizeText(request.Referral.ReferralReason),
                CaptureSource = isKiosk ? ReferralCaptureSource.Kiosk : ReferralCaptureSource.Staff,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            await CaptureInstitutionSnapshotAsync(
                referral,
                encounter.ReferralInstitutionId,
                DateOnly.FromDateTime(encounter.EncounterDate),
                cancellationToken);

            // Surat rujukan baru diunggah sesudah kunjungan terbentuk, sehingga rincian baru
            // selalu belum lengkap pada saat ini.
            ApplyCompletion(referral, encounter, activeDocumentCount: 0, now);

            _dbContext.Set<RegEncounterReferral>().Add(referral);

            return referral;
        }

        // =================================================================
        // GET / PUT rincian
        // =================================================================

        public async Task<EncounterReferralResponse?> GetAsync(
            Guid encounterId,
            CancellationToken cancellationToken = default)
        {
            var encounter = await _dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .Where(x => x.Id == encounterId && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.EncounterNumber,
                    x.IsReferral,
                    x.ReferralNumber,
                    x.EncounterStatus,
                    x.IsCancel,
                    Institution = x.ReferralInstitution == null ? null : new EncounterReferralReferenceResponse
                    {
                        Id = x.ReferralInstitution.Id,
                        Code = x.ReferralInstitution.InstitutionCode,
                        Name = x.ReferralInstitution.InstitutionName,
                        IsPartner = x.ReferralInstitution.IsPartner
                    },
                    Doctor = x.ReferralDoctor == null ? null : new EncounterReferralReferenceResponse
                    {
                        Id = x.ReferralDoctor.Id,
                        Name = x.ReferralDoctor.DoctorName
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (encounter == null)
                return null;

            var referral = await _dbContext.Set<RegEncounterReferral>()
                .AsNoTracking()
                .Where(x => x.PatientEncounterId == encounterId && !x.IsDelete)
                .Select(x => new
                {
                    Entity = x,
                    ServiceUnitName = x.TargetServiceUnit != null ? x.TargetServiceUnit.ServiceUnitName : null,
                    ClinicName = x.TargetClinic != null ? x.TargetClinic.ClinicName : null,
                    Diagnosis = x.Diagnosis == null ? null : new EncounterReferralReferenceResponse
                    {
                        Id = x.Diagnosis.Id,
                        Code = x.Diagnosis.DiagnosisCode,
                        Name = x.Diagnosis.DiagnosisName
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);

            var documents = referral == null
                ? new List<EncounterReferralDocumentResponse>()
                : await _dbContext.Set<RegEncounterReferralDocument>()
                    .AsNoTracking()
                    .Where(x => x.EncounterReferralId == referral.Entity.Id && !x.IsDelete)
                    .OrderBy(x => x.PageOrder)
                    .Select(x => new EncounterReferralDocumentResponse
                    {
                        Id = x.Id,
                        OriginalFileName = x.OriginalFileName,
                        ContentType = x.ContentType,
                        SizeBytes = x.SizeBytes,
                        PageOrder = x.PageOrder,
                        CreateDateTime = x.CreateDateTime
                    })
                    .ToListAsync(cancellationToken);

            var r = referral?.Entity;

            var response = new EncounterReferralResponse
            {
                Id = r?.Id,
                EncounterId = encounter.Id,
                EncounterNumber = encounter.EncounterNumber,
                IsReferral = encounter.IsReferral,
                ReferralNumber = encounter.ReferralNumber,
                ReferralDateTime = r?.ReferralDateTime,
                ReferralInstitution = encounter.Institution,
                ReferralDoctor = encounter.Doctor,
                InstitutionIsPartnerSnapshot = r?.InstitutionIsPartnerSnapshot ?? false,
                InstitutionCodeSnapshot = r?.InstitutionCodeSnapshot,
                InstitutionNameSnapshot = r?.InstitutionNameSnapshot,
                AgreementNumberSnapshot = r?.AgreementNumberSnapshot,
                TargetUnitType = r?.TargetUnitType,
                TargetUnitTypeName = r == null ? null : TargetUnitLabel(r.TargetUnitType),
                TargetServiceUnitId = r?.TargetServiceUnitId,
                TargetClinicId = r?.TargetClinicId,
                TargetUnitName = referral?.ClinicName ?? referral?.ServiceUnitName,
                Diagnosis = referral?.Diagnosis,
                DiagnosisNote = r?.DiagnosisNote,
                ReferralReason = r?.ReferralReason,
                CaptureSource = r?.CaptureSource,
                IsComplete = r?.IsComplete ?? false,
                IsLocked = IsLocked(encounter.EncounterStatus, encounter.IsCancel),
                RowVersion = r?.RowVersion,
                Documents = documents
            };

            response.MissingFields = ComputeMissingFields(
                encounter.ReferralNumber,
                encounter.Institution?.Id,
                r,
                documents.Count);

            return response;
        }

        public async Task<EncounterReferralResult> UpsertAsync(
            Guid encounterId,
            UpsertEncounterReferralRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var encounter = await _dbContext.Set<RegPatientEncounter>()
                .FirstOrDefaultAsync(x => x.Id == encounterId && !x.IsDelete, cancellationToken);

            if (encounter == null)
                return EncounterReferralResult.NotFound("Kunjungan tidak ditemukan.");

            if (IsLocked(encounter.EncounterStatus, encounter.IsCancel))
                return EncounterReferralResult.Conflict(Pm09);

            var referral = await _dbContext.Set<RegEncounterReferral>()
                .FirstOrDefaultAsync(x => x.PatientEncounterId == encounterId && !x.IsDelete, cancellationToken);

            if (referral != null && request.ExpectedRowVersion != referral.RowVersion)
                return EncounterReferralResult.Conflict(Pm11);

            if (string.IsNullOrWhiteSpace(request.ReferralNumber) ||
                !request.ReferralInstitutionId.HasValue ||
                request.ReferralInstitutionId.Value == Guid.Empty)
            {
                return EncounterReferralResult.Invalid(Pm03);
            }

            if (request.ReferralNumber.Trim().Length > 250)
                return EncounterReferralResult.Invalid("Nomor rujukan terlalu panjang. Batasnya 250 huruf.");

            var identityError = await ValidateInstitutionAndDoctorAsync(
                request.ReferralInstitutionId,
                request.ReferralDoctorId,
                encounter.ReferralInstitutionId,
                encounter.ReferralDoctorId,
                DateOnly.FromDateTime(encounter.EncounterDate),
                cancellationToken);

            if (identityError != null)
                return EncounterReferralResult.Invalid(identityError);

            if (request.ReferralDateTime.HasValue &&
                ToUtc(request.ReferralDateTime.Value) > DateTime.UtcNow.Add(ClockSkew))
            {
                return EncounterReferralResult.Invalid(Pm07);
            }

            var diagnosisError = await ValidateDiagnosisAsync(request.DiagnosisId, cancellationToken);

            if (diagnosisError != null)
                return EncounterReferralResult.Invalid(diagnosisError);

            var now = DateTime.UtcNow;
            var isNew = referral == null;

            if (isNew)
            {
                var targetError = ResolveNewTarget(encounter, request, out var targetType, out var targetServiceUnitId, out var targetClinicId);

                if (targetError != null)
                    return EncounterReferralResult.Invalid(targetError);

                referral = new RegEncounterReferral
                {
                    Id = Guid.NewGuid(),
                    PatientEncounterId = encounter.Id,
                    TargetUnitType = targetType,
                    TargetServiceUnitId = targetServiceUnitId,
                    TargetClinicId = targetClinicId,
                    CaptureSource = ReferralCaptureSource.Staff,
                    ReferralDateTime = now,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                _dbContext.Set<RegEncounterReferral>().Add(referral);
            }
            else if ((request.TargetUnitType.HasValue && request.TargetUnitType.Value != referral!.TargetUnitType) ||
                     (request.TargetServiceUnitId.HasValue &&
                      request.TargetServiceUnitId.Value != Guid.Empty &&
                      request.TargetServiceUnitId.Value != referral.TargetServiceUnitId))
            {
                return EncounterReferralResult.Invalid(Pm10);
            }

            var before = isNew ? null : Snapshot(encounter, referral!);
            var previousInstitutionId = encounter.ReferralInstitutionId;
            var wasComplete = !isNew && referral!.IsComplete;

            encounter.IsReferral = true;
            encounter.ReferralNumber = request.ReferralNumber.Trim();
            encounter.ReferralInstitutionId = request.ReferralInstitutionId;
            encounter.ReferralDoctorId = NormalizeGuid(request.ReferralDoctorId);
            encounter.UpdateDateTime = now;
            encounter.UpdateBy = actorUserId;

            referral!.ReferralDateTime = request.ReferralDateTime.HasValue
                ? ToUtc(request.ReferralDateTime.Value)
                : (isNew ? now : referral.ReferralDateTime);
            referral.DiagnosisId = NormalizeGuid(request.DiagnosisId);
            referral.DiagnosisNote = NormalizeText(request.DiagnosisNote);
            referral.ReferralReason = NormalizeText(request.ReferralReason);
            // Snapshot fasilitas diambil ulang hanya bila fasilitasnya diganti (atau belum pernah
            // ada), supaya koreksi ruas lain tidak menimpa histori kontrak saat rujukan dicatat.
            if (isNew ||
                previousInstitutionId != encounter.ReferralInstitutionId ||
                referral.InstitutionNameSnapshot == null)
            {
                await CaptureInstitutionSnapshotAsync(
                    referral,
                    encounter.ReferralInstitutionId,
                    DateOnly.FromDateTime(encounter.EncounterDate),
                    cancellationToken);
            }

            referral.RowVersion = Guid.NewGuid();
            referral.UpdateDateTime = now;
            referral.UpdateBy = actorUserId;

            var activeDocuments = isNew
                ? 0
                : await CountActiveDocumentsAsync(referral.Id, cancellationToken);

            ApplyCompletion(referral, encounter, activeDocuments, now);

            if (!isNew)
            {
                var after = Snapshot(encounter, referral);
                var changes = Diff(before!, after);

                if (changes.Old.Count > 0)
                {
                    AddRevision(
                        referral.Id,
                        !wasComplete && referral.IsComplete ? ReferralRevisionType.Completed : ReferralRevisionType.Corrected,
                        changes.Old,
                        changes.New,
                        actorUserId,
                        now);
                }
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return EncounterReferralResult.Conflict(Pm11);
            }

            return EncounterReferralResult.Ok(isNew ? "Rincian rujukan berhasil disimpan." : "Rincian rujukan berhasil diperbarui.");
        }

        // =================================================================
        // Dipakai juga oleh pengelola surat rujukan (RJ-DOC-REV-BE-019)
        // =================================================================

        public static bool IsLocked(EncounterStatus status, bool isCancel)
            => isCancel || status >= EncounterStatus.InConsultation;

        /// <summary>Menghitung ulang kelengkapan dan mencatat <c>CompletedAt</c> pertama kali.</summary>
        public static void ApplyCompletion(
            RegEncounterReferral referral,
            RegPatientEncounter encounter,
            int activeDocumentCount,
            DateTime now)
        {
            var missing = ComputeMissingFields(
                encounter.ReferralNumber,
                encounter.ReferralInstitutionId,
                referral,
                activeDocumentCount);

            referral.IsComplete = missing.Count == 0;

            if (referral.IsComplete && referral.CompletedAt == null)
                referral.CompletedAt = now;
        }

        public static List<string> ComputeMissingFields(
            string? referralNumber,
            Guid? referralInstitutionId,
            RegEncounterReferral? referral,
            int activeDocumentCount)
        {
            var missing = new List<string>();

            if (string.IsNullOrWhiteSpace(referralNumber)) missing.Add("referralNumber");
            if (referral == null || referral.ReferralDateTime == default) missing.Add("referralDateTime");
            if (!referralInstitutionId.HasValue || referralInstitutionId.Value == Guid.Empty) missing.Add("referralInstitution");
            if (referral == null || referral.TargetServiceUnitId == Guid.Empty) missing.Add("targetUnit");
            if (referral?.DiagnosisId == null) missing.Add("diagnosis");
            if (string.IsNullOrWhiteSpace(referral?.ReferralReason)) missing.Add("referralReason");
            if (activeDocumentCount < 1) missing.Add("documents");

            return missing;
        }

        public void AddRevision(
            Guid referralId,
            ReferralRevisionType type,
            object? oldValues,
            object? newValues,
            Guid actorUserId,
            DateTime now)
        {
            _dbContext.Set<RegEncounterReferralRevision>().Add(new RegEncounterReferralRevision
            {
                Id = Guid.NewGuid(),
                EncounterReferralId = referralId,
                RevisionType = type,
                OldValuesJson = oldValues == null ? null : JsonSerializer.Serialize(oldValues),
                NewValuesJson = newValues == null ? null : JsonSerializer.Serialize(newValues),
                ChangedAt = now,
                ChangedBy = actorUserId == Guid.Empty ? null : actorUserId,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }

        public Task<int> CountActiveDocumentsAsync(Guid referralId, CancellationToken cancellationToken)
            => _dbContext.Set<RegEncounterReferralDocument>()
                .CountAsync(x => x.EncounterReferralId == referralId && !x.IsDelete, cancellationToken);

        public static string TargetUnitLabel(ReferralTargetUnitType type) => type switch
        {
            ReferralTargetUnitType.Clinic => "Poliklinik",
            ReferralTargetUnitType.Laboratory => "Laboratorium",
            ReferralTargetUnitType.Radiology => "Radiologi",
            _ => type.ToString()
        };

        // =================================================================
        // Bantuan internal
        // =================================================================

        /// <summary>
        /// Unit tujuan rincian baru lewat <c>PUT</c> diturunkan dari kunjungan itu sendiri:
        /// kunjungan berklinik → <c>Clinic</c>, kunjungan Laboratorium → <c>Laboratory</c>.
        /// </summary>
        private static string? ResolveNewTarget(
            RegPatientEncounter encounter,
            UpsertEncounterReferralRequest request,
            out ReferralTargetUnitType targetType,
            out Guid targetServiceUnitId,
            out Guid? targetClinicId)
        {
            targetServiceUnitId = encounter.ServiceUnitId;
            targetClinicId = encounter.ClinicId;
            targetType = encounter.ClinicId.HasValue ? ReferralTargetUnitType.Clinic : ReferralTargetUnitType.Laboratory;

            if (request.TargetUnitType == ReferralTargetUnitType.Radiology)
                return Pm05;

            if (request.TargetUnitType.HasValue && request.TargetUnitType.Value != targetType)
                return Pm10;

            if (request.TargetServiceUnitId.HasValue &&
                request.TargetServiceUnitId.Value != Guid.Empty &&
                request.TargetServiceUnitId.Value != encounter.ServiceUnitId)
            {
                return Pm10;
            }

            return null;
        }

        private async Task<string?> ValidateInstitutionAndDoctorAsync(
            Guid? institutionId,
            Guid? doctorId,
            Guid? previousInstitutionId,
            Guid? previousDoctorId,
            DateOnly serviceDate,
            CancellationToken cancellationToken)
        {
            var institution = NormalizeGuid(institutionId);
            var doctor = NormalizeGuid(doctorId);

            if (institution == null)
                return doctor == null ? null : Pm04;

            if (institution == previousInstitutionId)
            {
                // Fasilitas yang sudah tercatat pada kunjungan tetap boleh dipertahankan saat
                // koreksi walaupun sesudahnya dinonaktifkan atau kontraknya berakhir: histori
                // tidak boleh dipaksa berubah (DEC-FRJ-001).
                var stillExists = await _dbContext.Set<MstReferralInstitution>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == institution.Value && !x.IsDelete, cancellationToken);

                if (!stillExists)
                    return Pm04;
            }
            else if (await ReferralPartnerEligibility.FindEligibleAsync(
                         _dbContext, institution.Value, serviceDate, cancellationToken) == null)
            {
                // DEC-FRJ-001: memilih fasilitas baru wajib mitra aktif dengan perjanjian yang
                // berlaku pada tanggal kunjungan — divalidasi saat submit, bukan hanya di dropdown.
                return Pm17;
            }

            if (doctor == null)
                return null;

            var doctorRow = await _dbContext.Set<MstReferralDoctor>()
                .AsNoTracking()
                .Where(x => x.Id == doctor.Value && !x.IsDelete)
                .Select(x => new { x.ReferralInstitutionId, x.IsActive })
                .FirstOrDefaultAsync(cancellationToken);

            if (doctorRow == null ||
                doctorRow.ReferralInstitutionId != institution.Value ||
                (!doctorRow.IsActive && doctor != previousDoctorId))
            {
                return Pm04;
            }

            return null;
        }

        private async Task<string?> ValidateDiagnosisAsync(Guid? diagnosisId, CancellationToken cancellationToken)
        {
            var id = NormalizeGuid(diagnosisId);

            if (id == null)
                return null;

            var exists = await _dbContext.Set<MstDiagnosis>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == id.Value && !x.IsDelete, cancellationToken);

            return exists ? null : "Diagnosa rujukan tidak ditemukan.";
        }

        /// <summary>
        /// Menyalin identitas fasilitas dan nomor perjanjian yang berlaku ke rincian rujukan
        /// (<c>DEC-FRJ-001</c>). Hanya dipanggil saat rincian baru dibuat atau fasilitasnya diganti,
        /// sehingga perubahan master/kontrak sesudahnya tidak mengubah histori transaksi lama.
        /// </summary>
        private async Task CaptureInstitutionSnapshotAsync(
            RegEncounterReferral referral,
            Guid? institutionId,
            DateOnly serviceDate,
            CancellationToken cancellationToken)
        {
            referral.InstitutionIsPartnerSnapshot = false;
            referral.InstitutionCodeSnapshot = null;
            referral.InstitutionNameSnapshot = null;
            referral.AgreementNumberSnapshot = null;

            if (!institutionId.HasValue)
                return;

            var row = await _dbContext.Set<MstReferralInstitution>()
                .AsNoTracking()
                .Where(x => x.Id == institutionId.Value)
                .Select(x => new
                {
                    x.IsPartner,
                    x.InstitutionCode,
                    x.InstitutionName,
                    AgreementNumber = x.Agreements
                        .Where(a => !a.IsDelete && a.StartDate <= serviceDate && a.EndDate >= serviceDate)
                        .OrderByDescending(a => a.StartDate)
                        .Select(a => a.AgreementNumber)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (row == null)
                return;

            referral.InstitutionIsPartnerSnapshot = row.IsPartner;
            referral.InstitutionCodeSnapshot = row.InstitutionCode;
            referral.InstitutionNameSnapshot = row.InstitutionName;
            referral.AgreementNumberSnapshot = row.AgreementNumber;
        }

        private static Dictionary<string, object?> Snapshot(RegPatientEncounter encounter, RegEncounterReferral referral)
            => new()
            {
                ["referralNumber"] = encounter.ReferralNumber,
                ["referralInstitutionId"] = encounter.ReferralInstitutionId,
                ["referralDoctorId"] = encounter.ReferralDoctorId,
                ["referralDateTime"] = referral.ReferralDateTime,
                ["diagnosisId"] = referral.DiagnosisId,
                ["diagnosisNote"] = referral.DiagnosisNote,
                ["referralReason"] = referral.ReferralReason
            };

        private static (Dictionary<string, object?> Old, Dictionary<string, object?> New) Diff(
            Dictionary<string, object?> before,
            Dictionary<string, object?> after)
        {
            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var key in before.Keys)
            {
                if (!Equals(before[key], after[key]))
                {
                    oldValues[key] = before[key];
                    newValues[key] = after[key];
                }
            }

            return (oldValues, newValues);
        }

        private static DateTime ToUtc(DateTime value)
            => value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };

        private static Guid? NormalizeGuid(Guid? value)
            => value.HasValue && value.Value != Guid.Empty ? value : null;

        private static string? NormalizeText(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public enum EncounterReferralResultStatus
    {
        Success = 0,
        NotFound = 1,
        Invalid = 2,
        Conflict = 3,
        Forbidden = 4
    }

    public sealed record EncounterReferralResult(EncounterReferralResultStatus Status, string Message)
    {
        public static EncounterReferralResult Ok(string message) => new(EncounterReferralResultStatus.Success, message);
        public static EncounterReferralResult NotFound(string message) => new(EncounterReferralResultStatus.NotFound, message);
        public static EncounterReferralResult Invalid(string message) => new(EncounterReferralResultStatus.Invalid, message);
        public static EncounterReferralResult Conflict(string message) => new(EncounterReferralResultStatus.Conflict, message);
        public static EncounterReferralResult Forbidden(string message) => new(EncounterReferralResultStatus.Forbidden, message);
    }
}
