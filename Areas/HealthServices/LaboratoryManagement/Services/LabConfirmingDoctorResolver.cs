using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Menyusun pilihan Dokter Konfirmator beserta sumbernya (<c>LAB-DEC-111</c>,
    /// <c>BE-LAB-59</c>).
    ///
    /// <b>Nol tabel baru dan nol jembatan baru.</b> Rantainya sudah lengkap pada satu
    /// <see cref="ApplicationDbContext"/>:
    /// <code>
    /// TrxOnCallAssignment (aktif pada jam ini)
    ///     -> WorkforceProfileId
    ///     -> MstDoctor.WorkforceProfileId
    ///     -> FullName + WhatsAppNumber
    /// </code>
    ///
    /// <b><c>MstDoctorSchedule</c> sengaja TIDAK dipakai</b> (<c>LAB-DEC-111</c> butir 4). Ia
    /// jadwal praktik poliklinik — ber-<c>ClinicId</c> wajib, berkuota pasien, berpenanda
    /// kiosk — dan <c>ScheduleType</c>-nya nol memuat nilai yang berarti "sedang jaga".
    /// Memakainya berarti menawarkan dokter yang sedang praktik poli sebagai penerima kabar
    /// hasil kritis pukul dua pagi.
    /// </summary>
    public class LabConfirmingDoctorResolver
    {
        /// <summary>
        /// Batas jumlah dokter pada jalur jatuh.
        ///
        /// <b>Ada supaya daftarnya tidak pernah tumbuh tanpa batas.</b> Rumah sakit ini punya
        /// belasan dokter hari ini, dan itu justru kenapa batasnya dipasang sekarang: daftar
        /// tanpa batas nol terlihat bermasalah sampai jumlahnya berubah.
        /// </summary>
        public const int FallbackLimit = 50;

        private const string FallbackNoteText =
            "Jadwal jaga belum tersedia untuk jam ini, sehingga seluruh dokter aktif ditampilkan.";

        private readonly ApplicationDbContext _dbContext;

        public LabConfirmingDoctorResolver(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Menyusun pilihan bagi satu pemeriksaan.
        /// </summary>
        /// <param name="examinationId">Pemeriksaan yang sedang dikerjakan.</param>
        /// <param name="search">
        /// Penyaring nama pada jalur jatuh. Opsional; diabaikan ketika jadwal jaga tersedia.
        /// </param>
        public async Task<LabConfirmingDoctorOptionsResponse> ResolveAsync(
            Guid examinationId,
            string? search = null,
            CancellationToken cancellationToken = default)
        {
            // EncounterId diambil lewat LabOrder, bukan diasumsikan. BE-LAB-54 sudah sekali
            // gagal karena penunjuk induknya tidak ikut dimuat lalu jatuh ke Guid.Empty.
            var encounterId = await _dbContext.LabExaminations
                .AsNoTracking()
                .Where(x => x.Id == examinationId && !x.IsDelete)
                .Select(x => (Guid?)x.LabOrder!.EncounterId)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException("Pemeriksaan laboratorium tidak ditemukan.");

            var now = DateTime.UtcNow;

            var response = new LabConfirmingDoctorOptionsResponse
            {
                AttendingDoctor = await ReadAttendingDoctorAsync(encounterId, cancellationToken),
                OnDutyDoctors = await ReadOnDutyDoctorsAsync(now, cancellationToken)
            };

            response.OnDutyScheduleAvailable = response.OnDutyDoctors.Count > 0;

            // Jalur jatuh. LAB-DEC-111 butir 3 menegaskan ia BUKAN sementara: ia berlaku setiap
            // kali jadwal jaga kosong pada jam tersebut, juga bertahun-tahun dari sekarang.
            if (!response.OnDutyScheduleAvailable)
            {
                var (dokter, terpotong) = await ReadActiveDoctorsAsync(search, cancellationToken);

                response.FallbackDoctors = dokter;
                response.FallbackTruncated = terpotong;
                response.FallbackNote = FallbackNoteText;
            }

            return response;
        }

        /// <summary>
        /// DPJP kunjungan yang menaungi pesanan.
        ///
        /// <b><c>LabOrder.InstructingDoctorId</c> sengaja TIDAK digabungkan ke sini.</b> Ia
        /// dokter pemberi instruksi pada pesanan rawat inap (<c>BE-RWI-104</c>) — pertanyaan
        /// yang berbeda dari siapa DPJP pasien. Menggabungkannya diam-diam akan memberi satu
        /// nama label yang bukan miliknya.
        /// </summary>
        private async Task<LabDoctorOption?> ReadAttendingDoctorAsync(
            Guid encounterId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.RegPatientEncounters
                .AsNoTracking()
                .Where(x => x.Id == encounterId && x.DoctorId != null && x.Doctor != null)
                .Select(x => new LabDoctorOption
                {
                    DoctorId = x.Doctor!.Id,
                    FullName = x.Doctor.FullName,
                    WhatsAppNumber = x.Doctor.WhatsAppNumber
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Dokter yang penugasan jaganya aktif pada <paramref name="now"/>.
        ///
        /// <b>Penugasan yang berakhir tepat pada jam ini masih ikut</b> — perbandingannya
        /// inklusif di kedua ujung. Petugas yang menelepon pukul 07.00 sementara jaga malam
        /// berakhir 07.00 sedang menghubungi orang yang memang masih memegangnya.
        /// </summary>
        private async Task<List<LabDoctorOption>> ReadOnDutyDoctorsAsync(
            DateTime now,
            CancellationToken cancellationToken)
        {
            var profil = _dbContext.TrxOnCallAssignments
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.IsActive
                            && x.StartAt <= now
                            && x.EndAt >= now
                            // Penugasan yang sudah dibatalkan atau selesai nol berarti sedang
                            // jaga. Kolomnya bertipe teks pada tabel milik Human Resource, dan
                            // perbandingannya ditulis apa adanya supaya nol menebak nilai yang
                            // tidak ada.
                            && (x.AssignmentStatus == "Scheduled"
                                || x.AssignmentStatus == "Confirmed"
                                || x.AssignmentStatus == "Active"))
                .Select(x => x.WorkforceProfileId);

            return await _dbContext.MstDoctors
                .AsNoTracking()
                .Where(d => !d.IsDelete && d.IsActive && profil.Contains(d.WorkforceProfileId))
                .OrderBy(d => d.FullName)
                .Select(d => new LabDoctorOption
                {
                    DoctorId = d.Id,
                    FullName = d.FullName,
                    WhatsAppNumber = d.WhatsAppNumber
                })
                .ToListAsync(cancellationToken);
        }

        private async Task<(List<LabDoctorOption> Doctors, bool Truncated)> ReadActiveDoctorsAsync(
            string? search,
            CancellationToken cancellationToken)
        {
            IQueryable<MstDoctor> query = _dbContext.MstDoctors
                .AsNoTracking()
                .Where(d => !d.IsDelete && d.IsActive);

            var kata = search?.Trim();

            if (!string.IsNullOrEmpty(kata))
            {
                query = query.Where(d => EF.Functions.ILike(d.FullName, $"%{kata}%"));
            }

            // Satu baris lebih diambil daripada batasnya, semata-mata untuk mengetahui apakah
            // masih ada sisa — menghitung ulang seluruh tabel hanya untuk menyalakan satu
            // penanda adalah kueri kedua yang tidak perlu.
            var baris = await query
                .OrderBy(d => d.FullName)
                .Take(FallbackLimit + 1)
                .Select(d => new LabDoctorOption
                {
                    DoctorId = d.Id,
                    FullName = d.FullName,
                    WhatsAppNumber = d.WhatsAppNumber
                })
                .ToListAsync(cancellationToken);

            var terpotong = baris.Count > FallbackLimit;

            if (terpotong) baris.RemoveAt(baris.Count - 1);

            return (baris, terpotong);
        }
    }
}
