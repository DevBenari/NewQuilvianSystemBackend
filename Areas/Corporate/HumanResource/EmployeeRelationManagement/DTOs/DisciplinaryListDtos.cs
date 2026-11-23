namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs
{
    /// <summary>Filter daftar sanksi/skorsing seluruh pegawai (MSL-14, DEC-FER-05 opsi A, 18 Nov 2026).</summary>
    public class DisciplinaryListQuery
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        /// <summary>
        /// Beberapa status sekaligus, hanya untuk daftar sanksi (kontrak v29 bagian 35.2, Riwayat Penyelesaian):
        /// <c>?statuses=Expired&amp;statuses=RevokedOnAppeal</c>. Nilai sah <c>Active</c>, <c>Expired</c>,
        /// <c>RevokedOnAppeal</c>; tidak boleh dikirim bersama <see cref="Status"/>.
        /// </summary>
        public List<string>? Statuses { get; set; }
        /// <summary>Tingkat sanksi, misalnya SP1, SP2, SP3 (hanya untuk daftar sanksi).</summary>
        public string? Level { get; set; }
        /// <summary>Sanksi: masih berlaku hari ini. Skorsing: belum diakhiri.</summary>
        public bool? ActiveOnly { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class SanctionListItemDto
    {
        public Guid SanctionId { get; set; }
        public string SanctionNumber { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public string SanctionLevel { get; set; } = string.Empty;
        public DateTime IssuedDate { get; set; }
        public DateTime ValidUntil { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsAppealed { get; set; }
        public DateTime? AppealedAt { get; set; }
    }

    public class SuspensionListItemDto
    {
        public Guid SuspensionId { get; set; }
        public string SuspensionNumber { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpectedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public string WagePaymentStatus { get; set; } = string.Empty;
        public bool IsSystemAccessRevoked { get; set; }
        public bool IsPhysicalAccessRevoked { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
