using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs
{
    // =========================================================================
    // Kontrak baca antar modul untuk berkas rekam medis (`INT-RM-10`, `INT-RM-11`).
    //
    // DTO di berkas ini BUKAN kontrak HTTP. Ia dipertukarkan di dalam proses antara rekam
    // medis dan modul pemilik dokumen, sehingga tidak pernah tampil di Swagger. Yang tampil ke
    // klien tetap MedicalRecordTimelineItemResponse.
    // =========================================================================

    /// <summary>Permintaan rekam medis kepada satu pembaca sumber.</summary>
    public class MedicalRecordSourceQuery
    {
        public Guid PatientId { get; set; }

        /// <summary>Jenis yang diminta, selalu himpunan bagian dari jenis milik pembaca itu.</summary>
        public IReadOnlyCollection<ClinicalDocumentKind> Kinds { get; set; } = [];

        public Guid? EncounterId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool NewestFirst { get; set; } = true;

        /// <summary>Jumlah baris maksimal per jenis. Nol berarti hanya menghitung.</summary>
        public int Limit { get; set; }
    }

    /// <summary>Jawaban satu pembaca sumber.</summary>
    public class MedicalRecordSourcePage
    {
        /// <summary>Jumlah dokumen final yang cocok, per jenis.</summary>
        public Dictionary<ClinicalDocumentKind, int> Totals { get; set; } = new();

        public List<MedicalRecordFinalDocumentDescriptor> Items { get; set; } = new();
    }

    /// <summary>
    /// Satu dokumen FINAL milik modul lain (`RM-DEC-041`). Pembaca dilarang mengembalikan
    /// dokumen yang belum final menurut modulnya sendiri.
    /// </summary>
    public class MedicalRecordFinalDocumentDescriptor
    {
        public ClinicalDocumentKind Kind { get; set; }

        /// <summary>Id baris pada tabel asalnya di modul pemilik.</summary>
        public Guid DocumentId { get; set; }

        /// <summary>Boleh kosong, misalnya golongan darah yang tidak terikat kunjungan.</summary>
        public Guid? EncounterId { get; set; }

        /// <summary>Saat dokumen menjadi final menurut pemiliknya. Wajib (`RM-DEC-043`).</summary>
        public DateTime FinalizedAt { get; set; }

        /// <summary>
        /// Waktu kejadian klinis bila berbeda dari waktu final, misalnya waktu pengambilan
        /// sampel. Kosong berarti linimasa mengurutkan memakai <see cref="FinalizedAt"/>.
        /// </summary>
        public DateTime? OccurredAt { get; set; }

        public string? DocumentNumber { get; set; }

        public string? Title { get; set; }

        /// <summary>Paling banyak 300 karakter. Bukan isi dokumen penuh.</summary>
        public string? Summary { get; set; }

        /// <summary>Pernah diralat lewat mekanisme modul pemilik (`RM-DEC-042`).</summary>
        public bool IsCorrected { get; set; }

        /// <summary>Isinya dapat berubah tanpa riwayat (`RM-DEC-059`).</summary>
        public bool IsProvisional { get; set; }

        public Guid? AuthorUserId { get; set; }

        /// <summary>Hanya pada ringkasan pencatatan berkala (`RM-DEC-052`); kosong pada dokumen lain.</summary>
        public MedicalRecordPeriodicSummaryResponse? PeriodicSummary { get; set; }
    }

    /// <summary>
    /// Isi satu dokumen final untuk layar rincian. Disajikan sebagai pasangan label dan nilai,
    /// sama seperti tiga belas jenis bawaan, supaya layar tidak perlu mengenali bentuk data
    /// setiap modul. Dibaca saat diminta dan tidak disimpan rekam medis (`RM-DEC-038`).
    /// </summary>
    public class MedicalRecordSourceDocumentDetail
    {
        public Guid? EncounterId { get; set; }

        public string? DocumentNumber { get; set; }

        public string? Title { get; set; }

        public DateTime FinalizedAt { get; set; }

        public DateTime? OccurredAt { get; set; }

        public bool IsCorrected { get; set; }

        public bool IsProvisional { get; set; }

        public List<MedicalRecordDocumentSectionResponse> Sections { get; set; } = new();
    }

    /// <summary>Permintaan rekam medis kepada satu pembaca uji kelengkapan.</summary>
    public class MedicalRecordCompletenessProbeQuery
    {
        public Guid EncounterId { get; set; }

        public Guid PatientId { get; set; }

        /// <summary>Kriteria yang dituntut profil kunjungan dan dijawab pembaca ini.</summary>
        public IReadOnlyCollection<CompletenessCriterion> Criteria { get; set; } = [];
    }

    /// <summary>Jawaban satu pembaca uji kelengkapan.</summary>
    public class MedicalRecordCompletenessProbeResult
    {
        public HashSet<CompletenessCriterion> SatisfiedCriteria { get; set; } = new();

        public HashSet<CompletenessCondition> PresentConditions { get; set; } = new();

        /// <summary>Pemeriksaan yang dipesan tetapi belum final — syarat 2 `RM-DEC-045`.</summary>
        public List<MedicalRecordPendingResult> PendingResults { get; set; } = new();
    }

    public class MedicalRecordPendingResult
    {
        public ClinicalDocumentKind Kind { get; set; }

        public Guid DocumentId { get; set; }

        /// <summary>Nama pemeriksaan untuk petugas, mis. "Kultur darah". Tanpa nilai hasil.</summary>
        public string? Name { get; set; }
    }
}
