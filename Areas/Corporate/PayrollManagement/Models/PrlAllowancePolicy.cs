using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models
{
    /// <summary>
    /// Kebijakan Tunjangan umum (PAY-BE-FIX-021): satu jenis tunjangan (<see cref="MstAllowanceType"/>) dapat memiliki beberapa kebijakan,
    /// dan satu kebijakan memiliki tarif per Grade Gaji (<see cref="PrlAllowancePolicyGradeRate"/>). Penetapan pegawai menunjuk kebijakan
    /// dan tarifnya (<see cref="PrlWorkforceAllowance.AllowancePolicyId"/>), tidak pernah menyalin nominal dari input klien.
    ///
    /// Hanya untuk tunjangan RUTIN bermetode FixedAmount atau PercentageOfBaseSalary. Kebijakan khusus Transportasi, Shift, Siaga, dan
    /// Risiko tetap pada tabel masing-masing sampai cutover terkontrol. Perubahan isi kebijakan yang sudah dipakai dibuat sebagai revisi
    /// (<see cref="RevisionNo"/>, <see cref="SupersedesPolicyId"/>), bukan menimpa.
    ///
    /// Tabel dibuat lewat DDL yang diterapkan pemilik (laporan PAY-BE-FIX-021), bukan migration pada task ini.
    /// </summary>
    [Table("PrlAllowancePolicy", Schema = "public")]
    public class PrlAllowancePolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AllowanceTypeId { get; set; }

        [Required, MaxLength(50)]
        public string PolicyCode { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string PolicyName { get; set; } = string.Empty;

        /// <summary>FixedAmount atau PercentageOfBaseSalary (<see cref="SalaryStructureAllowanceCalculationMethods"/>).</summary>
        [Required, MaxLength(30)]
        public string CalculationMethod { get; set; } = SalaryStructureAllowanceCalculationMethods.FixedAmount;

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        /// <summary>Komponen payroll; null memakai komponen pada Master Tunjangan.</summary>
        public Guid? PayrollComponentId { get; set; }

        /// <summary>Cakupan opsional. Null = berlaku untuk semua. Dicocokkan dengan cakupan Struktur Gaji pegawai.</summary>
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }

        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public int RevisionNo { get; set; } = 1;
        public Guid? SupersedesPolicyId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstAllowanceType? AllowanceType { get; set; }
        public MstPayrollComponent? PayrollComponent { get; set; }
        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public ICollection<PrlAllowancePolicyGradeRate> GradeRates { get; set; } = new List<PrlAllowancePolicyGradeRate>();
    }

    /// <summary>
    /// Tarif kebijakan untuk satu Grade Gaji (<see cref="MstSalaryGrade"/>; BUKAN grade pegawai). Nominal bila FixedAmount, persen gaji
    /// pokok bila PercentageOfBaseSalary. Dua tarif aktif untuk kebijakan dan grade yang sama tidak boleh beririsan masa berlakunya
    /// (dijaga layanan pengelola kebijakan dan pemeriksaan DDL; tidak dapat dijadikan unique index).
    /// </summary>
    [Table("PrlAllowancePolicyGradeRate", Schema = "public")]
    public class PrlAllowancePolicyGradeRate : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AllowancePolicyId { get; set; }

        [Required]
        public Guid SalaryGradeId { get; set; }

        /// <summary>Nominal sebulan penuh bila FixedAmount; 0 untuk PercentageOfBaseSalary.</summary>
        public decimal Amount { get; set; } = 0m;

        /// <summary>Persen gaji pokok (0 &lt; nilai ≤ 100) bila PercentageOfBaseSalary; 0 untuk FixedAmount.</summary>
        public decimal Percentage { get; set; } = 0m;

        public decimal? MaximumAmount { get; set; }

        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public PrlAllowancePolicy? AllowancePolicy { get; set; }
        public MstSalaryGrade? SalaryGrade { get; set; }
    }

    /// <summary>Asal penetapan pada <see cref="PrlWorkforceAllowance.AssignmentSource"/> (kolom string; tanpa enum database).</summary>
    public static class WorkforceAllowanceAssignmentSources
    {
        /// <summary>Penetapan langsung lama: nominal ditetapkan petugas. Data sebelum PAY-BE-FIX-021 semuanya Direct.</summary>
        public const string Direct = "Direct";

        /// <summary>Penetapan berbasis Kebijakan Tunjangan: nilai berasal dari tarif kebijakan untuk Grade Gaji pegawai.</summary>
        public const string Policy = "Policy";
    }
}
