namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    public static class PersonCategories
    {
        public const string General = "GENERAL";
        public const string Employee = "EMPLOYEE";
        public const string Doctor = "DOCTOR";
        public const string DoctorEmployee = "DOCTOR_EMPLOYEE";
        public const string EmployeeFamily = "EMPLOYEE_FAMILY";
        public const string ReviewRequired = "REVIEW_REQUIRED";
    }

    public static class WorkforceIdentityStatuses
    {
        public const string Active = "ACTIVE";
        public const string Inactive = "INACTIVE";
        public const string NotLinked = "NOT_LINKED";
        public const string Conflict = "CONFLICT";
        public const string Unavailable = "UNAVAILABLE";
    }

    public sealed class PatientWorkforceStatusResponse
    {
        public Guid PatientId { get; set; }
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string? NationalIdMasked { get; set; }

        /// <summary>
        /// GENERAL, EMPLOYEE, DOCTOR, DOCTOR_EMPLOYEE, EMPLOYEE_FAMILY, atau REVIEW_REQUIRED
        /// </summary>
        public string PersonCategory { get; set; } = PersonCategories.General;

        /// <summary>
        /// ACTIVE, INACTIVE, NOT_LINKED, CONFLICT, atau UNAVAILABLE
        /// </summary>
        public string IdentityStatus { get; set; } = WorkforceIdentityStatuses.NotLinked;

        public bool IsIdentityVerified { get; set; }
        public bool IsActiveWorkforce { get; set; }

        public string? StaffNumber { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? DoctorId { get; set; }
        public string? DoctorKind { get; set; }

        /// <summary>
        /// Identitas pemilik benefit (MstWorkforceProfile.Id) bila pasien adalah karyawan, dokter, atau
        /// anggota keluarga mereka. Dipisah dari PersonCategory: kategori adalah status identitas/profesi,
        /// kelayakan benefit adalah hasil pemeriksaan HR (IsBenefitEligible).
        /// </summary>
        public Guid? BenefitOwnerWorkforceProfileId { get; set; }

        /// <summary>EMPLOYEE atau DOCTOR; jenis pemilik benefit (bukan kategori pasien).</summary>
        public string? BenefitOwnerType { get; set; }
        public string? BenefitPlanName { get; set; }

        public string? OrganizationUnitName { get; set; }
        public string? PositionName { get; set; }

        public string? BenefitOwnerName { get; set; }
        public string? BenefitRelationship { get; set; }
        public Guid? InternalGuarantorCardId { get; set; }

        /// <summary>
        /// ELIGIBLE, NOT_ELIGIBLE, NOT_ENROLLED, atau UNAVAILABLE
        /// </summary>
        public string BenefitStatus { get; set; } = "NOT_ELIGIBLE";
        public bool IsBenefitEligible { get; set; }

        public bool RequiresReview { get; set; }
        public string? Message { get; set; }
    }
}
