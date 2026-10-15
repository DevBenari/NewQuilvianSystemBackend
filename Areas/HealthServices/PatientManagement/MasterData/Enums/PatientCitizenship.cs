using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Enums
{
    /// <summary>
    /// Kewarganegaraan pasien (RJ-REG-DEC-006). WNI tidak menyimpan negara asal; WNA wajib
    /// menunjuk negara pada data induk <c>MstCountry</c>.
    /// </summary>
    public enum PatientCitizenship
    {
        [Display(Name = "WNI")]
        Wni = 1,

        [Display(Name = "WNA")]
        Wna = 2
    }
}
