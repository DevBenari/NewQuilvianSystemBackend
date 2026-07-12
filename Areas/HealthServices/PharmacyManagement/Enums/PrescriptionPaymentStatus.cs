using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums
{
    public enum PrescriptionPaymentStatus
    {
        [Display(Name = "Belum Ditagihkan")]
        NotBilled = 1,

        [Display(Name = "Tagihan Dibuat")]
        BillingGenerated = 2,

        [Display(Name = "Menunggu Pembayaran")]
        WaitingForPayment = 3,

        [Display(Name = "Dibayar Sebagian")]
        PartiallyPaid = 4,

        [Display(Name = "Lunas")]
        Paid = 5,

        [Display(Name = "Disetujui Asuransi")]
        InsuranceApproved = 6,

        [Display(Name = "Pembayaran Ditiadakan")]
        PaymentWaived = 7,

        [Display(Name = "Dibatalkan")]
        Cancelled = 8
    }
}
