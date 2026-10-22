using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models
{
    [Table("MstPatientCompanyGuarantor", Schema = "public")]
    public class MstPatientCompanyGuarantor : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid CompanyGuarantorId { get; set; }

        [Required]
        [MaxLength(100)]
        public string EmployeeNumber { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? EmployeeName { get; set; }

        [MaxLength(100)]
        public string? DepartmentName { get; set; }

        [MaxLength(100)]
        public string? PositionName { get; set; }

        [MaxLength(100)]
        public string? GradeLevel { get; set; }

        [MaxLength(100)]
        public string? BenefitPlanCode { get; set; }

        [MaxLength(150)]
        public string? BenefitPlanName { get; set; }

        [MaxLength(100)]
        public string? ClassName { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsEligible { get; set; } = true;

        public DateTime? LastEligibilityCheckAt { get; set; }

        [MaxLength(100)]
        public string? LastEligibilityReferenceNumber { get; set; }

        [MaxLength(250)]
        public string? EligibilityNote { get; set; }

        public decimal? AnnualLimitAmount { get; set; }

        public decimal? RemainingLimitAmount { get; set; }

        public decimal? CoPaymentPercent { get; set; }

        public decimal? CoPaymentAmount { get; set; }

        public bool IsNeedGuaranteeLetter { get; set; } = true;

        public bool IsNeedEmployeeVerification { get; set; } = true;

        public bool IsAllowExcessPaymentByPatient { get; set; } = true;

        [MaxLength(500)]
        public string? GuaranteeDocumentPath { get; set; }

        [MaxLength(500)]
        public string? CardImagePath { get; set; }

        [MaxLength(250)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        // BE-FIN-105, FIN-DEC-198/199, kamus data 9.4: tautan pasti ke data HR untuk kartu
        // penjamin internal. Ketiganya kosong untuk kartu biasa. Hanya-baca bagi klien; diisi
        // layanan kelayakan (BE-FIN-106). Kolom sensitif: tidak boleh masuk log.

        /// <summary>
        /// MstEmployee.Id pegawai pemilik benefit. Referensi kompatibilitas/historis: kosong untuk dokter tetap
        /// yang tidak punya data karyawan dan tidak pernah berisi DoctorId.
        /// </summary>
        public Guid? EmployeeId { get; set; }

        /// <summary>
        /// MstWorkforceProfile.Id pemilik benefit (identitas utama, karyawan atau dokter tetap). Kartu lama yang
        /// hanya membawa EmployeeId tetap sah dan diterjemahkan lewat resolver Workforce.
        /// </summary>
        public Guid? BenefitOwnerWorkforceProfileId { get; set; }

        /// <summary>EMPLOYEE atau DOCTOR; jenis pemilik benefit pada saat kartu ditautkan (tampilan).</summary>
        [MaxLength(20)]
        public string? BenefitOwnerType { get; set; }

        /// <summary>WfpFamilyMember.Id bila pasiennya anggota keluarga; kosong bila pegawai sendiri.</summary>
        public Guid? FamilyMemberId { get; set; }

        /// <summary>SELF, SPOUSE, CHILD, PARENT, atau OTHER (FIN-INTEGRATION-1.9 R.1.1).</summary>
        [MaxLength(50)]
        public string? BenefitRelationship { get; set; }

        public MstPatient? Patient { get; set; }

        public MstCompanyGuarantor? CompanyGuarantor { get; set; }
    }
}
