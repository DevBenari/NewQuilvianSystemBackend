using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models
{
    /// <summary>
    /// Jejak tindakan otomatis atas akun aplikasi pegawai selama skorsing (INT-HR-88, BE-HRD-285, BE-HRD-278;
    /// remediasi Gelombang 2, 10 November 2026).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Keputusan yang diterapkan: saat surat skorsing aktif dan <c>IsSystemAccessRevoked</c> benar, worker
    /// <c>ItAccessRevocationSyncWorker</c> <b>mengunci</b> akun aplikasi pegawai (akun dinonaktifkan dan diberi
    /// <c>LockoutEnd</c>) sehingga pegawai yang diskors tidak lagi dapat membuka SIMRS/EMR. Saat skorsing berakhir
    /// atau pegawai direhabilitasi, worker <b>memulihkan</b> hanya akun yang ia kunci sendiri, ke keadaan aktif
    /// semula (<see cref="WasActiveBefore"/>). Akun yang sudah nonaktif sebelum skorsing tidak pernah dihidupkan.
    /// </para>
    /// <para>
    /// Satu baris per tindakan. Pasangan <c>Locked</c> tanpa <c>Restored</c> berarti akun masih terkunci oleh sistem.
    /// </para>
    /// </remarks>
    [Table("HrdSuspensionAccessAction", Schema = "public")]
    public class HrdSuspensionAccessAction : IdentityModel
    {
        public const string ActionLocked = "Locked";
        public const string ActionRestored = "Restored";

        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SuspensionId { get; set; }

        /// <summary>Pegawai yang diskors (<c>MstEmployee.Id</c>, sama dengan <c>TrxDisciplinarySuspension.EmployeeId</c>).</summary>
        public Guid EmployeeId { get; set; }

        /// <summary>Akun aplikasi yang dikunci atau dipulihkan.</summary>
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string ActionType { get; set; } = ActionLocked;

        /// <summary>Keadaan <c>IsActive</c> akun sebelum dikunci; dipakai saat memulihkan.</summary>
        public bool WasActiveBefore { get; set; } = true;

        public DateTime ActionAt { get; set; } = DateTime.UtcNow;

        public DateTime? LockoutUntil { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public TrxDisciplinarySuspension? Suspension { get; set; }
    }
}
