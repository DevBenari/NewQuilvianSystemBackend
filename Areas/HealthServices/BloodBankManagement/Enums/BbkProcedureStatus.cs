using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums
{
    /// <summary>
    /// Dua keadaan tindakan Bank Darah, sesuai <c>contracts/state-transition-matrix.md</c> bagian 5.
    /// </summary>
    /// <remarks>
    /// <b>Tidak ada status penagihan.</b> Penyaluran fakta biaya ke Billing tertahan
    /// <c>DEC-BD-016</c> dan milik <c>BE-BD-013</c>; tindakan yang <see cref="Completed"/> tidak
    /// memicu apa pun di luar Bank Darah.
    /// </remarks>
    public enum BbkProcedureStatus
    {
        /// <summary>Tindakan dicatat beserta salinan tarifnya.</summary>
        [Display(Name = "Dicatat")]
        Recorded = 0,

        /// <summary>Tindakan dinyatakan selesai. Terminal.</summary>
        [Display(Name = "Selesai")]
        Completed = 1
    }
}
