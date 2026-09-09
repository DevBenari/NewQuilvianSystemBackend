using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
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
    /// melayani pasien datang langsung adalah menulis sendiri ke <c>TrxPatientEncounter</c> —
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

        private const string DefaultOutpatientPatientClassName = "RAWAT JALAN";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AccessPermissionService _accessPermissionService;
        private readonly LoggerService _loggerService;

        public EncounterIntakeService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            AccessPermissionService accessPermissionService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _accessPermissionService = accessPermissionService;
            _loggerService = loggerService;
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
                    return MapResult(replay, isReplay: true);
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

            await ValidateReferralAsync(request, cancellationToken);

            var patientInsurance = await LoadPaymentSourceReferenceAsync(request, cancellationToken);

            var patientClassId = await ResolveOutpatientPatientClassIdAsync(cancellationToken);

            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var encounter = await PersistAsync(
                request,
                idempotencyKey,
                patient,
                serviceUnit,
                patientInsurance,
                patientClassId,
                actorUserId,
                now,
                cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "EncounterIntake.Register",
                "Kunjungan dibuat lewat jalur pendaftaran modul penunjang (INT-05).",
                new
                {
                    encounter.Id,
                    encounter.EncounterNumber,
                    encounter.PatientId,
                    encounter.ServiceUnitId,
                    encounter.IsReferral,
                    ActorUserId = actorUserId
                });

            return MapResult(encounter, isReplay: false);
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

        private async Task<MstPatientInsurance?> LoadPaymentSourceReferenceAsync(
            EncounterIntakeRequest request,
            CancellationToken cancellationToken)
        {
            if (request.PaymentType == EncounterPaymentType.CompanyGuarantor)
            {
                // Sama seperti jalur kiosk: Penjamin Perusahaan hanya diterima petugas admisi
                // (RWI-ENC-PAYER-001 bagian 7), sehingga wewenangnya tidak ikut meluas ke sini.
                throw new EncounterIntakeValidationException(
                    "Penjamin perusahaan hanya dapat dipilih lewat pendaftaran loket.");
            }

            if (request.PaymentType != EncounterPaymentType.Insurance)
            {
                return null;
            }

            if (!request.PatientInsuranceId.HasValue)
            {
                throw new EncounterIntakeValidationException(
                    "Kartu asuransi pasien wajib dipilih untuk pembayaran Asuransi.");
            }

            return await _dbContext.Set<MstPatientInsurance>()
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
        }

        // =================================================================
        // Penyimpanan
        // =================================================================

        private async Task<TrxPatientEncounter> PersistAsync(
            EncounterIntakeRequest request,
            string? idempotencyKey,
            MstPatient patient,
            MstServiceUnit serviceUnit,
            MstPatientInsurance? patientInsurance,
            Guid? patientClassId,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            for (var attempt = 1; ; attempt++)
            {
                await using var transaction =
                    await _dbContext.Database.BeginTransactionAsync(cancellationToken);

                var encounter = new TrxPatientEncounter
                {
                    Id = Guid.NewGuid(),
                    EncounterNumber = await AllocateRunningCodeAsync<TrxPatientEncounter>(
                        x => x.EncounterNumber, EncounterCodePrefix, cancellationToken),
                    PatientId = patient.Id,
                    ServiceUnitId = serviceUnit.Id,
                    PatientClassId = patientClassId,
                    EncounterDate = now,
                    EncounterType = EncounterType.Outpatient,
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
                    // penunjang itu sendiri. Karena itu tidak ada TrxQueue yang dibuat di sini.
                    IsQueueRequired = false,
                    IsDoctorRequired = false,
                    IsScreeningRequired = false,

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
                    encounter, request, patientInsurance, actorUserId, now, cancellationToken);

                encounter.PaymentSource = paymentSource;

                _dbContext.Set<TrxPatientEncounter>().Add(encounter);
                _dbContext.Set<TrxPatientEncounterGuarantor>().Add(paymentSource);

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return encounter;
                }
                catch (DbUpdateException exception) when (IsUniqueViolation(exception))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    DetachPending(encounter, paymentSource);

                    // Dua permintaan dengan kunci yang sama tiba bersamaan: keduanya membaca
                    // "belum ada", lalu unique index memutuskan siapa yang menang. Yang kalah
                    // membaca ulang milik pemenang — hasil akhirnya tetap satu kunjungan.
                    if (idempotencyKey != null)
                    {
                        var winner = await FindByIdempotencyKeyAsync(idempotencyKey, cancellationToken);

                        if (winner != null)
                        {
                            return winner;
                        }
                    }

                    // Sisanya adalah tabrakan nomor kunjungan; nomor berikutnya dialokasikan ulang.
                    if (attempt >= AllocationAttempts)
                    {
                        throw new EncounterIntakeConflictException(
                            "Pendaftaran gagal karena nomor kunjungan sedang diperebutkan. Silakan coba lagi.");
                    }
                }
            }
        }

        private void DetachPending(
            TrxPatientEncounter encounter,
            TrxPatientEncounterGuarantor paymentSource)
        {
            encounter.PaymentSource = null;
            _dbContext.Entry(paymentSource).State = EntityState.Detached;
            _dbContext.Entry(encounter).State = EntityState.Detached;
        }

        private async Task<TrxPatientEncounterGuarantor> BuildPaymentSourceAsync(
            TrxPatientEncounter encounter,
            EncounterIntakeRequest request,
            MstPatientInsurance? patientInsurance,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var paymentSource = new TrxPatientEncounterGuarantor
            {
                Id = Guid.NewGuid(),
                PaymentSourceNumber = await AllocateRunningCodeAsync<TrxPatientEncounterGuarantor>(
                    x => x.PaymentSourceNumber, PaymentSourceCodePrefix, cancellationToken),
                EncounterId = encounter.Id,
                PatientId = encounter.PatientId,
                PaymentType = request.PaymentType,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

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

        private async Task<TrxPatientEncounter?> FindByIdempotencyKeyAsync(
            string idempotencyKey,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<TrxPatientEncounter>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.RegistrationIdempotencyKey == idempotencyKey && !x.IsDelete,
                    cancellationToken);
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

        private static EncounterIntakeResult MapResult(TrxPatientEncounter encounter, bool isReplay) =>
            new()
            {
                EncounterId = encounter.Id,
                EncounterNumber = encounter.EncounterNumber,
                PatientId = encounter.PatientId,
                EncounterDate = encounter.EncounterDate,
                EncounterStatus = encounter.EncounterStatus,
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
    }

    /// <summary>Isian pendaftaran tidak memenuhi aturan Registrasi. Dipetakan menjadi <c>422</c>.</summary>
    public sealed class EncounterIntakeValidationException(string message) : Exception(message);

    /// <summary>Pemanggil tidak berhak membuat kunjungan. Dipetakan menjadi <c>403</c>.</summary>
    public sealed class EncounterIntakeForbiddenException(string message) : Exception(message);

    /// <summary>Bentrokan dengan pendaftaran lain yang sedang berjalan. Dipetakan menjadi <c>409</c>.</summary>
    public sealed class EncounterIntakeConflictException(string message) : Exception(message);
}
