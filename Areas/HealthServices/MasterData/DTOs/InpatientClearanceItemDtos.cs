using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    /// <summary>
    /// Bentuk balasan satu butir administrasi yang menahan penutupan episode Rawat Inap.
    /// </summary>
    public class InpatientClearanceItemResponse
    {
        public Guid Id { get; set; }

        /// <summary>Kode butir, unik di seluruh tabel. Contoh <c>ADM-DOC</c>.</summary>
        public string ItemCode { get; set; } = string.Empty;

        public string ItemName { get; set; } = string.Empty;

        public string? Description { get; set; }

        /// <summary>
        /// Butir wajib menahan penutupan episode selama belum ditandai. Butir tidak wajib
        /// tetap dapat ditandai, tetapi tidak menahan apa pun.
        /// </summary>
        public bool IsMandatory { get; set; }

        /// <summary>Urutan tampil butir pada daftar periksa yang dikerjakan petugas.</summary>
        public int SortOrder { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateInpatientClearanceItemRequest
    {
        [Required]
        [MaxLength(50)]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsMandatory { get; set; } = true;

        [Range(0, 9999)]
        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Bentuk permintaan mengubah butir administrasi. Sengaja memuat <c>ItemCode</c>, karena
    /// butir yang salah ketik kodenya harus dapat dibetulkan selama kode barunya belum dipakai
    /// butir lain.
    /// </summary>
    public class UpdateInpatientClearanceItemRequest : CreateInpatientClearanceItemRequest
    {
    }

    /// <summary>
    /// Bentuk permintaan mengaktifkan atau menonaktifkan butir administrasi.
    /// </summary>
    /// <remarks>
    /// Menonaktifkan butir TIDAK menghapus penandaan yang sudah ada pada episode lama.
    /// Penandaan adalah catatan bahwa sesuatu pernah diselesaikan seseorang pada suatu waktu;
    /// menghapusnya karena butirnya tidak berlaku lagi akan membuat riwayat episode lama
    /// berbohong.
    /// </remarks>
    public class UpdateInpatientClearanceItemStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteInpatientClearanceItemRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }
}
