using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.EventType.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.EventType.DTOs
{
    /// <summary>
    /// Penyaring daftar jenis kejadian (<c>GET /event-types</c>).
    /// </summary>
    public class EventTypePagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;

        public bool? IsActive { get; set; }

        /// <summary>Contoh <c>Finance</c>. Dicocokkan persis, tanpa membedakan huruf besar kecil.</summary>
        public string? SourceModule { get; set; }

        /// <summary>Dicocokkan ke kode maupun nama jenis kejadian.</summary>
        public string? Search { get; set; }

        /// <summary><c>eventTypeCode</c> (bawaan), <c>eventTypeName</c>, <c>sourceModule</c>, atau <c>createDateTime</c>.</summary>
        public string? SortBy { get; set; }

        public string? SortDirection { get; set; }
    }

    /// <summary>
    /// Satu baris daftar jenis kejadian.
    /// </summary>
    public class EventTypeListResponse
    {
        public Guid Id { get; set; }

        public string EventTypeCode { get; set; } = string.Empty;

        public string EventTypeName { get; set; } = string.Empty;

        public string SourceModule { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public EventTypeKind EventKind { get; set; }

        /// <summary>
        /// Jumlah aturan posting aktif yang memakai jenis ini, dari seluruh badan hukum. Dipakai
        /// layar untuk menjelaskan kenapa jenis ini tidak dapat dinonaktifkan: selama angkanya
        /// lebih dari nol, penonaktifan ditolak <c>409</c>.
        /// </summary>
        public int ActivePostingRuleCount { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    /// <summary>
    /// Rincian satu jenis kejadian beserta jejak perubahannya.
    /// </summary>
    public class EventTypeDetailResponse : EventTypeListResponse
    {
        public Guid CreateBy { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        /// <summary><c>Guid.Empty</c> bila belum pernah diubah.</summary>
        public Guid UpdateBy { get; set; }

        public int AccountingEventCount { get; set; }
    }

    /// <summary>
    /// Isian pilihan pada form aturan posting. Hanya jenis yang aktif.
    /// </summary>
    public class EventTypeOptionResponse
    {
        public Guid Id { get; set; }

        public string EventTypeCode { get; set; } = string.Empty;

        public string EventTypeName { get; set; } = string.Empty;

        public string SourceModule { get; set; } = string.Empty;
    }

    /// <summary>
    /// Menambah jenis kejadian. Jenis baru selalu lahir aktif (kamus data bagian 11).
    /// </summary>
    public class CreateEventTypeRequest
    {
        /// <summary>
        /// Contoh <c>PENGAKUAN-PIUTANG</c>. Wajib sama persis dengan kode yang dibawa pesan
        /// kejadian dari penerbit, karena pencocokannya lewat kode ini.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string EventTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string EventTypeName { get; set; } = string.Empty;

        /// <summary>Modul yang diharapkan menerbitkannya, contoh <c>Finance</c>.</summary>
        [Required]
        [MaxLength(50)]
        public string SourceModule { get; set; } = string.Empty;

        public EventTypeKind? EventKind { get; set; }
    }

    /// <summary>
    /// Mengubah nama dan modul asal (<c>ACC-API</c> grup Event Type). <b>Kode tidak dapat
    /// diubah</b>: kode adalah kunci pencocokan dengan pesan kejadian, dan kejadian tertahan
    /// menyimpan kode aslinya (<c>ACC-DEC-075</c>). Keaktifan diubah lewat
    /// <c>activate</c>/<c>deactivate</c>, bukan lewat sini.
    /// </summary>
    public class UpdateEventTypeRequest
    {
        [Required]
        [MaxLength(200)]
        public string EventTypeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string SourceModule { get; set; } = string.Empty;

        public EventTypeKind? EventKind { get; set; }
    }
}
