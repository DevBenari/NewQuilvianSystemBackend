using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    public enum EncounterPaymentType
    {
        [Display(Name = "Tunai")]
        Cash = 1,

        [Display(Name = "Asuransi")]
        Insurance = 2,

        /// <summary>
        /// Encounter dijamin oleh hubungan pasien dengan perusahaan.
        /// Ditambahkan RWI-ENC-PAYER-001 1.0.0 lewat BE-RWI-035 secara aditif;
        /// nilai Cash dan Insurance tidak boleh bergeser.
        /// </summary>
        [Display(Name = "Penjamin Perusahaan")]
        CompanyGuarantor = 3
    }
}
