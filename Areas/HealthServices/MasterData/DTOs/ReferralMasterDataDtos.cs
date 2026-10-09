using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    /// <summary>
    /// Penyaring daftar pilihan instansi perujuk.
    /// </summary>
    public class ReferralInstitutionOptionQuery
    {
        /// <summary>Pencarian bebas pada kode dan nama instansi.</summary>
        public string? Search { get; set; }

        /// <summary>
        /// Bawaannya hanya yang aktif. Instansi yang tidak lagi bekerja sama dinonaktifkan,
        /// bukan dihapus, sehingga kunjungan lama yang menunjuknya tetap dapat dibaca — dan
        /// karena itu ia tetap dapat ditampilkan bila memang diminta.
        /// </summary>
        public bool OnlyActive { get; set; } = true;

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Penyaring daftar pilihan dokter perujuk.
    /// </summary>
    public class ReferralDoctorOptionQuery
    {
        /// <summary>
        /// Instansi tempat dokter berpraktik. <b>Menyaring dengan ini sangat dianjurkan:</b>
        /// pendaftaran rujukan menolak dokter yang tidak berpraktik pada instansi yang dipilih,
        /// sehingga daftar yang tidak tersaring hanya akan menawarkan pilihan yang pasti ditolak.
        /// </summary>
        public Guid? ReferralInstitutionId { get; set; }

        /// <summary>Pencarian bebas pada nama dokter.</summary>
        public string? Search { get; set; }

        public bool OnlyActive { get; set; } = true;

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Satu baris pilihan instansi perujuk.
    /// </summary>
    public class ReferralInstitutionOptionResponse
    {
        public Guid Id { get; set; }

        public string InstitutionCode { get; set; } = string.Empty;

        public string InstitutionName { get; set; } = string.Empty;

        public string? Address { get; set; }

        public string? PhoneNumber { get; set; }

        /// <summary>Bermitra dengan rumah sakit (RJ-DOC-DEC-073).</summary>
        public bool IsPartner { get; set; }

        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Satu baris pilihan dokter perujuk.
    ///
    /// Nama instansinya ikut dibawa supaya layar dapat menampilkan asal dokter tanpa perlu
    /// memanggil daftar instansi lagi hanya untuk mencocokkan nama.
    /// </summary>
    public class ReferralDoctorOptionResponse
    {
        public Guid Id { get; set; }

        public Guid ReferralInstitutionId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string? InstitutionName { get; set; }

        public bool IsActive { get; set; }
    }

    // =====================================================================
    // Master Institusi dan Dokter Perujuk — RJ-DOC-REV-BE-017 (RJ-DOC-DEC-073, 080)
    // =====================================================================

    /// <summary>Ringkasan halaman index Institusi Perujuk.</summary>
    public class ReferralInstitutionSummaryResponse
    {
        public int TotalReferralInstitution { get; set; }
        public int ActiveReferralInstitution { get; set; }
        public int InactiveReferralInstitution { get; set; }
        public int PartnerReferralInstitution { get; set; }
    }

    public class ReferralInstitutionResponse
    {
        public Guid Id { get; set; }
        public string InstitutionCode { get; set; } = string.Empty;
        public string InstitutionName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsPartner { get; set; }
        public bool IsActive { get; set; }
        public int ActiveDoctorCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateReferralInstitutionRequest
    {
        [Required(ErrorMessage = "Kode institusi perujuk wajib diisi.")]
        [MaxLength(50, ErrorMessage = "Kode institusi perujuk terlalu panjang. Batasnya 50 huruf.")]
        public string InstitutionCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama institusi perujuk wajib diisi.")]
        [MaxLength(200, ErrorMessage = "Nama institusi perujuk terlalu panjang. Batasnya 200 huruf.")]
        public string InstitutionName { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Alamat terlalu panjang. Batasnya 500 huruf.")]
        public string? Address { get; set; }

        [MaxLength(50, ErrorMessage = "Nomor telepon terlalu panjang. Batasnya 50 huruf.")]
        public string? PhoneNumber { get; set; }

        public bool IsPartner { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateReferralInstitutionRequest : CreateReferralInstitutionRequest
    {
    }

    /// <summary>Ringkasan halaman index Dokter Perujuk.</summary>
    public class ReferralDoctorSummaryResponse
    {
        public int TotalReferralDoctor { get; set; }
        public int ActiveReferralDoctor { get; set; }
        public int InactiveReferralDoctor { get; set; }
    }

    public class ReferralDoctorResponse
    {
        public Guid Id { get; set; }
        public Guid ReferralInstitutionId { get; set; }
        public string? InstitutionCode { get; set; }
        public string? InstitutionName { get; set; }
        public bool InstitutionIsPartner { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateReferralDoctorRequest
    {
        [Required(ErrorMessage = "Institusi perujuk wajib dipilih.")]
        public Guid ReferralInstitutionId { get; set; }

        [Required(ErrorMessage = "Nama dokter perujuk wajib diisi.")]
        [MaxLength(200, ErrorMessage = "Nama dokter perujuk terlalu panjang. Batasnya 200 huruf.")]
        public string DoctorName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateReferralDoctorRequest : CreateReferralDoctorRequest
    {
    }

    public class UpdateReferralMasterStatusRequest
    {
        public bool IsActive { get; set; }
    }

    // ---------- Metadata filter bersama kedua master perujuk ----------

    public class ReferralMasterFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public ReferralMasterDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<ReferralMasterSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<ReferralMasterQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<ReferralMasterFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<ReferralMasterFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class ReferralMasterDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsPartner { get; set; }
        public Guid? ReferralInstitutionId { get; set; }
        public string SortBy { get; set; } = string.Empty;
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class ReferralMasterSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ReferralMasterQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class ReferralMasterFormFieldMetadataResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public bool IsRequiredOnCreate { get; set; }
        public bool IsRequiredOnUpdate { get; set; }
        public string RequiredType { get; set; } = "Optional";
        public int? MaxLength { get; set; }
        public string? OptionsSource { get; set; }
        public string? Description { get; set; }
        public string? Example { get; set; }
        public int SortOrder { get; set; }
    }
}
