using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models
{
    /// <summary>Resep kacamata atau lensa kontak (<c>AGG-OPT-02</c>). Terkunci bila <see cref="LockedAt"/> terisi (<c>OPT-DEC-007</c>); berlaku sampai <see cref="ValidUntil"/> (<c>OPT-DEC-023</c>).</summary>
    /// <remarks>OPT-BP-001, BE-OPT-002 — kolom mengikuti <c>erd/data-dictionary.md</c> r1.</remarks>
    [Table("OptPrescription", Schema = "public")]
    public class OptPrescription : IdentityModel
    {
        /// <summary>Kunci utama</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Contoh OPT-RX-RSMMC-000001</summary>
        [Required]
        [MaxLength(30)]
        public string PrescriptionNumber { get; set; } = string.Empty;

        /// <summary>Pemilik resep</summary>
        [Required]
        public Guid PatientId { get; set; }

        /// <summary>Kunjungan saat dibuat/disalin</summary>
        [Required]
        public Guid EncounterId { get; set; }

        /// <summary>Kosong untuk resep luar</summary>
        public Guid? RefractionExamId { get; set; }

        /// <summary>Spectacle/ContactLens</summary>
        public OptPrescriptionType PrescriptionType { get; set; }

        /// <summary>Internal/External</summary>
        public OptPrescriptionOrigin Origin { get; set; }

        /// <summary>Wajib bila External (OPT-DEC-017)</summary>
        [MaxLength(150)]
        public string? ExternalAuthorName { get; set; }

        /// <summary>Wajib bila External</summary>
        [MaxLength(150)]
        public string? ExternalFacilityName { get; set; }

        /// <summary>Tanggal terbit resep (resep luar: tanggal di kertas resep)</summary>
        public DateOnly IssuedDate { get; set; }

        /// <summary>Usia resep yang dibekukan (OPT-DEC-023, 040)</summary>
        public int ValidityMonthsSnapshot { get; set; }

        /// <summary>Batas berlaku, dibandingkan dengan tanggal WIB</summary>
        public DateOnly ValidUntil { get; set; }

        /// <summary>Versi terakhir</summary>
        public int CurrentVersionNo { get; set; } = 1;

        /// <summary>Terisi saat pesanan yang memakai resep masuk pengerjaan (OPT-DEC-007)</summary>
        public DateTime? LockedAt { get; set; }

        /// <summary>Pesanan pertama yang mengunci</summary>
        public Guid? LockedByOrderId { get; set; }

        /// <summary>Penanda concurrency (IsConcurrencyToken); naik setiap perubahan</summary>
        public int Version { get; set; } = 0;

        /// <summary>Kunci dari header Idempotency-Key agar Simpan dua kali tidak membuat data ganda (OPT-DEC-077). Kosong pada baris lama.</summary>
        public Guid? IdempotencyKey { get; set; }

        public ICollection<OptPrescriptionVersion> Versions { get; set; } = new List<OptPrescriptionVersion>();
    }
}
