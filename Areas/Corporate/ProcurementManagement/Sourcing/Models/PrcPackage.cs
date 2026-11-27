using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models
{
    /// <summary>
    /// Paket pengadaan (BE-PBJ-PS-004; kamus data pemilihan-penyedia bagian 3): kumpulan baris PR yang disetujui, digabung
    /// per barang katalog, beserta HPS, salinan metode, persetujuan persiapan, dan nomor PO BLM.
    /// </summary>
    /// <remarks>
    /// Contoh: PKT-000012 "Kursi roda IGD &amp; Rawat Inap", 15 unit dari PR-000031 (IGD) dan PR-000034 (Rawat Inap), HPS
    /// Rp 42.750.000, metode Penunjukan Langsung (salinan), nomor PO-2026-0412. Satu jenis pengadaan dan satu tahun
    /// anggaran per paket (PBJ-DEC-090, 091). Metode dan aturannya disalin saat dipilih (PBJ-DEC-068).
    /// </remarks>
    [Table("PrcPackage", Schema = "public")]
    public class PrcPackage : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nomor <c>PKT-000012</c>; dialokasikan saat persiapan pertama kali diajukan.</summary>
        [MaxLength(20)]
        public string? PackageNumber { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public Guid ProcurementTypeId { get; set; }

        public MstProcurementType? ProcurementType { get; set; }

        public int FiscalYear { get; set; }

        /// <summary>Rencana paket rujukan saja; isi paket tetap baris PR (PBJ-DEC-082).</summary>
        public Guid? PackagePlanId { get; set; }

        public PrcPackagePlan? PackagePlan { get; set; }

        /// <summary>Paket asal bila paket ini hasil pecah (PBJ-DEC-083).</summary>
        public Guid? SplitFromPackageId { get; set; }

        public PrcPackage? SplitFromPackage { get; set; }

        [Required]
        [MaxLength(40)]
        public string Status { get; set; } = ProcurementStatuses.Package.Draft;

        public Guid? MethodId { get; set; }

        public MstProcurementMethod? Method { get; set; }

        /// <summary>Salinan saat metode dipilih.</summary>
        [MaxLength(30)]
        public string? MethodCode { get; set; }

        [MaxLength(150)]
        public string? MethodName { get; set; }

        public decimal? MethodMinHpsAmount { get; set; }

        public decimal? MethodMaxHpsAmount { get; set; }

        public int? MethodMinimumQuotationCount { get; set; }

        public bool MethodRequiresTechnicalEvaluation { get; set; }

        /// <summary>Dihitung server: HPS di luar rentang salinan metode (PBJ-DEC-068).</summary>
        public bool IsMethodOutOfRange { get; set; }

        /// <summary>Wajib bila <see cref="IsMethodOutOfRange"/>.</summary>
        [MaxLength(500)]
        public string? MethodDeviationReason { get; set; }

        /// <summary>SENSITIF (harga). Σ HPS item, termasuk PPN (PBJ-DEC-084).</summary>
        public decimal HpsTotal { get; set; }

        /// <summary>SENSITIF. HPS yang terkunci saat persiapan disetujui (PBJ-DEC-073).</summary>
        public decimal? ApprovedHpsTotal { get; set; }

        public Guid? PreparationWorkflowInstanceId { get; set; }

        public int PreparationSubmissionCount { get; set; }

        public DateTimeOffset? PreparationSubmittedAt { get; set; }

        public DateTimeOffset? PreparationApprovedAt { get; set; }

        /// <summary>Penanggung jawab paket; bawaan = pembuat.</summary>
        public Guid PicUserId { get; set; }

        /// <summary>Nomor PO BLM, unik antar paket (PBJ-DEC-011, PBJ-ASM-009).</summary>
        [MaxLength(50)]
        public string? PurchaseOrderNumber { get; set; }

        public DateTimeOffset? PurchaseOrderRecordedAt { get; set; }

        public Guid? PurchaseOrderRecordedByUserId { get; set; }

        [MaxLength(500)]
        public string? CancelReason { get; set; }

        /// <summary>Token konkurensi; nilai lama ditolak 409 <c>PBJ-VAL-090</c>.</summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public List<PrcPackageItem> Items { get; set; } = new();

        public List<PrcPackageChecklistItem> ChecklistItems { get; set; } = new();

        public List<PrcPackageEvaluator> Evaluators { get; set; } = new();
    }
}
