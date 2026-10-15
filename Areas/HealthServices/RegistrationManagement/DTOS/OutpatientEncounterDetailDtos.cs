using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    /// <summary>
    /// Detail satu kunjungan pada Daftar Pasien Rawat Jalan (RJ-REG-REV-BE-003,
    /// RJ-REG-DEC-005). Baca saja, bercakupan pengguna seperti daftar.
    /// </summary>
    public class OutpatientEncounterDetailResponse
    {
        public Guid Id { get; set; }
        public string EncounterNumber { get; set; } = string.Empty;
        public DateTime EncounterDate { get; set; }
        public int EncounterStatus { get; set; }
        public string EncounterStatusName { get; set; } = string.Empty;
        public string VisitTypeName { get; set; } = string.Empty;
        public bool IsAppointment { get; set; }
        public bool IsWalkIn { get; set; }
        public bool IsFromKiosk { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelReason { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime? CheckedInAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public Guid ClinicId { get; set; }
        public string ClinicName { get; set; } = string.Empty;
        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public string? DoctorSpecialistName { get; set; }
        public string? ScheduleLabel { get; set; }

        public string? QueueCode { get; set; }
        public string? QueueStatusName { get; set; }
        public DateTime? QueueDate { get; set; }

        public string PaymentTypeName { get; set; } = string.Empty;
        public string PaymentLabel { get; set; } = string.Empty;
        public string? PolicyNumber { get; set; }
        public string? CardNumber { get; set; }

        public string ReferralStatus { get; set; } = OutpatientReferralStatuses.NotReferral;
        public string? ChiefComplaint { get; set; }
        public string? Notes { get; set; }

        public OutpatientEncounterPatientResponse Patient { get; set; } = new();

        /// <summary>Hak dan status mengizinkan edit data pendaftaran.</summary>
        public bool CanEdit { get; set; }

        /// <summary>Alasan edit ditutup oleh status kunjungan; <c>null</c> bila status mengizinkan.</summary>
        public string? EditBlockedReason { get; set; }

        public DateTime? UpdateDateTime { get; set; }
    }

    public class OutpatientEncounterPatientResponse
    {
        public Guid Id { get; set; }
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public string? GenderName { get; set; }
        public string? IdentityType { get; set; }
        public string? IdentityNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public PatientCitizenship? Citizenship { get; set; }
        public string? CitizenshipName { get; set; }
        public Guid? NationalityCountryId { get; set; }
        public string? NationalityCountryName { get; set; }
    }

    /// <summary>
    /// Data pendaftaran yang boleh diubah dari Daftar Pasien Rawat Jalan (RJ-REG-DEC-005).
    /// Seluruh field dikirim; nilai kosong mengosongkan, kecuali <see cref="Citizenship"/> yang
    /// <c>null</c>-nya berarti tidak diubah. Identitas pasien (nama, No. RM, NIK, tanggal lahir),
    /// status, poli, dan dokter sengaja tidak ada di sini. Panjang dan format diperiksa service
    /// supaya pesannya berbahasa layar, bukan pesan bawaan model binding.
    /// </summary>
    public class OutpatientEncounterRegistrationDataUpdateRequest
    {
        public string? PhoneNumber { get; set; }

        public string? WhatsAppNumber { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public PatientCitizenship? Citizenship { get; set; }

        public Guid? NationalityCountryId { get; set; }

        public string? Notes { get; set; }
    }
}
