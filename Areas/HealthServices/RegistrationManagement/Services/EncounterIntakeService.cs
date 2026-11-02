using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;
using System.Linq.Expressions;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Pelaksana <c>INT-05</c> — jalur pendaftaran kunjungan bagi modul lain
    /// (<c>LAB-DEC-032</c>, <c>BE-EXT-03</c>, dipakai <c>BE-LAB-08</c>).
    ///
    /// <b>Kenapa berkas ini ada, dan kenapa ia milik Registrasi.</b> Layar pendaftaran pasien
    /// laboratorium berada di modul Laboratorium supaya petugas tidak berpindah aplikasi.
    /// Kunjungannya tetap dibuat Registrasi. Tanpa berkas ini, satu-satunya cara Laboratorium
    /// melayani pasien datang langsung adalah menulis sendiri ke <c>RegPatientEncounter</c> —
    /// persis yang dilarang <c>AC-45</c>. Jadi berkas ini bukan kemudahan, melainkan
    /// <b>penjaga batas</b>: ia memberi Laboratorium jalur yang sah supaya jalur yang tidak sah
    /// tidak pernah perlu ditempuh.
    ///
    /// <b>Ini bukan pengganti jalur loket.</b> <c>PatientEncounterController</c> tetap menjadi
    /// jalur pendaftaran lengkap: antrean dokter, jadwal, kuota, kiosk, dan penjamin
    /// perusahaan. Yang dilayani di sini hanya kedatangan langsung ke unit penunjang — satu
    /// kunjungan tanpa antrean dokter, karena pasiennya memang tidak menuju poliklinik. Kedua
    /// jalur menulis ke tabel yang sama dan memakai format nomor yang sama.
    ///
    /// <b>Batas yang ditegakkan:</b> tidak ada satu pun jalur di sini yang membuat atau
    /// mengubah data induk pasien. Pasien wajib sudah terdaftar.
    ///
    /// <b>Kunjungan Medical Check-Up</b> (<c>INT-MCU-02</c>, <c>BE-MCU-001</c>) juga lewat sini:
    /// MCU yang meng-check-in peserta, Registrasi yang membuat kunjungannya. Bedanya dengan
    /// kunjungan penunjang, peserta MCU memang menuju klinik — antrean, skrining perawat, dan
    /// dokter tujuannya mengikuti master klinik MCU persis seperti jalur loket.
    /// </summary>
    public class EncounterIntakeService
    {
        private const string LogCategory = "HealthServices.RegistrationManagement";

        // Format nomor disamakan persis dengan PatientEncounterController: kedua jalur menulis
        // ke tabel dan unique index yang sama, sehingga penomorannya tidak boleh bercabang.
        private const string EncounterCodePrefix = "ENC-RSMMC-";
        private const string PaymentSourceCodePrefix = "EGT-RSMMC-";
        private const int CodeNumberLength = 5;
        private const int AllocationAttempts = 5;
        private const string IntakeSavepoint = "encounter_intake";

        private const string DefaultOutpatientPatientClassName = "RAWAT JALAN";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AccessPermissionService _accessPermissionService;
        private readonly LoggerService _loggerService;
        private readonly OutpatientQueueClassificationService _queueClassificationService;
        private readonly OutpatientQueueNumberAllocator _queueNumberAllocator;
        private readonly QueueRealtimeService? _queueRealtimeService;
        private readonly List<Func<Task>> _deferredAfterCommit = new();

        public EncounterIntakeService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            AccessPermissionService accessPermissionService,
            LoggerService loggerService,
            OutpatientQueueClassificationService queueClassificationService,
            OutpatientQueueNumberAllocator queueNumberAllocator,
            QueueRealtimeService? queueRealtimeService = null)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _accessPermissionService = accessPermissionService;
            _loggerService = loggerService;
            _queueClassificationService = queueClassificationService;
            _queueNumberAllocator = queueNumberAllocator;
            _queueRealtimeService = queueRealtimeService;
        }

        /// <summary>
        /// Mendaftarkan satu kunjungan atas nama modul pemanggil (<c>INT-05</c>).
        ///
        /// Urutannya disengaja: idempotensi diperiksa <b>sebelum</b> apa pun yang lain, supaya
        /// penekanan Simpan kedua tidak perlu lolos validasi maupun pemeriksaan kewenangan
        /// untuk mendapatkan jawaban yang sama.
        /// </summary>
        public async Task<EncounterIntakeResult> RegisterAsync(
            EncounterIntakeRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var idempotencyKey = NormalizeNullableText(request.IdempotencyKey);

            // VAL-45. Pengiriman ulang yang berurutan berhenti di sini.
            if (idempotencyKey != null)
            {
                var replay = await FindByIdempotencyKeyAsync(idempotencyKey, cancellationToken);

                if (replay != null)
                {
                    return MapResult(
                        replay,
                        await FindQueueAsync(replay.Id, cancellationToken),
                        isReplay: true);
                }
            }

            // VAL-41. Kewenangan membuat kunjungan tetap milik Registrasi, bukan milik modul
            // pemanggil. Hak akses pada layar Laboratorium hanya membuka layarnya.
            if (!await HasRegistrationAuthorityAsync(cancellationToken))
            {
                throw new EncounterIntakeForbiddenException(
                    "Anda tidak berhak membuat kunjungan baru. Hubungi bagian pendaftaran.");
            }

            var patient = await LoadPatientAsync(request.PatientId, cancellationToken);
            var serviceUnit = await LoadServiceUnitAsync(request.ServiceUnitId, cancellationToken);

            var visit = await ResolveVisitProfileAsync(request, cancellationToken);

            await ValidateReferralAsync(request, cancellationToken);

            var paymentReference = await LoadPaymentSourceReferenceAsync(request, visit, cancellationToken);

            var patientClassId = await ResolveOutpatientPatientClassIdAsync(cancellationToken);

            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var (encounter, queue) = await PersistAsync(
                request,
                idempotencyKey,
                patient,
                serviceUnit,
                visit,
                paymentReference,
                patientClassId,
                actorUserId,
                now,
                cancellationToken);

            async Task AfterCommitAsync()
            {
                await NotifyQueueCreatedAsync(queue, actorUserId);

                // LoggerService hanya menulis teks pesan; ruas payload bernama Id/UserId justru
                // menimpa pelaku. Karena itu isi yang wajib tercatat ada di pesan, dan payload
                // tidak memakai nama ruas tersebut.
                await _loggerService.AuditAsync(
                    LogCategory,
                    "EncounterIntake.Register",
                    $"Kunjungan {encounter.EncounterNumber} ({encounter.EncounterType}) dibuat lewat " +
                    $"jalur pendaftaran modul lain (INT-05)" +
                    (queue == null ? "." : $", antrean {queue.QueueCode} ({queue.QueueStatus})."),
                    new
                    {
                        EncounterId = encounter.Id,
                        encounter.EncounterNumber,
                        encounter.EncounterType,
                        encounter.PatientId,
                        encounter.ServiceUnitId,
                        encounter.ClinicId,
                        QueueId = queue?.Id,
                        encounter.IsReferral,
                        ActorUserId = actorUserId
                    });
            }

            // BE-MCU-013: di dalam transaksi pemanggil, kunjungan belum benar-benar ada sampai
            // pemanggil commit. Notifikasi antrean dan audit ditunda — pemanggil menjalankannya
            // lewat PublishDeferredAsync sesudah commit, atau membuangnya bila transaksi batal.
            if (_dbContext.Database.CurrentTransaction != null)
            {
                _deferredAfterCommit.Add(AfterCommitAsync);
            }
            else
            {
                await AfterCommitAsync();
            }

            return MapResult(encounter, queue, isReplay: false);
        }

        /// <summary>
        /// Menjalankan notifikasi antrean dan audit pendaftaran yang ditunda karena
        /// <see cref="RegisterAsync"/> berjalan di dalam transaksi pemanggil (<c>BE-MCU-013</c>).
        /// Dipanggil pemanggil <b>sesudah</b> transaksinya commit; tidak dipanggil bila batal.
        /// </summary>
        public async Task PublishDeferredAsync()
        {
            var pending = _deferredAfterCommit.ToList();
            _deferredAfterCommit.Clear();

            foreach (var action in pending)
            {
                await action();
            }
        }

        // =================================================================
        // Pembatalan kunjungan MCU — BE-MCU-025
        // =================================================================

        /// <summary>
        /// Membatalkan kunjungan <c>MedicalCheckup</c> beserta antreannya untuk batal penuh MCU
        /// (<c>MCU-DEC-021</c>). Kolom yang diisi dan aturan boleh-batal sama dengan pembatalan
        /// Daftar Pasien Rawat Jalan (<see cref="OutpatientEncounterListService.GetCancelBlockedReason"/>);
        /// yang berbeda hanya cakupannya — tidak memakai lingkup klinis petugas, karena pemanggilnya
        /// MCU yang sudah memeriksa hak <c>McuOrder : Cancel</c>.
        /// </summary>
        /// <remarks>
        /// Ikut transaksi pemanggil bila ada (keputusan BE-MCU-025, sama dengan <see cref="RegisterAsync"/>):
        /// tanpa commit/rollback sendiri, dan notifikasi antrean ditunda ke <see cref="PublishDeferredAsync"/>.
        /// Hanya kunjungan MCU yang boleh dibatalkan lewat jalur ini.
        /// </remarks>
        public async Task CancelMedicalCheckupAsync(
            Guid encounterId,
            string reason,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var normalizedReason = reason?.Trim();
            if (string.IsNullOrEmpty(normalizedReason))
                throw new EncounterIntakeValidationException("Alasan pembatalan wajib diisi.");
            if (normalizedReason.Length > 250)
                normalizedReason = normalizedReason[..250];

            var ownsTransaction = _dbContext.Database.CurrentTransaction == null;
            await using var transaction = ownsTransaction
                ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                : null;

            var encounter = _dbContext.Database.IsNpgsql()
                ? await _dbContext.Set<RegPatientEncounter>()
                    .FromSqlInterpolated($@"
                        SELECT *
                        FROM public.""RegPatientEncounter""
                        WHERE ""Id"" = {encounterId}
                          AND ""IsDelete"" = false
                        FOR UPDATE")
                    .FirstOrDefaultAsync(cancellationToken)
                : await _dbContext.Set<RegPatientEncounter>()
                    .FirstOrDefaultAsync(x => x.Id == encounterId && !x.IsDelete, cancellationToken);

            if (encounter == null)
                throw new EncounterIntakeValidationException("Kunjungan tidak ditemukan.");

            if (encounter.EncounterType != EncounterType.MedicalCheckup)
                throw new EncounterIntakeValidationException("Jalur ini hanya untuk kunjungan Medical Check-Up.");

            var hasActiveConsultation = await _dbContext.Set<TrxDoctorConsultation>()
                .AnyAsync(c => c.EncounterId == encounterId && !c.IsDelete && !c.IsCancel, cancellationToken);

            var blockedReason = OutpatientEncounterListService.GetCancelBlockedReason(
                encounter.EncounterStatus,
                encounter.IsCancel,
                encounter.CompletedAt.HasValue,
                hasActiveConsultation);

            if (blockedReason != null)
                throw new EncounterIntakeConflictException(blockedReason);

            var now = DateTime.UtcNow;
            encounter.CancelledAt = now;
            encounter.CancelledByUserId = actorUserId;
            encounter.CancelReason = normalizedReason;
            encounter.IsCancel = true;
            encounter.CancelDateTime = now;
            encounter.CancelBy = actorUserId;
            encounter.IsActive = false;
            encounter.UpdateDateTime = now;
            encounter.UpdateBy = actorUserId;

            var cancelledQueues = await _dbContext.Set<RegQueue>()
                .Where(x => x.EncounterId == encounterId && !x.IsDelete && !x.CompletedAt.HasValue && !x.CancelledAt.HasValue)
                .ToListAsync(cancellationToken);

            foreach (var queue in cancelledQueues)
            {
                queue.CancelledAt = now;
                queue.CancelledByUserId = actorUserId;
                queue.CancelReason = normalizedReason;
                queue.IsCancel = true;
                queue.CancelDateTime = now;
                queue.CancelBy = actorUserId;
                queue.IsActive = false;
                queue.UpdateDateTime = now;
                queue.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            async Task AfterCommitAsync()
            {
                foreach (var queue in cancelledQueues)
                {
                    try
                    {
                        if (_queueRealtimeService != null)
                            await _queueRealtimeService.NotifyQueueCancelledAsync(queue, actorUserId, "Kunjungan Medical Check-Up dibatalkan.");
                    }
                    catch (Exception exception)
                    {
                        await _loggerService.WarningAsync(LogCategory, "CancelMedicalCheckupEncounter",
                            "Notifikasi antrean batal gagal dikirim.", new { EntityId = queue.Id, Error = exception.GetType().Name });
                    }
                }

                // Tanpa alasan batal, nama pasien, maupun data medis.
                await _loggerService.AuditAsync(LogCategory, "CancelMedicalCheckupEncounter",
                    $"Kunjungan Medical Check-Up {encounter.EncounterNumber} dibatalkan, {cancelledQueues.Count} antrean ikut batal.");
            }

            if (transaction != null)
            {
                await transaction.CommitAsync(cancellationToken);
                await AfterCommitAsync();
            }
            else
            {
                _deferredAfterCommit.Add(AfterCommitAsync);
            }
        }

        // =================================================================
        // Kewenangan
        // =================================================================

        /// <summary>
        /// Seam pemeriksaan kewenangan. <c>protected virtual</c> semata-mata agar uji dapat
        /// menggantinya tanpa menyusun seluruh struktur RBAC; produksi selalu memakai
        /// <c>AccessPermissionService</c> yang sebenarnya.
        /// </summary>
        protected virtual async Task<bool> HasRegistrationAuthorityAsync(
            CancellationToken cancellationToken)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            return await _accessPermissionService.HasAccessAsync(user, "PatientEncounter", "Create");
        }

        // =================================================================
        // Validasi
        // =================================================================

        private async Task<MstPatient> LoadPatientAsync(
            Guid patientId,
            CancellationToken cancellationToken)
        {
            // VAL-40. Pasien wajib sudah terdaftar; jalur ini tidak pernah membuat data induk
            // pasien, dan tidak boleh dibuat begitu.
            return await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == patientId && x.IsActive && !x.IsDelete,
                    cancellationToken)
                ?? throw new EncounterIntakeValidationException(
                    "Pasien tidak ditemukan atau sudah tidak aktif. Daftarkan pasien lebih dulu di modul Pasien.");
        }

        private async Task<MstServiceUnit> LoadServiceUnitAsync(
            Guid serviceUnitId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == serviceUnitId && x.IsActive && !x.IsDelete,
                    cancellationToken)
                ?? throw new EncounterIntakeValidationException(
                    "Unit layanan tidak ditemukan atau sudah tidak aktif.");
        }

        /// <summary>
        /// Menentukan bentuk kunjungan menurut jenisnya. <c>Outpatient</c> tetap kunjungan
        /// penunjang tanpa antrean, persis perilaku sebelum <c>BE-MCU-001</c>.
        /// <c>MedicalCheckup</c> mengikuti master klinik MCU seperti jalur loket
        /// (<c>PatientEncounterController</c>), sehingga skrining perawat berjalan di nurse
        /// station yang sama.
        /// </summary>
        private async Task<IntakeVisitProfile> ResolveVisitProfileAsync(
            EncounterIntakeRequest request,
            CancellationToken cancellationToken)
        {
            var clinicId = NormalizeNullableGuid(request.ClinicId);
            var doctorId = NormalizeNullableGuid(request.DoctorId);

            if (request.EncounterType == EncounterType.Outpatient)
            {
                // Klinik dan dokter diabaikan diam-diam akan menghasilkan kunjungan yang tidak
                // sesuai harapan pemanggil; lebih baik ditolak.
                if (clinicId.HasValue || doctorId.HasValue)
                {
                    throw new EncounterIntakeValidationException(
                        "Klinik dan dokter hanya berlaku untuk kunjungan Medical Check-Up.");
                }

                return IntakeVisitProfile.Supporting;
            }

            if (request.EncounterType != EncounterType.MedicalCheckup)
            {
                throw new EncounterIntakeValidationException(
                    "Jalur ini hanya melayani kunjungan penunjang dan Medical Check-Up.");
            }

            if (!clinicId.HasValue)
            {
                throw new EncounterIntakeValidationException(
                    "Klinik Medical Check-Up wajib dipilih untuk kunjungan Medical Check-Up.");
            }

            var clinic = await _dbContext.Set<MstClinic>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == clinicId.Value &&
                         x.ServiceUnitId == request.ServiceUnitId &&
                         x.ClinicType == ClinicType.MedicalCheckup &&
                         x.IsAvailableForRegistration &&
                         x.IsActive &&
                         !x.IsDelete,
                    cancellationToken)
                ?? throw new EncounterIntakeValidationException(
                    "Klinik tidak valid: bukan klinik Medical Check-Up, tidak aktif, tidak tersedia untuk registrasi, atau tidak berada pada unit layanan yang dipilih.");

            if (clinic.IsDoctorRequired && !doctorId.HasValue)
            {
                throw new EncounterIntakeValidationException(
                    "Dokter MCU wajib dipilih karena klinik Medical Check-Up mewajibkan dokter.");
            }

            if (doctorId.HasValue)
            {
                var doctorExists = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.Id == doctorId.Value && x.IsActive && !x.IsDelete,
                        cancellationToken);

                if (!doctorExists)
                {
                    throw new EncounterIntakeValidationException(
                        "Dokter tidak valid atau tidak aktif.");
                }
            }

            return new IntakeVisitProfile(
                EncounterType.MedicalCheckup,
                clinic,
                doctorId,
                clinic.IsQueueRequired,
                clinic.IsScreeningRequired,
                clinic.IsDoctorRequired);
        }

        private async Task ValidateReferralAsync(
            EncounterIntakeRequest request,
            CancellationToken cancellationToken)
        {
            if (!request.IsReferral)
            {
                // Kunjungan bukan rujukan tidak boleh membawa penunjuk perujuk. Tanpa penjagaan
                // ini, kunjungan dapat tersimpan dengan perujuk terisi tetapi penanda rujukan
                // mati — dan laporan asal rujukan akan melewatkannya diam-diam.
                if (request.ReferralInstitutionId.HasValue || request.ReferralDoctorId.HasValue)
                {
                    throw new EncounterIntakeValidationException(
                        "Instansi dan dokter perujuk hanya berlaku untuk kunjungan rujukan.");
                }

                return;
            }

            // VAL-44.
            if (NormalizeNullableText(request.ReferralNumber) == null)
            {
                throw new EncounterIntakeValidationException(
                    "Nomor surat rujukan wajib diisi untuk pasien rujukan.");
            }

            // VAL-43 dan AC-50. Yang diterima hanya penunjuk ke data induk perujuk; nama yang
            // diketik bebas tidak punya tempat untuk masuk sama sekali.
            if (!request.ReferralInstitutionId.HasValue)
            {
                throw new EncounterIntakeValidationException(
                    "Pilih instansi perujuk dari daftar. Bila belum ada, hubungi bagian data induk untuk menambahkannya.");
            }

            var institutionExists = await _dbContext.Set<MstReferralInstitution>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == request.ReferralInstitutionId.Value && x.IsActive && !x.IsDelete,
                    cancellationToken);

            if (!institutionExists)
            {
                throw new EncounterIntakeValidationException(
                    "Pilih instansi perujuk dari daftar. Bila belum ada, hubungi bagian data induk untuk menambahkannya.");
            }

            // DEC-FRJ-001: alur Rawat Jalan mewajibkan mitra layak pada tanggal kunjungan (hari
            // ini; rujukan Laboratorium hanya untuk hari ini, RJ-VAL-PM-15). Pemanggil lain yang
            // tidak menyetel penanda ini tetap memakai aturan lama.
            if (request.RequirePartnerEligibility &&
                await ReferralPartnerEligibility.FindEligibleAsync(
                    _dbContext,
                    request.ReferralInstitutionId.Value,
                    ReferralPartnerEligibility.Today(),
                    cancellationToken) == null)
            {
                throw new EncounterIntakeValidationException(EncounterReferralService.Pm17);
            }

            if (!request.ReferralDoctorId.HasValue)
            {
                return;
            }

            var doctorBelongsToInstitution = await _dbContext.Set<MstReferralDoctor>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == request.ReferralDoctorId.Value &&
                         x.ReferralInstitutionId == request.ReferralInstitutionId.Value &&
                         x.IsActive &&
                         !x.IsDelete,
                    cancellationToken);

            if (!doctorBelongsToInstitution)
            {
                throw new EncounterIntakeValidationException(
                    "Dokter perujuk tidak terdaftar pada instansi perujuk yang dipilih.");
            }
        }

        /// <summary>Kartu sumber bayar yang dipilih: asuransi, atau kartu penjamin perusahaan.</summary>
        private sealed record PaymentReference(
            MstPatientInsurance? Insurance,
            MstPatientCompanyGuarantor? CompanyCard);

        private async Task<PaymentReference> LoadPaymentSourceReferenceAsync(
            EncounterIntakeRequest request,
            IntakeVisitProfile visit,
            CancellationToken cancellationToken)
        {
            if (request.PaymentType == EncounterPaymentType.CompanyGuarantor)
            {
                // Sama seperti jalur kiosk: Penjamin Perusahaan hanya diterima petugas admisi
                // (RWI-ENC-PAYER-001 bagian 7), sehingga wewenangnya tidak ikut meluas ke sini.
                // Satu-satunya pengecualian adalah check-in MCU di meja admisi MCU (INT-MCU-02,
                // BE-MCU-013): peserta korporat datang atas tanggungan perusahaannya.
                if (visit.EncounterType != EncounterType.MedicalCheckup)
                {
                    throw new EncounterIntakeValidationException(
                        "Penjamin perusahaan hanya dapat dipilih lewat pendaftaran loket.");
                }

                return new PaymentReference(null, await LoadCompanyCardAsync(request, cancellationToken));
            }

            if (request.PaymentType != EncounterPaymentType.Insurance)
            {
                return new PaymentReference(null, null);
            }

            if (!request.PatientInsuranceId.HasValue)
            {
                throw new EncounterIntakeValidationException(
                    "Kartu asuransi pasien wajib dipilih untuk pembayaran Asuransi.");
            }

            var insurance = await _dbContext.Set<MstPatientInsurance>()
                .AsNoTracking()
                .Include(x => x.InsuranceProvider)
                .FirstOrDefaultAsync(
                    x => x.Id == request.PatientInsuranceId.Value &&
                         x.PatientId == request.PatientId &&
                         x.IsActive &&
                         !x.IsDelete,
                    cancellationToken)
                ?? throw new EncounterIntakeValidationException(
                    "Kartu asuransi tidak ditemukan atau bukan milik pasien ini.");

            return new PaymentReference(insurance, null);
        }

        /// <summary>
        /// Kartu penjamin perusahaan untuk kunjungan MCU. Pemeriksaannya disalin dari penggantian
        /// sumber bayar loket (<c>EncounterPaymentSourceService</c>, <c>BIL-VAL-068</c>..<c>072</c>)
        /// supaya kartu yang ditolak loket juga ditolak di sini.
        /// </summary>
        private async Task<MstPatientCompanyGuarantor> LoadCompanyCardAsync(
            EncounterIntakeRequest request,
            CancellationToken cancellationToken)
        {
            if (!request.PatientCompanyGuarantorId.HasValue)
            {
                throw new EncounterIntakeValidationException(
                    "Kartu penjamin perusahaan pasien wajib dipilih untuk pembayaran Penjamin Perusahaan.");
            }

            var card = await _dbContext.Set<MstPatientCompanyGuarantor>()
                .AsNoTracking()
                .Include(x => x.CompanyGuarantor)
                .FirstOrDefaultAsync(x => x.Id == request.PatientCompanyGuarantorId.Value, cancellationToken);

            if (card == null || card.IsDelete || !card.IsActive)
            {
                throw new EncounterIntakeValidationException(
                    "Kartu penjamin yang dipilih sudah tidak berlaku. Perbarui data penjamin pasien di Registrasi.");
            }

            if (card.PatientId != request.PatientId)
            {
                throw new EncounterIntakeValidationException("Kartu penjamin yang dipilih bukan milik pasien ini.");
            }

            var serviceDate = AppDateTimeHelper.OperationalDate();

            if ((card.EffectiveStartDate.HasValue && card.EffectiveStartDate.Value.Date > serviceDate)
                || (card.EffectiveEndDate.HasValue && card.EffectiveEndDate.Value.Date < serviceDate))
            {
                throw new EncounterIntakeValidationException(
                    "Kartu penjamin ini tidak berlaku pada tanggal pelayanan. Pilih kartu lain atau perbarui masa berlakunya di Registrasi.");
            }

            if (!card.IsEligible)
            {
                throw new EncounterIntakeValidationException(
                    "Kartu penjamin ini belum dinyatakan layak. Periksa kelayakannya di Registrasi sebelum dipakai.");
            }

            var guarantor = card.CompanyGuarantor;
            if (guarantor is null || guarantor.IsDelete || !guarantor.IsActive
                || (guarantor.ContractStartDate.HasValue && guarantor.ContractStartDate.Value.Date > serviceDate)
                || (guarantor.ContractEndDate.HasValue && guarantor.ContractEndDate.Value.Date < serviceDate))
            {
                throw new EncounterIntakeValidationException(
                    "Kerja sama dengan perusahaan penjamin ini sudah berakhir pada tanggal pelayanan.");
            }

            // Kartu benefit karyawan internal butuh verifikasi HR (BE-FIN-106) yang hanya ada di loket.
            if (guarantor.IsEmployeeBenefitGuarantor)
            {
                throw new EncounterIntakeValidationException(
                    "Penjamin benefit karyawan internal hanya dapat dipilih lewat pendaftaran loket.");
            }

            return card;
        }

        // =================================================================
        // Penyimpanan
        // =================================================================

        private async Task<(RegPatientEncounter Encounter, RegQueue? Queue)> PersistAsync(
            EncounterIntakeRequest request,
            string? idempotencyKey,
            MstPatient patient,
            MstServiceUnit serviceUnit,
            IntakeVisitProfile visit,
            PaymentReference paymentReference,
            Guid? patientClassId,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            // Kunjungan berantrean memakai tanggal operasional (WIB) seperti jalur loket, karena
            // nurse station menyaring antrean menurut tanggal itu. Kunjungan penunjang tetap
            // memakai waktu pendaftaran seperti sebelumnya.
            var operationalDate = DateTime.SpecifyKind(
                AppDateTimeHelper.OperationalDate(), DateTimeKind.Utc);

            // BE-MCU-013, INT-MCU-02: check-in MCU memanggil service ini di dalam transaksinya
            // sendiri, agar kunjungan, baris paket, dan order Hadir tersimpan atau batal bersama.
            // Bila sudah ada transaksi terbuka, service ini memakainya — percobaan ulang nomor
            // kembali ke savepoint, dan commit tetap milik pemanggil. Tanpa transaksi pemanggil,
            // perilakunya sama persis dengan sebelumnya.
            var ambientTransaction = _dbContext.Database.CurrentTransaction;

            for (var attempt = 1; ; attempt++)
            {
                await using var transaction = ambientTransaction == null
                    ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                    : null;

                if (ambientTransaction != null)
                {
                    await ambientTransaction.CreateSavepointAsync(IntakeSavepoint, cancellationToken);
                }

                var encounter = new RegPatientEncounter
                {
                    Id = Guid.NewGuid(),
                    EncounterNumber = await AllocateRunningCodeAsync<RegPatientEncounter>(
                        x => x.EncounterNumber, EncounterCodePrefix, cancellationToken),
                    PatientId = patient.Id,
                    ServiceUnitId = serviceUnit.Id,
                    ClinicId = visit.Clinic?.Id,
                    DoctorId = visit.DoctorId,
                    PatientClassId = patientClassId,
                    EncounterDate = visit.IsQueueRequired ? operationalDate : now,
                    EncounterType = visit.EncounterType,
                    VisitType = VisitType.NewVisit,
                    RegistrationSource = EncounterRegistrationSource.WalkIn,
                    EncounterStatus = EncounterStatus.Registered,
                    PaymentType = request.PaymentType,
                    PaymentMethodId = request.PaymentType == EncounterPaymentType.Cash
                        ? NormalizeNullableGuid(request.PaymentMethodId)
                        : null,
                    IsReferral = request.IsReferral,
                    ReferralNumber = NormalizeNullableText(request.ReferralNumber),
                    ReferralInstitutionId = NormalizeNullableGuid(request.ReferralInstitutionId),
                    ReferralDoctorId = NormalizeNullableGuid(request.ReferralDoctorId),
                    IsWalkIn = true,

                    // Pasien penunjang tidak mengantre dokter dan tidak menunggu skrining
                    // perawat: ia menuju unit penunjang, dan daftar kerjanya ada di modul
                    // penunjang itu sendiri, sehingga ketiganya false dan tidak ada RegQueue.
                    // Peserta MCU mengikuti master klinik MCU (IntakeVisitProfile).
                    IsQueueRequired = visit.IsQueueRequired,
                    IsDoctorRequired = visit.IsDoctorRequired,
                    IsScreeningRequired = visit.IsScreeningRequired,

                    RegisteredAt = now,
                    RegisteredByUserId = actorUserId,
                    Notes = NormalizeNullableText(request.Notes),
                    RegistrationIdempotencyKey = idempotencyKey,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                var paymentSource = await BuildPaymentSourceAsync(
                    encounter, request, paymentReference, actorUserId, now, cancellationToken);

                encounter.PaymentSource = paymentSource;

                _dbContext.Set<RegPatientEncounter>().Add(encounter);
                _dbContext.Set<RegPatientEncounterGuarantor>().Add(paymentSource);

                var queue = visit.IsQueueRequired
                    ? await BuildQueueAsync(encounter, visit, actorUserId, now, cancellationToken)
                    : null;

                if (queue != null)
                {
                    _dbContext.Set<RegQueue>().Add(queue);
                    encounter.EncounterStatus = visit.IsScreeningRequired
                        ? EncounterStatus.WaitingForNurse
                        : EncounterStatus.WaitingForDoctor;
                }

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    if (transaction != null)
                    {
                        await transaction.CommitAsync(cancellationToken);
                    }
                    else
                    {
                        await ambientTransaction!.ReleaseSavepointAsync(IntakeSavepoint, cancellationToken);
                    }

                    return (encounter, queue);
                }
                catch (DbUpdateException exception) when (IsUniqueViolation(exception))
                {
                    if (transaction != null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                    }
                    else
                    {
                        await ambientTransaction!.RollbackToSavepointAsync(IntakeSavepoint, cancellationToken);
                    }

                    DetachPending(encounter, paymentSource, queue);

                    // Dua permintaan dengan kunci yang sama tiba bersamaan: keduanya membaca
                    // "belum ada", lalu unique index memutuskan siapa yang menang. Yang kalah
                    // membaca ulang milik pemenang — hasil akhirnya tetap satu kunjungan.
                    if (idempotencyKey != null)
                    {
                        var winner = await FindByIdempotencyKeyAsync(idempotencyKey, cancellationToken);

                        if (winner != null)
                        {
                            return (winner, await FindQueueAsync(winner.Id, cancellationToken));
                        }
                    }

                    // Sisanya adalah tabrakan nomor kunjungan atau nomor antrean; keduanya
                    // dialokasikan ulang pada percobaan berikutnya.
                    if (attempt >= AllocationAttempts)
                    {
                        throw new EncounterIntakeConflictException(
                            "Pendaftaran gagal karena nomor kunjungan sedang diperebutkan. Silakan coba lagi.");
                    }
                }
            }
        }

        private void DetachPending(
            RegPatientEncounter encounter,
            RegPatientEncounterGuarantor paymentSource,
            RegQueue? queue)
        {
            encounter.PaymentSource = null;

            if (queue != null)
            {
                _dbContext.Entry(queue).State = EntityState.Detached;
            }

            _dbContext.Entry(paymentSource).State = EntityState.Detached;
            _dbContext.Entry(encounter).State = EntityState.Detached;
        }

        /// <summary>
        /// Antrean kunjungan berklinik. Klasifikasi prioritas dan nomornya memakai service yang
        /// sama dengan jalur loket (RJ-DOC-DEC-055/056), sehingga nomor tidak bercabang dan
        /// nurse station memperlakukan antrean ini seperti antrean poli lainnya.
        /// </summary>
        private async Task<RegQueue> BuildQueueAsync(
            RegPatientEncounter encounter,
            IntakeVisitProfile visit,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var classification = await _queueClassificationService.ClassifyAsync(
                encounter.PatientId,
                encounter.EncounterDate,
                cancellationToken);

            var allocation = await _queueNumberAllocator.AllocateAsync(
                encounter.EncounterDate,
                encounter.ServiceUnitId,
                encounter.ClinicId,
                classification.IsPriorityQueue,
                cancellationToken);

            return new RegQueue
            {
                Id = Guid.NewGuid(),
                EncounterId = encounter.Id,
                PatientId = encounter.PatientId,
                ServiceUnitId = encounter.ServiceUnitId,
                ClinicId = encounter.ClinicId,
                DoctorId = encounter.DoctorId,
                QueueDate = encounter.EncounterDate,
                QueueNumber = allocation.QueueNumber,
                QueueCode = GenerateQueueCode(visit.Clinic, allocation.QueueNumber),
                QueueScopeKey = allocation.QueueScopeKey,
                QueueStatus = visit.IsScreeningRequired
                    ? QueueStatus.WaitingForNurse
                    : QueueStatus.WaitingForDoctor,
                IsPriorityQueue = classification.IsPriorityQueue,
                QueuePriorityLevelSnapshot = classification.PriorityLevel,
                QueueAudienceSnapshot = classification.QueueAudience,
                PublicDisplayModeSnapshot = classification.PublicDisplayMode,
                PatientMembershipIdSnapshot = classification.PatientMembershipId,
                MembershipTierIdSnapshot = classification.MembershipTierId,
                MembershipTierCodeSnapshot = classification.MembershipTierCode,
                PriorityReasonCode = classification.PriorityReasonCode,
                IsFromKiosk = encounter.IsFromKiosk,
                IsWalkIn = encounter.IsWalkIn,
                IsAppointment = encounter.IsAppointment,
                IsScreeningRequired = visit.IsScreeningRequired,
                IsDoctorRequired = visit.IsDoctorRequired,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };
        }

        private async Task NotifyQueueCreatedAsync(RegQueue? queue, Guid actorUserId)
        {
            if (queue == null || _queueRealtimeService == null)
            {
                return;
            }

            // Sama seperti jalur loket: kunjungan dan antrean sudah commit, sehingga kegagalan
            // notifikasi realtime tidak boleh menggagalkan pendaftaran.
            try
            {
                await _queueRealtimeService.NotifyQueueCreatedAsync(
                    queue,
                    actorUserId,
                    "Antrean pasien baru dibuat.");
            }
            catch (Exception exception)
            {
                try
                {
                    await _loggerService.ErrorAsync(
                        LogCategory,
                        "EncounterIntake.QueueNotification",
                        $"Kunjungan dan antrean tersimpan, tetapi notifikasi realtime gagal. EncounterId={queue.EncounterId}; QueueId={queue.Id}.",
                        exception);
                }
                catch
                {
                    // Pencatatan kegagalan notifikasi tidak boleh menggagalkan pendaftaran.
                }
            }
        }

        private async Task<RegPatientEncounterGuarantor> BuildPaymentSourceAsync(
            RegPatientEncounter encounter,
            EncounterIntakeRequest request,
            PaymentReference paymentReference,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var patientInsurance = paymentReference.Insurance;

            var paymentSource = new RegPatientEncounterGuarantor
            {
                Id = Guid.NewGuid(),
                PaymentSourceNumber = await AllocateRunningCodeAsync<RegPatientEncounterGuarantor>(
                    x => x.PaymentSourceNumber, PaymentSourceCodePrefix, cancellationToken),
                EncounterId = encounter.Id,
                PatientId = encounter.PatientId,
                PaymentType = request.PaymentType,
                Priority = request.Priority > 0 ? request.Priority : 1,
                IsPrimary = request.IsPrimary,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            if (paymentReference.CompanyCard is { } card)
            {
                // Snapshot yang sama dengan penggantian sumber bayar loket (EncounterPaymentSourceService),
                // supaya Billing membaca penjamin perusahaan MCU persis seperti penjamin loket.
                var guarantor = card.CompanyGuarantor!;
                paymentSource.PatientCompanyGuarantorId = card.Id;
                paymentSource.CompanyGuarantorId = card.CompanyGuarantorId;
                paymentSource.PaymentSourceNameSnapshot = guarantor.CompanyGuarantorName;
                paymentSource.CompanyGuarantorCodeSnapshot = guarantor.CompanyGuarantorCode;
                paymentSource.EmployeeNumberSnapshot = card.EmployeeNumber;
                paymentSource.EmployeeNameSnapshot = card.EmployeeName;
                paymentSource.EmployeeIdSnapshot = card.EmployeeId;
                paymentSource.FamilyMemberIdSnapshot = card.FamilyMemberId;
                paymentSource.BenefitRelationshipSnapshot = card.BenefitRelationship;
                paymentSource.BenefitPlanCodeSnapshot = card.BenefitPlanCode;
                paymentSource.PlanNameSnapshot = card.BenefitPlanName;
                paymentSource.ClassNameSnapshot = card.ClassName;
                paymentSource.EffectiveStartDateSnapshot = card.EffectiveStartDate;
                paymentSource.EffectiveEndDateSnapshot = card.EffectiveEndDate;
                paymentSource.IsEligible = card.IsEligible;
                paymentSource.IsPolicyActive = true;

                return paymentSource;
            }

            if (patientInsurance == null)
            {
                paymentSource.PaymentMethodId = encounter.PaymentMethodId;
                paymentSource.PaymentSourceNameSnapshot = "Tunai";
                paymentSource.IsEligible = true;
                paymentSource.IsPolicyActive = false;

                return paymentSource;
            }

            // Seluruh nilai di bawah adalah snapshot pada waktu registrasi, sehingga audit
            // kunjungan tidak ikut berubah ketika kartu asuransinya disunting kemudian.
            paymentSource.PatientInsuranceId = patientInsurance.Id;
            paymentSource.InsuranceProviderId = patientInsurance.InsuranceProviderId;
            paymentSource.PaymentSourceNameSnapshot =
                patientInsurance.InsuranceProvider?.InsuranceProviderName;
            paymentSource.PolicyNumberSnapshot = NormalizeNullableText(patientInsurance.PolicyNumber);
            paymentSource.CardNumberSnapshot = NormalizeNullableText(patientInsurance.CardNumber);
            paymentSource.MemberNumberSnapshot = NormalizeNullableText(patientInsurance.MemberNumber);
            paymentSource.PlanNameSnapshot = NormalizeNullableText(patientInsurance.PlanName);
            paymentSource.ClassNameSnapshot = NormalizeNullableText(patientInsurance.ClassName);
            paymentSource.BenefitPlanCodeSnapshot =
                NormalizeNullableText(patientInsurance.BenefitPlanCode);
            paymentSource.EffectiveStartDateSnapshot = patientInsurance.EffectiveStartDate;
            paymentSource.EffectiveEndDateSnapshot = patientInsurance.EffectiveEndDate;
            paymentSource.IsEligible = patientInsurance.IsEligible;
            paymentSource.IsPolicyActive = true;

            return paymentSource;
        }

        // =================================================================
        // Pembantu
        // =================================================================

        private async Task<RegPatientEncounter?> FindByIdempotencyKeyAsync(
            string idempotencyKey,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.RegistrationIdempotencyKey == idempotencyKey && !x.IsDelete,
                    cancellationToken);
        }

        private async Task<RegQueue?> FindQueueAsync(
            Guid encounterId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<RegQueue>()
                .AsNoTracking()
                .Where(x => x.EncounterId == encounterId && !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<Guid?> ResolveOutpatientPatientClassIdAsync(
            CancellationToken cancellationToken)
        {
            // Kelas pasien diselesaikan backend memakai business key yang sama dengan jalur
            // loket, karena GUID master berbeda antar-environment. Bila masternya belum ada,
            // kunjungan tetap sah — kolomnya memang boleh kosong.
            var candidates = await _dbContext.Set<MstPatientClass>()
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .Select(x => new { x.Id, x.PatientClassName })
                .ToListAsync(cancellationToken);

            return candidates
                .FirstOrDefault(x =>
                    string.Equals(
                        NormalizeLookupText(x.PatientClassName),
                        DefaultOutpatientPatientClassName,
                        StringComparison.OrdinalIgnoreCase))
                ?.Id;
        }

        private async Task<string> AllocateRunningCodeAsync<TEntity>(
            Expression<Func<TEntity, string>> selector,
            string prefix,
            CancellationToken cancellationToken)
            where TEntity : class
        {
            // Format dan perilakunya disamakan dengan PatientEncounterController agar penomoran
            // kedua jalur menyatu. Yang berbeda hanya perlindungannya: di sini tabrakan
            // ditangkap unique index lalu dialokasikan ulang oleh pemanggil, bukan diandaikan
            // tidak pernah terjadi (QBE-CODE-003, QBE-CODE-004).
            var existingCodes = await _dbContext.Set<TEntity>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Select(selector)
                .Where(x => x.StartsWith(prefix))
                .ToListAsync(cancellationToken);

            var usedNumbers = existingCodes
                .Select(x => x.Replace(prefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber)) nextNumber++;

            return prefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
        }

        // Kode antrean disamakan persis dengan PatientEncounterController.GenerateQueueCode:
        // huruf pertama nama pendek klinik (atau nama klinik tanpa kata "Poli"/"Klinik") diikuti
        // tiga digit nomor, sehingga layar antrean menampilkan kedua jalur dengan bentuk sama.
        private static string GenerateQueueCode(MstClinic? clinic, int queueNumber) =>
            $"{BuildQueueCodePrefix(clinic)}{queueNumber:D3}";

        private static string BuildQueueCodePrefix(MstClinic? clinic)
        {
            var source = !string.IsNullOrWhiteSpace(clinic?.ShortName)
                ? clinic.ShortName.Trim()
                : BuildClinicNamePrefixSource(clinic?.ClinicName);

            foreach (var character in source.Trim().ToUpperInvariant())
            {
                if (character >= 'A' && character <= 'Z')
                {
                    return character.ToString();
                }
            }

            return "Q";
        }

        private static string BuildClinicNamePrefixSource(string? clinicName)
        {
            if (string.IsNullOrWhiteSpace(clinicName))
            {
                return string.Empty;
            }

            var ignoredWords = new[] { "POLI", "POLIKLINIK", "KLINIK", "CLINIC" };

            return clinicName
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault(token => !ignoredWords.Contains(token.ToUpperInvariant()))
                ?? clinicName.Trim();
        }

        private static EncounterIntakeResult MapResult(
            RegPatientEncounter encounter,
            RegQueue? queue,
            bool isReplay) =>
            new()
            {
                EncounterId = encounter.Id,
                EncounterNumber = encounter.EncounterNumber,
                PatientId = encounter.PatientId,
                EncounterDate = encounter.EncounterDate,
                EncounterStatus = encounter.EncounterStatus,
                QueueId = queue?.Id,
                QueueCode = queue?.QueueCode,
                IsReplay = isReplay
            };

        private static bool IsUniqueViolation(DbUpdateException exception)
        {
            return exception.InnerException?.GetType().Name == "PostgresException" &&
                   exception.InnerException.Message.Contains(
                       "duplicate key value", StringComparison.OrdinalIgnoreCase);
        }

        private static Guid? NormalizeNullableGuid(Guid? value) =>
            value.HasValue && value.Value != Guid.Empty ? value : null;

        private static string? NormalizeNullableText(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string NormalizeLookupText(string? value) =>
            string.Join(
                ' ',
                (value ?? string.Empty).Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }

        /// <summary>Bentuk kunjungan yang sudah divalidasi: jenis, klinik, dan kebutuhan antreannya.</summary>
        private sealed record IntakeVisitProfile(
            EncounterType EncounterType,
            MstClinic? Clinic,
            Guid? DoctorId,
            bool IsQueueRequired,
            bool IsScreeningRequired,
            bool IsDoctorRequired)
        {
            public static IntakeVisitProfile Supporting { get; } =
                new(EncounterType.Outpatient, null, null, false, false, false);
        }
    }

    /// <summary>Isian pendaftaran tidak memenuhi aturan Registrasi. Dipetakan menjadi <c>422</c>.</summary>
    public sealed class EncounterIntakeValidationException(string message) : Exception(message);

    /// <summary>Pemanggil tidak berhak membuat kunjungan. Dipetakan menjadi <c>403</c>.</summary>
    public sealed class EncounterIntakeForbiddenException(string message) : Exception(message);

    /// <summary>Bentrokan dengan pendaftaran lain yang sedang berjalan. Dipetakan menjadi <c>409</c>.</summary>
    public sealed class EncounterIntakeConflictException(string message) : Exception(message);
}
