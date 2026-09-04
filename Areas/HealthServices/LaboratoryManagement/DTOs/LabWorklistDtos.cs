namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Penyaring daftar kerja dan daftar pantau keterlambatan cito.
    ///
    /// Keduanya memakai bentuk penyaring yang sama supaya layar tidak perlu mempelajari dua
    /// aturan berbeda untuk dua daftar yang bersebelahan.
    /// </summary>
    public class LabWorklistPagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        /// <summary>Menyaring per disiplin pesanan. Kosong berarti seluruhnya.</summary>
        public string? Discipline { get; set; }

        /// <summary>
        /// Menampilkan hanya pekerjaan cito. Tidak berlaku pada daftar pantau keterlambatan,
        /// yang memang seluruhnya cito.
        /// </summary>
        public bool? OnlyCito { get; set; }

        /// <summary>Pencarian bebas pada kode dan nama pemeriksaan serta barcode wadah.</summary>
        public string? Search { get; set; }
    }

    /// <summary>
    /// Satu baris daftar kerja — satu <b>pemeriksaan</b> yang belum selesai, bukan satu pesanan.
    ///
    /// Satuannya pemeriksaan karena sejak <c>LAB-DEC-026</c> kesegeraan melekat di situ: satu
    /// pesanan dapat memuat Kalium cito dan Kolesterol biasa, dan hanya Kalium yang naik ke
    /// urutan atas (<c>AC-39</c>).
    /// </summary>
    public class LabWorklistItemResponse
    {
        public Guid ExaminationId { get; set; }

        public Guid LabOrderId { get; set; }

        public Guid SpecimenId { get; set; }

        public string? SpecimenBarcode { get; set; }

        public Guid EncounterId { get; set; }

        public Guid ProcedureId { get; set; }

        public string? ProcedureCode { get; set; }

        public string? ProcedureName { get; set; }

        public string? Discipline { get; set; }

        /// <summary><c>Routine</c> atau <c>Cito</c>.</summary>
        public string Urgency { get; set; } = string.Empty;

        public DateTime? UrgencyMarkedAt { get; set; }

        public bool IsDuplo { get; set; }

        public string ExaminationStatus { get; set; } = string.Empty;

        public string SpecimenStatus { get; set; } = string.Empty;

        /// <summary>Waktu pesanan masuk. Inilah dasar urutan di antara sesama tingkat kesegeraan.</summary>
        public DateTime? RequestedAt { get; set; }

        /// <summary>Waktu wadah dinyatakan layak. Kosong selama belum diputuskan.</summary>
        public DateTime? ChargeEligibleAt { get; set; }
    }

    /// <summary>
    /// Satu baris daftar pantau keterlambatan cito.
    ///
    /// Keterlambatan dihitung <b>sejak wadah dinyatakan layak</b> (<c>FR-04.3</c>), bukan sejak
    /// pesanan dibuat: sebelum bahannya dinyatakan layak, laboratorium belum punya apa-apa untuk
    /// dikerjakan dan tidak adil dihitung terlambat.
    /// </summary>
    public class LabCitoOverdueResponse
    {
        public Guid ExaminationId { get; set; }

        public Guid LabOrderId { get; set; }

        public Guid SpecimenId { get; set; }

        public string? SpecimenBarcode { get; set; }

        public Guid EncounterId { get; set; }

        public Guid ProcedureId { get; set; }

        public string? ProcedureCode { get; set; }

        public string? ProcedureName { get; set; }

        public string? Discipline { get; set; }

        public DateTime? RequestedAt { get; set; }

        /// <summary>Titik mulai perhitungan keterlambatan.</summary>
        public DateTime ChargeEligibleAt { get; set; }

        /// <summary>Batas waktu penyelesaian cito yang berlaku, dalam menit. Kosong bila belum diatur.</summary>
        public int? CitoTurnaroundMinutes { get; set; }

        /// <summary>Saat pekerjaan ini seharusnya sudah selesai. Kosong bila batas waktunya belum diatur.</summary>
        public DateTime? DeadlineAt { get; set; }

        /// <summary>Kelebihan waktu dalam menit. Kosong bila batas waktunya belum diatur.</summary>
        public int? OverdueMinutes { get; set; }

        /// <summary>
        /// Salah bila jenis pemeriksaan ini belum punya batas waktu cito (<c>VAL-39</c>). Baris
        /// seperti itu <b>tidak</b> dianggap terlambat, tetapi tetap ditampilkan agar kepala
        /// instalasi tahu ada data induk yang belum lengkap.
        /// </summary>
        public bool HasCitoTurnaround { get; set; }

        /// <summary>Keterangan bagi pengguna. Terisi pada baris <c>VAL-39</c>.</summary>
        public string? Note { get; set; }
    }
}
