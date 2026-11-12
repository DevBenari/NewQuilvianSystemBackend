using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    /// <summary>
    /// Parameter konfigurasi master operasional penggajian rumah sakit (BE-HRD-146, BE-HRD-147, HRD-DEC-122 s.d. 124).
    /// </summary>
    [Table("HrdPayrollSetting", Schema = "public")]
    public class HrdPayrollSetting : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? HospitalSiteId { get; set; }

        [Required, MaxLength(100)]
        public string SettingName { get; set; } = "Standar Penggajian Rumah Sakit";

        [Column(TypeName = "decimal(18,2)")]
        public decimal BpjsKesehatanCapAmount { get; set; } = 12000000m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal BpjsKesehatanEmployeeRate { get; set; } = 1.00m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal BpjsKesehatanEmployerRate { get; set; } = 4.00m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal BpjsKetenagakerjaanJpCapAmount { get; set; } = 10042300m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal BpjsKetenagakerjaanJpEmployeeRate { get; set; } = 1.00m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal BpjsKetenagakerjaanJpEmployerRate { get; set; } = 2.00m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal BpjsKetenagakerjaanJhtEmployeeRate { get; set; } = 2.00m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal BpjsKetenagakerjaanJhtEmployerRate { get; set; } = 3.70m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal BpjsKetenagakerjaanJkkRate { get; set; } = 0.89m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal BpjsKetenagakerjaanJkmRate { get; set; } = 0.30m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal VarianceThresholdPercentage { get; set; } = 20.00m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal VarianceThresholdAmount { get; set; } = 2000000m;

        public int MonthlyCutoffDay { get; set; } = 20;

        public int DefaultPaymentDay { get; set; } = 25;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DefaultNightShiftAllowanceAmount { get; set; } = 50000m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal MaxLoanDeductionPercentage { get; set; } = 50.00m;

        public bool IsActive { get; set; } = true;

        public MstHospitalSite? HospitalSite { get; set; }
    }
}
