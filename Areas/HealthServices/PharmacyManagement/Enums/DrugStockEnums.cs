namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;

/// <summary>
/// Keadaan fisik sekumpulan stok pada satu batch di satu lokasi.
/// </summary>
/// <remarks>
/// <para>
/// Status ini melekat pada barangnya, bukan pada niat pemakaiannya. Karena itu satu batch di
/// satu lokasi dapat memiliki beberapa baris saldo sekaligus: sebagian <c>Available</c> dan
/// sebagian <c>Quarantine</c>. Memindahkan barang antar status adalah mutasi tersendiri yang
/// wajib menyimpan alasan dan pelakunya.
/// </para>
/// <para>
/// <c>Reserved</c> sengaja TIDAK menjadi status di sini. Reservasi adalah penahanan lunak atas
/// stok yang tetap berada di rak dan tetap milik status <c>Available</c>; ia dicatat sebagai
/// kolom jumlah, mengikuti rumus yang disepakati: OnHand − Reserved = Available.
/// </para>
/// </remarks>
public enum DrugStockStatus
{
    /// <summary>Siap dipakai. Hanya status ini yang boleh melayani dispensing dan transfer.</summary>
    Available = 1,

    /// <summary>Ditahan dan tidak boleh dipakai; tetap tercatat dan dapat ditelusuri alasannya.</summary>
    Quarantine = 2,

    /// <summary>Sudah lewat tanggal kedaluwarsa dan ditarik dari peredaran.</summary>
    Expired = 3,

    /// <summary>Rusak; tetap tercatat sampai dimusnahkan agar selisihnya tidak hilang diam-diam.</summary>
    Damaged = 4
}

/// <summary>
/// Jenis pergerakan pada kartu stok.
/// </summary>
/// <remarks>
/// Jenis ini menjawab "arah dan sifat pergerakan", bukan "dokumen apa yang menyebabkannya".
/// Dokumen asalnya dicatat terpisah pada <c>SourceDocumentType</c> dan <c>SourceDocumentId</c>,
/// supaya menambah jenis dokumen baru tidak memaksa enum ini ikut tumbuh.
/// </remarks>
public enum DrugStockMutationType
{
    /// <summary>Saldo pembuka saat stok pertama kali dicatat sistem.</summary>
    OpeningBalance = 1,

    /// <summary>Stok bertambah, misalnya dari penerimaan barang atau transfer masuk.</summary>
    StockIn = 2,

    /// <summary>Stok berkurang, misalnya dari penyerahan, pemakaian, atau transfer keluar.</summary>
    StockOut = 3,

    /// <summary>
    /// Penyesuaian saldo di luar transaksi normal, misalnya hasil stock opname atau koreksi.
    /// Alasan wajib diisi.
    /// </summary>
    /// <remarks>
    /// Penambahan stok dari pembelian TIDAK boleh memakai jenis ini; pembelian wajib melalui
    /// penerimaan barang resmi sebagai <see cref="StockIn"/>.
    /// </remarks>
    Adjustment = 4,

    /// <summary>Perpindahan antar status pada batch dan lokasi yang sama, misalnya ke karantina.</summary>
    StatusChange = 5
}

/// <summary>
/// Dokumen yang menyebabkan sebuah mutasi stok.
/// </summary>
/// <remarks>
/// Disimpan sebagai teks pendek dan bukan foreign key, karena dokumen asalnya tersebar di
/// beberapa tabel yang berbeda bentuk. Penelusuran memakai pasangan
/// <c>SourceDocumentType</c> + <c>SourceDocumentId</c>.
/// </remarks>
public static class DrugStockSourceDocumentTypes
{
    public const string StockRequest = "StockRequest";
    public const string GoodsReceipt = "GoodsReceipt";
    public const string StockTransfer = "StockTransfer";
    public const string DrugUsage = "DrugUsage";
    public const string PrescriptionReturn = "PrescriptionReturn";
    public const string StockOpname = "StockOpname";
    public const string ManualAdjustment = "ManualAdjustment";
}

/// <summary>
/// Daur hidup satu transfer stok antar lokasi.
/// </summary>
/// <remarks>
/// <para>
/// Barang berpindah dalam dua langkah terpisah — dikeluarkan dari lokasi asal, lalu diterima
/// di lokasi tujuan — karena di antara keduanya barang sedang di jalan dan tidak berada di
/// mana pun. Menggabungkan keduanya menjadi satu langkah membuat stok seolah berpindah
/// seketika, dan selisih saat barang hilang dalam perjalanan tidak akan pernah terlihat.
/// </para>
/// <para>
/// Reservasi terjadi saat persetujuan, bukan saat pengiriman. Dengan begitu stok yang sudah
/// dijanjikan kepada satu transfer tidak dapat diambil proses lain selagi menunggu petugas
/// menyiapkan barangnya.
/// </para>
/// </remarks>
public enum StockTransferStatus
{
    /// <summary>Masih disusun peminta; boleh diubah.</summary>
    Draft = 1,

    /// <summary>Sudah diajukan, menunggu persetujuan. Stok belum ditahan.</summary>
    Requested = 2,

    /// <summary>Disetujui dan stok di lokasi asal sudah ditahan.</summary>
    Approved = 3,

    /// <summary>Barang sudah keluar dari lokasi asal dan belum sampai di tujuan.</summary>
    InTransit = 4,

    /// <summary>Barang sudah diterima lokasi tujuan; saldo kedua sisi sudah terbarui.</summary>
    Completed = 5,

    /// <summary>Ditolak saat persetujuan; alasan wajib tercatat.</summary>
    Rejected = 6,

    /// <summary>Dibatalkan sebelum barang keluar; reservasinya dilepas.</summary>
    Cancelled = 7
}

/// <summary>
/// Daur hidup satu pencatatan pemakaian obat untuk pasien.
/// </summary>
/// <remarks>
/// <para>
/// Pemakaian tidak langsung menjadi tagihan. Ia berhenti di <c>NotBilled</c> sebagai transaksi
/// yang <em>dapat</em> ditagihkan; keputusan menagih beserta aturannya milik modul Billing,
/// bukan Farmasi.
/// </para>
/// <para>
/// <c>Billed</c> belum dapat dicapai sistem ini. Perpindahan ke sana memerlukan kontrak
/// integrasi Billing yang belum ditetapkan — khususnya sumber status pembayaran yang sah dan
/// cara menangani pengulangan panggilan. Nilainya sudah disediakan supaya kelak tidak perlu
/// mengubah bentuk tabel yang sudah berisi transaksi.
/// </para>
/// </remarks>
public enum DrugUsageStatus
{
    /// <summary>Masih disusun; belum menyentuh stok dan masih boleh diubah.</summary>
    Draft = 1,

    /// <summary>Sudah dicatat: stok berkurang, dan transaksinya menunggu ditagihkan.</summary>
    NotBilled = 2,

    /// <summary>Sudah ditagihkan Billing. Belum dapat dicapai; menunggu kontrak integrasi.</summary>
    Billed = 3,

    /// <summary>Dibatalkan sebelum dicatat; tidak menyentuh stok sama sekali.</summary>
    Cancelled = 4
}

/// <summary>
/// Daur hidup satu retur obat.
/// </summary>
/// <remarks>
/// Stok tidak bertambah pada saat retur diajukan, melainkan setelah diperiksa. Obat yang
/// kembali sudah pernah keluar dari pengawasan farmasi — suhunya, kemasannya, dan keasliannya
/// tidak lagi dijamin. Memasukkannya kembali tanpa pemeriksaan berarti menaruh barang yang
/// belum tentu layak ke rak yang sama dengan barang yang layak.
/// </remarks>
public enum DrugReturnStatus
{
    /// <summary>Masih disusun; belum diajukan dan belum menyentuh stok.</summary>
    Draft = 1,

    /// <summary>Sudah diajukan dan menunggu pemeriksaan.</summary>
    Submitted = 2,

    /// <summary>Sudah diperiksa dan diterima; stok bertambah sesuai keadaan barangnya.</summary>
    Verified = 3,

    /// <summary>Ditolak pemeriksa; alasan wajib tercatat dan stok tidak bertambah.</summary>
    Rejected = 4,

    /// <summary>Dibatalkan pengaju sebelum diperiksa.</summary>
    Cancelled = 5
}
