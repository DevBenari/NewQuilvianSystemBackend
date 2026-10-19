namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Services
{
    /// <summary>Permintaan membuat tagihan iuran untuk satu pendaftaran membership (INT-06).</summary>
    public sealed record MembershipFeeChargeRequest(
        Guid MembershipId,
        Guid PatientId,
        string MemberNumber,
        decimal FeeAmount,
        DateTimeOffset PaymentDueAt,
        Guid ActorUserId);

    public sealed record MembershipFeeChargeRef(Guid ChargeId, string ChargeNumber, bool IsNew);

    /// <summary>Hasil usaha membatalkan tagihan iuran saat membership dibatalkan (CRM-DEC-014, CRM-DEC-029).</summary>
    public enum MembershipFeeCancelOutcome
    {
        /// <summary>Tagihan dibatalkan (atau sudah dibatalkan sebelumnya).</summary>
        Cancelled = 1,

        /// <summary>Membership belum punya tagihan (data sebelum fitur ini); tidak ada yang dibatalkan.</summary>
        NoCharge = 2,

        /// <summary>Uang sudah diterima untuk tagihan ini; pembatalan ditolak dan perlu ditinjau (refund bukan otomatis).</summary>
        PaymentReceived = 3,

        /// <summary>Ada pembayaran yang sedang berjalan; coba lagi pada putaran berikutnya.</summary>
        PaymentInProgress = 4
    }

    /// <summary>Ringkasan tagihan untuk layar CRM; tidak memuat data sensitif penyedia pembayaran.</summary>
    public sealed record MembershipFeeChargeSummary(
        Guid ChargeId,
        string ChargeNumber,
        decimal FeeAmount,
        decimal PaidAmount,
        decimal OutstandingAmount,
        string Status,
        string StatusName,
        DateTimeOffset PaymentDueAt,
        DateTimeOffset? SettledAt,
        DateTimeOffset? MembershipConfirmedAt,
        string? LastPaymentMethodName,
        string? LastPaymentReference,
        string? LastKwitansiNumber,
        bool IsOverdue,
        bool NeedsSync,
        bool RequiresReview,
        string? ReviewReason);

    /// <summary>
    /// Pintu CRM ke Billing untuk tagihan iuran (`integration-contract.md` INT-06). Diimplementasikan oleh
    /// Billing; didefinisikan di sisi CRM supaya CRM tidak bergantung pada tipe internal Billing dan
    /// supaya tidak ada siklus dependensi (Billing memanggil balik CRM lewat
    /// <see cref="PatientMembershipFeeLinkService"/>, INT-05).
    /// </summary>
    public interface IMembershipFeeChargePort
    {
        /// <summary>
        /// Membuat tagihan iuran untuk membership, idempotent menurut <c>MembershipId</c>. Berjalan di
        /// transaksi pemanggil bila ada, sehingga pendaftaran dan tagihannya berhasil atau gagal bersama.
        /// </summary>
        Task<MembershipFeeChargeRef> EnsureChargeAsync(MembershipFeeChargeRequest request, CancellationToken cancellationToken);

        /// <summary>Membatalkan tagihan milik membership yang dibatalkan; menolak bila uang sudah masuk atau sedang berjalan.</summary>
        Task<MembershipFeeCancelOutcome> CancelChargeAsync(
            Guid membershipId, string reason, Guid actorUserId, CancellationToken cancellationToken);

        Task<IReadOnlyDictionary<Guid, MembershipFeeChargeSummary>> GetSummariesAsync(
            IReadOnlyCollection<Guid> chargeIds, CancellationToken cancellationToken);

        /// <summary>
        /// Mengulang konfirmasi pelunasan yang belum sampai ke membership (tagihan PAID, konfirmasi kosong).
        /// Aman diulang: konfirmasi bersifat idempotent. Mengembalikan jumlah tagihan yang berhasil disinkronkan.
        /// </summary>
        Task<int> RetryUnconfirmedAsync(CancellationToken cancellationToken);
    }
}
