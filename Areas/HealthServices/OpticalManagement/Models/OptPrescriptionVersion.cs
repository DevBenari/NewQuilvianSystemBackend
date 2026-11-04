using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models
{
    /// <summary>Satu isi resep. Tidak pernah diubah; silinder selalu bernotasi minus dan teks asli resep plus tersimpan di <see cref="OriginalNotationText"/> (<c>OPT-DEC-045</c>).</summary>
    /// <remarks>OPT-BP-001, BE-OPT-002 — kolom mengikuti <c>erd/data-dictionary.md</c> r1.</remarks>
    [Table("OptPrescriptionVersion", Schema = "public")]
    public class OptPrescriptionVersion : IdentityModel
    {
        /// <summary>Kunci utama</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Induk resep</summary>
        [Required]
        public Guid PrescriptionId { get; set; }

        public OptPrescription? Prescription { get; set; }

        /// <summary>1, 2, 3 …</summary>
        public int VersionNo { get; set; }

        /// <summary>Kacamata SPH mata kanan; −20,00..+20,00 kelipatan 0,25 SENSITIF: jangan masuk logger.</summary>
        public decimal? OdSph { get; set; }

        /// <summary>Kacamata CYL; −6,00..0,00 kelipatan 0,25 (notasi minus) SENSITIF: jangan masuk logger.</summary>
        public decimal? OdCyl { get; set; }

        /// <summary>0..180; wajib bila CYL ≠ 0, kosong bila CYL = 0 SENSITIF: jangan masuk logger.</summary>
        public int? OdAxis { get; set; }

        /// <summary>+0,75..+3,50 kelipatan 0,25 SENSITIF: jangan masuk logger.</summary>
        public decimal? OdAdd { get; set; }

        /// <summary>Kacamata SPH mata kiri; −20,00..+20,00 kelipatan 0,25 SENSITIF: jangan masuk logger.</summary>
        public decimal? OsSph { get; set; }

        /// <summary>Kacamata CYL; −6,00..0,00 kelipatan 0,25 (notasi minus) SENSITIF: jangan masuk logger.</summary>
        public decimal? OsCyl { get; set; }

        /// <summary>0..180; wajib bila CYL ≠ 0, kosong bila CYL = 0 SENSITIF: jangan masuk logger.</summary>
        public int? OsAxis { get; set; }

        /// <summary>+0,75..+3,50 kelipatan 0,25 SENSITIF: jangan masuk logger.</summary>
        public decimal? OsAdd { get; set; }

        /// <summary>50..80 mm SENSITIF: jangan masuk logger.</summary>
        public decimal? PdDistance { get; set; }

        /// <summary>50..80 mm SENSITIF: jangan masuk logger.</summary>
        public decimal? PdNear { get; set; }

        /// <summary>Lensa kontak power mata kanan; kosmetik tanpa ukuran = 0 SENSITIF: jangan masuk logger.</summary>
        public decimal? OdClPower { get; set; }

        /// <summary>Lensa kontak torik CYL, opsional SENSITIF: jangan masuk logger.</summary>
        public decimal? OdClCyl { get; set; }

        /// <summary>Lensa kontak torik AXIS, wajib bila CYL ≠ 0 SENSITIF: jangan masuk logger.</summary>
        public int? OdClAxis { get; set; }

        /// <summary>BC, rentang dari OptSetting SENSITIF: jangan masuk logger.</summary>
        public decimal? OdClBaseCurve { get; set; }

        /// <summary>DIA, rentang dari OptSetting SENSITIF: jangan masuk logger.</summary>
        public decimal? OdClDiameter { get; set; }

        /// <summary>Lensa kontak power mata kiri; kosmetik tanpa ukuran = 0 SENSITIF: jangan masuk logger.</summary>
        public decimal? OsClPower { get; set; }

        /// <summary>Lensa kontak torik CYL, opsional SENSITIF: jangan masuk logger.</summary>
        public decimal? OsClCyl { get; set; }

        /// <summary>Lensa kontak torik AXIS, wajib bila CYL ≠ 0 SENSITIF: jangan masuk logger.</summary>
        public int? OsClAxis { get; set; }

        /// <summary>BC, rentang dari OptSetting SENSITIF: jangan masuk logger.</summary>
        public decimal? OsClBaseCurve { get; set; }

        /// <summary>DIA, rentang dari OptSetting SENSITIF: jangan masuk logger.</summary>
        public decimal? OsClDiameter { get; set; }

        /// <summary>Wajib untuk resep lensa kontak (OPT-DEC-022)</summary>
        [MaxLength(150)]
        public string? ContactLensBrandOrType { get; set; }

        /// <summary>Harian/bulanan/…</summary>
        [MaxLength(50)]
        public string? ContactLensReplacement { get; set; }

        /// <summary>Benar bila resep luar plus diubah ke notasi minus (OPT-DEC-045)</summary>
        public bool IsTransposed { get; set; } = false;

        /// <summary>Teks asli resep sebelum transposisi, wajib bila IsTransposed SENSITIF: jangan masuk logger.</summary>
        [MaxLength(200)]
        public string? OriginalNotationText { get; set; }

        /// <summary>Alasan Sp.M melewati rentang; kosong untuk RO</summary>
        [MaxLength(500)]
        public string? RangeOverrideReason { get; set; }

        /// <summary>Penulis/penyalin/perevisi versi ini</summary>
        [Required]
        public Guid AuthorWorkforceProfileId { get; set; }

        /// <summary>SIP yang berlaku saat menulis</summary>
        [Required]
        [MaxLength(100)]
        public string AuthorLicenseNumberSnapshot { get; set; } = string.Empty;

        /// <summary>Waktu versi dibuat</summary>
        public DateTime AuthoredAt { get; set; } = DateTime.UtcNow;

        /// <summary>Wajib untuk versi ≥ 2</summary>
        [MaxLength(500)]
        public string? RevisionReason { get; set; }
    }
}
