using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    /// <summary>
    /// Integrasi dokumen sub-modul 05 dengan mesin persetujuan bersama (<c>WorkflowService</c>): T15 penilaian
    /// masa percobaan, T16 keputusan kontrak, T17 koreksi kontrak, T18 batch data awal (<c>INT-HR-30</c>).
    /// </summary>
    public interface IHrdEmploymentLifecycleWorkflowIntegrationService
    {
        /// <summary>Membuat dan mengajukan instance T15 atas nama pengaju. Dokumen tidak diubah.</summary>
        Task<LifecycleWorkflowResult> StartProbationAssessmentWorkflowAsync(Guid assessmentId, Guid submitterUserId, CancellationToken cancellationToken = default);

        /// <summary>Membuat dan mengajukan instance T16 atas nama pengaju. Dokumen tidak diubah.</summary>
        Task<LifecycleWorkflowResult> StartContractDecisionWorkflowAsync(Guid decisionId, Guid submitterUserId, CancellationToken cancellationToken = default);

        /// <summary>Membuat dan mengajukan instance T17 atas nama pengaju. Dokumen tidak diubah.</summary>
        Task<LifecycleWorkflowResult> StartContractCorrectionWorkflowAsync(Guid correctionId, Guid submitterUserId, CancellationToken cancellationToken = default);

        /// <summary>Membuat dan mengajukan instance T18 atas nama pengaju. Dokumen tidak diubah.</summary>
        Task<LifecycleWorkflowResult> StartInitialContractBatchWorkflowAsync(Guid batchId, Guid submitterUserId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengajukan ulang instance yang dikembalikan ke pengaju (Minta Revisi). Pengguna login harus pengaju instance.
        /// </summary>
        Task<LifecycleWorkflowResult> ResubmitAsync(Guid workflowInstanceId, string? comment, CancellationToken cancellationToken = default);

        /// <summary>
        /// Menarik instance atas nama pengajunya. Instance yang tidak ada (dokumen lama sebelum integrasi) atau
        /// sudah selesai dianggap berhasil tanpa perubahan.
        /// </summary>
        Task<LifecycleWorkflowResult> WithdrawAsync(Guid? workflowInstanceId, Guid requesterUserId, string reason, CancellationToken cancellationToken = default);

        /// <summary>True bila instance ada di mesin persetujuan (dokumen lama menyimpan GUID tanpa instance).</summary>
        Task<bool> InstanceExistsAsync(Guid? workflowInstanceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Menerapkan keputusan akhir instance ke dokumennya lewat service domain (maker-checker dan idempoten di sana).
        /// </summary>
        Task<bool> HandleWorkflowCompletedAsync(Guid workflowInstanceId, bool isApproved, Guid deciderUserId, string? decisionNote, CancellationToken cancellationToken = default);
    }

    public sealed class LifecycleWorkflowResult
    {
        public bool Success { get; private init; }

        public int StatusCode { get; private init; }

        public string Message { get; private init; } = string.Empty;

        public Guid? WorkflowInstanceId { get; private init; }

        public static LifecycleWorkflowResult Ok(Guid? workflowInstanceId, string message = "") => new()
        {
            Success = true,
            StatusCode = StatusCodes.Status200OK,
            Message = message,
            WorkflowInstanceId = workflowInstanceId
        };

        public static LifecycleWorkflowResult Fail(int statusCode, string message) => new()
        {
            Success = false,
            StatusCode = statusCode,
            Message = message
        };
    }
}
