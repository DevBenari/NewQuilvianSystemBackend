using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Daftar terkendali alasan <b>merangkap peran</b> pada hasil yang sama — pemvalidasi yang
    /// juga pengisi hasil, atau perilis yang juga pemvalidasi (<c>LAB-DEC-003</c>,
    /// <c>LAB-DC-058</c>, <c>INV-42</c>, <c>INV-43</c>).
    ///
    /// Bentuk kolomnya sama dengan <see cref="LabResultCorrectionReason"/>; <b>tabelnya sengaja
    /// terpisah</b> (<c>02-backend-architecture.md</c> 20.4).
    ///
    /// <see cref="ReasonName"/> <b>ikut tercetak</b> pada penanda pengecualian, sehingga
    /// <see cref="LabExamination"/> menyalinnya saat itu juga. Membetulkan ejaan alasan di sini
    /// kemudian hari tidak mengubah bunyi hasil lama.
    ///
    /// <b>Nol penghapusan</b> — dinonaktifkan lewat <see cref="IsActive"/>. Hasil yang sudah
    /// memakainya menunjuk ke sini lewat foreign key <c>Restrict</c>.
    /// </summary>
    public class LabFourEyesExceptionReason : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Kode alasan, misalnya <c>SHIFT-TUNGGAL</c>. Unik di antara baris yang belum ditandai terhapus.</summary>
        [Required]
        public string ReasonCode { get; set; } = string.Empty;

        /// <summary>
        /// Misalnya <i>Shift tunggal, tidak ada dokter lain bertugas</i>. Ikut tercetak pada
        /// penanda pengecualian.
        /// </summary>
        [Required]
        public string ReasonName { get; set; } = string.Empty;

        public string? Description { get; set; }

        /// <summary>
        /// Bila <c>true</c>, catatan bebas wajib diisi. Bagi alasan pengecualian, pola ini masih
        /// <b>usulan</b> (<c>ARCH-GAP-LAB-08</c> butir 4).
        /// </summary>
        public bool RequiresNote { get; set; }

        /// <summary>Alasan nonaktif tidak dapat dipilih pada tindakan baru, tetapi tetap terbaca pada hasil lama.</summary>
        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }
    }
}
