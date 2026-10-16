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

        /// <summary>Didaftarkan sebelum hari kunjungan dan pasien belum datang.</summary>
        public bool IsBooking { get; set; }
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
        public Guid? DoctorScheduleId { get; set; }
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

        /// <summary>
        /// Rujukan masuk kunjungan ini (<c>RJ-REF-REV-004</c>/<c>005</c>): sumber, fasilitas atau
        /// surat internal dasar, kelengkapan, dan berkas. <c>null</c> bila bukan kunjungan rujukan.
        /// </summary>
        public EncounterReferralResponse? Referral { get; set; }

        /// <summary>
        /// Surat Rujukan terkait: surat yang diterbitkan dokter pada kunjungan ini dan surat
        /// internal yang menjadi dasar kunjungan ini. Isi lengkap dibuka lewat
        /// <c>GET /{id}/referral-letters/{certificateId}</c>.
        /// </summary>
        public List<OutpatientReferralLetterResponse> ReferralLetters { get; set; } = new();

        /// <summary>Hak dan status mengizinkan edit data pendaftaran.</summary>
        public bool CanEdit { get; set; }

        /// <summary>Alasan edit ditutup oleh status kunjungan; <c>null</c> bila status mengizinkan.</summary>
        public string? EditBlockedReason { get; set; }

        /// <summary>Hak Update dan keadaan kunjungan mengizinkan ganti tanggal/sesi praktik (dokter tetap).</summary>
        public bool CanChangeSchedule { get; set; }

        /// <summary>Alasan ganti jadwal ditutup; <c>null</c> bila boleh.</summary>
        public string? ScheduleChangeBlockedReason { get; set; }

        /// <summary>Hak <c>OutpatientEncounter : Cancel</c> dan status mengizinkan pembatalan.</summary>
        public bool CanCancel { get; set; }

        /// <summary>Alasan pembatalan ditutup; <c>null</c> bila status mengizinkan.</summary>
        public string? CancelBlockedReason { get; set; }

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

        /// <summary>
        /// Tanggal kunjungan baru (<c>yyyy-MM-dd</c>). Diisi bersama <see cref="DoctorScheduleId"/>
        /// untuk mengganti jadwal; keduanya kosong = jadwal tidak diubah.
        /// </summary>
        public string? VisitDate { get; set; }

        /// <summary>Sesi praktik baru milik dokter yang sama di poliklinik kunjungan.</summary>
        public Guid? DoctorScheduleId { get; set; }
    }

    /// <summary>Satu jadwal tersedia (tanggal + sesi praktik) dokter kunjungan.</summary>
    public class OutpatientEncounterScheduleOptionResponse
    {
        /// <summary><c>yyyy-MM-dd|doctorScheduleId</c>.</summary>
        public string Value { get; set; } = string.Empty;
        public string VisitDate { get; set; } = string.Empty;
        public Guid DoctorScheduleId { get; set; }
        public string Label { get; set; } = string.Empty;
        public int MaxPatientQuota { get; set; }
        public int UsedPatientQuota { get; set; }

        /// <summary>Jadwal yang sedang dipakai kunjungan pada tanggal itu.</summary>
        public bool IsCurrent { get; set; }
    }

    /// <summary>Ringkasan satu Surat Rujukan terkait kunjungan (<c>RJ-REF-REV-004</c>).</summary>
    public class OutpatientReferralLetterResponse
    {
        /// <summary>Surat diterbitkan dokter pada kunjungan ini.</summary>
        public const string DirectionOutgoing = "outgoing";

        /// <summary>Surat internal yang menjadi dasar kunjungan ini.</summary>
        public const string DirectionIncoming = "incoming";

        public Guid CertificateId { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
        public string Direction { get; set; } = DirectionOutgoing;
        public string DirectionName { get; set; } = string.Empty;
        public DateTime IssuedDate { get; set; }
        public string? DoctorName { get; set; }
        public string? SourceClinicName { get; set; }
        public string? SourceEncounterNumber { get; set; }
        public string? TargetName { get; set; }

        /// <summary><c>1</c> Terbit, <c>2</c> Dibatalkan.</summary>
        public int CertificateStatus { get; set; }
        public string CertificateStatusName { get; set; } = string.Empty;

        /// <summary>Hanya surat berstatus terbit yang dapat dibuka sebagai surat resmi dan dicetak.</summary>
        public bool CanPrint { get; set; }

        /// <summary>Nomor kunjungan rujukan aktif yang memakai surat ini (surat keluar).</summary>
        public string? FollowUpEncounterNumber { get; set; }
    }
}
