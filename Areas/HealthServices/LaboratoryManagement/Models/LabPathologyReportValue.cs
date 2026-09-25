using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Nilai satu ruas isian pada satu laporan Patologi Anatomi (<c>LAB-DEC-086</c>,
    /// <c>LAB-DC-047</c>).
    ///
    /// <b>Baris, bukan kolom — dan itulah seluruh gunanya.</b> Bentuk laporan berbeda per
    /// golongan: Histologi memakai tiga ruas, Imunohistokimia sepuluh. Sebagai kolom, golongan
    /// berikutnya menuntut migration; sebagai baris, ia menuntut satu baris data induk.
    ///
    /// <b><see cref="Value"/> bertipe <c>text</c>, bukan <c>varchar(n)</c>.</b> <c>RULE-011</c>
    /// menyatakan nol batas panjang, dan itu bukan kelonggaran yang terlewat: uraian makroskopik
    /// sebuah reseksi memang dapat panjang, dan batas yang dipilih sembarangan akan memotong
    /// diagnosis pasien di tengah kalimat.
    ///
    /// <b><see cref="ParameterNameSnapshot"/> disalin, bukan dibaca ulang.</b> Nama data induk
    /// yang diperbarui <b>nol berlaku surut</b> pada laporan yang sudah terbit — pola yang sama
    /// dengan <c>ProcedureNameSnapshot</c> dan <c>ResultUnitSnapshot</c>. Tanpa salinan ini,
    /// merapikan satu label data induk akan mengubah bunyi ratusan laporan lama tanpa satu pun
    /// baris disunting.
    ///
    /// <b>Penunjuk parameternya TETAP disimpan di samping salinannya</b>, sebab keduanya
    /// menjawab hal berbeda: salinan menjawab <i>apa yang tertulis saat itu</i>, penunjuk
    /// menjawab <i>ruas mana ini sebenarnya</i> — dan hanya penunjuk yang dapat dipakai
    /// mengelompokkan lintas laporan.
    ///
    /// <b><see cref="Value"/> adalah isi diagnosis pasien.</b> Dilarang masuk logger, dilarang
    /// dipakai sebagai contoh berisi data asli, dan dilarang muncul pada DTO yang dipakai layar
    /// non-klinis (<c>LAB-PERM-v1</c> rev 7 bagian 9.5).
    /// </summary>
    public class LabPathologyReportValue : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Laporan yang memuat nilai ini.</summary>
        [Required]
        public Guid LabPathologyReportId { get; set; }

        /// <summary>
        /// Ruas isian dari data induk. Satu parameter muncul <b>sekali</b> per laporan — unik
        /// bersama <see cref="LabPathologyReportId"/> di antara baris yang belum ditandai
        /// terhapus.
        /// </summary>
        [Required]
        public Guid LabPathologyParameterId { get; set; }

        /// <summary>
        /// Salinan label parameter <b>saat nilai ini diisi</b>. Perubahan nama pada data induk
        /// nol berlaku surut ke sini.
        /// </summary>
        [Required]
        public string ParameterNameSnapshot { get; set; } = string.Empty;

        /// <summary>
        /// Isi yang ditulis patolog. <b>Diagnosis pasien</b>, dan tanpa batas panjang
        /// (<c>RULE-011</c>).
        /// </summary>
        [Required]
        public string Value { get; set; } = string.Empty;

        /// <summary>Laporan yang memuat nilai ini.</summary>
        public LabPathologyReport? LabPathologyReport { get; set; }

        /// <summary>Ruas isian dari data induk.</summary>
        public LabPathologyParameter? LabPathologyParameter { get; set; }
    }
}
