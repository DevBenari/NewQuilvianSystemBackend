using Microsoft.Extensions.Options;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Options;

public sealed class BedManagementOptionsValidator : IValidateOptions<BedManagementOptions>
{
    private static readonly HashSet<string> Operations = new(
        new[] {
            "InpatientBedManagement:Read", "InpatientBedManagement:ReadUsageHistory",
            "InpatientBedManagement:StartCleaning", "InpatientBedManagement:CompleteCleaning", "InpatientBedManagement:VerifyReadiness",
            "InpatientBedOccupancy:Read", "InpatientBedOccupancy:Create", "InpatientBedOccupancy:Update", "InpatientBedOccupancy:Transfer", "InpatientBedOccupancy:Correct",
            "InpatientDischarge:RecordDeparture", "InpatientEpisode:Create",
            "Bed:Read", "Bed:Create", "Bed:Update", "Bed:Delete", "Room:Read", "Room:Update", "Room:Delete",
            "ServiceUnit:Read", "ServiceUnit:Update", "ServiceUnit:Delete", "PatientClass:Read", "PatientClass:Update", "PatientClass:Delete"
        }, StringComparer.Ordinal);
    public ValidateOptionsResult Validate(string? name, BedManagementOptions options)
    {
        var errors = new List<string>();
        if (options.ClassOrder.Direction.HasValue && !Enum.IsDefined(options.ClassOrder.Direction.Value)) errors.Add("Direction tidak dikenal.");
        if (options.ClassOrder.Direction.HasValue && (string.IsNullOrWhiteSpace(options.ClassOrder.EvidenceReference) ||
            options.ClassOrder.ApprovedSourceDigest?.Length != 64 || options.ClassOrder.ApprovedSourceDigest.Any(x => !Uri.IsHexDigit(x))))
            errors.Add("ClassOrder memerlukan referensi dan SHA256 persetujuan yang sah.");
        foreach (var a in options.UnitAssignments)
        {
            if (a.UserId == Guid.Empty || a.ServiceUnitId == Guid.Empty || string.IsNullOrWhiteSpace(a.AssignmentReference) ||
                a.AssignmentReference.Length > 200 || a.SopReference?.Length > 200 || a.Operations.Any(x => !Operations.Contains(x)))
                errors.Add("Penugasan harus memakai akun, unit, referensi dan operation canonical.");
            if (a.OperationalOnly && (a.IsReadinessVerifier || a.Operations.Any(x => x is "InpatientBedManagement:ReadUsageHistory" or "InpatientBedOccupancy:Transfer" or "InpatientBedManagement:VerifyReadiness")))
                errors.Add("Profil OperationalOnly tidak boleh mendapat history pasien, transfer atau verifikasi.");
            if (a.Operations.Contains("InpatientBedManagement:VerifyReadiness") && !a.IsReadinessVerifier)
                errors.Add("VerifyReadiness memerlukan penunjukan verifier.");
        }
        if (options.ReservationExpiryActorUserId.HasValue && (options.ReservationExpiryActorUserId == Guid.Empty ||
            string.IsNullOrWhiteSpace(options.ReservationExpiryAssignmentReference) || options.ReservationExpiryAssignmentReference.Length > 200))
            errors.Add("Actor expiry harus mempunyai bukti penugasan sistem.");
        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}

