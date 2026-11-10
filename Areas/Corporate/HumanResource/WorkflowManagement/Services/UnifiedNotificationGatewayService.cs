using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public interface IUnifiedNotificationGatewayService
    {
        /// <summary>
        /// Pemberitahuan instan ke satu pegawai (<c>MstEmployee.Id</c>, atau profil tenaga kerja bila itu yang dikenal pemanggil).
        /// </summary>
        Task SendInstantNotificationAsync(Guid recipientEmployeeId, string title, string message, string channel = "WebPush");

        /// <summary>
        /// Pemberitahuan instan ke satu akun pengguna.
        /// </summary>
        Task SendInstantNotificationToUserAsync(Guid recipientUserId, string title, string message, string? deepLinkUrl = null, Guid? workflowInstanceId = null, string priority = HrdNotificationPriorities.Normal, string? dedupKey = null);

        /// <summary>
        /// Ringkasan harian permohonan tertunda ke setiap approver (WF-W3). Mengembalikan jumlah approver yang diberi tahu.
        /// </summary>
        Task<int> SendApprovalDigestAsync();
    }

    /// <summary>
    /// Gateway notifikasi workflow V2 (BE-HRD-314, WF-B7, INT-HR-95).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Remediasi Gelombang 2 (10 November 2026).</b> Sebelumnya metode pengiriman hanya berisi
    /// <c>await Task.CompletedTask</c>, sehingga tidak ada satu pun pemberitahuan yang pernah sampai. Kini
    /// gateway menyalurkan ke pusat pemberitahuan dalam aplikasi <see cref="IHrdNotificationService"/>
    /// (tabel <c>HrdNotification</c> + SignalR). Nama kanal yang diminta pemanggil (<c>WebPush</c>,
    /// <c>EmailDigest</c>) disimpan sebagai jejak; platform belum punya pengirim email/WhatsApp.
    /// </para>
    /// <para>
    /// Ringkasan harian dikunci dengan kunci anti-duplikat per approver per tanggal WIB, sehingga worker yang
    /// terbangun lebih dari sekali dalam sehari tidak mengirim ringkasan ganda.
    /// </para>
    /// </remarks>
    public class UnifiedNotificationGatewayService : IUnifiedNotificationGatewayService
    {
        public const string SourceModule = "Hrd.WorkflowV2";
        public const string ApprovalInboxDeepLink = "/hrd/approvals/inbox?filter=pending";
        private static readonly TimeSpan WibOffset = TimeSpan.FromHours(7);

        private readonly ApplicationDbContext _context;
        private readonly IHrdNotificationService _notificationService;
        private readonly ILogger<UnifiedNotificationGatewayService> _logger;

        public UnifiedNotificationGatewayService(
            ApplicationDbContext context,
            IHrdNotificationService notificationService,
            ILogger<UnifiedNotificationGatewayService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task SendInstantNotificationAsync(Guid recipientEmployeeId, string title, string message, string channel = "WebPush")
        {
            if (recipientEmployeeId == Guid.Empty) return;

            var request = new HrdNotificationRequest(
                Title: title,
                Body: message,
                Category: HrdNotificationCategories.Workflow,
                Priority: HrdNotificationPriorities.Normal,
                DeepLinkUrl: ApprovalInboxDeepLink,
                SourceModule: SourceModule,
                RequestedChannel: channel);

            // Pemanggil lama mengirim ID karyawan; sebagian data V2 lama menyimpan ID akun pada kolom yang sama.
            var sent = await _notificationService.NotifyEmployeeAsync(recipientEmployeeId, request);
            if (sent == 0)
            {
                sent = await _notificationService.NotifyWorkforceProfileAsync(recipientEmployeeId, request);
            }

            if (sent == 0)
            {
                sent = await _notificationService.NotifyUserAsync(recipientEmployeeId, request);
            }

            if (sent == 0)
            {
                _logger.LogWarning("Pemberitahuan '{Title}' tidak menemukan akun aktif untuk penerima {RecipientId}.", title, recipientEmployeeId);
            }
        }

        public Task SendInstantNotificationToUserAsync(Guid recipientUserId, string title, string message, string? deepLinkUrl = null, Guid? workflowInstanceId = null, string priority = HrdNotificationPriorities.Normal, string? dedupKey = null)
        {
            if (recipientUserId == Guid.Empty) return Task.CompletedTask;

            return _notificationService.NotifyUserAsync(recipientUserId, new HrdNotificationRequest(
                Title: title,
                Body: message,
                Category: HrdNotificationCategories.Workflow,
                Priority: priority,
                DeepLinkUrl: deepLinkUrl ?? ApprovalInboxDeepLink,
                SourceModule: SourceModule,
                ReferenceType: workflowInstanceId.HasValue ? "TrxWorkflowInstance" : null,
                ReferenceId: workflowInstanceId,
                DedupKey: dedupKey,
                RequestedChannel: "WebPush"));
        }

        public async Task<int> SendApprovalDigestAsync()
        {
            // Worker WF-W3: ringkasan harian tiket pending setiap pukul 07:00 WIB
            var pendingAssignments = await _context.TrxWorkflowApproverAssignments
                .AsNoTracking()
                .Include(a => a.WorkflowInstance)
                .Where(a => a.AssignmentStatus == "Pending" && a.IsCurrentAssignment && !a.IsDelete)
                .ToListAsync();

            if (pendingAssignments.Count == 0)
            {
                return 0;
            }

            var wibDate = (DateTime.UtcNow + WibOffset).Date;
            var notifiedApprovers = 0;

            // Penerima diturunkan dari akun approver bila ada; bila belum terisi, dari ID karyawan approver.
            var groups = pendingAssignments
                .Select(a => new
                {
                    Key = a.AssignedApproverUserId != Guid.Empty ? a.AssignedApproverUserId : (a.ApproverEmployeeId ?? Guid.Empty),
                    IsUser = a.AssignedApproverUserId != Guid.Empty,
                    Assignment = a
                })
                .Where(x => x.Key != Guid.Empty)
                .GroupBy(x => (x.Key, x.IsUser));

            foreach (var group in groups)
            {
                var pendingCount = group.Count();
                var urgentCount = group.Count(x => x.Assignment.WorkflowInstance != null && x.Assignment.WorkflowInstance.IsOverdue);

                var request = new HrdNotificationRequest(
                    Title: "Ringkasan Persetujuan Harian (07:00 WIB)",
                    Body: $"Selamat pagi, Anda memiliki {pendingCount} permohonan tertunda ({urgentCount} mendesak) yang menunggu persetujuan Anda sebelum ronde dinas.",
                    Category: HrdNotificationCategories.Workflow,
                    Priority: urgentCount > 0 ? HrdNotificationPriorities.High : HrdNotificationPriorities.Normal,
                    DeepLinkUrl: ApprovalInboxDeepLink,
                    SourceModule: SourceModule,
                    DedupKey: $"WF-W3-DIGEST|{wibDate:yyyyMMdd}",
                    RequestedChannel: "EmailDigest");

                var sent = group.Key.IsUser
                    ? await _notificationService.NotifyUserAsync(group.Key.Key, request)
                    : await _notificationService.NotifyEmployeeAsync(group.Key.Key, request);

                if (sent > 0) notifiedApprovers++;
            }

            return notifiedApprovers;
        }
    }
}
