using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public interface IIdempotentDownstreamExecutionService
    {
        /// <summary>
        /// Meneruskan keputusan workflow yang <b>sudah tersimpan</b> ke transaksi sumbernya.
        /// Wajib dipanggil sesudah <c>SaveChangesAsync</c>, karena handler membaca instance dari database.
        /// </summary>
        Task ExecuteDownstreamActionsAsync(Guid workflowInstanceId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// <c>WF-B8</c> / <c>INT-HR-93</c> — eksekusi domain hilir untuk aksi workflow V2.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Service ini sengaja tidak menulis ulang aturan modul hilir. Penanganan per reference type —
    /// cuti, penyesuaian dan pembatalan cuti, recall cuti, lembur, perubahan jadwal, tukar shift,
    /// koreksi presensi, perubahan profil, resign, dan perencanaan tenaga kerja — sudah dimiliki
    /// <see cref="WorkflowReferenceLifecycleService"/> beserta handler milik tiap modul. Mesin
    /// workflow utama (<c>WorkflowService</c>) memakai pintu yang sama, sehingga persetujuan dari
    /// kotak masuk V2 dan dari mesin utama menghasilkan efek hilir yang identik.
    /// </para>
    /// <para>
    /// <b>Idempoten.</b> Handler bekerja dengan pola sinkronisasi: status transaksi sumber diturunkan
    /// ulang dari status workflow saat ini. Pemanggilan kedua atas workflow yang sama menghasilkan
    /// keadaan yang sama, bukan transaksi kedua. Penjaga agar keputusan yang sama tidak diproses dua
    /// kali berada di <see cref="WorkflowApprovalActionService"/>.
    /// </para>
    /// <para>
    /// Kegagalan sinkronisasi dicatat oleh <see cref="WorkflowReferenceLifecycleService.HandleAsync"/>
    /// dan tidak membatalkan keputusan workflow yang sudah sah — perilaku yang sama dengan mesin utama.
    /// </para>
    /// <para>
    /// Reference type yang tidak punya handler (misalnya klaim atau perjalanan dinas, yang memakai
    /// endpoint persetujuannya sendiri) dikembalikan handler sebagai <c>IsHandled = false</c> tanpa efek.
    /// </para>
    /// </remarks>
    public class IdempotentDownstreamExecutionService : IIdempotentDownstreamExecutionService
    {
        private readonly WorkflowReferenceLifecycleService _referenceLifecycleService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IdempotentDownstreamExecutionService(
            WorkflowReferenceLifecycleService referenceLifecycleService,
            IHttpContextAccessor httpContextAccessor)
        {
            _referenceLifecycleService = referenceLifecycleService;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task ExecuteDownstreamActionsAsync(Guid workflowInstanceId, CancellationToken cancellationToken = default)
        {
            if (workflowInstanceId == Guid.Empty)
            {
                return Task.CompletedTask;
            }

            return _referenceLifecycleService.HandleAsync(
                workflowInstanceId,
                ResolveActorUserId(),
                cancellationToken);
        }

        /// <summary>
        /// Handler hilir mencatat aktor sebagai <b>user id</b>, sama seperti mesin utama. Nilai ini
        /// diambil langsung dari token, bukan dari identitas pegawai yang dipakai service V2.
        /// </summary>
        /// <remarks>
        /// <c>Guid.Empty</c> hanya terjadi di luar request HTTP. Pada keadaan itu handler memakai
        /// pemohon workflow sebagai aktor audit — perilaku handler yang sudah ada.
        /// </remarks>
        private Guid ResolveActorUserId()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            var userIdText =
                principal?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                principal?.FindFirstValue("user_id");

            return Guid.TryParse(userIdText, out var userId) ? userId : Guid.Empty;
        }
    }
}
