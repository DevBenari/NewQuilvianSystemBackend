using System;
using System.Collections.Generic;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs
{
    /// <summary>
    /// Parameter pencarian perhitungan gap tenaga kerja langsung pada tanggal acuan (BE-HRD-037).
    /// </summary>
    public sealed class StaffingGapQuery
    {
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public DateOnly? AsOfDate { get; set; }
    }

    /// <summary>
    /// Informasi penempatan bukan utama (Plt / rangkap) yang bertugas pada unit dan jabatan terkait.
    /// Tidak menambah hitungan terisi formasi utama (kriteria 1).
    /// </summary>
    public sealed class SecondaryAssignmentInfo
    {
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? AssignmentType { get; set; }
        public DateOnly EffectiveStartDate { get; set; }
    }

    /// <summary>
    /// Rincian satu baris kesenjangan tenaga kerja untuk kombinasi unit organisasi dan jabatan (BE-HRD-037).
    /// </summary>
    public sealed class StaffingGapLineResponse
    {
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitCode { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid PositionId { get; set; }
        public string? PositionCode { get; set; }
        public string? PositionName { get; set; }

        public string? StandardNumber { get; set; }
        public int? StandardVersionNumber { get; set; }

        /// <summary>
        /// Angka formasi yang disahkan dan aktif berlaku pada tanggal acuan.
        /// </summary>
        public decimal Formation { get; set; }

        /// <summary>
        /// Jumlah pegawai penempatan utama aktif dalam porsi kerja (FTE).
        /// </summary>
        public decimal Filled { get; set; }

        /// <summary>
        /// Jumlah pegawai yang akan kosong (misal: pengunduran diri disetujui, SK mutasi terjadwal).
        /// </summary>
        public decimal WillBeVacant { get; set; }

        /// <summary>
        /// Pegawai tidak tersedia sementara (misal: cuti panjang).
        /// Bernilai null bila ambang batas LongLeaveThresholdDays belum diatur (HRD-DEC-061).
        /// </summary>
        public decimal? TemporaryUnavailable { get; set; }

        /// <summary>
        /// Pegawai belum kompeten terhadap skill-mix standar.
        /// Bernilai null bila standar berlaku tidak mendefinisikan skill-mix wajib.
        /// </summary>
        public decimal? NotCompetent { get; set; }

        /// <summary>
        /// Jumlah tenaga alih daya (outsourced).
        /// Bernilai null sampai master jenis tenaga memiliki kategori alih daya.
        /// </summary>
        public decimal? Outsourced { get; set; }

        /// <summary>
        /// Gap Permanen = Formasi - (Terisi - Akan Kosong) [+ Alih Daya bila CountsOutsourced = true].
        /// </summary>
        public decimal PermanentGap { get; set; }

        /// <summary>
        /// Gap Sementara = Tidak Tersedia Sementara.
        /// </summary>
        public decimal? TemporaryGap { get; set; }

        /// <summary>
        /// Gap Kompetensi = Belum Kompeten.
        /// </summary>
        public decimal? CompetencyGap { get; set; }

        /// <summary>
        /// Bernilai false bila terdapat satu atau lebih lapisan yang bernilai null (kriteria 2).
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Catatan informasi penempatan tambahan (Plt/rangkap).
        /// </summary>
        public List<SecondaryAssignmentInfo> SecondaryAssignments { get; set; } = new();
    }

    /// <summary>
    /// Respons kalkulasi gap tenaga kerja langsung per tanggal acuan (BE-HRD-037).
    /// </summary>
    public sealed class StaffingGapResponse
    {
        public DateOnly AsOfDate { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public List<StaffingGapLineResponse> Lines { get; set; } = new();
    }
}
