using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models
{
    /// <summary>
    /// Bukti bahwa pemeriksaan kecocokan satu kantong terhadap satu pasien sudah
    /// dinyatakan selesai oleh petugas BDRS berwenang validasi.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Entity ini <b>bukan hasil perhitungan kompatibilitas oleh Quilvian</b>.
    /// Sistem hanya mencatat keputusan manusia (INV-BD-013, DEC-BD-042).
    /// </para>
    ///
    /// <para>
    /// Bukti selalu terikat pada pasangan <see cref="BloodUnitId"/> +
    /// <see cref="PatientId"/>. Karena itu bukti milik pasien A tidak pernah dapat
    /// dipakai untuk pasien B (INV-BD-020).
    /// </para>
    ///
    /// <para>
    /// Masa berlaku tidak disalin ke entity ini. Nilainya selalu dihitung ketika
    /// gerbang pemberian dievaluasi dari <see cref="CheckedAt"/> ditambah
    /// <c>MstBloodComponent.CompatibilityEvidenceValidityHours</c>
    /// (DEC-BD-027, INV-BD-023).
    /// </para>
    ///
    /// <para>
    /// Bukti tidak dihapus ketika tidak lagi berlaku. Pengalihan kantong ke pasien
    /// lain membuat bukti lama <see cref="IsSuperseded"/> tetapi tetap terbaca
    /// sebagai rekam klinis.
    /// </para>
    /// </remarks>
    [Table("BbkCompatibilityEvidence", Schema = "public")]
    public class BbkCompatibilityEvidence : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Kantong yang diperiksa.</summary>
        [Required]
        public Guid BloodUnitId { get; set; }

        [ForeignKey(nameof(BloodUnitId))]
        public BbkBloodUnit? BloodUnit { get; set; }

        /// <summary>
        /// Pasien yang menjadi tujuan pemeriksaan kecocokan.
        /// Nilai ini wajib sama dengan pasien pada alokasi aktif saat evidence dibuat.
        /// </summary>
        [Required]
        public Guid PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public MstPatient? Patient { get; set; }

        /// <summary>
        /// Keputusan pemeriksaan: cocok atau tidak cocok.
        /// Bukti Incompatible tetap disimpan dan menahan pemberian (VAL-BD-079).
        /// </summary>
        [Required]
        public BbkCompatibilityResult EvidenceResult { get; set; }

        /// <summary>
        /// Petugas BDRS berwenang validasi yang menyatakan hasil pemeriksaan.
        /// Pelaksana pemeriksaan boleh orang yang sama atau berbeda (DEC-BD-042).
        /// </summary>
        [Required]
        public Guid ValidatedByUserId { get; set; }

        /// <summary>
        /// Waktu pemeriksaan yang dinyatakan sah. Menjadi awal perhitungan masa berlaku.
        /// </summary>
        [Required]
        public DateTime CheckedAt { get; set; }

        /// <summary>
        /// Bukti sudah digugurkan karena kantong dialihkan ke pasien lain.
        /// Tidak berarti baris dihapus.
        /// </summary>
        public bool IsSuperseded { get; set; }
    }
}