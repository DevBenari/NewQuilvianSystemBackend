namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;

/// <summary>Status asuhan gizi pada satu episode rawat inap.</summary>
public enum GzOrderStatus
{
    Requested = 1,
    InProgress = 2,
    Closed = 3,
    Cancelled = 4
}

/// <summary>Kesegeraan penanganan yang diminta dokter.</summary>
public enum GzOrderPriority
{
    Routine = 1,
    Urgent = 2
}

/// <summary>Membedakan kunjungan pertama dari kunjungan lanjutan.</summary>
public enum GzCareRecordType
{
    Initial = 1,
    FollowUp = 2
}

/// <summary>Keadaan satu diet pasien.</summary>
public enum GzPatientDietStatus
{
    /// <summary>Sedang berlaku; inilah yang dibaca dapur.</summary>
    Active = 1,

    /// <summary>Diganti diet lain. Barisnya tetap disimpan sebagai riwayat.</summary>
    Changed = 2,

    /// <summary>Dihentikan tanpa pengganti, misalnya pasien puasa atau pulang.</summary>
    Stopped = 3
}

/// <summary>Hasil penyerahan makanan pada satu jadwal makan.</summary>
public enum GzMealDeliveryStatus
{
    Delivered = 1,

    /// <summary>Pasien menolak. Dibedakan dari tidak tersaji agar terlihat di evaluasi asupan.</summary>
    Refused = 2,

    /// <summary>Tidak tersaji karena alasan pelayanan, misalnya pasien sedang tindakan.</summary>
    NotServed = 3
}
