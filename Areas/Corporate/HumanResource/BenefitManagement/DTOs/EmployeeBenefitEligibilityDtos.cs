using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs
{
    // BE-FIN-104, FIN-INTEGRATION-1.9 R.2.3 (PeriksaKelayakan). Kontrak in-process, bukan endpoint.

    public sealed class BenefitEligibilityRequest
    {
        /// <summary>
        /// Identitas pemilik benefit (utama). Bila kosong, EmployeeId lama diterjemahkan ke WorkforceProfileId.
        /// Bila keduanya diisi, keduanya harus menunjuk orang yang sama.
        /// </summary>
        public Guid? WorkforceProfileId { get; set; }

        /// <summary>Kompatibilitas kontrak lama (MstEmployee.Id). Boleh kosong untuk dokter.</summary>
        public Guid EmployeeId { get; set; }

        /// <summary>Kosong bila pasiennya pegawai sendiri.</summary>
        public Guid? FamilyMemberId { get; set; }

        /// <summary>Tanggal layanan (tanggal operasional; jam diabaikan).</summary>
        public DateTime ServiceDate { get; set; }
    }

    public sealed class BenefitEligibilityResult
    {
        public bool IsEligible => ReasonCode == BenefitEligibilityReasonCodes.Ok;
        public string ReasonCode { get; set; } = string.Empty;
        public string? Message { get; set; }

        // Terisi hanya bila ReasonCode = OK. Tanpa saldo (R.2.3).

        /// <summary>Identitas pemilik benefit yang sah (utama). Selalu terisi bila ReasonCode = OK.</summary>
        public Guid? WorkforceProfileId { get; set; }

        /// <summary>EMPLOYEE atau DOCTOR.</summary>
        public string? OwnerType { get; set; }

        /// <summary>Identitas peran; kosong bagi dokter tanpa data karyawan. Tidak pernah berisi DoctorId.</summary>
        public Guid? EmployeeId { get; set; }

        public Guid? DoctorId { get; set; }

        /// <summary>NIP atau nomor dokter.</summary>
        public string? StaffNumber { get; set; }
        public string? StaffName { get; set; }

        /// <summary>
        /// NIP dan nama pegawai dari HR. Hanya untuk pemanggil di dalam proses (label kartu internal,
        /// FIN-INTEGRATION-1.9 R.2 butir "EmployeeNumber dan EmployeeName diisi dari data HR");
        /// tidak diteruskan ke respons API pemeriksaan kelayakan.
        /// </summary>
        public string? EmployeeNumber { get; set; }
        public string? EmployeeName { get; set; }
        public Guid? FamilyMemberId { get; set; }
        public string? RelationshipCode { get; set; }
        public string? BenefitPersonKind { get; set; }
        public Guid? BenefitPersonId { get; set; }
        public Guid? EnrollmentId { get; set; }
        public string? PlanCode { get; set; }
        public string? PlanName { get; set; }
        public string? CoverageType { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        /// <summary>
        /// NIK, nama lengkap, dan tanggal lahir orang yang bersangkutan (pegawai atau keluarga)
        /// untuk pencocokan identitas dengan pasien registrasi (FIN-DEC-228).
        /// </summary>
        public string? TargetPersonNationalId { get; set; }
        public string? TargetPersonFullName { get; set; }
        public DateTime? TargetPersonBirthDate { get; set; }
    }

    /// <summary>Satu baris hasil CariPegawai (R.2.1). Hanya data yang dibutuhkan petugas Registrasi.</summary>
    public sealed class EmployeeSearchItem
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

    public sealed class EmployeeSearchResult
    {
        public List<EmployeeSearchItem> Items { get; set; } = new();

        /// <summary>True bila ada lebih banyak pegawai cocok daripada batas (FIN-VAL-269).</summary>
        public bool IsTruncated { get; set; }
    }

    /// <summary>Satu baris hasil DaftarKeluarga (R.2.2).</summary>
    public sealed class FamilyMemberEligibilityItem
    {
        public Guid FamilyMemberId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string RelationshipCode { get; set; } = string.Empty;
        public int? AgeYears { get; set; }
        public string? IdentityMasked { get; set; }

        /// <summary>VERIFIED, REQUESTED, NOT_REGISTERED, REJECTED, atau ENDED.</summary>
        public string DependentVerification { get; set; } = string.Empty;

        /// <summary>Apakah CoverageType plan pegawai mencakup hubungan ini (R.1.2).</summary>
        public bool CoveredByPlan { get; set; }
    }
}
