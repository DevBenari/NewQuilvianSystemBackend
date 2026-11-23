using System;
using System.Collections.Generic;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    /// <summary>
    /// Lima panel Dashboard SDM (kontrak v29 bagian 35.1). Dihitung saat dipanggil, tanpa snapshot (<c>HRD-ASM-89</c>).
    /// Tidak memuat gaji, nomor identitas, data disiplin, maupun data kesehatan.
    /// </summary>
    public class WorkforceDashboardSummaryResponse
    {
        /// <summary>Tanggal operasional rumah sakit (WIB) saat angka dihitung.</summary>
        public DateTime OperationalDate { get; set; }

        /// <summary>Waktu perhitungan (UTC).</summary>
        public DateTime GeneratedAt { get; set; }

        public WorkforceDashboardEmployeeOverviewDto EmployeeOverview { get; set; } = new();
        public List<WorkforceDashboardProfessionItemDto> ProfessionDistribution { get; set; } = new();
        public WorkforceDashboardActiveStatusDto ActiveStatus { get; set; } = new();
        public WorkforceDashboardContractExpiryDto ContractExpiry { get; set; } = new();
        public WorkforceDashboardDocumentExpiryDto DocumentExpiry { get; set; } = new();
    }

    public class WorkforceDashboardEmployeeOverviewDto
    {
        public int ActiveCount { get; set; }
        public int NewThisMonthCount { get; set; }
        public int LeftThisMonthCount { get; set; }
    }

    public class WorkforceDashboardProfessionItemDto
    {
        /// <summary><c>null</c> = pegawai yang profesinya belum diisi.</summary>
        public Guid? ProfessionId { get; set; }
        public string ProfessionName { get; set; } = string.Empty;
        public int ActiveCount { get; set; }
    }

    public class WorkforceDashboardActiveStatusDto
    {
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
    }

    /// <summary>Panel Kontrak Berakhir — sama persis dengan layar Kontrak (<c>HRD-DEC-237</c>).</summary>
    public class WorkforceDashboardContractExpiryDto
    {
        /// <summary>Sama dengan <c>ExpiringSoonCount</c> pada <c>GET employment-contracts/summary</c>.</summary>
        public int TotalWithin60Days { get; set; }
        public int H60Count { get; set; }
        public int H14Count { get; set; }
        public int H7Count { get; set; }
        public int PendingDecisionCount { get; set; }
        public int OverdueCount { get; set; }
    }

    public class WorkforceDashboardDocumentExpiryDto
    {
        public int ExpiredCount { get; set; }
        public int Within30Count { get; set; }
        public int Days31To60Count { get; set; }
        public int Days61To90Count { get; set; }
        public List<WorkforceDashboardDocumentTypeExpiryDto> ByType { get; set; } = new();
    }

    public class WorkforceDashboardDocumentTypeExpiryDto
    {
        /// <summary><c>SIP_STR</c>, <c>SPK_RKK</c>, atau <c>MANDATORY_CERTIFICATE</c>.</summary>
        public string DocumentType { get; set; } = string.Empty;
        public int ExpiredCount { get; set; }
        public int Within30Count { get; set; }
        public int Days31To60Count { get; set; }
        public int Days61To90Count { get; set; }
    }

    /// <summary>Parameter daftar nama di balik satu angka panel (kontrak v29 bagian 35.1).</summary>
    public class WorkforceDashboardDrilldownQuery
    {
        public string? Panel { get; set; }
        public string? Bucket { get; set; }

        /// <summary>Hanya untuk panel <c>PROFESSION</c>; kosong berarti profesi belum diisi.</summary>
        public Guid? ProfessionId { get; set; }

        /// <summary>Hanya untuk panel <c>DOCUMENT_EXPIRY</c>; kosong = semua jenis.</summary>
        public string? DocumentType { get; set; }

        /// <summary>Nama atau nomor pegawai, maksimal 100 karakter.</summary>
        public string? Search { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>Satu baris daftar nama Dashboard SDM. Tidak memuat gaji, NIK, disiplin, atau data kesehatan.</summary>
    public class WorkforceDashboardDrilldownItem
    {
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string? ProfessionName { get; set; }

        /// <summary>Keterangan baris, mis. "SIP …0123", "PKWT berakhir", "BTCLS". Nomor lisensi hanya 4 karakter terakhir.</summary>
        public string DetailLabel { get; set; } = string.Empty;

        public DateTime? ReferenceDate { get; set; }

        /// <summary>Sisa hari terhadap tanggal acuan (negatif bila sudah lewat); kosong untuk panel 1–3.</summary>
        public int? DaysRemaining { get; set; }
    }
}
