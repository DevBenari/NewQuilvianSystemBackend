using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services
{
    /// <summary>
    /// Menggabungkan tiga belas sumber dokumen klinis menjadi satu riwayat berurut waktu
    /// (RM-DEC-002, RM-CAP-004, arsitektur backend bagian 5.8).
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

        public MedicalRecordTimelineService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

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

        /// <summary>Seluruh jenis dokumen yang dapat digabungkan service ini.</summary>
        public static readonly IReadOnlyList<ClinicalDocumentKind> SeluruhJenis =
            Enum.GetValues<ClinicalDocumentKind>();

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

            var jenisDiminta = TentukanJenis(permintaan.DocumentKinds);

            // Baris yang perlu dikumpulkan agar halaman yang diminta dapat dibentuk. Kasus
            // terburuk: seluruh isi halaman berasal dari satu sumber saja, jadi setiap sumber
            // harus menyediakan sebanyak itu.
            var diperlukan = halaman * ukuranHalaman;
            var batasPerSumber = Math.Min(diperlukan, BatasBarisPerSumber);

            var hasil = new MedicalRecordTimelineResult
            {
                RequestedKinds = jenisDiminta.ToList(),

                // Halaman yang terlalu jauh tidak dapat dijamin utuh, karena setiap sumber
                // dibatasi. Keadaan itu dinyatakan, bukan didiamkan.
                IsTruncated = diperlukan > BatasBarisPerSumber
            };

            var gabungan = new List<MedicalRecordTimelineItemResponse>();
            var jumlahSeluruhnya = 0;

            foreach (var jenis in jenisDiminta)
            {
                try
                {
                    var (jumlah, baris) = await AmbilJenisAsync(
                        jenis, permintaan, batasPerSumber, cancellationToken);

                    jumlahSeluruhnya += jumlah;

                    // Menyentuh batas berarti kemungkinan masih ada dokumen lain yang tidak
                    // ikut terambil.
                    if (baris.Count >= batasPerSumber)
                        hasil.IsTruncated = true;

                    gabungan.AddRange(baris);
                }
                catch (OperationCanceledException)
                {
                    // Pembatalan permintaan bukan kegagalan sumber. Teruskan apa adanya.
                    throw;
                }
                catch (Exception ex)
                {
                    hasil.FailedSources.Add(new MedicalRecordTimelineSourceFailure
                    {
                        DocumentKind = jenis,
                        DocumentKindName = NamaJenis(jenis),
                        Message = ex.Message
                    });
                }
            }

            var terurut = permintaan.NewestFirst
                ? gabungan.OrderByDescending(x => x.OccurredAt).ThenBy(x => x.DocumentId)
                : gabungan.OrderBy(x => x.OccurredAt).ThenBy(x => x.DocumentId);

            var isiHalaman = terurut
                .Skip((halaman - 1) * ukuranHalaman)
                .Take(ukuranHalaman)
                .ToList();

            await LengkapiStatusKeutuhanAsync(permintaan.PatientId, isiHalaman, cancellationToken);

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
        /// Menentukan jenis dokumen mana yang benar-benar ditanyakan ke basis data.
        ///
        /// Daftar kosong berarti seluruh tiga belas jenis. Nilai kembar dan nilai yang tidak
        /// dikenal dibuang lebih dulu supaya tidak ada query yang terkirim dua kali.
        /// </summary>
        private static IReadOnlyList<ClinicalDocumentKind> TentukanJenis(
            IReadOnlyCollection<ClinicalDocumentKind>? diminta)
        {
            if (diminta == null || diminta.Count == 0)
                return SeluruhJenis;

            var terpilih = diminta
                .Where(x => Enum.IsDefined(x))
                .Distinct()
                .ToList();

            return terpilih;
        }

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
            CancellationToken cancellationToken)
            where TEntity : IdentityModel
        {
            var query = _dbContext.Set<TEntity>()
                .AsNoTracking()
                .Where(Bandingkan(pasien, permintaan.PatientId, Expression.Equal))
                .Where(x => !x.IsDelete);

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

            var terurut = permintaan.NewestFirst
                ? query.OrderByDescending(tanggal)
                : query.OrderBy(tanggal);

            var baris = await terurut
                .Take(batas)
                .Select(proyeksi)
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
        /// </summary>
        private async Task LengkapiStatusKeutuhanAsync(
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
                return;

            var idDokumen = isiHalaman.Select(x => x.DocumentId).Distinct().ToList();

            var keutuhan = await _dbContext.Set<TrxClinicalDocumentIntegrity>()
                .AsNoTracking()
                .Where(x => x.PatientId == patientId
                            && !x.IsDelete
                            && idDokumen.Contains(x.DocumentId))
                .Select(x => new
                {
                    x.DocumentKind,
                    x.DocumentId,
                    x.IntegrityStatus
                })
                .ToListAsync(cancellationToken);

            if (keutuhan.Count == 0)
                return;

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
