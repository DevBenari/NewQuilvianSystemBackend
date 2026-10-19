namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    // BE-FIN-106, FIN-API-1.10 R.3. Seluruhnya hanya-baca. Tanpa saldo plafon: Registrasi hanya tahu
    // layak atau tidak (FIN-DEC-202).

    /// <summary>Satu pegawai pada hasil pencarian. Hanya data yang dibutuhkan petugas memilih orang yang tepat.</summary>
    public sealed class EmployeeLookupResponse
    {
        public Guid EmployeeId { get; set; }

        /// <summary>Identitas pemilik benefit (tambahan; EmployeeId tetap untuk kompatibilitas).</summary>
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;

        /// <summary>NIK tersamar: empat digit pertama dan empat terakhir, sisanya "•".</summary>
        public string NationalIdMasked { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? OrganizationUnitName { get; set; }
        public bool HasActiveMedicalEnrollment { get; set; }
    }

    /// <summary>
    /// Satu SDM pada pencarian pemilik benefit gabungan (karyawan dan dokter tetap). WorkforceProfileId adalah
    /// identitas pemilik; EmployeeId/DoctorId hanya identitas peran dan boleh kosong.
    /// </summary>
    public sealed class WorkforceProfileLookupResponse
    {
        public Guid WorkforceProfileId { get; set; }

        /// <summary>EMPLOYEE atau DOCTOR.</summary>
        public string OwnerType { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public Guid? DoctorId { get; set; }

        /// <summary>NIP atau nomor dokter.</summary>
        public string StaffNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        /// <summary>NIK tersamar: empat digit pertama dan empat terakhir, sisanya "•".</summary>
        public string NationalIdMasked { get; set; } = string.Empty;
        public string? OrganizationUnitName { get; set; }
        public string? EmploymentType { get; set; }
        public bool IsActive { get; set; }
        public bool HasActiveMedicalEnrollment { get; set; }
    }

    /// <summary>Satu anggota keluarga aktif beserta status verifikasi tanggungannya di HR.</summary>
    public sealed class FamilyMemberLookupResponse
    {
        public Guid FamilyMemberId { get; set; }
        public string FullName { get; set; } = string.Empty;

        /// <summary>SPOUSE, CHILD, PARENT, atau OTHER; diturunkan sistem dari data HR.</summary>
        public string RelationshipCode { get; set; } = string.Empty;
        public int? AgeYears { get; set; }
        public string? IdentityMasked { get; set; }

        /// <summary>VERIFIED, REQUESTED, NOT_REGISTERED, REJECTED, atau ENDED.</summary>
        public string DependentVerification { get; set; } = string.Empty;
        public bool CoveredByPlan { get; set; }
    }

    /// <summary>
    /// Hasil pemeriksaan kelayakan. "Tidak layak" bukan galat: alasannya dijelaskan ReasonCode dan Message.
    /// </summary>
    public sealed class BenefitEligibilityResponse
    {
        public bool IsEligible { get; set; }
        public string ReasonCode { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string? RelationshipCode { get; set; }
        public string? BenefitPersonKind { get; set; }
        public Guid? BenefitPersonId { get; set; }
        public Guid? EnrollmentId { get; set; }

        /// <summary>Pemilik benefit yang sah (terisi bila layak): identitas Workforce dan jenisnya.</summary>
        public Guid? WorkforceProfileId { get; set; }
        public string? OwnerType { get; set; }
        public string? OwnerName { get; set; }
        public string? OwnerStaffNumber { get; set; }
        public string? PlanCode { get; set; }
        public string? PlanName { get; set; }
        public string? CoverageType { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
    }
}
