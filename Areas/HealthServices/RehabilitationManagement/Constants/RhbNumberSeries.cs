namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants
{
    /// <summary>
    /// Kunci deret nomor bisnis Rehab Medik untuk <c>NumberSeriesAllocator</c>
    /// (<c>02-backend-architecture.md</c> bagian 8). Nomor hanya dialokasikan Module Service;
    /// Count+1 atau Max+1 tidak dipakai (QBE-CODE-002, QBE-CODE-003).
    /// </summary>
    /// <remarks>
    /// Contoh hasil: <c>RHB-REQ-2026-000001</c>, <c>RHB-EPS-2026-000001</c>,
    /// <c>RHB-SES-2026-000001</c>. Cakupan deret tahunan.
    /// </remarks>
    public static class RhbNumberSeries
    {
        public const string RequestSequenceKey = "RHB_REQUEST";
        public const string EpisodeSequenceKey = "RHB_EPISODE";
        public const string SessionSequenceKey = "RHB_SESSION";

        public const string RequestPrefix = "RHB-REQ";
        public const string EpisodePrefix = "RHB-EPS";
        public const string SessionPrefix = "RHB-SES";
    }
}
