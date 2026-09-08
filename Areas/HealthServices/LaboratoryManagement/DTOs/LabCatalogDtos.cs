namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Penyaring katalog pemeriksaan laboratorium.
    /// </summary>
    public class LabCatalogQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;

        /// <summary>
        /// Menyaring per disiplin: <c>ClinicalPathology</c>, <c>AnatomicalPathology</c>, atau
        /// <c>Microbiology</c> (<c>LAB-DEC-036</c>). Kosong berarti seluruhnya, termasuk
        /// pemeriksaan yang belum digolongkan.
        /// </summary>
        public string? Discipline { get; set; }

        /// <summary>Pencarian bebas pada kode dan nama pemeriksaan.</summary>
        public string? Search { get; set; }

        /// <summary>
        /// Penjamin yang berlaku bagi pasien. Bila diisi, harga kontraknya ikut ditampilkan.
        /// </summary>
        public Guid? InsuranceProviderId { get; set; }

        /// <summary>Kelas perawatan pasien, bila tarif penjaminnya dipecah per kelas.</summary>
        public Guid? PatientClassId { get; set; }
    }

    /// <summary>Penyaring harga satu pemeriksaan.</summary>
    public class LabPriceQuery
    {
        public Guid? InsuranceProviderId { get; set; }

        public Guid? PatientClassId { get; set; }
    }

    /// <summary>Penyaring tampilan tarif laboratorium.</summary>
    public class LabTariffQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;

        public Guid? ProcedureId { get; set; }

        /// <summary>Pencarian bebas pada kode dan nama tarif serta nama pemeriksaannya.</summary>
        public string? Search { get; set; }
    }

    /// <summary>
    /// Satu baris katalog pemeriksaan laboratorium.
    ///
    /// <b>Harga di sini bukan tagihan.</b> Ia rujukan yang ditampilkan saat memesan, supaya
    /// petugas dan pasien tahu besarannya di muka. Keputusan menagih tetap milik Billing
    /// (<c>LAB-INH-010</c>, <c>LAB-INH-012</c>).
    /// </summary>
    public class LabCatalogItemResponse
    {
        public Guid ProcedureId { get; set; }

        public string ProcedureCode { get; set; } = string.Empty;

        public string ProcedureName { get; set; } = string.Empty;

        /// <summary>
        /// Disiplin yang menaungi pemeriksaan ini. Kosong bila katalognya belum digolongkan —
        /// keadaan yang sah, dan bukan kesalahan data.
        /// </summary>
        public string? Discipline { get; set; }

        /// <summary>Harga rumah sakit yang berlaku saat ini. Kosong bila tarifnya belum diatur.</summary>
        public decimal? UnitPrice { get; set; }

        public Guid? TariffId { get; set; }

        public string? TariffCode { get; set; }

        /// <summary>Penanda bawaan tercakup penjamin, dari data induk tindakan.</summary>
        public bool IsCoveredByInsuranceDefault { get; set; }

        /// <summary>
        /// Terisi hanya bila penyaring penjamin dikirim dan kontraknya memang ada. Kosong
        /// berarti tidak ada harga kontrak untuk penjamin itu — bukan berarti gratis.
        /// </summary>
        public decimal? ContractPrice { get; set; }

        /// <summary>
        /// Salah bila penyaring penjamin dikirim tetapi tidak ada kontrak yang cocok. Kosong
        /// bila penyaring penjaminnya memang tidak dikirim.
        /// </summary>
        public bool? IsCoveredByThisInsurance { get; set; }
    }

    /// <summary>
    /// Harga berlaku satu pemeriksaan beserta status cakupan penjaminnya.
    /// </summary>
    public class LabPriceResponse
    {
        public Guid ProcedureId { get; set; }

        public string ProcedureCode { get; set; } = string.Empty;

        public string ProcedureName { get; set; } = string.Empty;

        public string? Discipline { get; set; }

        /// <summary>Harga rumah sakit yang berlaku. Kosong bila tarifnya belum diatur.</summary>
        public decimal? HospitalPrice { get; set; }

        public Guid? TariffId { get; set; }

        public string? TariffCode { get; set; }

        /// <summary>Harga kontrak penjamin bila ada.</summary>
        public decimal? ContractPrice { get; set; }

        public string? InsuranceTariffCode { get; set; }

        public bool IsCoveredByInsuranceDefault { get; set; }

        /// <summary>
        /// Penanda tidak tercakup. Benar bila penjamin dikirim tetapi tidak ada kontrak yang
        /// cocok untuk pemeriksaan ini.
        /// </summary>
        public bool IsNotCovered { get; set; }

        /// <summary>Keterangan bagi pengguna ketika ada yang perlu dijelaskan.</summary>
        public string? Note { get; set; }
    }

    /// <summary>
    /// Satu baris tampilan tarif laboratorium — <b>baca saja</b>.
    ///
    /// Tarif tetap milik Master Data (<c>LAB-DEC-033</c>). Laboratorium tidak memiliki tabel
    /// tarif sendiri, dan grup ini tidak punya satu pun jalur ubah.
    /// </summary>
    public class LabTariffViewResponse
    {
        public Guid TariffId { get; set; }

        public string TariffCode { get; set; } = string.Empty;

        public string TariffName { get; set; } = string.Empty;

        public Guid? ProcedureId { get; set; }

        public string? ProcedureCode { get; set; }

        public string? ProcedureName { get; set; }

        public string? Discipline { get; set; }

        public decimal NormalPrice { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool IsActive { get; set; }
    }
}
