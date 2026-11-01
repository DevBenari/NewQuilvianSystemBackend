using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models
{
    /// <summary>Alasan layanan belum dapat dihitung jasanya — data-dictionary.md bagian 8.</summary>
    public static class MdfUnresolvedReasons
    {
        public const string PerformerMissing = "PERFORMER_MISSING";
        public const string AgreementMissing = "AGREEMENT_MISSING";
        public const string RuleMissing = "RULE_MISSING";
        public const string ContractExpired = "CONTRACT_EXPIRED";
        public const string RoleUnmapped = "ROLE_UNMAPPED";
        public const string SourceUnsupported = "SOURCE_UNSUPPORTED";
    }

    /// <summary>Status penyelesaian layanan unresolved — data-dictionary.md bagian 8.</summary>
    public static class MdfUnresolvedStatuses
    {
        public const string Open = "Open";
        public const string Resolved = "Resolved";
        public const string Waived = "Waived";
    }

    /// <summary>
    /// Daftar layanan medis yang belum dapat dihitung jasanya beserta alasannya — MDF-DES-014, MF-DEC-018.
    /// Berdiri sendiri (bukan baris hasil jasa tertahan) agar tidak membutuhkan penerima yang belum ada.
    /// Periode tidak dapat ditutup bila masih ada layanan unresolved berstatus Open (MDF-DES-016).
    /// </summary>
    [Table("MdfUnresolvedService", Schema = "public")]
    public class MdfUnresolvedService : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>FK ke periode kalkulasi (MdfFeePeriod.Id), DeleteBehavior.Cascade.</summary>
        [Required]
        public Guid PeriodId { get; set; }

        /// <summary>Domain modul sumber layanan: OPERATING_ROOM, PROCEDURE, LABORATORY, ADHOC, ADHOC_CATALOG.</summary>
        [Required]
        [MaxLength(30)]
        public string SourceDomain { get; set; } = MdfServiceSourceDomains.OperatingRoom;

        /// <summary>ID referensi baris pada modul sumber layanan.</summary>
        [Required]
        [MaxLength(100)]
        public string SourceDetailId { get; set; } = string.Empty;

        /// <summary>Rujukan ke baris tagihan pasien (BilInvoiceItem.Id). Tanpa FK database.</summary>
        public Guid? InvoiceItemId { get; set; }

        /// <summary>FK ke tarif layanan (MstTariff.Id), DeleteBehavior.Restrict. Null jika non-katalog.</summary>
        public Guid? TariffId { get; set; }

        /// <summary>Tanggal pelaksanaan tindakan layanan medis.</summary>
        [Required]
        public DateOnly ServiceDate { get; set; }

        /// <summary>Nilai dasar kotor baris tagihan yang seharusnya dihitung (bila sudah teridentifikasi).</summary>
        public decimal? BaseAmount { get; set; }

        /// <summary>Kode alasan belum dapat dihitung (MdfUnresolvedReasons).</summary>
        [Required]
        [MaxLength(30)]
        public string Reason { get; set; } = MdfUnresolvedReasons.PerformerMissing;

        /// <summary>Penjelasan detail alasan untuk membantu tindak lanjut petugas pengelola jasa.</summary>
        [MaxLength(500)]
        public string? ReasonDetail { get; set; }

        /// <summary>Waktu masalah pertama kali terdeteksi oleh mesin kalkulasi.</summary>
        [Required]
        public DateTime DetectedAt { get; set; }

        /// <summary>Status penyelesaian: Open, Resolved, Waived (MdfUnresolvedStatuses).</summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = MdfUnresolvedStatuses.Open;

        /// <summary>User ID petugas yang menyelesaikan atau mengesampingkan masalah.</summary>
        public Guid? ResolvedBy { get; set; }

        /// <summary>Waktu penyelesaian dicatat.</summary>
        public DateTime? ResolvedAt { get; set; }

        /// <summary>Catatan penyelesaian (wajib diisi apabila Status = Waived).</summary>
        [MaxLength(500)]
        public string? ResolutionNote { get; set; }

        public MdfFeePeriod? Period { get; set; }
        public MstTariff? Tariff { get; set; }
    }
}

