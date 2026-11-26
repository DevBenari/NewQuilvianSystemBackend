using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Models
{
    /// <summary>
    /// Data pendamping satu vendor milik Pengadaan (BE-PBJ-VD-001; PBJ-DEC-010). Identitas vendor tetap
    /// <c>MstSupplier</c> dan tidak diubah dari sini.
    /// </summary>
    /// <remarks>
    /// Status vendor tidak disimpan; ia dihitung dari <c>MstSupplier.IsActive</c>, <c>MstSupplier.IsBlacklisted</c>,
    /// dan status Dibatasi di tabel ini. Contoh: dibatasi sampai 30-06-2027 → mulai 01-07-2027 tampil Aktif
    /// tanpa job terjadwal (PBJ-DEC-047).
    /// </remarks>
    [Table("PrcVendorProfile", Schema = "public")]
    public class PrcVendorProfile : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SupplierId { get; set; }

        public bool IsRestricted { get; set; }

        /// <summary>Kosong = Dibatasi sampai dicabut.</summary>
        public DateOnly? RestrictedUntil { get; set; }

        [MaxLength(1000)]
        public string? RestrictionReason { get; set; }

        public DateTimeOffset? RestrictedAt { get; set; }

        public Guid? RestrictedByUserId { get; set; }

        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public MstSupplier? Supplier { get; set; }

        public ICollection<PrcVendorCategory> Categories { get; set; } = new List<PrcVendorCategory>();
    }
}
