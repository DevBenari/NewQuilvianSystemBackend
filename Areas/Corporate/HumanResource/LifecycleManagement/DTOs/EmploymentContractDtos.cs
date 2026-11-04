using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs
{
    public sealed class EmploymentContractResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string ContractNumber { get; set; } = string.Empty;
        public string HistoryType { get; set; } = string.Empty;
        public string ContractStatus { get; set; } = string.Empty;
        public Guid? ContractTypeId { get; set; }
        public string? ContractTypeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? SignedDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string? SourceDocumentType { get; set; }
        public Guid? SourceDocumentId { get; set; }
        public string? DocumentPath { get; set; }
        public string? DocumentFileName { get; set; }
        public Guid RowVersion { get; set; }
    }

    public sealed class EmploymentContractPrintDataResponse
    {
        public Guid ContractId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string HospitalName { get; set; } = string.Empty;
        public string EmployeeFullName { get; set; } = string.Empty;
        public string EmployeeNationalId { get; set; } = string.Empty;
        public string PositionTitle { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string ContractTypeName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? BasicSalary { get; set; }
        public bool IsSalaryMasked { get; set; }
        public string NoticeText => "Perjanjian Kerja ini dibuat dan ditandatangani oleh Para Pihak dalam keadaan sadar dan tanpa paksaan.";
    }

    public sealed class RecordContractSignedRequest
    {
        [Required(ErrorMessage = "Tanggal penandatanganan wajib diisi.")]
        public DateTime SignedDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Berkas scan kontrak bertanda tangan wajib dilampirkan (VR-ONB-30).")]
        public IFormFile SignedScanFile { get; set; } = null!;
    }

    public sealed class EmploymentContractQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? ContractStatus { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public string? Search { get; set; }
    }
}
