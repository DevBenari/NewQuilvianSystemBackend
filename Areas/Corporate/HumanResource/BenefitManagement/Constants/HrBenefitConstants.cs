namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants
{
    /// <summary>
    /// Status enrollment benefit pegawai (FIN-STATE-1.9 R.1). Nilai mengikuti daftar yang sudah
    /// didokumentasikan pada model TrxEmployeeBenefitEnrollment; yang dipakai BE-FIN-103 adalah
    /// bagiannya: Draft, Submitted, Approved, Active, Cancelled, Expired.
    /// </summary>
    public static class EmployeeBenefitEnrollmentStatuses
    {
        public const string Draft = "Draft";
        public const string Submitted = "Submitted";
        public const string Approved = "Approved";
        public const string Active = "Active";
        public const string Cancelled = "Cancelled";
        public const string Expired = "Expired";

        public static readonly string[] All = { Draft, Submitted, Approved, Active, Cancelled, Expired };

        /// <summary>Status yang masih "terbuka" dan dijaga satu per pegawai per plan.</summary>
        public static readonly string[] Open = { Draft, Submitted, Approved, Active };

        public static readonly string[] Terminal = { Cancelled, Expired };
    }

    /// <summary>Status tanggungan benefit (FIN-STATE-1.9 R.2).</summary>
    public static class EmployeeBenefitDependentStatuses
    {
        public const string Draft = "Draft";
        public const string Requested = "Requested";
        public const string Verified = "Verified";
        public const string Rejected = "Rejected";
        public const string Ended = "Ended";

        public static readonly string[] All = { Draft, Requested, Verified, Rejected, Ended };

        /// <summary>Status yang dihitung terhadap MaximumDependents dan dijaga unik per anggota keluarga.</summary>
        public static readonly string[] Open = { Draft, Requested, Verified };
    }

    /// <summary>
    /// Kode hubungan baku (FIN-INTEGRATION-1.9 R.1.1, FIN-DEC-211). Teks hubungan pada
    /// WfpFamilyMember.Relationship dipetakan di sini; nilai tak dikenal menjadi OTHER.
    /// </summary>
    public static class BenefitRelationshipCodes
    {
        public const string Self = "SELF";
        public const string Spouse = "SPOUSE";
        public const string Child = "CHILD";
        public const string Parent = "PARENT";
        public const string Other = "OTHER";

        public static string FromFamilyRelationship(string? relationship) =>
            (relationship ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                "SPOUSE" => Spouse,
                "CHILD" => Child,
                "FATHER" or "MOTHER" => Parent,
                _ => Other
            };
    }

    /// <summary>
    /// CoverageType pada MstBenefitPlan menentukan hubungan yang boleh ditanggung (FIN-DEC-207,
    /// FIN-INTEGRATION-1.9 R.1.2). Family membolehkan semua hubungan, tetap hanya yang diverifikasi HR.
    /// </summary>
    public static class BenefitCoverageTypes
    {
        public const string Individual = "Individual";
        public const string Family = "Family";
        public const string EmployeeAndSpouse = "EmployeeAndSpouse";
        public const string EmployeeAndChildren = "EmployeeAndChildren";

        public static bool AllowsRelationship(string? coverageType, string relationshipCode)
        {
            if (relationshipCode == BenefitRelationshipCodes.Self) return true;
            return (coverageType ?? string.Empty) switch
            {
                Family => true,
                EmployeeAndSpouse => relationshipCode == BenefitRelationshipCodes.Spouse,
                EmployeeAndChildren => relationshipCode == BenefitRelationshipCodes.Child,
                _ => false
            };
        }
    }

    /// <summary>
    /// Penanda enrollment layanan RS (FIN-DEC-211 butir a): plan yang jenis benefitnya
    /// BenefitCategory = Medical dan bukan berbasis klaim.
    /// </summary>
    public static class BenefitServiceCategories
    {
        public const string Medical = "Medical";
    }

    /// <summary>Pemilik saldo plafon (FIN-DEC-207): pegawai sendiri di enrollment, atau satu baris tanggungan.</summary>
    public static class BenefitPersonKinds
    {
        public const string Enrollment = "ENROLLMENT";
        public const string Dependent = "DEPENDENT";
    }

    /// <summary>Jenis mutasi pada HrdBenefitBalanceMovement (kamus data 9.2).</summary>
    public static class BenefitMovementTypes
    {
        public const string Use = "USE";
        public const string Return = "RETURN";

        public static readonly string[] All = { Use, Return };
    }

    /// <summary>Kode alasan kelayakan (FIN-INTEGRATION-1.9 R.2.3), dipakai ulang oleh BacaPlafon (R.3.1).</summary>
    public static class BenefitEligibilityReasonCodes
    {
        public const string Ok = "OK";
        public const string EmployeeNotFound = "EMPLOYEE_NOT_FOUND";
        public const string EnrollmentNotActive = "ENROLLMENT_NOT_ACTIVE";
        public const string EnrollmentAmbiguous = "ENROLLMENT_AMBIGUOUS";
        public const string FamilyMemberNotFound = "FAMILY_MEMBER_NOT_FOUND";
        public const string FamilyMemberInactive = "FAMILY_MEMBER_INACTIVE";
        public const string RelationshipNotCovered = "RELATIONSHIP_NOT_COVERED";
        public const string DependentNotRegistered = "DEPENDENT_NOT_REGISTERED";
        public const string DependentNotVerified = "DEPENDENT_NOT_VERIFIED";
        public const string DependentNotEligible = "DEPENDENT_NOT_ELIGIBLE";
        public const string DependentOutOfPeriod = "DEPENDENT_OUT_OF_PERIOD";

        /// <summary>Diisi pemanggil (adapter Billing/Registrasi) bila pembacaan HR melempar galat.</summary>
        public const string HrUnavailable = "HR_UNAVAILABLE";
    }

    /// <summary>Hasil PakaiPlafon (FIN-INTEGRATION-1.9 R.3.2).</summary>
    public static class BenefitUseResults
    {
        public const string Applied = "APPLIED";
        public const string AlreadyApplied = "ALREADY_APPLIED";
        public const string KeyConflict = "KEY_CONFLICT";
        public const string Insufficient = "INSUFFICIENT";
        public const string NotUsable = "NOT_USABLE";
    }

    /// <summary>Hasil KembalikanPlafon (FIN-INTEGRATION-1.9 R.3.3).</summary>
    public static class BenefitReturnResults
    {
        public const string Returned = "RETURNED";
        public const string AlreadyReturned = "ALREADY_RETURNED";
        public const string NotFound = "NOT_FOUND";
    }

    /// <summary>Nama aksi pada AvailableActions; sama dengan argumen pertama [AccessAction].</summary>
    public static class EmployeeBenefitActions
    {
        public const string Update = "Update";
        public const string Submit = "Submit";
        public const string Approve = "Approve";
        public const string Activate = "Activate";
        public const string Cancel = "Cancel";
        public const string Request = "Request";
        public const string Verify = "Verify";
        public const string Reject = "Reject";
        public const string End = "End";
    }
}
