namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Enums
{
    public enum MembershipStatus
    {
        Active = 1,
        Expired = 2,
        Suspended = 3,
        Cancelled = 4,
        Upgraded = 5,
        Downgraded = 6,

        /// <summary>Tersimpan, iuran belum lunas; pasien masih terbaca Reguler (CRM-DEC-004).</summary>
        PendingPayment = 7,

        /// <summary>Perpanjangan lunas yang menunggu periode lama berakhir (CRM-DEC-024).</summary>
        Scheduled = 8,

        /// <summary>Data lama tanpa bukti bayar, menunggu diakui atau ditolak CRM (CRM-DEC-021).</summary>
        NeedsVerification = 9
    }
}
