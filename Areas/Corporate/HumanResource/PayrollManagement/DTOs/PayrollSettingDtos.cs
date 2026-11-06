using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class HrdPayrollSettingDto
    {
        public Guid Id { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public string SettingName { get; set; } = string.Empty;
        public decimal BpjsKesehatanCapAmount { get; set; }
        public decimal BpjsKesehatanEmployeeRate { get; set; }
        public decimal BpjsKesehatanEmployerRate { get; set; }
        public decimal BpjsKetenagakerjaanJpCapAmount { get; set; }
        public decimal BpjsKetenagakerjaanJpEmployeeRate { get; set; }
        public decimal BpjsKetenagakerjaanJpEmployerRate { get; set; }
        public decimal BpjsKetenagakerjaanJhtEmployeeRate { get; set; }
        public decimal BpjsKetenagakerjaanJhtEmployerRate { get; set; }
        public decimal BpjsKetenagakerjaanJkkRate { get; set; }
        public decimal BpjsKetenagakerjaanJkmRate { get; set; }
        public decimal BpjsJpCapAmount { get => BpjsKetenagakerjaanJpCapAmount; set => BpjsKetenagakerjaanJpCapAmount = value; }
        public decimal BpjsJpEmployeeRate { get => BpjsKetenagakerjaanJpEmployeeRate; set => BpjsKetenagakerjaanJpEmployeeRate = value; }
        public decimal BpjsJpEmployerRate { get => BpjsKetenagakerjaanJpEmployerRate; set => BpjsKetenagakerjaanJpEmployerRate = value; }
        public decimal BpjsJhtEmployeeRate { get => BpjsKetenagakerjaanJhtEmployeeRate; set => BpjsKetenagakerjaanJhtEmployeeRate = value; }
        public decimal BpjsJhtEmployerRate { get => BpjsKetenagakerjaanJhtEmployerRate; set => BpjsKetenagakerjaanJhtEmployerRate = value; }
        public decimal BpjsJkkRate { get => BpjsKetenagakerjaanJkkRate; set => BpjsKetenagakerjaanJkkRate = value; }
        public decimal BpjsJkmRate { get => BpjsKetenagakerjaanJkmRate; set => BpjsKetenagakerjaanJkmRate = value; }
        public decimal VarianceThresholdPercentage { get; set; }
        public decimal VarianceThresholdAmount { get; set; }
        public int MonthlyCutoffDay { get; set; }
        public int DefaultPaymentDay { get; set; }
        public decimal DefaultNightShiftAllowanceAmount { get; set; }
        public decimal MaxLoanDeductionPercentage { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }

    public class UpdateHrdPayrollSettingCommand
    {
        [Required, MaxLength(100)]
        public string SettingName { get; set; } = "Standar Penggajian Rumah Sakit";

        [Range(0, 1000000000)]
        public decimal BpjsKesehatanCapAmount { get; set; } = 12000000m;

        [Range(0, 100)]
        public decimal BpjsKesehatanEmployeeRate { get; set; } = 1.00m;

        [Range(0, 100)]
        public decimal BpjsKesehatanEmployerRate { get; set; } = 4.00m;

        [Range(0, 1000000000)]
        public decimal BpjsKetenagakerjaanJpCapAmount { get; set; } = 10042300m;

        [Range(0, 100)]
        public decimal BpjsKetenagakerjaanJpEmployeeRate { get; set; } = 1.00m;

        [Range(0, 100)]
        public decimal BpjsKetenagakerjaanJpEmployerRate { get; set; } = 2.00m;

        [Range(0, 100)]
        public decimal BpjsKetenagakerjaanJhtEmployeeRate { get; set; } = 2.00m;

        [Range(0, 100)]
        public decimal BpjsKetenagakerjaanJhtEmployerRate { get; set; } = 3.70m;

        [Range(0, 100)]
        public decimal BpjsKetenagakerjaanJkkRate { get; set; } = 0.89m;

        [Range(0, 100)]
        public decimal BpjsKetenagakerjaanJkmRate { get; set; } = 0.30m;

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
