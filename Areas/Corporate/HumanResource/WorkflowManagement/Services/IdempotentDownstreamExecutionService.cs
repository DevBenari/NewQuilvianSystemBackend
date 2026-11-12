using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Repositories;

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
    /// <c>WF-B8</c> / <c>INT-HR-93</c> — eksekusi domain hilir untuk aksi workflow V2, ditambah pemicu
    /// <c>INT-HR-94</c> (pengingat menunjuk Plh saat cuti pejabat disetujui).
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
    /// <b><c>INT-HR-94</c> (remediasi Gelombang 2, 10 November 2026).</b> Data jabatan tidak punya penanda
    /// "pejabat struktural"; yang ada adalah penugasan manajer (<c>WfpManagerAssignment</c>). Maka "pejabat" diturunkan
    /// dari bukti itu: pemohon cuti yang menjadi manajer aktif dengan wewenang menyetujui permohonan bagi minimal satu
    /// bawahan. Bila cutinya disetujui dan belum ada delegasi Plh aktif yang menutup rentang cuti, pejabat itu menerima
    /// pengingat wajib menunjuk Plh lewat pusat pemberitahuan (satu kali per permohonan cuti).
    /// </para>
    /// </remarks>
    public class IdempotentDownstreamExecutionService : IIdempotentDownstreamExecutionService
    {
        public const string OfficerLeaveDelegationDeepLink = "/hrd/workflow/delegations/new";

        private readonly WorkflowReferenceLifecycleService _referenceLifecycleService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private readonly IHrdNotificationService _notificationService;
        private readonly ILogger<IdempotentDownstreamExecutionService> _logger;

        public IdempotentDownstreamExecutionService(
            WorkflowReferenceLifecycleService referenceLifecycleService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            IHrdNotificationService notificationService,
            ILogger<IdempotentDownstreamExecutionService> logger)
        {
            _referenceLifecycleService = referenceLifecycleService;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task ExecuteDownstreamActionsAsync(Guid workflowInstanceId, CancellationToken cancellationToken = default)
        {
            if (workflowInstanceId == Guid.Empty)
            {
                return;
            }

            await _referenceLifecycleService.HandleAsync(
                workflowInstanceId,
                ResolveActorUserId(),
                cancellationToken);

            try
            {
                await PromptOfficerLeaveDelegationAsync(workflowInstanceId, cancellationToken);
            }
            catch (Exception ex)
            {
                // Pengingat Plh adalah efek samping; kegagalannya tidak boleh membatalkan keputusan yang sudah sah.
                _logger.LogError(ex, "INT-HR-94: pengingat Plh untuk workflow {WorkflowInstanceId} gagal diproses.", workflowInstanceId);
            }
        }

        /// <summary>
        /// <c>INT-HR-94</c>: bila cuti seorang pejabat (manajer aktif) disetujui penuh dan belum ada delegasi Plh yang
        /// menutup rentang cutinya, kirim pengingat wajib menunjuk Plh.
        /// </summary>
        private async Task PromptOfficerLeaveDelegationAsync(Guid workflowInstanceId, CancellationToken cancellationToken)
        {
            var instance = await _context.TrxWorkflowInstances
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == workflowInstanceId && !w.IsDelete, cancellationToken);

            if (instance == null
                || !string.Equals(instance.WorkflowStatus, WorkflowValueConstants.WorkflowStatus.Approved, StringComparison.OrdinalIgnoreCase)
                || !WorkflowReferenceLifecycleService.IsLeaveRequestReference(instance.ReferenceType))
            {
                return;
            }

            var leave = await _context.WfpLeaveRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == instance.ReferenceId && !l.IsDelete, cancellationToken);
            if (leave == null)
            {
                return;
            }

            var today = DateTime.UtcNow.Date;
            var isOfficer = await _context.WfpManagerAssignments
                .AsNoTracking()
                .AnyAsync(m => m.ManagerWorkforceProfileId == leave.WorkforceProfileId
                               && m.IsActive && !m.IsDelete && m.CanApproveRequests
                               && m.EffectiveStartDate <= today
                               && (m.EffectiveEndDate == null || m.EffectiveEndDate >= today), cancellationToken);
            if (!isOfficer)
            {
                return;
            }

            var leaveStart = leave.StartDate.ToDateTime(TimeOnly.MinValue);
            var leaveEnd = leave.EndDate.ToDateTime(TimeOnly.MaxValue);

            var principalEmployeeId = leave.EmployeeId ?? instance.RequestedByEmployeeId;
            var hasActiveDelegation = await _context.TrxApprovalDelegations
                .AsNoTracking()
                .AnyAsync(d => d.IsActive && !d.IsDelete
                               && (d.DelegatorWorkforceProfileId == leave.WorkforceProfileId
                                   || (principalEmployeeId != null && d.PrincipalEmployeeId == principalEmployeeId))
                               && d.EffectiveStartAt <= leaveStart
                               && d.EffectiveEndAt >= leaveEnd, cancellationToken);
            if (hasActiveDelegation)
            {
                return;
            }

            var sent = await _notificationService.NotifyWorkforceProfileAsync(
                leave.WorkforceProfileId,
                new HrdNotificationRequest(
                    Title: "Cuti Anda disetujui — tunjuk Pelaksana Harian (Plh) sebelum cuti dimulai",
                    Body: $"Cuti Anda pada {leave.StartDate:dd MMMM yyyy} s.d. {leave.EndDate:dd MMMM yyyy} telah disetujui. Anda tercatat sebagai atasan yang menyetujui permohonan staf, dan belum ada delegasi wewenang Plh yang menutup masa cuti tersebut. Mohon daftarkan Plh agar kotak masuk persetujuan staf tidak terbengkalai (INT-HR-94).",
                    Category: HrdNotificationCategories.Workflow,
                    Priority: HrdNotificationPriorities.High,
                    DeepLinkUrl: $"{OfficerLeaveDelegationDeepLink}?leaveStart={leave.StartDate:yyyy-MM-dd}&leaveEnd={leave.EndDate:yyyy-MM-dd}",
                    SourceModule: "Hrd.WorkflowV2.INT-HR-94",
                    ReferenceType: "WfpLeaveRequest",
                    ReferenceId: leave.Id,
                    DedupKey: $"INT-HR-94|{leave.Id:N}",
                    RequestedChannel: "WebPush",
                    ActorUserId: ResolveActorUserId()),
                cancellationToken);

            if (sent > 0)
            {
                _logger.LogInformation("INT-HR-94: pengingat Plh dikirim untuk cuti {LeaveId} pejabat {WorkforceProfileId}.", leave.Id, leave.WorkforceProfileId);
            }
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
