using QuilvianSystemBackend.Areas.Administrator.UserManagement.Enum;
using QuilvianSystemBackend.Enum;

namespace QuilvianSystemBackend.Areas.Administrator.UserManagement.DTOs
{
    public class EmployeeResponse
    {
        public Guid Id { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string? EmployeeNumber { get; set; }

        public string? AttendanceNumber { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string? NickName { get; set; }

        public Gender? Gender { get; set; }

        public string? GenderName { get; set; }

        public string? BirthPlace { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? Religion { get; set; }

        public string? MaritalStatus { get; set; }

        public string? BloodType { get; set; }

        public string? IdentityType { get; set; }

        public string? IdentityNumber { get; set; }

        public string? PhoneNumber { get; set; }

        public string? WhatsAppNumber { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? Province { get; set; }

        public string? City { get; set; }

        public string? District { get; set; }

        public string? Village { get; set; }

        public string? PostalCode { get; set; }

        public Guid DepartmentId { get; set; }

        public string DepartmentCode { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public Guid PositionId { get; set; }

        public string PositionCode { get; set; } = string.Empty;

        public string PositionName { get; set; } = string.Empty;

        public EmployeeStatus EmployeeStatus { get; set; }

        public string EmployeeStatusName { get; set; } = string.Empty;

        public string? EmploymentType { get; set; }

        public string? GradeLevel { get; set; }

        public string? WorkLocation { get; set; }

        public DateTime? JoinDate { get; set; }

        public DateTime? ProbationEndDate { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public DateTime? ResignDate { get; set; }

        public string? ResignReason { get; set; }

        public string? EmergencyContactName { get; set; }

        public string? EmergencyContactRelation { get; set; }

        public string? EmergencyContactPhone { get; set; }

        public string? EmergencyContactAddress { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class CreateEmployeeRequest
    {
        public string? EmployeeNumber { get; set; }

        public string? AttendanceNumber { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string? NickName { get; set; }

        public Gender? Gender { get; set; }

        public string? BirthPlace { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? Religion { get; set; }

        public string? MaritalStatus { get; set; }

        public string? BloodType { get; set; }

        public string? IdentityType { get; set; }

        public string? IdentityNumber { get; set; }

        public string? PhoneNumber { get; set; }

        public string? WhatsAppNumber { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? Province { get; set; }

        public string? City { get; set; }

        public string? District { get; set; }

        public string? Village { get; set; }

        public string? PostalCode { get; set; }

        public Guid DepartmentId { get; set; }

        public Guid PositionId { get; set; }

        public EmployeeStatus EmployeeStatus { get; set; } = EmployeeStatus.Permanent;

        public string? EmploymentType { get; set; }

        public string? GradeLevel { get; set; }

        public string? WorkLocation { get; set; }

        public DateTime? JoinDate { get; set; }

        public DateTime? ProbationEndDate { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public string? EmergencyContactName { get; set; }

        public string? EmergencyContactRelation { get; set; }

        public string? EmergencyContactPhone { get; set; }

        public string? EmergencyContactAddress { get; set; }
    }

    public class UpdateEmployeeRequest
    {
        public string? EmployeeNumber { get; set; }

        public string? AttendanceNumber { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string? NickName { get; set; }

        public Gender? Gender { get; set; }

        public string? BirthPlace { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? Religion { get; set; }

        public string? MaritalStatus { get; set; }

        public string? BloodType { get; set; }

        public string? IdentityType { get; set; }

        public string? IdentityNumber { get; set; }

        public string? PhoneNumber { get; set; }

        public string? WhatsAppNumber { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? Province { get; set; }

        public string? City { get; set; }

        public string? District { get; set; }

        public string? Village { get; set; }

        public string? PostalCode { get; set; }

        public Guid DepartmentId { get; set; }

        public Guid PositionId { get; set; }

        public EmployeeStatus EmployeeStatus { get; set; } = EmployeeStatus.Contract;

        public string? EmploymentType { get; set; }

        public string? GradeLevel { get; set; }

        public string? WorkLocation { get; set; }

        public DateTime? JoinDate { get; set; }

        public DateTime? ProbationEndDate { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public DateTime? ResignDate { get; set; }

        public string? ResignReason { get; set; }

        public string? EmergencyContactName { get; set; }

        public string? EmergencyContactRelation { get; set; }

        public string? EmergencyContactPhone { get; set; }

        public string? EmergencyContactAddress { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class EmployeeOptionResponse
    {
        public Guid Id { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string? EmployeeNumber { get; set; }

        public string FullName { get; set; } = string.Empty;

        public Guid DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public Guid PositionId { get; set; }

        public string PositionName { get; set; } = string.Empty;
    }
}