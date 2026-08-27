using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>Satu baris riwayat perpindahan status episode.</summary>
    /// <remarks>
    /// Baris riwayat <b>tidak dapat diubah dan tidak dapat dihapus</b>. Tidak ada endpoint
    /// <c>PUT</c> maupun <c>DELETE</c> yang menunjuknya, dan ketiadaan itu disengaja — api
    /// contract bagian 8 dan <c>RWI-RULE-031</c> aturan 5.
    /// </remarks>
    public class InpatientStatusHistoryResponse
    {
        public Guid Id { get; set; }

        public Guid EpisodeId { get; set; }

        public int SequenceNumber { get; set; }

        public int? FromStatus { get; set; }

        public string? FromStatusName { get; set; }

        public int ToStatus { get; set; }

        public string ToStatusName { get; set; } = string.Empty;

        public string ActionType { get; set; } = string.Empty;

        /// <summary>1 <c>User</c>, 2 <c>System</c>.</summary>
        public int ActorType { get; set; }

        public string ActorTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Kosong bila perpindahan dihitung sistem. Kolom ini <b>tidak boleh</b> diisi dengan
        /// identitas orang yang kebetulan membaca layar ketika perhitungan kedaluwarsa
        /// berjalan — itu akan membuat laporan pengecualian menuduh orang yang tidak melakukan
        /// apa-apa.
        /// </summary>
        public Guid? ChangedByUserId { get; set; }

        public DateTime ChangedAt { get; set; }

        public string? Reason { get; set; }
    }

    /// <summary>Bentuk permintaan membuka sesi koreksi pada episode yang sudah ditutup.</summary>
    public class OpenCorrectionSessionRequest
    {
        [Required]
        [MaxLength(500)]
        public string OpenReason { get; set; } = string.Empty;
    }

    /// <summary>Bentuk permintaan menutup sesi koreksi.</summary>
    /// <remarks>
    /// Daftar perubahan wajib diisi. Sesi koreksi yang ditutup tanpa daftar perubahan
    /// meninggalkan pertanyaan yang tidak dapat dijawab siapa pun: episode ini pernah dibuka
    /// kembali, tetapi tidak ada satu pun catatan tentang apa yang dibetulkan.
    /// </remarks>
    public class CloseCorrectionSessionRequest
    {
        [Required]
        [MaxLength(4000)]
        public string ChangedFieldSummary { get; set; } = string.Empty;
    }

    /// <summary>Satu sesi koreksi.</summary>
    /// <remarks>
    /// <b>Status episode tetap <c>Closed</c> sepanjang sesi berjalan.</b> Inilah yang
    /// membedakan sesi koreksi dari status keenam, dan yang membuat <c>RWI-DEC-009</c> serta
    /// <c>RWI-AC-004</c> tidak dilanggar. Selama sesi terbuka, tempat tidur tidak
    /// dikembalikan, pasien tidak muncul pada census, dan lama dirawat tidak bertambah.
    /// </remarks>
    public class InpatientCorrectionSessionResponse
    {
        public Guid Id { get; set; }

        public Guid EpisodeId { get; set; }

        public string? EpisodeNumber { get; set; }

        public int SequenceNumber { get; set; }

        public DateTime OpenedAt { get; set; }

        public Guid OpenedByUserId { get; set; }

        public string OpenReason { get; set; } = string.Empty;

        public DateTime? ClosedAt { get; set; }

        public Guid? ClosedByUserId { get; set; }

        public string? ChangedFieldSummary { get; set; }

        public bool IsOpen { get; set; }
    }
}
