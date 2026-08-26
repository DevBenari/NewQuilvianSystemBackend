using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>Satu butir daftar periksa administrasi beserta status penandaannya.</summary>
    /// <remarks>
    /// <b>Butir yang sudah tidak aktif tetap muncul bila episode ini pernah menandainya.</b>
    /// Menghilangkannya akan membuat penandaan lama seolah tidak pernah terjadi — dan pada
    /// episode yang diaudit setahun kemudian, hilangnya jejak itu tidak dapat dijelaskan
    /// siapa pun.
    /// </remarks>
    public class ClearanceChecklistItemResponse
    {
        public Guid ItemId { get; set; }

        public string ItemCode { get; set; } = string.Empty;

        public string ItemName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsMandatory { get; set; }

        /// <summary>
        /// Benar bila butirnya masih aktif pada master. Butir yang dinonaktifkan admin
        /// <b>tidak lagi menahan</b> penutupan, tetapi penandaan lamanya tetap terbaca.
        /// </summary>
        public bool IsActive { get; set; }

        public int SortOrder { get; set; }

        public bool IsMarked { get; set; }

        public DateTime? MarkedAt { get; set; }

        public Guid? MarkedByUserId { get; set; }

        public string? Note { get; set; }

        /// <summary>
        /// Benar bila butir ini sedang menahan penutupan episode: wajib, masih aktif, dan
        /// belum ditandai.
        /// </summary>
        public bool IsBlocking { get; set; }
    }

    /// <summary>Daftar periksa administrasi satu episode.</summary>
    public class ClearanceChecklistResponse
    {
        public Guid EpisodeId { get; set; }

        public string? EpisodeNumber { get; set; }

        public int TotalItem { get; set; }

        public int TotalMarked { get; set; }

        /// <summary>Jumlah butir wajib dan aktif yang belum ditandai.</summary>
        public int TotalBlocking { get; set; }

        public List<ClearanceChecklistItemResponse> Items { get; set; } = new();
    }

    /// <summary>Bentuk permintaan menandai satu butir daftar periksa administrasi.</summary>
    public class MarkClearanceItemRequest
    {
        [MaxLength(500)]
        public string? Note { get; set; }
    }

    /// <summary>
    /// Bentuk permintaan menandai kelayakan keuangan.
    /// </summary>
    /// <remarks>
    /// <b>Catatan wajib diisi</b> — <c>RWI-RULE-028</c> aturan 4. Penandaan tanpa catatan
    /// membuat riwayat kelayakan keuangan berisi deretan perubahan tanpa satu pun alasan, dan
    /// pada saat sengketa tagihan tidak ada yang dapat menjelaskan kenapa nilainya berubah.
    /// </remarks>
    public class MarkFinancialClearanceRequest
    {
        /// <summary>0 <c>Pending</c>, 1 <c>Cleared</c>, 2 <c>Blocked</c>.</summary>
        public int ClearanceStatus { get; set; }

        [Required]
        [MaxLength(500)]
        public string Note { get; set; } = string.Empty;
    }

    /// <summary>Satu baris riwayat kelayakan keuangan.</summary>
    public class FinancialClearanceEntryResponse
    {
        public Guid Id { get; set; }

        public int SequenceNumber { get; set; }

        public int ClearanceStatus { get; set; }

        public string ClearanceStatusName { get; set; } = string.Empty;

        public DateTime MarkedAt { get; set; }

        public Guid MarkedByUserId { get; set; }

        public string Note { get; set; } = string.Empty;

        /// <summary>
        /// Selalu benar selama MVP. Wajib ditampilkan pada layar dan laporan supaya pembacanya
        /// tahu angkanya berasal dari penilaian orang, bukan dari tagihan yang dihitung sistem
        /// — <c>RWI-RULE-028</c> dan `RWI-RISK-003`.
        /// </summary>
        public bool IsManualMarking { get; set; }
    }

    /// <summary>Kelayakan keuangan satu episode beserta riwayat penandaannya.</summary>
    public class FinancialClearanceResponse
    {
        public Guid EpisodeId { get; set; }

        public string? EpisodeNumber { get; set; }

        /// <summary>Nilai yang berlaku, yaitu penandaan terakhir. <c>Pending</c> bila belum pernah ditandai.</summary>
        public int CurrentStatus { get; set; }

        public string CurrentStatusName { get; set; } = string.Empty;

        public bool IsCleared { get; set; }

        public List<FinancialClearanceEntryResponse> History { get; set; } = new();
    }

    /// <summary>Satu syarat penutupan beserta keadaannya.</summary>
    /// <remarks>
    /// Bentuk daftar ini adalah <b>kontrak, bukan preferensi</b>. Jawaban berupa boolean
    /// tunggal membuat layar hanya dapat mematikan tombol tutup tanpa dapat memberi tahu
    /// petugas apa yang harus dikejar — dan petugas menebak.
    /// </remarks>
    public class ClosureConditionResponse
    {
        /// <summary>Nomor syarat, 1 sampai 5, mengikuti urutan <c>RWI-RULE-010</c>.</summary>
        public int Number { get; set; }

        /// <summary>Penanda syarat yang tetap sama walau kalimatnya diperbaiki.</summary>
        public string Code { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public bool IsSatisfied { get; set; }

        /// <summary>Kalimat yang dibaca petugas bila syarat ini belum terpenuhi.</summary>
        public string? UnmetMessage { get; set; }

        /// <summary>
        /// Benar bila syarat ini dapat ditembus supervisor lewat
        /// <c>POST .../close-with-override</c>. Hanya syarat kelayakan keuangan yang bernilai
        /// benar.
        /// </summary>
        public bool CanBeOverridden { get; set; }
    }

    /// <summary>Kesiapan penutupan satu episode: kelima syarat beserta keadaannya.</summary>
    public class ClosureReadinessResponse
    {
        public Guid EpisodeId { get; set; }

        public string? EpisodeNumber { get; set; }

        public int EpisodeStatus { get; set; }

        public string EpisodeStatusName { get; set; } = string.Empty;

        /// <summary>Benar bila kelima syarat terpenuhi.</summary>
        public bool IsReady { get; set; }

        /// <summary>
        /// Benar bila seluruh syarat selain kelayakan keuangan terpenuhi, sehingga supervisor
        /// dapat menutup lewat jalan keluar.
        /// </summary>
        public bool IsReadyWithOverride { get; set; }

        public List<ClosureConditionResponse> Conditions { get; set; } = new();
    }

    /// <summary>Bentuk permintaan menutup episode.</summary>
    public class CloseEpisodeRequest
    {
        [MaxLength(500)]
        public string? Note { get; set; }
    }

    /// <summary>
    /// Bentuk permintaan supervisor menutup episode menembus gerbang keuangan.
    /// </summary>
    /// <remarks>
    /// Alasannya wajib, dan ia tersimpan pada episode beserta baris riwayat statusnya. Jalan
    /// keluar yang tidak meninggalkan jejak akan menjadi jalur normal dalam hitungan minggu.
    /// </remarks>
    public class CloseEpisodeOverrideRequest
    {
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Bentuk permintaan mencatat pasien sudah meninggalkan ruangan.
    /// </summary>
    /// <remarks>
    /// <b>Ini fakta, bukan izin.</b> Sistem tidak memeriksa apakah butir administrasi atau
    /// kelayakan keuangan sudah selesai — pasien yang sudah pulang tetap harus dicatat pulang
    /// walaupun administrasinya belum beres. Episode tetap <c>DischargePending</c> dan tetap
    /// muncul pada daftar pantau penutupan tertunda.
    /// </remarks>
    public class RecordDepartureRequest
    {
        /// <summary>
        /// Waktu pasien meninggalkan ruangan. Dikosongkan berarti sekarang. Tidak boleh
        /// melewati waktu sekarang, dan tidak boleh mendahului keputusan pulang.
        /// </summary>
        public DateTime? DepartedAt { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
