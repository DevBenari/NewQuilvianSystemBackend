using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs
{
    // =====================================================================
    // MCU-API-v1 — grup Mcu Delivery (BE-MCU-033: data cetak hasil; BE-MCU-034: penyerahan dan persetujuan)
    // =====================================================================

    /// <summary>Catat satu penyerahan manual (<c>MCU-DEC-027</c>) — tanpa tombol Kirim (<c>MCU-COORD-005</c>).</summary>
    public class CreateMcuResultDeliveryRequest
    {
        public McuDeliveryRecipient RecipientType { get; set; }
        public McuDeliveryContent ContentType { get; set; }
        public McuDeliveryChannel Channel { get; set; }

        /// <summary>Wajib untuk WhatsApp, email, dan telepon (<c>VAL-MCU-33</c>). SENSITIF — tidak masuk log.</summary>
        public string? DestinationText { get; set; }

        /// <summary>Kosong = sekarang. Pencatatan manual boleh menyebut waktu sebelumnya, tidak di masa depan.</summary>
        public DateTime? DeliveredAt { get; set; }

        public string? Note { get; set; }

        /// <summary><c>Version</c> order (<c>VAL-MCU-46</c>).</summary>
        public int Version { get; set; }
    }

    public class RecordCorrectionNoticeRequest
    {
        /// <summary>Kosong = sekarang.</summary>
        public DateTime? NotifiedAt { get; set; }

        public int Version { get; set; }
    }

    public class McuResultDeliveryResponse
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public int OrderVersion { get; set; }
        public int ResumeVersionNumber { get; set; }
        public McuDeliveryRecipient RecipientType { get; set; }
        public string RecipientTypeLabel { get; set; } = string.Empty;
        public McuDeliveryContent ContentType { get; set; }
        public string ContentTypeLabel { get; set; } = string.Empty;
        public McuDeliveryChannel Channel { get; set; }
        public string ChannelLabel { get; set; } = string.Empty;
        public string? DestinationText { get; set; }
        public DateTime DeliveredAt { get; set; }
        public string? DeliveredByName { get; set; }
        public string? Note { get; set; }
        public bool RequiresCorrectionNotice { get; set; }
        public DateTime? CorrectionNoticeRecordedAt { get; set; }
    }

    /// <summary>Persetujuan peserta melepas hasil lengkap kepada perusahaan penjamin order (<c>MCU-DEC-010</c>).</summary>
    public class CreateMcuReleaseConsentRequest
    {
        /// <summary>Bentuk final menunggu <c>MCU-OQ-003</c>.</summary>
        public McuConsentEvidenceType EvidenceType { get; set; }

        /// <summary>Nomor formulir — wajib untuk formulir kertas.</summary>
        public string? EvidenceReference { get; set; }

        /// <summary>Kosong = sekarang; tidak di masa depan.</summary>
        public DateTime? ConsentGivenAt { get; set; }

        public int Version { get; set; }
    }

    public class RevokeMcuReleaseConsentRequest
    {
        /// <summary>Wajib (<c>VAL-MCU-40</c>).</summary>
        public string Reason { get; set; } = string.Empty;

        public int Version { get; set; }
    }

    public class McuReleaseConsentResponse
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public int OrderVersion { get; set; }
        public string? CompanyGuarantorName { get; set; }
        public DateTime ConsentGivenAt { get; set; }
        public string? RecordedByName { get; set; }
        public McuConsentEvidenceType EvidenceType { get; set; }
        public string EvidenceTypeLabel { get; set; } = string.Empty;
        public string? EvidenceReference { get; set; }
        public bool IsActive { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? RevokedByName { get; set; }
        public string? RevokeReason { get; set; }
    }

    /// <summary>
    /// Data cetak hasil <b>lengkap</b> — untuk peserta, atau perusahaan yang memegang persetujuan pelepasan
    /// (<c>MCU-DEC-010</c>). SENSITIF seluruhnya.
    /// </summary>
    public class McuResultDocumentResponse
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateOnly ExaminationDate { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public string ParticipantName { get; set; } = string.Empty;
        public string? MedicalRecordNumber { get; set; }
        public DateOnly? ParticipantBirthDate { get; set; }
        public string? CompanyGuarantorName { get; set; }

        public int ResumeVersionNumber { get; set; }
        public McuResumeStatus ResumeStatus { get; set; }
        public string ResumeStatusLabel { get; set; } = string.Empty;

        /// <summary>Hanya versi terverifikasi yang boleh diserahkan (<c>VAL-MCU-32</c>, <c>BE-MCU-034</c>).</summary>
        public bool IsVerified { get; set; }

        public string? ApprovedByDoctorName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? VerifiedByName { get; set; }
        public DateTime? VerifiedAt { get; set; }

        public string? FitnessCategoryName { get; set; }
        public string? GeneralCondition { get; set; }
        public string? ImportantFindings { get; set; }
        public string? Recommendation { get; set; }
        public string? WorkRecommendation { get; set; }

        public List<McuResultDocumentComponentResponse> Components { get; set; } = new();
    }

    public class McuResultDocumentComponentResponse
    {
        public string ComponentName { get; set; } = string.Empty;
        public McuComponentType ComponentType { get; set; }
        public string ComponentTypeLabel { get; set; } = string.Empty;
        public McuOrderComponentStatus ComponentStatus { get; set; }
        public string ComponentStatusLabel { get; set; } = string.Empty;
        public string? CancelReason { get; set; }

        /// <summary>Lab: pemeriksaan yang sudah dirilis. Pemeriksaan MCU: nilai per isian.</summary>
        public List<McuResultDocumentLineResponse> Lines { get; set; } = new();

        /// <summary>Pemeriksaan MCU: kesimpulan pemeriksa.</summary>
        public string? Conclusion { get; set; }

        /// <summary>Radiologi: versi laporan yang dirilis.</summary>
        public string? RadReportNumber { get; set; }
        public string? RadFindings { get; set; }
        public string? RadImpression { get; set; }
        public string? RadRecommendation { get; set; }
        public DateTime? ReleasedAt { get; set; }
    }

    public class McuResultDocumentLineResponse
    {
        public string Name { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? Unit { get; set; }
        public string? ReferenceText { get; set; }
        public bool IsOutOfReference { get; set; }
    }

    /// <summary>
    /// Ringkasan kelayakan untuk perusahaan penjamin (<c>MCU-DEC-010</c>, <c>MCU-FE-003</c>): kategori kelayakan dan
    /// rekomendasi kerja <b>saja</b>, disusun server.
    /// </summary>
    /// <remarks>
    /// <b>Jangan menambah ruas nilai, temuan, kondisi umum, rekomendasi medis, kesimpulan, atau komponen di sini</b> —
    /// uji refleksi <c>AT-MCU-42</c> mengunci daftar ruasnya. Tanpa nomor rekam medis dan tanggal lahir.
    /// </remarks>
    public class McuFitnessSummaryDocumentResponse
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateOnly ExaminationDate { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public string ParticipantName { get; set; } = string.Empty;
        public string? CompanyGuarantorName { get; set; }
        public int ResumeVersionNumber { get; set; }
        public bool IsVerified { get; set; }
        public string? ApprovedByDoctorName { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? FitnessCategoryName { get; set; }
        public string? WorkRecommendation { get; set; }
    }
}
