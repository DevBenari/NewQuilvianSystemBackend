using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class HrdPayrollSettingDto
    {
        public Guid Id { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public string SettingName { get; set; } = string.Empty;
        public decimal VarianceThresholdPercentage { get; set; }
        public decimal VarianceThresholdAmount { get; set; }
        public int MonthlyCutoffDay { get; set; }
        public int DefaultPaymentDay { get; set; }
        public decimal DefaultNightShiftAllowanceAmount { get; set; }
        public decimal MaxLoanDeductionPercentage { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastUpdatedAt { get; set; }

        // ---- Status kebijakan tarif PPh (hanya pada hasil GET; tidak dibaca dari snapshot run). Tarif PPh yang dipakai payroll hanya dari
        // kebijakan berversi yang diverifikasi HRD dan disahkan Finance. Kolom BPJS pada HrdPayrollSetting adalah data historis dan tidak
        // lagi dikelola lewat DTO ini. ----
        public bool TaxPolicyApproved { get; set; }
        public string? TaxPolicyVersion { get; set; }
    }

    public class UpdateHrdPayrollSettingCommand
    {
        [Required, MaxLength(100)]
        public string SettingName { get; set; } = "Standar Penggajian Rumah Sakit";

        [Range(0, 100)]
        public decimal VarianceThresholdPercentage { get; set; } = 20.00m;

        [Range(0, 1000000000)]
        public decimal VarianceThresholdAmount { get; set; } = 2000000m;

        [Range(1, 28)]
        public int MonthlyCutoffDay { get; set; } = 20;

        [Range(1, 31)]
        public int DefaultPaymentDay { get; set; } = 25;

        [Range(0, 10000000)]
        public decimal DefaultNightShiftAllowanceAmount { get; set; } = 50000m;

        [Range(0, 100)]
        public decimal MaxLoanDeductionPercentage { get; set; } = 50.00m;
    }
}
