using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models
{
    /// <summary>
    /// Penugasan evaluator teknis pada paket (BE-PBJ-PS-004; PBJ-DEC-074): dasar cakupan akses evaluator (PBJ-CAP-026).
    /// </summary>
    /// <remarks>
    /// Evaluator tidak boleh mengevaluasi penawaran yang ia catat sendiri (403 PBJ-VAL-135).
    /// </remarks>
    [Table("PrcPackageEvaluator", Schema = "public")]
    public class PrcPackageEvaluator : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PackageId { get; set; }

        public PrcPackage? Package { get; set; }

        public Guid EvaluatorUserId { get; set; }

        public Guid AssignedByUserId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
