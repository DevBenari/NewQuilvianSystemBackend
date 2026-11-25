using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    /// <summary>
    /// Verifikasi check-in pendaftaran online di kiosk (<c>BE-KSK-PO-006</c>, <c>KSK-CONTRACT-v2</c>).
    /// Mode 1 memakai <see cref="Code"/>; mode 2 (Lupa Kode) memakai <see cref="IdentityType"/> dan
    /// <see cref="IdentityValue"/>. Keduanya wajib disertai <see cref="BirthDate"/>.
    /// </summary>
    public class OnlineRegistrationCheckInVerifyRequest
    {
        public OnlineRegistrationCheckInMode? Mode { get; set; }

        /// <summary>Kode pendaftaran, boleh dengan atau tanpa tanda hubung (<c>K7QM-4TRZ</c>).</summary>
        public string? Code { get; set; }

        public OnlineRegistrationIdentityType? IdentityType { get; set; }

        public string? IdentityValue { get; set; }

        /// <summary>Kalender, <c>yyyy-MM-dd</c>.</summary>
        public DateTime? BirthDate { get; set; }
    }

    /// <summary>
    /// Paket check-in: semua yang dibutuhkan kiosk untuk menyusun permintaan kunjungan
    /// <c>POST patient-encounters/kiosk</c> beserta token sekali pakai. Tidak memuat kode pendaftaran.
    /// </summary>
    public class OnlineRegistrationCheckInPackage
    {
        public Guid OnlineRegistrationId { get; set; }

        public string RegistrationNumber { get; set; } = string.Empty;

        /// <summary>Token acak 256 bit (base64url). Dikirim kembali saat membuat kunjungan.</summary>
        public string CheckInToken { get; set; } = string.Empty;

        /// <summary>Batas berlaku token (UTC).</summary>
        public DateTime ExpiresAt { get; set; }

        public OnlineRegistrationPatientMode PatientMode { get; set; }

        /// <summary>Terisi untuk pasien lama saja.</summary>
        public Guid? PatientId { get; set; }

        public string MaskedName { get; set; } = string.Empty;

        /// <summary>
        /// NIK yang diisi online, hanya untuk pasien baru, sebagai pembanding hasil pindai KTP/KIA
        /// (<c>KSK-DEC-048</c>). Selalu <c>null</c> untuk pasien lama.
        /// </summary>
        public string? IdentityNumberForScan { get; set; }

        /// <summary>
        /// Kontak dari isian online, hanya untuk pasien baru, untuk master pasien yang dibuat kiosk
        /// (<c>KSK-DEC-048</c>: No. WA/email diambil dari isian online). Selalu <c>null</c> untuk pasien lama.
        /// </summary>
        public string? ContactWhatsAppNumber { get; set; }

        /// <inheritdoc cref="ContactWhatsAppNumber"/>
        public string? ContactEmail { get; set; }

        public OnlineRegistrationCheckInVisit Visit { get; set; } = new();

        public OnlineRegistrationCheckInPayer Payer { get; set; } = new();

        /// <summary><c>null</c> bila bukan Pasien Rujukan.</summary>
        public OnlineRegistrationCheckInReferral? Referral { get; set; }

        public bool HasConsent { get; set; }
    }

    public class OnlineRegistrationCheckInVisit
    {
        /// <summary>Kalender WIB, <c>yyyy-MM-dd</c>.</summary>
        public string VisitDate { get; set; } = string.Empty;

        public Guid ServiceUnitId { get; set; }

        public string ServiceUnitName { get; set; } = string.Empty;

        public Guid ClinicId { get; set; }

        public string ClinicName { get; set; } = string.Empty;

        public Guid DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public Guid DoctorScheduleId { get; set; }

        public Guid? RoomId { get; set; }

        /// <summary>Format <c>HH:mm</c>.</summary>
        public string StartTime { get; set; } = string.Empty;

        /// <summary>Format <c>HH:mm</c>.</summary>
        public string EndTime { get; set; } = string.Empty;

        public string? SessionName { get; set; }
    }

    public class OnlineRegistrationCheckInPayer
    {
        public OnlineRegistrationPatientType PatientType { get; set; }

        public EncounterPaymentType PaymentType { get; set; }

        /// <summary>Kartu asuransi milik pasien lama yang dipilih online.</summary>
        public Guid? PatientInsuranceId { get; set; }

        /// <summary>Penjamin perusahaan milik pasien lama yang dipilih online.</summary>
        public Guid? PatientCompanyGuarantorId { get; set; }

        /// <summary>Nama penyedia atau perusahaan dari kartu yang sudah ada, untuk ringkasan.</summary>
        public string? PayerName { get; set; }

        /// <summary>Nomor kartu yang sudah ada, tersamar <c>****1234</c>.</summary>
        public string? MaskedNumber { get; set; }

        /// <summary>Kartu asuransi baru yang dibuat kiosk saat check-in; <c>null</c> bila tidak ada.</summary>
        public OnlineRegistrationCheckInNewInsurance? NewInsurance { get; set; }
    }

    public class OnlineRegistrationCheckInNewInsurance
    {
        public Guid InsuranceProviderId { get; set; }

        public string ProviderName { get; set; } = string.Empty;

        public string PolicyNumber { get; set; } = string.Empty;

        public string? HolderName { get; set; }

        public string? HolderRelationship { get; set; }
    }

    public class OnlineRegistrationCheckInReferral
    {
        public Guid ReferralInstitutionId { get; set; }

        public string InstitutionName { get; set; } = string.Empty;

        public string? ReferralNumber { get; set; }

        /// <summary>Kalender, <c>yyyy-MM-dd</c>.</summary>
        public string? ReferralDate { get; set; }

        public string? ReferralDoctorName { get; set; }

        public string? DiagnosisNote { get; set; }

        public string? Reason { get; set; }

        /// <summary>Jumlah berkas surat rujukan yang diunggah online; dilekatkan server saat check-in.</summary>
        public int DocumentCount { get; set; }
    }
}
