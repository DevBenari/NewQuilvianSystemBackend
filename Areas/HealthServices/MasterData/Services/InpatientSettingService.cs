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

            entity.Name = NormalizeText(request.Name) ?? entity.Name;
            entity.BedReservationMinutes = request.BedReservationMinutes;
            entity.DraftEpisodeExpiryHours = request.DraftEpisodeExpiryHours;
            entity.InitialAssessmentTargetHours = request.InitialAssessmentTargetHours;
            entity.ProgressNoteVerificationTargetHours = request.ProgressNoteVerificationTargetHours;
            entity.PendingClosureThresholdHours = request.PendingClosureThresholdHours;
            entity.EpisodeNumberPrefix = NormalizePrefix(request.EpisodeNumberPrefix);
            entity.IsActive = request.IsActive;
            entity.Notes = NormalizeText(request.Notes);
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
    }

    public enum InpatientSettingUpdateStatus
    {
        Success = 0,
        NotFound = 1,
        Invalid = 2
    }

    public sealed record InpatientSettingUpdateResult(
        InpatientSettingUpdateStatus Status,
        MstInpatientSetting? Entity,
        string Message);
}
