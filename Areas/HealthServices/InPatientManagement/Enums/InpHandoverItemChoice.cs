namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    /// <summary>
    /// Pilihan per butir Serah Terima Pasien Baru — backend 13.9 (<c>BE-RWI-192</c>,
    /// <c>RWI-DEC-241</c>). Kosong berarti belum dipilih; kunci ditolak selama ada butir kosong.
    /// </summary>
    public enum InpHandoverItemChoice
    {
        /// <summary>Sudah.</summary>
        Done = 1,

        /// <summary>Belum — wajib berketerangan saat dikunci.</summary>
        NotDone = 2
    }
}
