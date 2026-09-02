namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;

/// <summary>
/// Daur hidup satu permintaan stok barang atau obat.
/// </summary>
/// <remarks>
/// <c>Draft</c> adalah satu-satunya keadaan yang isinya masih boleh diubah. Begitu
/// permintaan dikirim, gudang sudah melihatnya dan mungkin sudah mulai menyiapkan;
/// mengubah isinya diam-diam membuat yang disiapkan tidak lagi cocok dengan yang
/// diminta.
/// </remarks>
public enum StockRequestStatus
{
    /// <summary>Masih disusun peminta; boleh diubah dan dihapus barisnya.</summary>
    Draft = 1,

    /// <summary>Sudah dikirim ke gudang; isinya tidak boleh diubah lagi.</summary>
    Submitted = 2,

    /// <summary>Disetujui gudang, menunggu penyerahan barang.</summary>
    Approved = 3,

    /// <summary>Barang sudah diserahkan seluruhnya.</summary>
    Completed = 4,

    /// <summary>Ditolak gudang; alasan wajib tercatat.</summary>
    Rejected = 5,

    /// <summary>Dibatalkan peminta sebelum diserahkan.</summary>
    Cancelled = 6
}

/// <summary>Kesegeraan permintaan, menentukan urutan pengerjaan di gudang.</summary>
public enum StockRequestPriority
{
    Routine = 1,

    /// <summary>Dibutuhkan lebih cepat dari antrean biasa.</summary>
    Urgent = 2,

    /// <summary>Untuk keadaan gawat darurat; menyalip seluruh antrean.</summary>
    Emergency = 3
}
