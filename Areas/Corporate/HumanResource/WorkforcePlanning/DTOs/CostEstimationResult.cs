using System;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs
{
    /// <summary>
    /// Status ketersediaan tarif gaji tengah grade untuk estimasi biaya.
    /// </summary>
    public enum CostRateStatus
    {
        Available = 1,
        RateUnavailable = 2
    }

    /// <summary>
    /// Hasil perhitungan estimasi biaya penambahan tenaga kerja (BE-HRD-038).
    /// </summary>
    public sealed class CostEstimationResult
    {
        public decimal? EstimatedCost { get; set; }
        public decimal? MidpointSalary { get; set; }
        public int Months { get; set; }
        public decimal WorkloadPortion { get; set; }
        public CostRateStatus RateStatus { get; set; }
        public string? Note { get; set; }
        public bool CostVisible { get; set; } = true;
    }
}
