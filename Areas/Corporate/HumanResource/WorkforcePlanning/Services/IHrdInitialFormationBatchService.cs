using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Antarmuka layanan batch formasi awal rumah sakit (BE-HRD-033).
    /// Mengelola templat isian, validasi baris, pembuatan draf, unggah scan SK, pengajuan ke T10, dan pembatalan.
    /// </summary>
    public interface IHrdInitialFormationBatchService
    {
        /// <summary>
        /// Menghasilkan berkas Excel (.xlsx) templat isian formasi awal (kriteria 1).
        /// </summary>
        byte[] GenerateTemplate();

        /// <summary>
        /// Memvalidasi baris-baris formasi awal terhadap aturan VR-WFP-11 (kriteria 2).
        /// </summary>
        Task<List<InitialFormationBatchRowValidationResponse>> ValidateRowsAsync(
            List<InitialFormationBatchRowRequest> rows,
            Guid? currentBatchId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil daftar batch formasi berhalaman.
        /// </summary>
        Task<PagedResult<InitialFormationBatchListItemResponse>> GetPagedListAsync(
            InitialFormationBatchPagedQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil detail satu batch formasi awal beserta baris-barisnya.
        /// </summary>
        Task<InitialFormationBatchResponse?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Membuat draf batch formasi awal baru (status Draft).
        /// </summary>
        Task<WorkforcePlanningResult<InitialFormationBatchResponse>> CreateDraftAsync(
            CreateInitialFormationBatchRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengubah draf batch formasi awal selagi berstatus Draft atau Returned (kriteria 8).
        /// </summary>
        Task<WorkforcePlanningResult<InitialFormationBatchResponse>> UpdateDraftAsync(
            Guid id,
            UpdateInitialFormationBatchRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengunggah scan dokumen fisik SK formasi resmi (PDF/JPG/PNG max 10MB, kriteria 3).
        /// </summary>
        Task<WorkforcePlanningResult<InitialFormationBatchResponse>> UploadSourceDocumentAsync(
            Guid id,
            IFormFile file,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil stream berkas scan dokumen sumber untuk diunduh.
        /// </summary>
        Task<(Stream Stream, string ContentType, string FileName)?> GetSourceDocumentStreamAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengajukan batch formasi ke workflow persetujuan T10 (kriteria 4 dan 5).
        /// </summary>
        Task<WorkforcePlanningResult<InitialFormationBatchResponse>> SubmitAsync(
            Guid id,
            SubmitInitialFormationBatchRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Membatalkan draf atau dokumen yang dikembalikan (kriteria 7).
        /// </summary>
        Task<WorkforcePlanningResult<InitialFormationBatchResponse>> CancelAsync(
            Guid id,
            CancelInitialFormationBatchRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default);
    }
}
