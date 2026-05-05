using QuilvianSystemBackend.Areas.Administrator.UserManagement.Enum;

namespace QuilvianSystemBackend.Areas.Administrator.UserManagement.DTOs
{
    public class ExternalUserResponse
    {
        public Guid Id { get; set; }

        public string ExternalCode { get; set; } = string.Empty;

        public ExternalUserType ExternalUserType { get; set; }

        public string ExternalUserTypeName { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string? CompanyName { get; set; }

        public string? CompanyCode { get; set; }

        public string? JobTitle { get; set; }

        public string? ContactPersonName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? WhatsAppNumber { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? IdentityNumber { get; set; }

        public string? TaxNumber { get; set; }

        public string? BusinessLicenseNumber { get; set; }

        public string? ExternalStatus { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class CreateExternalUserRequest
    {
        public ExternalUserType ExternalUserType { get; set; } = ExternalUserType.Other;

        public string FullName { get; set; } = string.Empty;

        public string? CompanyName { get; set; }

        public string? CompanyCode { get; set; }

        public string? JobTitle { get; set; }

        public string? ContactPersonName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? WhatsAppNumber { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? IdentityNumber { get; set; }

        public string? TaxNumber { get; set; }

        public string? BusinessLicenseNumber { get; set; }

        public string? ExternalStatus { get; set; }
    }

    public class UpdateExternalUserRequest
    {
        public ExternalUserType ExternalUserType { get; set; } = ExternalUserType.Other;

        public string FullName { get; set; } = string.Empty;

        public string? CompanyName { get; set; }

        public string? CompanyCode { get; set; }

        public string? JobTitle { get; set; }

        public string? ContactPersonName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? WhatsAppNumber { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? IdentityNumber { get; set; }

        public string? TaxNumber { get; set; }

        public string? BusinessLicenseNumber { get; set; }

        public string? ExternalStatus { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ExternalUserOptionResponse
    {
        public Guid Id { get; set; }

        public string ExternalCode { get; set; } = string.Empty;

        public ExternalUserType ExternalUserType { get; set; }

        public string ExternalUserTypeName { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string? CompanyName { get; set; }

        public string? CompanyCode { get; set; }

        public string? JobTitle { get; set; }

        public string? ContactPersonName { get; set; }

        public string? Email { get; set; }
    }
}