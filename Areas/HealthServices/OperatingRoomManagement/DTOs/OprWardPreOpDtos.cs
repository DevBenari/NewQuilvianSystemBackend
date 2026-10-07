using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;

/// <summary>Butir yang disimpan pengirim pada draf (kontrak <c>0.10.0</c> API 11.3).</summary>
public class SaveWardPreOpDraftItemRequest
{
    [Required] public Guid PreparationItemId { get; set; }
    public bool SenderConfirmed { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
}

/// <summary>Titik penandaan area operasi; koordinat persen gambar 0–100.</summary>
public class WardPreOpSiteMarkRequest
{
    [Required] public OprBodyView BodyView { get; set; }
    [Range(0, 100)] public decimal X { get; set; }
    [Range(0, 100)] public decimal Y { get; set; }
    [MaxLength(100)] public string? Label { get; set; }
}

/// <summary>
/// Simpan draf pengirim (<c>PUT ward-pre-op/draft</c>). Bila versi terbaru "perlu diperbarui",
/// draf ini menjadi versi baru yang menyalin butir lama sebagai usulan.
/// </summary>
public class SaveWardPreOpDraftRequest
{
    public List<SaveWardPreOpDraftItemRequest> Items { get; set; } = [];
    public List<WardPreOpSiteMarkRequest> SiteMarks { get; set; } = [];

    /// <summary><c>Left</c>, <c>Right</c>, <c>Bilateral</c>, atau <c>NotApplicable</c>.</summary>
    [MaxLength(30)] public string? MarkingLaterality { get; set; }

    [MaxLength(500)] public string? MarkingLocationNote { get; set; }

    /// <summary>Versi baris terbaru yang dibaca klien; 0 bila belum ada versi.</summary>
    [Range(0, int.MaxValue)] public int ExpectedVersion { get; set; }
}

/// <summary>Kirim versi draf (<c>PATCH ward-pre-op/send</c>).</summary>
public class SendWardPreOpRequest
{
    [Range(0, int.MaxValue)] public int ExpectedVersion { get; set; }

    /// <summary>Cadangan bila header <c>Idempotency-Key</c> tidak dikirim.</summary>
    [MaxLength(100)] public string? IdempotencyKey { get; set; }
}

/// <summary>Konfirmasi penerima per butir.</summary>
public class ConfirmWardPreOpItemRequest
{
    /// <summary>Id butir versi (<c>OprWardPreOpItem.Id</c>).</summary>
    [Required] public Guid ItemId { get; set; }
    public bool ReceiverConfirmed { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
}

/// <summary>Konfirmasi penerima (<c>PATCH ward-pre-op/confirm</c>).</summary>
public class ConfirmWardPreOpRequest
{
    public List<ConfirmWardPreOpItemRequest> Items { get; set; } = [];
    public bool SiteMarkingConfirmed { get; set; }
    [Range(0, int.MaxValue)] public int ExpectedVersion { get; set; }

    /// <summary>Cadangan bila header <c>Idempotency-Key</c> tidak dikirim.</summary>
    [MaxLength(100)] public string? IdempotencyKey { get; set; }
}

/// <summary>Potret tanda vital yang dibekukan saat dikirim.</summary>
public class WardPreOpVitalSnapshot
{
    public Guid? VitalSignId { get; set; }
    public int? SystolicBp { get; set; }
    public int? DiastolicBp { get; set; }
    public int? PulseRate { get; set; }
    public int? RespiratoryRate { get; set; }
    public decimal? Temperature { get; set; }
    public decimal? SpO2 { get; set; }
    public DateTime? RecordedAt { get; set; }
}

/// <summary>Potret nyeri yang dibekukan saat dikirim; kosong bila belum pernah dinilai.</summary>
public class WardPreOpPainSnapshot
{
    public int? Score { get; set; }
    public string? ScaleName { get; set; }
    public DateTime? RecordedAt { get; set; }

    /// <summary>Rujukan pencatatan sumber: pengkajian nyeri atau tanda vital.</summary>
    public Guid? SourceId { get; set; }
    public string? SourceKind { get; set; }
}

public class WardPreOpItemResponse
{
    /// <summary>Id butir versi; <c>Guid.Empty</c> pada templat yang belum tersimpan.</summary>
    public Guid ItemId { get; set; }
    public Guid PreparationItemId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public bool SenderConfirmed { get; set; }
    public bool ReceiverConfirmed { get; set; }
    public string? ReceiverConfirmedByName { get; set; }
    public DateTime? ReceiverConfirmedAt { get; set; }
    public string? Note { get; set; }
}

public class WardPreOpSiteMarkResponse
{
    public Guid Id { get; set; }
    public OprBodyView BodyView { get; set; }
    public decimal X { get; set; }
    public decimal Y { get; set; }
    public string? Label { get; set; }
}

/// <summary>
/// Versi Catatan Pra-Operasi beserta butir dan penandaan. Bila kasus belum punya versi apa pun,
/// <c>IsTemplate = true</c> dan <c>Items</c> berisi butir aktif master sebagai bahan form.
/// </summary>
public class WardPreOpResponse
{
    public Guid Id { get; set; }
    public Guid OprCaseId { get; set; }
    public bool IsTemplate { get; set; }
    public int VersionNumber { get; set; }
    public OprWardPreOpStatus? Status { get; set; }
    public WardPreOpVitalSnapshot? VitalSnapshot { get; set; }
    public WardPreOpPainSnapshot? PainSnapshot { get; set; }
    public List<WardPreOpItemResponse> Items { get; set; } = [];
    public List<WardPreOpSiteMarkResponse> SiteMarks { get; set; } = [];
    public string? MarkingLaterality { get; set; }
    public string? MarkingLocationNote { get; set; }
    public bool SiteMarkingConfirmed { get; set; }

    /// <summary>Sisi pada pesanan operasi (<c>OprCase.Laterality</c>).</summary>
    public string? CaseLaterality { get; set; }

    public string? SentByName { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ConfirmedByName { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? NeedsUpdateAt { get; set; }
    public Guid? PreviousVersionId { get; set; }
    public int Version { get; set; }
}

public class WardPreOpVersionSummary
{
    public Guid Id { get; set; }
    public int VersionNumber { get; set; }
    public OprWardPreOpStatus Status { get; set; }
    public string? SentByName { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ConfirmedByName { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? NeedsUpdateAt { get; set; }
    public Guid? PreviousVersionId { get; set; }
}
