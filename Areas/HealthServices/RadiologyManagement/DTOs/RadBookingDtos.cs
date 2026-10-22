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

        /// <summary>Kosong bila tarif belum diatur — tidak pernah Rp0 diam-diam.</summary>
        public decimal? TariffAmount { get; set; }

        /// <summary>Alat yang diturunkan dari kategori/nama pemeriksaan; kosong bila tidak dapat ditentukan.</summary>
        public Guid? ModalityId { get; set; }

        public string? ModalityCode { get; set; }

        public string? ModalityName { get; set; }
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
