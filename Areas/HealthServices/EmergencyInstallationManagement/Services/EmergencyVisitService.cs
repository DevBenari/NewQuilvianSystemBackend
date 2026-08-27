using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    /// <summary>
    /// Aturan bisnis kunjungan IGD: OP sebagai jenis kunjungan, IGD sebagai asal/unit,
    /// pasien sementara, registrasi provisional, dan transisi status kunjungan.
    /// </summary>
    public class EmergencyVisitService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly EmergencyDocumentNumberService _documentNumberService;

        public EmergencyVisitService(
            ApplicationDbContext dbContext,
            EmergencyDocumentNumberService documentNumberService)
        {
            _dbContext = dbContext;
            _documentNumberService = documentNumberService;
        }

        public async Task<EmgSetting?> GetActiveSettingAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<EmgSetting>()
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<string?> ValidateRequestAsync(
            CreateEmergencyVisitRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.ServiceUnitId == Guid.Empty)
                return "ServiceUnitId wajib diisi.";

            if (!Enum.IsDefined(typeof(EmergencyRegistrationStatus), request.RegistrationStatus))
                return "Nilai RegistrationStatus tidak valid.";

            if (!Enum.IsDefined(typeof(EmergencyVisitStatus), request.VisitStatus))
                return "Nilai VisitStatus tidak valid.";

            var setting = await GetActiveSettingAsync(cancellationToken);
            if (setting == null)
                return "Setting IGD aktif belum tersedia.";

            if (request.ServiceUnitId != setting.DefaultEmergencyServiceUnitId)
                return "Asal kunjungan harus IGD. ServiceUnitId harus sama dengan DefaultEmergencyServiceUnitId pada setting IGD aktif.";

            if (request.IsUnknownPatient && !setting.AllowUnknownPatient)
                return "Setting IGD tidak mengizinkan pendaftaran pasien tanpa identitas.";

            if (request.RegistrationStatus == EmergencyRegistrationStatus.Provisional &&
                !setting.AllowProvisionalRegistration)
                return "Setting IGD tidak mengizinkan registrasi provisional.";

            if (!request.IsUnknownPatient &&
                (!request.PatientId.HasValue || request.PatientId.Value == Guid.Empty))
                return "PatientId wajib diisi untuk pasien yang sudah dikenal.";

            if (request.IsUnknownPatient && string.IsNullOrWhiteSpace(request.TemporaryPatientAlias))
                return "TemporaryPatientAlias wajib diisi untuk pasien yang belum diketahui identitasnya.";

            if ((request.RegistrationStatus == EmergencyRegistrationStatus.Registered ||
                 request.RegistrationStatus == EmergencyRegistrationStatus.Completed) &&
                (!request.EncounterId.HasValue || request.EncounterId.Value == Guid.Empty))
                return "EncounterId wajib tersedia ketika registrasi IGD sudah terdaftar atau selesai.";

            if (!await _dbContext.Set<MstServiceUnit>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.ServiceUnitId && !x.IsDelete, cancellationToken))
                return "ServiceUnitId tidak ditemukan.";

            if (request.EncounterId.HasValue && request.EncounterId.Value != Guid.Empty)
            {
                var encounter = await _dbContext.Set<TrxPatientEncounter>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == request.EncounterId.Value && !x.IsDelete,
                        cancellationToken);

                if (encounter == null)
                    return "EncounterId tidak ditemukan.";

                var pesanJenisEncounter = PeriksaJenisEncounter(encounter.EncounterType);
                if (pesanJenisEncounter != null)
                    return pesanJenisEncounter;

                if (encounter.ServiceUnitId != request.ServiceUnitId)
                    return "ServiceUnitId kunjungan IGD harus sama dengan ServiceUnitId pada encounter.";

                if (request.PatientId.HasValue &&
                    request.PatientId.Value != Guid.Empty &&
                    encounter.PatientId != request.PatientId.Value)
                    return "PatientId tidak sesuai dengan pasien pada encounter.";

                // Satu encounter hanya boleh memiliki satu kunjungan IGD. Pemeriksaan ini
                // sengaja TIDAK menyaring IsDelete, karena unique index di basis data juga
                // tidak menyaringnya. Menyaring di sini akan meloloskan permintaan yang
                // kemudian ditolak database sebagai 409 tanpa penjelasan.
                var encounterSudahDipakai = await _dbContext.Set<EmgVisit>()
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.EncounterId == request.EncounterId.Value,
                        cancellationToken);

                if (encounterSudahDipakai)
                    return "Encounter ini sudah memiliki kunjungan IGD. Gunakan kunjungan yang sudah ada atau buat encounter baru.";
            }

            if (request.PatientId.HasValue &&
                request.PatientId.Value != Guid.Empty &&
                !await _dbContext.Set<MstPatient>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.PatientId.Value && !x.IsDelete, cancellationToken))
                return "PatientId tidak ditemukan.";

            if (request.ArrivalModeId.HasValue &&
                request.ArrivalModeId.Value != Guid.Empty &&
                !await _dbContext.Set<EmgArrivalMode>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.ArrivalModeId.Value && !x.IsDelete, cancellationToken))
                return "ArrivalModeId tidak ditemukan.";

            if (request.CaseTypeId.HasValue &&
                request.CaseTypeId.Value != Guid.Empty &&
                !await _dbContext.Set<EmgCaseType>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.CaseTypeId.Value && !x.IsDelete, cancellationToken))
                return "CaseTypeId tidak ditemukan.";

            return null;
        }

        public Task<string?> ValidateRequestAsync(
            UpdateEmergencyVisitRequest request,
            CancellationToken cancellationToken = default)
            => ValidateRequestAsync((CreateEmergencyVisitRequest)request, cancellationToken);

        /// <summary>
        /// Jenis encounter yang diterima pendaftaran IGD. Mengembalikan pesan penolakan, atau
        /// <c>null</c> bila jenisnya diterima.
        /// </summary>
        /// <remarks>
        /// <c>BE-IGD-023</c>, requirement <c>FR-IGD-001</c>..<c>FR-IGD-004</c>, keputusan
        /// <c>IGD-DEC-067</c>, <c>IGD-DEC-074</c>, dan <c>IGD-DEC-109</c>.
        ///
        /// <para>
        /// Aturan ini dulu ditulis <b>dua kali</b> — sekali di service, sekali di controller —
        /// dan itulah bentuk cacat yang membuat <c>BE-IGD-008</c> terlihat selesai sementara
        /// jalur keduanya masih bocor. Kini keduanya memanggil method ini, sehingga rumusannya
        /// tidak dapat lagi menyimpang satu sama lain.
        /// </para>
        ///
        /// <para>
        /// <b>Masa transisi.</b> <c>IGD-DEC-109</c> menetapkan <c>Outpatient</c> tetap diterima
        /// sampai migration <c>ChangeEmergencyEncounterTypeToEmergency</c> benar-benar
        /// diterapkan. Seluruh kunjungan IGD lama bertipe <c>Outpatient</c>; menolaknya
        /// sekarang memutus setiap kunjungan yang sudah ada. Setelah migration itu berjalan
        /// dan jumlah barisnya cocok, <c>Outpatient</c> dihapus dari daftar di bawah — satu
        /// baris perubahan, satu tempat.
        /// </para>
        /// </remarks>
        public static string? PeriksaJenisEncounter(EncounterType encounterType)
        {
            if (encounterType is EncounterType.Emergency or EncounterType.Outpatient)
                return null;

            return "Encounter yang dipilih bukan kunjungan IGD. Pilih atau buat encounter " +
                   "dengan jenis kunjungan gawat darurat untuk pasien ini.";
        }

        /// <summary>
        /// Memastikan kunjungan IGD yang pendaftarannya sudah tuntas punya encounter.
        /// Mengembalikan pesan penolakan, atau <c>null</c> bila boleh dilanjutkan.
        /// </summary>
        /// <remarks>
        /// <c>BE-IGD-024</c>, requirement <c>FR-IGD-065</c>..<c>FR-IGD-068</c>.
        ///
        /// <para>
        /// Seluruh tabel <c>ClinicalManagement</c> bertumpu pada <c>EncounterId</c>. Kunjungan
        /// IGD tanpa encounter karena itu tidak dapat menyimpan satu pun catatan klinis, dan
        /// kegagalannya baru terlihat jauh di hilir sebagai galat yang tidak menyebut sebabnya.
        /// Penjagaan ini memindahkan kegagalan itu ke depan, dengan pesan yang menyebut apa
        /// yang harus dilakukan petugas.
        /// </para>
        ///
        /// <para>
        /// Kunjungan lama yang <c>EncounterId</c>-nya kosong <b>tidak</b> diperbaiki diam-diam.
        /// Ia tetap terbaca apa adanya; yang ditolak hanyalah <i>menuntaskan pendaftaran</i>
        /// tanpa encounter sejak sekarang.
        /// </para>
        /// </remarks>
        public static string? PeriksaEncounterPendaftaran(
            EmgVisit visit,
            EmergencyRegistrationStatus target)
        {
            ArgumentNullException.ThrowIfNull(visit);

            if (target is not (EmergencyRegistrationStatus.Registered or EmergencyRegistrationStatus.Completed))
                return null;

            if (visit.EncounterId.HasValue && visit.EncounterId.Value != Guid.Empty)
                return null;

            return "Pendaftaran IGD belum dapat dituntaskan karena kunjungan ini belum " +
                   "tertaut ke encounter pasien. Selesaikan pendaftaran pasien lebih dulu, " +
                   "lalu hubungkan encounter-nya ke kunjungan IGD ini.";
        }

        /// <summary>
        /// Status kunjungan yang berarti episode IGD-nya <b>masih berjalan</b>.
        /// </summary>
        /// <remarks>
        /// <c>Completed</c> dan <c>Cancelled</c> adalah satu-satunya dua status yang menutup
        /// episode. Seluruh sisanya — termasuk <c>Disposed</c> — berarti pasien masih menjadi
        /// tanggung jawab IGD.
        /// </remarks>
        public static bool EpisodeMasihBerjalan(EmergencyVisitStatus status)
            => status is not (EmergencyVisitStatus.Completed or EmergencyVisitStatus.Cancelled);

        /// <summary>
        /// Mencari kunjungan IGD milik pasien yang sama yang episodenya masih berjalan.
        /// Mengembalikan <c>null</c> bila tidak ada, atau bila pasiennya belum teridentifikasi.
        /// </summary>
        /// <remarks>
        /// <c>BE-IGD-025</c>, requirement <c>FR-IGD-005</c>..<c>FR-IGD-012</c>, keputusan
        /// <c>IGD-DEC-084</c>.
        ///
        /// <para>
        /// <b>Pasien tanpa identitas tidak pernah ikut tertahan</b> — <c>AT-IGD-085</c>. Selama
        /// <c>PatientId</c> belum terisi, tidak ada dasar untuk menyatakan dua kunjungan itu
        /// milik orang yang sama, dan menahan pendaftarannya berarti menahan pasien yang
        /// justru paling gawat di depan pintu IGD.
        /// </para>
        /// </remarks>
        public async Task<EmgVisit?> CariEpisodeAktifAsync(
            Guid? patientId,
            Guid? kecualiVisitId = null,
            CancellationToken cancellationToken = default)
        {
            if (!patientId.HasValue || patientId.Value == Guid.Empty)
                return null;

            return await _dbContext.Set<EmgVisit>()
                .AsNoTracking()
                .Where(x => x.PatientId == patientId.Value
                    && !x.IsDelete
                    && x.VisitStatus != EmergencyVisitStatus.Completed
                    && x.VisitStatus != EmergencyVisitStatus.Cancelled)
                .Where(x => kecualiVisitId == null || x.Id != kecualiVisitId.Value)
                .OrderByDescending(x => x.ArrivalDateTime)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Pesan penolakan episode ganda. Wajib menyebut <b>nomor kunjungan yang sudah ada</b>
        /// beserta cara membukanya, sesuai aturan penulisan pesan pada validation matrix.
        /// </summary>
        public static string PesanEpisodeGanda(EmgVisit episodeAktif)
        {
            ArgumentNullException.ThrowIfNull(episodeAktif);

            return $"Pasien ini masih punya kunjungan IGD yang berjalan, nomor " +
                   $"{episodeAktif.EmergencyVisitNumber} (status {episodeAktif.VisitStatus}). " +
                   "Buka kunjungan tersebut dari daftar kunjungan IGD dan lanjutkan di sana. " +
                   "Bila pasien memang datang kembali sebagai peristiwa baru, isi alasannya " +
                   "pada kolom alasan pendaftaran ganda.";
        }

        public bool CanTransition(
            EmergencyRegistrationStatus current,
            EmergencyRegistrationStatus target)
        {
            if (current == target)
                return true;

            return current switch
            {
                EmergencyRegistrationStatus.Pending => target is EmergencyRegistrationStatus.Provisional
                    or EmergencyRegistrationStatus.Registered
                    or EmergencyRegistrationStatus.Cancelled,
                EmergencyRegistrationStatus.Provisional => target is EmergencyRegistrationStatus.Registered
                    or EmergencyRegistrationStatus.Completed
                    or EmergencyRegistrationStatus.Cancelled,
                EmergencyRegistrationStatus.Registered => target is EmergencyRegistrationStatus.Completed
                    or EmergencyRegistrationStatus.Cancelled,
                _ => false
            };
        }

        public bool CanTransition(EmergencyVisitStatus current, EmergencyVisitStatus target)
        {
            // Penyelesaian klinis bersifat final. Diperiksa sebelum jalan pintas status-sama
            // di bawah, supaya Completed ke Completed pun ikut tertolak.
            if (current == EmergencyVisitStatus.Completed)
                return false;

            if (current == target)
                return true;

            return current switch
            {
                EmergencyVisitStatus.Arrived => target is EmergencyVisitStatus.WaitingForTriage
                    or EmergencyVisitStatus.InTreatment
                    or EmergencyVisitStatus.Cancelled,
                EmergencyVisitStatus.WaitingForTriage => target is EmergencyVisitStatus.Triaged
                    or EmergencyVisitStatus.InTreatment
                    or EmergencyVisitStatus.Cancelled,
                EmergencyVisitStatus.Triaged => target is EmergencyVisitStatus.InTreatment
                    or EmergencyVisitStatus.UnderObservation
                    or EmergencyVisitStatus.AwaitingDisposition
                    or EmergencyVisitStatus.Cancelled,
                EmergencyVisitStatus.InTreatment => target is EmergencyVisitStatus.UnderObservation
                    or EmergencyVisitStatus.AwaitingDisposition
                    or EmergencyVisitStatus.Cancelled,
                EmergencyVisitStatus.UnderObservation => target is EmergencyVisitStatus.InTreatment
                    or EmergencyVisitStatus.AwaitingDisposition
                    or EmergencyVisitStatus.Cancelled,
                EmergencyVisitStatus.AwaitingDisposition => target is EmergencyVisitStatus.Disposed
                    or EmergencyVisitStatus.InTreatment
                    or EmergencyVisitStatus.Cancelled,
                // Sah menurut state matrix, tetapi closure gate-nya hanya ditegakkan oleh
                // PATCH /{id}/complete. UpdateStatus menolak target ini secara terpisah.
                EmergencyVisitStatus.Disposed => target is EmergencyVisitStatus.Completed,
                _ => false
            };
        }

        /// <summary>
        /// Satu-satunya jalan yang dibenarkan untuk mengubah <see cref="EmgVisit.VisitStatus"/>.
        /// Memeriksa <see cref="CanTransition(EmergencyVisitStatus, EmergencyVisitStatus)"/> lebih dulu,
        /// lalu menulis status beserta jejak auditnya sekaligus.
        /// </summary>
        /// <remarks>
        /// Dibuat oleh <c>BE-IGD-018</c> untuk <c>FR-IGD-015</c>. Latarnya <c>IGD-CONF-05</c>:
        /// status kunjungan pernah ditulis langsung dari tujuh tempat tanpa melewati pemeriksaan
        /// transisi, sehingga kunjungan yang sudah ditutup dapat terbuka kembali.
        ///
        /// Pemanggil yang butuh pesan penolakan khusus — misalnya jalur triase pada
        /// validation-matrix bagian 2 aturan 5 — cukup mengabaikan <paramref name="penolakan"/>
        /// dan menyusun pesannya sendiri dari nilai balik <c>false</c>.
        ///
        /// Metode ini <b>tidak</b> memanggil <c>SaveChangesAsync</c>. Penyimpanan tetap milik
        /// pemanggil, supaya perubahan status ikut dalam transaksi yang sama dengan perubahan
        /// lain di jalur itu.
        /// </remarks>
        public bool TryApplyVisitStatus(
            EmgVisit visit,
            EmergencyVisitStatus target,
            Guid actorUserId,
            DateTime now,
            out string? penolakan)
        {
            ArgumentNullException.ThrowIfNull(visit);

            if (!CanTransition(visit.VisitStatus, target))
            {
                penolakan = $"Status kunjungan tidak dapat berubah dari {visit.VisitStatus} ke {target}.";
                return false;
            }

            penolakan = null;

            // Transisi ke status yang sama diterima CanTransition sebagai tindakan idempoten,
            // tetapi tidak ada yang berubah sehingga jejak audit tidak perlu ikut bergerak.
            if (visit.VisitStatus == target)
                return true;

            visit.VisitStatus = target;
            visit.UpdateDateTime = now;
            visit.UpdateBy = actorUserId;
            return true;
        }

        public async Task<string> GenerateVisitNumberAsync(
            DateTime now,
            CancellationToken cancellationToken = default)
        {
            var setting = await GetActiveSettingAsync(cancellationToken);
            var prefix = setting?.EmergencyVisitNumberPrefix ?? "IGD";

            for (var attempt = 0; attempt < 10; attempt++)
            {
                var number = _documentNumberService.Generate(prefix, now);
                // Tanpa saringan IsDelete. Unique index EmergencyVisitNumber berlaku untuk
                // seluruh baris termasuk yang sudah ditandai terhapus, sehingga menyaringnya
                // di sini membuat nomor yang sebenarnya bentrok dianggap tersedia.
                var alreadyExists = await _dbContext.Set<EmgVisit>()
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.EmergencyVisitNumber == number,
                        cancellationToken);

                if (!alreadyExists)
                    return number;
            }

            throw new InvalidOperationException("Nomor kunjungan IGD unik gagal dibentuk.");
        }
    }
}
