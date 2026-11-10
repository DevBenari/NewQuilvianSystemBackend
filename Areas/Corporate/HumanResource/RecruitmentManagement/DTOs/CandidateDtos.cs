using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs
{
    public sealed class RegisterCandidateRequest
    {
        [Required(ErrorMessage = "NIK KTP wajib diisi.")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "NIK harus terdiri dari 16 digit angka.")]
        public string NationalIdNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        public DateOnly BirthDate { get; set; }

        [MaxLength(100)]
        public string BirthPlace { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nomor telepon wajib diisi.")]
        [MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;
    }

    public sealed class UpdateCandidateRequest
    {
        [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        public DateOnly BirthDate { get; set; }

        [MaxLength(100)]
        public string BirthPlace { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nomor telepon wajib diisi.")]
        [MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "RowVersion wajib disertakan.")]
        public Guid RowVersion { get; set; }
    }

    public sealed class ApplyVacancyRequest
    {
        [Required(ErrorMessage = "VacancyId wajib diisi.")]
        public Guid VacancyId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public sealed class UploadCandidateDocumentRequest
    {
        public Guid? ApplicationId { get; set; }

        public DocumentType DocumentType { get; set; }

        [MaxLength(100)]
        public string? DocumentNumber { get; set; }

        [Required(ErrorMessage = "Judul dokumen wajib diisi.")]
        [MaxLength(200)]
        public string DocumentTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "File path wajib diisi.")]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        [MaxLength(100)]
        public string MimeType { get; set; } = "application/pdf";

        public DateOnly? ValidFrom { get; set; }

        public DateOnly? ValidUntil { get; set; }
    }

    public sealed class CandidateResponse
    {
        public Guid Id { get; set; }
        public string CandidateCode { get; set; } = string.Empty;
        public string NationalIdNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateOnly BirthDate { get; set; }
        public string BirthPlace { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool IsRehire { get; set; }
        public Guid? PreviousWorkforceProfileId { get; set; }
        public string? PreviousEmployeeNumber { get; set; }
        public CandidateStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public Guid RowVersion { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public List<CandidateApplicationResponse> Applications { get; set; } = new();
        public List<CandidateDocumentResponse> Documents { get; set; } = new();
    }

    public sealed class CandidateListItemResponse
    {
        public Guid Id { get; set; }
        public string CandidateCode { get; set; } = string.Empty;
        public string NationalIdNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsRehire { get; set; }
        public CandidateStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime CreateDateTime { get; set; }
    }

    public sealed class CandidateApplicationResponse
    {
        public Guid Id { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public Guid VacancyId { get; set; }
        public string? VacancyTitle { get; set; }
        public DateTime ApplicationDate { get; set; }
        public ApplicationStage CurrentStage { get; set; }
        public string CurrentStageName => CurrentStage.ToString();
        public string StageStatus { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public sealed class CandidateDocumentResponse
    {
        public Guid Id { get; set; }
        public Guid CandidateId { get; set; }
        public Guid? ApplicationId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string DocumentTypeName => DocumentType.ToString();
        public string? DocumentNumber { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public DateOnly? ValidFrom { get; set; }
        public DateOnly? ValidUntil { get; set; }
        public bool IsVerified { get; set; }
        public string? VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }

    public sealed class CandidateQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public string? SearchNik { get; set; }
        public bool? IsRehire { get; set; }
        public CandidateStatus? Status { get; set; }
    }
}
