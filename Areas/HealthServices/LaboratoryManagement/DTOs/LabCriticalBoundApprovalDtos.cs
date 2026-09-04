using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Pengajuan perubahan batas kritis.
    ///
    /// Nilai usulan boleh sebagian: pengaju yang hanya hendak menaikkan batas kritis atas cukup
    /// mengisi ruas itu saja. Ruas yang dikosongkan berarti "biarkan seperti yang berlaku",
    /// bukan "kosongkan nilainya".
    /// </summary>
    public class SubmitCriticalBoundChangeRequest
    {
        public decimal? ProposedCriticalLow { get; set; }

        public decimal? ProposedCriticalHigh { get; set; }

        /// <summary>
        /// Usulan daftar pilihan yang dianggap kritis, ditulis sebagai kode yang dipisah koma —
        /// misalnya <c>P3,P4</c>. Hanya bermakna untuk batas berbentuk pilihan. Kosong berarti
        /// daftar pilihan kritis tidak diusulkan berubah; untuk mengusulkan tidak ada satu pun
        /// pilihan kritis, kirim tanda hubung tunggal <c>-</c>.
        /// </summary>
        [MaxLength(500)]
        public string? ProposedCriticalOptionCodes { get; set; }

        /// <summary>
        /// Alasan pengajuan. Wajib (<c>VAL-31</c>).
        ///
        /// Sengaja <b>tanpa</b> <c>[Required]</c>. Dengan atribut itu, ASP.NET Core menolaknya
        /// lebih dulu lewat validasi model dan menerbitkan <c>400</c> berisi pesan bawaan
        /// framework — padahal <c>LAB-VAL-v1</c> r3 menetapkan <c>422</c> beserta kalimat
        /// "Jelaskan alasan perubahan batas kritis ini." Pemeriksaannya karena itu dilakukan di
        /// service, supaya kode status dan pesannya benar-benar sesuai kontrak.
        /// </summary>
        [MaxLength(1000)]
        public string RequestReason { get; set; } = string.Empty;
    }

    /// <summary>Keputusan atas sebuah pengajuan: menyetujui atau menolak.</summary>
    public class DecideCriticalBoundChangeRequest
    {
        [MaxLength(1000)]
        public string? DecisionNote { get; set; }
    }

    public class LabBoundChangeRequestResponse
    {
        public Guid Id { get; set; }

        public Guid ValueBoundId { get; set; }

        public string ProcedureName { get; set; } = string.Empty;

        /// <summary>Status pengajuan: <c>Submitted</c>, <c>Approved</c>, <c>Rejected</c>, atau <c>Withdrawn</c>.</summary>
        public string RequestStatus { get; set; } = string.Empty;

        /// <summary>Batas kritis bawah yang berlaku saat ini, untuk dibandingkan dengan usulannya.</summary>
        public decimal? CurrentCriticalLow { get; set; }

        public decimal? CurrentCriticalHigh { get; set; }

        public decimal? ProposedCriticalLow { get; set; }

        public decimal? ProposedCriticalHigh { get; set; }

        public string? ProposedCriticalOptionCodes { get; set; }

        public string RequestReason { get; set; } = string.Empty;

        public Guid RequestedByUserId { get; set; }

        public DateTime RequestedAt { get; set; }

        public Guid? DecidedByUserId { get; set; }

        public DateTime? DecidedAt { get; set; }

        public string? DecisionNote { get; set; }
    }
}
