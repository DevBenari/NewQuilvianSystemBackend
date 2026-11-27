using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models
{
    /// <summary>
    /// Asal satu baris PR pada item paket (BE-PBJ-PS-004; PBJ-DEC-071, 078), beserta salinan kunci garis pagu untuk
    /// komitmen per unit.
    /// </summary>
    /// <remarks>
    /// Satu baris PR hanya di satu paket hidup: index unik untuk baris yang belum dilepas (PBJ-AC-026, 409 PBJ-VAL-121).
    /// Contoh: baris 1 PR-000031 (IGD, sumber dana Operasional) 10 unit.
    /// </remarks>
    [Table("PrcPackageItemSource", Schema = "public")]
    public class PrcPackageItemSource : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PackageItemId { get; set; }

        public PrcPackageItem? PackageItem { get; set; }

        public Guid PurchaseRequestLineId { get; set; }

        public PrcPurchaseRequestLine? PurchaseRequestLine { get; set; }

        public Guid PurchaseRequestId { get; set; }

        public PrcPurchaseRequest? PurchaseRequest { get; set; }

        /// <summary>Salinan unit PR: kunci garis pagu dan cakupan unit.</summary>
        public Guid DepartmentId { get; set; }

        /// <summary>Salinan sumber dana PR.</summary>
        public Guid FundSourceId { get; set; }

        /// <summary>= jumlah beli baris PR.</summary>
        public decimal Quantity { get; set; }

        /// <summary>true saat paket batal/ditolak atau baris dilepas (PBJ-DEC-077).</summary>
        public bool IsReleased { get; set; }

        public DateTimeOffset? ReleasedAt { get; set; }
    }
}
