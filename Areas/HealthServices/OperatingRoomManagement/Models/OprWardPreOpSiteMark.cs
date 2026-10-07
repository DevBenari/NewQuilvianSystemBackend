using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

/// <summary>
/// Titik penandaan area operasi pada gambar tubuh (<c>BE-RWI-176</c>, kamus data 19.6,
/// <c>RWI-DEC-174</c>). Koordinat dalam persen gambar, 0–100. Foto tubuh pasien tidak disimpan.
/// </summary>
public class OprWardPreOpSiteMark : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid NoteId { get; set; }
    public OprBodyView BodyView { get; set; }

    /// <summary>Persen lebar gambar, 0–100.</summary>
    public decimal X { get; set; }

    /// <summary>Persen tinggi gambar, 0–100.</summary>
    public decimal Y { get; set; }

    public string? Label { get; set; }

    public OprWardPreOpNote? PreOpNote { get; set; }
}
