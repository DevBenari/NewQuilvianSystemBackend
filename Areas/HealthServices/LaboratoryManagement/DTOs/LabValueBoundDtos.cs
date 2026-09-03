using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Penyaring daftar batas nilai. Mengikuti bentuk paging yang sudah dipakai keluarga
    /// endpoint lain di repository ini.
    /// </summary>
    public class LabValueBoundPagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        /// <summary>Menyaring per jenis pemeriksaan.</summary>
        public Guid? ProcedureId { get; set; }

        /// <summary>Menyaring aktif atau tidak. Kosong berarti keduanya ditampilkan.</summary>
        public bool? IsActive { get; set; }

        /// <summary>Pencarian bebas pada kode dan nama pemeriksaan.</summary>
        public string? Search { get; set; }
    }

    public class LabValueBoundListResponse
    {
        public Guid Id { get; set; }

        public Guid ProcedureId { get; set; }

        public string ProcedureCode { get; set; } = string.Empty;

        public string ProcedureName { get; set; } = string.Empty;

        /// <summary>Bentuk hasil: <c>Numeric</c> atau <c>Choice</c>.</summary>
        public string ResultForm { get; set; } = string.Empty;

        public string? Unit { get; set; }

        /// <summary>Pembatas jenis kelamin: <c>All</c>, <c>Male</c>, atau <c>Female</c>.</summary>
        public string GenderScope { get; set; } = string.Empty;

        public Guid? AgeCategoryId { get; set; }

        /// <summary>Nama kelompok umur. Kosong berarti berlaku untuk semua umur.</summary>
        public string? AgeCategoryName { get; set; }

        public decimal? NormalLow { get; set; }

        public decimal? NormalHigh { get; set; }

        public decimal? CriticalLow { get; set; }

        public decimal? CriticalHigh { get; set; }

        public int? CitoTurnaroundMinutes { get; set; }

        public bool IsActive { get; set; }

        /// <summary>Jumlah pilihan sah. Selalu nol untuk bentuk hasil angka.</summary>
        public int OptionCount { get; set; }
    }

    public class LabValueBoundDetailResponse : LabValueBoundListResponse
    {
        public List<LabValueOptionResponse> Options { get; set; } = new();

        /// <summary>
        /// Ada pengajuan perubahan batas kritis yang belum diputuskan untuk batas nilai ini.
        /// Selama bernilai benar, batas kritisnya sedang menunggu keputusan pihak klinis.
        /// </summary>
        public bool HasPendingCriticalChangeRequest { get; set; }
    }

    public class LabValueOptionResponse
    {
        public Guid Id { get; set; }

        public string OptionCode { get; set; } = string.Empty;

        public string OptionName { get; set; } = string.Empty;

        public bool IsOutOfReference { get; set; }

        public bool IsCritical { get; set; }

        public int SortOrder { get; set; }
    }

    public class LabValueOptionRequest
    {
        [Required]
        [MaxLength(20)]
        public string OptionCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string OptionName { get; set; } = string.Empty;

        public bool IsOutOfReference { get; set; }

        /// <summary>
        /// Penanda pilihan kritis. Pada <c>PUT</c> ruas ini dijaga <c>VAL-28</c>: mengubahnya
        /// berarti mengubah batas kritis, dan itu hanya boleh lewat pengajuan yang disetujui.
        /// </summary>
        public bool IsCritical { get; set; }

        /// <summary>Urutan pilihan pada skala hasilnya, misalnya Negatif, +1, +2, +3, +4.</summary>
        public int SortOrder { get; set; }
    }

    public class CreateLabValueBoundRequest
    {
        [Required]
        public Guid ProcedureId { get; set; }

        public LabResultForm ResultForm { get; set; } = LabResultForm.Numeric;

        /// <summary>Satuan hasil. Wajib untuk bentuk angka (<c>VAL-22</c>).</summary>
        [MaxLength(20)]
        public string? Unit { get; set; }

        public decimal? NormalLow { get; set; }

        public decimal? NormalHigh { get; set; }

        public decimal? CriticalLow { get; set; }

        public decimal? CriticalHigh { get; set; }

        public LabGenderScope GenderScope { get; set; } = LabGenderScope.All;

        /// <summary>Kelompok umur. Kosong berarti berlaku untuk semua umur.</summary>
        public Guid? AgeCategoryId { get; set; }

        public int? CitoTurnaroundMinutes { get; set; }

        /// <summary>
        /// Daftar pilihan sah. Wajib berisi sekurang-kurangnya satu untuk bentuk pilihan
        /// (<c>VAL-23</c>), dan wajib kosong untuk bentuk angka (<c>VAL-24</c>).
        /// </summary>
        public List<LabValueOptionRequest> Options { get; set; } = new();
    }

    /// <summary>
    /// Permintaan ubah batas nilai.
    ///
    /// Jenis pemeriksaan, jenis kelamin, dan kelompok umur **tidak** dapat diubah lewat sini —
    /// ketiganya membentuk identitas kelompok pasien sebuah baris batas, dan mengubahnya sama
    /// dengan membuat baris lain. Batas kritis juga tidak dapat diubah di sini; ruasnya tetap
    /// diterima supaya perubahan yang dicoba dapat dikenali dan ditolak <c>VAL-28</c>, bukan
    /// diabaikan diam-diam.
    /// </summary>
    public class UpdateLabValueBoundRequest
    {
        [MaxLength(20)]
        public string? Unit { get; set; }

        public decimal? NormalLow { get; set; }

        public decimal? NormalHigh { get; set; }

        /// <summary>Diperiksa <c>VAL-28</c>. Nilai yang sama dengan yang berlaku diterima; yang berbeda ditolak.</summary>
        public decimal? CriticalLow { get; set; }

        /// <summary>Diperiksa <c>VAL-28</c>, sama seperti <see cref="CriticalLow"/>.</summary>
        public decimal? CriticalHigh { get; set; }

        public int? CitoTurnaroundMinutes { get; set; }

        /// <summary>
        /// Daftar pilihan pengganti. Kosong berarti daftar pilihan tidak diubah; kirim daftar
        /// lengkapnya untuk menggantinya.
        /// </summary>
        public List<LabValueOptionRequest>? Options { get; set; }

        /// <summary>Alasan perubahan, ikut tersimpan pada riwayat (<c>AC-34</c>).</summary>
        [MaxLength(1000)]
        public string? ChangeReason { get; set; }
    }

    public class LabValueBoundHistoryResponse
    {
        public Guid Id { get; set; }

        public Guid ValueBoundId { get; set; }

        public string ChangedField { get; set; } = string.Empty;

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public Guid ActorUserId { get; set; }

        /// <summary>Terisi hanya bila yang berubah batas kritis.</summary>
        public Guid? ApprovedByUserId { get; set; }

        public string? ChangeReason { get; set; }

        public DateTime OccurredAt { get; set; }
    }
}
