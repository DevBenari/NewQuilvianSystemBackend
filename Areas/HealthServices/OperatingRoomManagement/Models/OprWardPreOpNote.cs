using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

/// <summary>
/// Catatan Pra-Operasi bangsal, satu baris per versi (<c>BE-RWI-176</c>, kamus data 19.4,
/// <c>RWI-DEC-173</c>, <c>RWI-DEC-199</c>).
/// </summary>
/// <remarks>
/// Contoh: versi 1 dikirim Ns. Siti 1 Okt 08.30 (TD 120/80) lalu dikonfirmasi Ns. Dewi 09.20. Kasus
/// ditunda 09.40 → versi 1 <c>NeedsUpdate</c>. Esok harinya Siti menyimpan draf → versi 2
/// <c>Draft</c> dengan butir versi 1 sebagai usulan; saat dikirim, TD terbaru 160/100 dibekukan dan
/// versi 1 menjadi <c>Superseded</c>. Paling banyak satu versi berjalan (Draft/Sent/Confirmed) per kasus.
/// </remarks>
public class OprWardPreOpNote : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OprCaseId { get; set; }

    /// <summary>Naik satu setiap versi baru setelah penundaan.</summary>
    public int VersionNumber { get; set; } = 1;

    /// <summary>Versi yang diperbarui oleh versi ini.</summary>
    public Guid? PreviousVersionId { get; set; }

    public OprWardPreOpStatus Status { get; set; } = OprWardPreOpStatus.Draft;

    /// <summary>
    /// Potret tanda vital saat dikirim (TD, nadi, napas, suhu, SpO₂, waktu catat, <c>VitalSignId</c>).
    /// SENSITIF.
    /// </summary>
    public string? VitalSnapshotJson { get; set; }

    /// <summary>Potret nyeri saat dikirim; kosong bila belum pernah dinilai. SENSITIF.</summary>
    public string? PainSnapshotJson { get; set; }

    /// <summary><c>Left</c>, <c>Right</c>, <c>Bilateral</c>, atau <c>NotApplicable</c>; wajib saat kirim.</summary>
    public string? MarkingLaterality { get; set; }

    /// <summary>Contoh "Kuadran kanan bawah abdomen". SENSITIF.</summary>
    public string? MarkingLocationNote { get; set; }

    /// <summary>Konfirmasi penerima atas penandaan area operasi.</summary>
    public bool SiteMarkingConfirmed { get; set; }

    public Guid? SentByUserId { get; set; }
    public DateTime? SentAt { get; set; }

    /// <summary>Penerima; selalu berbeda dari pengirim (<c>CK_OprWardPreOpNote_TwoAccounts</c>).</summary>
    public Guid? ConfirmedByUserId { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>Waktu kasus ditunda sehingga versi ini perlu diperbarui.</summary>
    public DateTime? NeedsUpdateAt { get; set; }

    /// <summary>Konkurensi optimistis, pola OK.</summary>
    public int Version { get; set; }

    public OprCase? OprCase { get; set; }
    public OprWardPreOpNote? PreviousVersion { get; set; }
    public ICollection<OprWardPreOpItem> Items { get; set; } = [];
    public ICollection<OprWardPreOpSiteMark> SiteMarks { get; set; } = [];
}
