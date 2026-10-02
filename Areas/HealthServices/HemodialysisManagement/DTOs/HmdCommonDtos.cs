using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs
{
    /// <summary>Satu pilihan enum untuk penyaring dan formulir.</summary>
    public class HmdOptionItemResponse
    {
        public int Value { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>Satu pilihan pengurutan pada metadata daftar.</summary>
    public class HmdSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>Kepala konteks pasien. Tidak pernah memuat diagnosis maupun status serologi.</summary>
    public class HmdPatientHeaderResponse
    {
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }
        public DateTime? BirthDate { get; set; }
    }

    /// <summary>Label Bahasa Indonesia untuk seluruh enum modul, siap ditampilkan.</summary>
    public static class HmdLabels
    {
        public static List<HmdOptionItemResponse> Options<TEnum>(Func<TEnum, string> label)
            where TEnum : struct, Enum =>
            Enum.GetValues<TEnum>()
                .Select(x => new HmdOptionItemResponse { Value = Convert.ToInt32(x), Label = label(x) })
                .ToList();

        public static List<string> SortDirections() => new() { "asc", "desc" };

        public static List<int> PageSizeOptions() => new() { 10, 25, 50, 100 };

        public static string OrderStatus(HmdOrderStatus value) => value switch
        {
            HmdOrderStatus.Requested => "Menunggu",
            HmdOrderStatus.Accepted => "Diterima",
            HmdOrderStatus.OnHold => "Ditahan",
            HmdOrderStatus.Rejected => "Ditolak",
            HmdOrderStatus.Cancelled => "Dibatalkan",
            HmdOrderStatus.Fulfilled => "Terjadwal",
            _ => value.ToString()
        };

        public static string Priority(HmdOrderPriority value) => value switch
        {
            HmdOrderPriority.Routine => "Rutin",
            HmdOrderPriority.Cito => "Cito",
            _ => value.ToString()
        };

        public static string EpisodeStatus(HmdEpisodeStatus value) => value switch
        {
            HmdEpisodeStatus.Draft => "Draf",
            HmdEpisodeStatus.Active => "Aktif",
            HmdEpisodeStatus.Suspended => "Ditangguhkan",
            HmdEpisodeStatus.Closed => "Ditutup",
            _ => value.ToString()
        };

        public static string ClosureReason(HmdEpisodeClosureReason value) => value switch
        {
            HmdEpisodeClosureReason.Completed => "Program selesai",
            HmdEpisodeClosureReason.Transferred => "Pindah fasilitas",
            HmdEpisodeClosureReason.Discontinued => "Berhenti",
            HmdEpisodeClosureReason.Deceased => "Meninggal",
            HmdEpisodeClosureReason.Other => "Lainnya",
            _ => value.ToString()
        };

        public static string EligibilityOutcome(HmdEligibilityOutcome value) => value switch
        {
            HmdEligibilityOutcome.Eligible => "Layak",
            HmdEligibilityOutcome.Deferred => "Ditunda",
            HmdEligibilityOutcome.Modified => "Layak dengan penyesuaian",
            HmdEligibilityOutcome.Referred => "Dirujuk",
            _ => value.ToString()
        };

        public static string AccessType(HmdVascularAccessType value) => value switch
        {
            HmdVascularAccessType.ArteriovenousFistula => "AV Fistula",
            HmdVascularAccessType.ArteriovenousGraft => "AV Graft",
            HmdVascularAccessType.Catheter => "Kateter (CDL)",
            HmdVascularAccessType.Other => "Lainnya",
            _ => value.ToString()
        };

        public static string AccessStatus(HmdVascularAccessStatus value) => value switch
        {
            HmdVascularAccessStatus.Usable => "Layak dipakai",
            HmdVascularAccessStatus.NeedsAttention => "Perlu perhatian",
            HmdVascularAccessStatus.NotUsable => "Tidak dapat dipakai",
            _ => value.ToString()
        };

        public static string SerologyTest(HmdSerologyTestType value) => value switch
        {
            HmdSerologyTestType.HBsAg => "HBsAg",
            HmdSerologyTestType.AntiHbs => "Anti-HBs",
            HmdSerologyTestType.AntiHcv => "Anti-HCV",
            HmdSerologyTestType.AntiHiv => "Anti-HIV",
            _ => value.ToString()
        };

        public static string SerologyResult(HmdSerologyResultFlag value) => value switch
        {
            HmdSerologyResultFlag.Reactive => "Reaktif",
            HmdSerologyResultFlag.NonReactive => "Non-reaktif",
            HmdSerologyResultFlag.Indeterminate => "Tidak dapat ditentukan",
            _ => value.ToString()
        };

        public static string SerologyReview(HmdSerologyReviewStatus value) => value switch
        {
            HmdSerologyReviewStatus.PendingReview => "Menunggu tinjauan",
            HmdSerologyReviewStatus.Reviewed => "Sudah ditinjau",
            _ => value.ToString()
        };

        public static string Isolation(HmdIsolationRequirement value) => value switch
        {
            HmdIsolationRequirement.None => "Tidak perlu isolasi",
            HmdIsolationRequirement.HepatitisB => "Khusus Hepatitis B",
            HmdIsolationRequirement.Other => "Isolasi khusus lain",
            _ => value.ToString()
        };

        public static string PrescriptionStatus(HmdPrescriptionStatus value) => value switch
        {
            HmdPrescriptionStatus.Draft => "Draf",
            HmdPrescriptionStatus.Active => "Aktif",
            HmdPrescriptionStatus.Superseded => "Digantikan",
            HmdPrescriptionStatus.Cancelled => "Dibatalkan",
            _ => value.ToString()
        };

        public static string SessionStatus(HmdSessionStatus value) => value switch
        {
            HmdSessionStatus.Planned => "Direncanakan",
            HmdSessionStatus.Scheduled => "Terjadwal",
            HmdSessionStatus.CheckedIn => "Pasien datang",
            HmdSessionStatus.PreCheck => "Persiapan Pra-HD",
            HmdSessionStatus.Held => "Ditahan",
            HmdSessionStatus.Ready => "Siap dimulai",
            HmdSessionStatus.InProgress => "Berlangsung",
            HmdSessionStatus.Stopped => "Dihentikan",
            HmdSessionStatus.Completed => "Selesai",
            HmdSessionStatus.AwaitingFinalization => "Menunggu pengesahan",
            HmdSessionStatus.Finalized => "Disahkan",
            HmdSessionStatus.Cancelled => "Dibatalkan",
            _ => value.ToString()
        };

        public static string Shift(HmdShift value) => value switch
        {
            HmdShift.Morning => "Pagi",
            HmdShift.Afternoon => "Siang",
            HmdShift.Evening => "Sore",
            _ => value.ToString()
        };

        public static string StopReason(HmdSessionStopReason value) => value switch
        {
            HmdSessionStopReason.HemodynamicInstability => "Hemodinamik tidak stabil",
            HmdSessionStopReason.CircuitClotting => "Pembekuan sirkulasi ekstrakorporeal",
            HmdSessionStopReason.VascularAccessProblem => "Masalah akses vaskular",
            HmdSessionStopReason.PatientRefusal => "Pasien menolak melanjutkan",
            HmdSessionStopReason.MachineFailure => "Gangguan mesin",
            HmdSessionStopReason.Other => "Lainnya",
            _ => value.ToString()
        };

        public static string Disposition(HmdDisposition value) => value switch
        {
            HmdDisposition.Home => "Pulang",
            HmdDisposition.ReturnToWard => "Kembali ke bangsal",
            HmdDisposition.TransferToEmergency => "Pindah ke IGD",
            HmdDisposition.Other => "Lainnya",
            _ => value.ToString()
        };

        public static string ChecklistResult(HmdChecklistResult value) => value switch
        {
            HmdChecklistResult.NotChecked => "Belum diperiksa",
            HmdChecklistResult.Met => "Terpenuhi",
            HmdChecklistResult.NotMet => "Tidak terpenuhi",
            HmdChecklistResult.NotApplicable => "Tidak berlaku",
            _ => value.ToString()
        };

        public static string ChecklistCategory(HmdChecklistCategory value) => value switch
        {
            HmdChecklistCategory.Identity => "Identitas",
            HmdChecklistCategory.ClinicalContext => "Konteks klinis",
            HmdChecklistCategory.Resource => "Sumber daya",
            _ => value.ToString()
        };

        public static string ReadinessCategory(HmdReadinessCategory value) => value switch
        {
            HmdReadinessCategory.Machine => "Mesin",
            HmdReadinessCategory.Station => "Station",
            HmdReadinessCategory.Water => "Pengolahan air",
            HmdReadinessCategory.Supply => "Obat dan BMHP",
            HmdReadinessCategory.Staff => "Tenaga",
            _ => value.ToString()
        };

        public static string ReadinessStatus(HmdReadinessStatus value) => value switch
        {
            HmdReadinessStatus.Draft => "Draf",
            HmdReadinessStatus.Ready => "Siap",
            HmdReadinessStatus.NotReady => "Tidak siap",
            _ => value.ToString()
        };

        public static string MachineStatus(HmdMachineStatus value) => value switch
        {
            HmdMachineStatus.Ready => "Siap",
            HmdMachineStatus.Blocked => "Diblokir",
            HmdMachineStatus.Maintenance => "Dalam perawatan",
            HmdMachineStatus.NotEligible => "Tidak laik",
            _ => value.ToString()
        };

        public static string StationStatus(HmdStationStatus value) => value switch
        {
            HmdStationStatus.Available => "Tersedia",
            HmdStationStatus.Blocked => "Diblokir",
            HmdStationStatus.Maintenance => "Dalam perawatan",
            _ => value.ToString()
        };

        public static string MedicationRoute(HmdMedicationRoute value) => value switch
        {
            HmdMedicationRoute.IntravenousBolus => "Bolus intravena",
            HmdMedicationRoute.ContinuousInfusion => "Infus kontinu",
            HmdMedicationRoute.ExtracorporealCircuit => "Melalui sirkuit dialisis",
            HmdMedicationRoute.Subcutaneous => "Subkutan",
            HmdMedicationRoute.Oral => "Oral",
            HmdMedicationRoute.Other => "Lainnya",
            _ => value.ToString()
        };

        public static string PharmacyHandoff(HmdPharmacyHandoffStatus value) => value switch
        {
            HmdPharmacyHandoffStatus.Pending => "Menunggu diteruskan",
            HmdPharmacyHandoffStatus.Succeeded => "Sudah diteruskan",
            HmdPharmacyHandoffStatus.Failed => "Ditolak Farmasi",
            _ => value.ToString()
        };

        public static string ComplicationType(HmdComplicationType value) => value switch
        {
            HmdComplicationType.IntradialyticHypotension => "Hipotensi intradialisis",
            HmdComplicationType.MuscleCramp => "Kram otot",
            HmdComplicationType.ChillsOrFever => "Menggigil atau demam",
            HmdComplicationType.AccessBleeding => "Perdarahan akses",
            HmdComplicationType.ChestPain => "Nyeri dada",
            HmdComplicationType.Arrhythmia => "Aritmia",
            HmdComplicationType.NauseaOrVomiting => "Mual atau muntah",
            HmdComplicationType.Headache => "Sakit kepala",
            HmdComplicationType.AllergicReaction => "Reaksi alergi",
            HmdComplicationType.IntradialyticHypertension => "Hipertensi intradialisis",
            HmdComplicationType.Other => "Lainnya",
            _ => value.ToString()
        };

        public static string ComplicationOutcome(HmdComplicationOutcome value) => value switch
        {
            HmdComplicationOutcome.Resolved => "Teratasi",
            HmdComplicationOutcome.Ongoing => "Masih berlangsung",
            HmdComplicationOutcome.Escalated => "Dieskalasi",
            _ => value.ToString()
        };

        public static string ComplicationImpact(HmdComplicationSessionImpact value) => value switch
        {
            HmdComplicationSessionImpact.Continued => "Sesi dilanjutkan",
            HmdComplicationSessionImpact.Modified => "Sesi disesuaikan",
            HmdComplicationSessionImpact.Stopped => "Sesi dihentikan",
            _ => value.ToString()
        };

        public static string StaffRole(HmdStaffRole value) => value switch
        {
            HmdStaffRole.PrimaryNurse => "Perawat penanggung jawab",
            HmdStaffRole.AssistantNurse => "Perawat pendamping",
            HmdStaffRole.Technician => "Teknisi",
            _ => value.ToString()
        };

        public static string Competency(HmdCompetencyVerificationStatus value) => value switch
        {
            HmdCompetencyVerificationStatus.Verified => "Terverifikasi",
            HmdCompetencyVerificationStatus.NotAuthorized => "Tidak berwenang",
            HmdCompetencyVerificationStatus.NotVerifiable => "Belum dapat diverifikasi",
            _ => value.ToString()
        };

        public static string BillingHandoff(HmdBillingHandoffStatus value) => value switch
        {
            HmdBillingHandoffStatus.NotRequired => "Tidak ditagihkan",
            HmdBillingHandoffStatus.Pending => "Menunggu diserahkan",
            HmdBillingHandoffStatus.Succeeded => "Sudah diserahkan",
            HmdBillingHandoffStatus.Failed => "Gagal diserahkan",
            _ => value.ToString()
        };
    }
}
