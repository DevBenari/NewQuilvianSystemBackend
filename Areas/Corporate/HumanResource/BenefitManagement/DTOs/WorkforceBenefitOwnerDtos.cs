using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs
{
    /// <summary>
    /// Pemilik benefit hasil resolusi bersama (kontrak in-process, bukan endpoint). Identitas pemilik adalah
    /// <see cref="WorkforceProfileId"/>; <see cref="EmployeeId"/> dan <see cref="DoctorId"/> hanya identitas
    /// peran/profesi dan tidak pernah dipertukarkan.
    /// </summary>
    public sealed class WorkforceBenefitOwner
    {
        public Guid WorkforceProfileId { get; set; }

        /// <summary>EMPLOYEE atau DOCTOR (<see cref="WorkforceBenefitOwnerTypes"/>), mengikuti jenis SDM pada profil.</summary>
        public string OwnerType { get; set; } = WorkforceBenefitOwnerTypes.Employee;

        /// <summary>MstEmployee.Id bila SDM ini juga mempunyai data karyawan.</summary>
        public Guid? EmployeeId { get; set; }

        /// <summary>MstDoctor.Id bila SDM ini mempunyai data dokter.</summary>
        public Guid? DoctorId { get; set; }

        /// <summary>NIP (karyawan) atau nomor dokter, sesuai <see cref="OwnerType"/>.</summary>
        public string StaffNumber { get; set; } = string.Empty;
        public string StaffName { get; set; } = string.Empty;

        /// <summary>NIK mentah dari HR. Hanya untuk pencocokan di dalam proses; jangan diteruskan ke klien tanpa masking.</summary>
        public string? NationalId { get; set; }

        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public string? EmploymentTypeName { get; set; }
        public string? OrganizationUnitName { get; set; }
        public DateTime? JoinDate { get; set; }

        /// <summary>Profil, peran utama, dan status kepegawaian semuanya aktif (tidak resign).</summary>
        public bool IsActive { get; set; }

        public QuilvianSystemBackend.Enums.UserType UserType { get; set; }
    }

    /// <summary>Hasil resolusi: pemilik, atau kode alasan resmi (<see cref="BenefitEligibilityReasonCodes"/>).</summary>
    public sealed class WorkforceOwnerResolution
    {
        public WorkforceBenefitOwner? Owner { get; private init; }
        public string? ReasonCode { get; private init; }
        public string? Message { get; private init; }
        public bool IsResolved => Owner is not null;

        public static WorkforceOwnerResolution Ok(WorkforceBenefitOwner owner) => new() { Owner = owner };

        public static WorkforceOwnerResolution Fail(string reasonCode, string message) =>
            new() { ReasonCode = reasonCode, Message = message };
    }

    /// <summary>Satu baris pencarian pemilik benefit gabungan karyawan dan dokter tetap.</summary>
    public sealed class WorkforceSearchItem
    {
        public Guid WorkforceProfileId { get; set; }
        public string OwnerType { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public Guid? DoctorId { get; set; }
        public string StaffNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        /// <summary>NIK tersamar: empat digit pertama dan empat terakhir.</summary>
        public string NationalIdMasked { get; set; } = string.Empty;
        public string? OrganizationUnitName { get; set; }
        public string? EmploymentType { get; set; }
        public bool IsActive { get; set; }
        public bool HasActiveMedicalEnrollment { get; set; }
    }

    public sealed class WorkforceSearchResult
    {
        public List<WorkforceSearchItem> Items { get; set; } = new();
        public bool IsTruncated { get; set; }
    }
}
