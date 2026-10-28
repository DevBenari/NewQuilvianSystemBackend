using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models
{
    /// <summary>Status kesepakatan tarif sharing — data-dictionary.md bagian 2.</summary>
    public static class MdfSharingAgreementStatuses
    {
        public const string Draft = "Draft";
        public const string Active = "Active";
        public const string Expired = "Expired";
        public const string Terminated = "Terminated";
    }

    /// <summary>Jenis penerima hak jasa medis — data-dictionary.md bagian 2.</summary>
    public static class MdfPayeeTypes
    {
        public const string Doctor = "Doctor";
        public const string Workforce = "Workforce";
    }

    /// <summary>
    /// Kesepakatan tarif sharing jasa medis (aggregate root) — MDF-DES-008, MF-DEC-004, MF-DEC-012.
    /// Menunjuk satu baris kontrak kerja HR (<see cref="WfpContractHistory"/>) lewat
    /// <see cref="SourceContractHistoryId"/> tanpa menyalin nomor kontrak, dokumen, atau masa berlakunya.
    /// Masa berlaku kesepakatan tarif (<see cref="EffectiveStart"/> .. <see cref="EffectiveEnd"/>)
    /// wajib berada di dalam masa berlaku kontrak sumbernya.
    /// </summary>
    [Table("MdfSharingAgreement", Schema = "public")]
    public class MdfSharingAgreement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nomor unik kesepakatan tarif sharing.</summary>
        [Required]
        [MaxLength(50)]
        public string AgreementNumber { get; set; } = string.Empty;

        /// <summary>FK ke kontrak HR (WfpContractHistory.Id), DeleteBehavior.Restrict.</summary>
        [Required]
        public Guid SourceContractHistoryId { get; set; }

        /// <summary>Jenis penerima: Doctor atau Workforce (MdfPayeeTypes).</summary>
        [Required]
        [MaxLength(30)]
        public string PayeeType { get; set; } = MdfPayeeTypes.Doctor;

        /// <summary>Rujukan polimorfik ke MstDoctor atau MstWorkforceProfile menurut PayeeType. Tanpa FK database.</summary>
        [Required]
        public Guid PayeeReferenceId { get; set; }

        /// <summary>Tanggal awal berlakunya kesepakatan tarif.</summary>
        [Required]
        public DateOnly EffectiveStart { get; set; }

        /// <summary>Tanggal akhir berlakunya kesepakatan tarif. Null = mengikuti akhir masa berlaku kontrak sumber.</summary>
        public DateOnly? EffectiveEnd { get; set; }

        /// <summary>Status kesepakatan tarif: Draft, Active, Expired, Terminated.</summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = MdfSharingAgreementStatuses.Draft;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>Token konkurensi optimistik (MDF-DES-005).</summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public WfpContractHistory? SourceContractHistory { get; set; }

        public ICollection<MdfSharingRule> SharingRules { get; set; } = new List<MdfSharingRule>();
    }
}

