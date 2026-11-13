using System.Text.Json;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;

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

        public ProcurementApprovalGateway(WorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

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
