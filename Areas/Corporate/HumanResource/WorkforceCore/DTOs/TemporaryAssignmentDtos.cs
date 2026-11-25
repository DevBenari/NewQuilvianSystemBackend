namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    /// <summary>
    /// Saringan daftar penempatan sementara lintas pegawai (HRD-DEC-240, kontrak v30 API 36.6).
    /// </summary>
    public class TemporaryAssignmentPagedQuery
    {
        /// <summary>Unit tujuan atau unit asal. Unit di luar cakupan baca ditolak 403 (VR-PK-27).</summary>
        public Guid? OrganizationUnitId { get; set; }

        /// <summary>Awal rentang tanggal; penempatan yang bersinggungan dengan rentang ikut tampil.</summary>
        public DateOnly? FromDate { get; set; }

        /// <summary>Akhir rentang tanggal; penempatan yang bersinggungan dengan rentang ikut tampil.</summary>
        public DateOnly? ToDate { get; set; }

        /// <summary>Upcoming, Ongoing, atau Ended (tanggal WIB hari ini). Kosong = semua.</summary>
        public string? TimeStatus { get; set; }

        /// <summary>Nama atau kode pegawai, atau nomor surat penempatan.</summary>
        public string? Search { get; set; }

        public int PageNumber { get; set; } = 1;

        /// <summary>Paling banyak 100.</summary>
        public int PageSize { get; set; } = 25;
    }

    public class TemporaryAssignmentFilterMetadataResponse
    {
        public List<TemporaryAssignmentOptionResponse> TimeStatusOptions { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class TemporaryAssignmentOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class TemporaryAssignmentResponse
    {
        /// <summary>Id penempatan (WfpOrganizationAssignment).</summary>
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;

        /// <summary>
        /// Unit penempatan utama pegawai pada tanggal mulai penempatan sementara; null bila tidak ada penempatan utama yang berlaku.
        /// </summary>
        public string? OriginUnitName { get; set; }

        public Guid? DestinationUnitId { get; set; }
        public string? DestinationUnitName { get; set; }

        public DateOnly EffectiveStartDate { get; set; }
        public DateOnly? EffectiveEndDate { get; set; }

        /// <summary>Upcoming (belum mulai), Ongoing (sedang berjalan), atau Ended (sudah selesai), menurut tanggal WIB hari ini.</summary>
        public string TimeStatus { get; set; } = string.Empty;
        public string TimeStatusLabel { get; set; } = string.Empty;

        public string? AssignmentNumber { get; set; }
        public string? Description { get; set; }
    }

    public sealed class TemporaryAssignmentQueryResult<T>
    {
        public bool Success { get; private init; }
        public int StatusCode { get; private init; }
        public string Message { get; private init; } = string.Empty;
        public T? Data { get; private init; }

        public static TemporaryAssignmentQueryResult<T> Ok(T data, string message) =>
            new() { Success = true, StatusCode = 200, Message = message, Data = data };

        public static TemporaryAssignmentQueryResult<T> Fail(int statusCode, string message) =>
            new() { Success = false, StatusCode = statusCode, Message = message };
    }
}
