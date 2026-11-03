using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models
{
    /// <summary>Status penyerahan hasil jasa medis ke Finance AP — data-dictionary.md bagian 9.</summary>
    public static class MdfFinanceHandoffStatuses
    {
        public const string Created = "Created";
        public const string Acknowledged = "Acknowledged";
        public const string Failed = "Failed";
    }

    /// <summary>
    /// Baris penyerahan hasil jasa medis ke modul Finance AP (aggregate / handoff entity) — MDF-DES-018, MF-DEC-005, MF-DEC-008.
    /// Ditulis di dalam transaksi yang sama dengan persetujuan hasil jasa (T-71, T-78).
    /// Empat kolom disalin dari hasil jasa (PayeeType, PayeeReferenceId, PeriodCode, GrossAmount)
    /// agar modul Finance dapat membaca baris handoff tanpa menyentuh tabel Medical Fee mana pun (Rule 7.1).
    /// HandoffKey bersifat unik dan tetap sama saat pengiriman ulang (Rule 7.7, T-76).
    /// </summary>
    [Table("MdfFinanceHandoff", Schema = "public")]
    public class MdfFinanceHandoff : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>FK ke MdfServiceFee.Id, DeleteBehavior.Restrict. Unik satu hasil jasa satu penyerahan (MDF-VAL-1.0 §7.5, T-73).</summary>
        [Required]
        public Guid ServiceFeeId { get; set; }

        /// <summary>Kunci idempoten untuk Finance. Unik dan persisten (MDF-VAL-1.0 §7.6, T-76).</summary>
        [Required]
        public Guid HandoffKey { get; set; } = Guid.NewGuid();

        /// <summary>Jenis penerima: Doctor atau Workforce (MdfPayeeTypes). Disalin dari hasil jasa.</summary>
        [Required]
        [MaxLength(30)]
        public string PayeeType { get; set; } = string.Empty;

        /// <summary>Rujukan polimorfik ke MstDoctor atau MstWorkforceProfile. Disalin dari hasil jasa.</summary>
        [Required]
        public Guid PayeeReferenceId { get; set; }

        /// <summary>Kode periode kalkulasi (contoh: '2026-10'). Disalin agar Finance tidak perlu join (MDF-DES-018).</summary>
        [Required]
        [MaxLength(20)]
        public string PeriodCode { get; set; } = string.Empty;

        /// <summary>Nilai kotor jasa medis sebelum potongan PPh 21 atau kasbon (MdfServiceFee.FinalAmount, MF-DEC-005, T-72).</summary>
        [Required]
        public decimal GrossAmount { get; set; }

        /// <summary>Status penyerahan: Created, Acknowledged, Failed (MdfFinanceHandoffStatuses).</summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = MdfFinanceHandoffStatuses.Created;

        /// <summary>ID korelasi untuk penelusuran transaksi lintas modul.</summary>
        [Required]
        public Guid CorrelationId { get; set; } = Guid.NewGuid();

        /// <summary>ID kausasi pemicu penyerahan (misal: PeriodId saat disetujui).</summary>
        public Guid? CausationId { get; set; }

        /// <summary>Waktu konfirmasi penerimaan (ACK) oleh modul Finance AP.</summary>
        public DateTime? AcknowledgedAt { get; set; }

        /// <summary>Alasan penolakan dari Finance jika status Failed (MDF-VAL-1.0 §7.9, T-77).</summary>
        [MaxLength(500)]
        public string? FailureReason { get; set; }

        /// <summary>Token konkurensi optimistik (MDF-DES-005).</summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        /// <summary>Relasi navigasi ke hasil jasa medis yang diserahkan.</summary>
        public MdfServiceFee? ServiceFee { get; set; }
    }
}

