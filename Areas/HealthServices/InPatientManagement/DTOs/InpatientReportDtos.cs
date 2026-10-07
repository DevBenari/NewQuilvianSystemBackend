namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>
    /// Saringan laporan transfer ruangan (<c>BE-RWI-184</c>, API 11.7). Periode wajib dan paling
    /// lama 31 hari per permintaan (<c>VAL-RWF-90</c>).
    /// </summary>
    public class RoomTransferReportQuery
    {
        /// <summary>Wajib. Awal periode (tanggal saja berarti mulai pukul 00.00).</summary>
        public DateTime? PeriodFrom { get; set; }

        /// <summary>Wajib. Akhir periode (tanggal saja berarti sampai akhir hari itu).</summary>
        public DateTime? PeriodTo { get; set; }

        public Guid? FromServiceUnitId { get; set; }

        public Guid? ToServiceUnitId { get; set; }

        /// <summary>Kelas asal atau kelas tujuan.</summary>
        public Guid? ClassId { get; set; }

        /// <summary>Bawaan <c>true</c>: koreksi salah catat ikut tampil, dibedakan lewat <c>EntryKind</c>.</summary>
        public bool IncludeCorrections { get; set; } = true;

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 50;
    }

    /// <summary>Satu baris laporan transfer ruangan (API 11.7).</summary>
    public class RoomTransferReportRow
    {
        public Guid PlacementId { get; set; }
        public DateTime TransferredAt { get; set; }
        public string? MedicalRecordNumber { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? FromClassName { get; set; }
        public string? FromRoomName { get; set; }
        public string? FromBedNumber { get; set; }
        public string? ToClassName { get; set; }
        public string? ToRoomName { get; set; }
        public string? ToBedNumber { get; set; }
        public string? Reason { get; set; }
        public string? RecordedByName { get; set; }

        /// <summary><c>Transfer</c> atau <c>Correction</c> (<c>INV-RWF-34</c>).</summary>
        public string EntryKind { get; set; } = string.Empty;
    }

    /// <summary>Nilai baku <see cref="RoomTransferReportRow.EntryKind"/>.</summary>
    public static class RoomTransferEntryKinds
    {
        public const string Transfer = "Transfer";
        public const string Correction = "Correction";
    }
}
