namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;

/// <summary>Status asuhan gizi pada satu episode rawat inap.</summary>
public enum GziOrderStatus
{
    Requested = 1,
    InProgress = 2,
    Closed = 3,
    Cancelled = 4
}

/// <summary>Kesegeraan penanganan yang diminta dokter.</summary>
public enum GziOrderPriority
{
    Routine = 1,
    Urgent = 2
}

/// <summary>Membedakan kunjungan pertama dari kunjungan lanjutan.</summary>
public enum GziCareRecordType
{
    Initial = 1,
    FollowUp = 2
}

/// <summary>Keadaan satu diet pasien.</summary>
public enum GziPatientDietStatus
{
    /// <summary>Sedang berlaku; inilah yang dibaca dapur.</summary>
    Active = 1,

    /// <summary>Diganti diet lain. Barisnya tetap disimpan sebagai riwayat.</summary>
    Changed = 2,

    /// <summary>Dihentikan tanpa pengganti, misalnya pasien puasa atau pulang.</summary>
    Stopped = 3
}

/// <summary>Hasil penyerahan makanan pada satu jadwal makan.</summary>
public enum GziMealDeliveryStatus
{
    Delivered = 1,

    /// <summary>Pasien menolak. Dibedakan dari tidak tersaji agar terlihat di evaluasi asupan.</summary>
    Refused = 2,

    /// <summary>Tidak tersaji karena alasan pelayanan, misalnya pasien sedang tindakan.</summary>
    NotServed = 3
}

/// <summary>
/// Daur hidup satu angkatan produksi makanan.
/// </summary>
/// <remarks>
/// Snapshot diambil saat batch berpindah dari <c>Draft</c> ke <c>Confirmed</c>. Sebelum
/// dikonfirmasi, isinya masih dapat dihitung ulang; sesudahnya tidak, karena dapur sudah
/// bekerja berdasarkan angka itu.
/// </remarks>
public enum GziProductionBatchStatus
{
    Draft = 1,
    Confirmed = 2,
    InProduction = 3,
    ReadyForDistribution = 4,
    Completed = 5,
    Cancelled = 6
}
