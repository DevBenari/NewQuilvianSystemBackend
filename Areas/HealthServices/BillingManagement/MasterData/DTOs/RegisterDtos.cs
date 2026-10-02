using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.DTOs
{
    public sealed class CreateRegisterRequest
    {
        [Required, MaxLength(50)] public string RegisterCode { get; set; } = string.Empty;
        [Required, MaxLength(150)] public string RegisterName { get; set; } = string.Empty;
        [MaxLength(150)] public string? Location { get; set; }
        [MaxLength(250)] public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public sealed class UpdateRegisterRequest
    {
        [Required, MaxLength(50)] public string RegisterCode { get; set; } = string.Empty;
        [Required, MaxLength(150)] public string RegisterName { get; set; } = string.Empty;
        [MaxLength(150)] public string? Location { get; set; }
        [MaxLength(250)] public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public sealed class RegisterResponse
    {
        public Guid Id { get; set; }
        public string RegisterCode { get; set; } = string.Empty;
        public string RegisterName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public sealed class RegisterOptionResponse
    {
        public Guid Id { get; set; }
        public string RegisterCode { get; set; } = string.Empty;
        public string RegisterName { get; set; } = string.Empty;
        public string? Location { get; set; }
    }

    public sealed class RegisterStatusResponse
    {
        public Guid Id { get; set; }
        public string RegisterCode { get; set; } = string.Empty;
        public string RegisterName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public sealed class RegisterDeleteResponse
    {
        public Guid Id { get; set; }
        public string RegisterCode { get; set; } = string.Empty;
        public string RegisterName { get; set; } = string.Empty;
        public bool IsDelete { get; set; }
    }

    public sealed class RegisterSummaryResponse
    {
        public int TotalRegister { get; set; }
        public int ActiveRegister { get; set; }
        public int InactiveRegister { get; set; }
    }

    public sealed class RegisterSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public sealed class RegisterDefaultFilterResponse
    {
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
    }

    public sealed class RegisterFilterMetadataResponse
    {
        public RegisterDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<RegisterSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }
}
