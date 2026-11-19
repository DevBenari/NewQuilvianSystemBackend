using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models
{
    /// <summary>
    /// Penetapan satu jenis tunjangan tetap KHUSUS satu pegawai atau dokter (kunci WorkforceProfileId + AllowanceTypeId), berbasis
    /// Master Tunjangan (MstAllowanceType). Berbeda dari <see cref="MstSalaryStructureAllowance"/> yang berlaku untuk SEMUA pemilik
    /// struktur gaji yang sama, dari <see cref="PrlTransportAllowance"/> yang dihitung kebijakan transport, dan dari
    /// <see cref="PrlPayrollVariableInput"/> yang terikat satu payroll run. Milik WorkforceProfileId: tidak bergantung pada
    /// <see cref="PrlPayroll"/>, sehingga tunjangan dapat disiapkan sebelum profil operasional payroll ada.
    ///
    /// Yang disalin dari master saat penetapan dibuat (snapshot, supaya master yang kemudian berubah tidak diam-diam mengubah syarat
    /// yang sudah disetujui): metode hitung, mata uang, IsTaxable, IsProrated, RequiresApproval, batas maksimum. Nominal atau persen
    /// adalah nilai yang ditetapkan untuk pegawai ini. Hanya penetapan berstatus Approved yang ikut dihitung mesin payroll.
    /// </summary>
    [Table("PrlWorkforceAllowance", Schema = "public")]
    public class PrlWorkforceAllowance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid AllowanceTypeId { get; set; }

        /// <summary>Salinan <see cref="MstAllowanceType.PayrollComponentId"/> saat penetapan dibuat; null bila jenis tunjangan tidak punya komponen.</summary>
        public Guid? PayrollComponentId { get; set; }

        /// <summary>FixedAmount atau PercentageOfBaseSalary (<see cref="SalaryStructureAllowanceCalculationMethods"/>), diturunkan dari master (Fixed / Percentage).</summary>
        [Required, MaxLength(30)]
        public string CalculationMethod { get; set; } = SalaryStructureAllowanceCalculationMethods.FixedAmount;

        /// <summary>Nominal sebulan penuh bila FixedAmount; 0 untuk PercentageOfBaseSalary.</summary>
        public decimal Amount { get; set; } = 0m;

        /// <summary>Persen gaji pokok penetapan gaji bila PercentageOfBaseSalary (0 &lt; nilai ≤ 100); 0 untuk FixedAmount.</summary>
        public decimal Percentage { get; set; } = 0m;

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        // Snapshot syarat dari master saat penetapan dibuat.
        public bool IsTaxable { get; set; } = true;
        public bool IsProrated { get; set; } = true;
        public bool RequiresApproval { get; set; } = false;
        public decimal? MaximumAmount { get; set; }

        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        /// <summary>Draft | PendingApproval | Approved | Rejected | Inactive (<see cref="WorkforceAllowanceStatuses"/>).</summary>
        [Required, MaxLength(30)]
        public string AllowanceStatus { get; set; } = WorkforceAllowanceStatuses.Draft;

        /// <summary>Alasan penolakan atau penonaktifan; catatan keputusan. Tidak berisi nominal.</summary>
        [MaxLength(500)]
        public string? StatusReason { get; set; }

        public DateTime? SubmittedAt { get; set; }
        public Guid? SubmittedByUserId { get; set; }

        /// <summary>Waktu dan pemutus (disetujui atau ditolak); null selama belum diputuskan.</summary>
        public DateTime? DecidedAt { get; set; }
        public Guid? DecidedByUserId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstAllowanceType? AllowanceType { get; set; }
    }

    /// <summary>Status siklus <see cref="PrlWorkforceAllowance.AllowanceStatus"/> (kolom string; tanpa enum database).</summary>
    public static class WorkforceAllowanceStatuses
    {
        public const string Draft = "Draft";
        public const string PendingApproval = "PendingApproval";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Inactive = "Inactive";

        public static readonly IReadOnlyList<string> All = new[] { Draft, PendingApproval, Approved, Rejected, Inactive };

        /// <summary>Penetapan yang masih "hidup" untuk pemeriksaan tumpang-tindih: belum ditolak dan belum dinonaktifkan.</summary>
        public static bool IsLive(string? status) =>
            status is Draft or PendingApproval or Approved;
    }
}
