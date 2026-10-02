using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums
{
    /// <summary>
    /// Dua keadaan tindakan Bank Darah, sesuai <c>contracts/state-transition-matrix.md</c> bagian 5.
    /// </summary>
    /// <remarks>
    /// <b>Tidak ada status penagihan.</b> Perpindahan ke <see cref="Completed"/> menyerahkan satu
    /// fakta biaya ke Billing (<c>DEC-BD-016</c>, <c>BE-BD-013</c>), tetapi status penyerahannya
    /// tinggal di ledger <c>CliClinicalMilestoneFact</c>, bukan di enum ini. Kegagalan Billing tidak
    /// mengembalikan tindakan ke <see cref="Recorded"/>.
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
