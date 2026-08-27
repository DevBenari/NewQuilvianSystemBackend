using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Katalog alasan penolakan sampel yang dapat dikonfigurasi.
    ///
    /// Keputusan author <c>RJ-BIL-OQ-009</c> menolak free-text sebagai satu-satunya alasan dan
    /// menolak enum permanen di dalam source. Karena itu alasan disimpan sebagai master data:
    /// Lab dan Clinical Governance dapat menambah, menonaktifkan, atau memperinci alasan tanpa
    /// perubahan program dan tanpa merusak riwayat yang sudah tersimpan.
    ///
    /// Daftar bawaan yang diisi migration adalah baseline implementasi, bukan SOP klinis final.
    /// </summary>
    public class MstLabRejectionReason : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string ReasonCode { get; set; } = string.Empty;

        [Required]
        public string ReasonName { get; set; } = string.Empty;

        public string? Description { get; set; }

        /// <summary>
        /// Menandai alasan yang berakar pada kesalahan internal rumah sakit. Penanda ini tidak
        /// menghitung apa pun secara finansial; ia hanya menyertakan sebab pada fakta klinis
        /// sehingga Billing dapat menerapkan aturan FOC atau write-off tanpa menebak.
        /// </summary>
        public bool IsInternalHospitalError { get; set; }

        /// <summary>Menandai alasan yang mewajibkan petugas mengisi catatan tambahan.</summary>
        public bool RequiresNote { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }
    }
}
