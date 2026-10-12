using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
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

        /// <summary>
        /// Hari terakhir perjanjian yang berlaku pada tanggal layanan. Hanya diisi feed mitra
        /// layak (<c>partner-options</c>, <c>kiosk/options</c>); <c>null</c> pada <c>options</c>.
        /// </summary>
        public DateOnly? AgreementEndDate { get; set; }
    }

    /// <summary>
    /// Penyaring feed mitra layak (<c>DEC-FRJ-001</c>) untuk Kiosk dan Pendaftaran Rawat Jalan.
    /// </summary>
    public class ReferralPartnerOptionQuery
    {
        /// <summary>Pencarian pada kode dan nama fasilitas.</summary>
        [MaxLength(100)]
        public string? Search { get; set; }

        /// <summary>
        /// Tanggal layanan (<c>yyyy-MM-dd</c>, kalender WIB) untuk menilai masa berlaku perjanjian.
        /// Bawaannya hari ini. Contoh: kunjungan terjadwal besok memakai tanggal besok.
        /// </summary>
        public DateOnly? ServiceDate { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
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

        // ---------- DEC-FRJ-001: indikator halaman Fasilitas Perujuk ----------
        // Tidak saling lepas; satu fasilitas paling banyak terhitung sekali per indikator.

        /// <summary>Seluruh master mitra yang belum dihapus.</summary>
        public int TotalPartner { get; set; }

        /// <summary>Mitra aktif dengan perjanjian yang berlaku hari ini.</summary>
        public int ActivePartner { get; set; }

        /// <summary>Mitra yang dinonaktifkan secara administratif.</summary>
        public int InactivePartner { get; set; }

        /// <summary>Mitra yang perjanjian terakhirnya sudah berakhir tanpa perpanjangan.</summary>
        public int ExpiredContractPartner { get; set; }
    }

    /// <summary>Penyaring daftar utama Fasilitas Perujuk (<c>GET /</c>).</summary>
    public class ReferralInstitutionListQuery
    {
        /// <summary>Tanggal dibuat mulai, kalender WIB (<c>yyyy-MM-dd</c>).</summary>
        public DateTime? StartDate { get; set; }

        /// <summary>Tanggal dibuat sampai, kalender WIB, inklusif.</summary>
        public DateTime? EndDate { get; set; }

        /// <summary><c>custom</c>, <c>today</c>, <c>last7days</c>, <c>last30days</c>, <c>thismonth</c>, <c>lastmonth</c>.</summary>
        [MaxLength(20)]
        public string? CustomPeriod { get; set; }

        public ReferralInstitutionType? InstitutionType { get; set; }

        public ReferralPartnershipStatus? PartnershipStatus { get; set; }

        [MaxLength(100)]
        public string? Search { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsPartner { get; set; }

        [MaxLength(50)]
        public string? SortBy { get; set; }

        [MaxLength(4)]
        public string? SortDirection { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    /// <summary>Satu periode perjanjian kerja sama (histori pada detail).</summary>
    public class ReferralInstitutionAgreementResponse
    {
        public Guid Id { get; set; }
        public string AgreementNumber { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        /// <summary><c>Berlaku</c>, <c>Akan Berlaku</c>, atau <c>Berakhir</c> terhadap hari ini.</summary>
        public string PeriodStatus { get; set; } = string.Empty;

        public DateTime CreateDateTime { get; set; }
        public Guid CreateBy { get; set; }
        public string? CreatedByName { get; set; }
    }

    public class ReferralInstitutionResponse
    {
        public Guid Id { get; set; }
        public string InstitutionCode { get; set; } = string.Empty;
        public string InstitutionName { get; set; } = string.Empty;
        public ReferralInstitutionType InstitutionType { get; set; }
        public string InstitutionTypeName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public Guid? ProvinceId { get; set; }
        public string? ProvinceName { get; set; }
        public Guid? CityId { get; set; }
        public string? CityName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? PicName { get; set; }
        public string? ExternalFacilityCode { get; set; }
        public string? Description { get; set; }
        public bool IsPartner { get; set; }
        public bool IsActive { get; set; }

        /// <summary>Status kerja sama terhitung terhadap hari ini (<c>DEC-FRJ-001</c>).</summary>
        public ReferralPartnershipStatus PartnershipStatus { get; set; }
        public string PartnershipStatusName { get; set; } = string.Empty;

        /// <summary>Benar bila dapat dipilih pada transaksi rujukan baru hari ini.</summary>
        public bool IsEligible { get; set; }

        /// <summary>
        /// Perjanjian yang relevan untuk kolom "Masa Kerja Sama": yang berlaku hari ini, bila tidak
        /// ada yang akan berlaku paling awal, bila tidak ada yang terakhir berakhir.
        /// </summary>
        public Guid? AgreementId { get; set; }
        public string? AgreementNumber { get; set; }
        public DateOnly? AgreementStartDate { get; set; }
        public DateOnly? AgreementEndDate { get; set; }

        public int ActiveDoctorCount { get; set; }
        public Guid RowVersion { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid CreateBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid UpdateBy { get; set; }
        public string? UpdatedByName { get; set; }

        /// <summary>Histori perjanjian, terbaru di atas. Hanya diisi pada detail.</summary>
        public List<ReferralInstitutionAgreementResponse> Agreements { get; set; } = new();
    }

    /// <summary>
    /// Isian master Fasilitas Perujuk mitra (<c>DEC-FRJ-001</c>). Seluruh ruas wajib divalidasi
    /// ulang service; atribut di sini hanya batas bentuk.
    /// </summary>
    public class CreateReferralInstitutionRequest
    {
        [Required(ErrorMessage = "Kode fasilitas wajib diisi.")]
        [MaxLength(50, ErrorMessage = "Kode fasilitas terlalu panjang. Batasnya 50 karakter.")]
        public string InstitutionCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama fasilitas wajib diisi.")]
        [MaxLength(200, ErrorMessage = "Nama fasilitas terlalu panjang. Batasnya 200 karakter.")]
        public string InstitutionName { get; set; } = string.Empty;

        public ReferralInstitutionType InstitutionType { get; set; }

        [MaxLength(500, ErrorMessage = "Alamat terlalu panjang. Batasnya 500 karakter.")]
        public string? Address { get; set; }

        public Guid? ProvinceId { get; set; }

        public Guid? CityId { get; set; }

        [MaxLength(50, ErrorMessage = "Nomor telepon terlalu panjang. Batasnya 50 karakter.")]
        public string? PhoneNumber { get; set; }

        [MaxLength(150, ErrorMessage = "Email terlalu panjang. Batasnya 150 karakter.")]
        public string? Email { get; set; }

        [MaxLength(150, ErrorMessage = "Nama PIC terlalu panjang. Batasnya 150 karakter.")]
        public string? PicName { get; set; }

        [MaxLength(50, ErrorMessage = "Kode faskes eksternal terlalu panjang. Batasnya 50 karakter.")]
        public string? ExternalFacilityCode { get; set; }

        [MaxLength(1000, ErrorMessage = "Keterangan terlalu panjang. Batasnya 1000 karakter.")]
        public string? Description { get; set; }

        [MaxLength(100, ErrorMessage = "Nomor perjanjian terlalu panjang. Batasnya 100 karakter.")]
        public string? AgreementNumber { get; set; }

        public DateOnly? AgreementStartDate { get; set; }

        public DateOnly? AgreementEndDate { get; set; }

        /// <summary>
        /// Diabaikan sejak <c>DEC-FRJ-001</c>: master yang dibuat atau diubah selalu mitra.
        /// Dipertahankan agar klien lama tidak gagal deserialisasi.
        /// </summary>
        public bool IsPartner { get; set; } = true;

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Update penuh. Ruas perjanjian mengoreksi perjanjian terakhir; perpanjangan memakai
    /// <c>POST /{id}/agreements</c> supaya histori tidak tertimpa.
    /// </summary>
    public class UpdateReferralInstitutionRequest : CreateReferralInstitutionRequest
    {
        /// <summary><c>RowVersion</c> dari detail; ditolak <c>409</c> bila sudah berubah.</summary>
        public Guid? ExpectedRowVersion { get; set; }
    }

    /// <summary>Perpanjangan atau perjanjian baru (<c>POST /{id}/agreements</c>).</summary>
    public class CreateReferralInstitutionAgreementRequest
    {
        [Required(ErrorMessage = "Nomor perjanjian wajib diisi.")]
        [MaxLength(100, ErrorMessage = "Nomor perjanjian terlalu panjang. Batasnya 100 karakter.")]
        public string AgreementNumber { get; set; } = string.Empty;

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public Guid? ExpectedRowVersion { get; set; }
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
        public List<ReferralMasterCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<ReferralMasterEnumOptionResponse> InstitutionTypeOptions { get; set; } = new();
        public List<ReferralMasterEnumOptionResponse> PartnershipStatusOptions { get; set; } = new();
        public List<ReferralMasterQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<ReferralMasterFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<ReferralMasterFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class ReferralMasterDefaultFilterResponse
    {
        public string? CustomPeriod { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public int? InstitutionType { get; set; }
        public int? PartnershipStatus { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsPartner { get; set; }
        public Guid? ReferralInstitutionId { get; set; }
        public string SortBy { get; set; } = string.Empty;
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class ReferralMasterCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class ReferralMasterEnumOptionResponse
    {
        public int Value { get; set; }
        public string Label { get; set; } = string.Empty;
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
