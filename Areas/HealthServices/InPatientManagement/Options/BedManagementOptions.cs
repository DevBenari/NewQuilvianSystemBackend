namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Options;

public class BedManagementOptions
{
    public bool EnableWorkflow { get; set; } public Guid? ReservationExpiryActorUserId { get; set; } public string? ReservationExpiryAssignmentReference { get; set; }
    public BedClassOrderOptions ClassOrder { get; set; } = new();
    public List<BedUnitAssignment> UnitAssignments { get; set; } = new();
    /// <summary>Kesiapan bed hasil impor denah per lingkungan (BE-RWI-237, RWI-DEC-321).</summary>
    public BedFloorPlanImportOptions FloorPlanImport { get; set; } = new();
}

/// <summary>
/// Penetapan siap bed hasil impor hanya untuk dev/UAT. Bawaan mati; aplikasi menolak menyala bila
/// opsi aktif di lingkungan <c>Production</c> atau tanpa referensi bukti (INV-DNH-07, VAL-DNH-18).
/// </summary>
public class BedFloorPlanImportOptions
{
    public bool MarkImportedBedsReady { get; set; } = false;
    public string? ReadyEvidenceReference { get; set; }
}

public enum BedClassOrderDirection { HigherValueIsHigherGrade = 1, LowerValueIsHigherGrade = 2 }
public class BedClassOrderOptions
{
    public BedClassOrderDirection? Direction { get; set; }
    public string? EvidenceReference { get; set; }
    public string? ApprovedSourceDigest { get; set; }
    public List<BedClassEquivalence> EquivalencePairs { get; set; } = new();
}
public class BedClassEquivalence
{
    public Guid ClassIdA { get; set; }
    public Guid ClassIdB { get; set; }
}
public class BedUnitAssignment
{
    public Guid UserId { get; set; }
    public Guid ServiceUnitId { get; set; }
    public string[] Operations { get; set; } = Array.Empty<string>();
    public string AssignmentReference { get; set; } = string.Empty;
    public string? SopReference { get; set; }
    public bool OperationalOnly { get; set; }
    public bool IsReadinessVerifier { get; set; }
}
