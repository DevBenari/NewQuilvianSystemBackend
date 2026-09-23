using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Responses;

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

        /// <summary>
        /// Peringatan yang perlu diketahui petugas sebelum menutup, dan yang <b>tidak</b>
        /// mempengaruhi <see cref="IsReady"/> maupun <see cref="IsReadyWithOverride"/> sama
        /// sekali. Ditambahkan <c>BE-RWI-084</c> untuk <c>FR-RI-201</c>.
        /// </summary>
        /// <remarks>
        /// <b>Daftar ini tidak pernah boleh dibaca sebagai syarat.</b> Seluruh
        /// <c>VAL-INP-13</c> sampai <c>VAL-INP-17</c> bersifat peringatan — roadmap
        /// <c>BE-RWI-084</c> acceptance criteria 2. Layar yang mematikan tombol tutup karena
        /// daftar ini tidak kosong sedang menegakkan aturan yang sengaja tidak dibuat.
        /// </remarks>
        public List<ClosureWarningResponse> Warnings { get; set; } = new();
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

    /// <summary>
    /// Satu peringatan penutupan episode: keadaan yang perlu diketahui petugas sebelum menekan
    /// tutup, tetapi <b>tidak</b> menahan penutupannya. <c>BE-RWI-084</c>.
    /// </summary>
    /// <remarks>
    /// <b>Bentuknya sengaja berbeda dari <see cref="ClosureConditionResponse"/>.</b> Syarat
    /// punya <c>IsSatisfied</c> dan menahan; peringatan punya <c>Count</c> dan tidak pernah
    /// menahan. Memakai satu bentuk untuk keduanya membuat layar hanya perlu salah membaca satu
    /// field untuk mengubah peringatan menjadi penghalang — dan pasien yang sudah pulang
    /// tertahan di sistem karena sebuah konsep catatan yang belum ditandatangani.
    /// </remarks>
    public class ClosureWarningResponse
    {
        /// <summary>Penanda peringatan; lihat <c>ClosureWarningCode</c>.</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>Banyaknya hal yang terdampak.</summary>
        public int Count { get; set; }

        /// <summary>Kalimat sebagaimana dibaca petugas di layar.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Rincian singkat per butir, bila ada. Tidak pernah memuat isi klinis — permission
        /// matrix bagian 5.4.
        /// </summary>
        public List<string> Details { get; set; } = new();

        /// <summary>
        /// Benar bila angkanya benar-benar dihitung dari sumbernya. Bernilai salah ketika sumber
        /// yang bersangkutan belum tersedia pada repository ini; <see cref="Count"/> kemudian
        /// bernilai <c>0</c> karena <b>belum terbaca</b>, bukan karena tidak ada.
        /// </summary>
        /// <remarks>
        /// Perbedaan ini wajib terlihat. Angka nol yang berarti "tidak ada" dan angka nol yang
        /// berarti "belum dapat dibaca" akan menuntun petugas pada dua keputusan yang berbeda,
        /// dan layar tidak punya cara membedakannya tanpa field ini.
        /// </remarks>
        public bool IsMeasured { get; set; } = true;
    }

    /// <summary>
    /// Akibat penutupan episode yang <b>benar-benar tersimpan</b>, bukan yang diperkirakan
    /// sebelumnya. <c>BE-RWI-084</c>.
    /// </summary>
    /// <remarks>
    /// <b>Kenapa angkanya diambil dari hasil, bukan dari perkiraan.</b> Antara layar
    /// menampilkan peringatan dan petugas menekan tutup, seorang dokter dapat menandatangani
    /// konsepnya. Menyalin angka perkiraan ke dalam ringkasan akibat akan membuat layar
    /// melaporkan sebuah penguncian yang tidak pernah terjadi — roadmap <c>BE-RWI-084</c>
    /// acceptance criteria 3.
    /// </remarks>
    public class ClosureSideEffectsResponse
    {
        /// <summary>Konsep catatan dokter yang terkunci menjadi "Tidak Ditandatangani".</summary>
        public int LockedDraftCount { get; set; }

        /// <summary>Pesanan tindakan tertunda belum ditagih yang dibatalkan.</summary>
        public int CancelledProcedureOrderCount { get; set; }

        /// <summary>
        /// Pesanan tindakan tertunda <b>sudah ditagih</b> yang sengaja <b>tidak</b> dibatalkan
        /// dan kini menunggu tindak lanjut bersama Billing.
        /// </summary>
        public int BilledPendingProcedureOrderCount { get; set; }

        /// <summary>Dosis obat berjadwal setelah waktu tutup yang dibatalkan.</summary>
        public int CancelledFutureDoseCount { get; set; }

        /// <summary>
        /// Langkah penutupan yang <b>belum terpasang</b> pada rilis ini beserta sebabnya. Dibaca
        /// layar supaya ringkasan akibat tidak menyatakan "0 pesanan dibatalkan" untuk langkah
        /// yang sebenarnya belum berjalan sama sekali.
        /// </summary>
        public List<string> NotYetWiredSteps { get; set; } = new();
    }

    /// <summary>Penyaring daftar pantau pesanan tindakan tertagih yang tidak dibatalkan.</summary>
    public class BilledPendingProcedureOrderQuery
    {
        public Guid? ServiceUnitId { get; set; }

        /// <summary>Batas bawah waktu penutupan episode.</summary>
        public DateTime? ClosedFrom { get; set; }

        /// <summary>Batas atas waktu penutupan episode.</summary>
        public DateTime? ClosedTo { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Satu pesanan tindakan yang masih tertunda, sudah ditagih, dan episodenya sudah ditutup.
    /// <c>BE-RWI-083</c>.
    /// </summary>
    /// <remarks>
    /// <b>Daftar ini ada justru karena pesanannya sengaja tidak disentuh.</b> Membatalkan
    /// pesanan yang uangnya sudah masuk tagihan berarti menghapus dasar sebuah tagihan tanpa ada
    /// yang memutuskannya — <c>RWI-DEC-143</c> (c). Yang dilakukan sistem adalah
    /// <b>memunculkannya</b>, supaya ada orang yang menindaklanjutinya bersama Billing. Apa
    /// tindak lanjut itu belum diputuskan; <c>04-prd-to-mvp.md</c> 22.7 nomor 1 masih terbuka.
    /// </remarks>
    public class BilledPendingProcedureOrderItem
    {
        public Guid ProcedureId { get; set; }

        public string? ProcedureCode { get; set; }

        public string? ProcedureName { get; set; }

        public int ProcedureStatus { get; set; }

        public string ProcedureStatusName { get; set; } = string.Empty;

        public Guid EpisodeId { get; set; }

        public string? EpisodeNumber { get; set; }

        public Guid PatientId { get; set; }

        public string? PatientName { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public string? ServiceUnitName { get; set; }

        /// <summary>Dokter yang memesan tindakan ini.</summary>
        public Guid OrderedByDoctorId { get; set; }

        public string? OrderedByDoctorName { get; set; }

        public DateTime OrderedAt { get; set; }

        public DateTime? EpisodeClosedAt { get; set; }

        /// <summary>Butir tagihan yang sudah terbentuk untuk pesanan ini, bila ada.</summary>
        public Guid? BillingItemId { get; set; }

        public DateTime? BillingGeneratedAt { get; set; }
    }

    /// <summary>Daftar pantau pesanan tindakan tertagih, bertingkat.</summary>
    public class BilledPendingProcedureOrderPagedResult : PagedResult<BilledPendingProcedureOrderItem>
    {
    }
}
