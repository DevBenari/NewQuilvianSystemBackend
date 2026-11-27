namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums
{
    /// <summary>
    /// Status satu versi dokumen hasil untuk pasien (<c>LAB-STATE-v1</c> <c>r13</c> bagian 15.2,
    /// <c>INV-72</c>). Versi lama hanya digantikan oleh versi berikutnya — tidak pernah kembali berlaku.
    /// </summary>
    public enum LabPatientResultVersionStatus
    {
        Current = 1,
        Superseded = 2
    }

    /// <summary>Keputusan penyetuju kedua atas satu versi bersensitif (<c>LAB-DEC-297</c>).</summary>
    public enum LabPatientResultApprovalDecision
    {
        Approved = 1,
        Rejected = 2
    }

    /// <summary>
    /// Saluran pengiriman. Hanya WhatsApp manual berbantu (<c>LAB-DEC-279</c>); kanal lain kelak
    /// menambah nilai, bukan kolom (<c>02-backend-architecture.md</c> 13.4).
    /// </summary>
    public enum LabPatientResultDeliveryChannel
    {
        WhatsAppManual = 1
    }

    /// <summary>
    /// Hasil satu upaya kirim yang dinyatakan petugas (<c>LAB-DEC-283</c>). <c>Pending</c> dan
    /// <c>Delivered</c> sengaja tidak ada — hanya hidup bersama gerbang WhatsApp otomatis kelak
    /// (<c>LAB-DEC-298</c>).
    /// </summary>
    public enum LabPatientResultDeliveryOutcome
    {
        Sent = 1,
        Failed = 2
    }

    /// <summary>Alasan pengiriman gagal (<c>LAB-DEC-283</c>); <c>Other</c> wajib keterangan.</summary>
    public enum LabPatientResultDeliveryFailureReason
    {
        NotRegisteredOnWhatsApp = 1,
        PatientRefused = 2,
        Other = 3
    }
}
