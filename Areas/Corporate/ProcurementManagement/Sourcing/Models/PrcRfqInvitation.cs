using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models
{
    /// <summary>
    /// Vendor yang diundang pada RFQ (BE-PBJ-PS-004; PBJ-ASM-006): layak untuk semua kategori paket saat diundang.
    /// </summary>
    /// <remarks>
    /// Vendor Dibatasi boleh diundang dengan alasan (PBJ-DEC-047, 422 PBJ-VAL-131 bila tanpa alasan).
    /// </remarks>
    [Table("PrcRfqInvitation", Schema = "public")]
    public class PrcRfqInvitation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid RfqId { get; set; }

        public PrcRfq? Rfq { get; set; }

        public Guid SupplierId { get; set; }

        public MstSupplier? Supplier { get; set; }

        public bool WasRestricted { get; set; }

        /// <summary>Wajib bila <see cref="WasRestricted"/>.</summary>
        [MaxLength(500)]
        public string? RestrictedReason { get; set; }
    }
}
