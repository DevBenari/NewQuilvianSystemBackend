using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    /// <summary>
    /// Bentuk balasan pengaturan Rawat Inap yang berlaku. Seluruh angkanya adalah batas waktu
    /// dalam menit atau jam, dan dapat diubah admin tanpa satu baris kode pun disentuh.
    /// </summary>
    public class InpatientSettingResponse
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        /// <summary>Lama pemesanan tempat tidur mengunci, dalam menit. Bawaan 120 menit.</summary>
        public int BedReservationMinutes { get; set; }

        /// <summary>Lama episode Draft boleh telantar sebelum gugur, dalam jam. Bawaan 24 jam.</summary>
        public int DraftEpisodeExpiryHours { get; set; }

        /// <summary>Target penyelesaian pengkajian awal, dalam jam. Belum final secara klinis.</summary>
        public int InitialAssessmentTargetHours { get; set; }

        /// <summary>Target verifikasi catatan perkembangan, dalam jam. Belum final secara klinis.</summary>
        public int ProgressNoteVerificationTargetHours { get; set; }

        /// <summary>Ambang episode dianggap tertahan menunggu penutupan, dalam jam. Bawaan 4 jam.</summary>
        public int PendingClosureThresholdHours { get; set; }

        /// <summary>Awalan nomor episode, misalnya <c>RI</c>.</summary>
        public string EpisodeNumberPrefix { get; set; } = string.Empty;

        public bool IsDefault { get; set; }

        public bool IsActive { get; set; }

        public string? Notes { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }
    }

    /// <summary>
    /// Bentuk permintaan mengubah pengaturan Rawat Inap.
    /// </summary>
    /// <remarks>
    /// Batas bawah dan atas setiap angka dijaga <c>[Range]</c>. Batas atas bukan hiasan:
    /// pemesanan yang mengunci 100.000 menit berarti tempat tidur tertahan lebih dari dua
    /// bulan tanpa ada pasien di atasnya, dan tidak ada layar yang menampilkan hal itu
    /// sebagai kesalahan.
    /// </remarks>
    public class UpdateInpatientSettingRequest
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        /// <summary>Antara 1 menit dan 1440 menit, yaitu paling lama satu hari penuh.</summary>
        [Range(1, 1440)]
        public int BedReservationMinutes { get; set; }

        /// <summary>Antara 1 jam dan 720 jam, yaitu paling lama 30 hari.</summary>
        [Range(1, 720)]
        public int DraftEpisodeExpiryHours { get; set; }

        [Range(1, 720)]
        public int InitialAssessmentTargetHours { get; set; }

        [Range(1, 720)]
        public int ProgressNoteVerificationTargetHours { get; set; }

        [Range(1, 720)]
        public int PendingClosureThresholdHours { get; set; }

        [Required]
        [MaxLength(20)]
        public string EpisodeNumberPrefix { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
