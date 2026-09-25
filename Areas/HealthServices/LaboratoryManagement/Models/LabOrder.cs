using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Pesanan pemeriksaan laboratorium.
    ///
    /// Entity ini otoritatif atas keadaan operasional pesanan saja. Tidak ada satu pun kolom
    /// finansial di sini — tidak ada Paid, Settlement, PayerApproval, Void, Refund, maupun
    /// Reversal. Akibat finansial sepenuhnya milik Billing sesuai <c>RJ-BIL-GATE-DEC-003</c>.
    /// </summary>
    public class LabOrder : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Nomor pesanan yang dapat dibaca, dicetak, dan <b>disebut lewat telepon</b>
        /// (<c>LAB-DEC-072</c>). Bentuknya <c>LAB-RSMMC-000001</c>.
        ///
        /// Nilainya dialokasikan <see cref="LabOrderNumberService"/> pada saat pesanan dibuat dan
        /// tidak pernah berubah sesudahnya. Celah penomoran <b>dibiarkan ada</b> dan tidak pernah
        /// diisi ulang — nomor ini dicetak pada amplop hasil pasien, dan dua benda fisik bernomor
        /// sama adalah kesalahan yang tidak terlihat oleh siapa pun. Batas jaminannya ada pada
        /// <see cref="LabOrderNumberService"/>.
        ///
        /// <b>Pola <c>LSP-{Guid:N}</c> milik barcode wadah sengaja tidak dipakai.</b> Barcode
        /// wadah dibaca mesin; nomor ini dibaca dan diucapkan manusia.
        ///
        /// Kolomnya wajib, dan itu disengaja: nomor order tidak punya keadaan "belum". Kolom yang
        /// boleh kosong hanya akan menyembunyikan jalur tulis yang lupa mengalokasikan.
        /// </summary>
        [Required]
        [MaxLength(32)]
        public string OrderNumber { get; set; } = string.Empty;

        /// <summary>
        /// Nomor yang tercetak pada lembar hasil — <c>26-1129</c> — dialokasikan
        /// <b>per disiplin per tahun</b> (<c>LAB-DEC-117</c>).
        ///
        /// <b>Ia BUKAN pengganti <see cref="OrderNumber"/>, dan keduanya hidup berdampingan.</b>
        /// <c>OrderNumber</c> tetap identitas internal serta sumber barcode Label Lab
        /// (<c>LAB-DEC-072</c> utuh); nomor ini dipegang pasien dan disebut lisan antarpetugas.
        /// Mengubah <c>OrderNumber</c> menjadi format cetak berarti membongkar layanan alokasi,
        /// index unik, dan sumber barcode yang sudah berjalan — serta meninggalkan pesanan lama
        /// berformat berbeda selamanya.
        ///
        /// <b>Boleh kosong, berbeda dari <see cref="OrderNumber"/>.</b> Seluruh pesanan yang
        /// sudah ada sebelum kolom ini lahir nol punya nomor cetak, dan membubuhkannya
        /// belakangan akan memberi nomor tahun ini kepada pesanan tahun lalu.
        /// </summary>
        [MaxLength(32)]
        public string? LabReportNumber { get; set; }

        [Required]
        public Guid EncounterId { get; set; }

        /// <summary>
        /// Procedure utama pesanan. Sejak <c>RJ-BIL-BE-003</c> setiap sampel membawa procedure
        /// komponen pemeriksaannya sendiri, sehingga satu pesanan dapat memuat beberapa
        /// pemeriksaan dengan tarif berbeda. Nilai ini tetap dipertahankan sebagai procedure
        /// yang dipesan dokter dan sebagai default komponen pertama.
        /// </summary>
        [Required]
        public Guid ProcedureId { get; set; }

        /// <summary>
        /// Perawatan rawat inap yang menaungi pesanan ini. Boleh kosong.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-042</c>, <c>AC-CAP015-01</c>. Pesanan sudah terikat kunjungan tanpa antrean
        /// maupun catatan dokter, sehingga pemesanan laboratorium rawat inap sudah mungkin
        /// sebelum kolom ini ada; kolom inilah yang membuat kepemilikan perawatannya dapat
        /// dibuktikan.
        /// </remarks>
        public Guid? InpEpisodeId { get; set; }

        /// <summary>
        /// Disiplin yang menaungi pesanan ini — Patologi Klinik, Patologi Anatomi, atau
        /// Mikrobiologi (<c>LAB-DEC-025</c>).
        ///
        /// Nilainya ditetapkan sekali pada saat pesanan dibuat dan tidak dapat berpindah
        /// sesudahnya (<c>INV-21</c>). Larangan itu ditegakkan pada
        /// <c>LabOrderConfiguration</c> lewat <c>PropertySaveBehavior.Throw</c>, bukan hanya
        /// lewat ketiadaan endpoint yang mengubahnya.
        ///
        /// Boleh kosong semata-mata karena pesanan yang sudah terlanjur ada sebelum kolom ini
        /// dibuat memang tidak pernah punya disiplin. Pesanan baru selalu diminta membawanya.
        /// </summary>
        public LabDiscipline? Discipline { get; set; }

        public LabOrderStatus OrderStatus { get; set; } = LabOrderStatus.Requested;

        /// <summary>
        /// Status operasional sebelum pesanan ditahan. Disimpan agar <c>OnHold</c> benar-benar
        /// mempertahankan keadaan sebelumnya dan dapat dilanjutkan tanpa menebak.
        /// </summary>
        public LabOrderStatus? StatusBeforeHold { get; set; }

        public DateTime? RequestedAt { get; set; }

        public Guid? RequestedByUserId { get; set; }

        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Petugas yang mengonfirmasi pesanan ini (<c>LAB-DEC-061</c>).
        ///
        /// Nilainya diturunkan server dari pengguna yang sedang login, tidak pernah dari badan
        /// permintaan. Kosong selama pesanan belum pernah dikonfirmasi.
        /// </summary>
        public Guid? ConfirmedByUserId { get; set; }

        /// <summary>
        /// Waktu pesanan dikonfirmasi (<c>LAB-DEC-061</c>).
        ///
        /// Sengaja didenormalisasi dari jejak audit supaya daftar pesanan tidak perlu menggabung
        /// riwayat baris per baris hanya untuk menampilkan satu tanggal.
        /// </summary>
        public DateTime? ConfirmedAt { get; set; }

        /// <summary>
        /// Dokter pemeriksa yang dipilih saat konfirmasi (<c>LAB-DEC-061</c>). Menunjuk
        /// <c>MstDoctor</c>.
        ///
        /// Boleh kosong semata-mata karena seluruh pesanan yang sudah ada tidak pernah memilih
        /// dokter pemeriksa. Kolom wajib akan menggagalkan migrationnya atau memaksa pengisian
        /// tebakan atas pesanan yang benar-benar sudah terjadi.
        /// </summary>
        public Guid? ExaminerDoctorId { get; set; }

        /// <summary>
        /// Token konkurensi. Dua petugas yang memindahkan status pesanan yang sama secara
        /// bersamaan tidak boleh sama-sama berhasil.
        /// </summary>
        public int Version { get; set; }

        // =====================================================================
        // BE-RWI-104 / migration R8 — kamus data 0.5 bagian 13.11, RWI-DEC-153.
        //
        // Pesanan rawat inap yang dimasukkan perawat membawa dokter pemberi instruksi dan status
        // verifikasinya. Penginput tetap RequestedByUserId yang sudah ada; nol kolom penginput baru.
        // Pesanan poliklinik, IGD, dan pesanan lama bernilai NotRequired tanpa diisi.
        // =====================================================================

        /// <summary>Dokter pemberi instruksi pada pesanan rawat inap yang dibuat perawat.</summary>
        public Guid? InstructingDoctorId { get; set; }

        public LabOrderInstructionVerificationStatus InstructionVerificationStatus { get; set; } = LabOrderInstructionVerificationStatus.NotRequired;

        public DateTime? InstructionVerifiedAt { get; set; }

        /// <summary>Wajib akun dokter pemberi instruksi.</summary>
        public Guid? InstructionVerifiedByUserId { get; set; }

        public RegPatientEncounter? Encounter { get; set; }

        public MstProcedure? Procedure { get; set; }

        public ICollection<LabSpecimen> Specimens { get; set; } = new List<LabSpecimen>();

        /// <summary>
        /// Pemeriksaan yang dipesan. Sejak <c>LAB-DEC-024</c> inilah satuan yang ditagihkan,
        /// terpisah dari wadah fisik yang menopangnya.
        /// </summary>
        public ICollection<LabExamination> Examinations { get; set; } = new List<LabExamination>();
    }
}
