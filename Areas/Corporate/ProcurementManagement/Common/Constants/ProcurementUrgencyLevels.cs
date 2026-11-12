namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants
{
    /// <summary>
    /// Tingkat urgensi permintaan pengadaan (PBJ-DEC-016). PR mendesak wajib beralasan dan berbatas
    /// waktu, lalu memakai definisi approval <c>PBJ-PR-MENDESAK</c>.
    /// </summary>
    public static class ProcurementUrgencyLevels
    {
        public const string Normal = "NORMAL";
        public const string Urgent = "URGENT";

        public static bool IsKnown(string? value) =>
            string.Equals(value, Normal, StringComparison.Ordinal) ||
            string.Equals(value, Urgent, StringComparison.Ordinal);
    }
}
