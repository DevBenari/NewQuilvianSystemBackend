using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

/// <summary>
/// Satu butir checklist pada satu versi Catatan Pra-Operasi (<c>BE-RWI-176</c>, kamus data 19.5).
/// Nama dan sifat wajib disalin saat versi dibuat, sehingga perubahan master tidak mengubah riwayat.
/// </summary>
public class OprWardPreOpItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid NoteId { get; set; }
    public Guid PreparationItemId { get; set; }
    public string ItemNameSnapshot { get; set; } = string.Empty;
    public bool IsMandatorySnapshot { get; set; }
    public bool SenderConfirmed { get; set; }
    public bool ReceiverConfirmed { get; set; }

    /// <summary>Penerima butir; selalu berbeda dari pengirim versi.</summary>
    public Guid? ReceiverConfirmedByUserId { get; set; }
    public DateTime? ReceiverConfirmedAt { get; set; }

    /// <summary>Contoh "Puasa sejak 22.00". SENSITIF.</summary>
    public string? Note { get; set; }

    public OprWardPreOpNote? PreOpNote { get; set; }
    public MstSurgicalPreparationItem? PreparationItem { get; set; }
}
