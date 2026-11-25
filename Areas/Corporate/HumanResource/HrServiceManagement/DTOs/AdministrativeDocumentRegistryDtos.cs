namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs
{
    /// <summary>Sumber dokumen pada registri administrasi kepegawaian (BE-HRD-385).</summary>
    public static class AdministrativeDocumentSources
    {
        public const string PersonnelActionOrder = "SK";
        public const string AssignmentLetter = "SuratTugas";
        public const string EmployeeLetter = "SuratKeterangan";
        public const string PersonalDocument = "DokumenPersonal";
    }

    /// <summary>Status keberlakuan dokumen pada registri.</summary>
    public static class AdministrativeDocumentValidity
    {
        public const string Valid = "BERLAKU";
        public const string Scheduled = "TERJADWAL";
        public const string Revoked = "TIDAK_BERLAKU";
        public const string Expired = "KEDALUWARSA";

        public static string Label(string value) => value switch
        {
            Valid => "Berlaku",
            Scheduled => "Terjadwal",
            Revoked => "TIDAK BERLAKU",
            Expired => "Kedaluwarsa",
            _ => value
        };
    }

    public class AdministrativeDocumentQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
        /// <summary><c>SK</c>, <c>SuratTugas</c>, <c>SuratKeterangan</c>, atau <c>DokumenPersonal</c>.</summary>
        public string? Source { get; set; }
        /// <summary>Jenis dokumen di dalam sumber (kategori SK, kode jenis surat, jenis dokumen personal, kategori tugas).</summary>
        public string? DocumentType { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        /// <summary>Unit penempatan pemilik dokumen (penempatan aktif hari ini).</summary>
        public Guid? OrganizationUnitId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        /// <summary><c>BERLAKU</c>, <c>TERJADWAL</c>, <c>TIDAK_BERLAKU</c>, atau <c>KEDALUWARSA</c>.</summary>
        public string? Validity { get; set; }
        public bool? IsConfidential { get; set; }
    }

    public class AdministrativeDocumentStatusHistoryEntry
    {
        public string Status { get; set; } = string.Empty;
        public DateTime? At { get; set; }
        public string? Note { get; set; }
    }

    public class AdministrativeDocumentRegistryItem
    {
        public string Source { get; set; } = string.Empty;
        public string SourceLabel { get; set; } = string.Empty;
        public Guid Id { get; set; }
        public string? DocumentNumber { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public Guid? OwnerWorkforceProfileId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string? OwnerCode { get; set; }
        public DateTime? DocumentDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public int? Version { get; set; }
        public string SourceStatus { get; set; } = string.Empty;
        public string Validity { get; set; } = string.Empty;
        public string ValidityLabel { get; set; } = string.Empty;
        public bool IsConfidential { get; set; }
        /// <summary><c>Rahasia</c> atau <c>Umum</c>.</summary>
        public string AccessLevel { get; set; } = string.Empty;
        /// <summary>Endpoint terotorisasi untuk melihat/mengunduh dokumen (bukan URL berkas statis).</summary>
        public string? AccessPath { get; set; }
        public List<AdministrativeDocumentStatusHistoryEntry> StatusHistory { get; set; } = new();
    }
}
