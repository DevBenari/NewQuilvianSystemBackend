using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class DailyNursingActionSummaryResponse
    {
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public int CategoryCount { get; set; }
    }

    public class DailyNursingActionListItemDto
    {
        public Guid Id { get; set; }
        public string ActionCode { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? DefaultNotes { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class DailyNursingActionDetailDto : DailyNursingActionListItemDto
    {
        public Guid? CreateBy { get; set; }
        public Guid? UpdateBy { get; set; }
    }

    public class DailyNursingActionActiveItemDto
    {
        public Guid Id { get; set; }
        public string ActionCode { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? DefaultNotes { get; set; }
        public int SortOrder { get; set; }
    }

    public class CreateDailyNursingActionRequest
    {
        [Required(ErrorMessage = "Kode tindakan wajib diisi.")]
        [MaxLength(50, ErrorMessage = "Kode tindakan maksimal 50 karakter.")]
        public string ActionCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama tindakan wajib diisi.")]
        [MaxLength(250, ErrorMessage = "Nama tindakan maksimal 250 karakter.")]
        public string ActionName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori tindakan wajib diisi.")]
        [MaxLength(100, ErrorMessage = "Kategori tindakan maksimal 100 karakter.")]
        public string Category { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Catatan default maksimal 500 karakter.")]
        public string? DefaultNotes { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateDailyNursingActionRequest
    {
        [Required(ErrorMessage = "Nama tindakan wajib diisi.")]
        [MaxLength(250, ErrorMessage = "Nama tindakan maksimal 250 karakter.")]
        public string ActionName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori tindakan wajib diisi.")]
        [MaxLength(100, ErrorMessage = "Kategori tindakan maksimal 100 karakter.")]
        public string Category { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Catatan default maksimal 500 karakter.")]
        public string? DefaultNotes { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
