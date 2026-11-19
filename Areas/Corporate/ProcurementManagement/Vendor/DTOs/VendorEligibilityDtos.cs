using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.DTOs;

// =====================================================================================
// BE-PBJ-VD-005 — DTO kelayakan vendor per kategori (PBJ-VD-API 1.0 GET /vendors/eligibility; PBJ-VD-INT 1.0 §2).
// Dipakai layar vendor dan, kelak, pemilihan-penyedia untuk mencegah pilihan BARU (PBJ-DEC-054).
// =====================================================================================

public sealed class VendorEligibilityQuery
{
    /// <summary>Kategori yang diperiksa (wajib). Contoh: kategori Obat.</summary>
    [Required] public Guid? ItemCategoryId { get; set; }

    /// <summary>Opsional. Kosong = semua vendor yang melayani kategori itu.</summary>
    public Guid? SupplierId { get; set; }

    /// <summary>Hanya kembalikan vendor yang layak (dipakai daftar pilihan). Bawaan <c>false</c>.</summary>
    public bool OnlyEligible { get; set; }
}

/// <summary>
/// Hasil kelayakan satu vendor untuk satu kategori. Contoh tidak layak:
/// <c>{ "isEligible": false, "isRestricted": false, "reasons": ["Dokumen wajib Izin Pedagang Besar Farmasi kedaluwarsa sejak 16-11-2026."] }</c>.
/// Dibatasi tetap layak (boleh dipilih dengan alasan pemilih, PBJ-DEC-047) dan ditandai <see cref="IsRestricted"/>.
/// </summary>
public sealed class VendorEligibilityResponse
{
    public Guid SupplierId { get; set; }
    public Guid ItemCategoryId { get; set; }
    public bool IsEligible { get; set; }
    public bool IsRestricted { get; set; }

    /// <summary>Sebab tidak layak; kosong bila layak.</summary>
    public List<string> Reasons { get; set; } = new();

    /// <summary>Tambahan (delta INT): kode dan nama vendor untuk daftar pilihan.</summary>
    public string SupplierCode { get; set; } = string.Empty;

    public string SupplierName { get; set; } = string.Empty;

    /// <summary>
    /// Tambahan (delta INT): hal yang tidak menggugurkan tetapi perlu diketahui pemilih. Contoh: "Dibatasi sampai
    /// 30-06-2027: terlambat kirim tiga kali." atau "Dokumen wajib Izin PBF berakhir dalam 20 hari (07-12-2026)."
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}
