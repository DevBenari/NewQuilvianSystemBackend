using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;

/// <summary>
/// Perkakas bersama seluruh service modul operasi: identitas aktor, sidik jari idempotency,
/// pembuatan histori append-only, dan daftar tindakan lanjutan per status.
/// Disatukan supaya aturan `OPR012`/`OPR013` dan `availableActions` hanya punya satu definisi.
/// </summary>
internal static class OperatingRoomCommandSupport
{
    public const string LogCategory = "HealthServices.OperatingRoomManagement";

    public static Guid GetUserId(IHttpContextAccessor accessor)
    {
        var value = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            accessor.HttpContext?.User.FindFirstValue("user_id");
        if (!Guid.TryParse(value, out var id) || id == Guid.Empty)
            throw new OperatingRoomForbiddenException("Identitas pengguna tidak valid.");
        return id;
    }

    public static Guid? GetDoctorId(IHttpContextAccessor accessor)
    {
        var value = accessor.HttpContext?.User.FindFirstValue("doctor_id") ??
            accessor.HttpContext?.User.FindFirstValue("DoctorId");
        return Guid.TryParse(value, out var id) && id != Guid.Empty ? id : null;
    }

    public static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    public static string BuildSource(string fingerprint) => $"API:{fingerprint[..46]}";

    public static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static void EnsureSameFingerprint(string source, string fingerprint)
    {
        if (!string.Equals(source, BuildSource(fingerprint), StringComparison.Ordinal))
            throw new OperatingRoomConflictException("OPR013",
                "Idempotency key digunakan dengan isi permintaan yang berbeda.");
    }

    public static void EnsureSameCase(OprStatusHistory prior, Guid caseId)
    {
        if (prior.OprCaseId != caseId)
            throw new OperatingRoomConflictException("OPR013", "Idempotency key sudah digunakan untuk kasus lain.");
    }

    public static void EnsureVersion(int actual, int expected)
    {
        if (actual != expected)
            throw new OperatingRoomConflictException("OPR012",
                "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
    }

    public static OprStatusHistory NewHistory(Guid caseId, OprCaseStatus to, OprCaseStatus? from, string action,
        string? reason, string idempotencyKey, string fingerprint, Guid actorUserId, DateTime now) => new()
    {
        OprCaseId = caseId,
        FromStatus = from,
        ToStatus = to,
        Action = action,
        Reason = reason,
        ActorUserId = actorUserId,
        OccurredAt = now,
        Source = BuildSource(fingerprint),
        CorrelationId = idempotencyKey.Trim(),
        CreateDateTime = now,
        CreateBy = actorUserId
    };

    /// <summary>Tindakan lanjutan yang boleh ditawarkan frontend; satu-satunya definisi.</summary>
    public static List<string> AvailableActions(OprCaseStatus status) => status switch
    {
        OprCaseStatus.Requested => ["Update", "Schedule", "Postpone", "Cancel"],
        OprCaseStatus.Scheduled => ["Reschedule", "SaveChecklist", "SignOff", "EmergencyBypass", "Postpone", "Cancel"],
        OprCaseStatus.Postponed => ["Reschedule"],
        OprCaseStatus.Ready => ["SaveChecklist", "Start", "Cancel"],
        OprCaseStatus.InProgress =>
        [
            "SaveChecklist", "SaveExecutionRecord", "SaveAnesthesiaRecord",
            "RecordMaterial", "SaveRecovery", "SendHandover"
        ],
        _ => []
    };
}
