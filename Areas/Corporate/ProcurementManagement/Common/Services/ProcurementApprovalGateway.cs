using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services
{
    /// <summary>Permintaan pengiriman satu dokumen Pengadaan ke approval bersama (Wfl).</summary>
    /// <param name="DocumentType">Jenis dokumen, mis. <c>PBJ_PR</c>; dipakai sebagai <c>ReferenceType</c>.</param>
    /// <param name="DocumentId">ID dokumen.</param>
    /// <param name="DocumentNumber">Nomor dokumen, mis. <c>PR-000031</c>; tampil sebagai nomor rujukan di kotak masuk.</param>
    /// <param name="DefinitionCode">Kode definisi dari <see cref="ProcurementWorkflowCodes"/>.</param>
    /// <param name="RequesterUserId">Pengusul dokumen (bukan petugas yang menekan tombol).</param>
    /// <param name="Amount">Nominal penentu jenjang matriks, mis. nilai kebutuhan beli PR.</param>
    /// <param name="ConditionCodes">Kode kondisi pengaktif langkah, mis. <c>PBJ_OVER_BUDGET</c>.</param>
    /// <param name="SubmissionCount">Urutan pengajuan dokumen; bagian kunci idempoten.</param>
    public sealed record ProcurementApprovalRequest(
        string DocumentType,
        Guid DocumentId,
        string? DocumentNumber,
        string DefinitionCode,
        Guid RequesterUserId,
        decimal? Amount,
        IReadOnlyCollection<string> ConditionCodes,
        int SubmissionCount);

    /// <summary>Hasil pengiriman ke approval.</summary>
    public sealed record ProcurementApprovalResult(
        bool Success,
        Guid? WorkflowInstanceId,
        string? WorkflowStatus,
        bool ConfigurationMissing,
        int StatusCode,
        string Message);

    /// <summary>
    /// Satu-satunya pintu Pengadaan ke mesin approval bersama (BE-PBJ-005; PBJ-DEC-019, 027, 030,
    /// 038; <c>02-backend-architecture.md</c> bagian 8).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Instance dibuat dan diajukan <b>atas nama pengusul</b> lewat
    /// <see cref="WorkflowService.CreateAndSubmitOnBehalfAsync"/>. Contoh: Tim Pengadaan menyelesaikan
    /// verifikasi PR-000031 milik pengusul IGD. Pemohon instance tetap pengusul IGD, sehingga
    /// jenjang dipilih dari unit IGD dan pengusul tidak dapat menyetujui PR-nya sendiri.
    /// </para>
    /// <para>
    /// <b>Urutan dan pengulangan.</b> Pemanggil memanggil gateway lebih dulu, lalu menyimpan status
    /// dokumennya. Kunci idempoten <c>&lt;DocumentType&gt;:&lt;DocumentId&gt;:&lt;SubmissionCount&gt;</c>
    /// membuat pengulangan setelah kegagalan penyimpanan dokumen mengembalikan instance yang sama,
    /// bukan instance kedua.
    /// </para>
    /// <para>
    /// Definisi yang tidak ditemukan atau tidak aktif dilaporkan sebagai
    /// <see cref="ProcurementApprovalResult.ConfigurationMissing"/> (PBJ-VAL-091), supaya pemanggil
    /// dapat menolak dengan pesan "alur persetujuan belum dikonfigurasi".
    /// </para>
    /// </remarks>
    public sealed class ProcurementApprovalGateway
    {
        private readonly WorkflowService _workflowService;
        private readonly ApplicationDbContext _dbContext;

        public ProcurementApprovalGateway(WorkflowService workflowService, ApplicationDbContext dbContext)
        {
            _workflowService = workflowService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Menutup instance lama yang dikembalikan penyetuju (<c>Returned</c>/<c>RevisionRequested</c>)
        /// sebelum dokumen diajukan ulang (BE-PBJ-009). Tanpa ini, Wfl menolak instance baru karena
        /// "transaksi sumber masih memiliki workflow aktif".
        /// </summary>
        /// <remarks>
        /// Contoh: rencana IGD dikembalikan Manajer Pengadaan, diperbaiki, lalu diajukan lagi. Instance
        /// lama dibatalkan atas nama pemohonnya, lalu instance baru dibuat dengan kunci idempoten
        /// berikutnya. Instance yang sudah final atau tidak ada dibiarkan. Wajib dipanggil sebelum
        /// pemanggil mengubah entity yang dilacak, karena Wfl menyimpan context yang sama.
        /// </remarks>
        public async Task<ProcurementApprovalResult> CloseReturnedAsync(
            Guid? workflowInstanceId,
            string reason,
            CancellationToken cancellationToken)
        {
            if (!workflowInstanceId.HasValue)
                return new ProcurementApprovalResult(true, null, null, false, StatusCodes.Status200OK, string.Empty);

            var instance = await _dbContext.Set<TrxWorkflowInstance>()
                .AsNoTracking()
                .Where(x => x.Id == workflowInstanceId.Value && !x.IsDelete)
                .Select(x => new { x.Id, x.WorkflowStatus, x.RequestedByUserId })
                .FirstOrDefaultAsync(cancellationToken);
            if (instance == null || instance.RequestedByUserId == Guid.Empty ||
                !(Eq(instance.WorkflowStatus, WorkflowValueConstants.WorkflowStatus.Returned) ||
                  Eq(instance.WorkflowStatus, WorkflowValueConstants.WorkflowStatus.RevisionRequested)))
            {
                return new ProcurementApprovalResult(true, instance?.Id, instance?.WorkflowStatus, false, StatusCodes.Status200OK, string.Empty);
            }

            var result = await _workflowService.CancelOnBehalfAsync(
                instance.Id,
                new WorkflowCancelRequest { Reason = reason },
                instance.RequestedByUserId,
                cancellationToken);

            return new ProcurementApprovalResult(
                result.Success, instance.Id, result.Data?.WorkflowStatus, false, result.StatusCode, result.Message ?? string.Empty);
        }

        /// <summary>
        /// Menarik instance yang sedang diproses oleh pemohonnya sendiri (pengguna login), dipakai aksi
        /// "Tarik" PR <c>PENDING_APPROVAL</c> (BE-PBJ-007). Wfl hanya mengizinkan tarik saat
        /// <c>InProgress</c>; selain itu dijawab 409.
        /// </summary>
        public async Task<ProcurementApprovalResult> WithdrawAsync(
            Guid workflowInstanceId,
            string reason,
            CancellationToken cancellationToken)
        {
            var result = await _workflowService.WithdrawAsync(
                workflowInstanceId,
                new WorkflowWithdrawRequest { Reason = reason },
                cancellationToken);
            return new ProcurementApprovalResult(
                result.Success, workflowInstanceId, result.Data?.WorkflowStatus, false, result.StatusCode, result.Message ?? string.Empty);
        }

        private static bool Eq(string? a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        public static string BuildIdempotencyKey(string documentType, Guid documentId, int submissionCount) =>
            $"{documentType}:{documentId:N}:{submissionCount}";

        public async Task<ProcurementApprovalResult> SubmitAsync(
            ProcurementApprovalRequest request,
            CancellationToken cancellationToken)
        {
            if (!ProcurementDocumentTypes.IsKnown(request.DocumentType))
                throw new ArgumentException($"Jenis dokumen Pengadaan tidak dikenal: '{request.DocumentType}'.", nameof(request));
            if (request.DocumentId == Guid.Empty || request.RequesterUserId == Guid.Empty)
                throw new ArgumentException("Dokumen dan pengusul wajib diisi.", nameof(request));
            if (string.IsNullOrWhiteSpace(request.DefinitionCode))
                throw new ArgumentException("Kode definisi workflow wajib diisi.", nameof(request));

            var context = JsonSerializer.SerializeToElement(new
            {
                amount = request.Amount,
                conditionCodes = request.ConditionCodes
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray()
            });

            var result = await _workflowService.CreateAndSubmitOnBehalfAsync(
                new CreateWorkflowInstanceRequest
                {
                    WorkflowDefinitionCode = request.DefinitionCode,
                    ReferenceType = request.DocumentType,
                    ReferenceId = request.DocumentId,
                    ExternalReferenceNumber = request.DocumentNumber,
                    SourceChannel = "Web",
                    IdempotencyKey = BuildIdempotencyKey(request.DocumentType, request.DocumentId, request.SubmissionCount),
                    RequestContext = context
                },
                request.RequesterUserId,
                cancellationToken);

            if (result.Success && result.Data != null)
            {
                return new ProcurementApprovalResult(
                    true, result.Data.Id, result.Data.WorkflowStatus, false, result.StatusCode, result.Message);
            }

            // Definisi tidak ditemukan/tidak aktif (404) atau tanpa langkah aktif (400 "belum memiliki
            // step aktif"): konfigurasi approval belum siap (PBJ-VAL-091).
            var configurationMissing =
                result.StatusCode == StatusCodes.Status404NotFound ||
                (result.Message?.Contains("belum memiliki step aktif", StringComparison.OrdinalIgnoreCase) ?? false);

            return new ProcurementApprovalResult(
                false, null, null, configurationMissing, result.StatusCode, result.Message ?? string.Empty);
        }
    }
}
