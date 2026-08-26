using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>
    /// Bentuk permintaan mengalihkan DPJP. Penugasan lama ditutup dan penugasan baru dibuka
    /// pada tindakan yang sama.
    /// </summary>
    /// <remarks>
    /// <b>Tidak ada kolom waktu mulai.</b> Pengalihan berlaku sejak permintaannya diterima.
    /// Menerima waktu dari pemanggil membuka jalan bagi periode yang tumpang tindih maupun
    /// berlubang, dan riwayat berperiode kehilangan gunanya begitu itu terjadi.
    /// </remarks>
    public class HandoverDoctorRequest
    {
        /// <summary>DPJP baru. Wajib, dan wajib berbeda dari DPJP yang sedang berlaku.</summary>
        public Guid DoctorId { get; set; }

        /// <summary>
        /// Alasan pengalihan. Wajib diisi — <c>RWI-RULE-016</c> menolak pengalihan tanpa
        /// alasan, karena riwayat DPJP dipakai resume pulang dan penagihan.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string HandoverReason { get; set; } = string.Empty;
    }

    /// <summary>Satu baris riwayat penugasan DPJP.</summary>
    public class InpatientDoctorAssignmentResponse
    {
        public Guid Id { get; set; }

        public Guid EpisodeId { get; set; }

        public Guid DoctorId { get; set; }

        public string? DoctorName { get; set; }

        public int SequenceNumber { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        /// <summary>Benar bila penugasan ini yang sedang berlaku, yaitu belum ditutup.</summary>
        public bool IsCurrent { get; set; }

        public Guid AssignedByUserId { get; set; }

        public string? HandoverReason { get; set; }
    }

    /// <summary>
    /// Bentuk permintaan menugaskan perawat penanggung jawab.
    /// </summary>
    /// <remarks>
    /// Penugasan menutup penugasan sebelumnya dan membuka yang baru. Episode <b>boleh</b>
    /// berjalan tanpa perawat penanggung jawab sama sekali; ketiadaannya tidak menahan satu
    /// pun tindakan, sesuai <c>RWI-DEC-032</c>.
    /// </remarks>
    public class AssignNurseRequest
    {
        /// <summary>Pegawai yang ditugaskan sebagai perawat penanggung jawab.</summary>
        public Guid EmployeeId { get; set; }
    }

    /// <summary>Satu baris riwayat penugasan perawat penanggung jawab.</summary>
    public class InpatientNurseAssignmentResponse
    {
        public Guid Id { get; set; }

        public Guid EpisodeId { get; set; }

        public Guid EmployeeId { get; set; }

        public string? EmployeeName { get; set; }

        public int SequenceNumber { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        public bool IsCurrent { get; set; }

        public Guid AssignedByUserId { get; set; }
    }

    /// <summary>
    /// Bentuk permintaan menetapkan atau mengubah kebutuhan isolasi episode.
    /// </summary>
    /// <remarks>
    /// <b>Sumber catatan tidak dikirim pemanggil.</b> Validation matrix bagian 4A menetapkan
    /// <c>IsolationSource</c> ditentukan sistem: petugas admisi selagi episode masih
    /// <c>Draft</c> menghasilkan <c>AdmissionRecord</c>, dan DPJP aktif setelah episode
    /// berjalan menghasilkan <c>ClinicalDecision</c>. Menerimanya dari pemanggil akan
    /// membuat catatan awal petugas admisi dapat menyamar sebagai keputusan klinis DPJP.
    /// </remarks>
    public class SetIsolationRequirementRequest
    {
        /// <summary>Benar bila pasien membutuhkan isolasi.</summary>
        public bool RequiresIsolation { get; set; }

        /// <summary>
        /// Keterangan kebutuhan isolasi. Wajib diisi ketika kebutuhan dinyalakan.
        /// Kolom sensitif; tidak boleh masuk payload logger.
        /// </summary>
        [MaxLength(500)]
        public string? IsolationNote { get; set; }
    }
}
