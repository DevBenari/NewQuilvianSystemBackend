using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    /// <summary>
    /// Status siklus hidup pesan outbox integrasi Rawat Inap (Transactional Outbox Pattern).
    /// </summary>
    public enum OutboxStatus
    {
        [Display(Name = "Pending")]
        Pending = 0,

        [Display(Name = "Processing")]
        Processing = 1,

        [Display(Name = "Published")]
        Published = 2,

        [Display(Name = "Failed")]
        Failed = 3,

        [Display(Name = "Dead Letter")]
        DeadLetter = 4
    }
}
