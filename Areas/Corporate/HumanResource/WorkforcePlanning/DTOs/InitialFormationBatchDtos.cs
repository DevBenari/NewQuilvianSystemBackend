using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs
{
    /// <summary>
    /// Permintaan pembuatan draf batch formasi awal baru.
    /// Sesuai kontrak API v7 Bagian 13 grup Initial Formation Batch (BE-HRD-033).
    /// </summary>
    public sealed class CreateInitialFormationBatchRequest
    {
        [Required(ErrorMessage = "ID rumah sakit wajib diisi.")]
        public Guid HospitalSiteId { get; set; }

        [Required(ErrorMessage = "Nomor/rujukan dokumen sumber wajib diisi.")]
        [MaxLength(200, ErrorMessage = "Nomor dokumen sumber maksimal 200 karakter.")]
        public string SourceDocumentReference { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Catatan maksimal 1000 karakter.")]
        public string? Notes { get; set; }

        public List<InitialFormationBatchRowRequest> Rows { get; set; } = new();
    }

    /// <summary>
    /// Permintaan pembaruan draf batch formasi awal.
    /// </summary>
    public sealed class UpdateInitialFormationBatchRequest
    {
        [Required(ErrorMessage = "Nomor/rujukan dokumen sumber wajib diisi.")]
        [MaxLength(200, ErrorMessage = "Nomor dokumen sumber maksimal 200 karakter.")]
        public string SourceDocumentReference { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Catatan maksimal 1000 karakter.")]
        public string? Notes { get; set; }

        public List<InitialFormationBatchRowRequest> Rows { get; set; } = new();

        [Required(ErrorMessage = "RowVersion wajib disertakan.")]
        public Guid RowVersion { get; set; }
    }

    /// <summary>
    /// Data satu baris formasi jabatan dalam batch.
    /// </summary>
    public sealed class InitialFormationBatchRowRequest
    {
        [Required(ErrorMessage = "Unit organisasi wajib diisi.")]
        public Guid OrganizationUnitId { get; set; }

        [Required(ErrorMessage = "Jabatan wajib diisi.")]
        public Guid PositionId { get; set; }

        [Range(0.01, 999999.99, ErrorMessage = "Jumlah formasi harus lebih besar dari 0.")]
        public decimal FormationQuantity { get; set; }

        [Required(ErrorMessage = "Tanggal berlaku mulai wajib diisi.")]
        public DateOnly EffectiveStartDate { get; set; }
    }

    /// <summary>
    /// Hasil validasi satu baris formasi jabatan dalam batch (VR-WFP-11).
    /// </summary>
    public sealed class InitialFormationBatchRowValidationResponse
    {
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid PositionId { get; set; }
        public string? PositionName { get; set; }
        public decimal FormationQuantity { get; set; }
        public DateOnly EffectiveStartDate { get; set; }
        public bool IsValid { get; set; }
        public string? ValidationMessage { get; set; }
    }

    /// <summary>
    /// Item ringkas dalam daftar batch formasi awal.
    /// </summary>
    public sealed class InitialFormationBatchListItemResponse
    {
        public Guid Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public Guid HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public string SourceDocumentReference { get; set; } = string.Empty;
        public string? SourceDocumentFileName { get; set; }
        public InitialFormationBatchStatus BatchStatus { get; set; }
        public string BatchStatusName => BatchStatus.ToString();
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? SubmittedByUserName { get; set; }
        public DateTime? AppliedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid RowVersion { get; set; }
    }

    /// <summary>
    /// Detail lengkap batch formasi awal beserta baris-baris formasinya.
    /// </summary>
    public sealed class InitialFormationBatchResponse
    {
        public Guid Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public Guid HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public string SourceDocumentReference { get; set; } = string.Empty;
        public string? SourceDocumentPath { get; set; }
        public string? SourceDocumentFileName { get; set; }
        public InitialFormationBatchStatus BatchStatus { get; set; }
        public string BatchStatusName => BatchStatus.ToString();
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public string? Notes { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? SubmittedByUserName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedByUserName { get; set; }
        public DateTime? AppliedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid RowVersion { get; set; }

        public List<InitialFormationBatchRowResponse> Rows { get; set; } = new();
    }

    /// <summary>
    /// Detail satu baris formasi dalam batch.
    /// </summary>
    public sealed class InitialFormationBatchRowResponse
    {
        public Guid Id { get; set; }
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitCode { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid PositionId { get; set; }
        public string? PositionCode { get; set; }
        public string? PositionName { get; set; }
        public decimal FormationQuantity { get; set; }
        public DateOnly EffectiveStartDate { get; set; }
        public bool IsValid { get; set; }
        public string? ValidationMessage { get; set; }
        public Guid? CreatedFormationId { get; set; }
    }

    /// <summary>
    /// Permintaan pengajuan batch ke workflow T10.
    /// </summary>
    public sealed class SubmitInitialFormationBatchRequest
    {
        public string? Notes { get; set; }

        [Required(ErrorMessage = "RowVersion wajib disertakan.")]
        public Guid RowVersion { get; set; }
    }

    /// <summary>
    /// Permintaan pembatalan draf atau batch yang dikembalikan.
    /// </summary>
    public sealed class CancelInitialFormationBatchRequest
    {
        public string? Reason { get; set; }

        [Required(ErrorMessage = "RowVersion wajib disertakan.")]
        public Guid RowVersion { get; set; }
    }

    /// <summary>
    /// Parameter pencarian batch berhalaman.
    /// </summary>
    public sealed class InitialFormationBatchPagedQuery
    {
        public Guid? HospitalSiteId { get; set; }
        public InitialFormationBatchStatus? Status { get; set; }
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }
}
