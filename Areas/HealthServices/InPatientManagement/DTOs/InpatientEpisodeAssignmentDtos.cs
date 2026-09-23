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

    /// <summary>
    /// Bentuk permintaan kepala ruangan atau supervisor melibatkan dokter pendukung: konsulen,
    /// dokter jaga, atau penugasan singkat penulisan catatan terlambat. <c>BE-RWI-080</c>.
    /// </summary>
    /// <remarks>
    /// <b>Bukan jalur pengalihan DPJP.</b> Penugasan yang dibuat lewat bentuk ini tidak pernah
    /// berperan DPJP dan tidak pernah menggeser DPJP yang sedang berlaku — <c>INV-INP-12</c>.
    /// Pengalihan DPJP tetap memakai <see cref="HandoverDoctorRequest"/>, dan pemisahan itu
    /// disengaja: dua tindakan dengan akibat sangat berbeda tidak boleh dibedakan hanya oleh
    /// satu nilai enum di dalam badan permintaan yang sama.
    ///
    /// <para>
    /// <b>Dokter tidak dapat menugaskan dirinya sendiri.</b> Yang menekan tombolnya wajib
    /// kepala ruangan atau supervisor — <c>RWI-DEC-130</c> (4). Tanpa penjaga itu, seorang
    /// dokter dapat memberi dirinya akses menulis rekam medis pasien yang bukan tanggung
    /// jawabnya hanya dengan membuat satu baris penugasan.
    /// </para>
    /// </remarks>
    public class AssignSupportingDoctorRequest
    {
        /// <summary>Dokter yang dilibatkan. Wajib, dan wajib aktif pada master dokter.</summary>
        public Guid DoctorId { get; set; }

        /// <summary>
        /// Peran penugasan: <c>2</c> konsulen, <c>3</c> dokter jaga. Nilai <c>1</c> DPJP
        /// ditolak — <c>VAL-INP-09</c>.
        /// </summary>
        public int AssignmentRole { get; set; }

        /// <summary>
        /// Tujuan penugasan: <c>0</c> penugasan biasa, <c>1</c> penugasan singkat penulisan
        /// catatan terlambat. Bawaannya <c>0</c>.
        /// </summary>
        /// <remarks>
        /// Tujuan <c>1</c> menuntut tiga hal sekaligus: peran dokter jaga, waktu selesai yang
        /// terisi dan lebih besar dari waktu mulai, serta alasan yang terisi — <c>INV-INP-12</c>
        /// dan check constraint <c>CK_InpDoctorAssignment_LateDocumentation</c>.
        /// </remarks>
        public int AssignmentPurpose { get; set; }

        /// <summary>
        /// Waktu mulai berlakunya penugasan. Dikosongkan berarti sekarang.
        /// </summary>
        /// <remarks>
        /// <b>Diabaikan untuk penugasan singkat penulisan catatan terlambat.</b> Jendela
        /// penulisan selalu dibuka "sekarang": jendela yang dapat dimundurkan pemanggil
        /// memungkinkan sebuah catatan ditulis seolah-olah dibuat pada masa yang sudah lewat —
        /// roadmap <c>BE-RWI-080</c> acceptance criteria 3.
        /// </remarks>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// Waktu berakhirnya penugasan. <b>Wajib</b> untuk penugasan singkat penulisan catatan
        /// terlambat; opsional untuk konsulen dan dokter jaga biasa.
        /// </summary>
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// Alasan pelibatan. Wajib diisi — <c>VAL-INP-01</c>. Riwayat penugasan dibaca resume
        /// pulang, penagihan, dan audit kewenangan menulis; baris tanpa alasan membuat ketiganya
        /// tidak dapat dijelaskan.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Bentuk permintaan mengakhiri penugasan konsulen atau dokter jaga. <c>BE-RWI-080</c>.
    /// </summary>
    /// <remarks>
    /// <b>Mengakhiri, bukan menghapus.</b> Barisnya tetap ada beserta seluruh periodenya; yang
    /// berubah hanya waktu selesainya. Pertanyaan "siapa yang berwenang atas pasien ini pada
    /// 22 September" tetap dapat dijawab sesudah konsultasinya selesai.
    /// </remarks>
    public class EndSupportingAssignmentRequest
    {
        /// <summary>
        /// Waktu berakhir. Dikosongkan berarti sekarang. Tidak boleh mendahului waktu mulai
        /// penugasan dan tidak boleh melewati waktu sekarang.
        /// </summary>
        public DateTime? EndDateTime { get; set; }

        /// <summary>Alasan pengakhiran. Opsional.</summary>
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    /// <summary>Satu baris riwayat penugasan DPJP.</summary>
    public class InpatientDoctorAssignmentResponse
    {
        public Guid Id { get; set; }

        public Guid EpisodeId { get; set; }

        public Guid DoctorId { get; set; }

        public string? DoctorName { get; set; }

        /// <summary>
        /// Peran dokter pada baris ini: <c>1</c> DPJP, <c>2</c> konsulen, <c>3</c> dokter
        /// jaga. Ditambahkan <c>BE-RWI-074</c>.
        /// </summary>
        /// <remarks>
        /// Tanpa field ini, riwayat penugasan menyajikan konsulen dan DPJP sebagai baris yang
        /// tidak dapat dibedakan, dan layar tidak punya cara menampilkan siapa yang sebenarnya
        /// bertanggung jawab.
        /// </remarks>
        public int AssignmentRole { get; set; }

        /// <summary>
        /// Alasan penugasan ini dibuat: <c>0</c> penugasan biasa, <c>1</c> penugasan singkat
        /// penulisan catatan terlambat. Ditambahkan <c>BE-RWI-079</c>.
        /// </summary>
        /// <remarks>
        /// Tanpa field ini, layar menampilkan dokter jaga yang benar-benar menjaga shift dan
        /// dokter jaga yang hanya diberi jendela satu jam untuk menulis catatan sebagai dua
        /// baris yang tidak dapat dibedakan — <c>RWI-DEC-130</c>.
        /// </remarks>
        public int AssignmentPurpose { get; set; }

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
