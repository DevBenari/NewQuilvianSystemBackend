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
}
