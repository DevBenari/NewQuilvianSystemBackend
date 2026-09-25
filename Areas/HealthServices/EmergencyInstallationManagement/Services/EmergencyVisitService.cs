using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

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
        private readonly ClinicalDocumentIntegrityService _integrityService;

        public EmergencyVisitService(
            ApplicationDbContext dbContext,
            EmergencyDocumentNumberService documentNumberService,
            ClinicalDocumentIntegrityService integrityService)
        {
            _dbContext = dbContext;
            _documentNumberService = documentNumberService;
            _integrityService = integrityService;
        }

        /// <summary>
        /// Alasan pembatalan encounter bila petugas tidak menulis catatan saat membatalkan
        /// kunjungan IGD (API <c>0.11.0</c> §8.3.7).
        /// </summary>
        public const string AlasanBakuPembatalanEncounter = "Kunjungan IGD dibatalkan";

        // Batas kolom RegPatientEncounter.CancelReason (HasMaxLength(250)). Catatan permintaan
        // pembatalan kunjungan boleh sampai 2000 karakter; tanpa pemotongan, catatan panjang
        // menggagalkan seluruh pembatalan.
        private const int PanjangMaksimalAlasanPembatalanEncounter = 250;

        /// <summary>
        /// Pesan penolakan ketika pengaturan IGD tidak tersedia dan tidak dapat disimpulkan.
        /// Dipakai bersama jalur controller supaya petugas membaca kalimat yang sama, beserta
        /// langkah perbaikannya, dari endpoint mana pun penolakan itu datang.
        /// </summary>
        public const string PesanPengaturanTidakTersedia =
            "Pengaturan IGD aktif belum tersedia dan unit IGD tidak dapat disimpulkan sendiri. " +
            "Pastikan ada tepat satu unit pelayanan aktif bertipe gawat darurat pada master " +
            "Unit Pelayanan, atau isi master Pengaturan IGD lebih dulu.";

        public sealed record Hasil<T>(T? Data, int StatusCode, string? Penolakan)
        {
            public bool Berhasil => Penolakan == null;
            public static Hasil<T> Ok(T data) => new(data, StatusCodes.Status200OK, null);
            public static Hasil<T> Dibuat(T data) => new(data, StatusCodes.Status201Created, null);
            public static Hasil<T> Gagal(int statusCode, string penolakan) => new(default, statusCode, penolakan);
        }

        private sealed record MasukanMulaiKunjungan(
            Guid EncounterId,
            EmergencyVisitStartMode Mode,
            DateTime? WaktuTiba,
            Guid? ArrivalModeId,
            Guid? CaseTypeId,
            string? ChiefComplaint,
            bool IsUnknownPatient,
            string? TemporaryPatientAlias);

        private static readonly TimeZoneInfo ZonaWaktuPesan = TentukanZonaWaktuPesan();

        public Task<EmgSetting?> GetActiveSettingAsync(
            CancellationToken cancellationToken = default)
            => ResolveActiveSettingAsync(_dbContext, cancellationToken);

        /// <summary>
        /// Menentukan pengaturan IGD yang berlaku. Mengembalikan <c>null</c> hanya bila tidak
        /// ada baris tersimpan <b>dan</b> unit IGD tidak dapat disimpulkan.
        /// </summary>
        /// <remarks>
        /// Baris <c>EmgSetting</c> tidak punya layar admin sendiri, sehingga rumah sakit yang
        /// tabelnya masih kosong tidak punya jalan mengisinya lewat aplikasi. Selama itu,
        /// setiap pendaftaran gawat darurat ditolak - bukan karena datanya salah, melainkan
        /// karena satu baris konfigurasi belum ada. Menahan pasien IGD di pintu masuk karena
        /// alasan itu adalah kegagalan yang paling mahal di modul ini.
        ///
        /// <para>
        /// Karena itu, ketika tabelnya kosong dan di master Unit Pelayanan hanya ada
        /// <b>tepat satu</b> unit aktif bertipe gawat darurat, unit itulah yang dipakai.
        /// Tidak ada yang perlu ditebak ketika pilihannya tunggal, dan aturan ini sama persis
        /// dengan yang sudah dipakai frontend saat memilih unit pendaftaran IGD - sehingga
        /// kedua sisi tidak dapat menyimpulkan unit yang berbeda.
        /// </para>
        ///
        /// <para>
        /// Dua batas yang membuatnya tetap aman. Pertama, hasil simpulan ini <b>tidak pernah
        /// disimpan</b>: ia dibentuk ulang tiap permintaan dan langsung kalah begitu ada satu
        /// baris pengaturan sungguhan, sehingga keputusan rumah sakit selalu menang atas
        /// simpulan aplikasi. <c>Id</c>-nya sengaja <c>Guid.Empty</c> sebagai penanda bahwa ia
        /// bukan baris tabel; nol kolom di basis data menunjuk ke id pengaturan, jadi penanda
        /// itu tidak dapat bocor menjadi foreign key. Kedua, bila unit gawat darurat aktif
        /// berjumlah nol atau lebih dari satu, penolakan tetap terjadi - menebak unit di IGD
        /// jauh lebih berbahaya daripada berhenti, karena pasien akan terdaftar di unit yang
        /// salah dan tidak muncul pada antrean triase mana pun.
        /// </para>
        ///
        /// <para>
        /// Sisa nilai pengaturan memakai default yang sudah tertulis pada <c>EmgSetting</c>
        /// sendiri, termasuk penjagaan seperti
        /// <c>RequireRegistrationCompletionBeforeDisposition</c>. Sebelumnya penjagaan itu
        /// ikut mati diam-diam saat tabelnya kosong, karena pemeriksaannya berbentuk
        /// <c>setting?.X == true</c>.
        /// </para>
        /// </remarks>
        public static async Task<EmgSetting?> ResolveActiveSettingAsync(
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken = default)
        {
            var tersimpan = await dbContext.Set<EmgSetting>()
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            if (tersimpan != null)
                return tersimpan;

            // Dua baris cukup untuk membedakan "tepat satu" dari "lebih dari satu"; jumlah
            // persisnya tidak dipakai, sehingga tabel unit tidak perlu dihitung seluruhnya.
            var unitGawatDarurat = await dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .Where(x => !x.IsDelete &&
                            x.IsActive &&
                            x.ServiceUnitType == ServiceUnitType.Emergency)
                .Select(x => x.Id)
                .Take(2)
                .ToListAsync(cancellationToken);

            if (unitGawatDarurat.Count != 1)
                return null;

            return new EmgSetting
            {
                Id = Guid.Empty,
                Code = "IMPLICIT",
                Name = "Pengaturan IGD tersirat",
                DefaultEmergencyServiceUnitId = unitGawatDarurat[0],
                Notes = "Disimpulkan dari satu-satunya unit pelayanan aktif bertipe gawat " +
                        "darurat karena master Pengaturan IGD masih kosong. Tidak tersimpan " +
                        "di basis data dan digantikan begitu pengaturan sungguhan dibuat."
            };
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
                return PesanPengaturanTidakTersedia;

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
                var encounter = await _dbContext.Set<RegPatientEncounter>()
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
        ///
        /// <para>
        /// <b>Pra-cek sebelum encounter dibuat</b> — <c>BE-IGD-050</c>, <c>IGD-DEC-138</c>.
        /// <c>GET emergency-visits/active-episode</c> memanggil method ini juga, sehingga
        /// aturan "episode berjalan" hanya ada di satu tempat dan pra-cek tidak dapat
        /// menyimpang dari penolakan <c>409</c> pada <c>POST /</c>. Parameter
        /// <paramref name="sertakanPasien"/> memuat navigasi <see cref="EmgVisit.Patient"/>
        /// dalam kueri yang sama (satu <c>JOIN</c>, bukan kueri kedua) supaya nama pasien
        /// tersedia tanpa <c>N+1</c>. Bawaannya <c>false</c>, jadi pemanggil lama — <c>POST /</c>
        /// — menjalankan kueri yang persis sama seperti sebelumnya.
        /// </para>
        /// </remarks>
        public async Task<EmgVisit?> CariEpisodeAktifAsync(
            Guid? patientId,
            Guid? kecualiVisitId = null,
            CancellationToken cancellationToken = default,
            bool sertakanPasien = false)
        {
            if (!patientId.HasValue || patientId.Value == Guid.Empty)
                return null;

            IQueryable<EmgVisit> kueri = _dbContext.Set<EmgVisit>().AsNoTracking();

            if (sertakanPasien)
                kueri = kueri.Include(x => x.Patient);

            return await kueri
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

        /// <summary>
        /// Menutup encounter milik kunjungan IGD yang baru saja berakhir. Mengembalikan
        /// <c>true</c> bila encounter ditutup oleh pemanggilan ini.
        /// </summary>
        /// <remarks>
        /// <c>BE-IGD-051</c>, keputusan <c>IGD-DEC-139</c> butir 5 dan <c>IGD-DEC-148</c>
        /// (TK-1 tidak, TK-2 ya); kontrak API <c>0.11.0</c> §8.3.7, validation <c>0.8.0</c>
        /// §10.5, integration <c>0.4.0</c> §5.2.
        ///
        /// <para>
        /// Sebelum task ini, IGD tidak pernah menyentuh <c>EncounterStatus</c>. Setiap kunjungan
        /// yang selesai meninggalkan encounter "masih terbuka" selamanya, dan catatan klinis yang
        /// lupa ditandatangani tetap dapat diubah.
        /// </para>
        ///
        /// <para>
        /// Metode ini <b>tidak</b> memanggil <c>SaveChangesAsync</c> dan tidak membuka transaksi.
        /// Perubahan encounter dan penguncian catatan ikut penyimpanan aksi kunjungan, sehingga
        /// bila salah satunya gagal, kunjungan dan encounter sama-sama tidak berubah.
        /// </para>
        ///
        /// <para>
        /// Kolom encounter yang ditulis hanya yang ada pada daftar tertutup integration §5.2.
        /// <c>CancelDateTime</c>/<c>CancelBy</c> dan pembatalan antrean yang dilakukan jalur
        /// Registrasi <b>tidak</b> ikut ditulis.
        /// </para>
        ///
        /// <para>
        /// Encounter tidak ditulis bila: kunjungan tanpa encounter; encounter tidak ditemukan
        /// atau terhapus; tipenya bukan <c>Emergency</c> maupun <c>Outpatient</c> masa transisi
        /// (lihat <see cref="PeriksaJenisEncounter"/>); atau encounter sudah berakhir menurut
        /// <see cref="EmergencyEpisodeRule.IsEncounterEnded"/> — waktu, pelaku, dan alasan lama
        /// tidak ditimpa. Hapus lunak kunjungan <b>tidak</b> memanggil metode ini (TK-1).
        /// </para>
        /// </remarks>
        /// <param name="visit">Kunjungan yang statusnya baru saja menjadi terminal.</param>
        /// <param name="terminalStatus"><c>Completed</c> atau <c>Cancelled</c>.</param>
        /// <param name="catatanPembatalan">
        /// Catatan permintaan pembatalan kunjungan. Dipakai sebagai <c>CancelReason</c> encounter,
        /// dipotong ke batas kolomnya; bila kosong dipakai
        /// <see cref="AlasanBakuPembatalanEncounter"/>. Diabaikan untuk <c>Completed</c>.
        /// </param>
        public async Task<bool> ApplyEncounterClosureAsync(
            EmgVisit visit,
            EmergencyVisitStatus terminalStatus,
            Guid actorUserId,
            DateTime now,
            string? catatanPembatalan = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(visit);

            if (terminalStatus is not (EmergencyVisitStatus.Completed or EmergencyVisitStatus.Cancelled))
                throw new ArgumentOutOfRangeException(
                    nameof(terminalStatus),
                    terminalStatus,
                    "Encounter hanya ditutup ketika kunjungan IGD selesai atau dibatalkan.");

            if (!visit.EncounterId.HasValue || visit.EncounterId.Value == Guid.Empty)
                return false;

            var encounter = await _dbContext.Set<RegPatientEncounter>()
                .FirstOrDefaultAsync(
                    x => x.Id == visit.EncounterId.Value && !x.IsDelete,
                    cancellationToken);

            if (encounter == null)
                return false;

            if (PeriksaJenisEncounter(encounter.EncounterType) != null)
                return false;

            if (EmergencyEpisodeRule.IsEncounterEnded(encounter))
                return false;

            if (terminalStatus == EmergencyVisitStatus.Completed)
            {
                encounter.EncounterStatus = EncounterStatus.Completed;
                encounter.CompletedAt ??= now;
                encounter.UpdateDateTime = now;
                encounter.UpdateBy = actorUserId;

                // RM-DEC-003 lapis kedua, sama dengan PATCH /patient-encounters/{id}/status ke
                // Completed. Penguncian tidak menyimpan sendiri; ia ikut SaveChanges pemanggil.
                await _integrityService.LockOpenDocumentsForEncounterAsync(
                    encounter.Id,
                    actorUserId,
                    now,
                    encounter.CompletedAt,
                    cancellationToken: cancellationToken);

                return true;
            }

            var alasan = string.IsNullOrWhiteSpace(catatanPembatalan)
                ? AlasanBakuPembatalanEncounter
                : catatanPembatalan.Trim();

            if (alasan.Length > PanjangMaksimalAlasanPembatalanEncounter)
                alasan = alasan[..PanjangMaksimalAlasanPembatalanEncounter];

            encounter.EncounterStatus = EncounterStatus.Cancelled;
            encounter.IsCancel = true;
            encounter.IsActive = false;
            encounter.CancelledAt = now;
            encounter.CancelledByUserId = actorUserId;
            encounter.CancelReason = alasan;
            encounter.UpdateDateTime = now;
            encounter.UpdateBy = actorUserId;

            return true;
        }

        public async Task<Hasil<EmgVisit>> StartVisitAsync(
            StartEmergencyVisitRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!request.EncounterId.HasValue || request.EncounterId.Value == Guid.Empty)
                return Hasil<EmgVisit>.Gagal(StatusCodes.Status400BadRequest, "encounterId wajib diisi.");

            var mode = ParseStartMode(request.Mode);
            if (mode == null)
                return Hasil<EmgVisit>.Gagal(StatusCodes.Status400BadRequest, "Pilih Mulai Triage atau Tangani Segera.");

            var now = DateTime.UtcNow;
            DateTime? waktuTiba = null;

            if (mode == EmergencyVisitStartMode.Triage)
            {
                if (!request.ArrivalDateTime.HasValue || request.ArrivalDateTime.Value == default)
                    return Hasil<EmgVisit>.Gagal(StatusCodes.Status400BadRequest, "Waktu tiba wajib diisi untuk memulai triage.");

                waktuTiba = NormalizeUtc(request.ArrivalDateTime.Value);

                if (waktuTiba.Value > now)
                    return Hasil<EmgVisit>.Gagal(StatusCodes.Status400BadRequest, "Waktu tiba tidak boleh melewati waktu sekarang.");
            }

            var alias = NormalizeText(request.TemporaryPatientAlias);
            if (request.IsUnknownPatient && alias == null)
                return Hasil<EmgVisit>.Gagal(
                    StatusCodes.Status400BadRequest,
                    "Nama sementara wajib diisi untuk pasien yang belum diketahui identitasnya.");

            var arrivalModeId = ToNullableReference(request.ArrivalModeId);
            if (arrivalModeId.HasValue &&
                !await _dbContext.Set<EmgArrivalMode>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == arrivalModeId.Value && !x.IsDelete, cancellationToken))
                return Hasil<EmgVisit>.Gagal(StatusCodes.Status400BadRequest, "ArrivalModeId tidak ditemukan.");

            var caseTypeId = ToNullableReference(request.CaseTypeId);
            if (caseTypeId.HasValue &&
                !await _dbContext.Set<EmgCaseType>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == caseTypeId.Value && !x.IsDelete, cancellationToken))
                return Hasil<EmgVisit>.Gagal(StatusCodes.Status400BadRequest, "CaseTypeId tidak ditemukan.");

            var masukan = new MasukanMulaiKunjungan(
                request.EncounterId.Value,
                mode.Value,
                waktuTiba,
                arrivalModeId,
                caseTypeId,
                NormalizeText(request.ChiefComplaint),
                request.IsUnknownPatient,
                alias);

            try
            {
                return await MulaiKunjunganDalamKunciAsync(masukan, actorUserId, cancellationToken);
            }
            catch (DbUpdateException exception) when (IsUniqueViolation(exception))
            {
                return await MulaiKunjunganDalamKunciAsync(masukan, actorUserId, cancellationToken);
            }
        }

        public static string PesanKunjunganLainBerjalan(EmgVisit kunjungan)
        {
            ArgumentNullException.ThrowIfNull(kunjungan);

            return $"Pasien ini masih memiliki kunjungan IGD aktif bernomor {kunjungan.EmergencyVisitNumber}, " +
                   $"tiba pukul {FormatWaktuLokal(kunjungan.ArrivalDateTime)}. " +
                   "Buka kunjungan tersebut, jangan mendaftar ulang.";
        }

        private async Task<Hasil<EmgVisit>> MulaiKunjunganDalamKunciAsync(
            MasukanMulaiKunjungan masukan,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            await using var transaksi = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var encounter = await CariEncounterAsync(masukan.EncounterId, cancellationToken);
            if (encounter == null)
                return Hasil<EmgVisit>.Gagal(StatusCodes.Status404NotFound, "Encounter tidak ditemukan.");

            if (encounter.EncounterType != EncounterType.Emergency)
                return Hasil<EmgVisit>.Gagal(StatusCodes.Status400BadRequest, "Encounter ini bukan kunjungan gawat darurat.");

            await EmergencyEpisodeRule.LockPatientEpisodeAsync(_dbContext, encounter.PatientId, cancellationToken);

            var now = DateTime.UtcNow;

            encounter = await CariEncounterAsync(masukan.EncounterId, cancellationToken);
            if (encounter == null)
                return Hasil<EmgVisit>.Gagal(StatusCodes.Status404NotFound, "Encounter tidak ditemukan.");

            if (EmergencyEpisodeRule.IsEncounterEnded(encounter))
                return Hasil<EmgVisit>.Gagal(
                    StatusCodes.Status409Conflict,
                    $"Encounter ini sudah berakhir ({StatusAkhirEncounter(encounter)}). Daftarkan ulang pasien bila ia kembali.");

            var kunjungan = await _dbContext.Set<EmgVisit>()
                .FirstOrDefaultAsync(x => x.EncounterId == encounter.Id, cancellationToken);

            if (kunjungan != null)
            {
                if (kunjungan.IsDelete)
                    return Hasil<EmgVisit>.Gagal(
                        StatusCodes.Status409Conflict,
                        "Kunjungan IGD untuk encounter ini pernah dihapus, sehingga encounter ini tidak dapat dimulai lagi. Daftarkan ulang pasien.");

                if (masukan.Mode == EmergencyVisitStartMode.ImmediateCare &&
                    kunjungan.VisitStatus is (EmergencyVisitStatus.Arrived or EmergencyVisitStatus.WaitingForTriage) &&
                    TryApplyVisitStatus(kunjungan, EmergencyVisitStatus.InTreatment, actorUserId, now, out _))
                {
                    kunjungan.TreatmentStartedAt ??= now;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaksi.CommitAsync(cancellationToken);
                }

                return Hasil<EmgVisit>.Ok(await MuatKunjunganAsync(kunjungan.Id, cancellationToken) ?? kunjungan);
            }

            var episodeLain = await EmergencyEpisodeRule.FindOpenEpisodeAsync(
                _dbContext,
                encounter.PatientId,
                encounter.Id,
                cancellationToken);

            if (episodeLain is { Kind: EmergencyEpisodeRule.OpenEpisodeKind.Visit, Visit: { } kunjunganLain })
                return Hasil<EmgVisit>.Gagal(StatusCodes.Status409Conflict, PesanKunjunganLainBerjalan(kunjunganLain));

            var triage = masukan.Mode == EmergencyVisitStartMode.Triage;
            var pelaku = actorUserId == Guid.Empty ? (Guid?)null : actorUserId;

            var kunjunganBaru = new EmgVisit
            {
                Id = Guid.NewGuid(),
                EmergencyVisitNumber = await GenerateVisitNumberAsync(now, cancellationToken),
                EncounterId = encounter.Id,
                PatientId = encounter.PatientId,
                ServiceUnitId = encounter.ServiceUnitId,
                ArrivalModeId = masukan.ArrivalModeId,
                CaseTypeId = masukan.CaseTypeId,
                ArrivalDateTime = triage ? masukan.WaktuTiba!.Value : NormalizeUtc(encounter.RegisteredAt),
                ArrivalTimeSource = triage ? EmergencyArrivalTimeSource.Confirmed : EmergencyArrivalTimeSource.Fallback,
                ArrivalConfirmedByUserId = triage ? pelaku : null,
                ArrivalConfirmedAt = triage ? now : null,
                ChiefComplaint = masukan.ChiefComplaint ?? NormalizeText(encounter.ChiefComplaint),
                IsUnknownPatient = masukan.IsUnknownPatient,
                TemporaryPatientAlias = masukan.TemporaryPatientAlias,
                IsImmediateCareAllowed = true,
                RegistrationStatus = EmergencyRegistrationStatus.Registered,
                RegistrationCompletedAt = NormalizeUtc(encounter.RegisteredAt),
                VisitStatus = triage ? EmergencyVisitStatus.WaitingForTriage : EmergencyVisitStatus.InTreatment,
                TreatmentStartedAt = triage ? null : now,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<EmgVisit>().Add(kunjunganBaru);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                _dbContext.Entry(kunjunganBaru).State = EntityState.Detached;
                throw;
            }

            await transaksi.CommitAsync(cancellationToken);

            return Hasil<EmgVisit>.Dibuat(await MuatKunjunganAsync(kunjunganBaru.Id, cancellationToken) ?? kunjunganBaru);
        }

        private Task<RegPatientEncounter?> CariEncounterAsync(Guid encounterId, CancellationToken cancellationToken)
            => _dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == encounterId && !x.IsDelete, cancellationToken);

        private Task<EmgVisit?> MuatKunjunganAsync(Guid id, CancellationToken cancellationToken)
            => _dbContext.Set<EmgVisit>()
                .AsNoTracking()
                .Include(x => x.Patient)
                .Include(x => x.ServiceUnit)
                .Include(x => x.ArrivalMode)
                .Include(x => x.CaseType)
                .Include(x => x.ArrivalConfirmedByUser)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        private static string StatusAkhirEncounter(RegPatientEncounter encounter)
        {
            if (encounter.EncounterStatus is EncounterStatus.Completed or EncounterStatus.Cancelled or EncounterStatus.NoShow)
                return encounter.EncounterStatus.ToString();

            if (encounter.IsCancel || encounter.CancelledAt != null)
                return nameof(EncounterStatus.Cancelled);

            if (encounter.NoShowAt != null)
                return nameof(EncounterStatus.NoShow);

            return nameof(EncounterStatus.Completed);
        }

        private static EmergencyVisitStartMode? ParseStartMode(string? mode)
        {
            var nilai = mode?.Trim();

            if (string.Equals(nilai, nameof(EmergencyVisitStartMode.Triage), StringComparison.OrdinalIgnoreCase))
                return EmergencyVisitStartMode.Triage;

            if (string.Equals(nilai, nameof(EmergencyVisitStartMode.ImmediateCare), StringComparison.OrdinalIgnoreCase))
                return EmergencyVisitStartMode.ImmediateCare;

            return null;
        }

        private static bool IsUniqueViolation(DbUpdateException exception)
            => exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

        private static string FormatWaktuLokal(DateTime waktu)
            => TimeZoneInfo.ConvertTimeFromUtc(NormalizeUtc(waktu), ZonaWaktuPesan)
                .ToString("HH.mm 'WIB tanggal' dd-MM-yyyy", CultureInfo.InvariantCulture);

        private static TimeZoneInfo TentukanZonaWaktuPesan()
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById("Asia/Jakarta", out var zona))
                return zona;

            if (TimeZoneInfo.TryFindSystemTimeZoneById("SE Asia Standard Time", out zona))
                return zona;

            return TimeZoneInfo.CreateCustomTimeZone("WIB", TimeSpan.FromHours(7), "WIB", "WIB");
        }

        private static DateTime NormalizeUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        private static string? NormalizeText(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static Guid? ToNullableReference(Guid? value)
            => value.HasValue && value.Value != Guid.Empty ? value.Value : null;

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

        private const int JenisBarisEncounter = 1;
        private const int JenisBarisKunjungan = 2;
        private const int BatasBarisAntreanTriage = 100;
        private const string NamaPasienBelumTeridentifikasi = "Pasien belum teridentifikasi";
        private const string AksiMulaiTriage = "StartTriage";
        private const string AksiTanganiSegera = "ImmediateCare";
        private const string AksiPergiSebelumTriage = "NoShow";
        private const string AksiIsiTriage = "FillTriage";

        /// <summary>
        /// Daftar <i>Menunggu Triage</i> terpadu — <c>BE-IGD-054</c>, <c>FR-IGD-070</c>,
        /// API <c>0.11.0</c> §8.3.1, keputusan <c>IGD-DEC-139</c> butir 2 dan <c>IGD-DEC-144</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Dua asal baris digabung di basis data, bukan di layar: encounter IGD yang belum
        /// berakhir dan belum punya kunjungan, serta kunjungan yang episodenya masih terbuka.
        /// "Belum berakhir" memakai <see cref="EmergencyEpisodeRule.EncounterNotEnded"/> —
        /// rumus yang sama dengan penjaga episode dan rekonsiliasi, tidak disalin ulang di sini.
        /// </para>
        /// <para>
        /// Kunjungan yang tampil dibatasi klausa B validation §10.1 aturan 2: <c>VisitStatus</c>
        /// bukan <c>Completed</c> dan bukan <c>Cancelled</c>. Episode yang sudah tuntas bukan
        /// pekerjaan triage, dan encounter-nya pun sudah ditutup <c>BE-IGD-051</c>.
        /// </para>
        /// <para>
        /// Satu episode satu baris dijaga oleh penyaring "encounter tanpa kunjungan": encounter
        /// yang kunjungannya sudah lahir hanya muncul sebagai baris kunjungan. Encounter yang
        /// satu-satunya kunjungannya dihapus lunak (kelas K4 <c>BE-IGD-052</c>) tetap muncul
        /// sebagai baris tanpa kunjungan, dan <c>POST /start-triage</c> menjawabnya <c>409</c>.
        /// </para>
        /// <para>
        /// Jumlah kueri tetap dua — satu <c>COUNT</c> dan satu halaman — berapa pun jumlah
        /// barisnya. Urutan halaman ditutup <see cref="BarisAntreanTriage.KunciUrutan"/> supaya
        /// baris berwaktu sama tidak berpindah halaman di antara dua permintaan.
        /// </para>
        /// </remarks>
        public async Task<Hasil<PagedResult<EmergencyTriageQueueRowResponse>>> GetTriageQueueAsync(
            EmergencyTriageQueueQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

            if (query.Page < 1)
            {
                return Hasil<PagedResult<EmergencyTriageQueueRowResponse>>.Gagal(
                    StatusCodes.Status400BadRequest,
                    "Nomor halaman minimal 1.");
            }

            if (query.PageSize < 1 || query.PageSize > BatasBarisAntreanTriage)
            {
                return Hasil<PagedResult<EmergencyTriageQueueRowResponse>>.Gagal(
                    StatusCodes.Status400BadRequest,
                    $"Jumlah baris per halaman harus di antara 1 sampai {BatasBarisAntreanTriage}.");
            }

            EmergencyVisitStatus? statusKunjungan = null;
            var sertakanBarisEncounter = true;
            var statusDiminta = NormalizeText(query.QueueStatus);

            if (statusDiminta != null)
            {
                if (!Enum.TryParse(statusDiminta, ignoreCase: true, out EmergencyVisitStatus statusTerbaca)
                    || !Enum.IsDefined(statusTerbaca))
                {
                    return Hasil<PagedResult<EmergencyTriageQueueRowResponse>>.Gagal(
                        StatusCodes.Status400BadRequest,
                        $"Status antrean \"{statusDiminta}\" tidak dikenal. Pakai salah satu dari: " +
                        $"{string.Join(", ", Enum.GetNames<EmergencyVisitStatus>())}.");
                }

                statusKunjungan = statusTerbaca;
                sertakanBarisEncounter = statusTerbaca == EmergencyVisitStatus.WaitingForTriage;
            }

            var kataPencarian = NormalizeText(query.Search)?.ToLower() ?? string.Empty;
            var adaPencarian = kataPencarian.Length > 0;

            IQueryable<EmgVisit> kunjungan = _dbContext.Set<EmgVisit>()
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.VisitStatus != EmergencyVisitStatus.Completed
                            && x.VisitStatus != EmergencyVisitStatus.Cancelled);

            if (statusKunjungan.HasValue)
                kunjungan = kunjungan.Where(x => x.VisitStatus == statusKunjungan.Value);

            if (adaPencarian)
            {
                kunjungan = kunjungan.Where(x =>
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(kataPencarian))
                    || (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(kataPencarian))
                    || x.EmergencyVisitNumber.ToLower().Contains(kataPencarian)
                    || (x.Encounter != null && x.Encounter.EncounterNumber.ToLower().Contains(kataPencarian))
                    || (x.TemporaryPatientAlias != null && x.TemporaryPatientAlias.ToLower().Contains(kataPencarian)));
            }

            IQueryable<BarisAntreanTriage> barisKunjungan = kunjungan.Select(x => new BarisAntreanTriage
            {
                Jenis = JenisBarisKunjungan,
                EncounterId = x.EncounterId,
                EmergencyVisitId = x.Id,
                PatientId = x.PatientId,
                NamaPasien = x.Patient != null ? x.Patient.FullName : null,
                NomorRekamMedis = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                IsUnknownPatient = x.IsUnknownPatient,
                TemporaryPatientAlias = x.TemporaryPatientAlias,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : null,
                EmergencyVisitNumber = x.EmergencyVisitNumber,
                VisitStatus = x.VisitStatus,
                RegisteredAt = x.Encounter != null ? x.Encounter.RegisteredAt : x.ArrivalDateTime,
                ArrivalDateTime = x.ArrivalDateTime,
                Urutan = x.ArrivalDateTime,
                KunciUrutan = x.Id
            });

            IQueryable<BarisAntreanTriage> gabungan = barisKunjungan;

            if (sertakanBarisEncounter)
            {
                IQueryable<RegPatientEncounter> encounter = _dbContext.Set<RegPatientEncounter>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.EncounterType == EncounterType.Emergency)
                    .Where(EmergencyEpisodeRule.EncounterNotEnded)
                    .Where(x => !_dbContext.Set<EmgVisit>().Any(v => v.EncounterId == x.Id && !v.IsDelete));

                if (adaPencarian)
                {
                    encounter = encounter.Where(x =>
                        (x.Patient != null && x.Patient.FullName.ToLower().Contains(kataPencarian))
                        || (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(kataPencarian))
                        || x.EncounterNumber.ToLower().Contains(kataPencarian));
                }

                IQueryable<BarisAntreanTriage> barisEncounter = encounter.Select(x => new BarisAntreanTriage
                {
                    Jenis = JenisBarisEncounter,
                    EncounterId = x.Id,
                    EmergencyVisitId = (Guid?)null,
                    PatientId = x.PatientId,
                    NamaPasien = x.Patient != null ? x.Patient.FullName : null,
                    NomorRekamMedis = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    IsUnknownPatient = false,
                    TemporaryPatientAlias = (string?)null,
                    EncounterNumber = x.EncounterNumber,
                    EmergencyVisitNumber = (string?)null,
                    VisitStatus = (EmergencyVisitStatus?)null,
                    RegisteredAt = x.RegisteredAt,
                    ArrivalDateTime = (DateTime?)null,
                    Urutan = x.RegisteredAt,
                    KunciUrutan = x.Id
                });

                gabungan = barisEncounter.Concat(barisKunjungan);
            }

            var totalData = await gabungan.CountAsync(cancellationToken);

            var baris = await gabungan
                .OrderByDescending(x => x.Urutan)
                .ThenByDescending(x => x.KunciUrutan)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            var halaman = new PagedResult<EmergencyTriageQueueRowResponse>
            {
                PageNumber = query.Page,
                PageSize = query.PageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)query.PageSize),
                Items = baris.Select(ToTriageQueueRow).ToList()
            };

            return Hasil<PagedResult<EmergencyTriageQueueRowResponse>>.Ok(halaman);
        }

        private static EmergencyTriageQueueRowResponse ToTriageQueueRow(BarisAntreanTriage baris)
        {
            var dariKunjungan = baris.Jenis == JenisBarisKunjungan;

            return new EmergencyTriageQueueRowResponse
            {
                RowKey = dariKunjungan
                    ? $"visit:{baris.EmergencyVisitId}"
                    : $"enc:{baris.EncounterId}",
                EncounterId = baris.EncounterId,
                EmergencyVisitId = baris.EmergencyVisitId,
                PatientId = baris.PatientId,
                PatientName = ResolveNamaPasienAntrean(baris),
                MedicalRecordNumber = baris.NomorRekamMedis,
                IsUnknownPatient = baris.IsUnknownPatient,
                TemporaryPatientAlias = baris.TemporaryPatientAlias,
                EncounterNumber = baris.EncounterNumber,
                EmergencyVisitNumber = baris.EmergencyVisitNumber,
                QueueStatus = dariKunjungan && baris.VisitStatus.HasValue
                    ? baris.VisitStatus.Value.ToString()
                    : nameof(EmergencyVisitStatus.WaitingForTriage),
                VisitStatus = baris.VisitStatus,
                RegisteredAt = baris.RegisteredAt,
                ArrivalDateTime = baris.ArrivalDateTime,
                AvailableActions = AksiBarisAntrean(baris)
            };
        }

        /// <summary>
        /// Aksi yang boleh muncul pada satu baris — kartu <c>BE-IGD-054</c> aturan 4,
        /// <c>IGD-DEC-142</c> dan <c>IGD-DEC-128</c>.
        /// </summary>
        /// <remarks>
        /// Daftar ini hanya menyatakan aksi yang masuk akal bagi keadaan barisnya. Hak akses
        /// tetap diperiksa backend pada endpoint masing-masing, dan layar menyembunyikan
        /// tombol yang hak aksesnya tidak dimiliki pemakai.
        /// </remarks>
        private static List<string> AksiBarisAntrean(BarisAntreanTriage baris)
        {
            if (baris.Jenis == JenisBarisEncounter)
                return new List<string> { AksiMulaiTriage, AksiTanganiSegera, AksiPergiSebelumTriage };

            return baris.VisitStatus switch
            {
                EmergencyVisitStatus.Arrived or EmergencyVisitStatus.WaitingForTriage
                    => new List<string> { AksiIsiTriage, AksiTanganiSegera },
                _ => new List<string>()
            };
        }

        /// <summary>
        /// Urutan nama yang sama dengan daftar kunjungan: nama pasien, lalu alias sementara,
        /// lalu keterangan bawaan. Tidak pernah kosong.
        /// </summary>
        private static string ResolveNamaPasienAntrean(BarisAntreanTriage baris)
        {
            if (!string.IsNullOrWhiteSpace(baris.NamaPasien))
                return baris.NamaPasien;

            if (!string.IsNullOrWhiteSpace(baris.TemporaryPatientAlias))
                return baris.TemporaryPatientAlias;

            return NamaPasienBelumTeridentifikasi;
        }

        /// <summary>
        /// Bentuk baris bersama kedua asal data, supaya keduanya dapat digabung dan dihalamani
        /// di basis data. Bukan entity dan tidak pernah keluar dari service ini.
        /// </summary>
        private sealed class BarisAntreanTriage
        {
            public int Jenis { get; set; }
            public Guid? EncounterId { get; set; }
            public Guid? EmergencyVisitId { get; set; }
            public Guid? PatientId { get; set; }
            public string? NamaPasien { get; set; }
            public string? NomorRekamMedis { get; set; }
            public bool IsUnknownPatient { get; set; }
            public string? TemporaryPatientAlias { get; set; }
            public string? EncounterNumber { get; set; }
            public string? EmergencyVisitNumber { get; set; }
            public EmergencyVisitStatus? VisitStatus { get; set; }
            public DateTime RegisteredAt { get; set; }
            public DateTime? ArrivalDateTime { get; set; }

            /// <summary>Waktu yang dipakai mengurutkan: waktu tiba bagi kunjungan, waktu terdaftar bagi encounter.</summary>
            public DateTime Urutan { get; set; }

            /// <summary>Penutup urutan supaya dua baris berwaktu sama selalu berurutan tetap.</summary>
            public Guid KunciUrutan { get; set; }
        }
    }
}
