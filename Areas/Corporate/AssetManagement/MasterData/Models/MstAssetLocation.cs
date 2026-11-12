using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;

/// <summary>
/// Ruangan tempat aset berada — gedung, lantai, ruangan — milik satu unit organisasi (AST-DEC-034,
/// data-dictionary.md bagian 2.7, BE-AST-002). Unit tetap milik HR (<c>MstOrganizationUnit</c>) dan ruangan
/// klinis tetap milik <c>MstRoom</c>; tabel ini hanya menautkan, tidak menyalin datanya.
/// </summary>
[Table("MstAssetLocation", Schema = "public")]
public sealed class MstAssetLocation : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Kode lokasi, contoh <c>IGD-RESUS</c>.</summary>
    [Required, MaxLength(30)] public string LocationCode { get; set; } = string.Empty;

    [Required, MaxLength(150)] public string LocationName { get; set; } = string.Empty;

    /// <summary>Unit pemilik ruangan. Unit harus aktif saat lokasi disimpan (AST-VAL-REG-055).</summary>
    public Guid OrganizationUnitId { get; set; }
    public MstOrganizationUnit? OrganizationUnit { get; set; }

    [MaxLength(100)] public string? BuildingName { get; set; }

    [MaxLength(50)] public string? FloorName { get; set; }

    [MaxLength(100)] public string? RoomName { get; set; }

    /// <summary>Tautan opsional ke ruangan klinis <c>MstRoom</c>.</summary>
    public Guid? RoomId { get; set; }
    public MstRoom? Room { get; set; }

    public bool IsActive { get; set; } = true;
}
