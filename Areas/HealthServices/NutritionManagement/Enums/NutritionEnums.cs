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
