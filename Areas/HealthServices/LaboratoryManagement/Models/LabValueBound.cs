using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Batas nilai rujukan sebuah jenis pemeriksaan laboratorium (<c>LAB-DEC-006</c>,
    /// <c>LAB-DEC-018</c>, <c>LAB-DEC-021</c>).
    ///
    /// Tabel ini sengaja berdiri sendiri dan bukan kolom tambahan pada <c>MstProcedure</c>.
    /// Alasannya bukan kerapian: satu jenis pemeriksaan dapat memiliki <b>beberapa</b> baris
    /// batas yang dibedakan menurut jenis kelamin dan kelompok umur, sehingga bentuk kolom
    /// tidak mungkin menampungnya. Hemoglobin, misalnya, punya tiga baris — pria dewasa,
    /// wanita dewasa, dan anak (BR-14, <c>AC-24</c>).
    ///
    /// <c>MstProcedure</c> tetap milik <c>master-data</c> dan tidak bertambah satu pun kolom
    /// operasional laboratorium karena tabel ini ada (<c>FR-03.6</c>, <c>AC-25</c>);
    /// Laboratorium hanya menunjuk ke sana.
    ///
    /// Bentuk hasilnya menentukan kolom mana yang bermakna: <see cref="LabResultForm.Numeric"/>
    /// memakai <see cref="Unit"/> beserta keempat batas angka, sedangkan
    /// <see cref="LabResultForm.Choice"/> memakai <see cref="Options"/>. Penegakan aturan itu
    /// — <c>VAL-22</c> sampai <c>VAL-24</c> — adalah pekerjaan endpoint pengelolaan pada
    /// <c>BE-LAB-04</c>, bukan pekerjaan lapisan penyimpanan ini.
    /// </summary>
    public class LabValueBound : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Jenis pemeriksaan yang dibatasi. Menunjuk ke <c>MstProcedure</c> milik <c>master-data</c>.</summary>
        [Required]
        public Guid ProcedureId { get; set; }

        /// <summary>
        /// Bentuk hasil pemeriksaan ini — angka atau pilihan terbatas (<c>LAB-DEC-021</c>).
        /// </summary>
        public LabResultForm ResultForm { get; set; } = LabResultForm.Numeric;

        /// <summary>
        /// Satuan hasil, misalnya <c>g/dL</c>. Wajib bila <see cref="ResultForm"/> berupa
        /// angka (<c>VAL-22</c>) dan tidak bermakna bila berupa pilihan.
        /// </summary>
        [MaxLength(20)]
        public string? Unit { get; set; }

        /// <summary>Batas normal bawah.</summary>
        public decimal? NormalLow { get; set; }

        /// <summary>Batas normal atas.</summary>
        public decimal? NormalHigh { get; set; }

        /// <summary>
        /// Batas kritis bawah. Perubahannya memerlukan persetujuan klinis lewat pengajuan
        /// tersendiri (<c>LAB-DEC-023</c>), yang dibangun pada <c>BE-LAB-03</c> dan
        /// <c>BE-LAB-05</c>.
        /// </summary>
        public decimal? CriticalLow { get; set; }

        /// <summary>Batas kritis atas. Perlindungannya sama dengan <see cref="CriticalLow"/>.</summary>
        public decimal? CriticalHigh { get; set; }

        /// <summary>Pembatas jenis kelamin baris batas ini (BR-14).</summary>
        public LabGenderScope GenderScope { get; set; } = LabGenderScope.All;

        /// <summary>
        /// Pembatas kelompok umur. Kosong berarti baris ini berlaku untuk semua umur.
        /// Menunjuk ke <c>MstAgeCategory</c> milik <c>master-data</c>; Laboratorium tidak
        /// menyalin data induk global ke dalam modulnya (<c>AC-49</c>).
        /// </summary>
        public Guid? AgeCategoryId { get; set; }

        /// <summary>
        /// Batas waktu penyelesaian pemeriksaan cito, dihitung sejak wadah dinyatakan layak.
        /// </summary>
        public int? CitoTurnaroundMinutes { get; set; }

        public bool IsActive { get; set; } = true;

        // Tidak ada SortOrder di sini. Kamus data sempat menyebutnya, tetapi urutan tampil baris
        // batas pada layar pengelolaan adalah kebutuhan presentasi murni, dan
        // BACKEND_ENGINEERING_CONTRACT melarang SortOrder presentasi yang dipersistensi untuk
        // kode baru (QBE-ENT-003). Urutan yang bermakna bagi baris batas sudah tersedia dari
        // datanya sendiri — jenis kelamin dan kelompok umur — sehingga layar dapat mengurutkannya
        // tanpa kolom tambahan. Bandingkan dengan LabValueOption.SortOrder, yang tetap ada karena
        // di sana urutan menyatakan tingkatan skala ordinal hasil dan itu isi bisnis.

        public MstProcedure? Procedure { get; set; }

        public MstAgeCategory? AgeCategory { get; set; }

        /// <summary>
        /// Daftar pilihan yang sah bila bentuk hasilnya <see cref="LabResultForm.Choice"/>.
        /// Kosong untuk bentuk angka.
        /// </summary>
        public ICollection<LabValueOption> Options { get; set; } = new List<LabValueOption>();
    }
}
