using System.Linq.Expressions;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// <b>Satu rumus</b> kelayakan penjadwalan ulang pesanan laboratorium (<c>LAB-DC-068</c>,
    /// <c>INV-67</c>, <c>02-backend-architecture.md</c> 33.4): dipakai penjaga
    /// <c>PUT /lab-orders/{id}/reschedule</c> dan — mulai <c>BE-LAB-103</c> — penanda
    /// <c>canReschedule</c> pada detail dan daftar, sehingga tombol dan penolakan tidak pernah
    /// berbeda pendapat tentang pesanan yang sama.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Murni dan tanpa akses basis data: pemanggil menyusun <see cref="LabRescheduleSnapshot"/>
    /// (satu pesanan lewat kueri, satu halaman daftar lewat proyeksi) lalu menyerahkan
    /// <i>hari ini</i> menurut tanggal operasional WIB.
    /// </para>
    /// <para>
    /// <b>Bukan pemeriksaan izin.</b> Pesanan yang layak tetap ditolak <c>403</c> bagi pengguna
    /// tanpa <c>LabOrder : Reschedule</c>; penanda ini tidak pernah menggantikan izin.
    /// </para>
    /// </remarks>
    public static class LabRescheduleEligibility
    {
        public const string McuLinkedCode = "McuLinked";
        public const string StatusNotEligibleCode = "StatusNotEligible";
        public const string SampleTakenCode = "SampleTaken";
        public const string EncounterClosedCode = "EncounterClosed";
        public const string ActiveCitoCode = "ActiveCito";

        public const string McuLinkedMessage =
            "Pesanan ini bagian MCU; jadwal ulang dilakukan petugas MCU.";
        public const string SampleTakenMessage =
            "Sampel sudah diambil; batalkan wadah atau ambil ulang lebih dulu.";
        public const string SampleRejectedMessage =
            "Sampel ditolak; minta ambil ulang lebih dulu.";
        public const string EncounterClosedMessage =
            "Kunjungan pesanan ini sudah selesai; pesan ulang pada kunjungan baru.";
        public const string ActiveCitoMessage =
            "Pesanan memuat pemeriksaan CITO; minta dokter mencabut tanda CITO lebih dulu, atau batalkan lalu pesan ulang.";

        /// <summary>
        /// Pesan <c>VAL-160</c>. <c>{status}</c> diisi label status berbahasa Indonesia yang sama dengan
        /// layar, penyaring, dan berkas Lab (<see cref="LabFilterMetadataFactory.LabelOrderStatus"/>,
        /// <c>LAB-DEC-250</c>) — bukan nama enum, karena pesan ini dibaca petugas di tombol nonaktif dan
        /// respons <c>409</c> (keputusan pemilik modul 2026-11-27). <i>Contoh:</i> <c>InProcess</c> →
        /// "Pesanan berstatus Sedang Dikerjakan tidak dapat dijadwalkan ulang."
        /// </summary>
        public static string StatusNotEligibleMessage(LabOrderStatus status) =>
            $"Pesanan berstatus {LabFilterMetadataFactory.LabelOrderStatus(status)} tidak dapat dijadwalkan ulang.";

        private static readonly LabRescheduleVerdict Eligible = new(true, null, null);

        /// <summary>
        /// Menilai kelayakan dengan urutan tetap — MCU → status → wadah efektif → kunjungan → CITO
        /// (<c>VAL-164</c>, <c>VAL-160</c>, <c>VAL-161</c>, <c>VAL-162</c>, <c>VAL-163</c>). Alasan
        /// pertama yang terpenuhi yang dikembalikan.
        /// </summary>
        /// <param name="snapshot">Potret pesanan.</param>
        /// <param name="today">Hari ini WIB. Tidak memengaruhi kelayakan — dipertahankan pada
        /// tanda tangan supaya <see cref="Evaluate"/> dan <see cref="IsOverdue"/> menerima masukan
        /// yang sama (33.3 diagram 2).</param>
        public static LabRescheduleVerdict Evaluate(LabRescheduleSnapshot snapshot, DateOnly today)
        {
            ArgumentNullException.ThrowIfNull(snapshot);

            // 1 — VAL-164, LAB-DEC-272. Tanda paket MCU dan komponen MCU di luar paket sama-sama
            // milik petugas MCU; tidak seorang pun dari Lab menjadwal ulang pesanan ini.
            if (snapshot.IsMcuLinked)
                return new LabRescheduleVerdict(false, McuLinkedCode, McuLinkedMessage);

            // 2 — VAL-160, LAB-DEC-270.
            if (!IsStatusEligible(snapshot))
            {
                return new LabRescheduleVerdict(
                    false, StatusNotEligibleCode, StatusNotEligibleMessage(snapshot.OrderStatus));
            }

            // 3 — VAL-161, LAB-DEC-275, LAB-DEC-276.
            var specimenBlock = SpecimenBlockMessage(snapshot);
            if (specimenBlock is not null)
                return new LabRescheduleVerdict(false, SampleTakenCode, specimenBlock);

            // 4 — VAL-162, LAB-DEC-261 (penjaga yang sama dengan VAL-67 saat memesan).
            if (snapshot.EncounterStatus is EncounterStatus.Completed
                or EncounterStatus.Cancelled
                or EncounterStatus.NoShow)
            {
                return new LabRescheduleVerdict(false, EncounterClosedCode, EncounterClosedMessage);
            }

            // 5 — VAL-163, LAB-DEC-262, LAB-DEC-271.
            if (snapshot.HasActiveCito)
                return new LabRescheduleVerdict(false, ActiveCitoCode, ActiveCitoMessage);

            return Eligible;
        }

        /// <summary>
        /// Penanda <i>Terlewat</i> (<c>LAB-DC-069</c>, <c>LAB-DEC-260</c>): tanggal rencana sudah
        /// lewat <b>dan</b> pesanan masih sebelum sampel — hanya syarat 2 (status) dan 3 (wadah).
        /// MCU, kunjungan, dan CITO sengaja tidak ikut: pesanan MCU atau CITO yang tidak datang
        /// tetap perlu terlihat.
        /// </summary>
        public static bool IsOverdue(LabRescheduleSnapshot snapshot, DateOnly today)
        {
            ArgumentNullException.ThrowIfNull(snapshot);

            return snapshot.PlannedDate < today &&
                   IsStatusEligible(snapshot) &&
                   SpecimenBlockMessage(snapshot) is null;
        }

        /// <summary>
        /// Jumlah hari sejak tanggal rencana bila <see cref="IsOverdue"/>, selain itu <c>0</c>
        /// (<c>LAB-API-v1</c> <c>r50</c> 45.3). <i>Contoh:</i> rencana Senin, hari ini Rabu → <c>2</c>.
        /// </summary>
        public static int OverdueDays(LabRescheduleSnapshot snapshot, DateOnly today) =>
            IsOverdue(snapshot, today) ? today.DayNumber - snapshot.PlannedDate.DayNumber : 0;

        /// <summary>
        /// Bentuk kueri dari <see cref="IsOverdue"/> untuk penyaring <c>includeOverdue</c> daftar pantau
        /// (<c>r50</c> 45.5) — supaya basis data yang menyaring, bukan memori.
        /// </summary>
        /// <remarks>
        /// <b>Wajib setara dengan <see cref="IsOverdue"/></b> (syarat 2 status dan syarat 3 wadah
        /// efektif, <see cref="IsStatusEligible"/> dan <see cref="SpecimenBlockMessage"/>). Ditulis di
        /// berkas yang sama supaya perubahan satu sisi terlihat di sisi lain; kesetaraannya dibuktikan
        /// uji yang membandingkan keduanya atas setiap kasus status dan wadah (<c>BE-LAB-103</c>).
        /// </remarks>
        internal static Expression<Func<LabOrder, bool>> OverdueExpression(DateOnly today) =>
            x => x.PlannedDate < today &&
                 (x.OrderStatus == LabOrderStatus.Requested ||
                  x.OrderStatus == LabOrderStatus.Confirmed ||
                  (x.OrderStatus == LabOrderStatus.OnHold &&
                   (x.StatusBeforeHold == LabOrderStatus.Requested ||
                    x.StatusBeforeHold == LabOrderStatus.Confirmed))) &&
                 !x.Specimens.Any(s =>
                     !s.IsDelete &&
                     !(s.SpecimenStatus == LabSpecimenStatus.Planned ||
                       s.SpecimenStatus == LabSpecimenStatus.Cancelled ||
                       s.SpecimenStatus == LabSpecimenStatus.RecollectionRequired ||
                       (s.SpecimenStatus == LabSpecimenStatus.OnHold &&
                        (s.StatusBeforeHold == LabSpecimenStatus.Planned ||
                         s.StatusBeforeHold == LabSpecimenStatus.Cancelled ||
                         s.StatusBeforeHold == LabSpecimenStatus.RecollectionRequired))));

        private static bool IsStatusEligible(LabRescheduleSnapshot snapshot)
        {
            var effective = snapshot.OrderStatus == LabOrderStatus.OnHold
                ? snapshot.StatusBeforeHold
                : snapshot.OrderStatus;

            return effective is LabOrderStatus.Requested or LabOrderStatus.Confirmed;
        }

        /// <summary>
        /// <c>null</c> bila setiap wadah yang dinilai masih <c>Planned</c>. Wadah <c>Cancelled</c> dan
        /// <c>RecollectionRequired</c> tidak dinilai (penggantinya yang dinilai); wadah <c>OnHold</c>
        /// dinilai menurut status sebelum ditahan. Bila setiap wadah penghalang berstatus
        /// <c>Rejected</c>, pesannya pesan ambil ulang; selain itu pesan sampel sudah diambil.
        /// </summary>
        private static string? SpecimenBlockMessage(LabRescheduleSnapshot snapshot)
        {
            var adaPenghalang = false;
            var semuaDitolak = true;

            foreach (var specimen in snapshot.Specimens)
            {
                var effective = specimen.SpecimenStatus == LabSpecimenStatus.OnHold
                    ? specimen.StatusBeforeHold
                    : specimen.SpecimenStatus;

                if (effective is LabSpecimenStatus.Cancelled or LabSpecimenStatus.RecollectionRequired)
                    continue;

                if (effective == LabSpecimenStatus.Planned)
                    continue;

                // Termasuk OnHold tanpa status sebelum ditahan: keadaannya tidak dapat dipastikan,
                // sehingga diperlakukan sebagai sampel yang sudah berjalan.
                adaPenghalang = true;
                if (effective != LabSpecimenStatus.Rejected)
                    semuaDitolak = false;
            }

            if (!adaPenghalang)
                return null;

            return semuaDitolak ? SampleRejectedMessage : SampleTakenMessage;
        }
    }

    /// <summary>
    /// Potret pesanan yang dibutuhkan <see cref="LabRescheduleEligibility"/>. Wadah yang terhapus
    /// tidak dimasukkan pemanggil.
    /// </summary>
    /// <param name="OrderStatus">Status pesanan saat ini.</param>
    /// <param name="StatusBeforeHold">Status pesanan sebelum ditahan; dinilai bila pesanan <c>OnHold</c>.</param>
    /// <param name="PlannedDate">Tanggal rencana saat ini — dipakai penanda <i>Terlewat</i>.</param>
    /// <param name="Specimens">Wadah pesanan yang tidak terhapus.</param>
    /// <param name="EncounterStatus">Status kunjungan pesanan, <c>null</c> bila tidak terbaca.</param>
    /// <param name="IsMcuLinked"><c>CoveringMcuOrderId</c> terisi, <b>atau</b> ada komponen order MCU
    /// tidak terhapus yang menautkan pesanan (<c>LAB-DEC-272</c>).</param>
    /// <param name="HasActiveCito">Ada permintaan <c>Ordered</c> ber-CITO, atau pemeriksaan ber-CITO
    /// yang tidak gugur/batal (<c>LAB-DEC-271</c>).</param>
    public sealed record LabRescheduleSnapshot(
        LabOrderStatus OrderStatus,
        LabOrderStatus? StatusBeforeHold,
        DateOnly PlannedDate,
        IReadOnlyList<LabRescheduleSpecimenState> Specimens,
        EncounterStatus? EncounterStatus,
        bool HasActiveCito,
        bool IsMcuLinked);

    /// <summary>Status satu wadah beserta status sebelum ditahan.</summary>
    public readonly record struct LabRescheduleSpecimenState(
        LabSpecimenStatus SpecimenStatus,
        LabSpecimenStatus? StatusBeforeHold);

    /// <summary>
    /// Hasil <see cref="LabRescheduleEligibility.Evaluate"/>. <see cref="BlockReason"/> adalah pesan
    /// bagi pengguna — sama persis dengan pesan <c>409</c>.
    /// </summary>
    public sealed record LabRescheduleVerdict(
        bool CanReschedule,
        string? BlockReasonCode,
        string? BlockReason);
}
