using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs
{
    /// <summary>
    /// Kunjungan pasien yang dapat dibooking — "No. RM (kunjungan hari ini)" Booking Radiologi v1.
    /// </summary>
    public class RadBookingEncounterResponse
    {
        public Guid EncounterId { get; set; }

        public DateTime EncounterDate { get; set; }

        /// <summary>Konteks pasien dengan bentuk yang sama dengan baris Daftar Pasien.</summary>
        public RadOrderPatientContextResponse Patient { get; set; } = new();

        /// <summary>No. Handphone v1 — WhatsApp bila ada, selain itu telepon pasien.</summary>
        public string? PhoneNumber { get; set; }

        /// <summary>Dokter DPJP kunjungan — "Nama Dokter DPJP" v1, dokter perujuk booking.</summary>
        public Guid? DoctorId { get; set; }

        public string? DoctorName { get; set; }

        /// <summary>Diagnosa kunjungan, diagnosa utama lebih dulu.</summary>
        public string? DiagnosisText { get; set; }
    }

    /// <summary>
    /// Harga tampilan satu pemeriksaan untuk kelas kunjungan, beserta alat pencitraan yang diturunkan
    /// dari pemeriksaannya — kolom Harga tabel pemeriksaan Booking v1.
    /// </summary>
    public class RadBookingPriceResponse
    {
        public Guid ProcedureId { get; set; }

        /// <summary>Kosong bila tarif belum diatur atau ambigu — tidak pernah Rp0 diam-diam.</summary>
        public decimal? TariffAmount { get; set; }

        /// <summary>Alat yang diturunkan dari kategori/nama pemeriksaan; kosong bila tidak dapat ditentukan.</summary>
        public Guid? ModalityId { get; set; }

        public string? ModalityCode { get; set; }

        public string? ModalityName { get; set; }

        // --- Tambahan (aditif; pemanggil lama yang hanya membaca TariffAmount tidak terpengaruh) ---

        /// <summary><see cref="RadTariffStatuses"/>.</summary>
        public string TariffStatus { get; set; } = RadTariffStatuses.NotFound;

        /// <summary>Alasan yang dapat dibaca kasir bila tarif tidak sah; kosong bila <c>RESOLVED</c>.</summary>
        public string? TariffMessage { get; set; }

        public Guid? TariffId { get; set; }

        public string? TariffCode { get; set; }

        /// <summary>Nama tarif (<c>MstTariff.TariffName</c>) — nama pemeriksaan yang ditampilkan.</summary>
        public string? TariffName { get; set; }

        /// <summary>Kelas tarif terpilih; kosong untuk tarif umum (tanpa kelas).</summary>
        public Guid? TariffPatientClassId { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        /// <summary>Kelas yang dipakai menghitung harga ini (kelas kunjungan, atau kelas pratinjau yang dipilih).</summary>
        public Guid? PriceBasisPatientClassId { get; set; }

        public string? PriceBasisPatientClassName { get; set; }

        /// <summary>Kelas yang tercatat pada kunjungan — tidak pernah diubah oleh pratinjau.</summary>
        public Guid? EncounterPatientClassId { get; set; }

        public string? EncounterPatientClassName { get; set; }

        /// <summary>
        /// <c>true</c> bila kelas pratinjau berbeda dari kelas kunjungan. Harga ini hanya pratinjau: Billing tetap
        /// menagih menurut kelas kunjungan.
        /// </summary>
        public bool IsPreviewClass { get; set; }
    }

    /// <summary>Status pemilihan tarif Radiologi untuk satu pemeriksaan dan kelas.</summary>
    public static class RadTariffStatuses
    {
        /// <summary>Tepat satu tarif sah.</summary>
        public const string Resolved = "RESOLVED";

        /// <summary>Tidak ada tarif yang berlaku untuk kelas dan tanggal ini.</summary>
        public const string NotFound = "NOT_FOUND";

        /// <summary>Beberapa tarif sama spesifik dengan harga berbeda — konflik master tarif.</summary>
        public const string Ambiguous = "AMBIGUOUS";

        /// <summary>Pemeriksaan bukan pemeriksaan radiologi aktif.</summary>
        public const string ProcedureInvalid = "PROCEDURE_INVALID";

        /// <summary>Kunjungan belum dipilih sehingga kelas dan klinik tidak dapat ditentukan.</summary>
        public const string EncounterRequired = "ENCOUNTER_REQUIRED";

        /// <summary>Tarif radiologi yang <c>ProcedureId</c>-nya kosong: belum dapat dipesan.</summary>
        public const string ProcedureNotMapped = "PROCEDURE_NOT_MAPPED";

        /// <summary>Pemeriksaan bertarif radiologi tetapi belum bertanda <c>IsRadiology</c>: belum dapat dipesan.</summary>
        public const string ProcedureNotRadiology = "PROCEDURE_NOT_RADIOLOGY";
    }

    /// <summary>
    /// Penyaring pilihan pemeriksaan Booking — pencarian nama/kode serta konteks kunjungan dan kelas.
    /// </summary>
    public class RadBookingProcedureOptionQuery : RadAssigneeOptionQuery
    {
        /// <summary>Kunjungan yang dibooking. Menentukan kelas bawaan dan klinik; wajib agar tarif dapat dinilai.</summary>
        public Guid? EncounterId { get; set; }

        /// <summary>
        /// Kelas pratinjau. Kosong = kelas kunjungan. Hanya memengaruhi harga yang ditampilkan; kelas kunjungan
        /// tidak pernah diubah.
        /// </summary>
        public Guid? PatientClassId { get; set; }
    }

    /// <summary>
    /// Satu baris pilihan "Nama Pemeriksaan" — SATU baris per pemeriksaan, berapa pun variasi tarif kelasnya.
    /// Harga adalah tarif sah untuk kelas yang dipakai; tarif kelas lain tidak muncul.
    /// </summary>
    public class RadBookingProcedureOptionResponse
    {
        /// <summary>Id pemeriksaan (<c>MstProcedure.Id</c>) — nilai yang dikirim saat booking.</summary>
        public Guid Id { get; set; }

        public string ProcedureCode { get; set; } = string.Empty;

        public string ProcedureName { get; set; } = string.Empty;

        public string? ProcedureCategoryName { get; set; }

        public string? ExternalProcedureCode { get; set; }

        /// <summary>Nama tampilan: <c>MstTariff.TariffName</c> bila tarif sah, selain itu nama pemeriksaan.</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary><see cref="RadTariffStatuses"/>.</summary>
        public string TariffStatus { get; set; } = RadTariffStatuses.NotFound;

        /// <summary>Dapat dipilih hanya bila tarif sah (<c>RESOLVED</c>).</summary>
        public bool IsSelectable { get; set; }

        /// <summary>Alasan pemeriksaan tersedia tetapi tidak dapat dipilih.</summary>
        public string? UnavailableReason { get; set; }

        public decimal? TariffAmount { get; set; }

        public Guid? TariffId { get; set; }

        public string? TariffCode { get; set; }

        public string? TariffName { get; set; }

        /// <summary>Kelas tarif terpilih; kosong untuk tarif umum.</summary>
        public Guid? TariffPatientClassId { get; set; }

        public string? TariffPatientClassName { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }
    }

    /// <summary>Pilihan "Kelas Pasien" Booking — dari <c>MstPatientClass</c>.</summary>
    public class RadBookingClassOptionResponse
    {
        public Guid Id { get; set; }

        public string PatientClassCode { get; set; } = string.Empty;

        public string PatientClassName { get; set; } = string.Empty;

        /// <summary>Kelas yang tercatat pada kunjungan (kelas bawaan filter).</summary>
        public bool IsEncounterClass { get; set; }

        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Pilihan "Pilih Diagnosa ICD-10" Booking v1 — diagnosis klinis ICD-10 dari master
    /// <c>MstDiagnosis</c>. Hanya ruas yang perlu tampil; pilihan mengisi Diagnosa Awal.
    /// </summary>
    public class RadBookingDiagnosisOptionResponse
    {
        public Guid Id { get; set; }

        public string DiagnosisCode { get; set; } = string.Empty;

        public string DiagnosisName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Booking Radiologi v1 — satu booking dengan beberapa pemeriksaan untuk satu kunjungan.
    /// Di V2 tiap pemeriksaan menjadi satu pesanan; seluruhnya dibuat dalam satu transaksi.
    /// </summary>
    public class RadBookingRequest
    {
        [Required]
        public Guid EncounterId { get; set; }

        /// <summary>Tanggal + jam order v1. Kosong = sekarang.</summary>
        public DateTime? OrderedAt { get; set; }

        /// <summary>
        /// Tanggal + jam pemeriksaan v1. Hari lain dari tanggal order menjadi jadwal (tab Pasien
        /// Perjanjian); kosong atau hari yang sama = diperiksa pada hari order.
        /// </summary>
        public DateTime? ExaminationAt { get; set; }

        /// <summary>Diagnosa awal dan keterangan v1, disimpan sebagai indikasi klinis.</summary>
        [MaxLength(1000)]
        public string? ClinicalIndication { get; set; }

        /// <summary>Pasien perlu persiapan — tab Pasien Persiapan v1.</summary>
        public bool? RequiresPreparation { get; set; }

        [Required, MinLength(1), MaxLength(20)]
        public List<RadBookingItemRequest> Items { get; set; } = new();
    }

    public class RadBookingItemRequest
    {
        [Required]
        public Guid ProcedureId { get; set; }

        /// <summary>Alat pencitraan. Kosong = diturunkan dari kategori/nama pemeriksaan.</summary>
        public Guid? ModalityId { get; set; }

        public bool IsUrgent { get; set; }

        /// <summary>Dokter Pemeriksa baris v1.</summary>
        public Guid? ExaminingDoctorId { get; set; }
    }

    public class RadBookingResponse
    {
        public List<Guid> OrderIds { get; set; } = new();

        public List<string> OrderNumbers { get; set; } = new();
    }
}
