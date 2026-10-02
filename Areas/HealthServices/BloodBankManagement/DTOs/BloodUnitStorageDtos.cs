using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs
{
    /// <summary>Menetapkan lokasi penyimpanan <b>pertama</b> sebuah kantong (<c>DEC-BD-036</c>).</summary>
    /// <remarks>
    /// Petugas pencatat diambil dari akun yang login, tidak pernah dari body. Lokasi wajib dipilih
    /// dari master yang <b>sedang aktif</b> (<c>VAL-BD-060</c>).
    /// </remarks>
    public class AssignStorageLocationRequest
    {
        [Required]
        public Guid StorageLocationId { get; set; }

        /// <summary>Keterangan bebas, bukan alasan terkendali.</summary>
        [MaxLength(500)]
        public string? Note { get; set; }

        /// <summary>
        /// Token konkurensi kantong yang dipegang layar. Bila kantong sudah berubah di tangan orang
        /// lain, penetapan ditolak <c>409</c>.
        /// </summary>
        public int? Version { get; set; }
    }

    /// <summary>Memindahkan kantong ke lokasi penyimpanan lain. Status kantong tidak berubah.</summary>
    /// <remarks>
    /// Tetap berlaku ketika lokasi asal sudah dinonaktifkan — inilah jalan keluar kantong dari
    /// kulkas yang rusak (<c>DEC-BD-037</c>). Lokasi tujuan wajib aktif (<c>VAL-BD-060</c>).
    /// </remarks>
    public class MoveStorageLocationRequest
    {
        [Required]
        public Guid StorageLocationId { get; set; }

        /// <summary>Keterangan bebas, bukan alasan terkendali (<c>validation-matrix</c> §4b).</summary>
        [MaxLength(500)]
        public string? Note { get; set; }

        /// <summary>Token konkurensi kantong yang dipegang layar.</summary>
        public int? Version { get; set; }
    }

    /// <summary>Satu baris riwayat penempatan kantong: di kulkas mana, sejak kapan, oleh siapa.</summary>
    public class BloodUnitPlacementDto
    {
        public Guid Id { get; set; }
        public Guid BloodUnitId { get; set; }

        public Guid StorageLocationId { get; set; }
        public string? StorageLocationCode { get; set; }
        public string? StorageLocationName { get; set; }

        /// <summary>
        /// Keaktifan lokasi <b>saat ini</b>, dibaca dari master ketika riwayat diambil — bukan
        /// keaktifannya ketika kantong ditaruh (saat itu lokasi pasti aktif, <c>INV-BD-027</c>).
        /// </summary>
        public bool IsStorageLocationActive { get; set; }

        /// <summary>Penempatan sebelumnya. Kosong pada penempatan pertama.</summary>
        public Guid? PreviousPlacementId { get; set; }
        public Guid? PreviousStorageLocationId { get; set; }
        public string? PreviousStorageLocationCode { get; set; }
        public string? PreviousStorageLocationName { get; set; }

        public DateTime PlacedAt { get; set; }
        public Guid PlacedByUserId { get; set; }

        /// <summary>Penempatan yang sedang berlaku. Paling banyak satu per kantong.</summary>
        public bool IsCurrent { get; set; }

        public string? Note { get; set; }
    }
}
