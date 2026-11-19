using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models
{
    /// <summary>
    /// Parameter operasional lembur rumah sakit (BE-HRD-132, HRD-DEC-117, HRD-DEC-118, HRD-DEC-121).
    /// </summary>
    /// <remarks>
    /// Register masalah SDM <c>MSL-10</c> (18 November 2026): sebelumnya parameter ini hanya disimpan di memori statis
    /// <c>OvertimeSettingService</c>, sehingga hilang saat server dimulai ulang, berbeda antar-instans, dan tidak teraudit.
    /// Satu baris aktif per rumah sakit (<see cref="HospitalCode"/>), sama dengan pola <c>HrdAttendanceSetting</c>.
    /// </remarks>
    public class HrdOvertimeSetting : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string HospitalCode { get; set; } = "RSMMC";

        /// <summary>Batas lembur reguler per hari dalam jam (PP 35/2021: 4 jam).</summary>
        public decimal DailyMaxHoursRegular { get; set; } = 4m;

        /// <summary>Batas lembur reguler per minggu dalam jam (PP 35/2021: 18 jam).</summary>
        public decimal WeeklyMaxHoursRegular { get; set; } = 18m;

        public int RoundingIntervalMinutes { get; set; } = 30;

        public int MinimumThresholdMinutes { get; set; } = 30;

        /// <summary>Masa berlaku libur pengganti (CTO) dalam hari.</summary>
        public int CtoValidityDays { get; set; } = 90;

        /// <summary>Tanggal tutup buku lembur bulanan (1–28).</summary>
        public int MonthlyCutoffDay { get; set; } = 20;

        /// <summary>Toleransi selisih jam lembur terhadap mesin presensi dalam menit.</summary>
        public int BiometricDiscrepancyTolerance { get; set; } = 30;

        /// <summary>Tenggat klaim lembur darurat (cito) setelah dinas, dalam hari.</summary>
        public int PostClaimCitoGraceDays { get; set; } = 1;

        public bool IsActive { get; set; } = true;
    }
}
