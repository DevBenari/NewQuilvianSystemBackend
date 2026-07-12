using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    public enum ConsultationValidationSeverity
    {
        [Display(Name = "Informasi")]
        Information = 1,

        [Display(Name = "Peringatan")]
        Warning = 2,

        [Display(Name = "Error")]
        Error = 3
    }
}
