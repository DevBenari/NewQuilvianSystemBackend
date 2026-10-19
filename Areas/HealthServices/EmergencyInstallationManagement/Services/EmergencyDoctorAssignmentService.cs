using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
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
        private readonly PatientProcedureOrderService _procedureOrderService;
        private readonly PatientProcedureExecutionService _procedureExecutionService;
        private readonly IConfiguration _configuration;
        private readonly LoggerService _loggerService;

        public const string KunciKodeJasaKunjungan = "EmergencyBilling:VisitFeeProcedureCode";

        private const string LogCategory = "EmergencyInstallationManagement.EmergencyDoctorAssignmentService";

        public EmergencyDoctorAssignmentService(
            ApplicationDbContext dbContext,
            PatientProcedureOrderService procedureOrderService,
            PatientProcedureExecutionService procedureExecutionService,
            IConfiguration configuration,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _procedureOrderService = procedureOrderService;
            _procedureExecutionService = procedureExecutionService;
            _configuration = configuration;
            _loggerService = loggerService;
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
        /// Teks nama pelaku yang ditampilkan untuk baris riwayat hasil pengisian data lama,
        /// yaitu baris yang pelaku historisnya tidak dapat dibuktikan.
        /// </summary>
        /// <remarks>
        /// <c>BE-IGD-048</c>, <c>IGD-DEC-136</c>. Baris seperti ini mustahil punya
        /// <c>AssignedByUserId</c> terisi — jadi memeriksa nilai ini sudah cukup, tanpa perlu
        /// membedakan lebih lanjut sumber datanya.
        /// </remarks>
        private const string NamaPelakuDataHistoris = "Data historis";

        /// <summary>
        /// Bentuk balasan tunggal untuk daftar, pencarian, penetapan, dan pengalihan.
        /// </summary>
        /// <remarks>
        /// API 0.8.0 bagian 3.2, <c>IGD-DEC-129</c>. Ditulis sebagai satu expression supaya
        /// nama dokter dan nama penugas ikut terbawa SATU kueri lewat relasi <c>Doctor</c> dan
        /// <c>AssignedByUser</c> yang sudah dikonfigurasi, jadi nol <c>N+1</c>. Nama yang tidak
        /// tersedia menjadi <c>null</c>, bukan GUID. Urutan cadangan nama penugas mengikuti
        /// pola <c>recordedByName</c> pada <c>BE-IGD-046</c>.
        ///
        /// <para>
        /// <b><c>assignedByName</c> pada baris legacy — <c>BE-IGD-048</c>.</b>
        /// <c>AssignedByUserId</c> kini <c>Guid?</c>; baris hasil pengisian data lama yang
        /// pelaku historisnya tidak dapat dibuktikan menyimpannya <c>NULL</c>
        /// (<c>IGD-DEC-136</c>). Proyeksi ini memeriksa <c>x.AssignedByUserId == null</c> lebih
        /// dulu dan mengembalikan teks tetap <see cref="NamaPelakuDataHistoris"/> — dihasilkan
        /// BACKEND di sini, bukan diserahkan ke fallback frontend. Pemeriksaan ini tetap bagian
        /// dari expression tree yang sama, sehingga tetap diterjemahkan EF menjadi SATU kueri;
        /// nol pemanggilan tambahan ke basis data.
        /// </para>
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
                AssignedByName = x.AssignedByUserId == null
                    ? NamaPelakuDataHistoris
                    : (x.AssignedByUser == null
                        ? null
                        : x.AssignedByUser.DisplayName ?? x.AssignedByUser.UserName ?? x.AssignedByUser.Email ?? x.AssignedByUser.UserCode),
                AssignmentReason = x.AssignmentReason,
                IsRosterOverride = x.IsRosterOverride,
                RosterOverrideReason = x.RosterOverrideReason
            };

        private static readonly Expression<Func<MstDoctor, bool>> DokterUmumAtauEmergency =
            x => x.SpecialistName == null ||
                x.SpecialistName == "" ||
                x.SpecialistName == "Umum" ||
                EF.Functions.ILike(x.SpecialistName, "%Umum%") ||
                EF.Functions.ILike(x.SpecialistName, "%Emergency%") ||
                EF.Functions.ILike(x.SpecialistName, "%Gawat Darurat%");

        private sealed record HasilKelayakan(string? Penolakan, bool IsRosterOverride, string? Alasan);

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
            var query = at.HasValue
                ? _dbContext.Set<EmgDoctorAssignment>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete
                        && x.EffectiveFrom <= at.Value
                        && (x.EffectiveTo == null || at.Value < x.EffectiveTo))
                : KueriBerjalan();

            return await query
                .Where(x => x.EmergencyVisitId == emergencyVisitId)
                .OrderByDescending(x => x.EffectiveFrom)
                .Select(ProyeksiResponse)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public IQueryable<EmgDoctorAssignment> KueriBerjalan()
        {
            return _dbContext.Set<EmgDoctorAssignment>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.EffectiveTo == null);
        }

        public async Task<IReadOnlyDictionary<Guid, EmergencyDoctorAssignmentResponse>> AmbilBerjalanPerKunjunganAsync(
            IReadOnlyCollection<Guid> emergencyVisitIds,
            CancellationToken cancellationToken = default)
        {
            if (emergencyVisitIds.Count == 0)
                return new Dictionary<Guid, EmergencyDoctorAssignmentResponse>();

            var baris = await KueriBerjalan()
                .Where(x => emergencyVisitIds.Contains(x.EmergencyVisitId))
                .OrderByDescending(x => x.EffectiveFrom)
                .Select(ProyeksiResponse)
                .ToListAsync(cancellationToken);

            return baris
                .GroupBy(x => x.EmergencyVisitId)
                .ToDictionary(g => g.Key, g => g.First());
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

            var kelayakan = await PeriksaKelayakanAsync(
                request.DoctorId, effectiveFrom, request.RosterOverrideReason, cancellationToken);
            if (kelayakan.Penolakan != null)
                return Hasil<EmergencyDoctorAssignmentResponse>.Gagal(
                    StatusCodes.Status400BadRequest, kelayakan.Penolakan);

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
                IsRosterOverride = kelayakan.IsRosterOverride,
                RosterOverrideReason = kelayakan.Alasan,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<EmgDoctorAssignment>().Add(entity);
            await SelaraskanDokterEncounterAsync(visit, request.DoctorId, actorUserId, now, cancellationToken);
            var jasaKunjungan = await TambahkanJasaKunjunganAsync(visit, request.DoctorId, effectiveFrom, actorUserId, cancellationToken);

            var penolakanSimpan = await SimpanAsync(cancellationToken);
            if (penolakanSimpan != null)
                return Hasil<EmergencyDoctorAssignmentResponse>.Gagal(
                    StatusCodes.Status409Conflict, penolakanSimpan);

            if (jasaKunjungan != null)
                await TerbitkanFaktaJasaKunjunganAsync(jasaKunjungan, visit.Id, actorUserId);

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

            var kelayakan = await PeriksaKelayakanAsync(
                request.DoctorId, effectiveFrom, request.RosterOverrideReason, cancellationToken);
            if (kelayakan.Penolakan != null)
                return Hasil<EmergencyDoctorAssignmentResponse>.Gagal(
                    StatusCodes.Status400BadRequest, kelayakan.Penolakan);

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
                IsRosterOverride = kelayakan.IsRosterOverride,
                RosterOverrideReason = kelayakan.Alasan,
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

        private async Task<TrxPatientProcedure?> TambahkanJasaKunjunganAsync(
            EmgVisit visit,
            Guid doctorId,
            DateTime performedAt,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var kode = _configuration[KunciKodeJasaKunjungan];
            TrxPatientProcedure? tindakan = null;
            string? alasan;

            if (string.IsNullOrWhiteSpace(kode))
                alasan = $"Konfigurasi {KunciKodeJasaKunjungan} kosong.";
            else
            {
                try
                {
                    (tindakan, alasan) = await _procedureOrderService.TambahkanJasaKunjunganIgdAsync(
                        visit, doctorId, performedAt, kode, actorUserId, cancellationToken);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    await _loggerService.ErrorAsync(
                        LogCategory,
                        "EmergencyVisitFee.PrepareFailed",
                        "Jasa kunjungan IGD gagal disiapkan; penetapan DPJP tetap dilanjutkan.",
                        exception,
                        new { EmergencyVisitId = visit.Id, visit.EncounterId });
                    return null;
                }
            }

            if (tindakan == null)
                await _loggerService.WarningAsync(
                    LogCategory,
                    "EmergencyVisitFee.Skipped",
                    "Jasa kunjungan IGD tidak dicatat saat DPJP pertama ditetapkan.",
                    new { EmergencyVisitId = visit.Id, visit.EncounterId, DoctorId = doctorId, ActorUserId = actorUserId, Alasan = alasan });

            return tindakan;
        }

        private async Task TerbitkanFaktaJasaKunjunganAsync(TrxPatientProcedure tindakan, Guid emergencyVisitId, Guid actorUserId)
        {
            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyVisitFee.Recorded",
                "Jasa kunjungan IGD dicatat saat DPJP pertama ditetapkan.",
                new { EmergencyVisitId = emergencyVisitId, ProcedureId = tindakan.Id, tindakan.EncounterId, tindakan.DoctorId, ActorUserId = actorUserId });

            try
            {
                await _procedureExecutionService.EnsureCompletedFactAsync(tindakan, actorUserId, CancellationToken.None);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                await _loggerService.ErrorAsync(
                    LogCategory,
                    "EmergencyVisitFee.FactFailed",
                    "Fakta tagih jasa kunjungan IGD gagal diterbitkan; tindakan tetap tersimpan.",
                    exception,
                    new { EmergencyVisitId = emergencyVisitId, ProcedureId = tindakan.Id });
            }
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

        /// <summary>
        /// Mengambil daftar dokter jaga IGD yang memiliki jadwal shift dinas IGD aktif pada saat ini
        /// (memisahkan dokter praktek poli rawat jalan dari dokter jaga IGD).
        /// </summary>
        public async Task<IReadOnlyList<EmergencyOnDutyDoctorResponse>> AmbilDokterJagaAktifAsync(
            string? search,
            DateTime? at,
            bool onlyOnDuty = false,
            CancellationToken cancellationToken = default)
        {
            var effectiveNow = at.HasValue
                ? ConvertToOperationalTime(at.Value)
                : GetOperationalNow();

            IQueryable<MstDoctorSchedule> query = KueriJadwalIgdBerlaku(effectiveNow)
                .Include(x => x.Doctor)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.Room);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var trimmed = search.Trim();
                query = query.Where(x =>
                    x.Doctor != null && (
                        EF.Functions.ILike(x.Doctor.FullName, $"%{trimmed}%") ||
                        EF.Functions.ILike(x.Doctor.DoctorCode, $"%{trimmed}%")
                    )
                );
            }

            var schedules = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Doctor!.FullName)
                .ToListAsync(cancellationToken);

            var items = new List<EmergencyOnDutyDoctorResponse>();
            var seenDoctorIds = new HashSet<Guid>();

            foreach (var s in schedules)
            {
                if (s.Doctor == null || !seenDoctorIds.Add(s.DoctorId))
                    continue;

                var startStr = s.StartTime.ToString(@"hh\:mm");
                var endStr = s.EndTime.ToString(@"hh\:mm");
                var timeStr = $"{startStr} - {endStr}";
                var sessionName = !string.IsNullOrWhiteSpace(s.SessionName)
                    ? s.SessionName.Trim()
                    : "Shift IGD";

                var sessionTag = sessionName.Contains("IGD", StringComparison.OrdinalIgnoreCase)
                    ? sessionName
                    : $"{sessionName} IGD";

                var label = $"{s.Doctor.FullName} ({sessionTag} - {timeStr})";

                items.Add(new EmergencyOnDutyDoctorResponse
                {
                    DoctorId = s.DoctorId,
                    DoctorCode = s.Doctor.DoctorCode,
                    DoctorName = s.Doctor.FullName,
                    SpecialistName = s.Doctor.SpecialistName,
                    ScheduleId = s.Id,
                    ScheduleCode = s.ScheduleCode,
                    ScheduleName = s.ScheduleName,
                    SessionName = sessionName,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    FormattedShiftTime = timeStr,
                    IsOvernight = s.IsOvernight,
                    ServiceUnitId = s.ServiceUnitId,
                    ServiceUnitName = s.ServiceUnit?.ServiceUnitName,
                    ClinicId = s.ClinicId,
                    ClinicName = s.Clinic?.ClinicName,
                    RoomId = s.RoomId,
                    RoomName = s.Room?.RoomName,
                    IsOnDuty = true,
                    DisplayLabel = label
                });
            }

            if (!onlyOnDuty)
            {
                var fallbackQuery = await KueriKelompokLuarJadwalAsync(effectiveNow, cancellationToken);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var trimmed = search.Trim();
                    fallbackQuery = fallbackQuery.Where(x =>
                        EF.Functions.ILike(x.FullName, $"%{trimmed}%") ||
                        EF.Functions.ILike(x.DoctorCode, $"%{trimmed}%")
                    );
                }

                var fallbackDoctors = await fallbackQuery
                    .OrderBy(x => x.FullName)
                    .Take(50)
                    .ToListAsync(cancellationToken);

                foreach (var d in fallbackDoctors)
                {
                    var label = !string.IsNullOrWhiteSpace(d.SpecialistName)
                        ? $"{d.FullName} — {d.SpecialistName} (di luar jadwal jaga)"
                        : $"{d.FullName} (di luar jadwal jaga)";

                    items.Add(new EmergencyOnDutyDoctorResponse
                    {
                        DoctorId = d.Id,
                        DoctorCode = d.DoctorCode,
                        DoctorName = d.FullName,
                        SpecialistName = d.SpecialistName,
                        IsOnDuty = false,
                        DisplayLabel = label
                    });
                }
            }

            return items;
        }

        private IQueryable<MstDoctorSchedule> KueriJadwalIgdBerlaku(DateTime waktuOperasional)
        {
            var today = waktuOperasional.Date;
            var currentTime = waktuOperasional.TimeOfDay;
            var currentDay = today.DayOfWeek;
            var yesterday = today.AddDays(-1);
            var previousDay = yesterday.DayOfWeek;

            var query = _dbContext.Set<MstDoctorSchedule>()
                .AsNoTracking()
                .Where(x =>
                    x.IsActive && !x.IsDelete &&
                    x.ScheduleStatus == DoctorScheduleStatus.Active &&
                    x.Doctor != null && x.Doctor.IsActive && !x.Doctor.IsDelete);

            // Filter HANYA ke Instalasi Gawat Darurat (IGD)
            query = query.Where(x =>
                (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitType == ServiceUnitType.Emergency) ||
                (x.Clinic != null && x.Clinic.ServiceUnit != null && x.Clinic.ServiceUnit.ServiceUnitType == ServiceUnitType.Emergency)
            );

            // Kecualikan secara tegas poli rawat jalan
            query = query.Where(x =>
                (x.ServiceUnit == null || x.ServiceUnit.ServiceUnitType != ServiceUnitType.Outpatient) &&
                (x.Clinic == null || x.Clinic.ServiceUnit == null || x.Clinic.ServiceUnit.ServiceUnitType != ServiceUnitType.Outpatient)
            );

            // Filter hari dan jam shift dinas aktif (termasuk shift overnight / lintas tengah malam)
            return query.Where(x =>
                (
                    !x.IsOvernight &&
                    (
                        (x.ScheduleType == DoctorScheduleType.WeeklyRecurring && x.PracticeDay == currentDay) ||
                        (x.ScheduleType != DoctorScheduleType.WeeklyRecurring && x.PracticeDate.HasValue && x.PracticeDate.Value.Date == today)
                    ) &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= today) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today) &&
                    x.StartTime <= currentTime &&
                    currentTime < x.EndTime
                )
                ||
                (
                    x.IsOvernight &&
                    (
                        (
                            (
                                (x.ScheduleType == DoctorScheduleType.WeeklyRecurring && x.PracticeDay == currentDay) ||
                                (x.ScheduleType != DoctorScheduleType.WeeklyRecurring && x.PracticeDate.HasValue && x.PracticeDate.Value.Date == today)
                            ) &&
                            (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= today) &&
                            (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today) &&
                            currentTime >= x.StartTime
                        )
                        ||
                        (
                            (
                                (x.ScheduleType == DoctorScheduleType.WeeklyRecurring && x.PracticeDay == previousDay) ||
                                (x.ScheduleType != DoctorScheduleType.WeeklyRecurring && x.PracticeDate.HasValue && x.PracticeDate.Value.Date == yesterday)
                            ) &&
                            (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= yesterday) &&
                            (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= yesterday) &&
                            currentTime < x.EndTime
                        )
                    )
                )
            );
        }

        private async Task<IQueryable<MstDoctor>> KueriKelompokLuarJadwalAsync(
            DateTime waktuOperasional,
            CancellationToken cancellationToken)
        {
            var dokterJaga = KueriJadwalIgdBerlaku(waktuOperasional).Select(x => x.DoctorId);

            var dokterTidakJaga = _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete && !dokterJaga.Contains(x.Id));

            var dokterUmumTidakJaga = dokterTidakJaga.Where(DokterUmumAtauEmergency);

            return await dokterUmumTidakJaga.AnyAsync(cancellationToken)
                ? dokterUmumTidakJaga
                : dokterTidakJaga;
        }

        private async Task<HasilKelayakan> PeriksaKelayakanAsync(
            Guid doctorId,
            DateTime effectiveFrom,
            string? alasanOverride,
            CancellationToken cancellationToken)
        {
            var waktuOperasional = ConvertToOperationalTime(effectiveFrom);

            var sedangJaga = await KueriJadwalIgdBerlaku(waktuOperasional)
                .AnyAsync(x => x.DoctorId == doctorId, cancellationToken);
            if (sedangJaga)
                return new HasilKelayakan(null, false, null);

            var kelompokLuarJadwal = await KueriKelompokLuarJadwalAsync(waktuOperasional, cancellationToken);
            var dapatDipilih = await kelompokLuarJadwal.AnyAsync(x => x.Id == doctorId, cancellationToken);
            if (!dapatDipilih)
                return new HasilKelayakan(
                    "Dokter ini tidak dapat dipilih sebagai dokter penanggung jawab IGD. Pilih dokter jaga IGD, atau dokter umum/emergency dengan alasan.",
                    false,
                    null);

            var alasan = alasanOverride?.Trim();
            if (string.IsNullOrEmpty(alasan))
                return new HasilKelayakan(
                    "Dokter ini tidak sedang jaga IGD pada waktu penugasan. Isi alasan memilih dokter di luar jadwal jaga.",
                    false,
                    null);

            if (alasan.Length > 500)
                return new HasilKelayakan(
                    "Alasan memilih dokter di luar jadwal jaga paling banyak 500 karakter.",
                    false,
                    null);

            return new HasilKelayakan(null, true, alasan);
        }

        private static DateTime GetOperationalNow()
        {
            var utcNow = DateTime.UtcNow;
            try
            {
                var jakartaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
                return TimeZoneInfo.ConvertTimeFromUtc(utcNow, jakartaTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                try
                {
                    var windowsTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                    return TimeZoneInfo.ConvertTimeFromUtc(utcNow, windowsTimeZone);
                }
                catch
                {
                    return utcNow.AddHours(7);
                }
            }
            catch (InvalidTimeZoneException)
            {
                return utcNow.AddHours(7);
            }
        }

        private static DateTime ConvertToOperationalTime(DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Utc)
                return dateTime;

            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
                return TimeZoneInfo.ConvertTimeFromUtc(dateTime, tz);
            }
            catch
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                    return TimeZoneInfo.ConvertTimeFromUtc(dateTime, tz);
                }
                catch
                {
                    return dateTime.AddHours(7);
                }
            }
        }
    }
}
