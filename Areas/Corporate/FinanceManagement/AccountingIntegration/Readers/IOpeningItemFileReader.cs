namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Readers;

/// <summary>
/// BE-FIN-080, FIN-DES-093: batas antara berkas migrasi tagihan lama (CSV/XLSX) dan validasi
/// baris. Nol pengetahuan tentang piutang, utang, validasi, maupun database — hanya menguraikan
/// berkas menjadi baris mentah bernomor. Tipe milik paket pembaca (mis. XLSX, BE-FIN-083)
/// MUST NOT bocor melewati antarmuka ini, supaya paketnya dapat diganti kelak tanpa menyentuh
/// lapisan validasi (02-backend-architecture.md M.3).
/// </summary>
public interface IOpeningItemFileReader
{
    /// <summary>
    /// Menentukan apakah pembaca ini sanggup membaca berkas dengan tipe media/ekstensi tersebut.
    /// Pemilihan pembaca MUST memakai tipe media dan ekstensi berkas, bukan ruas yang diisi
    /// pengguna — pengguna tidak dapat memaksa pembaca yang salah.
    /// </summary>
    bool CanRead(string? mediaType, string? fileExtension);

    /// <summary>Menguraikan isi berkas menjadi baris mentah bernomor, nilai sel sebagai teks mentah.</summary>
    List<OpeningItemRawRow> Read(Stream stream);
}
