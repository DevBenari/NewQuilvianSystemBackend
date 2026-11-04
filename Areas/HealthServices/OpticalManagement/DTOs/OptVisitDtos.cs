using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs
{
    // OPT-API-v1 grup "Health Services / Optical Management / Opt Visit" (BE-OPT-010).

    /// <summary>
    /// Pendaftaran kunjungan Optik. Kunjungannya dibuat Registrasi lewat jalur unit penunjang (<c>INT-OPT-01</c>);
    /// kunci idempoten dikirim lewat header <c>Idempotency-Key</c>, bukan isian ini.
    /// </summary>
    public class OptVisitRegisterRequest
    {
        /// <summary>Pasien wajib sudah terdaftar di modul Pasien.</summary>
        [Required]
        public Guid? PatientId { get; set; }

        /// <summary>
        /// Cara bayar. <c>CompanyGuarantor</c> saat ini ditolak Registrasi untuk kunjungan penunjang (hanya MCU);
        /// pasien perusahaan menunggu perluasan jalur Registrasi.
        /// </summary>
        public EncounterPaymentType PaymentType { get; set; } = EncounterPaymentType.Cash;

        /// <summary>Diisi hanya bila cara bayar tunai.</summary>
        public Guid? PaymentMethodId { get; set; }

        /// <summary>Wajib bila cara bayar asuransi; kartu milik pasien yang sama.</summary>
        public Guid? PatientInsuranceId { get; set; }

        /// <summary>Kartu penjamin perusahaan milik pasien (lihat catatan <see cref="PaymentType"/>).</summary>
        public Guid? PatientCompanyGuarantorId { get; set; }

        /// <summary>Benar bila pasien datang dengan rujukan.</summary>
        public bool IsReferral { get; set; }

        [MaxLength(250)]
        public string? ReferralNumber { get; set; }

        /// <summary>Instansi perujuk (penunjuk master, bukan nama).</summary>
        public Guid? ReferralInstitutionId { get; set; }

        /// <summary>Dokter perujuk, contoh dr. Sp.M Poli Mata; wajib berpraktik pada instansi perujuk.</summary>
        public Guid? ReferralDoctorId { get; set; }

        /// <summary>
        /// Kunjungan Poli Mata asal rujukan (<c>OPT-DEC-037</c>). <b>Belum tersedia</b> sampai <c>BE-OPT-003</c>
        /// menambah kolom tujuannya di Registrasi; bila diisi sekarang permintaan ditolak, bukan dibuang diam-diam.
        /// </summary>
        public Guid? ReferralSourceEncounterId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>Penyaring daftar kunjungan Optik.</summary>
    public class OptVisitListQuery
    {
        /// <summary>Tanggal kunjungan WIB. Kosong = semua tanggal.</summary>
        public DateOnly? Date { get; set; }

        /// <summary>Benar = hanya kunjungan yang belum selesai, batal, atau tidak hadir.</summary>
        public bool? OpenOnly { get; set; }

        /// <summary>Benar = hanya kunjungan yang masih punya pesanan Optik belum selesai.</summary>
        public bool? HasOpenOrder { get; set; }

        /// <summary>Mencari nomor kunjungan, nomor RM, atau nama pasien.</summary>
        public string? Search { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>Satu kunjungan Optik pada daftar.</summary>
    public class OptVisitListItemResponse
    {
        public Guid EncounterId { get; set; }
        public string EncounterNumber { get; set; } = string.Empty;
        public DateTime EncounterDate { get; set; }
        public EncounterStatus EncounterStatus { get; set; }
        public string EncounterStatusLabel { get; set; } = string.Empty;
        public EncounterPaymentType PaymentType { get; set; }
        public Guid PatientId { get; set; }
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsReferral { get; set; }
        public int ExamCount { get; set; }
        public int PrescriptionCount { get; set; }
        public int OpenOrderCount { get; set; }

        /// <summary>Benar bila jawaban ini berasal dari kunci idempoten yang sama (tombol Simpan ditekan dua kali).</summary>
        public bool IsReplay { get; set; }
    }

    /// <summary>Ringkasan satu kunjungan Optik.</summary>
    public class OptVisitDetailResponse
    {
        public OptVisitListItemResponse Visit { get; set; } = new();
        public List<OptVisitDocumentItem> RefractionExams { get; set; } = new();
        public List<OptVisitDocumentItem> Prescriptions { get; set; } = new();
        public List<OptVisitDocumentItem> Orders { get; set; } = new();
        public List<OptVisitDocumentItem> WarrantyClaims { get; set; } = new();
    }

    /// <summary>Satu dokumen Optik pada ringkasan kunjungan.</summary>
    public class OptVisitDocumentItem
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public int Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
