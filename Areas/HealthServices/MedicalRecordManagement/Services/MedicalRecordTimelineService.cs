using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services.Sources;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services
{
    /// <summary>
    /// Menggabungkan tiga belas sumber dokumen klinis menjadi satu riwayat berurut waktu
    /// (RM-DEC-002, RM-CAP-004, arsitektur backend bagian 5.8).
    ///
    /// SEJAK `BE-21` (RM-BP-001 revision 4): selain tiga belas jenis bawaan, dokumen modul lain
    /// dibaca lewat <see cref="IMedicalRecordDocumentSource"/> yang diimplementasikan modul
    /// pemiliknya. Jenis yang dikenal tetapi belum punya pembaca dilaporkan sebagai "belum
    /// tersedia", dan setiap sumber dibaca dengan batas waktu sendiri (A4.9, A4.14).
    ///
    /// MASALAH YANG DISELESAIKAN. Sebelum service ini ada, menampilkan satu halaman rekam medis
    /// berarti memanggil sampai tiga belas endpoint terpisah, masing-masing dengan penomoran
    /// halaman sendiri, lalu mengurutkan hasilnya di sisi layar. Akibatnya riwayat lintas
    /// kunjungan praktis tidak dapat dibaca utuh.
    ///
    /// SERVICE INI HANYA MEMBACA. Tidak ada transaksi, tidak ada penyimpanan, dan seluruh query
    /// memakai AsNoTracking. Entity yang dibaca berasal dari modul lain dan TIDAK BOLEH diubah
    /// dari sini.
    ///
    /// TIGA PEMBACAAN YANG DILAYANI, seluruhnya untuk `MedicalRecordController`:
    /// <list type="bullet">
    /// <item><see cref="GetTimelineAsync"/> — riwayat gabungan berurut waktu (`BE-13`);</item>
    /// <item><see cref="GetSummaryAsync"/> — ringkasan berkas (`BE-14`);</item>
    /// <item><see cref="GetDocumentDetailAsync"/> — detail satu dokumen (`BE-14`).</item>
    /// </list>
    /// Ketiganya diletakkan di sini, bukan di controller, karena arsitektur bagian 5.9
    /// mewajibkan controller rekam medis memakai service dan tidak menyentuh
    /// <c>ApplicationDbContext</c> langsung. Pengetahuan tentang tiga belas tabel klinis juga
    /// hanya boleh tinggal di satu tempat.
    ///
    /// KEWENANGAN BUKAN URUSAN SERVICE INI. Service ini tidak memeriksa apakah pengguna berhak
    /// membuka rekam medis pasien tersebut. Pemeriksaan dan pencatatan jejaknya wajib dijalankan
    /// controller LEBIH DULU lewat <see cref="MedicalRecordAccessAuditService"/>.
    ///
    /// TIGA PEMBATAS YANG WAJIB ADA. Menggabungkan tiga belas sumber berarti berpotensi tiga
    /// belas query dalam satu permintaan. Karena itu setiap permintaan selalu tunduk pada:
    /// <list type="number">
    /// <item>penyaringan jenis dokumen — hanya jenis yang diminta yang ditanyakan;</item>
    /// <item>penyaringan rentang tanggal;</item>
    /// <item>batas jumlah baris per sumber, lihat <see cref="BatasBarisPerSumber"/>.</item>
    /// </list>
    /// Tanpa ketiganya, satu pasien lama dengan ribuan dokumen dapat membuat satu permintaan
    /// berjalan tanpa batas. Lihat uji penerimaan AT-RM-31.
    ///
    /// SATU SUMBER GAGAL BUKAN BERARTI SELURUHNYA GAGAL. Setiap sumber dibaca terpisah. Bila
    /// satu di antaranya bermasalah, sumber lain tetap dikembalikan dan yang gagal dicatat pada
    /// <see cref="MedicalRecordTimelineResult.FailedSources"/>. Pilihan ini disengaja: riwayat
    /// yang hilang seluruhnya lebih berbahaya bagi pelayanan daripada riwayat yang kurang satu
    /// jenis — asalkan kekurangannya dinyatakan, bukan disembunyikan.
    /// </summary>
    public class MedicalRecordTimelineService
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>Pembaca sumber modul lain per jenis dokumen (`INT-RM-10`, `BE-21`).</summary>
        private readonly IReadOnlyDictionary<ClinicalDocumentKind, IMedicalRecordDocumentSource> _sumberPerJenis;

        private readonly TimeSpan _batasWaktuSumber;
        private readonly TimeSpan _batasWaktuBerkas;

        public MedicalRecordTimelineService(
            ApplicationDbContext dbContext,
            IEnumerable<IMedicalRecordDocumentSource> sources,
            IConfiguration configuration)
        {
            _dbContext = dbContext;

            // Satu jenis hanya boleh punya satu pembaca. Bila dua pembaca terdaftar untuk jenis
            // yang sama, yang terdaftar lebih dulu dipakai — urutannya mengikuti pendaftaran DI
            // sehingga tetap deterministik. Melempar galat di sini justru menutup seluruh berkas
            // pasien karena satu kesalahan pendaftaran.
            var sumber = new Dictionary<ClinicalDocumentKind, IMedicalRecordDocumentSource>();
            foreach (var pembaca in sources)
            {
                foreach (var jenis in pembaca.Kinds)
                {
                    if (!JenisBawaan.Contains(jenis))
                        sumber.TryAdd(jenis, pembaca);
                }
            }

            _sumberPerJenis = sumber;

            // A4.14: bawaan teknis, bukan kebijakan rumah sakit; dapat diubah per lingkungan.
            _batasWaktuSumber = TimeSpan.FromSeconds(
                Math.Max(1, configuration.GetValue("MedicalRecord:SourceTimeoutSeconds", BatasWaktuSumberBawaanDetik)));
            _batasWaktuBerkas = TimeSpan.FromSeconds(
                Math.Max(1, configuration.GetValue("MedicalRecord:FileTimeoutSeconds", BatasWaktuBerkasBawaanDetik)));
        }

        /// <summary>Batas waktu bawaan satu sumber, dalam detik (A4.14).</summary>
        public const int BatasWaktuSumberBawaanDetik = 3;

        /// <summary>Batas waktu bawaan satu pembukaan berkas, dalam detik (A4.14).</summary>
        public const int BatasWaktuBerkasBawaanDetik = 20;

        /// <summary>Ukuran halaman bawaan bila pemanggil tidak menentukannya.</summary>
        public const int UkuranHalamanBawaan = 25;

        /// <summary>Batas atas ukuran halaman. Permintaan yang melebihi ini dipotong, bukan ditolak.</summary>
        public const int UkuranHalamanMaksimal = 100;

        /// <summary>
        /// Batas jumlah baris yang boleh diambil dari satu sumber dalam satu permintaan.
        ///
        /// Angka ini adalah pengaman terakhir. Bila sebuah sumber menyentuh batas ini, hasilnya
        /// ditandai terpotong lewat <see cref="MedicalRecordTimelineResult.IsTruncated"/> supaya
        /// pembacanya tahu daftar yang tampil belum tentu seluruhnya.
        /// </summary>
        public const int BatasBarisPerSumber = 500;

        /// <summary>Panjang maksimal judul dan keterangan pendek pada satu baris riwayat.</summary>
        private const int PanjangKeteranganMaksimal = 200;

        /// <summary>
        /// Batas jumlah alergi dan diagnosis aktif yang ikut pada ringkasan berkas.
        ///
        /// Ringkasan bukan daftar lengkap. Yang lengkap dibuka lewat riwayat dengan penyaring
        /// jenis dokumen.
        /// </summary>
        private const int BatasRingkasan = 50;

        /// <summary>
        /// Seluruh nilai jenis dokumen yang dikenal sistem, termasuk yang belum punya pembaca.
        ///
        /// Dipakai pilihan saringan dan penghitungan jenis yang ditegakkan. Untuk menentukan
        /// jenis yang benar-benar DIBACA, pakai <see cref="JenisTersedia"/> — bukan daftar ini.
        /// Memakai daftar ini sebagai daftar baca adalah penyebab jenis 14 dan 15 tampil kosong
        /// diam-diam (`RM-CAP-035`).
        /// </summary>
        public static readonly IReadOnlyList<ClinicalDocumentKind> SeluruhJenis =
            Enum.GetValues<ClinicalDocumentKind>();

        /// <summary>
        /// Jenis yang dibaca service ini sendiri dari `ClinicalManagement`: tiga belas jenis rilis
        /// pertama, ditambah delapan model yang lahir sesudahnya (`BE-22`, `RM-CAP-059`–`065`, `067`).
        /// </summary>
        public static readonly IReadOnlySet<ClinicalDocumentKind> JenisBawaan = new HashSet<ClinicalDocumentKind>
        {
            ClinicalDocumentKind.ProgressNote,
            ClinicalDocumentKind.Consultation,
            ClinicalDocumentKind.Assessment,
            ClinicalDocumentKind.Diagnosis,
            ClinicalDocumentKind.Procedure,
            ClinicalDocumentKind.VitalSign,
            ClinicalDocumentKind.Allergy,
            ClinicalDocumentKind.MedicalHistory,
            ClinicalDocumentKind.FamilyHistory,
            ClinicalDocumentKind.ClinicalDocument,
            ClinicalDocumentKind.NoteAttachment,
            ClinicalDocumentKind.MedicalCertificate,
            ClinicalDocumentKind.Consent,
            ClinicalDocumentKind.NursingCarePlan,
            ClinicalDocumentKind.PhysicianVisit,
            ClinicalDocumentKind.NursingIntervention,
            ClinicalDocumentKind.TransfusionMonitoring,
            ClinicalDocumentKind.DoctorCertificate,
            ClinicalDocumentKind.CaseManagementEvaluation,
            ClinicalDocumentKind.NosocomialInfection,
            ClinicalDocumentKind.TransferHandover
        };

        /// <summary>Jenis yang punya pembaca: tiga belas jenis bawaan ditambah jenis milik pembaca sumber.</summary>
        public IReadOnlyList<ClinicalDocumentKind> JenisTersedia =>
            SeluruhJenis.Where(x => JenisBawaan.Contains(x) || _sumberPerJenis.ContainsKey(x)).ToList();

        /// <summary>Jenis yang dikenal tetapi belum punya pembaca — dilaporkan, tidak dilewati.</summary>
        public IReadOnlyList<ClinicalDocumentKind> JenisBelumTersedia =>
            SeluruhJenis.Where(x => !JenisBawaan.Contains(x) && !_sumberPerJenis.ContainsKey(x)).ToList();

        /// <summary>
        /// Mengambil riwayat klinis seorang pasien dari sumber-sumber yang diminta.
        /// </summary>
        /// <exception cref="InvalidOperationException">Bila id pasien tidak diisi.</exception>
        public async Task<MedicalRecordTimelineResult> GetTimelineAsync(
            MedicalRecordTimelineQuery permintaan,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(permintaan);

            if (permintaan.PatientId == Guid.Empty)
                throw new InvalidOperationException("Id pasien tidak valid.");

            var halaman = permintaan.Page < 1 ? 1 : permintaan.Page;
            var ukuranHalaman = permintaan.PageSize <= 0
                ? UkuranHalamanBawaan
                : Math.Min(permintaan.PageSize, UkuranHalamanMaksimal);

            var (jenisDiminta, belumTersedia) = TentukanJenis(permintaan.DocumentKinds);

            // Baris yang perlu dikumpulkan agar halaman yang diminta dapat dibentuk. Kasus
            // terburuk: seluruh isi halaman berasal dari satu sumber saja, jadi setiap sumber
            // harus menyediakan sebanyak itu.
            var diperlukan = halaman * ukuranHalaman;
            var batasPerSumber = Math.Min(diperlukan, BatasBarisPerSumber);

            var hasil = new MedicalRecordTimelineResult
            {
                RequestedKinds = jenisDiminta.ToList(),
                UnavailableKinds = KeteranganBelumTersedia(belumTersedia),

                // Halaman yang terlalu jauh tidak dapat dijamin utuh, karena setiap sumber
                // dibatasi. Keadaan itu dinyatakan, bukan didiamkan.
                IsTruncated = diperlukan > BatasBarisPerSumber
            };

            var gabungan = new List<MedicalRecordTimelineItemResponse>();
            var jumlahSeluruhnya = 0;
            var jamBerkas = Stopwatch.StartNew();

            // Sumber dibaca BERURUTAN, bukan serentak: seluruhnya memakai satu ApplicationDbContext
            // per permintaan, dan EF Core menolak dua query bersamaan pada satu context (A4.9 no. 3).
            foreach (var jenis in jenisDiminta.Where(JenisBawaan.Contains))
            {
                var (berhasil, nilai) = await BacaDenganBatasWaktuAsync(
                    [jenis],
                    batas => AmbilJenisAsync(jenis, permintaan, batasPerSumber, batas),
                    jamBerkas,
                    hasil.FailedSources,
                    cancellationToken);

                if (!berhasil)
                    continue;

                var (jumlah, baris) = nilai;
                jumlahSeluruhnya += jumlah;

                // Menyentuh batas berarti kemungkinan masih ada dokumen lain yang tidak ikut
                // terambil.
                if (baris.Count >= batasPerSumber)
                    hasil.IsTruncated = true;

                gabungan.AddRange(baris);
            }

            // Satu pembaca dipanggil satu kali untuk seluruh jenisnya yang diminta. Bila ia gagal,
            // seluruh jenisnya tercatat gagal — jenis lain tidak terpengaruh.
            foreach (var kelompok in jenisDiminta
                         .Where(_sumberPerJenis.ContainsKey)
                         .GroupBy(x => _sumberPerJenis[x]))
            {
                var pembaca = kelompok.Key;
                var jenisPembaca = kelompok.ToList();

                var (berhasil, halamanSumber) = await BacaDenganBatasWaktuAsync(
                    jenisPembaca,
                    batas => pembaca.ReadAsync(new MedicalRecordSourceQuery
                    {
                        PatientId = permintaan.PatientId,
                        Kinds = jenisPembaca,
                        EncounterId = permintaan.EncounterId,
                        StartDate = permintaan.StartDate,
                        EndDate = permintaan.EndDate,
                        NewestFirst = permintaan.NewestFirst,
                        Limit = batasPerSumber
                    }, batas),
                    jamBerkas,
                    hasil.FailedSources,
                    cancellationToken);

                if (!berhasil)
                    continue;

                foreach (var jenis in jenisPembaca)
                {
                    jumlahSeluruhnya += halamanSumber.Totals.GetValueOrDefault(jenis);

                    var barisJenis = halamanSumber.Items
                        .Where(x => x.Kind == jenis)
                        .Take(batasPerSumber)
                        .ToList();

                    if (barisJenis.Count >= batasPerSumber)
                        hasil.IsTruncated = true;

                    gabungan.AddRange(barisJenis.Select(x => KeBarisLinimasa(x, pembaca.SourceModule)));
                }
            }

            var terurut = permintaan.NewestFirst
                ? gabungan.OrderByDescending(x => x.OccurredAt).ThenBy(x => x.DocumentId)
                : gabungan.OrderBy(x => x.OccurredAt).ThenBy(x => x.DocumentId);

            var isiHalaman = terurut
                .Skip((halaman - 1) * ukuranHalaman)
                .Take(ukuranHalaman)
                .ToList();

            var waktuTandaTangan = await LengkapiStatusKeutuhanAsync(permintaan.PatientId, isiHalaman, cancellationToken);
            await LengkapiPenandaSusulanAsync(isiHalaman, waktuTandaTangan, cancellationToken);

            foreach (var baris in isiHalaman)
            {
                baris.DocumentKindName = NamaJenis(baris.DocumentKind);
                baris.Title = Potong(baris.Title);
                baris.Summary = Potong(baris.Summary);
            }

            hasil.Page = new PagedResult<MedicalRecordTimelineItemResponse>
            {
                PageNumber = halaman,
                PageSize = ukuranHalaman,
                TotalData = jumlahSeluruhnya,
                TotalPage = (int)Math.Ceiling(jumlahSeluruhnya / (double)ukuranHalaman),
                Items = isiHalaman
            };

            return hasil;
        }

        // =====================================================================
        // Penentuan sumber
        // =====================================================================

        /// <summary>
        /// Memisahkan jenis yang diminta menjadi jenis yang dibaca dan jenis yang belum punya
        /// pembaca.
        ///
        /// Daftar kosong berarti seluruh jenis yang dikenal. Nilai kembar dan nilai yang tidak
        /// dikenal dibuang lebih dulu supaya tidak ada query yang terkirim dua kali. Jenis tanpa
        /// pembaca TIDAK dilewati diam-diam: ia dikembalikan supaya dilaporkan sebagai "belum
        /// tersedia" (`RM-DEC-030`, `RM-DEC-058`, `RM-CAP-035`).
        /// </summary>
        private (IReadOnlyList<ClinicalDocumentKind> Dibaca, IReadOnlyList<ClinicalDocumentKind> BelumTersedia)
            TentukanJenis(IReadOnlyCollection<ClinicalDocumentKind>? diminta)
        {
            var terpilih = diminta == null || diminta.Count == 0
                ? SeluruhJenis.ToList()
                : diminta.Where(x => Enum.IsDefined(x)).Distinct().ToList();

            var dibaca = terpilih
                .Where(x => JenisBawaan.Contains(x) || _sumberPerJenis.ContainsKey(x))
                .ToList();

            var belumTersedia = terpilih.Except(dibaca).ToList();

            return (dibaca, belumTersedia);
        }

        /// <summary>Kalimat bagi pembaca berkas untuk setiap jenis yang belum tersedia.</summary>
        private static List<MedicalRecordUnavailableKindResponse> KeteranganBelumTersedia(
            IEnumerable<ClinicalDocumentKind> jenis) => jenis
            .Select(x => new MedicalRecordUnavailableKindResponse
            {
                DocumentKind = x,
                DocumentKindName = NamaJenis(x),
                Message = x == ClinicalDocumentKind.PathologyAnatomyResult
                    ? "Hasil Patologi Anatomi belum tersedia di berkas rekam medis."
                    : $"{NamaJenis(x)} belum tersambung ke berkas rekam medis. Dokumen jenis ini belum dapat ditampilkan di sini."
            })
            .ToList();

        /// <summary>
        /// Membaca satu sumber dengan batas waktu per sumber dan batas waktu seluruh berkas
        /// (A4.14, `RM-DEC-044`).
        ///
        /// DUA JENIS PEMBATALAN DIBEDAKAN. Pembatalan dari pengguna — permintaan HTTP diputus —
        /// diteruskan apa adanya, karena tidak ada lagi yang menunggu jawabannya. Pembatalan
        /// karena batas waktu sumber habis justru dicatat sebagai gangguan sumber, supaya satu
        /// sumber lambat tidak membatalkan seluruh berkas. Sebelum `BE-21` seluruh
        /// <see cref="OperationCanceledException"/> diteruskan, sehingga batas waktu per sumber
        /// tidak mungkin dipasang.
        /// </summary>
        private async Task<(bool Berhasil, T Nilai)> BacaDenganBatasWaktuAsync<T>(
            IReadOnlyList<ClinicalDocumentKind> jenis,
            Func<CancellationToken, Task<T>> baca,
            Stopwatch jamBerkas,
            List<MedicalRecordTimelineSourceFailure> gagal,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sisaBerkas = _batasWaktuBerkas - jamBerkas.Elapsed;
            if (sisaBerkas <= TimeSpan.Zero)
            {
                CatatGagal(gagal, jenis, "Sumber tidak dibaca karena batas waktu pembukaan berkas sudah habis.");
                return (false, default!);
            }

            using var batas = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            batas.CancelAfter(sisaBerkas < _batasWaktuSumber ? sisaBerkas : _batasWaktuSumber);

            try
            {
                return (true, await baca(batas.Token));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException) when (batas.IsCancellationRequested)
            {
                CatatGagal(gagal, jenis, "Sumber tidak menjawab dalam batas waktu.");
                return (false, default!);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                // Npgsql dapat membungkus pembatalan karena batas waktu dalam galatnya sendiri.
                CatatGagal(gagal, jenis, batas.IsCancellationRequested
                    ? "Sumber tidak menjawab dalam batas waktu."
                    : ex.Message);
                return (false, default!);
            }
        }

        private static void CatatGagal(
            List<MedicalRecordTimelineSourceFailure> gagal,
            IEnumerable<ClinicalDocumentKind> jenis,
            string pesan)
        {
            foreach (var x in jenis)
            {
                gagal.Add(new MedicalRecordTimelineSourceFailure
                {
                    DocumentKind = x,
                    DocumentKindName = NamaJenis(x),
                    Message = pesan
                });
            }
        }

        /// <summary>Mengubah dokumen final dari modul lain menjadi satu baris linimasa.</summary>
        private static MedicalRecordTimelineItemResponse KeBarisLinimasa(
            MedicalRecordFinalDocumentDescriptor dokumen,
            string modul) => new()
            {
                DocumentKind = dokumen.Kind,
                DocumentId = dokumen.DocumentId,
                EncounterId = dokumen.EncounterId,
                OccurredAt = dokumen.OccurredAt ?? dokumen.FinalizedAt,
                FinalizedAt = dokumen.FinalizedAt,
                DocumentNumber = dokumen.DocumentNumber,
                Title = dokumen.Title,
                Summary = dokumen.Summary,
                IsCorrected = dokumen.IsCorrected,
                IsProvisional = dokumen.IsProvisional,
                SourceModule = modul,
                PeriodicSummary = dokumen.PeriodicSummary
            };

        /// <summary>
        /// Menghubungkan satu jenis dokumen dengan tabel asalnya.
        ///
        /// Setiap sumber menyebut sendiri kolom tanggalnya, karena tiga belas tabel itu memang
        /// tidak memakai satu nama kolom yang sama. Kolom tanggal inilah yang dipakai menyaring
        /// rentang tanggal sekaligus mengurutkan daftar gabungan.
        /// </summary>
        private Task<(int Jumlah, List<MedicalRecordTimelineItemResponse> Baris)> AmbilJenisAsync(
            ClinicalDocumentKind jenis,
            MedicalRecordTimelineQuery permintaan,
            int batas,
            CancellationToken cancellationToken) => jenis switch
            {
                ClinicalDocumentKind.ProgressNote => AmbilAsync<TrxPatientIntegratedProgressNote>(
                    permintaan, batas,
                    x => x.PatientId,
                    x => x.EncounterId,
                    x => x.NoteDateTime,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.ProgressNote,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        OccurredAt = x.NoteDateTime,
                        DocumentNumber = x.ProgressNoteNumber,
                        Title = x.ProfessionName ?? x.ProfessionType,
                        Summary = x.ProviderDisplayNameSnapshot,
                        IsCancelled = x.IsCancel
                    },
                    cancellationToken),

                ClinicalDocumentKind.Consultation => AmbilAsync<TrxDoctorConsultation>(
                    permintaan, batas,
                    x => x.PatientId,
                    x => (Guid?)x.EncounterId,
                    x => x.ConsultationDateTime,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.Consultation,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        OccurredAt = x.ConsultationDateTime,
                        DocumentNumber = x.ConsultationNumber,
                        Title = "Konsultasi Dokter",
                        Summary = null,
                        IsCancelled = x.IsCancel
                    },
                    cancellationToken),

                ClinicalDocumentKind.Assessment => AmbilAsync<TrxPatientAssessment>(
                    permintaan, batas,
                    x => x.PatientId,
                    x => (Guid?)x.EncounterId,
                    x => x.AssessmentDateTime,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.Assessment,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        OccurredAt = x.AssessmentDateTime,
                        DocumentNumber = x.AssessmentNumber,
                        Title = "Asesmen Pasien",
                        Summary = null,
                        IsCancelled = x.IsCancel
                    },
                    cancellationToken),

                ClinicalDocumentKind.Diagnosis => AmbilAsync<TrxPatientDiagnosis>(
                    permintaan, batas,
                    x => x.PatientId,
                    x => (Guid?)x.EncounterId,
                    x => x.DiagnosisDateTime,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.Diagnosis,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        OccurredAt = x.DiagnosisDateTime,
                        DocumentNumber = x.DiagnosisCode,
                        Title = x.DiagnosisName,
                        Summary = x.IcdVersion,
                        IsCancelled = x.IsCancel
                    },
                    cancellationToken),

                ClinicalDocumentKind.Procedure => AmbilAsync<TrxPatientProcedure>(
                    permintaan, batas,
                    x => x.PatientId,
                    x => (Guid?)x.EncounterId,
                    x => x.ProcedureDateTime,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.Procedure,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        OccurredAt = x.ProcedureDateTime,
                        DocumentNumber = x.ProcedureCodeSnapshot,
                        Title = x.ProcedureNameSnapshot,
                        Summary = x.ProcedureCategoryNameSnapshot,
                        IsCancelled = x.IsCancel
                    },
                    cancellationToken),

                ClinicalDocumentKind.VitalSign => AmbilAsync<TrxPatientVitalSign>(
                    permintaan, batas,
                    x => x.PatientId,
                    x => x.EncounterId,
                    x => x.ObservationDateTime,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.VitalSign,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        OccurredAt = x.ObservationDateTime,
                        DocumentNumber = x.VitalSignRecordNumber,
                        Title = "Tanda Vital",
                        Summary = x.ObservationLocation,
                        IsCancelled = x.IsCancel
                    },
                    cancellationToken),

                ClinicalDocumentKind.Allergy => AmbilAsync<TrxPatientAllergy>(
                    permintaan, batas,
                    x => x.PatientId,
                    x => x.EncounterId,
                    x => x.ReportedDateTime,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.Allergy,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        OccurredAt = x.ReportedDateTime,
                        DocumentNumber = x.AllergyRecordNumber,
                        Title = x.AllergenName,
                        Summary = x.AllergenGroupName,
                        IsCancelled = x.IsCancel
                    },
                    cancellationToken),

                ClinicalDocumentKind.MedicalHistory => AmbilAsync<TrxPatientMedicalHistory>(
                    permintaan, batas,
                    x => x.PatientId,
                    x => x.EncounterId,
                    x => x.RecordedDateTime,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.MedicalHistory,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        OccurredAt = x.RecordedDateTime,
                        DocumentNumber = x.MedicalHistoryRecordNumber,
                        Title = x.ConditionName,
                        Summary = x.ConditionGroupName,
                        IsCancelled = x.IsCancel
                    },
                    cancellationToken),

                ClinicalDocumentKind.FamilyHistory => AmbilAsync<TrxPatientFamilyHistory>(
                    permintaan, batas,
                    x => x.PatientId,
                    x => x.EncounterId,
                    x => x.RecordedDateTime,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.FamilyHistory,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        OccurredAt = x.RecordedDateTime,
                        DocumentNumber = x.FamilyHistoryRecordNumber,
                        Title = x.ConditionName,
                        Summary = x.RelationshipDescription,
                        IsCancelled = x.IsCancel
                    },
                    cancellationToken),

                ClinicalDocumentKind.ClinicalDocument => AmbilAsync<TrxPatientClinicalDocument>(
                    permintaan, batas,
                    x => x.PatientId,
                    x => x.EncounterId,
                    x => x.DocumentDateTime,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.ClinicalDocument,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        OccurredAt = x.DocumentDateTime,
                        DocumentNumber = x.ClinicalDocumentNumber,
                        Title = x.DocumentTitle,
                        Summary = x.DocumentCategoryName,
                        IsCancelled = x.IsCancel
                    },
                    cancellationToken),

                ClinicalDocumentKind.NoteAttachment => AmbilAsync<TrxClinicalNoteAttachment>(
                    permintaan, batas,
                    x => x.PatientId,
                    x => x.EncounterId,
                    x => x.UploadedAt,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.NoteAttachment,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        OccurredAt = x.UploadedAt,
                        DocumentNumber = x.AttachmentNumber,
                        Title = x.AttachmentTitle,
                        Summary = x.AttachmentCategoryName,
                        IsCancelled = x.IsCancel
                    },
                    cancellationToken),

                ClinicalDocumentKind.MedicalCertificate => AmbilAsync<TrxMedicalCertificate>(
                    permintaan, batas,
                    x => x.PatientId,
                    x => x.EncounterId,
                    x => x.CertificateDateTime,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.MedicalCertificate,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        OccurredAt = x.CertificateDateTime,
                        DocumentNumber = x.MedicalCertificateNumber,
                        Title = x.CertificateTitle,
                        Summary = x.CertificateCategoryName,
                        IsCancelled = x.IsCancel
                    },
                    cancellationToken),

                ClinicalDocumentKind.Consent => AmbilAsync<TrxPatientConsent>(
                    permintaan, batas,
                    x => x.PatientId,
                    x => x.EncounterId,
                    x => x.ConsentDateTime,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.Consent,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        OccurredAt = x.ConsentDateTime,
                        DocumentNumber = x.ConsentNumber,
                        Title = x.ConsentTitle,
                        Summary = x.ConsentCategoryName,
                        IsCancelled = x.IsCancel
                    },
                    cancellationToken),


                // ---- BE-22: model ClinicalManagement yang lahir sesudah rilis pertama ----

                ClinicalDocumentKind.NursingCarePlan => AmbilAsync<CliNursingCarePlan>(
                    permintaan, batas,
                    x => x.PatientId,
                    x => x.EncounterId,
                    x => x.OpenedAt,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.NursingCarePlan,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        OccurredAt = x.OpenedAt,
                        FinalizedAt = x.OpenedAt,
                        Title = "Rencana Asuhan Keperawatan",
                        Summary = x.Items.Where(i => !i.IsDelete).OrderBy(i => i.AuthoredAt).Select(i => i.ProblemStatement).FirstOrDefault(),
                        IsCancelled = x.IsCancel,
                        IsCorrected = _dbContext.CliNursingCarePlanItemRevisions.Any(r => !r.IsDelete && x.Items.Any(i => i.Id == r.CarePlanItemId))
                    },
                    cancellationToken),

                ClinicalDocumentKind.PhysicianVisit => AmbilAsync<CliPhysicianVisit>(
                    permintaan, batas,
                    x => x.PatientId,
                    x => x.EncounterId,
                    x => x.VisitDateTime,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.PhysicianVisit,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        OccurredAt = x.VisitDateTime,
                        FinalizedAt = x.VisitDateTime,
                        DocumentNumber = x.PhysicianVisitNumber,
                        Title = x.VisitRole == PhysicianVisitRole.Dpjp ? "Visite DPJP"
                            : x.VisitRole == PhysicianVisitRole.Consultant ? "Visite Konsulen" : "Visite Dokter Jaga",
                        Summary = x.Note,
                        IsCancelled = x.IsCancel,
                        IsCorrected = x.CorrectsVisitId != null
                    },
                    cancellationToken,
                    x => x.VisitStatus == PhysicianVisitStatus.Recorded),

                ClinicalDocumentKind.NursingIntervention => AmbilAsync<CliNursingIntervention>(
                    permintaan, batas,
                    x => x.PatientId,
                    x => x.EncounterId,
                    x => x.PerformedAt,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.NursingIntervention,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        OccurredAt = x.PerformedAt,
                        FinalizedAt = x.FinalizedAt,
                        Title = x.InterventionName,
                        Summary = x.ResultNote,
                        IsCancelled = x.IsCancel
                    },
                    cancellationToken,
                    x => x.RecordStatus == NursingInterventionStatus.Finalized),

                ClinicalDocumentKind.TransfusionMonitoring => AmbilAsync<CliTransfusionMonitoring>(
                    permintaan, batas,
                    x => x.PatientId,
                    x => x.EncounterId,
                    x => x.TransfusionStartedAt,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.TransfusionMonitoring,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        OccurredAt = x.TransfusionStartedAt,
                        FinalizedAt = x.CompletedAt ?? x.StoppedAt,
                        Title = "Monitoring Transfusi",
                        Summary = x.Status == CliTransfusionMonitoringStatus.Stopped ? "Dihentikan: " + x.StopReason : "Selesai",
                        IsCancelled = x.IsCancel
                    },
                    cancellationToken,
                    x => x.Status == CliTransfusionMonitoringStatus.Completed || x.Status == CliTransfusionMonitoringStatus.Stopped),

                ClinicalDocumentKind.DoctorCertificate => AmbilAsync<CliDoctorCertificate>(
                    permintaan, batas,
                    x => x.PatientId,
                    x => x.EncounterId,
                    x => x.IssuedDate,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.DoctorCertificate,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        OccurredAt = x.IssuedDate,
                        FinalizedAt = x.IssuedDate,
                        DocumentNumber = x.CertificateNumber,
                        Title = x.CertificateType == DoctorCertificateType.SickLeave ? "Surat Keterangan Sakit"
                            : x.CertificateType == DoctorCertificateType.Health ? "Surat Keterangan Sehat"
                            : x.CertificateType == DoctorCertificateType.InpatientReferral ? "Surat Pengantar Rawat Inap"
                            : "Surat Admisi Rawat Inap",
                        Summary = x.Purpose,
                        IsCancelled = x.IsCancel
                    },
                    cancellationToken,
                    x => x.CertificateStatus == DoctorCertificateStatus.Issued),

                ClinicalDocumentKind.CaseManagementEvaluation => AmbilAsync<CliCaseManagementEvaluation>(
                    permintaan, batas,
                    x => x.PatientId,
                    x => x.EncounterId,
                    x => x.ClinicalDateTime,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.CaseManagementEvaluation,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        OccurredAt = x.ClinicalDateTime,
                        FinalizedAt = x.CompletedAt,
                        DocumentNumber = x.EvaluationNumber,
                        Title = "Evaluasi Manajemen Kasus",
                        IsCancelled = x.IsCancel
                    },
                    cancellationToken,
                    x => x.EvaluationStatus == CaseManagementEvaluationStatus.Completed),

                ClinicalDocumentKind.NosocomialInfection => AmbilAsync<TrxNosocomialInfection>(
                    permintaan, batas,
                    x => x.PatientId,
                    x => x.EncounterId,
                    x => x.OnsetDateTime,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.NosocomialInfection,
                        DocumentId = x.Id,
                        EncounterId = x.EncounterId,
                        OccurredAt = x.OnsetDateTime,
                        FinalizedAt = x.VerifiedAt,
                        DocumentNumber = x.NosocomialRecordNumber,
                        Title = "Infeksi Nosokomial",
                        Summary = x.Status == NosocomialInfectionStatus.Confirmed ? "Terkonfirmasi"
                            : x.Status == NosocomialInfectionStatus.RuledOut ? "Disingkirkan"
                            : x.Status == NosocomialInfectionStatus.Resolved ? "Sembuh" : "Dicurigai",
                        IsCancelled = x.IsCancel
                    },
                    cancellationToken,
                    x => x.VerifiedAt != null && x.Status != NosocomialInfectionStatus.Cancelled),

                // Serah terima tidak menyimpan PatientId; pasien dan kunjungan dibaca dari episode
                // rawat inapnya (baca saja, tanpa berkas di modul Rawat Inap).
                ClinicalDocumentKind.TransferHandover => AmbilAsync<CliTransferHandover>(
                    permintaan, batas,
                    x => x.Episode!.PatientId,
                    x => x.Episode!.EncounterId,
                    x => x.ReceivedAt!.Value,
                    x => new MedicalRecordTimelineItemResponse
                    {
                        DocumentKind = ClinicalDocumentKind.TransferHandover,
                        DocumentId = x.Id,
                        EncounterId = x.Episode!.EncounterId,
                        OccurredAt = x.ReceivedAt!.Value,
                        FinalizedAt = x.ReceivedAt,
                        Title = "Serah Terima Pindah Ruang",
                        Summary = x.SoapSummary,
                        IsCancelled = x.IsCancel
                    },
                    cancellationToken,
                    x => x.Status == CliTransferHandoverStatus.Accepted && x.ReceivedAt != null),

                _ => Task.FromResult((0, new List<MedicalRecordTimelineItemResponse>()))
            };

        /// <summary>
        /// Membaca satu sumber dokumen klinis.
        ///
        /// Bentuknya dibuat umum supaya aturan penyaringan — pasien, kunjungan, rentang
        /// tanggal, dokumen terhapus, dokumen dibatalkan — ditulis SATU KALI, bukan tiga belas
        /// kali. Pengulangan tiga belas kali persis masalah RM-CAP-010 yang sedang dibereskan
        /// modul ini.
        ///
        /// Selalu AsNoTracking: service ini tidak pernah menulis, dan entity dari modul lain
        /// tidak boleh ikut terlacak lalu tidak sengaja tersimpan.
        ///
        /// DUA QUERY PER SUMBER, BUKAN SATU. Satu untuk menghitung jumlah dokumen yang cocok,
        /// satu untuk mengambil barisnya. Penghitungan diperlukan supaya nomor halaman pada
        /// layar benar; tanpa itu, jumlah total hanya sebesar isi halaman dan tombol "halaman
        /// berikutnya" kehilangan artinya. Query penghitungan tidak mengambil baris apa pun dan
        /// dilayani index <c>(PatientId, tanggal, IsDelete)</c> yang sudah ada di tiga belas
        /// tabel klinis, jadi biayanya kecil. Inilah alasan penyaringan jenis dokumen penting:
        /// satu jenis berarti dua query, bukan dua puluh enam.
        /// </summary>
        private async Task<(int Jumlah, List<MedicalRecordTimelineItemResponse> Baris)> AmbilAsync<TEntity>(
            MedicalRecordTimelineQuery permintaan,
            int batas,
            Expression<Func<TEntity, Guid>> pasien,
            Expression<Func<TEntity, Guid?>> kunjungan,
            Expression<Func<TEntity, DateTime>> tanggal,
            Expression<Func<TEntity, MedicalRecordTimelineItemResponse>> proyeksi,
            CancellationToken cancellationToken,
            Expression<Func<TEntity, bool>>? syaratFinal = null)
            where TEntity : IdentityModel
        {
            var query = _dbContext.Set<TEntity>()
                .AsNoTracking()
                .Where(Bandingkan(pasien, permintaan.PatientId, Expression.Equal))
                .Where(x => !x.IsDelete);

            // BE-22: model yang punya status alur kerja hanya dibaca bila sudah final menurut
            // modulnya (`RM-DEC-041`). Tiga belas jenis rilis pertama tidak memakai syarat ini.
            if (syaratFinal != null)
                query = query.Where(syaratFinal);

            if (!permintaan.IncludeCancelled)
                query = query.Where(x => !x.IsCancel);

            if (permintaan.EncounterId.HasValue)
            {
                query = query.Where(Bandingkan(
                    kunjungan, (Guid?)permintaan.EncounterId.Value, Expression.Equal));
            }

            if (permintaan.StartDate.HasValue)
            {
                query = query.Where(Bandingkan(
                    tanggal, permintaan.StartDate.Value, Expression.GreaterThanOrEqual));
            }

            if (permintaan.EndDate.HasValue)
            {
                query = query.Where(Bandingkan(
                    tanggal, permintaan.EndDate.Value, Expression.LessThanOrEqual));
            }

            var jumlah = await query.CountAsync(cancellationToken);

            // Batas nol berarti pemanggil hanya membutuhkan jumlahnya, misalnya untuk ringkasan
            // berkas. Query pengambilan baris tidak dikirim sama sekali.
            if (batas <= 0)
                return (jumlah, []);

            var terurut = permintaan.NewestFirst
                ? query.OrderByDescending(tanggal)
                : query.OrderBy(tanggal);

            var baris = await terurut
                .Take(batas)
                .Select(SertakanWaktuMasuk(proyeksi))
                .ToListAsync(cancellationToken);

            return (jumlah, baris);
        }

        // =====================================================================
        // Status keutuhan
        // =====================================================================

        /// <summary>
        /// Menempelkan status keutuhan pada baris yang benar-benar ditampilkan.
        ///
        /// Hanya isi halaman yang ditanyakan, bukan seluruh hasil gabungan, supaya biayanya
        /// tetap satu query kecil berapa pun banyaknya dokumen pasien.
        ///
        /// Baris tanpa pasangan pada daftar keutuhan bukan kesalahan: dua belas dari tiga belas
        /// jenis dokumen memang belum tunduk aturan keutuhan pada rilis pertama (RM-DEC-019).
        /// Keadaan itu ditandai <see cref="MedicalRecordTimelineItemResponse.IsIntegrityEnforced"/>
        /// supaya layar dapat menyatakannya terbuka sesuai RM-FE-009.
        ///
        /// Mengembalikan waktu tanda tangan dokumen yang sudah ditandatangani, dipakai penanda
        /// Susulan sebagai waktu final jenis yang tunduk aturan keutuhan (`BE-26`).
        /// </summary>
        private async Task<Dictionary<(ClinicalDocumentKind, Guid), DateTime>> LengkapiStatusKeutuhanAsync(
            Guid patientId,
            List<MedicalRecordTimelineItemResponse> isiHalaman,
            CancellationToken cancellationToken)
        {
            foreach (var baris in isiHalaman)
            {
                baris.IsIntegrityEnforced =
                    ClinicalDocumentIntegrityService.DitegakkanUntuk(baris.DocumentKind);
            }

            if (isiHalaman.Count == 0)
                return [];

            var idDokumen = isiHalaman.Select(x => x.DocumentId).Distinct().ToList();

            var keutuhan = await _dbContext.Set<MrcClinicalDocumentIntegrity>()
                .AsNoTracking()
                .Where(x => x.PatientId == patientId
                            && !x.IsDelete
                            && idDokumen.Contains(x.DocumentId))
                .Select(x => new
                {
                    x.DocumentKind,
                    x.DocumentId,
                    x.IntegrityStatus,
                    x.SignedAt
                })
                .ToListAsync(cancellationToken);

            if (keutuhan.Count == 0)
                return [];

            // Dicocokkan memakai pasangan jenis dan id, bukan id saja. Id dokumen hanya unik di
            // dalam tabel asalnya, jadi mencocokkan dengan id saja berisiko salah pasang.
            var peta = keutuhan.ToDictionary(
                x => (x.DocumentKind, x.DocumentId),
                x => x.IntegrityStatus);

            foreach (var baris in isiHalaman)
            {
                if (!peta.TryGetValue((baris.DocumentKind, baris.DocumentId), out var status))
                    continue;

                baris.IntegrityStatus = status;
                baris.IntegrityStatusName = NamaStatusKeutuhan(status);
            }

            return keutuhan
                .Where(x => x.SignedAt.HasValue)
                .ToDictionary(x => (x.DocumentKind, x.DocumentId), x => x.SignedAt!.Value);
        }

        // =====================================================================
        // Penanda Susulan (BE-26)
        // =====================================================================

        /// <summary>
        /// Menandai baris yang final SESUDAH tutup klinis kunjungannya (`RM-DEC-043`, `051`).
        ///
        /// Dihitung saat dibaca dan tidak disimpan. Hanya isi halaman yang ditanyakan, sehingga
        /// biayanya paling banyak lima query kecil berapa pun banyaknya dokumen pasien.
        ///
        /// WAKTU FINAL, sumber pertama yang tersedia:
        /// <list type="number">
        /// <item>waktu final dari modul pemilik atau model `ClinicalManagement` baru (<c>FinalizedAt</c>);</item>
        /// <item>waktu tanda tangan pada daftar keutuhan, untuk jenis lama yang sudah ditandatangani;</item>
        /// <item>waktu baris masuk sistem — jenis lama tanpa status final dianggap final saat tersimpan,
        /// sama seperti sumber sementara pada A4.10, dan sesuai "tanggal masuk" `RM-DEC-006`.</item>
        /// </list>
        /// Waktu kejadian klinis (<c>OccurredAt</c>) sengaja tidak dipakai: waktu itu diisi petugas
        /// dan dapat dimundurkan, sehingga catatan yang dientri belakangan akan lolos dari penanda.
        ///
        /// Final TEPAT pada waktu tutup tidak bertanda — penandanya "sesudah", bukan "sejak".
        /// </summary>
        private async Task LengkapiPenandaSusulanAsync(
            List<MedicalRecordTimelineItemResponse> isiHalaman,
            IReadOnlyDictionary<(ClinicalDocumentKind, Guid), DateTime> waktuTandaTangan,
            CancellationToken cancellationToken)
        {
            var idKunjungan = isiHalaman
                .Where(x => x.EncounterId.HasValue && x.EncounterId.Value != Guid.Empty)
                .Select(x => x.EncounterId!.Value)
                .Distinct()
                .ToList();

            if (idKunjungan.Count == 0)
                return;

            // Urutan sumber waktu tutup (A4.12) ada di ClinicalClosureTimeReader, dipakai bersama
            // penilaian kelengkapan (BE-39).
            var waktuTutup = await ClinicalClosureTimeReader.ReadAsync(_dbContext, idKunjungan, cancellationToken);

            foreach (var baris in isiHalaman)
            {
                if (baris.EncounterId is not { } kunjungan
                    || !waktuTutup.TryGetValue(kunjungan, out var tutup))
                    continue;

                baris.ClinicalClosedAt = tutup.Waktu;
                baris.IsClinicalClosureDerived = tutup.Turunan;

                var final = baris.FinalizedAt
                    ?? (waktuTandaTangan.TryGetValue((baris.DocumentKind, baris.DocumentId), out var ditandatangani)
                        ? ditandatangani
                        : baris.EnteredAt);

                baris.IsLateEntry = final.HasValue && final.Value > tutup.Waktu;
            }
        }

        // =====================================================================
        // Ringkasan berkas
        // =====================================================================

        /// <summary>
        /// Ringkasan berkas rekam medis: identitas, alergi aktif, diagnosis aktif, dan jumlah
        /// dokumen per jenis.
        ///
        /// Mengembalikan <c>null</c> bila pasien tidak ditemukan. Pada pemakaian normal keadaan
        /// itu tidak terjadi, karena controller sudah menilai keberadaan pasien lebih dulu lewat
        /// <see cref="MedicalRecordAccessAuditService"/>.
        ///
        /// Alergi didahulukan pada urutan yang mengancam jiwa. Ini bukan selera penyusunan:
        /// alergi yang mengancam jiwa harus terbaca lebih dulu oleh siapa pun yang membuka
        /// berkas.
        /// </summary>
        public async Task<MedicalRecordSummaryResponse?> GetSummaryAsync(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            if (patientId == Guid.Empty)
                throw new InvalidOperationException("Id pasien tidak valid.");

            var pasien = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => x.Id == patientId && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.MedicalRecordNumber,
                    x.PatientCode,
                    x.FullName,
                    x.BirthDate,
                    x.Gender
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (pasien == null)
                return null;

            var hasil = new MedicalRecordSummaryResponse
            {
                Patient = new MedicalRecordPatientIdentityResponse
                {
                    PatientId = pasien.Id,
                    MedicalRecordNumber = pasien.MedicalRecordNumber,
                    PatientCode = pasien.PatientCode,
                    FullName = pasien.FullName,
                    BirthDate = pasien.BirthDate,
                    AgeYear = HitungUmur(pasien.BirthDate),
                    GenderName = NamaJenisKelamin(pasien.Gender)
                }
            };

            // Tingkat keparahan diambil apa adanya lalu diterjemahkan setelah data sampai, karena
            // penerjemahan enum menjadi teks tidak dapat dijalankan basis data.
            var alergi = await _dbContext.Set<TrxPatientAllergy>()
                .AsNoTracking()
                .Where(x => x.PatientId == patientId
                            && !x.IsDelete
                            && !x.IsCancel
                            && x.IsActive
                            && x.AllergyStatus == PatientAllergyStatus.Active)
                .OrderByDescending(x => x.IsLifeThreatening)
                .ThenByDescending(x => x.Severity)
                .ThenByDescending(x => x.ReportedDateTime)
                .Take(BatasRingkasan)
                .Select(x => new
                {
                    x.Id,
                    x.AllergenName,
                    x.AllergenGroupName,
                    x.ReactionType,
                    x.Severity,
                    x.IsLifeThreatening,
                    x.IsHighRisk,
                    x.ReportedDateTime
                })
                .ToListAsync(cancellationToken);

            hasil.ActiveAllergies = alergi
                .Select(x => new MedicalRecordAllergyBriefResponse
                {
                    DocumentId = x.Id,
                    AllergenName = x.AllergenName,
                    AllergenGroupName = x.AllergenGroupName,
                    ReactionType = x.ReactionType,
                    SeverityName = NamaKeparahanAlergi(x.Severity),
                    IsLifeThreatening = x.IsLifeThreatening,
                    IsHighRisk = x.IsHighRisk,
                    ReportedDateTime = x.ReportedDateTime
                })
                .ToList();

            hasil.ActiveDiagnoses = await _dbContext.Set<TrxPatientDiagnosis>()
                .AsNoTracking()
                .Where(x => x.PatientId == patientId
                            && !x.IsDelete
                            && !x.IsCancel
                            && x.IsActive
                            && x.DiagnosisStatus == PatientDiagnosisStatus.Active)
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.DiagnosisDateTime)
                .Take(BatasRingkasan)
                .Select(x => new MedicalRecordDiagnosisBriefResponse
                {
                    DocumentId = x.Id,
                    DiagnosisCode = x.DiagnosisCode,
                    DiagnosisName = x.DiagnosisName,
                    IsPrimary = x.IsPrimary,
                    IsChronic = x.IsChronic,
                    DiagnosisDateTime = x.DiagnosisDateTime
                })
                .ToListAsync(cancellationToken);

            var (jumlah, gagal) = await GetDocumentCountsAsync(patientId, cancellationToken);

            hasil.DocumentCounts = jumlah;
            hasil.FailedSources = gagal;
            hasil.UnavailableKinds = KeteranganBelumTersedia(JenisBelumTersedia);
            hasil.TotalDocument = jumlah.Sum(x => x.Total);

            return hasil;
        }

        /// <summary>
        /// Menghitung jumlah dokumen pasien pada setiap jenis.
        ///
        /// Memakai jalur pembacaan yang sama dengan riwayat, hanya tanpa mengambil barisnya.
        /// Akibatnya aturan penyaringan — dokumen terhapus dan dibatalkan — dijamin sama persis
        /// dengan yang dipakai daftar riwayat. Angka ringkasan yang berbeda dari isi daftar
        /// adalah kebingungan yang tidak perlu.
        ///
        /// Sumber yang gagal dihitung diisolasi sama seperti pada riwayat.
        /// </summary>
        public async Task<(List<MedicalRecordDocumentCountResponse> Counts,
                           List<MedicalRecordTimelineSourceFailure> Failures)>
            GetDocumentCountsAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            if (patientId == Guid.Empty)
                throw new InvalidOperationException("Id pasien tidak valid.");

            var permintaan = new MedicalRecordTimelineQuery { PatientId = patientId };

            var jumlah = new List<MedicalRecordDocumentCountResponse>();
            var gagal = new List<MedicalRecordTimelineSourceFailure>();
            var jamBerkas = Stopwatch.StartNew();

            void Catat(ClinicalDocumentKind jenis, int total) => jumlah.Add(new MedicalRecordDocumentCountResponse
            {
                DocumentKind = jenis,
                DocumentKindName = NamaJenis(jenis),
                Total = total,
                IsIntegrityEnforced = ClinicalDocumentIntegrityService.DitegakkanUntuk(jenis)
            });

            // Jenis tanpa pembaca tidak dihitung nol — angka nol terbaca "pasien tidak punya
            // dokumen jenis ini". Jenis itu dilaporkan lewat JenisBelumTersedia oleh pemanggil.
            foreach (var jenis in SeluruhJenis.Where(JenisBawaan.Contains))
            {
                // Batas nol: hanya menghitung, tidak mengambil baris.
                var (berhasil, nilai) = await BacaDenganBatasWaktuAsync(
                    [jenis],
                    batas => AmbilJenisAsync(jenis, permintaan, 0, batas),
                    jamBerkas,
                    gagal,
                    cancellationToken);

                if (berhasil)
                    Catat(jenis, nilai.Jumlah);
            }

            foreach (var kelompok in SeluruhJenis
                         .Where(_sumberPerJenis.ContainsKey)
                         .GroupBy(x => _sumberPerJenis[x]))
            {
                var jenisPembaca = kelompok.ToList();

                var (berhasil, halamanSumber) = await BacaDenganBatasWaktuAsync(
                    jenisPembaca,
                    batas => kelompok.Key.ReadAsync(new MedicalRecordSourceQuery
                    {
                        PatientId = patientId,
                        Kinds = jenisPembaca,
                        Limit = 0
                    }, batas),
                    jamBerkas,
                    gagal,
                    cancellationToken);

                if (!berhasil)
                    continue;

                foreach (var jenis in jenisPembaca)
                    Catat(jenis, halamanSumber.Totals.GetValueOrDefault(jenis));
            }

            return (jumlah, gagal);
        }

        // =====================================================================
        // Detail satu dokumen
        // =====================================================================

        /// <summary>
        /// Detail satu dokumen klinis beserta status keutuhan dan addendumnya.
        ///
        /// Mengembalikan <c>null</c> bila dokumen tidak ditemukan **atau bukan milik pasien
        /// yang diminta**. Pemeriksaan kepemilikan itu bukan kemewahan: tanpa itu, siapa pun
        /// yang berhak membuka rekam medis satu pasien dapat membaca dokumen pasien lain hanya
        /// dengan menebak id-nya.
        ///
        /// `PrivateNote` TIDAK PERNAH ikut. Yang dikembalikan hanya penandanya.
        /// </summary>
        public async Task<MedicalRecordDocumentDetailResponse?> GetDocumentDetailAsync(
            Guid patientId,
            ClinicalDocumentKind documentKind,
            Guid documentId,
            CancellationToken cancellationToken = default)
        {
            if (patientId == Guid.Empty)
                throw new InvalidOperationException("Id pasien tidak valid.");

            if (documentId == Guid.Empty || !Enum.IsDefined(documentKind))
                return null;

            // Jenis milik modul lain dibaca langsung ke pemiliknya; isinya tidak disimpan di sini.
            if (_sumberPerJenis.TryGetValue(documentKind, out var pembaca))
            {
                var rincian = await pembaca.ReadDetailAsync(documentKind, documentId, patientId, cancellationToken);
                return rincian == null ? null : await LengkapiRincianAsync(new MedicalRecordDocumentDetailResponse
                {
                    DocumentKind = documentKind,
                    DocumentKindName = NamaJenis(documentKind),
                    DocumentId = documentId,
                    PatientId = patientId,
                    EncounterId = rincian.EncounterId,
                    DocumentNumber = Potong(rincian.DocumentNumber),
                    Title = Potong(rincian.Title),
                    OccurredAt = rincian.OccurredAt ?? rincian.FinalizedAt,
                    IsIntegrityEnforced = ClinicalDocumentIntegrityService.DitegakkanUntuk(documentKind),
                    Sections = rincian.Sections
                }, cancellationToken);
            }

            if (!JenisBawaan.Contains(documentKind))
                return null;

            var isi = await AmbilIsiDokumenAsync(patientId, documentKind, documentId, cancellationToken);

            if (isi == null)
                return null;

            return await LengkapiRincianAsync(new MedicalRecordDocumentDetailResponse
            {
                DocumentKind = documentKind,
                DocumentKindName = NamaJenis(documentKind),
                DocumentId = documentId,
                PatientId = patientId,
                EncounterId = isi.EncounterId,
                DocumentNumber = Potong(isi.DocumentNumber),
                Title = Potong(isi.Title),
                OccurredAt = isi.OccurredAt,
                IsCancelled = isi.IsCancelled,
                HasPrivateNote = isi.HasPrivateNote,
                IsIntegrityEnforced = ClinicalDocumentIntegrityService.DitegakkanUntuk(documentKind),
                Sections = isi.Sections
            }, cancellationToken);
        }

        /// <summary>Menempelkan status keutuhan dan addendum pada rincian satu dokumen.</summary>
        private async Task<MedicalRecordDocumentDetailResponse> LengkapiRincianAsync(
            MedicalRecordDocumentDetailResponse hasil,
            CancellationToken cancellationToken)
        {
            var documentKind = hasil.DocumentKind;
            var documentId = hasil.DocumentId;

            var keutuhan = await _dbContext.Set<MrcClinicalDocumentIntegrity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.DocumentKind == documentKind
                         && x.DocumentId == documentId
                         && !x.IsDelete,
                    cancellationToken);

            if (keutuhan == null)
                return hasil;

            hasil.IntegrityStatus = keutuhan.IntegrityStatus;
            hasil.IntegrityStatusName = NamaStatusKeutuhan(keutuhan.IntegrityStatus);
            hasil.SignedAt = keutuhan.SignedAt;
            hasil.AuthorUserId = keutuhan.IsAuthorKnown ? keutuhan.AuthorUserId : null;
            hasil.AddendumCount = keutuhan.AddendumCount;

            var addendum = await _dbContext.Set<MrcClinicalNoteAddendum>()
                .AsNoTracking()
                .Where(x => x.IntegrityId == keutuhan.Id && !x.IsDelete)
                .OrderBy(x => x.Sequence)
                .Select(x => new ClinicalNoteAddendumResponse
                {
                    Id = x.Id,
                    IntegrityId = x.IntegrityId,
                    Sequence = x.Sequence,
                    AuthorUserId = x.AuthorUserId,
                    IsSubstituteAuthor = x.IsSubstituteAuthor,
                    DelegationId = x.DelegationId,
                    AddendumText = x.AddendumText,
                    CorrectionReason = x.CorrectionReason,
                    SignedAt = x.SignedAt,
                    Purpose = x.AddendumPurpose
                })
                .ToListAsync(cancellationToken);

            hasil.Addendums = addendum;

            // Nama penulis dan nama pembuat addendum diambil sekali untuk seluruhnya.
            var idPengguna = addendum.Select(x => x.AuthorUserId).ToList();
            if (hasil.AuthorUserId.HasValue)
                idPengguna.Add(hasil.AuthorUserId.Value);

            idPengguna = idPengguna.Distinct().ToList();

            if (idPengguna.Count == 0)
                return hasil;

            var nama = await _dbContext.Set<ApplicationUser>()
                .AsNoTracking()
                .Where(x => idPengguna.Contains(x.Id))
                .Select(x => new { x.Id, x.DisplayName })
                .ToListAsync(cancellationToken);

            if (hasil.AuthorUserId.HasValue)
            {
                hasil.AuthorName = nama
                    .FirstOrDefault(x => x.Id == hasil.AuthorUserId.Value)?.DisplayName;
            }

            foreach (var baris in hasil.Addendums)
                baris.AuthorName = nama.FirstOrDefault(x => x.Id == baris.AuthorUserId)?.DisplayName;

            return hasil;
        }

        // =====================================================================
        // Catatan pribadi
        // =====================================================================

        /// <summary>
        /// Apakah jenis dokumen ini memang memiliki kolom catatan pribadi.
        ///
        /// Hanya CPPT yang punya. Dua belas jenis lain tidak — dan itu perlu dibedakan dengan
        /// tegas dari "punya tetapi kosong", supaya pembaca tidak menyangka ada sesuatu yang
        /// disembunyikan darinya.
        /// </summary>
        public static bool MendukungCatatanPribadi(ClinicalDocumentKind jenis)
            => jenis == ClinicalDocumentKind.ProgressNote;

        /// <summary>
        /// Membaca isi catatan pribadi sebuah dokumen klinis (`RM-DEC-022`).
        ///
        /// **Satu-satunya tempat isi `PrivateNote` keluar dari modul ini.** Seluruh pembacaan
        /// lain — riwayat, ringkasan, detail dokumen — tidak pernah membawanya.
        ///
        /// Service ini TIDAK memeriksa izin `MedicalRecord : ReadPrivateNote` maupun keharusan
        /// menyertakan keperluan akses. Keduanya ditegakkan controller lebih dulu, lewat atribut
        /// izin dan <see cref="MedicalRecordAccessAuditService"/> dengan cakupan
        /// <see cref="MedicalRecordAccessScope.PrivateNote"/>.
        ///
        /// Mengembalikan <c>null</c> bila dokumen tidak ditemukan atau bukan milik pasien yang
        /// diminta.
        /// </summary>
        public async Task<MedicalRecordPrivateNoteResponse?> GetPrivateNoteAsync(
            Guid patientId,
            ClinicalDocumentKind documentKind,
            Guid documentId,
            CancellationToken cancellationToken = default)
        {
            if (patientId == Guid.Empty)
                throw new InvalidOperationException("Id pasien tidak valid.");

            if (documentId == Guid.Empty || !MendukungCatatanPribadi(documentKind))
                return null;

            var dokumen = await _dbContext.Set<TrxPatientIntegratedProgressNote>()
                .AsNoTracking()
                .Where(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.EncounterId,
                    x.ProgressNoteNumber,
                    x.NoteDateTime,
                    x.ProviderUserId,
                    x.ProviderDisplayNameSnapshot,
                    x.PrivateNote
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (dokumen == null)
                return null;

            var hasil = new MedicalRecordPrivateNoteResponse
            {
                DocumentKind = documentKind,
                DocumentKindName = NamaJenis(documentKind),
                DocumentId = dokumen.Id,
                PatientId = patientId,
                EncounterId = dokumen.EncounterId,
                DocumentNumber = dokumen.ProgressNoteNumber,
                OccurredAt = dokumen.NoteDateTime,
                AuthorUserId = dokumen.ProviderUserId,
                AuthorName = dokumen.ProviderDisplayNameSnapshot,
                PrivateNote = dokumen.PrivateNote,
                HasPrivateNote = !string.IsNullOrWhiteSpace(dokumen.PrivateNote)
            };

            // Nama penulis diambil dari akun bila snapshot-nya kosong. Snapshot tetap
            // didahulukan, karena ia mencatat nama sebagaimana berlaku saat catatan dibuat.
            if (!string.IsNullOrWhiteSpace(hasil.AuthorName) || !hasil.AuthorUserId.HasValue)
                return hasil;

            hasil.AuthorName = await _dbContext.Set<ApplicationUser>()
                .AsNoTracking()
                .Where(x => x.Id == hasil.AuthorUserId.Value)
                .Select(x => x.DisplayName)
                .FirstOrDefaultAsync(cancellationToken);

            return hasil;
        }

        /// <summary>Isi mentah satu dokumen sebelum status keutuhan dan addendum ditempelkan.</summary>
        private sealed record IsiDokumen(
            Guid? EncounterId,
            string? DocumentNumber,
            string? Title,
            DateTime OccurredAt,
            bool IsCancelled,
            bool HasPrivateNote,
            List<MedicalRecordDocumentSectionResponse> Sections);

        /// <summary>
        /// Membaca isi satu dokumen dari tabel asalnya.
        ///
        /// Isinya disajikan sebagai pasangan label dan nilai, bukan sebagai tiga belas bentuk
        /// balasan yang berbeda. Alasannya sederhana: layar rekam medis harus dapat menampilkan
        /// dokumen apa pun tanpa harus mengenali tiga belas bentuk data, dan menambah jenis
        /// dokumen kelak tidak boleh berarti mengubah bentuk kontrak.
        ///
        /// Kolom yang panjangnya tidak terbatas — misalnya isi catatan — tetap disertakan di
        /// sini, karena inilah tempatnya. Yang tidak pernah disertakan hanya `PrivateNote`.
        /// </summary>
        private async Task<IsiDokumen?> AmbilIsiDokumenAsync(
            Guid patientId,
            ClinicalDocumentKind jenis,
            Guid documentId,
            CancellationToken cancellationToken)
        {
            var bagian = new List<MedicalRecordDocumentSectionResponse>();

            switch (jenis)
            {
                case ClinicalDocumentKind.ProgressNote:
                {
                    var e = await _dbContext.Set<TrxPatientIntegratedProgressNote>().AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete, cancellationToken);
                    if (e == null) return null;

                    Tambah(bagian, "Profesi", e.ProfessionName ?? e.ProfessionType);
                    Tambah(bagian, "Pemberi layanan", e.ProviderDisplayNameSnapshot);
                    Tambah(bagian, "Unit layanan", e.ServiceUnitNameSnapshot);
                    Tambah(bagian, "Subjektif", e.SubjectiveSummary);
                    Tambah(bagian, "Objektif", e.ObjectiveSummary);
                    Tambah(bagian, "Asesmen", e.AssessmentSummary);
                    Tambah(bagian, "Rencana", e.PlanSummary);
                    Tambah(bagian, "Instruksi", e.Instruction);
                    Tambah(bagian, "Evaluasi", e.Evaluation);
                    Tambah(bagian, "Catatan", e.NoteText);

                    // PrivateNote sengaja TIDAK disertakan. Hanya penandanya.
                    return new IsiDokumen(
                        e.EncounterId, e.ProgressNoteNumber,
                        e.ProfessionName ?? e.ProfessionType, e.NoteDateTime, e.IsCancel,
                        !string.IsNullOrWhiteSpace(e.PrivateNote), bagian);
                }

                case ClinicalDocumentKind.Consultation:
                {
                    var e = await _dbContext.Set<TrxDoctorConsultation>().AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete, cancellationToken);
                    if (e == null) return null;

                    Tambah(bagian, "Keluhan utama", e.ChiefComplaint);
                    Tambah(bagian, "Riwayat penyakit sekarang", e.HistoryOfPresentIllness);
                    Tambah(bagian, "Pemeriksaan fisik", e.PhysicalExamination);
                    Tambah(bagian, "Subjektif", e.Subjective);
                    Tambah(bagian, "Objektif", e.Objective);
                    Tambah(bagian, "Asesmen", e.Assessment);
                    Tambah(bagian, "Rencana", e.Plan);
                    Tambah(bagian, "Diagnosis", e.DiagnosisText);
                    Tambah(bagian, "Tindakan", e.ProcedureText);
                    Tambah(bagian, "Resep", e.PrescriptionText);
                    Tambah(bagian, "Rencana kontrol", e.FollowUpNote);
                    Tambah(bagian, "Catatan dokter", e.DoctorNote);

                    return new IsiDokumen(
                        e.EncounterId, e.ConsultationNumber, "Konsultasi Dokter",
                        e.ConsultationDateTime, e.IsCancel, false, bagian);
                }

                case ClinicalDocumentKind.Assessment:
                {
                    var e = await _dbContext.Set<TrxPatientAssessment>().AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete, cancellationToken);
                    if (e == null) return null;

                    Tambah(bagian, "Keluhan utama", e.ChiefComplaint);
                    Tambah(bagian, "Riwayat penyakit sekarang", e.CurrentIllnessHistory);
                    Tambah(bagian, "Riwayat obat", e.MedicationHistory);
                    Tambah(bagian, "Catatan alergi", e.AllergyNote);
                    Tambah(bagian, "Catatan nyeri", e.PainNote);
                    Tambah(bagian, "Catatan gizi", e.NutritionNote);
                    Tambah(bagian, "Catatan risiko jatuh", e.FallRiskNote);
                    Tambah(bagian, "Catatan fungsional", e.FunctionalNote);
                    Tambah(bagian, "Catatan psikososial", e.PsychosocialNote);
                    Tambah(bagian, "Catatan edukasi", e.EducationNote);
                    Tambah(bagian, "Catatan perawat", e.NurseNote);

                    return new IsiDokumen(
                        e.EncounterId, e.AssessmentNumber, "Asesmen Pasien",
                        e.AssessmentDateTime, e.IsCancel, false, bagian);
                }

                case ClinicalDocumentKind.Diagnosis:
                {
                    var e = await _dbContext.Set<TrxPatientDiagnosis>().AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete, cancellationToken);
                    if (e == null) return null;

                    Tambah(bagian, "Kode diagnosis", e.DiagnosisCode);
                    Tambah(bagian, "Nama diagnosis", e.DiagnosisName);
                    Tambah(bagian, "Diagnosis utama", e.IsPrimary ? "Ya" : "Tidak");
                    Tambah(bagian, "Menahun", e.IsChronic ? "Ya" : "Tidak");
                    Tambah(bagian, "Catatan klinis", e.ClinicalNote);
                    Tambah(bagian, "Catatan asesmen", e.AssessmentNote);
                    Tambah(bagian, "Catatan rencana", e.PlanNote);
                    Tambah(bagian, "Diagnosis banding", e.DifferentialDiagnosisNote);
                    Tambah(bagian, "Temuan pendukung", e.SupportingFindingNote);

                    return new IsiDokumen(
                        e.EncounterId, e.DiagnosisCode, e.DiagnosisName,
                        e.DiagnosisDateTime, e.IsCancel, false, bagian);
                }

                case ClinicalDocumentKind.Procedure:
                {
                    var e = await _dbContext.Set<TrxPatientProcedure>().AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete, cancellationToken);
                    if (e == null) return null;

                    Tambah(bagian, "Kode tindakan", e.ProcedureCodeSnapshot);
                    Tambah(bagian, "Nama tindakan", e.ProcedureNameSnapshot);
                    Tambah(bagian, "Kelompok tindakan", e.ProcedureCategoryNameSnapshot);
                    Tambah(bagian, "Unit pelaksana", e.UnitNameSnapshot);
                    Tambah(bagian, "Catatan klinis", e.ClinicalNote);
                    Tambah(bagian, "Hasil", e.ResultNote);
                    Tambah(bagian, "Instruksi", e.InstructionNote);
                    Tambah(bagian, "Komplikasi", e.ComplicationNote);
                    Tambah(bagian, "Rencana lanjutan", e.FollowUpInstruction);

                    return new IsiDokumen(
                        e.EncounterId, e.ProcedureCodeSnapshot, e.ProcedureNameSnapshot,
                        e.ProcedureDateTime, e.IsCancel, false, bagian);
                }

                case ClinicalDocumentKind.VitalSign:
                {
                    var e = await _dbContext.Set<TrxPatientVitalSign>().AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete, cancellationToken);
                    if (e == null) return null;

                    Tambah(bagian, "Tekanan darah",
                        e.BloodPressureSystolic.HasValue && e.BloodPressureDiastolic.HasValue
                            ? $"{e.BloodPressureSystolic}/{e.BloodPressureDiastolic} mmHg"
                            : null);
                    Tambah(bagian, "Nadi", e.PulseRate?.ToString());
                    Tambah(bagian, "Laju napas", e.RespiratoryRate?.ToString());
                    Tambah(bagian, "Suhu", e.Temperature?.ToString());
                    Tambah(bagian, "Saturasi oksigen", e.OxygenSaturation?.ToString());
                    Tambah(bagian, "Berat badan", e.Weight?.ToString());
                    Tambah(bagian, "Tinggi badan", e.Height?.ToString());
                    Tambah(bagian, "Skala nyeri", e.HasPain ? e.PainScale?.ToString() : null);
                    Tambah(bagian, "Lokasi pengukuran", e.ObservationLocation);
                    Tambah(bagian, "Catatan klinis", e.ClinicalNote);
                    Tambah(bagian, "Catatan", e.Notes);

                    return new IsiDokumen(
                        e.EncounterId, e.VitalSignRecordNumber, "Tanda Vital",
                        e.ObservationDateTime, e.IsCancel, false, bagian);
                }

                case ClinicalDocumentKind.Allergy:
                {
                    var e = await _dbContext.Set<TrxPatientAllergy>().AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete, cancellationToken);
                    if (e == null) return null;

                    Tambah(bagian, "Alergen", e.AllergenName);
                    Tambah(bagian, "Golongan alergen", e.AllergenGroupName);
                    Tambah(bagian, "Jenis reaksi", e.ReactionType);
                    Tambah(bagian, "Uraian reaksi", e.ReactionDescription);
                    Tambah(bagian, "Tingkat keparahan", NamaKeparahanAlergi(e.Severity));
                    Tambah(bagian, "Mengancam jiwa", e.IsLifeThreatening ? "Ya" : "Tidak");
                    Tambah(bagian, "Catatan keselamatan pasien", e.PatientSafetyNote);
                    Tambah(bagian, "Sumber keterangan", e.SourceOfInformation);
                    Tambah(bagian, "Catatan klinis", e.ClinicalNote);

                    return new IsiDokumen(
                        e.EncounterId, e.AllergyRecordNumber, e.AllergenName,
                        e.ReportedDateTime, e.IsCancel, false, bagian);
                }

                case ClinicalDocumentKind.MedicalHistory:
                {
                    var e = await _dbContext.Set<TrxPatientMedicalHistory>().AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete, cancellationToken);
                    if (e == null) return null;

                    Tambah(bagian, "Kondisi", e.ConditionName);
                    Tambah(bagian, "Kelompok kondisi", e.ConditionGroupName);
                    Tambah(bagian, "Menahun", e.IsChronic ? "Ya" : "Tidak");
                    Tambah(bagian, "Sedang dalam pengobatan", e.IsUnderTreatment ? "Ya" : "Tidak");
                    Tambah(bagian, "Riwayat pengobatan", e.TreatmentHistory);
                    Tambah(bagian, "Riwayat obat", e.MedicationHistory);
                    Tambah(bagian, "Riwayat pembedahan", e.SurgeryHistory);
                    Tambah(bagian, "Riwayat rawat inap", e.HospitalizationHistory);
                    Tambah(bagian, "Catatan komplikasi", e.ComplicationNote);
                    Tambah(bagian, "Catatan klinis", e.ClinicalNote);

                    return new IsiDokumen(
                        e.EncounterId, e.MedicalHistoryRecordNumber, e.ConditionName,
                        e.RecordedDateTime, e.IsCancel, false, bagian);
                }

                case ClinicalDocumentKind.FamilyHistory:
                {
                    var e = await _dbContext.Set<TrxPatientFamilyHistory>().AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete, cancellationToken);
                    if (e == null) return null;

                    Tambah(bagian, "Kondisi", e.ConditionName);
                    Tambah(bagian, "Anggota keluarga", e.FamilyMemberNameSnapshot);
                    Tambah(bagian, "Hubungan", e.RelationshipDescription);
                    Tambah(bagian, "Keturunan", e.IsHereditaryDisease ? "Ya" : "Tidak");
                    Tambah(bagian, "Sebab meninggal", e.CauseOfDeath);
                    Tambah(bagian, "Catatan risiko", e.RiskNote);
                    Tambah(bagian, "Saran penapisan", e.ScreeningRecommendation);
                    Tambah(bagian, "Catatan klinis", e.ClinicalNote);

                    return new IsiDokumen(
                        e.EncounterId, e.FamilyHistoryRecordNumber, e.ConditionName,
                        e.RecordedDateTime, e.IsCancel, false, bagian);
                }

                case ClinicalDocumentKind.ClinicalDocument:
                {
                    var e = await _dbContext.Set<TrxPatientClinicalDocument>().AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete, cancellationToken);
                    if (e == null) return null;

                    Tambah(bagian, "Judul", e.DocumentTitle);
                    Tambah(bagian, "Kelompok dokumen", e.DocumentCategoryName);
                    Tambah(bagian, "Penerbit luar", e.ExternalProviderName);
                    Tambah(bagian, "Dokter luar", e.ExternalDoctorName);
                    Tambah(bagian, "Nomor dokumen luar", e.ExternalDocumentNumber);
                    Tambah(bagian, "Ringkasan", e.DocumentSummary);
                    Tambah(bagian, "Ringkasan temuan klinis", e.ClinicalFindingSummary);
                    Tambah(bagian, "Kesan", e.Impression);
                    Tambah(bagian, "Saran", e.Recommendation);
                    Tambah(bagian, "Nama berkas", e.FileName);

                    return new IsiDokumen(
                        e.EncounterId, e.ClinicalDocumentNumber, e.DocumentTitle,
                        e.DocumentDateTime, e.IsCancel, false, bagian);
                }

                case ClinicalDocumentKind.NoteAttachment:
                {
                    var e = await _dbContext.Set<TrxClinicalNoteAttachment>().AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete, cancellationToken);
                    if (e == null) return null;

                    Tambah(bagian, "Judul", e.AttachmentTitle);
                    Tambah(bagian, "Kelompok lampiran", e.AttachmentCategoryName);
                    Tambah(bagian, "Uraian", e.AttachmentDescription);
                    Tambah(bagian, "Bagian catatan", e.NoteSectionName);
                    Tambah(bagian, "Lokasi tubuh", e.BodySite);
                    Tambah(bagian, "Catatan klinis", e.ClinicalNote);
                    Tambah(bagian, "Temuan", e.FindingNote);
                    Tambah(bagian, "Penafsiran", e.InterpretationNote);
                    Tambah(bagian, "Tindak lanjut", e.FollowUpNote);
                    Tambah(bagian, "Nama berkas", e.FileName);

                    return new IsiDokumen(
                        e.EncounterId, e.AttachmentNumber, e.AttachmentTitle,
                        e.UploadedAt, e.IsCancel, false, bagian);
                }

                case ClinicalDocumentKind.MedicalCertificate:
                {
                    var e = await _dbContext.Set<TrxMedicalCertificate>().AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete, cancellationToken);
                    if (e == null) return null;

                    Tambah(bagian, "Judul", e.CertificateTitle);
                    Tambah(bagian, "Kelompok surat", e.CertificateCategoryName);
                    Tambah(bagian, "Keperluan", e.CertificatePurpose);
                    Tambah(bagian, "Diagnosis", e.DiagnosisNameSnapshot);
                    Tambah(bagian, "Ringkasan klinis", e.ClinicalSummary);
                    Tambah(bagian, "Isi pernyataan", e.CertificateStatement);
                    Tambah(bagian, "Saran medis", e.MedicalRecommendation);
                    Tambah(bagian, "Jumlah hari istirahat", e.SickLeaveDays?.ToString());
                    Tambah(bagian, "Diterbitkan untuk", e.RecipientName);
                    Tambah(bagian, "Instansi penerima", e.RecipientInstitutionName);

                    return new IsiDokumen(
                        e.EncounterId, e.MedicalCertificateNumber, e.CertificateTitle,
                        e.CertificateDateTime, e.IsCancel, false, bagian);
                }

                case ClinicalDocumentKind.Consent:
                {
                    var e = await _dbContext.Set<TrxPatientConsent>().AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete, cancellationToken);
                    if (e == null) return null;

                    Tambah(bagian, "Judul", e.ConsentTitle);
                    Tambah(bagian, "Kelompok persetujuan", e.ConsentCategoryName);
                    Tambah(bagian, "Uraian", e.ConsentDescription);
                    Tambah(bagian, "Tindakan", e.ProcedureNameSnapshot);
                    Tambah(bagian, "Penjelasan diagnosis", e.DiagnosisExplanation);
                    Tambah(bagian, "Penjelasan tindakan", e.ProcedureExplanation);
                    Tambah(bagian, "Penjelasan risiko", e.RiskExplanation);
                    Tambah(bagian, "Penjelasan pilihan lain", e.AlternativeExplanation);
                    Tambah(bagian, "Pasien menyatakan paham", e.IsPatientUnderstood ? "Ya" : "Tidak");
                    Tambah(bagian, "Pasien menyetujui", e.IsPatientAgreed ? "Ya" : "Tidak");
                    Tambah(bagian, "Penanda tangan", e.SignerName);
                    Tambah(bagian, "Hubungan penanda tangan", e.SignerRelationship);
                    Tambah(bagian, "Saksi", e.WitnessName);

                    return new IsiDokumen(
                        e.EncounterId, e.ConsentNumber, e.ConsentTitle,
                        e.ConsentDateTime, e.IsCancel, false, bagian);
                }

                // ---- BE-22: rincian model ClinicalManagement yang lahir sesudah rilis pertama ----
                // Aturan final sama dengan linimasa: dokumen yang belum final tidak dibuka.

                case ClinicalDocumentKind.NursingCarePlan:
                {
                    var e = await _dbContext.CliNursingCarePlans.AsNoTracking()
                        .Where(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete)
                        .Select(x => new
                        {
                            x.EncounterId, x.OpenedAt, x.IsCancel,
                            Butir = x.Items.Where(i => !i.IsDelete).OrderBy(i => i.AuthoredAt)
                                .Select(i => new { i.ProblemStatement, i.GoalStatement, i.PlannedIntervention, i.ItemStatus }).ToList()
                        })
                        .FirstOrDefaultAsync(cancellationToken);
                    if (e == null) return null;

                    var nomor = 1;
                    foreach (var b in e.Butir)
                    {
                        Tambah(bagian, $"Masalah {nomor}", b.ProblemStatement);
                        Tambah(bagian, $"Tujuan {nomor}", b.GoalStatement);
                        Tambah(bagian, $"Rencana tindakan {nomor}", b.PlannedIntervention);
                        Tambah(bagian, $"Keadaan {nomor}", b.ItemStatus == NursingCarePlanItemStatus.Active ? "Aktif"
                            : b.ItemStatus == NursingCarePlanItemStatus.Resolved ? "Teratasi" : "Dihentikan");
                        nomor++;
                    }

                    return new IsiDokumen(e.EncounterId, null, "Rencana Asuhan Keperawatan", e.OpenedAt, e.IsCancel, false, bagian);
                }

                case ClinicalDocumentKind.PhysicianVisit:
                {
                    var e = await _dbContext.CliPhysicianVisits.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete
                                                  && x.VisitStatus == PhysicianVisitStatus.Recorded, cancellationToken);
                    if (e == null) return null;

                    Tambah(bagian, "Peran", e.VisitRole == PhysicianVisitRole.Dpjp ? "DPJP"
                        : e.VisitRole == PhysicianVisitRole.Consultant ? "Konsulen" : "Dokter jaga");
                    Tambah(bagian, "Catatan", e.Note);

                    return new IsiDokumen(e.EncounterId, e.PhysicianVisitNumber, "Visite Dokter", e.VisitDateTime, e.IsCancel, false, bagian);
                }

                case ClinicalDocumentKind.NursingIntervention:
                {
                    var e = await _dbContext.CliNursingInterventions.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete
                                                  && x.RecordStatus == NursingInterventionStatus.Finalized, cancellationToken);
                    if (e == null) return null;

                    Tambah(bagian, "Intervensi", e.InterventionName);
                    Tambah(bagian, "Hasil", e.ResultNote);

                    return new IsiDokumen(e.EncounterId, null, e.InterventionName, e.PerformedAt, e.IsCancel, false, bagian);
                }

                case ClinicalDocumentKind.TransfusionMonitoring:
                {
                    var e = await _dbContext.CliTransfusionMonitorings.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete
                                                  && (x.Status == CliTransfusionMonitoringStatus.Completed
                                                      || x.Status == CliTransfusionMonitoringStatus.Stopped), cancellationToken);
                    if (e == null) return null;

                    Tambah(bagian, "Keadaan", e.Status == CliTransfusionMonitoringStatus.Stopped ? "Dihentikan" : "Selesai");
                    Tambah(bagian, "Alasan dihentikan", e.StopReason);

                    return new IsiDokumen(e.EncounterId, null, "Monitoring Transfusi", e.TransfusionStartedAt, e.IsCancel, false, bagian);
                }

                case ClinicalDocumentKind.DoctorCertificate:
                {
                    var e = await _dbContext.CliDoctorCertificates.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete
                                                  && x.CertificateStatus == DoctorCertificateStatus.Issued, cancellationToken);
                    if (e == null) return null;

                    Tambah(bagian, "Keperluan", e.Purpose);
                    Tambah(bagian, "Diagnosis", e.Diagnosis);
                    Tambah(bagian, "Ringkasan klinis", e.ClinicalSummary);
                    Tambah(bagian, "Catatan tambahan", e.AdditionalNote);

                    return new IsiDokumen(e.EncounterId, e.CertificateNumber, "Surat Dokter", e.IssuedDate, e.IsCancel, false, bagian);
                }

                case ClinicalDocumentKind.CaseManagementEvaluation:
                {
                    var e = await _dbContext.CliCaseManagementEvaluations.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete
                                                  && x.EvaluationStatus == CaseManagementEvaluationStatus.Completed, cancellationToken);
                    if (e == null) return null;

                    return new IsiDokumen(e.EncounterId, e.EvaluationNumber, "Evaluasi Manajemen Kasus", e.ClinicalDateTime, e.IsCancel, false, bagian);
                }

                case ClinicalDocumentKind.NosocomialInfection:
                {
                    var e = await _dbContext.TrxNosocomialInfections.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == documentId && x.PatientId == patientId && !x.IsDelete
                                                  && x.VerifiedAt != null && x.Status != NosocomialInfectionStatus.Cancelled, cancellationToken);
                    if (e == null) return null;

                    Tambah(bagian, "Jenis infeksi", e.InfectionTypeOther ?? e.InfectionType.ToString());
                    Tambah(bagian, "Terkait alat", e.IsDeviceAssociated ? e.DeviceName ?? "Ya" : "Tidak");
                    Tambah(bagian, "Kriteria terpenuhi", e.CriteriaMet);

                    return new IsiDokumen(e.EncounterId, e.NosocomialRecordNumber, "Infeksi Nosokomial", e.OnsetDateTime, e.IsCancel, false, bagian);
                }

                case ClinicalDocumentKind.TransferHandover:
                {
                    var e = await _dbContext.CliTransferHandovers.AsNoTracking()
                        .Where(x => x.Id == documentId && x.Episode!.PatientId == patientId && !x.IsDelete
                                    && x.Status == CliTransferHandoverStatus.Accepted && x.ReceivedAt != null)
                        .Select(x => new { x.Episode!.EncounterId, x.ReceivedAt, x.IsCancel, x.SoapSummary, x.HandedItems, x.SpecialInstructions })
                        .FirstOrDefaultAsync(cancellationToken);
                    if (e == null) return null;

                    Tambah(bagian, "Ringkasan SOAP", e.SoapSummary);
                    Tambah(bagian, "Barang yang diserahkan", e.HandedItems);
                    Tambah(bagian, "Instruksi khusus", e.SpecialInstructions);

                    return new IsiDokumen(e.EncounterId, null, "Serah Terima Pindah Ruang", e.ReceivedAt!.Value, e.IsCancel, false, bagian);
                }

                default:
                    return null;
            }
        }

        // =====================================================================
        // Alat bantu
        // =====================================================================

        /// <summary>
        /// Wadah satu nilai, dipakai membangun perbandingan pada query.
        ///
        /// Nilai dibungkus lebih dulu supaya EF Core mengirimkannya sebagai parameter, bukan
        /// menempelkannya sebagai angka atau tanggal di dalam teks SQL. Bila ditempelkan, setiap
        /// rentang tanggal yang berbeda menghasilkan SQL yang berbeda pula, dan rencana
        /// eksekusi basis data tidak dapat dipakai ulang.
        /// </summary>
        private sealed class Wadah<TNilai>
        {
            public TNilai Isi = default!;
        }

        /// <summary>
        /// Membentuk perbandingan pada sebuah kolom yang namanya berbeda-beda antar tabel.
        ///
        /// Diperlukan karena tiga belas tabel klinis tidak memakai satu nama kolom tanggal
        /// maupun satu bentuk kolom kunjungan yang sama.
        /// </summary>
        private static Expression<Func<TEntity, bool>> Bandingkan<TEntity, TNilai>(
            Expression<Func<TEntity, TNilai>> pemilih,
            TNilai nilai,
            Func<Expression, Expression, BinaryExpression> pembanding)
        {
            var wadah = new Wadah<TNilai> { Isi = nilai };

            Expression parameter = Expression.Field(
                Expression.Constant(wadah),
                nameof(Wadah<TNilai>.Isi));

            return Expression.Lambda<Func<TEntity, bool>>(
                pembanding(pemilih.Body, parameter),
                pemilih.Parameters);
        }

        private static readonly PropertyInfo PropertiWaktuMasuk =
            typeof(MedicalRecordTimelineItemResponse).GetProperty(
                nameof(MedicalRecordTimelineItemResponse.EnteredAt),
                BindingFlags.Instance | BindingFlags.NonPublic)!;

        /// <summary>
        /// Menambahkan waktu masuk baris (<c>CreateDateTime</c>) ke proyeksi sebuah sumber, untuk
        /// penanda Susulan pada jenis tanpa waktu final (`BE-26`).
        ///
        /// Ditambahkan di sini, satu kali, bukan pada dua puluh satu proyeksi di
        /// <see cref="AmbilJenisAsync"/> — sumber baru otomatis ikut dan tidak mungkin terlupa.
        /// Proyeksi yang bukan inisialisasi objek dibiarkan apa adanya.
        /// </summary>
        private static Expression<Func<TEntity, MedicalRecordTimelineItemResponse>> SertakanWaktuMasuk<TEntity>(
            Expression<Func<TEntity, MedicalRecordTimelineItemResponse>> proyeksi)
            where TEntity : IdentityModel
        {
            if (proyeksi.Body is not MemberInitExpression isi)
                return proyeksi;

            var entitas = proyeksi.Parameters[0];
            var waktuMasuk = Expression.Convert(
                Expression.Property(entitas, nameof(IdentityModel.CreateDateTime)),
                typeof(DateTime?));

            return Expression.Lambda<Func<TEntity, MedicalRecordTimelineItemResponse>>(
                Expression.MemberInit(isi.NewExpression, isi.Bindings.Append(Expression.Bind(PropertiWaktuMasuk, waktuMasuk))),
                entitas);
        }

        /// <summary>
        /// Memotong keterangan yang terlalu panjang.
        ///
        /// Daftar riwayat sengaja tidak membawa isi catatan klinis. Isi lengkapnya dibuka lewat
        /// endpoint detail dokumen, yang jalur aksesnya tercatat tersendiri.
        /// </summary>
        private static string? Potong(string? teks)
        {
            if (string.IsNullOrWhiteSpace(teks))
                return null;

            var rapi = teks.Trim();

            return rapi.Length <= PanjangKeteranganMaksimal
                ? rapi
                : rapi[..PanjangKeteranganMaksimal];
        }

        /// <summary>
        /// Menambahkan satu bagian isi dokumen, selama nilainya benar-benar ada.
        ///
        /// Bagian kosong sengaja tidak ikut. Menampilkan puluhan label bernilai kosong membuat
        /// yang benar-benar terisi justru sulit ditemukan.
        /// </summary>
        private static void Tambah(
            List<MedicalRecordDocumentSectionResponse> bagian,
            string label,
            string? nilai)
        {
            if (string.IsNullOrWhiteSpace(nilai))
                return;

            bagian.Add(new MedicalRecordDocumentSectionResponse
            {
                Label = label,
                Value = nilai.Trim()
            });
        }

        /// <summary>Umur dalam tahun penuh pada hari ini.</summary>
        private static int? HitungUmur(DateTime? tanggalLahir)
        {
            if (!tanggalLahir.HasValue)
                return null;

            var hariIni = DateTime.UtcNow.Date;
            var lahir = tanggalLahir.Value.Date;

            if (lahir > hariIni)
                return null;

            var umur = hariIni.Year - lahir.Year;

            if (lahir.AddYears(umur) > hariIni)
                umur--;

            return umur;
        }

        private static string? NamaJenisKelamin(Gender? jenisKelamin) => jenisKelamin switch
        {
            Gender.Male => "Laki-laki",
            Gender.Female => "Perempuan",
            Gender.Unknown => "Tidak diketahui",
            null => null,
            _ => jenisKelamin.ToString()
        };

        private static string NamaKeparahanAlergi(PatientAllergySeverity keparahan) => keparahan switch
        {
            PatientAllergySeverity.Mild => "Ringan",
            PatientAllergySeverity.Moderate => "Sedang",
            PatientAllergySeverity.Severe => "Berat",
            PatientAllergySeverity.LifeThreatening => "Mengancam Jiwa",
            _ => "Tidak Diketahui"
        };

        /// <summary>Nama jenis dokumen dalam Bahasa Indonesia, siap ditampilkan di layar.</summary>
        public static string NamaJenis(ClinicalDocumentKind jenis) => jenis switch
        {
            ClinicalDocumentKind.ProgressNote => "CPPT",
            ClinicalDocumentKind.Consultation => "Konsultasi Dokter",
            ClinicalDocumentKind.Assessment => "Asesmen Pasien",
            ClinicalDocumentKind.Diagnosis => "Diagnosis",
            ClinicalDocumentKind.Procedure => "Tindakan",
            ClinicalDocumentKind.VitalSign => "Tanda Vital",
            ClinicalDocumentKind.Allergy => "Alergi",
            ClinicalDocumentKind.MedicalHistory => "Riwayat Penyakit",
            ClinicalDocumentKind.FamilyHistory => "Riwayat Keluarga",
            ClinicalDocumentKind.ClinicalDocument => "Dokumen Klinis",
            ClinicalDocumentKind.NoteAttachment => "Lampiran Catatan",
            ClinicalDocumentKind.MedicalCertificate => "Surat Keterangan",
            ClinicalDocumentKind.Consent => "Persetujuan Tindakan",
            ClinicalDocumentKind.HemodialysisSession => "Sesi Hemodialisa",
            ClinicalDocumentKind.LaboratoryResult => "Hasil Laboratorium",
            ClinicalDocumentKind.RadiologyReport => "Hasil Radiologi",
            ClinicalDocumentKind.EmergencyTriage => "Triase IGD",
            ClinicalDocumentKind.EmergencyObservation => "Observasi IGD",
            ClinicalDocumentKind.EmergencyResuscitation => "Resusitasi IGD",
            ClinicalDocumentKind.EmergencyDisposition => "Disposisi IGD",
            ClinicalDocumentKind.DischargeSummary => "Ringkasan Pulang",
            ClinicalDocumentKind.AdmissionDocument => "Dokumen Admisi Rawat Inap",
            ClinicalDocumentKind.SurgeryReport => "Laporan Operasi",
            ClinicalDocumentKind.AnesthesiaRecord => "Catatan Anestesi",
            ClinicalDocumentKind.SurgicalSafetyChecklist => "Checklist Keselamatan Operasi",
            ClinicalDocumentKind.BirthRecord => "Catatan Kelahiran",
            ClinicalDocumentKind.NutritionCareRecord => "Asuhan Gizi",
            ClinicalDocumentKind.BloodGroupExam => "Pemeriksaan Golongan Darah",
            ClinicalDocumentKind.TransfusionReactionNotice => "Reaksi Transfusi",
            ClinicalDocumentKind.Prescription => "Resep",
            ClinicalDocumentKind.MedicationAdministration => "Pemberian Obat",
            ClinicalDocumentKind.MedicationReconciliation => "Rekonsiliasi Obat",
            ClinicalDocumentKind.McuResume => "Resume MCU",
            ClinicalDocumentKind.McuExaminationResult => "Hasil Pemeriksaan MCU",
            ClinicalDocumentKind.NursingCarePlan => "Rencana Asuhan Keperawatan",
            ClinicalDocumentKind.PhysicianVisit => "Visite Dokter",
            ClinicalDocumentKind.NursingIntervention => "Intervensi Keperawatan",
            ClinicalDocumentKind.TransfusionMonitoring => "Monitoring Transfusi",
            ClinicalDocumentKind.DoctorCertificate => "Surat Dokter",
            ClinicalDocumentKind.CaseManagementEvaluation => "Evaluasi Manajemen Kasus",
            ClinicalDocumentKind.NosocomialInfection => "Infeksi Nosokomial",
            ClinicalDocumentKind.TransferHandover => "Serah Terima Pindah Ruang",
            ClinicalDocumentKind.PeriodicObservationSummary => "Ringkasan Pencatatan Berkala",
            ClinicalDocumentKind.PathologyAnatomyResult => "Hasil Patologi Anatomi",
            _ => jenis.ToString()
        };

        /// <summary>Nama status keutuhan dalam Bahasa Indonesia.</summary>
        public static string NamaStatusKeutuhan(ClinicalDocumentIntegrityStatus status) => status switch
        {
            ClinicalDocumentIntegrityStatus.Draft => "Draf",
            ClinicalDocumentIntegrityStatus.Signed => "Ditandatangani",
            ClinicalDocumentIntegrityStatus.LockedUnsigned => "Terkunci, Tidak Ditandatangani",
            ClinicalDocumentIntegrityStatus.Cancelled => "Dibatalkan",
            _ => status.ToString()
        };
    }
}
