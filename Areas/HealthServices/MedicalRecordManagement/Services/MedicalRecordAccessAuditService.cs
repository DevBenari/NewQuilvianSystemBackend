using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services
{
    /// <summary>
    /// Keterangan satu permintaan pembukaan rekam medis, sebagaimana diterima controller.
    /// </summary>
    public sealed record MedicalRecordAccessRequest(
        Guid PatientId,
        Guid UserId,
        MedicalRecordAccessScope Scope,
        Guid? AccessPurposeId,
        string? AccessReason,
        string? IpAddress,
        string? ClientInfo,
        string? RequestPath);

    /// <summary>
    /// Hasil penilaian dan pencatatan satu pembukaan rekam medis.
    /// </summary>
    public sealed record MedicalRecordAccessResult(
        bool IsAllowed,
        int StatusCode,
        string? ErrorMessage,
        MedicalRecordAccessType AccessType,
        bool IsFlaggedForReview,
        Guid? AccessLogId)
    {
        public static MedicalRecordAccessResult Allowed(
            MedicalRecordAccessType type, bool flagged, Guid logId)
            => new(true, StatusCodes.Status200OK, null, type, flagged, logId);

        public static MedicalRecordAccessResult Denied(int statusCode, string message)
            => new(false, statusCode, message,
                   MedicalRecordAccessType.ReasonedAccess, false, null);
    }

    /// <summary>
    /// Menegakkan kewenangan tingkat pasien dan mencatat jejak setiap pembukaan berkas rekam
    /// medis (RM-DEC-005, RM-DEC-015, RM-DEC-016, RM-DEC-017).
    ///
    /// Ini lapisan kewenangan KEDUA. Lapisan pertama — apakah pengguna boleh membuka menu rekam
    /// medis sama sekali — sudah ditegakkan sistem hak akses yang ada. Lapisan kedua menjawab
    /// pertanyaan yang tidak dapat dijawab lapisan pertama: bolehkah pengguna ini membuka rekam
    /// medis PASIEN INI.
    ///
    /// ATURAN YANG MENUTUP RAPAT. Pencatatan jejak diselesaikan SEBELUM isi rekam medis
    /// dikembalikan. Bila pencatatan gagal, isi tidak dikembalikan sama sekali. Ini pilihan
    /// sadar: membaca diam-diam dinilai lebih berbahaya daripada tidak bisa membaca.
    /// </summary>
    public class MedicalRecordAccessAuditService
    {
        private readonly ApplicationDbContext _dbContext;

        public MedicalRecordAccessAuditService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Status kunjungan yang dianggap sudah tidak berjalan.
        /// </summary>
        private static readonly EncounterStatus[] StatusKunjunganSelesai =
        [
            EncounterStatus.Completed,
            EncounterStatus.Cancelled,
            EncounterStatus.NoShow
        ];

        /// <summary>
        /// Menilai kewenangan lalu mencatat jejaknya. Panggil ini SEBELUM mengambil isi rekam
        /// medis.
        ///
        /// Bila hasilnya tidak diizinkan, controller wajib mengembalikan galat tanpa menyentuh
        /// isi rekam medis sama sekali — termasuk tidak memuatnya lebih dulu untuk kemudian
        /// disembunyikan.
        /// </summary>
        public async Task<MedicalRecordAccessResult> EvaluateAndRecordAsync(
            MedicalRecordAccessRequest request,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
        {
            if (request.PatientId == Guid.Empty)
            {
                return MedicalRecordAccessResult.Denied(
                    StatusCodes.Status404NotFound, "Pasien tidak ditemukan.");
            }

            var pasien = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => x.Id == request.PatientId && !x.IsDelete)
                .Select(x => new { x.Id, x.MergedToPatientId, x.MedicalRecordNumber })
                .FirstOrDefaultAsync(cancellationToken);

            if (pasien == null)
            {
                return MedicalRecordAccessResult.Denied(
                    StatusCodes.Status404NotFound, "Pasien tidak ditemukan.");
            }

            // RM-CAP-007 — pasien hasil penggabungan. Riwayatnya pasti terpecah karena
            // penggabungan di sistem ini hanya berupa penandaan dan tidak memindahkan data
            // klinis. Menampilkan riwayat sebagian tanpa peringatan lebih berbahaya daripada
            // menolak: riwayat tidak lengkap akan dibaca sebagai riwayat lengkap.
            if (pasien.MergedToPatientId.HasValue && pasien.MergedToPatientId.Value != Guid.Empty)
            {
                var pengganti = await _dbContext.Set<MstPatient>()
                    .AsNoTracking()
                    .Where(x => x.Id == pasien.MergedToPatientId.Value)
                    .Select(x => x.MedicalRecordNumber)
                    .FirstOrDefaultAsync(cancellationToken);

                return MedicalRecordAccessResult.Denied(
                    StatusCodes.Status409Conflict,
                    $"Nomor rekam medis ini sudah digabungkan. Buka nomor rekam medis " +
                    $"penggantinya ({pengganti ?? "tidak diketahui"}) agar riwayat tampil utuh.");
            }

            var punyaKunjunganAktif = await PunyaKunjunganAktifAsync(
                request.PatientId, cancellationToken);

            var jenisAkses = punyaKunjunganAktif
                ? MedicalRecordAccessType.RoutineCare
                : MedicalRecordAccessType.ReasonedAccess;

            // Catatan pribadi SELALU menuntut keperluan, bahkan untuk pasien rawatan sendiri
            // (RM-DEC-022). Kolom itu ditulis dengan harapan bersifat pribadi, sehingga
            // membukanya selalu merupakan tindakan yang perlu dipertanggungjawabkan.
            var wajibBeralasan = jenisAkses == MedicalRecordAccessType.ReasonedAccess
                                 || request.Scope == MedicalRecordAccessScope.PrivateNote;

            if (wajibBeralasan)
            {
                jenisAkses = MedicalRecordAccessType.ReasonedAccess;

                var pemeriksaan = await PeriksaKeperluanAsync(request, cancellationToken);
                if (pemeriksaan != null)
                    return pemeriksaan;
            }

            var perluDitinjau = await PerluDitinjauAsync(
                jenisAkses, request.AccessPurposeId, cancellationToken);

            var jejak = new TrxMedicalRecordAccessLog
            {
                PatientId = request.PatientId,
                UserId = request.UserId,
                UserDisplayNameSnapshot = await AmbilNamaPenggunaAsync(request.UserId, cancellationToken),
                UserRoleSnapshot = null,
                AccessType = jenisAkses,
                AccessScope = request.Scope,
                AccessPurposeId = request.AccessPurposeId,
                AccessReason = Potong(request.AccessReason, 500),
                HasActiveEncounter = punyaKunjunganAktif,
                IsFlaggedForReview = perluDitinjau,
                AccessedAt = nowUtc,
                IpAddress = Potong(request.IpAddress, 64),
                ClientInfo = Potong(request.ClientInfo, 250),
                RequestPath = Potong(request.RequestPath, 250),
                CreateDateTime = nowUtc,
                CreateBy = request.UserId
            };

            try
            {
                await _dbContext.Set<TrxMedicalRecordAccessLog>()
                    .AddAsync(jejak, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception)
            {
                // Gagal mencatat jejak berarti gagal membaca. Isi rekam medis tidak boleh
                // dikembalikan tanpa jejaknya.
                //
                // Konsekuensi yang diterima sadar: gangguan pada tabel jejak akan menghambat
                // pembacaan rekam medis. Itu lebih baik daripada ada pembacaan yang tidak
                // tercatat.
                return MedicalRecordAccessResult.Denied(
                    StatusCodes.Status503ServiceUnavailable,
                    "Berkas tidak dapat dibuka saat ini. Silakan coba lagi.");
            }

            return MedicalRecordAccessResult.Allowed(jenisAkses, perluDitinjau, jejak.Id);
        }

        /// <summary>
        /// Menilai apakah pasien sedang memiliki kunjungan aktif, yaitu kunjungan yang belum
        /// ditutup (RM-DEC-016).
        ///
        /// Bila penilaian gagal karena gangguan, hasilnya dianggap TIDAK punya kunjungan aktif.
        /// Kegagalan teknis tidak boleh berubah menjadi pelonggaran kewenangan.
        /// </summary>
        public async Task<bool> PunyaKunjunganAktifAsync(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbContext.Set<TrxPatientEncounter>()
                    .AsNoTracking()
                    .AnyAsync(x => x.PatientId == patientId
                                   && !x.IsDelete
                                   && !x.IsCancel
                                   && x.CompletedAt == null
                                   && !StatusKunjunganSelesai.Contains(x.EncounterStatus),
                              cancellationToken);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<MedicalRecordAccessResult?> PeriksaKeperluanAsync(
            MedicalRecordAccessRequest request,
            CancellationToken cancellationToken)
        {
            if (!request.AccessPurposeId.HasValue || request.AccessPurposeId.Value == Guid.Empty)
            {
                var pesan = request.Scope == MedicalRecordAccessScope.PrivateNote
                    ? "Membuka catatan pribadi selalu memerlukan keperluan akses."
                    : "Pasien ini sedang tidak dalam perawatan Anda. Pilih keperluan akses terlebih dahulu.";

                return MedicalRecordAccessResult.Denied(StatusCodes.Status400BadRequest, pesan);
            }

            var keperluan = await _dbContext.Set<MstMedicalRecordAccessPurpose>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.AccessPurposeId.Value && !x.IsDelete,
                                     cancellationToken);

            if (keperluan == null || !keperluan.IsActive)
            {
                return MedicalRecordAccessResult.Denied(
                    StatusCodes.Status400BadRequest,
                    "Keperluan akses yang dipilih sudah tidak berlaku. Pilih yang lain.");
            }

            if (keperluan.IsFreeTextRequired && string.IsNullOrWhiteSpace(request.AccessReason))
            {
                return MedicalRecordAccessResult.Denied(
                    StatusCodes.Status400BadRequest,
                    "Keperluan yang Anda pilih mengharuskan penjelasan. Tuliskan alasannya.");
            }

            return null;
        }

        private async Task<bool> PerluDitinjauAsync(
            MedicalRecordAccessType jenisAkses,
            Guid? accessPurposeId,
            CancellationToken cancellationToken)
        {
            if (jenisAkses == MedicalRecordAccessType.RoutineCare)
                return false;

            if (!accessPurposeId.HasValue)
                return true;

            var perlu = await _dbContext.Set<MstMedicalRecordAccessPurpose>()
                .AsNoTracking()
                .Where(x => x.Id == accessPurposeId.Value)
                .Select(x => (bool?)x.RequiresReview)
                .FirstOrDefaultAsync(cancellationToken);

            return perlu ?? true;
        }

        private async Task<string> AmbilNamaPenggunaAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            var nama = await _dbContext.Set<ApplicationUser>()
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => x.DisplayName)
                .FirstOrDefaultAsync(cancellationToken);

            return string.IsNullOrWhiteSpace(nama) ? "Pengguna tidak diketahui" : nama;
        }

        private static string? Potong(string? nilai, int panjangMaksimum)
        {
            if (string.IsNullOrWhiteSpace(nilai))
                return null;

            var bersih = nilai.Trim();
            return bersih.Length <= panjangMaksimum ? bersih : bersih[..panjangMaksimum];
        }
    }
}
