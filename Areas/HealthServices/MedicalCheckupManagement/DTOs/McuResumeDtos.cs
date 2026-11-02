using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs
{
    // =====================================================================
    // MCU-API-v1 — grup Mcu Resume (BE-MCU-030, BE-MCU-031)
    // =====================================================================

    /// <summary>Draf resume berlaku. Ruas kosong boleh selama draf; approve menuntut isi minimum (<c>VAL-MCU-37</c>).</summary>
    public class SaveMcuResumeRequest
    {
        public string? GeneralCondition { get; set; }
        public string? ImportantFindings { get; set; }
        public string? Recommendation { get; set; }
        public Guid? FitnessCategoryId { get; set; }

        /// <summary>Satu-satunya teks yang boleh sampai ke perusahaan tanpa persetujuan pelepasan.</summary>
        public string? WorkRecommendation { get; set; }

        /// <summary><c>Version</c> order yang dibaca dokter (<c>VAL-MCU-46</c>).</summary>
        public int Version { get; set; }
    }

    /// <summary>Approve dan verifikasi — tanpa isi; pemverifikasi tidak dapat mengubah isi klinis (<c>MCU-DEC-008</c>).</summary>
    public class McuResumeActionRequest
    {
        /// <summary><c>Version</c> order yang dibaca pengguna (<c>VAL-MCU-46</c>).</summary>
        public int Version { get; set; }
    }

    public class ReturnMcuResumeRequest
    {
        /// <summary>Wajib (<c>VAL-MCU-40</c>), contoh <i>Kategori kelayakan tidak sesuai hasil Thorax</i>.</summary>
        public string Reason { get; set; } = string.Empty;

        public int Version { get; set; }
    }

    public class ReopenMcuResumeRequest
    {
        /// <summary>Wajib (<c>VAL-MCU-40</c>), contoh <i>Salah baca nilai tekanan darah</i>.</summary>
        public string Reason { get; set; } = string.Empty;

        public int Version { get; set; }
    }

    public class McuResumeResponse
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;

        /// <summary><c>Version</c> order sesudah tindakan — dikirim balik pada tindakan berikutnya.</summary>
        public int OrderVersion { get; set; }

        public int VersionNumber { get; set; }
        public McuResumeStatus ResumeStatus { get; set; }
        public string ResumeStatusLabel { get; set; } = string.Empty;
        public bool IsCurrent { get; set; }
        public string? GeneralCondition { get; set; }
        public string? ImportantFindings { get; set; }
        public string? Recommendation { get; set; }
        public Guid? FitnessCategoryId { get; set; }
        public string? FitnessCategoryName { get; set; }
        public string? WorkRecommendation { get; set; }
        public string? AuthorDoctorName { get; set; }

        /// <summary>Persetujuan terakhir; tetap terisi bila resume dikembalikan ke dokter.</summary>
        public string? ApprovedByDoctorName { get; set; }

        public DateTime? ApprovedAt { get; set; }
        public string? ReturnReason { get; set; }
        public string? VerifiedByName { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? ReopenReason { get; set; }
        public string? ReopenedByDoctorName { get; set; }
        public DateTime? ReopenedAt { get; set; }
    }
}
