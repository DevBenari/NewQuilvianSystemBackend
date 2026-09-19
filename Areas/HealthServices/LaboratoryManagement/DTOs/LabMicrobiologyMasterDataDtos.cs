using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    // =====================================================================
    // Organisme
    // =====================================================================

    /// <summary>
    /// Penyaring daftar organisme untuk layar pengelolaan kepala instalasi. Memuat yang nonaktif
    /// juga; tanpa itu organisme yang dinonaktifkan nol akan pernah dapat diaktifkan kembali.
    /// </summary>
    public class LabOrganismPagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public bool? IsActive { get; set; }

        /// <summary>Pencarian bebas pada kode, nama, dan keterangan.</summary>
        public string? Search { get; set; }
    }

    /// <summary>
    /// Penyaring daftar pilihan organisme untuk layar pencatatan isolat. Berbeda dari
    /// <see cref="LabOrganismPagedQuery"/>: jalur ini <b>hanya</b> mengembalikan organisme aktif,
    /// sehingga analis nol dapat memilih kuman yang sudah ditarik dari daftar (<c>VAL-85</c>).
    /// </summary>
    public class LabOrganismOptionQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 50;

        public string? Search { get; set; }
    }

    /// <summary>Bentuk tampilan satu organisme pada layar pengelolaan.</summary>
    public class LabOrganismResponse
    {
        public Guid Id { get; set; }

        public string OrganismCode { get; set; } = string.Empty;

        public string OrganismName { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }
    }

    /// <summary>
    /// Bentuk ringan untuk kotak pilihan saat mencatat isolat. Sengaja tanpa status aktif:
    /// daftar ini hanya berisi yang aktif, sehingga ruas itu nol menambah keputusan bagi analis.
    /// </summary>
    public class LabOrganismOptionResponse
    {
        public Guid Id { get; set; }

        public string OrganismCode { get; set; } = string.Empty;

        public string OrganismName { get; set; } = string.Empty;

        public int SortOrder { get; set; }
    }

    /// <summary>Menambah organisme. Organisme baru selalu lahir aktif.</summary>
    public class CreateLabOrganismRequest
    {
        /// <summary>Kode organisme, dinormalkan menjadi huruf kapital dan wajib unik (<c>VAL-91</c>).</summary>
        [Required]
        [MaxLength(32)]
        public string OrganismCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string OrganismName { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Mengubah nama, keterangan, urutan, dan status aktif.
    ///
    /// <b>Kode organisme tidak ikut berubah.</b> Ia penanda yang dirujuk integrasi dan pelaporan;
    /// mengubahnya diam-diam memutus rujukan yang sudah ada — pola yang sama dengan
    /// <c>UpdateLabSpecimenTypeRequest</c>.
    ///
    /// <b>Penonaktifan berjalan lewat sini</b>, sebab kontrak <c>r24</c> bagian 19.4 menyediakan
    /// tepat empat jalur: <c>GET</c>, <c>GET /options</c>, <c>POST</c>, dan <c>PUT /{id}</c> —
    /// nol <c>DELETE</c> dan nol jalur aktivasi tersendiri.
    /// </summary>
    public class UpdateLabOrganismRequest
    {
        [Required]
        [MaxLength(200)]
        public string OrganismName { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // =====================================================================
    // Antibiotik
    // =====================================================================

    /// <summary>Penyaring daftar antibiotik untuk layar pengelolaan.</summary>
    public class LabAntibioticPagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public bool? IsActive { get; set; }

        public string? Search { get; set; }
    }

    /// <summary>
    /// Penyaring daftar pilihan antibiotik untuk panel uji kepekaan. Hanya yang aktif
    /// (<c>VAL-86</c>).
    /// </summary>
    public class LabAntibioticOptionQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 50;

        public string? Search { get; set; }
    }

    /// <summary>Bentuk tampilan satu antibiotik pada layar pengelolaan.</summary>
    public class LabAntibioticResponse
    {
        public Guid Id { get; set; }

        public string AntibioticCode { get; set; } = string.Empty;

        public string AntibioticName { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }
    }

    /// <summary>Bentuk ringan untuk kotak pilihan panel uji kepekaan.</summary>
    public class LabAntibioticOptionResponse
    {
        public Guid Id { get; set; }

        public string AntibioticCode { get; set; } = string.Empty;

        public string AntibioticName { get; set; } = string.Empty;

        public int SortOrder { get; set; }
    }

    /// <summary>Menambah antibiotik ke panel uji. Selalu lahir aktif.</summary>
    public class CreateLabAntibioticRequest
    {
        /// <summary>Kode antibiotik, dinormalkan menjadi huruf kapital dan wajib unik (<c>VAL-91</c>).</summary>
        [Required]
        [MaxLength(32)]
        public string AntibioticCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string AntibioticName { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Mengubah nama, keterangan, urutan, dan status aktif. Kode tidak ikut berubah, sebab yang
    /// sama dengan <see cref="UpdateLabOrganismRequest"/>.
    /// </summary>
    public class UpdateLabAntibioticRequest
    {
        [Required]
        [MaxLength(200)]
        public string AntibioticName { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
