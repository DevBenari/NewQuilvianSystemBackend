using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs
{
    // =========================================================================
    // Penilaian kelengkapan berkas per kunjungan dan daftar kerja (`BE-39`, kontrak API 0.2.0 A2).
    //
    // Penilaian adalah KESIMPULAN rekam medis sendiri: keadaan utama dan rincian kekurangan.
    // Tidak memuat isi klinis dokumen mana pun (`RM-DEC-038`, invariant 3 `AGG-RM-03`).
    // =========================================================================

    /// <summary>Penyaring daftar kerja dan ringkasan.</summary>
    public class MedicalRecordCompletenessPagedQuery
    {
        /// <summary>Kosong = seluruh keadaan selain <c>Complete</c> (bawaan daftar kerja).</summary>
        public CompletenessState? State { get; set; }

        public EncounterType? EncounterType { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public CompletenessDeficiencyType? DeficiencyType { get; set; }

        /// <summary>Batas awal waktu tutup klinis (UTC).</summary>
        public DateTime? ClosedFrom { get; set; }

        /// <summary>Batas akhir waktu tutup klinis (UTC).</summary>
        public DateTime? ClosedTo { get; set; }

        /// <summary>Nomor rekam medis, nama pasien, atau nomor kunjungan.</summary>
        public string? Search { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    /// <summary>Satu kekurangan berkas (`DC-RM-08`). Seluruh kekurangan ditampilkan (`RM-FE-023`).</summary>
    public class MedicalRecordCompletenessDeficiencyResponse
    {
        public CompletenessDeficiencyType Type { get; set; }

        public string TypeLabel { get; set; } = string.Empty;

        /// <summary>Kalimat untuk petugas, mis. "Ringkasan pulang belum ada".</summary>
        public string Label { get; set; } = string.Empty;

        public ClinicalDocumentKind? DocumentKind { get; set; }

        public Guid? DocumentId { get; set; }

        /// <summary>Penanggung jawab tindak lanjut, mis. penulis catatan yang belum ditandatangani.</summary>
        public string? ResponsibleUserName { get; set; }
    }

    /// <summary>Satu baris daftar kerja.</summary>
    public class MedicalRecordCompletenessListItemResponse
    {
        public Guid EncounterId { get; set; }

        public string EncounterNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public string? PatientName { get; set; }

        public EncounterType EncounterType { get; set; }

        public string? ServiceUnitName { get; set; }

        public DateTime ClinicalClosedAt { get; set; }

        public CompletenessState State { get; set; }

        public string StateLabel { get; set; } = string.Empty;

        public int DeficiencyCount { get; set; }

        public DateTime EvaluatedAt { get; set; }
    }

    /// <summary>Penilaian satu kunjungan beserta seluruh kekurangannya.</summary>
    public class MedicalRecordCompletenessDetailResponse
    {
        public Guid EncounterId { get; set; }

        public string EncounterNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public string? PatientName { get; set; }

        public EncounterType EncounterType { get; set; }

        public string? ServiceUnitName { get; set; }

        public DateTime ClinicalClosedAt { get; set; }

        /// <summary>Benar bila waktu tutup diturunkan dari data lama (kunjungan sebelum rilis).</summary>
        public bool IsClinicalClosureDerived { get; set; }

        public CompletenessState State { get; set; }

        public string StateLabel { get; set; } = string.Empty;

        public List<MedicalRecordCompletenessDeficiencyResponse> Deficiencies { get; set; } = new();

        public DateTime EvaluatedAt { get; set; }

        public DateTime? FirstCompletedAt { get; set; }

        /// <summary>
        /// Salah bila penilaian tidak disimpan: kunjungan ditutup sebelum fitur dirilis (`RM-DEC-050`)
        /// sehingga tidak masuk daftar kerja.
        /// </summary>
        public bool IsPersisted { get; set; }

        /// <summary>Kode profil dokumen wajib yang dipakai, mis. <c>RJ</c>, <c>K-OK</c>.</summary>
        public List<string> ProfileCodes { get; set; } = new();
    }

    public class MedicalRecordCompletenessSummaryResponse
    {
        public int Total { get; set; }
        public int Complete { get; set; }
        public int Incomplete { get; set; }
        public int AwaitingResult { get; set; }
        public int Undetermined { get; set; }
    }

    public class MedicalRecordCompletenessFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";

        /// <summary>Kosong = belum <c>Complete</c>.</summary>
        public CompletenessState? DefaultState { get; set; }

        public List<MedicalRecordCompletenessOptionResponse> StateOptions { get; set; } = new();
        public List<MedicalRecordCompletenessOptionResponse> EncounterTypeOptions { get; set; } = new();
        public List<MedicalRecordCompletenessOptionResponse> DeficiencyTypeOptions { get; set; } = new();

        /// <summary>Pilihan unit layanan diambil dari master unit layanan, bukan dari sini.</summary>
        public string ServiceUnitOptionsSource { get; set; } = "/api/v1/health-services/master-data/service-units/options";

        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class MedicalRecordCompletenessOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
