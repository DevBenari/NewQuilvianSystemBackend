namespace QuilvianSystemBackend.DTOs.System
{
    /// <summary>Status fitur yang dikendalikan Backend. Frontend tidak menyimpan atau mengubahnya; ia hanya mengikuti nilai ini.</summary>
    public class SystemFeaturesResponse
    {
        /// <summary>True bila fitur BPJS (iuran, kebijakan, kepesertaan, klaim COB/SEP, klaim JKK) aktif. Nonaktif = konfigurasi dan jalur BPJS disembunyikan.</summary>
        public bool BpjsEnabled { get; set; }

        /// <summary>Pesan baku saat BPJS nonaktif; null bila aktif.</summary>
        public string? BpjsDisabledMessage { get; set; }
    }
}
