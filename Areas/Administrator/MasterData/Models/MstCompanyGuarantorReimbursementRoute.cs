using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Models
{
    /// <summary>
    /// Master rute reimbursement perusahaan penjamin (MPY-DEC-008, CAP-35).
    /// Menerangkan hubungan kontraktual bagaimana perusahaan penjamin memperoleh penggantian biaya:
    /// menanggung sendiri (SELF) atau lewat asuransi mitra (INSURANCE_PROVIDER).
    /// </summary>
    [Table("MstCompanyGuarantorReimbursementRoute", Schema = "public")]
    public class MstCompanyGuarantorReimbursementRoute : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        
        public Guid? CompanyGuarantorId { get; set; }

        [Required]
        [MaxLength(30)]
        public string RouteType { get; set; } = "SELF";

        public Guid? InsuranceProviderId { get; set; }

        public int Priority { get; set; } = 1;

        public bool IsDefault { get; set; } = false;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigations
        public MstCompanyGuarantor? CompanyGuarantor { get; set; }
        public MstInsuranceProvider? InsuranceProvider { get; set; }
    }
}
