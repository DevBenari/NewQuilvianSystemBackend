using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models
{
    [Table("MstKpiCatalog", Schema = "public")]
    public class MstKpiCatalog : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? OrganizationUnitId { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? PositionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string KpiCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string KpiName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string KpiCategory { get; set; } = "General";

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? MeasurementUnit { get; set; }

        [Required]
        [MaxLength(50)]
        public string TargetDirection { get; set; } = "HigherIsBetter";
        // HigherIsBetter, LowerIsBetter, ExactTarget, RangeTarget, Milestone

        [Required]
        [MaxLength(50)]
        public string MeasurementFrequency { get; set; } = "Annual";
        // Daily, Weekly, Monthly, Quarter, Semester, Annual, OnDemand

        [MaxLength(250)]
        public string? DataSource { get; set; }

        [MaxLength(2000)]
        public string? CalculationFormula { get; set; }

        public decimal? DefaultTargetValue { get; set; }

        public decimal? MinimumTargetValue { get; set; }

        public decimal? MaximumTargetValue { get; set; }

        public decimal DefaultWeight { get; set; } = 0m;

        public bool IsQuantitative { get; set; } = true;

        public bool IsCascadable { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public MstOrganizationUnit? OrganizationUnit { get; set; }

        public MstDepartment? Department { get; set; }

        public MstPosition? Position { get; set; }

        public ICollection<MstPerformanceTemplateDetail> TemplateDetails { get; set; }
            = new List<MstPerformanceTemplateDetail>();
    }
}
