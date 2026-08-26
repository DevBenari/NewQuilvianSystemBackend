using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Membaca angka pengaturan Rawat Inap dari master, dan menyediakan nilai bawaan bila
    /// baris pengaturan belum terisi. Seluruh service Rawat Inap membaca angkanya lewat sini,
    /// sehingga tidak ada satu pun batas waktu yang ditanam di kode.
    /// </summary>
    /// <remarks>
    /// Dua sifat yang mengikat service ini, keduanya dari RWI-DEC-008:
    ///
    /// 1. Tidak ada penyimpanan sementara (cache). Setiap pemanggilan membaca ulang dari
    ///    database, sehingga angka yang baru diubah admin berlaku pada pembacaan berikutnya
    ///    tanpa aplikasi perlu dinyalakan ulang. Ini yang dituntut RWI-AC-003 dan RWI-AC-110.
    /// 2. Bila baris pengaturan belum ada, service memakai nilai bawaan DAN mencatat
    ///    peringatan. Peringatan itu bukan hiasan: tanpa peringatan, angka bawaan yang salah
    ///    dapat terpakai berbulan-bulan di produksi tanpa ada yang menyadarinya.
    /// </remarks>
    public class InpSettingService
    {
        private const string LogCategory = "HealthServices.InPatientManagement.Setting";

        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<InpSettingService> _logger;

        public InpSettingService(
            ApplicationDbContext dbContext,
            ILogger<InpSettingService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Mengambil angka pengaturan yang berlaku. Baris yang dipilih adalah baris aktif
        /// yang bertanda default; bila ada beberapa baris aktif, yang paling baru dibuat
        /// yang dipakai. Bila tidak ada satu pun baris, nilai bawaan dikembalikan beserta
        /// satu peringatan pada log.
        /// </summary>
        public async Task<InpatientSettingValues> GetEffectiveSettingAsync(
            CancellationToken cancellationToken = default)
        {
            var entity = await GetEffectiveSettingEntityAsync(cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning(
                    "[{Category}] Baris pengaturan Rawat Inap tidak ditemukan. Modul memakai " +
                    "nilai bawaan: pemesanan {BedReservationMinutes} menit, episode Draft " +
                    "gugur {DraftEpisodeExpiryHours} jam, awalan nomor {EpisodeNumberPrefix}. " +
                    "Isi master pengaturan lewat layar admin atau jalankan seeder master " +
                    "Rawat Inap supaya angka yang dipakai benar-benar ditetapkan rumah sakit.",
                    LogCategory,
                    InpatientSettingValues.Defaults.BedReservationMinutes,
                    InpatientSettingValues.Defaults.DraftEpisodeExpiryHours,
                    InpatientSettingValues.Defaults.EpisodeNumberPrefix);

                return InpatientSettingValues.Defaults;
            }

            return InpatientSettingValues.From(entity);
        }

        /// <summary>
        /// Mengambil awalan nomor episode yang berlaku, misalnya <c>RI</c>. Dipakai
        /// <see cref="InpEpisodeNumberService"/> supaya awalan tidak pernah ditanam di kode.
        /// </summary>
        public async Task<string> GetEpisodeNumberPrefixAsync(
            CancellationToken cancellationToken = default)
        {
            var setting = await GetEffectiveSettingAsync(cancellationToken);

            return setting.EpisodeNumberPrefix;
        }

        /// <summary>
        /// Mengambil baris pengaturan yang berlaku apa adanya, tanpa penggantian nilai bawaan.
        /// Mengembalikan <c>null</c> bila master pengaturan memang belum terisi.
        /// </summary>
        public Task<MstInpatientSetting?> GetEffectiveSettingEntityAsync(
            CancellationToken cancellationToken = default)
        {
            return _dbContext.Set<MstInpatientSetting>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Angka pengaturan Rawat Inap yang berlaku pada satu saat pembacaan. Bentuk ini sengaja
    /// dipisahkan dari entity supaya pemanggilnya tidak dapat mengubah baris master secara
    /// tidak sengaja lewat objek yang dibacanya.
    /// </summary>
    public sealed record InpatientSettingValues(
        int BedReservationMinutes,
        int DraftEpisodeExpiryHours,
        int InitialAssessmentTargetHours,
        int ProgressNoteVerificationTargetHours,
        int PendingClosureThresholdHours,
        string EpisodeNumberPrefix,
        bool IsFromMasterData)
    {
        /// <summary>
        /// Nilai bawaan yang dipakai hanya ketika master pengaturan belum terisi. Angkanya
        /// sama persis dengan yang di-seed <c>InpatientMasterDataSeeder</c>, mengikuti
        /// 02-backend-architecture.md bagian 8.1.
        /// </summary>
        public static readonly InpatientSettingValues Defaults = new(
            BedReservationMinutes: 120,
            DraftEpisodeExpiryHours: 24,
            InitialAssessmentTargetHours: 24,
            ProgressNoteVerificationTargetHours: 24,
            PendingClosureThresholdHours: 4,
            EpisodeNumberPrefix: "RI",
            IsFromMasterData: false);

        public static InpatientSettingValues From(MstInpatientSetting entity)
            => new(
                BedReservationMinutes: entity.BedReservationMinutes,
                DraftEpisodeExpiryHours: entity.DraftEpisodeExpiryHours,
                InitialAssessmentTargetHours: entity.InitialAssessmentTargetHours,
                ProgressNoteVerificationTargetHours: entity.ProgressNoteVerificationTargetHours,
                PendingClosureThresholdHours: entity.PendingClosureThresholdHours,
                EpisodeNumberPrefix: string.IsNullOrWhiteSpace(entity.EpisodeNumberPrefix)
                    ? Defaults.EpisodeNumberPrefix
                    : entity.EpisodeNumberPrefix.Trim().ToUpperInvariant(),
                IsFromMasterData: true);
    }
}
