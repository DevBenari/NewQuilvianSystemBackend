using QuilvianSystemBackend.Areas.Administrator.UserManagement.Enum;
using QuilvianSystemBackend.Enum;

namespace QuilvianSystemBackend.Areas.Administrator.UserManagement.DTOs
{
    public class DoctorResponse
    {
        public Guid Id { get; set; }

        public string DoctorCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public DoctorType DoctorType { get; set; }

        public string DoctorTypeName { get; set; } = string.Empty;

        public Gender? Gender { get; set; }

        public string? GenderName { get; set; }

        public string? BirthPlace { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? IdentityType { get; set; }

        public string? IdentityNumber { get; set; }

        public string? PhoneNumber { get; set; }

        public string? WhatsAppNumber { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? SpecialistName { get; set; }

        public string? SubSpecialistName { get; set; }

        public string? MedicalStaffGroup { get; set; }

        public Guid? DepartmentId { get; set; }

        public string? DepartmentCode { get; set; }

        public string? DepartmentName { get; set; }

        public Guid? PositionId { get; set; }

        public string? PositionCode { get; set; }

        public string? PositionName { get; set; }

        public DateTime? JoinDate { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public bool IsAvailableForAppointment { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class CreateDoctorRequest
    {
        public string FullName { get; set; } = string.Empty;

        public DoctorType DoctorType { get; set; } = DoctorType.PermanentDoctor;

        public Gender? Gender { get; set; }

        public string? BirthPlace { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? IdentityType { get; set; }

        public string? IdentityNumber { get; set; }

        public string? PhoneNumber { get; set; }

        public string? WhatsAppNumber { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? SpecialistName { get; set; }

        public string? SubSpecialistName { get; set; }

        public string? MedicalStaffGroup { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? PositionId { get; set; }

        public DateTime? JoinDate { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public bool IsAvailableForAppointment { get; set; } = true;
    }

    public class UpdateDoctorRequest
    {
        public string FullName { get; set; } = string.Empty;

        public DoctorType DoctorType { get; set; } = DoctorType.PermanentDoctor;

        public Gender? Gender { get; set; }

        public string? BirthPlace { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? IdentityType { get; set; }

        public string? IdentityNumber { get; set; }

        public string? PhoneNumber { get; set; }

        public string? WhatsAppNumber { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? SpecialistName { get; set; }

        public string? SubSpecialistName { get; set; }

        public string? MedicalStaffGroup { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? PositionId { get; set; }

        public DateTime? JoinDate { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public bool IsAvailableForAppointment { get; set; } = true;

        public bool IsActive { get; set; } = true;
    }

    public class DoctorOptionResponse
    {
        public Guid Id { get; set; }

        public string DoctorCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public DoctorType DoctorType { get; set; }

        public string DoctorTypeName { get; set; } = string.Empty;

        public string? SpecialistName { get; set; }

        public string? SubSpecialistName { get; set; }

        public Guid? DepartmentId { get; set; }

        public string? DepartmentName { get; set; }

        public Guid? PositionId { get; set; }

        public string? PositionName { get; set; }
    }
}