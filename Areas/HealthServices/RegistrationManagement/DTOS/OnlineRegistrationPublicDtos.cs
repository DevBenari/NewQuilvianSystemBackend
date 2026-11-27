using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    /// <summary>Poli yang punya jadwal online pada tanggal kunjungan (<c>KSK-CONTRACT-v2</c>, grup publik).</summary>
    public class OnlineRegistrationClinicOption
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Jadwal dokter untuk halaman publik. Hanya field minimum: tanpa ruangan, kode jadwal, maupun
    /// angka kuota mentah (<c>CAP-PO-09</c>).
    /// </summary>
    public class OnlineRegistrationScheduleOption
    {
        public Guid Id { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        /// <summary>Format <c>HH:mm</c>.</summary>
        public string StartTime { get; set; } = string.Empty;

        /// <summary>Format <c>HH:mm</c>.</summary>
        public string EndTime { get; set; } = string.Empty;

        public string? SessionName { get; set; }

        /// <summary>Sisa kuota appointment; <c>null</c> bila jadwal tidak membatasi.</summary>
        public int? RemainingAppointmentQuota { get; set; }

        public bool IsFull { get; set; }
    }

    public class OnlineRegistrationInsuranceProviderOption
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    public class OnlineRegistrationReferralInstitutionOption
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }

    /// <summary>Pencocokan pasien lama: NIK atau No. RM ditambah tanggal lahir (<c>KSK-DEC-031</c>).</summary>
    public class OnlineRegistrationPatientMatchRequest
    {
        public OnlineRegistrationIdentityType? IdentityType { get; set; }

        public string? IdentityValue { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? TurnstileToken { get; set; }
    }

    /// <summary>Hasil cocok: hanya nama tersamar dan kartu penjamin tersamar (<c>KSK-PO-FE-002</c>).</summary>
    public class OnlineRegistrationPatientMatchResponse
    {
        public string MaskedName { get; set; } = string.Empty;

        public List<OnlineRegistrationPayerOption> Payers { get; set; } = new();
    }

    public class OnlineRegistrationPayerOption
    {
        /// <summary>Id kartu asuransi atau penjamin perusahaan milik pasien.</summary>
        public Guid Id { get; set; }

        public OnlineRegistrationPayerKind PayerKind { get; set; }

        public string ProviderName { get; set; } = string.Empty;

        /// <summary>Empat karakter terakhir saja, contoh <c>****1234</c>.</summary>
        public string MaskedNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// Bagian JSON (<c>payload</c>) dari pengiriman pendaftaran online <c>multipart/form-data</c>
    /// (<c>BE-KSK-PO-005</c>, <c>KSK-CONTRACT-v2</c>). Berkas surat rujukan dikirim terpisah sebagai
    /// <c>referralDocuments</c>. Enum dikirim sebagai angka.
    /// </summary>
    /// <remarks>
    /// Tanpa sesi (<c>KSK-DSN-PO-005</c>): pasien lama mengirim ulang identitas dan tanggal lahir di
    /// <see cref="ExistingPatient"/>, lalu server mencocokkan ulang. Pasien baru mengisi
    /// <see cref="NewPatient"/> yang hanya disimpan bersama pendaftaran (<c>INV-PO-06</c>).
    /// </remarks>
    public class OnlineRegistrationSubmitRequest
    {
        public OnlineRegistrationPatientMode? PatientMode { get; set; }

        /// <summary>Wajib bila <see cref="PatientMode"/> = 1 (pasien lama).</summary>
        public OnlineRegistrationExistingPatientInput? ExistingPatient { get; set; }

        /// <summary>Wajib bila <see cref="PatientMode"/> = 2 (pasien baru).</summary>
        public OnlineRegistrationNewPatientInput? NewPatient { get; set; }

        /// <summary>No. WhatsApp lokal <c>08…</c> atau alamat email (<c>KSK-DEC-043</c>).</summary>
        public string? Contact { get; set; }

        public OnlineRegistrationPatientType? PatientType { get; set; }

        public EncounterPaymentType? PaymentType { get; set; }

        /// <summary>Kartu asuransi milik pasien lama (dari <c>payers[]</c> pencocokan).</summary>
        public Guid? PatientInsuranceId { get; set; }

        /// <summary>Penjamin perusahaan milik pasien lama (dari <c>payers[]</c> pencocokan).</summary>
        public Guid? PatientCompanyGuarantorId { get; set; }

        /// <summary>Kartu asuransi baru; tidak boleh bersamaan dengan <see cref="PatientInsuranceId"/>.</summary>
        public OnlineRegistrationNewInsuranceInput? NewInsurance { get; set; }

        /// <summary>Kalender WIB, <c>yyyy-MM-dd</c>.</summary>
        public DateTime? VisitDate { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? DoctorScheduleId { get; set; }

        /// <summary>Wajib bila <see cref="PatientType"/> = 3 (Rujukan); selain itu harus kosong.</summary>
        public OnlineRegistrationReferralInput? Referral { get; set; }

        /// <summary>Wajib bila pasien baru; selain itu harus kosong (<c>KSK-DEC-034</c>).</summary>
        public OnlineRegistrationConsentInput? Consent { get; set; }

        public string? TurnstileToken { get; set; }
    }

    /// <summary>Identitas pasien lama yang dicocokkan ulang saat kirim (<c>KSK-DSN-PO-005</c>).</summary>
    public class OnlineRegistrationExistingPatientInput
    {
        public OnlineRegistrationIdentityType? IdentityType { get; set; }

        public string? IdentityValue { get; set; }

        public DateTime? BirthDate { get; set; }
    }

    /// <summary>Data diri pasien baru, hanya empat field (<c>KSK-DSN-PO-004</c>).</summary>
    public class OnlineRegistrationNewPatientInput
    {
        public string? IdentityNumber { get; set; }

        public string? FullName { get; set; }

        public DateTime? BirthDate { get; set; }

        public Gender? Gender { get; set; }
    }

    /// <summary>Kartu asuransi baru (<c>KSK-DSN-PO-012</c>).</summary>
    public class OnlineRegistrationNewInsuranceInput
    {
        public Guid? InsuranceProviderId { get; set; }

        public string? PolicyNumber { get; set; }

        public string? HolderName { get; set; }

        public string? HolderRelationship { get; set; }
    }

    /// <summary>Rujukan eksternal saja (<c>KSK-DSN-PO-011</c>).</summary>
    public class OnlineRegistrationReferralInput
    {
        public Guid? ReferralInstitutionId { get; set; }

        public DateTime? ReferralDate { get; set; }

        public string? ReferralNumber { get; set; }

        public string? ReferralDoctorName { get; set; }

        public string? DiagnosisNote { get; set; }

        public string? Reason { get; set; }
    }

    /// <summary>
    /// General Consent pasien baru, isi dan aturannya sama dengan menu RJ
    /// (<c>OutpatientGeneralConsentService</c>): seluruh klausul wajib disetujui dan tanda tangan
    /// berupa data URL PNG.
    /// </summary>
    public class OnlineRegistrationConsentInput
    {
        public GeneralConsentSignerType? SignerType { get; set; }

        public string? SignerName { get; set; }

        public string? SignerRelationshipText { get; set; }

        public string? SignerIdentityNumber { get; set; }

        public string? SignerPhoneNumber { get; set; }

        public string? SignerAddress { get; set; }

        public List<int>? AgreedClauses { get; set; }

        /// <summary>Data URL <c>data:image/png;base64,…</c>.</summary>
        public string? SignatureImageBase64 { get; set; }
    }

    /// <summary>
    /// Hasil pendaftaran online. <see cref="Code"/> hanya muncul di respons ini dan tidak tersimpan
    /// dalam bentuk asli (<c>INV-PO-05</c>).
    /// </summary>
    public class OnlineRegistrationSubmitResponse
    {
        public string RegistrationNumber { get; set; } = string.Empty;

        /// <summary>Kode check-in, format <c>XXXX-XXXX</c>.</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>Kalender WIB, <c>yyyy-MM-dd</c>.</summary>
        public string VisitDate { get; set; } = string.Empty;

        public string ClinicName { get; set; } = string.Empty;

        public string DoctorName { get; set; } = string.Empty;

        /// <summary>Format <c>HH:mm</c>.</summary>
        public string StartTime { get; set; } = string.Empty;

        /// <summary>Format <c>HH:mm</c>.</summary>
        public string EndTime { get; set; } = string.Empty;

        public OnlineRegistrationEmailStatus EmailStatus { get; set; }

        /// <summary>No. WhatsApp yang diisi (bentuk <c>08…</c>), atau <c>null</c> bila kontaknya email.</summary>
        public string? WhatsAppNumber { get; set; }
    }
}
