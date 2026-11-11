using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public interface IUnifiedNotificationGatewayService
    {
        Task SendInstantNotificationAsync(Guid recipientEmployeeId, string title, string message, string channel = "WebPush");
        Task SendApprovalDigestAsync();
    }

    public class UnifiedNotificationGatewayService : IUnifiedNotificationGatewayService
    {
        private readonly ApplicationDbContext _context;

        public UnifiedNotificationGatewayService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendInstantNotificationAsync(Guid recipientEmployeeId, string title, string message, string channel = "WebPush")
        {
            // Simulasi pengiriman SignalR, Webhook push, dan email provider RS
            // Log ke audit atau persistent notification table jika tersedia
            await Task.CompletedTask;
        }

        public async Task SendApprovalDigestAsync()
        {
            // Worker WF-W3: Pengiriman ringkasan harian tiket pending setiap pukul 07:00 pagi
            var pendingAssignments = await _context.TrxWorkflowApproverAssignments
                .Include(a => a.WorkflowInstance)
                .Where(a => a.AssignmentStatus == "Pending" && a.IsCurrentAssignment)
                .ToListAsync();

            var groupedByApprover = pendingAssignments
                .Where(a => a.ApproverEmployeeId.HasValue)
                .GroupBy(a => a.ApproverEmployeeId!.Value);

            foreach (var group in groupedByApprover)
            {
                var approverId = group.Key;
                var pendingCount = group.Count();
                var urgentCount = group.Count(a => a.WorkflowInstance != null && a.WorkflowInstance.IsOverdue);

                await SendInstantNotificationAsync(
                    approverId,
                    "Ringkasan Persetujuan Harian (07:00 WIB)",
                    $"Selamat pagi, Anda memiliki {pendingCount} permohonan tertunda ({urgentCount} mendesak) yang menunggu persetujuan Anda sebelum ronde dinas.",
                    "EmailDigest");
            }
        }
    }
}
