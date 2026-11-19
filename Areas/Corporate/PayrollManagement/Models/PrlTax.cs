using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models
{
    [Table("PrlTax", Schema = "public")]
    public class PrlTax : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [MaxLength(50)]
        public string? NpwpNumber { get; set; }

        [Required, MaxLength(30)]
        public string TaxStatus { get; set; } = "TK/0";

        [Required, MaxLength(30)]
        public string TaxMethod { get; set; } = "Gross";
        // Gross, GrossUp, Net.

        [Required, MaxLength(3)]
        public string TaxCountryCode { get; set; } = "ID";

        [MaxLength(50)]
        public string? TaxOfficeCode { get; set; }

        public bool IsNpwpRegistered { get; set; } = false;
        public bool IsTaxResident { get; set; } = true;
        public bool HasPreviousEmployer { get; set; } = false;
        public bool IsEmployerBorneTax { get; set; } = false;

        public decimal PreviousEmployerTaxableIncome { get; set; } = 0m;
        public decimal PreviousEmployerTaxPaid { get; set; } = 0m;
        public decimal AnnualNonTaxableIncome { get; set; } = 0m;

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        // ---- Konfigurasi PPh yang ditetapkan HRD (DEC-PRL-TAX-001/005/006). Tiga konsep terpisah dari TaxMethod (Gross/GrossUp/Net) dan
        // TaxStatus (PTKP). Kolom nullable: kosong berarti BELUM dikonfigurasi, bukan "nol yang sah". Nilai dijaga PayrollTaxInsuranceRules. ----

        /// <summary>Jenis PPh: PPH21 atau PPH26 (TaxArticles).</summary>
        [MaxLength(20)]
        public string? TaxArticle { get; set; }

        /// <summary>Kategori penerima penghasilan (TaxRecipientCategories), ditentukan HRD dari status hubungan kerja aktual.</summary>
        [MaxLength(40)]
        public string? RecipientTaxCategory { get; set; }

        /// <summary>Skema hitung (TaxCalculationModes): AUTO_REGULATORY, APPROVED_MANUAL_AMOUNT, atau EXEMPT_OR_NOT_APPLICABLE.</summary>
        [MaxLength(40)]
        public string? CalculationMode { get; set; }

        /// <summary>Referensi kebijakan yang menjadi dasar penetapan (mis. nomor peraturan atau keputusan); dicatat pada snapshot hitung.</summary>
        [MaxLength(100)]
        public string? PolicyVersion { get; set; }

        /// <summary>Alasan pengecualian; wajib untuk skema EXEMPT_OR_NOT_APPLICABLE.</summary>
        [MaxLength(500)]
        public string? ExemptionReason { get; set; }

        /// <summary>Dasar penetapan nominal manual atau dasar aturan pengecualian (nomor/referensi persetujuan).</summary>
        [MaxLength(1000)]
        public string? DecisionBasis { get; set; }

        /// <summary>Nominal PPh bulanan yang disetujui; hanya untuk skema APPROVED_MANUAL_AMOUNT. Nominal sensitif: dibatasi izin ViewAmount.</summary>
        public decimal? ApprovedManualAmount { get; set; }

        // ---- Persetujuan tersendiri atas nominal manual (DEC-PRL-TAX-009). Menetapkan/melihat nominal (Update, ViewAmount) bukan
        // persetujuan: nominal manual dipakai mesin hanya bila disetujui pengguna lain yang memegang izin ApproveManualAmount. Setiap
        // perubahan nominal atau dasar mengembalikan status ke PENDING. ----

        /// <summary>PENDING | APPROVED | REJECTED (<see cref="Constants.ManualAmountApprovalStatuses"/>); null bila skema bukan nominal manual.</summary>
        [MaxLength(20)]
        public string? ManualAmountApprovalStatus { get; set; }

        public DateTime? ManualAmountSubmittedAt { get; set; }
        public Guid? ManualAmountSubmittedByUserId { get; set; }

        public DateTime? ManualAmountDecidedAt { get; set; }
        public Guid? ManualAmountDecidedByUserId { get; set; }

        /// <summary>Alasan penolakan atau catatan keputusan; wajib saat menolak.</summary>
        [MaxLength(500)]
        public string? ManualAmountDecisionNote { get; set; }

        // ---- PPh Pasal 26 / P3B: tarif perjanjian hanya dipakai bila dasarnya (dokumen/ketentuan P3B) tercatat. ----

        /// <summary>Tarif P3B (persen) pengganti tarif PPh 26 umum; wajib disertai <see cref="TreatyBasis"/>.</summary>
        [Column(TypeName = "decimal(9,4)")]
        public decimal? TreatyRatePercent { get; set; }

        /// <summary>Dasar P3B (negara mitra, nomor dokumen/SKD yang berlaku).</summary>
        [MaxLength(500)]
        public string? TreatyBasis { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        [NotMapped]
        public string? TaxIdentificationNumber
        {
            get => NpwpNumber;
            set => NpwpNumber = value;
        }

        [NotMapped]
        public string PtkpStatus
        {
            get => TaxStatus;
            set => TaxStatus = value;
        }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
    }
}
