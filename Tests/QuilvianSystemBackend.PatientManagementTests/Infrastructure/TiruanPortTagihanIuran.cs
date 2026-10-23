using System.Security.Cryptography;
using System.Text;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Services;

namespace QuilvianSystemBackend.Tests.PatientManagement.Infrastructure
{
    /// <summary>
    /// Tiruan port tagihan iuran CRM ke Billing (<see cref="IMembershipFeeChargePort"/>, INT-06) untuk uji
    /// crm-membership. Tagihan hanya hidup di memori: tidak memanggil service Billing, jaringan, maupun
    /// basis data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Yang ditiru adalah janji port, bukan isi Billing. <see cref="EnsureChargeAsync"/> idempotent menurut
    /// <c>MembershipId</c>, dan <c>ChargeId</c>-nya diturunkan dari <c>MembershipId</c> sehingga
    /// deterministik dan berbeda antar-membership. Ini perlu karena
    /// <c>MstPatientMembership.FeeChargeReferenceId</c> berindeks unik: satu ID tetap untuk semua
    /// membership akan menggagalkan pendaftaran kedua pada basis data yang sama.
    /// </para>
    /// <para>
    /// Tagihan tiruan tidak pernah menerima uang. Pembatalan menghasilkan <c>Cancelled</c> bila membership
    /// punya tagihan dari tiruan ini, dan <c>NoCharge</c> bila tidak — sama dengan data sebelum fitur
    /// tagihan. <see cref="RetryUnconfirmedAsync"/> selalu 0. Tagihan tetap tercatat walau transaksi
    /// pemanggil di-rollback; tidak berpengaruh karena setiap pendaftaran memakai <c>MembershipId</c> baru.
    /// </para>
    /// </remarks>
    public sealed class TiruanPortTagihanIuran(Func<DateTimeOffset> sekarang) : IMembershipFeeChargePort
    {
        private readonly Dictionary<Guid, TagihanTiruan> _perMembership = [];
        private readonly List<MembershipFeeChargeRequest> _permintaanTagihan = [];
        private readonly List<Guid> _pembatalanDiminta = [];

        /// <summary>Seluruh permintaan pembuatan tagihan, termasuk panggilan ulang, sesuai urutan.</summary>
        public IReadOnlyList<MembershipFeeChargeRequest> PermintaanTagihan => _permintaanTagihan;

        /// <summary><c>MembershipId</c> yang tagihannya diminta dibatalkan, sesuai urutan.</summary>
        public IReadOnlyList<Guid> PembatalanDiminta => _pembatalanDiminta;

        /// <summary><c>ChargeId</c> tagihan yang dibuat tiruan ini untuk sebuah membership, atau <c>null</c> bila belum ada.</summary>
        public Guid? ChargeIdMilik(Guid membershipId) =>
            _perMembership.TryGetValue(membershipId, out var tagihan) ? tagihan.ChargeId : null;

        public Task<MembershipFeeChargeRef> EnsureChargeAsync(
            MembershipFeeChargeRequest request,
            CancellationToken cancellationToken)
        {
            _permintaanTagihan.Add(request);

            if (_perMembership.TryGetValue(request.MembershipId, out var ada))
            {
                return Task.FromResult(new MembershipFeeChargeRef(ada.ChargeId, ada.ChargeNumber, IsNew: false));
            }

            var tagihan = new TagihanTiruan(
                TurunkanChargeId(request.MembershipId),
                $"IUR-UJI-{_perMembership.Count + 1:D6}",
                request.FeeAmount,
                request.PaymentDueAt);
            _perMembership.Add(request.MembershipId, tagihan);

            return Task.FromResult(new MembershipFeeChargeRef(tagihan.ChargeId, tagihan.ChargeNumber, IsNew: true));
        }

        public Task<MembershipFeeCancelOutcome> CancelChargeAsync(
            Guid membershipId,
            string reason,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            _pembatalanDiminta.Add(membershipId);

            if (!_perMembership.TryGetValue(membershipId, out var tagihan))
            {
                return Task.FromResult(MembershipFeeCancelOutcome.NoCharge);
            }

            tagihan.Dibatalkan = true;
            return Task.FromResult(MembershipFeeCancelOutcome.Cancelled);
        }

        public Task<IReadOnlyDictionary<Guid, MembershipFeeChargeSummary>> GetSummariesAsync(
            IReadOnlyCollection<Guid> chargeIds,
            CancellationToken cancellationToken)
        {
            IReadOnlyDictionary<Guid, MembershipFeeChargeSummary> hasil = _perMembership.Values
                .Where(x => chargeIds.Contains(x.ChargeId))
                .ToDictionary(x => x.ChargeId, Ringkas);

            return Task.FromResult(hasil);
        }

        public Task<int> RetryUnconfirmedAsync(CancellationToken cancellationToken) => Task.FromResult(0);

        // Aturan nominal dan status sama dengan pemetaan BillingMembershipFeeService: tagihan batal tidak
        // punya sisa, tagihan terbuka lewat batas bayar terhitung terlambat.
        private MembershipFeeChargeSummary Ringkas(TagihanTiruan tagihan)
        {
            var status = tagihan.Dibatalkan
                ? BillingMembershipFeeChargeStatuses.Cancelled
                : BillingMembershipFeeChargeStatuses.Unpaid;

            return new MembershipFeeChargeSummary(
                tagihan.ChargeId,
                tagihan.ChargeNumber,
                tagihan.FeeAmount,
                PaidAmount: 0m,
                OutstandingAmount: tagihan.Dibatalkan ? 0m : tagihan.FeeAmount,
                status,
                tagihan.Dibatalkan ? "Dibatalkan" : "Belum Dibayar",
                tagihan.PaymentDueAt,
                SettledAt: null,
                MembershipConfirmedAt: null,
                LastPaymentMethodName: null,
                LastPaymentReference: null,
                LastKwitansiNumber: null,
                IsOverdue: !tagihan.Dibatalkan && sekarang() >= tagihan.PaymentDueAt,
                NeedsSync: false,
                RequiresReview: false,
                ReviewReason: null);
        }

        private static Guid TurunkanChargeId(Guid membershipId)
        {
            var digest = SHA256.HashData(Encoding.UTF8.GetBytes("tagihan-iuran-uji:" + membershipId.ToString("N")));
            return new Guid(digest.AsSpan(0, 16));
        }

        private sealed class TagihanTiruan(Guid chargeId, string chargeNumber, decimal feeAmount, DateTimeOffset paymentDueAt)
        {
            public Guid ChargeId { get; } = chargeId;

            public string ChargeNumber { get; } = chargeNumber;

            public decimal FeeAmount { get; } = feeAmount;

            public DateTimeOffset PaymentDueAt { get; } = paymentDueAt;

            public bool Dibatalkan { get; set; }
        }
    }
}
