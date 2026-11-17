using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;

/// <summary>
/// Pengaturan register aset, satu baris per badan hukum (data-dictionary.md bagian 2.8, BE-AST-002).
/// Rentang nilai ditegakkan service master (AST-VAL-REG-056). Batas rupiah pemanfaatan bukan milik tabel
/// ini, melainkan sub-modul nilai (AST-DEC-027).
/// </summary>
[Table("MstAssetSetting", Schema = "public")]
public sealed class MstAssetSetting : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid LegalEntityId { get; set; }
    public MstLegalEntity? LegalEntity { get; set; }

    /// <summary>Site rumah sakit yang disimpan pada aset baru (AST-DEC-035).</summary>
    public Guid HospitalSiteId { get; set; }
    public MstHospitalSite? HospitalSite { get; set; }

    /// <summary>Jumlah hari tanpa pemakaian sebelum aset dianggap menganggur; 1–3650 (AST-RCG-G13).</summary>
    public int IdleThresholdDays { get; set; } = 90;

    /// <summary>Jumlah hari sebelum dokumen berakhir untuk mulai diperingatkan; 1–365 (AST-RCG-G14).</summary>
    public int DocumentExpiryWarningDays { get; set; } = 30;

    /// <summary>Masa pemanfaatan terlama yang boleh disetujui Kepala Bagian Aset, dalam bulan (AST-DEC-029).</summary>
    public int UtilizationMaxMonthsForAssetHead { get; set; } = 12;

    /// <summary>Tanggal awal pencatatan register; kosong sampai ditetapkan (AST-OQ-006).</summary>
    public DateOnly? CutoverDate { get; set; }
}
