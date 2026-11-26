namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs
{
    /// <summary>
    /// Satu pilihan Periode pada baris filter daftar Pengadaan (PBJ-DEC-055). Dikirim lewat
    /// <c>filters/metadata</c> sebagai <c>customPeriods</c>; nilai <c>custom</c> memakai
    /// <c>startDate</c>/<c>endDate</c>, nilai lain menghitung rentang dari tanggal operasional.
    /// </summary>
    public sealed class ProcurementPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }

        /// <summary>Daftar pilihan bawaan, sama dengan pola periode Master Data Supplier.</summary>
        public static List<ProcurementPeriodOptionResponse> Defaults() =>
        [
            new() { Value = "custom", Label = "Custom", Description = "Gunakan tanggal awal dan tanggal akhir.", UsesStartDate = true, UsesEndDate = true },
            new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini." },
            new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir." },
            new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir." },
            new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan." },
            new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya." }
        ];
    }
}
