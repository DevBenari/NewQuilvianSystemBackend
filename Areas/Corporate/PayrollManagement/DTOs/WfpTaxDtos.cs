using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs
{
    public class WfpTaxSummaryResponse
    {
        public int TotalTaxProfile { get; set; }
        public int ActiveTaxProfile { get; set; }
        public int InactiveTaxProfile { get; set; }
        public int NpwpRegisteredProfile { get; set; }
        public int TaxResidentProfile { get; set; }
        public int PreviousEmployerProfile { get; set; }
        public int EmployerBorneTaxProfile { get; set; }

        /// <summary>Profil yang jenis PPh, kategori, dan skema hitungnya sudah ditetapkan HRD dan berlaku saat ini.</summary>
        public int ConfiguredProfile { get; set; }

        /// <summary>Profil yang belum lengkap, belum/tidak berlaku, atau tidak sah. Bukan "nol": belum ada keputusan HRD yang dapat dihitung.</summary>
        public int NotConfiguredProfile { get; set; }
    }

    public class WfpTaxResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public string? NpwpNumber { get; set; }
        public string TaxStatus { get; set; } = string.Empty;
        public string TaxMethod { get; set; } = string.Empty;
        public string TaxCountryCode { get; set; } = string.Empty;
        public string? TaxOfficeCode { get; set; }
        public bool IsNpwpRegistered { get; set; }
        public bool IsTaxResident { get; set; }
        public bool HasPreviousEmployer { get; set; }
        public bool IsEmployerBorneTax { get; set; }
        public decimal PreviousEmployerTaxableIncome { get; set; }
        public decimal PreviousEmployerTaxPaid { get; set; }
        public decimal AnnualNonTaxableIncome { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        // ---- Konfigurasi PPh HRD (additive). Jenis PPh, kategori penerima, metode pajak (TaxMethod di atas), dan skema hitung adalah
        // empat konsep berbeda. Kosong berarti belum dikonfigurasi. ----
        public string? TaxArticle { get; set; }
        public string? RecipientTaxCategory { get; set; }
        public string? CalculationMode { get; set; }
        public string? PolicyVersion { get; set; }
        public string? ExemptionReason { get; set; }
        public string? DecisionBasis { get; set; }

        /// <summary>True bila pemanggil boleh melihat nominal (izin ViewAmount); bila false, <see cref="ApprovedManualAmount"/> null.</summary>
        public bool AmountsVisible { get; set; }

        /// <summary>Ada nominal manual yang disetujui. Tidak membuka nominalnya.</summary>
        public bool HasApprovedManualAmount { get; set; }

        /// <summary>Null bila pemanggil tidak memiliki izin ViewAmount (tidak pernah diganti 0).</summary>
        public decimal? ApprovedManualAmount { get; set; }

        /// <summary>
        /// Persetujuan tersendiri atas nominal manual: PENDING | APPROVED | REJECTED; null bila skema bukan nominal manual. Nominal
        /// hanya dipakai payroll bila APPROVED. Perubahan nominal atau dasar mengembalikan status ke PENDING.
        /// </summary>
        public string? ManualAmountApprovalStatus { get; set; }
        public DateTime? ManualAmountSubmittedAt { get; set; }
        public DateTime? ManualAmountDecidedAt { get; set; }
        public string? ManualAmountDecisionNote { get; set; }

        /// <summary>Bantuan tampilan: pemanggil boleh menyetujui/menolak nominal ini (izin, status PENDING, bukan pengaju). Bukan pengaman.</summary>
        public bool CanDecideManualAmount { get; set; }

        /// <summary>Tarif P3B (persen) dan dasarnya untuk PPh Pasal 26; kosong bila memakai tarif umum kebijakan.</summary>
        public decimal? TreatyRatePercent { get; set; }
        public string? TreatyBasis { get; set; }

        /// <summary>NOT_CONFIGURED | CONFIGURED | NOT_EFFECTIVE | ERROR (turunan dari isi profil dan masa berlaku hari ini).</summary>
        public string ConfigurationStatus { get; set; } = string.Empty;
        public List<string> ConfigurationIssues { get; set; } = new();

        /// <summary>Status PTKP hanya dipakai perhitungan untuk PPh 21 penerima berstatus pegawai.</summary>
        public bool IsPtkpRelevant { get; set; }

        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpTaxDetailResponse : WfpTaxResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpTaxFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpTaxDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpTaxStringOptionResponse> TaxStatusOptions { get; set; } = new();
        public List<WfpTaxStringOptionResponse> TaxMethodOptions { get; set; } = new();

        /// <summary>Pilihan jenis PPh dari kontrak Backend (bukan daftar tertanam di layar).</summary>
        public List<WfpTaxStringOptionResponse> TaxArticleOptions { get; set; } = new();
        public List<WfpTaxStringOptionResponse> RecipientTaxCategoryOptions { get; set; } = new();

        /// <summary>Pilihan skema hitung. Skema yang belum didukung ditandai <c>IsSupported = false</c> beserta alasannya.</summary>
        public List<WfpTaxStringOptionResponse> CalculationModeOptions { get; set; } = new();
        public List<WfpTaxSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();

        /// <summary>Ketersediaan kebijakan tarif PPh berversi yang disahkan hari ini. Tanpanya skema otomatis menahan payroll (Menunggu Verifikasi Kebijakan).</summary>
        public bool TaxPolicyApproved { get; set; }
        public string? TaxPolicyVersion { get; set; }
        public string TaxPolicyStatusLabel { get; set; } = string.Empty;
    }

    public class WfpTaxDefaultFilterResponse
    {
        public string? TaxStatus { get; set; }
        public string? TaxMethod { get; set; }
        public bool? IsNpwpRegistered { get; set; }
        public bool? IsTaxResident { get; set; }
        public bool? HasPreviousEmployer { get; set; }
        public bool? IsEmployerBorneTax { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpTaxStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;

        /// <summary>False bila pilihan dikenal tetapi belum dapat dipakai (belum ada kebijakan yang sah); layar menonaktifkannya.</summary>
        public bool IsSupported { get; set; } = true;

        /// <summary>Penjelasan singkat atau alasan pilihan belum didukung.</summary>
        public string? Note { get; set; }
    }

    public class WfpTaxSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpTaxRequest
    {
        [MaxLength(50)]
        public string? NpwpNumber { get; set; }

        [Required, MaxLength(30)]
        public string TaxStatus { get; set; } = "TK/0";

        [Required, MaxLength(30)]
        public string TaxMethod { get; set; } = "Gross";

        [Required, MaxLength(3)]
        public string TaxCountryCode { get; set; } = "ID";

        [MaxLength(50)]
        public string? TaxOfficeCode { get; set; }

        public bool IsNpwpRegistered { get; set; }
        public bool IsTaxResident { get; set; } = true;
        public bool HasPreviousEmployer { get; set; }
        public bool IsEmployerBorneTax { get; set; }
        public decimal PreviousEmployerTaxableIncome { get; set; }
        public decimal PreviousEmployerTaxPaid { get; set; }
        public decimal AnnualNonTaxableIncome { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        // ---- Konfigurasi PPh HRD (additive; kosong semua = belum dikonfigurasi, kompatibel dengan klien lama). Bila salah satu dari
        // jenis, kategori, atau skema diisi, ketiganya wajib. Kode dikirim dalam bentuk kanonik (huruf besar). ----

        [MaxLength(20)]
        public string? TaxArticle { get; set; }

        [MaxLength(40)]
        public string? RecipientTaxCategory { get; set; }

        [MaxLength(40)]
        public string? CalculationMode { get; set; }

        [MaxLength(100)]
        public string? PolicyVersion { get; set; }

        [MaxLength(500)]
        public string? ExemptionReason { get; set; }

        [MaxLength(1000)]
        public string? DecisionBasis { get; set; }

        /// <summary>Hanya untuk skema APPROVED_MANUAL_AMOUNT. Menulis nominal membutuhkan izin ViewAmount.</summary>
        public decimal? ApprovedManualAmount { get; set; }

        /// <summary>Tarif P3B (persen) pengganti tarif PPh 26 umum; wajib disertai TreatyBasis dan hanya untuk PPH26.</summary>
        public decimal? TreatyRatePercent { get; set; }

        [MaxLength(500)]
        public string? TreatyBasis { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpTaxRequest : CreateWfpTaxRequest
    {
    }

    /// <summary>Keputusan atas nominal PPh manual. Alasan wajib saat menolak.</summary>
    public class WfpTaxManualAmountDecisionRequest
    {
        [MaxLength(500)]
        public string? Note { get; set; }
    }

    public class UpdateWfpTaxStatusRequest
    {
        public bool IsActive { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
