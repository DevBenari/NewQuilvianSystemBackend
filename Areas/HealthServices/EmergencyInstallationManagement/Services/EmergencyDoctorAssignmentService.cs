using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Linq.Expressions;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    /// <summary>
    /// Penetapan, pengalihan, dan pencarian dokter penanggung jawab kunjungan IGD.
    /// </summary>
    /// <remarks>
    /// <c>BE-IGD-045</c>. Seluruh aturan bisnis tinggal di sini; controller hanya menerjemahkan
    /// hasilnya menjadi kode status. Penugasan berjalan dan penugasan pada waktu tertentu
    /// dihitung HANYA dari <c>EffectiveFrom</c> dan <c>EffectiveTo</c> sesuai
    /// <c>IGD-DEC-130</c>; <c>IsActive</c> tidak pernah dibaca sebagai status penugasan.
    /// </remarks>
    public class EmergencyDoctorAssignmentService
    {
        private const string IndexPenugasanBerjalan = "IX_EmgDoctorAssignment_EmergencyVisitId_Active";

        private readonly ApplicationDbContext _dbContext;

        public EmergencyDoctorAssignmentService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public class Hasil<T>
        {
            private Hasil(T? data, int statusCode, string? penolakan)
            {
                Data = data;
                StatusCode = statusCode;
                Penolakan = penolakan;
            }

            public T? Data { get; }

            public int StatusCode { get; }

            public string? Penolakan { get; }

            public bool Berhasil => Penolakan == null;

            public static Hasil<T> Ok(T data, int statusCode = StatusCodes.Status200OK)
                => new(data, statusCode, null);

            public static Hasil<T> Gagal(int statusCode, string penolakan)
                => new(default, statusCode, penolakan);
        }

        /// <summary>
        /// Bentuk balasan tunggal untuk daftar, pencarian, penetapan, dan pengalihan.
        /// </summary>
        /// <remarks>
        /// API 0.7.0 bagian 3.2, <c>IGD-DEC-129</c>. Ditulis sebagai satu expression supaya
        /// nama dokter dan nama penugas ikut terbawa SATU kueri lewat relasi <c>Doctor</c> dan
        /// <c>AssignedByUser</c> yang sudah dikonfigurasi, jadi nol <c>N+1</c>. Nama yang tidak
        /// tersedia menjadi <c>null</c>, bukan GUID. Urutan cadangan nama penugas mengikuti
        /// pola <c>recordedByName</c> pada <c>BE-IGD-046</c>.
        /// </remarks>
        private static readonly Expression<Func<EmgDoctorAssignment, EmergencyDoctorAssignmentResponse>> ProyeksiResponse =
            x => new EmergencyDoctorAssignmentResponse
            {
                Id = x.Id,
                EmergencyVisitId = x.EmergencyVisitId,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor == null ? null : x.Doctor.FullName,
                EffectiveFrom = x.EffectiveFrom,
                EffectiveTo = x.EffectiveTo,
                AssignedByUserId = x.AssignedByUserId,
                AssignedByName = x.AssignedByUser == null
                    ? null
                    : x.AssignedByUser.DisplayName ?? x.AssignedByUser.UserName ?? x.AssignedByUser.Email ?? x.AssignedByUser.UserCode,
                AssignmentReason = x.AssignmentReason
            };

        /// <summary>
        /// Riwayat penugasan satu kunjungan, urut waktu dari yang pertama.
        /// </summary>
        public async Task<IReadOnlyList<EmergencyDoctorAssignmentResponse>> AmbilRiwayatAsync(
            Guid emergencyVisitId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<EmgDoctorAssignment>()
                .AsNoTracking()
                .Where(x => x.EmergencyVisitId == emergencyVisitId && !x.IsDelete)
                .OrderBy(x => x.EffectiveFrom)
                .ThenBy(x => x.CreateDateTime)
                .Select(ProyeksiResponse)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Dokter penanggung jawab sekarang, atau pada waktu tertentu bila <paramref name="at"/>
        /// diisi.
        /// </summary>
        /// <remarks>
        /// <c>IGD-DEC-117</c> dan <c>IGD-DEC-130</c>. Berjalan berarti <c>EffectiveTo</c>
        /// kosong; pada waktu T berarti <c>EffectiveFrom</c> tidak melewati T dan
        /// <c>EffectiveTo</c> kosong atau masih di depan T. Batas atasnya tegas supaya detik
        /// pengalihan dimiliki tepat satu baris, bukan dua.
        /// </remarks>
        public async Task<EmergencyDoctorAssignmentResponse?> AmbilAktifAsync(
            Guid emergencyVisitId,
            DateTime? at,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<EmgDoctorAssignment>()
                .AsNoTracking()
                .Where(x => x.EmergencyVisitId == emergencyVisitId && !x.IsDelete);

            query = at.HasValue
                ? query.Where(x => x.EffectiveFrom <= at.Value
                    && (x.EffectiveTo == null || at.Value < x.EffectiveTo))
                : query.Where(x => x.EffectiveTo == null);

            return await query
                .OrderByDescending(x => x.EffectiveFrom)
                .Select(ProyeksiResponse)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Menetapkan dokter penanggung jawab pertama pada satu kunjungan.
        /// </summary>
        public async Task<Hasil<EmergencyDoctorAssignmentResponse>> TetapkanAsync(
            AssignEmergencyDoctorRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var visit = await _dbContext.Set<EmgVisit>()
                .FirstOrDefaultAsync(x => x.Id == request.EmergencyVisitId && !x.IsDelete, cancellationToken);
            if (visit == null)
                return Hasil<EmergencyDoctorAssignmentResponse>.Gagal(
                    StatusCodes.Status404NotFound, "Kunjungan IGD tidak ditemukan.");

            var penolakanDokter = await PeriksaDokterAsync(request.DoctorId, cancellationToken);
            if (penolakanDokter != null)
                return Hasil<EmergencyDoctorAssignmentResponse>.Gagal(
                    StatusCodes.Status400BadRequest, penolakanDokter);

            var effectiveFrom = request.EffectiveFrom ?? DateTime.UtcNow;
            if (effectiveFrom < visit.ArrivalDateTime)
                return Hasil<EmergencyDoctorAssignmentResponse>.Gagal(
                    StatusCodes.Status400BadRequest,
                    "Waktu penugasan tidak boleh lebih awal dari waktu kedatangan pasien.");

            // Validation bagian 3 aturan 2. Pemeriksaan ini ramah bagi pengguna; penjaga
            // sesungguhnya tetap index unik bersyarat, yang menangkap dua permintaan bersamaan
            // yang sama-sama lolos di sini.
            var sudahAdaBerjalan = await _dbContext.Set<EmgDoctorAssignment>()
                .AsNoTracking()
                .AnyAsync(x => x.EmergencyVisitId == visit.Id && x.EffectiveTo == null && !x.IsDelete,
                    cancellationToken);
            if (sudahAdaBerjalan)
                return Hasil<EmergencyDoctorAssignmentResponse>.Gagal(
                    StatusCodes.Status409Conflict,
                    "Kunjungan ini sudah memiliki dokter penanggung jawab. Gunakan aksi pengalihan dokter.");

            var now = DateTime.UtcNow;
            var entity = new EmgDoctorAssignment
            {
                Id = Guid.NewGuid(),
                EmergencyVisitId = visit.Id,
                DoctorId = request.DoctorId,
                EffectiveFrom = effectiveFrom,
                EffectiveTo = null,
                AssignedByUserId = actorUserId,
                AssignmentReason = null,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<EmgDoctorAssignment>().Add(entity);
            await SelaraskanDokterEncounterAsync(visit, request.DoctorId, actorUserId, now, cancellationToken);

            var penolakanSimpan = await SimpanAsync(cancellationToken);
            if (penolakanSimpan != null)
                return Hasil<EmergencyDoctorAssignmentResponse>.Gagal(
                    StatusCodes.Status409Conflict, penolakanSimpan);

            var dibuat = await AmbilSatuAsync(entity.Id, cancellationToken);
            return Hasil<EmergencyDoctorAssignmentResponse>.Ok(dibuat!, StatusCodes.Status201Created);
        }

        /// <summary>
        /// Mengalihkan penugasan berjalan ke dokter lain.
        /// </summary>
        /// <remarks>
        /// Menutup baris lama dan membuka baris baru dalam SATU <c>SaveChangesAsync</c>, jadi
        /// satu transaksi. Mustahil baris lama tertutup tanpa penggantinya ikut tersimpan.
        /// </remarks>
        public async Task<Hasil<EmergencyDoctorAssignmentResponse>> AlihkanAsync(
            Guid assignmentId,
            HandoverEmergencyDoctorRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            // Satu-satunya gerbang kewajiban alasan. DTO-nya bertipe string? justru supaya
            // ModelState tidak menolak lebih dulu dengan ProblemDetails generik, sehingga
            // keempat bentuk masukan kosong - tidak dikirim, null, string kosong, dan spasi
            // saja - berakhir di sini dengan pesan kontrak yang sama.
            var alasanPengalihan = request.AssignmentReason?.Trim();
            if (string.IsNullOrEmpty(alasanPengalihan))
                return Hasil<EmergencyDoctorAssignmentResponse>.Gagal(
                    StatusCodes.Status400BadRequest, "Alasan pengalihan dokter wajib diisi.");

            var lama = await _dbContext.Set<EmgDoctorAssignment>()
                .FirstOrDefaultAsync(x => x.Id == assignmentId && !x.IsDelete, cancellationToken);
            if (lama == null)
                return Hasil<EmergencyDoctorAssignmentResponse>.Gagal(
                    StatusCodes.Status404NotFound, "Penugasan dokter tidak ditemukan.");

            if (lama.EffectiveTo != null)
                return Hasil<EmergencyDoctorAssignmentResponse>.Gagal(
                    StatusCodes.Status409Conflict,
                    "Penugasan ini sudah berakhir, sehingga tidak dapat dialihkan. Alihkan dari penugasan yang sedang berjalan.");

            var visit = await _dbContext.Set<EmgVisit>()
                .FirstOrDefaultAsync(x => x.Id == lama.EmergencyVisitId && !x.IsDelete, cancellationToken);
            if (visit == null)
                return Hasil<EmergencyDoctorAssignmentResponse>.Gagal(
                    StatusCodes.Status404NotFound, "Kunjungan IGD milik penugasan ini tidak ditemukan.");

            var penolakanDokter = await PeriksaDokterAsync(request.DoctorId, cancellationToken);
            if (penolakanDokter != null)
                return Hasil<EmergencyDoctorAssignmentResponse>.Gagal(
                    StatusCodes.Status400BadRequest, penolakanDokter);

            var effectiveFrom = request.EffectiveFrom ?? DateTime.UtcNow;
            if (effectiveFrom < visit.ArrivalDateTime)
                return Hasil<EmergencyDoctorAssignmentResponse>.Gagal(
                    StatusCodes.Status400BadRequest,
                    "Waktu penugasan tidak boleh lebih awal dari waktu kedatangan pasien.");

            // Riwayat berperiode tidak boleh mundur: baris baru selalu mulai tidak lebih awal
            // daripada baris lama, kalau tidak urutan waktunya mustahil dibaca.
            if (effectiveFrom < lama.EffectiveFrom)
                return Hasil<EmergencyDoctorAssignmentResponse>.Gagal(
                    StatusCodes.Status400BadRequest,
                    "Waktu pengalihan tidak boleh lebih awal dari waktu mulai penugasan yang sedang berjalan.");

            var now = DateTime.UtcNow;

            // Baris lama DITUTUP, bukan ditimpa - IGD-DEC-082. Dokter, waktu mulai, dan
            // pelakunya tetap apa adanya supaya sejarah tidak berubah.
            lama.EffectiveTo = effectiveFrom;
            lama.UpdateDateTime = now;
            lama.UpdateBy = actorUserId;

            var baru = new EmgDoctorAssignment
            {
                Id = Guid.NewGuid(),
                EmergencyVisitId = lama.EmergencyVisitId,
                DoctorId = request.DoctorId,
                EffectiveFrom = effectiveFrom,
                EffectiveTo = null,
                AssignedByUserId = actorUserId,
                AssignmentReason = alasanPengalihan,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<EmgDoctorAssignment>().Add(baru);
            await SelaraskanDokterEncounterAsync(visit, request.DoctorId, actorUserId, now, cancellationToken);

            var penolakanSimpan = await SimpanAsync(cancellationToken);
            if (penolakanSimpan != null)
                return Hasil<EmergencyDoctorAssignmentResponse>.Gagal(
                    StatusCodes.Status409Conflict, penolakanSimpan);

            var dibuat = await AmbilSatuAsync(baru.Id, cancellationToken);
            return Hasil<EmergencyDoctorAssignmentResponse>.Ok(dibuat!);
        }

        private async Task<EmergencyDoctorAssignmentResponse?> AmbilSatuAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<EmgDoctorAssignment>()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ProyeksiResponse)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Validation bagian 3 aturan 1. Dipakai penetapan maupun pengalihan supaya teksnya
        /// hanya ada di satu tempat.
        /// </summary>
        private async Task<string?> PeriksaDokterAsync(Guid doctorId, CancellationToken cancellationToken)
        {
            if (doctorId == Guid.Empty)
                return "Dokter tidak ditemukan atau tidak aktif.";

            var ada = await _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == doctorId && !x.IsDelete && x.IsActive, cancellationToken);

            return ada ? null : "Dokter tidak ditemukan atau tidak aktif.";
        }

        /// <summary>
        /// Menyelaraskan nilai efektif <c>RegPatientEncounter.DoctorId</c> dengan dokter yang
        /// baru ditetapkan.
        /// </summary>
        /// <remarks>
        /// <c>FR-IGD-020</c> dan API bagian 3: penulisan penugasan juga memperbarui nilai
        /// efektif pada encounter DALAM TRANSAKSI YANG SAMA, sehingga layar dan laporan lama
        /// tidak pernah membaca dokter yang berbeda dari riwayat. Perubahannya hanya dilacak di
        /// sini; penyimpanannya ikut <c>SaveChangesAsync</c> pemanggil.
        ///
        /// <para>
        /// Kunjungan yang belum tertaut encounter dilewati tanpa galat: penugasan dokternya
        /// tetap sah dan tersimpan pada riwayat IGD.
        /// </para>
        /// </remarks>
        private async Task SelaraskanDokterEncounterAsync(
            EmgVisit visit,
            Guid doctorId,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            if (!visit.EncounterId.HasValue || visit.EncounterId.Value == Guid.Empty)
                return;

            var encounter = await _dbContext.Set<RegPatientEncounter>()
                .FirstOrDefaultAsync(x => x.Id == visit.EncounterId.Value && !x.IsDelete, cancellationToken);
            if (encounter == null)
                return;

            encounter.DoctorId = doctorId;
            encounter.UpdateDateTime = now;
            encounter.UpdateBy = actorUserId;
        }

        /// <summary>
        /// Menyimpan, lalu menerjemahkan tabrakan index unik bersyarat menjadi penolakan yang
        /// dapat dibaca petugas.
        /// </summary>
        /// <remarks>
        /// <c>FR-IGD-019</c>: dua permintaan bersamaan yang sama-sama lolos pemeriksaan
        /// sebelumnya tetap ditolak basis data. Satu berhasil, satu menerima pesan ini, dan
        /// tidak pernah ada dua dokter berjalan. Kegagalan simpan lain diteruskan apa adanya
        /// supaya tidak tersamar menjadi konflik.
        /// </remarks>
        private async Task<string?> SimpanAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return null;
            }
            catch (DbUpdateException ex) when (
                ex.InnerException is PostgresException postgres
                && postgres.SqlState == PostgresErrorCodes.UniqueViolation
                && string.Equals(postgres.ConstraintName, IndexPenugasanBerjalan, StringComparison.Ordinal))
            {
                return "Kunjungan ini sudah memiliki dokter penanggung jawab. Gunakan aksi pengalihan dokter.";
            }
        }
    }
}
