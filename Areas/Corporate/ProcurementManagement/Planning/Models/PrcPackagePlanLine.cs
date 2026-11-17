using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Models
{
    /// <summary>
    /// Alokasi satu baris rencana kebutuhan ke rencana paket, dengan unit asal tersimpan — kamus data
    /// bagian 3 (BE-PBJ-011).
    /// </summary>
    /// <remarks>
    /// Menunjuk <c>PrcNeedPlanLine.LineKey</c> (tanpa FK fisik) agar tetap tertaut walau rencana direvisi.
    /// Σ alokasi semua rencana paket yang masih hidup untuk satu baris rencana tidak boleh melebihi
    /// kuantitasnya (PBJ-VAL-050).
    /// </remarks>
    [Table("PrcPackagePlanLine", Schema = "public")]
    public class PrcPackagePlanLine : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PackagePlanId { get; set; }
        public PrcPackagePlan? PackagePlan { get; set; }

        public Guid NeedPlanLineKey { get; set; }

        /// <summary>Unit asal kebutuhan.</summary>
        public Guid DepartmentId { get; set; }
        public MstDepartment? Department { get; set; }

        public Guid ItemId { get; set; }
        public MstProcurementItem? Item { get; set; }

        [Column(TypeName = "numeric(18,4)")]
        public decimal AllocatedQuantity { get; set; }
    }
}
