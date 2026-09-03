using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.DTOs
{
    public class JournalTypePagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;

        public bool? IsActive { get; set; }

        public bool? IsSystemType { get; set; }

        /// <summary>Dicocokkan ke kode maupun nama jenis jurnal.</summary>
        public string? Search { get; set; }

        public string? SortBy { get; set; }

        public string? SortDirection { get; set; }
    }

    public class JournalTypeResponse
    {
        public Guid Id { get; set; }

        public string JournalTypeCode { get; set; } = string.Empty;

        public string JournalTypeName { get; set; } = string.Empty;

        public string NumberPrefix { get; set; } = string.Empty;

        public bool RequiresApproval { get; set; }

        public bool IsSystemType { get; set; }

        public bool IsActive { get; set; }

        /// <summary>
        /// Benar bila jenis ini sudah dipakai jurnal mana pun. Dipakai layar untuk menjelaskan
        /// kenapa sebuah jenis tidak dapat dinonaktifkan begitu saja.
        /// </summary>
        public bool HasJournals { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    /// <summary>
    /// Isian pilihan pada form jurnal. Hanya jenis yang aktif.
    /// </summary>
    public class JournalTypeOptionResponse
    {
        public Guid Id { get; set; }

        public string JournalTypeCode { get; set; } = string.Empty;

        public string JournalTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Awalan nomor jurnal. Wajib diambil dari master ini — penomoran jurnal
        /// (`BE-ACC-010`) tidak boleh menuliskan awalannya di kode.
        /// </summary>
        public string NumberPrefix { get; set; } = string.Empty;

        public bool RequiresApproval { get; set; }
    }

    /// <summary>
    /// Sengaja <b>tanpa</b> <c>IsSystemType</c>.
    /// </summary>
    /// <remarks>
    /// Tanda sistem hanya lahir dari data master awal (`JB` dan `SA`, lihat
    /// <c>02-backend-architecture.md</c> bagian 9.1). Bila pengguna dapat menetapkannya sendiri,
    /// ia dapat membuat jenis jurnal yang kemudian terkunci dari perubahan tanpa alasan yang sah —
    /// dan aturan "jenis sistem terkunci" berubah dari pengaman menjadi jebakan.
    /// </remarks>
    public class CreateJournalTypeRequest
    {
        [Required]
        [MaxLength(10)]
        public string JournalTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string JournalTypeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string NumberPrefix { get; set; } = string.Empty;
    }

    /// <summary>
    /// <c>IsSystemType</c> dan <c>RequiresApproval</c> tidak dapat diubah lewat sini — keduanya
    /// bukan sesuatu yang dapat dicabut atau diberikan pengguna. Lihat keterangan pada
    /// <see cref="CreateJournalTypeRequest"/>.
    /// </summary>
    public class UpdateJournalTypeRequest
    {
        [Required]
        [MaxLength(10)]
        public string JournalTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string JournalTypeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string NumberPrefix { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Hasil pengisian data master awal, meneruskan apa adanya dari
    /// <c>AccountingMasterDataSeeder</c>.
    /// </summary>
    public class JournalTypeSeedResponse
    {
        public int Inserted { get; set; }

        public int Skipped { get; set; }

        public string? SkippedReason { get; set; }

        public List<JournalTypeResponse> Items { get; set; } = new();
    }
}
