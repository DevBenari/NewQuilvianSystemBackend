using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models
{
    /// <summary>
    /// Jenis instrumen yang ditawarkan pada asesmen satu disiplin — <c>data-dictionary.md</c>
    /// bagian 2.20, RHB-DEC-017.
    /// </summary>
    /// <remarks>
    /// <see cref="DisciplineId"/> kosong berarti asesmen medis Sp.KFR. Definisi instrumennya
    /// tetap milik mesin instrumen klinis dan baru berlaku setelah disahkan. Urutan tampil tidak
    /// disimpan (tanpa <c>SortOrder</c>).
    /// </remarks>
    [Table("RhbDisciplineInstrument", Schema = "public")]
    public class RhbDisciplineInstrument : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? DisciplineId { get; set; }

        public RhbDiscipline? Discipline { get; set; }

        public ClinicalInstrumentKind InstrumentKind { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
