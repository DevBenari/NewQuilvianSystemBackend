using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    /// <summary>
    /// Permintaan Cek Nomor Rekam Medis dari Kiosk (<c>BE-KSK-001</c>).
    /// Dikirim lewat body POST supaya No. KTP/HP tidak pernah masuk URL (PRIV-3).
    /// </summary>
    public class KioskPatientLookupRequest
    {
        /// <summary>Wajib. <c>1</c> KTP, <c>2</c> HP, <c>3</c> kartu asuransi, <c>4</c> nomor member.</summary>
        public KioskPatientLookupSearchType? SearchType { get; set; }

        /// <summary>Wajib, maks. 32 karakter. Sensitif: tidak pernah dicatat utuh.</summary>
        public string? Value { get; set; }
    }

    public class KioskPatientLookupResponse
    {
        public KioskPatientLookupResult Result { get; set; }

        /// <summary>
        /// <c>EXISTING_PATIENT_REGISTRATION</c>, <c>NEW_PATIENT_REGISTRATION</c>,
        /// <c>USE_IDENTITY_NUMBER_OR_CONTACT_STAFF</c>, atau <c>CONTACT_STAFF</c>.
        /// </summary>
        public string NextAction { get; set; } = string.Empty;

        /// <summary>Terisi hanya bila <see cref="Result"/> = <see cref="KioskPatientLookupResult.Found"/>.</summary>
        public KioskPatientCardResponse? Patient { get; set; }
    }

    /// <summary>
    /// Field Kartu Pasien existing saja ditambah <c>PatientId</c> (<c>KSK-FACT-005</c>, SEC-KSK-004).
    /// Sengaja tanpa KTP, HP, alamat, tanggal lahir, maupun data medis lain.
    /// </summary>
    public class KioskPatientCardResponse
    {
        public Guid PatientId { get; set; }

        public string MedicalRecordNumber { get; set; } = string.Empty;

        public string PatientCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string PatientTypeName { get; set; } = string.Empty;

        public string? GenderName { get; set; }

        public string? BloodTypeName { get; set; }
    }
}
