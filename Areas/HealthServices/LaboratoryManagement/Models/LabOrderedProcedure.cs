using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Satu jenis pemeriksaan yang <b>diminta</b> untuk seorang pasien (<c>LAB-DEC-057</c>).
    ///
    /// <b>Mengapa entity ini ada, dan kenapa ia bukan <see cref="LabExamination"/>.</b> Keduanya
    /// tampak menjawab hal yang sama, tetapi sebenarnya menjawab dua pertanyaan berbeda pada dua
    /// saat berbeda:
    ///
    /// <list type="bullet">
    /// <item><b>Entity ini</b> menjawab <i>apa yang diminta</i>. Ia lahir saat pendaftaran,
    /// ketika petugas memilih pemeriksaan untuk pasien — dan pada saat itu belum ada satu pun
    /// wadah fisik. Ia tidak membawa akibat finansial apa pun.</item>
    /// <item><see cref="LabExamination"/> menjawab <i>apa yang dikerjakan dari sebuah wadah</i>.
    /// Ia lahir saat wadah dicatat, membawa salinan tarifnya sendiri, dan menerbitkan fakta
    /// kelayakan tagih per baris (<c>AC-37</c>).</item>
    /// </list>
    ///
    /// <b>Kenapa keduanya tidak boleh digabung.</b> <see cref="LabExamination.SpecimenId"/>
    /// wajib dan menjadi bagian unique index <c>(SpecimenId, ProcedureId)</c> yang dipasang atas
    /// dasar <c>BR-20</c> dan <c>AC-35</c>. Artinya pemeriksaan tidak dapat hidup sebelum
    /// wadahnya ada — dan permintaan pemeriksaan justru selalu mendahului wadahnya.
    ///
    /// Alternatif yang ditolak adalah membuat <c>SpecimenId</c> nullable. Di PostgreSQL dua
    /// baris ber-<c>NULL</c> <b>tidak</b> saling bentrok pada unique index, sehingga penjagaan
    /// <c>BR-20</c> akan bocor diam-diam. Index yang sama sudah membatalkan <c>BE-LAB-23</c>;
    /// melemahkannya sekarang berarti membayar dua kali untuk pelajaran yang sama.
    ///
    /// <b>Yang sengaja tidak ada di sini.</b> Nol kolom tarif, nol kolom hasil, nol
    /// <c>ChargeEligibleAt</c>. Permintaan belum menagihkan apa pun; yang menagihkan adalah
    /// wadah yang dinyatakan layak.
    /// </summary>
    public class LabOrderedProcedure : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Pesanan yang memuat permintaan ini.</summary>
        [Required]
        public Guid LabOrderId { get; set; }

        /// <summary>Jenis pemeriksaan yang diminta. Wajib berpenanda <c>IsLaboratory</c>.</summary>
        [Required]
        public Guid ProcedureId { get; set; }

        /// <summary>Salinan kode jenis pemeriksaan saat dipesan.</summary>
        public string? ProcedureCodeSnapshot { get; set; }

        /// <summary>Salinan nama jenis pemeriksaan saat dipesan.</summary>
        public string? ProcedureNameSnapshot { get; set; }

        /// <summary>
        /// Disiplin jenis pemeriksaan ini <b>pada saat dipesan</b>.
        ///
        /// Disalin, bukan dibaca ulang dari katalog. Penggolongan katalog yang berubah kemudian
        /// tidak boleh mengubah makna permintaan yang sudah terjadi — sama seperti salinan kode
        /// dan nama di atas.
        ///
        /// Boleh kosong: pemeriksaan yang belum digolongkan tetap sah dipesan (<c>AC-85</c>).
        /// </summary>
        public LabDiscipline? DisciplineSnapshot { get; set; }

        /// <summary>
        /// Kesegeraan permintaan ini. Penanda cito melekat pada pemeriksaan, bukan pada pesanan
        /// (<c>LAB-DEC-026</c>) — satu pesanan boleh memuat cito dan biasa sekaligus.
        /// </summary>
        public LabExaminationUrgency Urgency { get; set; } = LabExaminationUrgency.Routine;

        /// <summary>Sudah masuk wadah, masih menunggu, atau dibatalkan sebelum sempat dikerjakan.</summary>
        public LabOrderedProcedureStatus OrderedStatus { get; set; } = LabOrderedProcedureStatus.Ordered;

        /// <summary>
        /// Baris pemeriksaan yang akhirnya mengerjakan permintaan ini.
        ///
        /// Kosong selama permintaannya belum masuk wadah. Inilah tautan yang membuat pertanyaan
        /// "mana yang masih menunggu wadah" dapat dijawab tanpa menebak (<c>AC-91</c>).
        /// </summary>
        public Guid? FulfilledExaminationId { get; set; }

        public LabOrder? LabOrder { get; set; }

        public MstProcedure? Procedure { get; set; }

        public LabExamination? FulfilledExamination { get; set; }
    }
}
