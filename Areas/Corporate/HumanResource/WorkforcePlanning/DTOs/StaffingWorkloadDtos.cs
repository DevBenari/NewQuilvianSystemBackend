using System;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs
{
    /// <summary>
    /// Permintaan pengisian atau perbaikan angka beban kerja manual (BE-HRD-034).
    /// Sesuai kontrak API v7 Bagian 13 grup Staffing Workload.
    /// </summary>
    public sealed class UpsertStaffingWorkloadRequest
    {
        [Required(ErrorMessage = "Unit organisasi wajib dipilih.")]
        public Guid OrganizationUnitId { get; set; }

        [Required(ErrorMessage = "Jabatan wajib dipilih.")]
        public Guid PositionId { get; set; }

        [Required(ErrorMessage = "Tanggal mulai periode wajib diisi.")]
        public DateOnly PeriodStartDate { get; set; }

        [Required(ErrorMessage = "Tanggal akhir periode wajib diisi.")]
        public DateOnly PeriodEndDate { get; set; }

        [Range(0, 999999.99, ErrorMessage = "Angka beban tidak boleh bernilai negatif.")]
        public decimal WorkloadValue { get; set; }

        [Required(ErrorMessage = "Tuliskan sumber angka beban, misalnya rekap sensus harian.")]
        [MaxLength(500, ErrorMessage = "Keterangan sumber maksimal 500 karakter.")]
        public string SourceNote { get; set; } = string.Empty;
    }

    /// <summary>
    /// Respons data angka beban kerja unit dan jabatan rumah sakit.
    /// </summary>
    public sealed class StaffingWorkloadResponse
    {
        public Guid Id { get; set; }
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitCode { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid PositionId { get; set; }
        public string? PositionCode { get; set; }
        public string? PositionName { get; set; }
        public DateOnly PeriodStartDate { get; set; }
        public DateOnly PeriodEndDate { get; set; }
        public decimal WorkloadValue { get; set; }
        public string SourceNote { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Parameter pencarian angka beban kerja berhalaman.
    /// </summary>
    public sealed class StaffingWorkloadPagedQuery
    {
        public Guid? OrganizationUnitId { get; set; }
        public Guid? PositionId { get; set; }
        public DateOnly? PeriodDate { get; set; }
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }
}
