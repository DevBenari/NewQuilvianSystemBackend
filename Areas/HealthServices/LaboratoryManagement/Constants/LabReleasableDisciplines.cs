using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Constants
{
    /// <summary>
    /// Disiplin yang hasilnya <b>dapat divalidasi dan dirilis</b> — satu jawaban bagi penjaga
    /// <c>VAL-126</c> dan laporan operasional (<c>ARCH-GAP-LAB-11</c>,
    /// <c>02-backend-architecture.md</c> 23.4). Penjaga dan laporan yang berbeda pendapat akan
    /// menampilkan angka 0 bagi disiplin yang sebenarnya belum dapat dirilis.
    ///
    /// <para>
    /// Isinya mengikuti jalur yang sudah dibangun: <b>Patologi Klinik</b> (<c>S4</c>,
    /// <c>BE-LAB-73</c>). Mikrobiologi ditambahkan <b>di sini</b> oleh <c>BE-LAB-78</c>
    /// (<c>S4d-1</c>), bukan pada penjaga di service; Patologi Anatomi bersama <c>S4e</c>.
    /// </para>
    ///
    /// <para>
    /// <b>Konstanta, bukan data</b> — sama dengan <see cref="LabClinicalPrivilegeCodes"/>: himpunan
    /// ini menentukan tindakan klinis mana yang terbuka, sehingga mengubahnya lewat tinjauan kode.
    /// </para>
    /// </summary>
    public static class LabReleasableDisciplines
    {
        private static readonly LabDiscipline[] Disciplines =
        {
            LabDiscipline.ClinicalPathology
        };

        /// <summary>Benar bila hasil disiplin ini dapat divalidasi dan dirilis.</summary>
        public static bool Contains(LabDiscipline discipline) =>
            Array.IndexOf(Disciplines, discipline) >= 0;
    }
}
