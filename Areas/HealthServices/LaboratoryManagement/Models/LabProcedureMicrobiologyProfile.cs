using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Menyatakan apakah sebuah pemeriksaan katalog memakai <b>set bakteri</b> — isolat beserta
    /// antibiogramnya (<c>LAB-DEC-125</c>).
    ///
    /// <b>Tidak semua pemeriksaan Mikrobiologi memakainya.</b> Pemilik modul menyatakannya
    /// langsung pada 2026-09-21: yang berawalan <c>MO KUL</c> memakai set bakteri, sebagian
    /// lain tidak.
    ///
    /// <b>Kenapa BUKAN diturunkan dari nama pemeriksaan.</b> Awalan <c>MO KUL</c> dan
    /// <c>JAM KUL</c> memang menandainya, tetapi <c>LAB-DEC-087</c> <b>sudah menolak</b>
    /// menurunkan kategori dari kata kunci nama — dan menurunkannya di sini mengulang persis
    /// hal yang ditolak itu. Kata kunci pada nama pernah menggolongkan
    /// <i>Imuno<b>histo</b>kimia</i> ke Histologi hanya karena mengandung <c>HISTO</c>.
    ///
    /// <b>Kenapa BUKAN kolom pada <c>MstProcedure</c>.</b> Tabel itu milik <c>master-data</c>.
    /// Menambah kolom di sana menuntut koordinasi lintas modul, dan modul ini sudah dua kali
    /// tertahan pola yang sama lewat <c>LAB-COORD-006</c> dan <c>MST-POS-WRITE</c>. Pemetaan
    /// ini menunjuk dari sisi Laboratorium, persis seperti
    /// <see cref="LabProcedurePathologyCategory"/>.
    /// </summary>
    public class LabProcedureMicrobiologyProfile : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Pemeriksaan katalog yang diprofilkan. Unik di antara baris yang belum terhapus.</summary>
        [Required]
        public Guid ProcedureId { get; set; }

        /// <summary>
        /// Apakah pemeriksaan ini memakai set bakteri.
        ///
        /// Bernilai salah membuat bagian isolat dan antibiogram <b>tidak tampil sama sekali</b>
        /// pada layar, dan pengirimannya ditolak (<c>VAL-118</c>).
        /// </summary>
        public bool UsesSusceptibilitySet { get; set; } = true;

        /// <summary>
        /// Jenis biakan yang lazim bagi pemeriksaan ini — mengisi awal layar, bukan mengunci.
        /// Analis tetap dapat mengubahnya.
        /// </summary>
        public LabCultureType? DefaultCultureType { get; set; }

        /// <summary>
        /// Metode uji yang lazim bagi pemeriksaan ini — mengisi awal layar, bukan mengunci.
        /// </summary>
        public LabSusceptibilityMethod? DefaultSusceptibilityMethod { get; set; }

        /// <summary>Pemetaan yang tidak lagi dipakai dinonaktifkan, bukan dihapus.</summary>
        public bool IsActive { get; set; } = true;

        public MstProcedure? Procedure { get; set; }
    }
}
