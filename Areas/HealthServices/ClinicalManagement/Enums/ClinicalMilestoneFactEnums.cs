using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Alasan klinis yang menyebabkan satu revisi fact diterbitkan.
    ///
    /// Nilai ini adalah catatan klinis, bukan status finansial. Billing tetap yang menentukan
    /// akibat finansialnya.
    /// </summary>
    public enum ClinicalMilestoneKind
    {
        [Display(Name = "Charge Eligibility")]
        ChargeEligibility = 1,

        [Display(Name = "Clinical Cancellation")]
        ClinicalCancellation = 2
    }

    /// <summary>
    /// Keadaan pengiriman satu revisi fact ke Billing.
    /// </summary>
    public enum ClinicalFactDispatchStatus
    {
        /// <summary>Fact sudah tercatat tetapi belum dikonfirmasi diterima Billing.</summary>
        [Display(Name = "Pending")]
        Pending = 1,

        /// <summary>Billing sudah memproses fact dan mengembalikan hasil canonical.</summary>
        [Display(Name = "Dispatched")]
        Dispatched = 2,

        /// <summary>Billing menolak fact karena input tidak memenuhi kontrak.</summary>
        [Display(Name = "Rejected")]
        Rejected = 3,

        /// <summary>
        /// Pengiriman dilakukan tetapi hasilnya tidak diketahui. Tidak boleh di-retry buta dan
        /// tidak boleh dianggap gagal; wajib melalui rekonsiliasi.
        /// </summary>
        [Display(Name = "Outcome Unknown")]
        OutcomeUnknown = 4,

        /// <summary>
        /// Pembatalan klinis terjadi sebelum charge terbentuk, sehingga tidak ada apa pun yang
        /// perlu dikoreksi di Billing. Baris tetap disimpan sebagai histori klinis.
        /// </summary>
        [Display(Name = "Suppressed No Prior Charge")]
        SuppressedNoPriorCharge = 5
    }
}
