using QuilvianSystemBackend.Responses;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>
    /// Penyaring pencarian tempat tidur yang benar-benar dapat ditempati.
    /// </summary>
    /// <remarks>
    /// <b><c>EpisodeId</c> mengubah arti pencarian.</b> Tanpa <c>EpisodeId</c>, hasilnya
    /// adalah tempat tidur yang tidak sedang dipegang siapa pun. Dengan <c>EpisodeId</c>,
    /// hasilnya disaring memakai <b>seluruh</b> aturan Kelayakan Penempatan milik episode
    /// tersebut — jenis kelamin pasien dan kebutuhan isolasinya ikut diperhitungkan.
    ///
    /// <para>
    /// Layar penempatan wajib mengirim <c>EpisodeId</c>. Bila tidak, petugas akan melihat
    /// tempat tidur yang tampak kosong lalu ditolak 422 saat menekan simpan — penyaring dan
    /// penolak memberi jawaban berbeda, dan itulah cacat yang dijaga <c>BE-RWI-013</c>.
    /// </para>
    /// </remarks>
    public class AvailableBedQuery
    {
        public Guid? EpisodeId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? RoomId { get; set; }

        public Guid? PatientClassId { get; set; }

        /// <summary>Kata kunci kode atau nama tempat tidur, dan nama kamar.</summary>
        public string? Search { get; set; }

        public bool? IsIsolationBed { get; set; }

        public bool? IsForNewborn { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    /// <summary>Satu tempat tidur yang lolos pemeriksaan kelayakan.</summary>
    public class AvailableBedResponse
    {
        public Guid BedId { get; set; }

        public string BedCode { get; set; } = string.Empty;

        public string BedName { get; set; } = string.Empty;

        public string? BedNumber { get; set; }

        public Guid RoomId { get; set; }

        public string? RoomCode { get; set; }

        public string? RoomName { get; set; }

        public Guid ServiceUnitId { get; set; }

        public string? ServiceUnitName { get; set; }

        public Guid? PatientClassId { get; set; }

        public string? PatientClassName { get; set; }

        public int BedStatus { get; set; }

        public string BedStatusName { get; set; } = string.Empty;

        public bool IsForMale { get; set; }

        public bool IsForFemale { get; set; }

        public bool IsForNewborn { get; set; }

        public bool IsIsolationBed { get; set; }

        public bool IsReservable { get; set; }
    }

    /// <summary>Daftar tempat tidur yang dapat ditempati, bertingkat.</summary>
    public class AvailableBedPagedResult : PagedResult<AvailableBedResponse>
    {
    }

    /// <summary>
    /// Papan ketersediaan tempat tidur, dikelompokkan per unit layanan lalu per kamar.
    /// </summary>
    public class BedBoardResponse
    {
        public int TotalBed { get; set; }

        public int TotalAvailable { get; set; }

        public int TotalOccupied { get; set; }

        public int TotalReserved { get; set; }

        /// <summary>Tempat tidur yang sedang ditutup: pembersihan, perbaikan, atau diblokir.</summary>
        public int TotalUnavailable { get; set; }

        public List<BedBoardServiceUnitResponse> ServiceUnits { get; set; } = new();
    }

    /// <summary>Satu unit layanan pada papan ketersediaan.</summary>
    public class BedBoardServiceUnitResponse
    {
        public Guid ServiceUnitId { get; set; }

        public string? ServiceUnitName { get; set; }

        public int TotalBed { get; set; }

        public int TotalAvailable { get; set; }

        public int TotalOccupied { get; set; }

        public int TotalReserved { get; set; }

        public int TotalUnavailable { get; set; }

        public List<BedBoardRoomResponse> Rooms { get; set; } = new();
    }

    /// <summary>Satu kamar pada papan ketersediaan.</summary>
    public class BedBoardRoomResponse
    {
        public Guid RoomId { get; set; }

        public string? RoomCode { get; set; }

        public string? RoomName { get; set; }

        public Guid? PatientClassId { get; set; }

        public string? PatientClassName { get; set; }

        public int Capacity { get; set; }

        public List<BedBoardBedResponse> Beds { get; set; } = new();
    }

    /// <summary>
    /// Satu tempat tidur pada papan ketersediaan.
    /// </summary>
    /// <remarks>
    /// <b>Keadaan penghunian dibaca dari catatan penempatan, bukan dari salinan status.</b>
    /// Kolom <c>BedStatus</c> ikut ditampilkan supaya selisih antara salinan dan kenyataan
    /// terlihat, tetapi kolom <c>IsOccupied</c> dan <c>IsReserved</c>-lah yang benar.
    /// Dasarnya <c>RWI-DEC-039</c>.
    /// </remarks>
    public class BedBoardBedResponse
    {
        public Guid BedId { get; set; }

        public string BedCode { get; set; } = string.Empty;

        public string BedName { get; set; } = string.Empty;

        public int BedStatus { get; set; }

        public string BedStatusName { get; set; } = string.Empty;

        public bool IsOccupied { get; set; }

        public bool IsReserved { get; set; }

        public bool IsIsolationBed { get; set; }

        public bool IsForNewborn { get; set; }

        /// <summary>Nomor episode yang sedang memegang tempat tidur ini, bila memang ada.</summary>
        public string? HoldingEpisodeNumber { get; set; }

        /// <summary>
        /// Nama pasien yang sedang menempati. Papan ketersediaan dibaca peran ruangan yang
        /// memang berhak melihat census, sehingga nama pasien boleh tampil; isi klinis tidak.
        /// </summary>
        public string? PatientName { get; set; }
    }

    /// <summary>Bentuk permintaan memesan tempat tidur untuk satu episode <c>Draft</c>.</summary>
    public class ReserveBedRequest
    {
        public Guid EpisodeId { get; set; }

        public Guid BedId { get; set; }
    }

    /// <summary>Bentuk permintaan membatalkan pemesanan sebelum dipakai.</summary>
    public class CancelReservationRequest
    {
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    /// <summary>Satu pemesanan tempat tidur.</summary>
    public class BedReservationResponse
    {
        public Guid Id { get; set; }

        public Guid EpisodeId { get; set; }

        public string? EpisodeNumber { get; set; }

        public Guid BedId { get; set; }

        public string? BedCode { get; set; }

        public string? BedName { get; set; }

        public Guid? RoomId { get; set; }

        public string? RoomName { get; set; }

        public DateTime ReservedAt { get; set; }

        /// <summary>
        /// Waktu pemesanan gugur. Dihitung dari <c>MstInpatientSetting.BedReservationMinutes</c>
        /// yang berlaku pada saat pemesanan dibuat, bukan angka yang ditanam di kode.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        public int ReservationStatus { get; set; }

        public string ReservationStatusName { get; set; } = string.Empty;

        public Guid ReservedByUserId { get; set; }

        public DateTime? ReleasedAt { get; set; }
    }

    /// <summary>
    /// Bentuk permintaan menempatkan pasien ke tempat tidur.
    /// </summary>
    /// <remarks>
    /// <b>Tidak ada kolom waktu mulai.</b> Untuk jalur datang langsung dan poliklinik, waktu
    /// mulai penempatan adalah waktu penempatan dibuat (<c>RWI-AC-147</c>). Untuk jalur serah
    /// terima IGD ia dibaca dari catatan kepergian IGD dan tidak pernah ditetapkan pemanggil
    /// — jalur itu <c>INP-S09</c> yang di luar scope revisi ini.
    /// </remarks>
    public class PlacePatientRequest
    {
        public Guid EpisodeId { get; set; }

        public Guid BedId { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }

    /// <summary>Bentuk permintaan memindahkan pasien ke tempat tidur lain.</summary>
    public class TransferPatientRequest
    {
        public Guid EpisodeId { get; set; }

        /// <summary>Tempat tidur tujuan. Wajib berbeda dari tempat tidur yang ditempati sekarang.</summary>
        public Guid TargetBedId { get; set; }

        /// <summary>
        /// Alasan medis perpindahan. Wajib diisi — <c>RWI-RULE-007</c> menolak perpindahan
        /// tanpa alasan, dan tidak ada kolom keterangan lain yang dapat dipakai melewatinya.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string TransferReason { get; set; } = string.Empty;
    }

    /// <summary>Satu baris penempatan tempat tidur.</summary>
    public class BedPlacementResponse
    {
        public Guid Id { get; set; }

        public Guid EpisodeId { get; set; }

        public string? EpisodeNumber { get; set; }

        public Guid BedId { get; set; }

        public string? BedCode { get; set; }

        public string? BedName { get; set; }

        public Guid RoomId { get; set; }

        public string? RoomName { get; set; }

        public Guid ServiceUnitId { get; set; }

        public string? ServiceUnitName { get; set; }

        /// <summary>
        /// Kelas yang ditagihkan selama penempatan ini. Mengikuti kamar yang ditempati, bukan
        /// kelas yang dipilih saat admisi dibuka — <c>RWI-DEC-013</c>.
        /// </summary>
        public Guid PatientClassId { get; set; }

        public string? PatientClassName { get; set; }

        public int SequenceNumber { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        public int? EndReason { get; set; }

        public string? EndReasonName { get; set; }

        public string? TransferReason { get; set; }

        public Guid PlacedByUserId { get; set; }

        public Guid? EndedByUserId { get; set; }

        /// <summary>Benar bila penempatan ini yang sedang berlaku, yaitu belum ditutup.</summary>
        public bool IsCurrent { get; set; }
    }
}
