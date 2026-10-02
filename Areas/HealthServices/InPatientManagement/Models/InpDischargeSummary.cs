using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    [Table("InpDischargeSummary", Schema = "public")]
    public class InpDischargeSummary : IdentityModel
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string PrimaryDiagnosisText { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? SecondaryDiagnosisText { get; set; }

        [MaxLength(2000)]
        public string? ProcedureSummary { get; set; }

        [MaxLength(2000)]
        public string? DischargeMedicationNote { get; set; }

        [MaxLength(2000)]
        public string? FollowUpInstruction { get; set; }

        [MaxLength(250)]
        public string? ReferralDestination { get; set; }

        [MaxLength(4000)]
        public string? ClinicalSummary { get; set; }

        /// <summary>
        /// Pemeriksaan Penting — hasil penunjang yang membentuk keputusan klinis selama
        /// perawatan. Kolom baru <c>BE-RWI-085</c> menyerap <c>RWI-DEC-112</c>. <b>SENSITIF.</b>
        /// </summary>
        /// <remarks>
        /// Contoh isinya: "Hb 7,8 g/dL (12/09) → transfusi 2 kolf; Rontgen toraks: efusi pleura
        /// kanan". Dokter penerima di fasilitas berikutnya membaca bagian ini lebih dulu, karena
        /// di sinilah alasan sebuah tindakan diambil terbaca — bukan pada daftar tindakannya.
        ///
        /// <para>
        /// <b>Nullable, dan itu disengaja.</b> Seluruh resume yang sudah ada — termasuk yang
        /// sudah ditandatangani — tetap terbaca dan tetap dapat ditandatangani tanpa isian ini.
        /// Isi minimal resume masih berada di bawah gerbang pemilik klinis
        /// (<c>RWI-RULE-032</c>, pertanyaan terbuka 22.7 nomor 3), sehingga menjadikannya wajib
        /// sekarang akan menahan pemulangan pasien atas kebijakan yang belum diputuskan.
        /// </para>
        /// </remarks>
        [MaxLength(4000)]
        public string? ImportantFindingsSummary { get; set; }

        /// <summary>
        /// Kondisi Saat Pulang — keadaan pasien pada saat meninggalkan rumah sakit. Kolom baru
        /// <c>BE-RWI-085</c>. <b>SENSITIF.</b>
        /// </summary>
        /// <remarks>
        /// Contoh isinya: "Sadar penuh, TD 120/80, jalan sendiri, luka operasi kering". Inilah
        /// garis dasar yang dipakai fasilitas berikutnya untuk menilai apakah pasien membaik
        /// atau memburuk sesudah pulang.
        /// </remarks>
        [MaxLength(2000)]
        public string? DischargeConditionNote { get; set; }

        /// <summary>
        /// Edukasi — apa yang sudah dijelaskan kepada pasien dan keluarganya. Kolom baru
        /// <c>BE-RWI-085</c>. <b>SENSITIF.</b>
        /// </summary>
        /// <remarks>
        /// Contoh isinya: "Diet rendah garam; tanda bahaya sesak — segera ke IGD; kontrol poli
        /// jantung 22/09". Berbeda dari <see cref="FollowUpInstruction"/> yang berisi rencana
        /// kontrol, kolom ini mencatat apa yang <b>sudah</b> disampaikan, bukan apa yang harus
        /// dilakukan.
        /// </remarks>
        [MaxLength(2000)]
        public string? EducationSummary { get; set; }

        public DateTime? SignedAt { get; set; }

        public Guid? SignedByDoctorId { get; set; }

        public bool IsActive { get; set; } = true;

        public InpEpisode? Episode { get; set; }

        public MstDoctor? SignedByDoctor { get; set; }

        public ICollection<InpDischargeSummaryRevision> Revisions { get; set; } = new List<InpDischargeSummaryRevision>();
    }
}
