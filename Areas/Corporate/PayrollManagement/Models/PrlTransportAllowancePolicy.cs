using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models
{
    [Table("PrlTransportAllowancePolicy", Schema = "public")]
    public class PrlTransportAllowancePolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? EmployeeGradeId { get; set; }

        /// <summary>
        /// Grade Gaji (penggajian) yang menjadi cakupan kebijakan; BUKAN <see cref="EmployeeGradeId"/> (grade kepegawaian). Null pada
        /// kebijakan lama: tidak dimigrasikan dan tidak dianggap berlaku untuk grade tertentu (PAY-BE-FIX-014). Membutuhkan kolom database
        /// yang diterapkan pemilik lebih dulu (lihat laporan PAY-BE-FIX-013 bagian 3).
        /// </summary>
        public Guid? SalaryGradeId { get; set; }

        /// <summary>
        /// Struktur Gaji yang menjadi cakupan kebijakan. Harus milik <see cref="SalaryGradeId"/> yang sama (divalidasi service pada Create,
        /// Update, dan aktivasi). Null berarti seluruh struktur milik grade itu (bila grade terisi) atau data lama (bila grade kosong).
        /// </summary>
        public Guid? SalaryStructureId { get; set; }

        public Guid? PayrollComponentId { get; set; }

        [Required, MaxLength(50)]
        public string PolicyCode { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string PolicyName { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string CalculationMethod { get; set; } = "FixedMonthly";
        // FixedMonthly, PerAttendance, PerDay, Reimbursement.

        public decimal FixedMonthlyAmount { get; set; } = 0m;
        public decimal PerAttendanceAmount { get; set; } = 0m;
        public decimal DailyLimitAmount { get; set; } = 0m;
        public decimal MonthlyLimitAmount { get; set; } = 0m;
        public int MinimumAttendanceMinutes { get; set; } = 0;

        public bool IsAttendanceBased { get; set; } = false;
        public bool IsProrated { get; set; } = true;
        public bool IsTaxable { get; set; } = true;

        /// <summary>
        /// Klasifikasi tunjangan tetap (perjanjian kerja/kebijakan HRD), terpisah dari <see cref="IsTaxable"/>. Transportasi berbasis
        /// kehadiran/per hari tidak pernah tunjangan tetap. Null = belum diklasifikasikan. LEGACY_PERSISTED: klasifikasi untuk dasar upah iuran BPJS yang tidak lagi dipakai (PAY-BE-FIX-007); kolom dipertahankan.
        /// </summary>
        public bool? IsFixedAllowance { get; set; }

        /// <summary>Masuk dasar upah iuran BPJS; hanya boleh true bila <see cref="IsFixedAllowance"/> true. Null = belum diklasifikasikan.</summary>
        public bool? IncludeInBpjsWageBase { get; set; }

        public bool IsIncludedInPayroll { get; set; } = true;
        public bool IncludeBusinessTravelDay { get; set; } = false;
        public bool IncludePaidLeaveDay { get; set; } = false;
        public bool IncludeHoliday { get; set; } = false;

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstEmployeeGrade? EmployeeGrade { get; set; }
        public MstSalaryGrade? SalaryGrade { get; set; }
        public MstSalaryStructure? SalaryStructure { get; set; }
        public MstPayrollComponent? PayrollComponent { get; set; }

        public ICollection<PrlTransportAllowance> TransportAllowances { get; set; }
            = new List<PrlTransportAllowance>();
    }
}
