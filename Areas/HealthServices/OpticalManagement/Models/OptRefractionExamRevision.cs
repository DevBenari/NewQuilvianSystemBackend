using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models
{
    /// <summary>Satu versi hasil ukur pemeriksaan refraksi. Versi tidak pernah diubah; koreksi menambah versi baru (<c>OPT-DEC-047</c>).</summary>
    /// <remarks>OPT-BP-001, BE-OPT-002 — kolom mengikuti <c>erd/data-dictionary.md</c> r1.</remarks>
    [Table("OptRefractionExamRevision", Schema = "public")]
    public class OptRefractionExamRevision : IdentityModel
    {
        /// <summary>Kunci utama</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Induk pemeriksaan</summary>
        [Required]
        public Guid RefractionExamId { get; set; }

        public OptRefractionExam? RefractionExam { get; set; }

        /// <summary>1, 2, 3 …</summary>
        public int RevisionNo { get; set; }

        /// <summary>Visus jauh tanpa koreksi, mata kanan; notasi Snellen, contoh 6/12 (PROPOSED RCG n1) SENSITIF: jangan masuk logger.</summary>
        [MaxLength(10)]
        public string? VisusDistanceUncorrectedOd { get; set; }

        /// <summary>Visus jauh tanpa koreksi, mata kiri; notasi Snellen, contoh 6/12 (PROPOSED RCG n1) SENSITIF: jangan masuk logger.</summary>
        [MaxLength(10)]
        public string? VisusDistanceUncorrectedOs { get; set; }

        /// <summary>Visus jauh dengan koreksi, mata kanan; notasi Snellen, contoh 6/12 (PROPOSED RCG n1) SENSITIF: jangan masuk logger.</summary>
        [MaxLength(10)]
        public string? VisusDistanceCorrectedOd { get; set; }

        /// <summary>Visus jauh dengan koreksi, mata kiri; notasi Snellen, contoh 6/12 (PROPOSED RCG n1) SENSITIF: jangan masuk logger.</summary>
        [MaxLength(10)]
        public string? VisusDistanceCorrectedOs { get; set; }

        /// <summary>Visus dekat tanpa koreksi, mata kanan; notasi Snellen, contoh 6/12 (PROPOSED RCG n1) SENSITIF: jangan masuk logger.</summary>
        [MaxLength(10)]
        public string? VisusNearUncorrectedOd { get; set; }

        /// <summary>Visus dekat tanpa koreksi, mata kiri; notasi Snellen, contoh 6/12 (PROPOSED RCG n1) SENSITIF: jangan masuk logger.</summary>
        [MaxLength(10)]
        public string? VisusNearUncorrectedOs { get; set; }

        /// <summary>Visus dekat dengan koreksi, mata kanan; notasi Snellen, contoh 6/12 (PROPOSED RCG n1) SENSITIF: jangan masuk logger.</summary>
        [MaxLength(10)]
        public string? VisusNearCorrectedOd { get; set; }

        /// <summary>Visus dekat dengan koreksi, mata kiri; notasi Snellen, contoh 6/12 (PROPOSED RCG n1) SENSITIF: jangan masuk logger.</summary>
        [MaxLength(10)]
        public string? VisusNearCorrectedOs { get; set; }

        /// <summary>Autorefraktometer SPH mata kanan SENSITIF: jangan masuk logger.</summary>
        public decimal? AutorefOdSph { get; set; }

        /// <summary>Autorefraktometer CYL, notasi minus SENSITIF: jangan masuk logger.</summary>
        public decimal? AutorefOdCyl { get; set; }

        /// <summary>Autorefraktometer AXIS 0..180 SENSITIF: jangan masuk logger.</summary>
        public int? AutorefOdAxis { get; set; }

        /// <summary>Autorefraktometer SPH mata kiri SENSITIF: jangan masuk logger.</summary>
        public decimal? AutorefOsSph { get; set; }

        /// <summary>Autorefraktometer CYL, notasi minus SENSITIF: jangan masuk logger.</summary>
        public decimal? AutorefOsCyl { get; set; }

        /// <summary>Autorefraktometer AXIS 0..180 SENSITIF: jangan masuk logger.</summary>
        public int? AutorefOsAxis { get; set; }

        /// <summary>Koreksi subjektif SPH mata kanan SENSITIF: jangan masuk logger.</summary>
        public decimal? SubjectiveOdSph { get; set; }

        /// <summary>Koreksi subjektif CYL, notasi minus SENSITIF: jangan masuk logger.</summary>
        public decimal? SubjectiveOdCyl { get; set; }

        /// <summary>Koreksi subjektif AXIS SENSITIF: jangan masuk logger.</summary>
        public int? SubjectiveOdAxis { get; set; }

        /// <summary>Tambahan baca (ADD) SENSITIF: jangan masuk logger.</summary>
        public decimal? SubjectiveOdAdd { get; set; }

        /// <summary>Koreksi subjektif SPH mata kiri SENSITIF: jangan masuk logger.</summary>
        public decimal? SubjectiveOsSph { get; set; }

        /// <summary>Koreksi subjektif CYL, notasi minus SENSITIF: jangan masuk logger.</summary>
        public decimal? SubjectiveOsCyl { get; set; }

        /// <summary>Koreksi subjektif AXIS SENSITIF: jangan masuk logger.</summary>
        public int? SubjectiveOsAxis { get; set; }

        /// <summary>Tambahan baca (ADD) SENSITIF: jangan masuk logger.</summary>
        public decimal? SubjectiveOsAdd { get; set; }

        /// <summary>Jarak pupil jauh (mm) SENSITIF: jangan masuk logger.</summary>
        public decimal? PdDistance { get; set; }

        /// <summary>Jarak pupil dekat (mm) SENSITIF: jangan masuk logger.</summary>
        public decimal? PdNear { get; set; }

        /// <summary>Keratometri, opsional SENSITIF: jangan masuk logger.</summary>
        [MaxLength(200)]
        public string? KeratometryNote { get; set; }

        /// <summary>Catatan klinis pemeriksa SENSITIF: jangan masuk logger.</summary>
        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }

        /// <summary>Pembuat revisi ini</summary>
        [Required]
        public Guid RevisedByWorkforceProfileId { get; set; }

        /// <summary>Waktu revisi</summary>
        public DateTime RevisedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Wajib untuk revisi sesudah Final (OPT-DEC-047)</summary>
        [MaxLength(500)]
        public string? RevisionReason { get; set; }
    }
}
