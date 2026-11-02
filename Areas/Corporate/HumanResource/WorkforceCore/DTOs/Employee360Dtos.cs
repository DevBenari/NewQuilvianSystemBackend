using System;
using System.Collections.Generic;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    public class Employee360Response
    {
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public bool IsActive { get; set; }

        // Mode Tampilan: "HR" (Profil Penuh) atau "DirectManager" (Profil Kedinasan Bawahan Terbatas)
        public string ViewMode { get; set; } = "DirectManager";

        // Profil Kedinasan Saat Ini (Boleh diakses HR dan Atasan Langsung)
        public CurrentEmploymentInfoDto CurrentEmployment { get; set; } = new();

        // Atasan Langsung
        public DirectManagerInfoDto? DirectManager { get; set; }

        // Riwayat SK Kepegawaian (Mutasi/Promosi/Rotasi)
        public List<PersonnelActionOrderListResponse> PersonnelActionOrders { get; set; } = new();

        // Riwayat Penempatan
        public List<EmploymentAssignmentHistoryDto> AssignmentHistories { get; set; } = new();

        // Riwayat Pelatihan & Kompetensi
        public List<EmployeeCompetencyDto> Competencies { get; set; } = new();

        // Data Konfidensial & Finansial (HANYA MUNCUL DI MODE HR - NULL DI MODE ATASAN)
        public ConfidentialFinancialProfileDto? ConfidentialFinancialProfile { get; set; }

        // Data Pribadi & Disiplin Sensitif (HANYA MUNCUL DI MODE HR - NULL DI MODE ATASAN)
        public ConfidentialPersonalAndDisciplineDto? ConfidentialPersonalAndDiscipline { get; set; }
    }

    public class CurrentEmploymentInfoDto
    {
        public Guid? LegalEntityId { get; set; }
        public string? LegalEntityName { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public Guid? EmployeeGradeId { get; set; }
        public string? EmployeeGradeName { get; set; }
        public string? AssignmentType { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
    }

    public class DirectManagerInfoDto
    {
        public Guid ManagerProfileId { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public string? ManagerPositionName { get; set; }
        public string? ManagerDepartmentName { get; set; }
    }

    public class EmploymentAssignmentHistoryDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty; // Organization, Position, Manager
        public string Detail { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsPrimary { get; set; }
        public string? ActionOrderNumber { get; set; }
    }

    public class EmployeeCompetencyDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Issuer { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? CertificateNumber { get; set; }
    }

    public class ConfidentialFinancialProfileDto
    {
        public decimal? BaseSalary { get; set; }
        public string? CurrencyCode { get; set; }
        public string? SalaryGradeName { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankAccountHolderName { get; set; }
    }

    public class ConfidentialPersonalAndDisciplineDto
    {
        public string? NationalIdentificationNumber { get; set; } // NIK
        public string? TaxIdentificationNumber { get; set; } // NPWP
        public DateTime? DateOfBirth { get; set; }
        public string? BloodType { get; set; }
        public List<string> FamilyMembersSummary { get; set; } = new();
        public List<string> DisciplinarySanctions { get; set; } = new();
    }
}
