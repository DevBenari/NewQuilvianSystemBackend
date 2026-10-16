using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs
{
    /// <summary>Penyaring daftar Template Hasil (<c>BE-RAD-26</c>).</summary>
    public class RadReportTemplateQuery
    {
        /// <summary>Template untuk alat ini ditambah template umum.</summary>
        public Guid? ModalityId { get; set; }

        /// <summary>Template untuk pemeriksaan ini ditambah template umum.</summary>
        public Guid? ProcedureId { get; set; }

        /// <summary>Benar (bawaan) = hanya yang aktif — dipakai layar tulis draf.</summary>
        public bool OnlyActive { get; set; } = true;

        public string? Search { get; set; }
    }

    public class RadReportTemplateRequest
    {
        [Required, MaxLength(50)]
        public string TemplateCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string TemplateName { get; set; } = string.Empty;

        public Guid? ModalityId { get; set; }

        public Guid? ProcedureId { get; set; }

        [MaxLength(8000)]
        public string? Findings { get; set; }

        [Required, MaxLength(4000)]
        public string Impression { get; set; } = string.Empty;

        [MaxLength(4000)]
        public string? Recommendation { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }
    }

    public class RadReportTemplateResponse
    {
        public Guid Id { get; set; }
        public string TemplateCode { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public Guid? ModalityId { get; set; }
        public string? ModalityName { get; set; }
        public Guid? ProcedureId { get; set; }
        public string? ProcedureName { get; set; }
        public string? Findings { get; set; }
        public string Impression { get; set; } = string.Empty;
        public string? Recommendation { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }
}
