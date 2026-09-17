using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models
{
    /// <summary>
    /// Riwayat dokter penanggung jawab satu kunjungan IGD.
    /// </summary>
    /// <remarks>
    /// <b>Tambah-saja.</b> Baris lama tidak pernah ditimpa: pengalihan menutup baris berjalan
    /// dengan mengisi <see cref="EffectiveTo"/>, lalu membuka baris baru — <c>IGD-DEC-082</c>.
    /// Menimpa baris lama akan menulis ulang sejarah, karena dokumen yang ditulis semasa dokter
    /// sebelumnya bertanggung jawab mendadak terbaca seakan milik dokter penggantinya.
    ///
    /// <para>
    /// <b>Tidak ada kolom <c>IsActive</c> — <c>IGD-DEC-130</c>.</b> Rancangan sebelumnya memuat
    /// <c>IsActive</c> <i>dan</i> <see cref="EffectiveTo"/>, padahal keduanya menyatakan fakta
    /// yang sama dan hanya <see cref="EffectiveTo"/> yang dijaga unique bersyarat. Dua penanda
    /// untuk satu fakta pasti berbeda suatu hari — satu diperbarui, satunya tertinggal. Kolom
    /// itu dihapus <i>sebelum</i> model dan migration dibuat, bukan ditambahkan lalu dicabut.
    /// </para>
    ///
    /// <para>Penentu waktunya tinggal satu:</para>
    /// <list type="bullet">
    ///   <item>Penugasan berjalan: <c>EffectiveTo IS NULL</c>.</item>
    ///   <item>Penugasan pada waktu <c>T</c>:
    ///     <c>EffectiveFrom &lt;= T AND (EffectiveTo IS NULL OR T &lt; EffectiveTo)</c>.</item>
    /// </list>
    ///
    /// <para>
    /// <c>IsDelete</c> dan <c>IsCancel</c> bawaan <see cref="IdentityModel"/> tetap ada sebagai
    /// urusan validitas baris dan audit, dan <b>bukan</b> pengganti <see cref="EffectiveTo"/>.
    /// </para>
    ///
    /// <para>
    /// Pola aslinya <c>InpDoctorAssignment</c>. Dua hal sengaja <b>tidak</b> ditiru:
    /// <c>IsActive</c> (dilarang <c>IGD-DEC-130</c>) dan <c>SequenceNumber</c> — kamus data §4
    /// tidak memuatnya, dan urutan riwayat sudah terbaca dari <see cref="EffectiveFrom"/>.
    /// </para>
    /// </remarks>
    [Table("EmgDoctorAssignment", Schema = "public")]
    public class EmgDoctorAssignment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmergencyVisitId { get; set; }

        [Required]
        public Guid DoctorId { get; set; }

        /// <summary>
        /// Saat penugasan ini mulai berlaku. Tidak boleh mendahului waktu kedatangan pasien —
        /// aturan itu ditegakkan service, bukan kolomnya.
        /// </summary>
        [Required]
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Saat penugasan ini berakhir. <c>null</c> berarti penugasan <b>sedang berjalan</b>,
        /// dan itulah satu-satunya penanda penugasan berjalan (<c>IGD-DEC-130</c>).
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// Pengguna yang menetapkan atau mengalihkan. Diisi sistem dari pengguna aktif, bukan
        /// dari kiriman pemanggil.
        /// </summary>
        [Required]
        public Guid AssignedByUserId { get; set; }

        /// <summary>
        /// Alasan pengalihan. Wajib saat <b>pengalihan</b>, tidak wajib saat penetapan pertama;
        /// kewajiban bersyarat itu milik service, sehingga kolomnya tetap nullable.
        /// </summary>
        [MaxLength(500)]
        public string? AssignmentReason { get; set; }

        public EmgVisit? EmergencyVisit { get; set; }

        public MstDoctor? Doctor { get; set; }

        public ApplicationUser? AssignedByUser { get; set; }
    }
}
