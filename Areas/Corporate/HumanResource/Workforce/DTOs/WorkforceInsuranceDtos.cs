using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceInsuranceResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public bool IsBpjsKesehatanEnabled { get; set; }

        public string? BpjsKesehatanNumber { get; set; }

        public bool IsBpjsKetenagakerjaanEnabled { get; set; }

        public string? BpjsKetenagakerjaanNumber { get; set; }

        public bool IsPrivateInsuranceEnabled { get; set; }

        public string? PrivateInsuranceProvider { get; set; }

        public string? PrivateInsuranceNumber { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceInsuranceListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int BpjsKesehatanData { get; set; }

        public int BpjsKetenagakerjaanData { get; set; }

        public int PrivateInsuranceData { get; set; }

        public List<WorkforceInsuranceResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceInsuranceRequest
    {
        public bool IsBpjsKesehatanEnabled { get; set; } = false;

        [MaxLength(50)]
        public string? BpjsKesehatanNumber { get; set; }

        public bool IsBpjsKetenagakerjaanEnabled { get; set; } = false;

        [MaxLength(50)]
        public string? BpjsKetenagakerjaanNumber { get; set; }

        public bool IsPrivateInsuranceEnabled { get; set; } = false;

        [MaxLength(100)]
        public string? PrivateInsuranceProvider { get; set; }

        [MaxLength(100)]
        public string? PrivateInsuranceNumber { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceInsuranceRequest
    {
        public bool IsBpjsKesehatanEnabled { get; set; } = false;

        [MaxLength(50)]
        public string? BpjsKesehatanNumber { get; set; }

        public bool IsBpjsKetenagakerjaanEnabled { get; set; } = false;

        [MaxLength(50)]
        public string? BpjsKetenagakerjaanNumber { get; set; }

        public bool IsPrivateInsuranceEnabled { get; set; } = false;

        [MaxLength(100)]
        public string? PrivateInsuranceProvider { get; set; }

        [MaxLength(100)]
        public string? PrivateInsuranceNumber { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceInsuranceStatusRequest
    {
        public bool IsActive { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }
}
