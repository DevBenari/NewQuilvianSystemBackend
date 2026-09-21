using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    /// <summary>
    /// Status kelayakan kasir dari perspektif gerbang pemulangan rawat inap.
    /// </summary>
    public enum BillingClearanceStatus
    {
        [Display(Name = "None")]
        None = 0,

        [Display(Name = "Pending")]
        Pending = 1,

        [Display(Name = "Cleared")]
        Cleared = 2,

        [Display(Name = "Revoked")]
        Revoked = 3,

        [Display(Name = "Overridden")]
        Overridden = 4
    }
}
