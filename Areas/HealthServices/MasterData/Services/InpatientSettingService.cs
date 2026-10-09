using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services
{
    /// <summary>
    /// Pemilik pembacaan dan perubahan baris master pengaturan Rawat Inap. Controller
    /// <c>InpatientSettingController</c> tidak menyentuh <c>ApplicationDbContext</c> sendiri;
    /// seluruh pembacaan dan perubahannya lewat service ini, sesuai QBE-SVC-001.
    /// </summary>
    /// <remarks>
    /// <b>Jangan tertukar dengan <c>InpSettingService</c>.</b> Keduanya berbeda pemilik dan
    /// berbeda tugas:
    ///
    /// <list type="bullet">
    /// <item><description>
    /// <c>InpatientSettingService</c> — milik modul Master Data. Melayani layar admin:
    /// membaca satu baris pengaturan dan mengubah nilainya.
    /// </description></item>
    /// <item><description>
    /// <c>InpSettingService</c> — milik modul Rawat Inap. Melayani service lain: membaca
    /// angka yang berlaku, dan menyediakan nilai bawaan bila master belum terisi.
    /// </description></item>
    /// </list>
    /// </remarks>
    public class InpatientSettingService
    {
        private readonly ApplicationDbContext _dbContext;

        public InpatientSettingService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Mengambil baris pengaturan yang berlaku. Baris aktif yang bertanda default
        /// didahulukan; bila ada beberapa baris aktif, yang paling baru dibuat yang dipakai.
        /// Mengembalikan <c>null</c> bila master memang belum terisi.
        /// </summary>
        public Task<MstInpatientSetting?> GetEffectiveAsync(
            CancellationToken cancellationToken = default)
        {
            return _dbContext.Set<MstInpatientSetting>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Mengubah nilai satu baris pengaturan. Kode baris tidak ikut berubah: tabel ini
        /// dipakai sebagai satu baris tunggal berkode <c>DEFAULT</c>, dan mengganti kodenya
        /// akan membuat seluruh modul kehilangan baris yang dibacanya.
        /// </summary>
        public async Task<InpatientSettingUpdateResult> UpdateAsync(
            Guid id,
            UpdateInpatientSettingRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstInpatientSetting>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return new InpatientSettingUpdateResult(
                    InpatientSettingUpdateStatus.NotFound,
                    null,
                    "Data pengaturan Rawat Inap tidak ditemukan.");
            }

            var validationMessage = await ValidateAsync(entity, request, cancellationToken);

            if (validationMessage != null)
            {
                return new InpatientSettingUpdateResult(
                    InpatientSettingUpdateStatus.Invalid,
                    null,
                    validationMessage);
            }

            // BE-RWI-186 / validation 15.7 — isian cetak Workspace PPRI.
            var printFieldFailure = ValidatePrintFields(request);

            if (printFieldFailure != null)
                return printFieldFailure;

            entity.Name = NormalizeText(request.Name) ?? entity.Name;
            entity.BedReservationMinutes = request.BedReservationMinutes;
            entity.DraftEpisodeExpiryHours = request.DraftEpisodeExpiryHours;
            entity.InitialAssessmentTargetHours = request.InitialAssessmentTargetHours;
            entity.ProgressNoteVerificationTargetHours = request.ProgressNoteVerificationTargetHours;
            entity.PendingClosureThresholdHours = request.PendingClosureThresholdHours;
            entity.DepositFollowUpIntervalDays = request.DepositFollowUpIntervalDays;
            // BE-RWI-172: isian kosong dari layar lama mempertahankan nilai yang tersimpan.
            if (request.PendingSurgicalHandoverAlertMinutes.HasValue)
                entity.PendingSurgicalHandoverAlertMinutes = request.PendingSurgicalHandoverAlertMinutes.Value;
            if (request.PendingAdmissionReferralAlertMinutes.HasValue)
                entity.PendingAdmissionReferralAlertMinutes = request.PendingAdmissionReferralAlertMinutes.Value;
            entity.EpisodeNumberPrefix = NormalizePrefix(request.EpisodeNumberPrefix);
            entity.IsActive = request.IsActive;
            entity.Notes = NormalizeText(request.Notes);

            // BE-RWI-186: isian yang tidak dikirim (null) mempertahankan nilai tersimpan; teks
            // kosong mengosongkannya. Kode formulir kosong dicetak tanpa kode (RWI-DEC-247).
            entity.GeneralConsentFormCode = KeepOrReplace(entity.GeneralConsentFormCode, request.GeneralConsentFormCode);
            entity.NewPatientHandoverFormCode = KeepOrReplace(entity.NewPatientHandoverFormCode, request.NewPatientHandoverFormCode);
            entity.PrivacyRequestFormCode = KeepOrReplace(entity.PrivacyRequestFormCode, request.PrivacyRequestFormCode);
            entity.BeliefValuesFormCode = KeepOrReplace(entity.BeliefValuesFormCode, request.BeliefValuesFormCode);
            entity.CostDifferenceFormCode = KeepOrReplace(entity.CostDifferenceFormCode, request.CostDifferenceFormCode);
            entity.DepositSettlementFormCode = KeepOrReplace(entity.DepositSettlementFormCode, request.DepositSettlementFormCode);
            entity.CostEstimateFormCode = KeepOrReplace(entity.CostEstimateFormCode, request.CostEstimateFormCode);
            entity.InpatientBaseDataFormCode = KeepOrReplace(entity.InpatientBaseDataFormCode, request.InpatientBaseDataFormCode);
            entity.DocumentSigningCity = KeepOrReplace(entity.DocumentSigningCity, request.DocumentSigningCity);
            entity.PatientLabelHospitalCode = KeepOrReplace(entity.PatientLabelHospitalCode, request.PatientLabelHospitalCode);
            if (request.InfantWristbandMaxAgeYears.HasValue)
                entity.InfantWristbandMaxAgeYears = request.InfantWristbandMaxAgeYears.Value;

            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new InpatientSettingUpdateResult(
                InpatientSettingUpdateStatus.Success,
                entity,
                "Data pengaturan Rawat Inap berhasil diubah.");
        }

        /// <summary>
        /// Menghitung berapa banyak baris pengaturan yang masih hidup. Dipakai untuk menjaga
        /// tabel ini tetap berisi satu baris.
        /// </summary>
        public Task<int> CountLiveSettingsAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.Set<MstInpatientSetting>()
                .AsNoTracking()
                .CountAsync(x => !x.IsDelete, cancellationToken);
        }

        /// <remarks>
        /// Satu aturan di sini tidak berasal dari batas angka, melainkan dari akibatnya.
        ///
        /// <b>Contoh.</b> Admin membuka layar pengaturan lalu mematikan tanda aktif pada
        /// satu-satunya baris pengaturan. Sejak saat itu modul Rawat Inap tidak menemukan
        /// baris mana pun, sehingga ia diam-diam kembali memakai angka bawaan: pemesanan
        /// 120 menit, walaupun rumah sakit sudah menyetelnya 90 menit sebulan sebelumnya.
        /// Tidak ada satu pun layar yang menampilkan hal itu sebagai kesalahan. Karena itu
        /// menonaktifkan baris terakhir ditolak di sini.
        /// </remarks>
        private async Task<string?> ValidateAsync(
            MstInpatientSetting entity,
            UpdateInpatientSettingRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return "Nama pengaturan wajib diisi.";

            if (string.IsNullOrWhiteSpace(request.EpisodeNumberPrefix))
                return "Awalan nomor episode wajib diisi.";

            if (request.BedReservationMinutes is < 1 or > 1440)
                return "Lama pemesanan tempat tidur harus antara 1 dan 1440 menit.";

            if (request.DraftEpisodeExpiryHours is < 1 or > 720)
                return "Lama episode Draft boleh telantar harus antara 1 dan 720 jam.";

            if (request.InitialAssessmentTargetHours is < 1 or > 720)
                return "Target pengkajian awal harus antara 1 dan 720 jam.";

            if (request.ProgressNoteVerificationTargetHours is < 1 or > 720)
                return "Target verifikasi catatan perkembangan harus antara 1 dan 720 jam.";

            if (request.PendingClosureThresholdHours is < 1 or > 720)
                return "Ambang episode tertahan menunggu penutupan harus antara 1 dan 720 jam.";

            if (request.DepositFollowUpIntervalDays is < 1 or > 365)
                return "Ambang tindak lanjut kekurangan deposit harus antara 1 dan 365 hari.";

            // BE-RWI-172 / AC 4: ambang Daftar Pantau Finishing 1–1440 menit (paling lama satu hari).
            if (request.PendingSurgicalHandoverAlertMinutes is < 1 or > 1440)
                return "Ambang serah terima pasca operasi tertunda harus antara 1 dan 1440 menit.";

            if (request.PendingAdmissionReferralAlertMinutes is < 1 or > 1440)
                return "Ambang permintaan admisi tertunda harus antara 1 dan 1440 menit.";

            if (!request.IsActive && entity.IsActive)
            {
                var otherActiveExists = await _dbContext.Set<MstInpatientSetting>()
                    .AsNoTracking()
                    .AnyAsync(
                        x => !x.IsDelete && x.IsActive && x.Id != entity.Id,
                        cancellationToken);

                if (!otherActiveExists)
                {
                    return
                        "Pengaturan ini satu-satunya yang masih aktif, sehingga tidak dapat " +
                        "dinonaktifkan. Tanpa pengaturan aktif, modul Rawat Inap kembali " +
                        "memakai angka bawaan tanpa ada yang memberitahu petugas.";
                }
            }

            return null;
        }

        private static string NormalizePrefix(string value)
            => value.Trim().ToUpperInvariant();

        private static string? NormalizeText(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? KeepOrReplace(string? current, string? requested)
            => requested == null ? current : NormalizeText(requested);

        /// <summary>
        /// Validation 15.7: batas umur gelang bayi 0–16 (<c>MST-IST-001</c>); kode formulir 50, kota
        /// 100, dan kode label 30 karakter (<c>MST-IST-002</c>).
        /// </summary>
        /// <remarks>
        /// Contoh: admin mengisi batas umur gelang bayi 17 → ditolak "Batas umur gelang bayi 0 sampai
        /// 16 tahun." Mengisi kode formulir Selisih Biaya sepanjang 60 karakter → ditolak "Kode
        /// formulir Selisih Biaya terlalu panjang."
        /// </remarks>
        private static InpatientSettingUpdateResult? ValidatePrintFields(UpdateInpatientSettingRequest request)
        {
            if (request.InfantWristbandMaxAgeYears is < 0 or > 16)
            {
                return new InpatientSettingUpdateResult(
                    InpatientSettingUpdateStatus.Invalid,
                    null,
                    "Batas umur gelang bayi 0 sampai 16 tahun.",
                    "MST-IST-001");
            }

            var lengthRules = new (string? Value, int Max, string Label)[]
            {
                (request.GeneralConsentFormCode, 50, "Kode formulir General Consent"),
                (request.NewPatientHandoverFormCode, 50, "Kode formulir Serah Terima Pasien Baru"),
                (request.PrivacyRequestFormCode, 50, "Kode formulir Permintaan Privasi"),
                (request.BeliefValuesFormCode, 50, "Kode formulir Nilai Kepercayaan"),
                (request.CostDifferenceFormCode, 50, "Kode formulir Selisih Biaya"),
                (request.DepositSettlementFormCode, 50, "Kode formulir Pelunasan Deposit"),
                (request.CostEstimateFormCode, 50, "Kode formulir Estimasi Biaya"),
                (request.InpatientBaseDataFormCode, 50, "Kode formulir IPD"),
                (request.DocumentSigningCity, 100, "Kota penandatanganan"),
                (request.PatientLabelHospitalCode, 30, "Kode rumah sakit pada label")
            };

            foreach (var rule in lengthRules)
            {
                if (rule.Value != null && rule.Value.Trim().Length > rule.Max)
                {
                    return new InpatientSettingUpdateResult(
                        InpatientSettingUpdateStatus.Invalid,
                        null,
                        $"{rule.Label} terlalu panjang.",
                        "MST-IST-002");
                }
            }

            return null;
        }
    }

    public enum InpatientSettingUpdateStatus
    {
        Success = 0,
        NotFound = 1,
        Invalid = 2
    }

    /// <param name="Code">Kode alasan validation 15.7 (<c>MST-IST-001</c>, <c>002</c>); kosong untuk kegagalan lama.</param>
    public sealed record InpatientSettingUpdateResult(
        InpatientSettingUpdateStatus Status,
        MstInpatientSetting? Entity,
        string Message,
        string? Code = null);
}
