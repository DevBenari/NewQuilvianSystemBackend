namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Readers;

/// <summary>
/// BE-FIN-080, FIN-DES-093: DTO internal (bukan DTO API) hasil penguraian satu baris berkas
/// migrasi tagihan lama. Nilai sel disimpan sebagai teks mentah — penguraian angka/tanggal
/// MUST terjadi satu tempat saja, di lapisan validasi (BE-FIN-081), bukan di dalam pembaca.
/// </summary>
public sealed class OpeningItemRawRow
{
    /// <summary>
    /// Nomor baris persis seperti terlihat bila berkas dibuka di aplikasi spreadsheet —
    /// baris header berkas asal adalah baris 1, sehingga baris data pertama adalah baris 2.
    /// Dipakai seluruh pesan galat (FIN-VAL-226/227) supaya petugas dapat menemukan barisnya sendiri.
    /// </summary>
    public required int RowNumber { get; init; }

    /// <summary>Nama kolom (dari baris header) memetakan ke nilai sel sebagai teks mentah, apa adanya.</summary>
    public required IReadOnlyDictionary<string, string> Cells { get; init; }
}
