using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;

/// <summary>
/// Keadaan sebuah lembar copy resep yang sudah diterbitkan.
/// </summary>
/// <remarks>
/// Tidak ada keadaan draft. Copy resep lahir ketika dicetak dan diserahkan; sebelum dicetak
/// yang ada hanyalah pratinjau yang tidak disimpan. Lembar yang salah tidak dihapus melainkan
/// dicabut, karena lembarnya mungkin sudah berpindah tangan dan riwayatnya harus tetap
/// menjelaskan bahwa ia pernah ada.
/// </remarks>
public enum PrescriptionCopyStatus
{
    [Display(Name = "Diterbitkan")]
    Issued = 1,

    [Display(Name = "Dicabut")]
    Revoked = 2
}

/// <summary>
/// Penanda kelengkapan penyerahan sebuah baris pada lembar copy resep.
/// </summary>
/// <remarks>
/// <c>det</c> — <em>detur</em>, sudah diserahkan seluruhnya. <c>nedet</c> — <em>ne detur</em>,
/// masih ada yang belum diserahkan. Keduanya diturunkan dari jumlah sisa, tidak pernah
/// dipilih petugas.
/// </remarks>
public enum PrescriptionCopyMark
{
    [Display(Name = "det")]
    Det = 1,

    [Display(Name = "nedet")]
    Nedet = 2
}
