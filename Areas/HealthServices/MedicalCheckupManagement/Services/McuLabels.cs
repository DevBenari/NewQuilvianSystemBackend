using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services
{
    /// <summary>
    /// Label tampil enum MCU dalam bahasa Indonesia. Label status order adalah sembilan label yang
    /// disahkan <c>MCU-DEC-031</c>; layar memakai label ini apa adanya.
    /// </summary>
    public static class McuLabels
    {
        /// <summary>Urutan alur status (<c>MCU-STATE-v1</c>), dipakai kartu ringkasan dan penyaring.</summary>
        public static readonly McuOrderStatus[] OrderStatusFlow =
        [
            McuOrderStatus.Scheduled,
            McuOrderStatus.NoShow,
            McuOrderStatus.CheckedIn,
            McuOrderStatus.InExamination,
            McuOrderStatus.AwaitingResume,
            McuOrderStatus.DoctorApproved,
            McuOrderStatus.Verified,
            McuOrderStatus.Delivered,
            McuOrderStatus.Cancelled
        ];

        public static string OrderStatus(McuOrderStatus status) => status switch
        {
            McuOrderStatus.Scheduled => "Terjadwal",
            McuOrderStatus.NoShow => "Tidak Hadir",
            McuOrderStatus.CheckedIn => "Hadir",
            McuOrderStatus.InExamination => "Dalam Pemeriksaan",
            McuOrderStatus.AwaitingResume => "Menunggu Resume",
            McuOrderStatus.DoctorApproved => "Disetujui Dokter",
            McuOrderStatus.Verified => "Terverifikasi",
            McuOrderStatus.Delivered => "Diserahkan",
            McuOrderStatus.Cancelled => "Dibatalkan",
            _ => status.ToString()
        };

        public static string Source(McuOrderSource source) => source switch
        {
            McuOrderSource.DirectBooking => "Booking",
            McuOrderSource.OutpatientRecommendation => "Rekomendasi Rawat Jalan",
            McuOrderSource.InpatientRecommendation => "Rekomendasi Rawat Inap",
            McuOrderSource.EmergencyRecommendation => "Rekomendasi IGD",
            McuOrderSource.WalkInAdmission => "Walk-in Admisi",
            McuOrderSource.WalkInKiosk => "Walk-in Kiosk",
            McuOrderSource.CompanyImport => "Impor Perusahaan",
            _ => source.ToString()
        };

        public static string ResumeStatus(McuResumeStatus status) => status switch
        {
            McuResumeStatus.Draft => "Draf",
            McuResumeStatus.Approved => "Disetujui Dokter",
            McuResumeStatus.Verified => "Terverifikasi",
            McuResumeStatus.Corrected => "Dikoreksi",
            _ => status.ToString()
        };

        public static string DeliveryRecipient(McuDeliveryRecipient recipient) => recipient switch
        {
            McuDeliveryRecipient.Participant => "Peserta",
            McuDeliveryRecipient.Company => "Perusahaan",
            _ => recipient.ToString()
        };

        public static string DeliveryContent(McuDeliveryContent content) => content switch
        {
            McuDeliveryContent.Full => "Hasil lengkap",
            McuDeliveryContent.FitnessSummary => "Ringkasan kelayakan",
            _ => content.ToString()
        };

        public static string DeliveryChannel(McuDeliveryChannel channel) => channel switch
        {
            McuDeliveryChannel.Pickup => "Ambil langsung",
            McuDeliveryChannel.WhatsApp => "WhatsApp",
            McuDeliveryChannel.Email => "Email",
            McuDeliveryChannel.Phone => "Telepon",
            _ => channel.ToString()
        };

        public static string ConsentEvidence(McuConsentEvidenceType type) => type switch
        {
            McuConsentEvidenceType.PaperForm => "Formulir kertas",
            McuConsentEvidenceType.ElectronicAcknowledgement => "Persetujuan elektronik",
            _ => type.ToString()
        };

        public static string ComponentStatus(McuOrderComponentStatus status) => status switch
        {
            McuOrderComponentStatus.Pending => "Menunggu",
            McuOrderComponentStatus.Ordered => "Dipesan",
            McuOrderComponentStatus.ReadyToFill => "Siap diisi",
            McuOrderComponentStatus.Rescheduled => "Dijadwal ulang",
            McuOrderComponentStatus.Completed => "Selesai",
            McuOrderComponentStatus.Cancelled => "Dibatalkan",
            _ => status.ToString()
        };

        public static string RequirementOutcome(McuRequirementOutcome outcome) => outcome switch
        {
            McuRequirementOutcome.Fulfilled => "Terpenuhi",
            McuRequirementOutcome.ProceedWithNote => "Lanjut dengan catatan",
            McuRequirementOutcome.ComponentRescheduled => "Komponen dijadwal ulang",
            _ => outcome.ToString()
        };

        public static string ClearanceBasis(McuClearanceBasis basis) => basis switch
        {
            McuClearanceBasis.Paid => "Lunas",
            McuClearanceBasis.Guaranteed => "Terjamin",
            _ => basis.ToString()
        };

        public static string CheckInChannel(McuCheckInChannel channel) => channel switch
        {
            McuCheckInChannel.Admission => "Admisi",
            McuCheckInChannel.Kiosk => "Kiosk",
            _ => channel.ToString()
        };

        public static string PatientMatchMethod(McuPatientMatchMethod method) => method switch
        {
            McuPatientMatchMethod.ExistingAtBooking => "Pasien terdaftar saat booking",
            McuPatientMatchMethod.NikAndBirthDateMatch => "Cocok NIK dan tanggal lahir",
            McuPatientMatchMethod.ConfirmedByStaff => "Dikonfirmasi petugas",
            McuPatientMatchMethod.NewPatientRegistered => "Pasien baru didaftarkan",
            _ => method.ToString()
        };

        /// <summary>
        /// Komponen <b>tuntas</b> = selesai di modulnya/terisi, atau dibatalkan resmi oleh dokter MCU
        /// (glosarium <c>03-domain-architecture.md</c>). Semua tuntas → order boleh <i>Menunggu Resume</i>.
        /// </summary>
        public static bool IsComponentResolved(McuOrderComponentStatus status) =>
            status is McuOrderComponentStatus.Completed or McuOrderComponentStatus.Cancelled;

        /// <summary>Contoh <c>3 dari 4</c>; <c>null</c> selama order belum berkomponen.</summary>
        public static string? Completion(int resolved, int total) =>
            total == 0 ? null : $"{resolved} dari {total}";

        /// <summary>
        /// NIK untuk daftar: empat digit awal dan akhir tampak, sisanya bintang —
        /// <c>3201230101900001</c> → <c>3201********0001</c> (<c>MCU-FE-003</c>).
        /// </summary>
        public static string? MaskNik(string? nik)
        {
            if (string.IsNullOrWhiteSpace(nik))
                return null;

            var value = nik.Trim();
            return value.Length <= 8
                ? new string('*', value.Length)
                : value[..4] + new string('*', value.Length - 8) + value[^4..];
        }

        public static List<McuOptionItemResponse> EnumOptions<T>(IEnumerable<T> values, Func<T, string> label) where T : struct, Enum =>
            values.Select(x => new McuOptionItemResponse { Value = x.ToString(), Label = label(x) }).ToList();
    }
}
