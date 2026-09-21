using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Pemetaan satu jenis pemeriksaan katalog ke satu golongan Patologi Anatomi
    /// (<c>LAB-DEC-087</c>, <c>LAB-DC-048</c>).
    ///
    /// <b>Tabel ini yang membuat halaman hasil Patologi Anatomi berisi atau kosong.</b> Formulir
    /// laporan dibentuk dari parameter yang berlaku bagi golongan pesanan; golongan itu
    /// ditemukan lewat baris di sini. Jenis pemeriksaan yang <b>tidak</b> punya baris di sini
    /// nol menyumbang parameter, dan itu disengaja — <c>INV-39</c> menetapkan sistem nol
    /// menebak golongan sebuah pemeriksaan.
    ///
    /// <b>Konsekuensinya wajib dibaca sebelum rilis pertama.</b> Sebelum tabel ini terisi, nol
    /// pemeriksaan Patologi Anatomi punya golongan, sehingga formulirnya kosong sama sekali.
    /// <c>VAL-100</c> karena itu mewajibkan penolakannya menyebut <b>apa</b> yang belum diatur
    /// dan <b>siapa</b> yang mengaturnya — menolak dengan daftar kosong tanpa sebab akan membuat
    /// patolog pertama yang membuka layar mengira sistemnya rusak.
    ///
    /// <b>Isinya bukan pekerjaan programmer.</b> Ketiga data induk lain bersifat tetap dan
    /// diseed; yang ini bergantung pada jenis pemeriksaan Patologi Anatomi yang benar-benar ada
    /// di katalog rumah sakit, sehingga pengisiannya milik kepala instalasi bersama
    /// <c>DR-LAB-003</c>. Jalur usulan otomatis hanya <b>mengusulkan</b>; manusia yang
    /// menyimpannya.
    ///
    /// <b><see cref="MstProcedure"/> nol disentuh.</b> Tabel ini milik Laboratorium dan hanya
    /// <i>menunjuk</i> katalog, sehingga penggolongan Patologi Anatomi nol menambah kolom pada
    /// data induk milik modul lain.
    /// </summary>
    public class LabProcedurePathologyCategory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Jenis pemeriksaan katalog yang digolongkan. Unik di antara baris yang belum ditandai
        /// terhapus — satu jenis pemeriksaan <b>tepat satu</b> golongan.
        /// </summary>
        [Required]
        public Guid ProcedureId { get; set; }

        /// <summary>Golongan Patologi Anatomi bagi jenis pemeriksaan itu.</summary>
        [Required]
        public Guid LabPathologyCategoryId { get; set; }

        /// <summary>Jenis pemeriksaan katalog yang digolongkan.</summary>
        public MstProcedure? Procedure { get; set; }

        /// <summary>Golongan Patologi Anatomi bagi jenis pemeriksaan itu.</summary>
        public LabPathologyCategory? LabPathologyCategory { get; set; }
    }
}
