using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums
{
    public enum PrescriptionStatus
    {
        [Display(Name = "Draft")]
        Draft = 1,

        [Display(Name = "Diajukan")]
        Submitted = 2,

        [Display(Name = "Dibatalkan")]
        Cancelled = 3
    }
}
