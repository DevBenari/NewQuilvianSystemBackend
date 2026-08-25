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
}
