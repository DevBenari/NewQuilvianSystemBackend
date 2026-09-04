using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Riwayat perpindahan status pesanan dan sampel laboratorium.
    ///
    /// Baris pada tabel ini hanya ditambahkan, tidak pernah diubah dan tidak pernah dihapus
    /// oleh alur operasional. Service tidak menyediakan satu pun jalur update terhadapnya,
    /// sehingga alasan penolakan yang pernah tercatat tetap dapat dibaca walaupun kemudian
    /// terjadi pengambilan ulang atau pembatalan.
    /// </summary>
    public class LabTransitionHistory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid LabOrderId { get; set; }

        public Guid? LabSpecimenId { get; set; }

        /// <summary>
        /// Terisi bila yang berpindah adalah satu pemeriksaan terpesan (<c>LAB-DEC-026</c>).
        ///
        /// Kosong pada baris berlingkup pesanan maupun wadah. Ketiga penunjuk itu tidak saling
        /// menggantikan: satu baris riwayat menyebut pesanannya selalu, wadahnya bila memang
        /// wadah yang berpindah, dan pemeriksaannya bila memang pemeriksaan yang berpindah.
        /// </summary>
        public Guid? LabExaminationId { get; set; }

        [Required]
        public Guid EncounterId { get; set; }

        public LabTransitionScope Scope { get; set; }

        /// <summary>Nama tindakan operasional, misalnya <c>Specimen.Accept</c>.</summary>
        [Required]
        public string Action { get; set; } = string.Empty;

        /// <summary>Status sebelum perpindahan. Kosong pada pembuatan baris pertama.</summary>
        public string? FromStatus { get; set; }

        [Required]
        public string ToStatus { get; set; } = string.Empty;

        /// <summary>
        /// Salinan kode alasan pada saat kejadian, bukan hanya foreign key. Disimpan sebagai
        /// teks agar penonaktifan alasan di kemudian hari tidak mengubah makna riwayat lama.
        /// </summary>
        public string? ReasonCode { get; set; }

        public string? ReasonNote { get; set; }

        [Required]
        public Guid ActorUserId { get; set; }

        public DateTime OccurredAt { get; set; }

        public Guid? CorrelationId { get; set; }

        public LabOrder? LabOrder { get; set; }

        public LabSpecimen? LabSpecimen { get; set; }

        public LabExamination? LabExamination { get; set; }
    }
}
