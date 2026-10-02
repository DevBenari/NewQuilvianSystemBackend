using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums
{
    public enum PrescriptionFulfillmentStatus
    {
        [Display(Name = "Menunggu Finalisasi Klinis")]
        WaitingForClinicalFinalization = 1,

        [Display(Name = "Menunggu Pembayaran")]
        WaitingForPayment = 2,

        [Display(Name = "Siap Masuk Farmasi")]
        ReadyForPharmacy = 3,

        [Display(Name = "Dalam Antrean Farmasi")]
        QueuedAtPharmacy = 4,

        [Display(Name = "Diverifikasi Farmasi")]
        VerifiedByPharmacy = 5,

        [Display(Name = "Sedang Disiapkan")]
        InPreparation = 6,

        [Display(Name = "Siap Diserahkan")]
        ReadyToDispense = 7,

        [Display(Name = "Diserahkan Sebagian")]
        PartiallyDispensed = 8,

        [Display(Name = "Sudah Diserahkan")]
        Dispensed = 9,

        [Display(Name = "Ditolak Farmasi")]
        Rejected = 10,

        [Display(Name = "Dibatalkan")]
        Cancelled = 11,

        /// <summary>Penyiapan selesai, menunggu telaah obat akhir oleh apoteker.</summary>
        /// <remarks>
        /// Telaah obat akhir wajib bagi seluruh resep sebelum obat boleh diserahkan.
        /// Nilainya ditambahkan di urutan terakhir supaya angka status yang sudah
        /// tersimpan di basis data tidak bergeser.
        /// </remarks>
        [Display(Name = "Menunggu Telaah Obat Akhir")]
        AwaitingFinalCheck = 12
    }
}
