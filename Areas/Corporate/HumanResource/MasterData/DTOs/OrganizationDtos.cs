using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs
{
    public class OrganizationSummaryResponse
    {
        public int TotalDepartment { get; set; }

        public int ActiveDepartment { get; set; }

        public int InactiveDepartment { get; set; }

        public int TotalPosition { get; set; }

        public int ActivePosition { get; set; }

        public int InactivePosition { get; set; }
    }

    public class OrganizationDepartmentResponse
    {
        public Guid Id { get; set; }

        public string DepartmentCode { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }

        public int PositionCount { get; set; }

        public int ActivePositionCount { get; set; }

        public List<OrganizationPositionCompactResponse> Positions { get; set; } = new();
    }

    public class OrganizationDepartmentOptionResponse
    {
        public Guid Id { get; set; }

        public string DepartmentCode { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;
    }

    public class OrganizationPositionResponse
    {
        public Guid Id { get; set; }

        public Guid DepartmentId { get; set; }

        public string DepartmentCode { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public string PositionCode { get; set; } = string.Empty;

        public string PositionName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class OrganizationPositionCompactResponse
    {
        public Guid Id { get; set; }

        public Guid DepartmentId { get; set; }

        public string PositionCode { get; set; } = string.Empty;

        public string PositionName { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }

    public class OrganizationPositionOptionResponse
    {
        public Guid Id { get; set; }

        public Guid DepartmentId { get; set; }

        public string PositionCode { get; set; } = string.Empty;

        public string PositionName { get; set; } = string.Empty;
    }

    public class CreateOrganizationDepartmentRequest
    {
        [Required]
        [MaxLength(150)]
        public string DepartmentName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateOrganizationDepartmentRequest
    {
        [Required]
        [MaxLength(150)]
        public string DepartmentName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool CascadeToPositions { get; set; } = true;
    }

    public class CreateOrganizationPositionRequest
    {
        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        [MaxLength(150)]
        public string PositionName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateOrganizationPositionRequest
    {
        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        [MaxLength(150)]
        public string PositionName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateOrganizationStatusRequest
    {
        public bool IsActive { get; set; }

        public bool CascadeToPositions { get; set; } = true;
    }
}