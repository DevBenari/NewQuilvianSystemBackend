using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models
{
    /// <summary>
    /// Parameter konfigurasi aturan waktu kerja dan penjadwalan dinas rumah sakit per lokasi (HospitalSite).
    /// Mengakomodasi HRD-DEC-099, HRD-DEC-102, VR-SCH-80.
    /// </summary>
    [Table("HrdSchedulingSettings", Schema = "public")]
    public class HrdSchedulingSetting : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid HospitalSiteId { get; set; }

        /// <summary>
        /// Jeda istirahat minimal antar shift dinas dalam satuan jam (default: 11 jam). Aturan W1.
        /// </summary>
        public int MinRestHoursBetweenShifts { get; set; } = 11;

        /// <summary>
        /// Batas maksimal hari kerja berturut-turut tanpa libur (default: 6 hari). Aturan W2.
        /// </summary>
        public int MaxConsecutiveWorkDays { get; set; } = 6;

        /// <summary>
        /// Batas maksimal shift malam berturut-turut (default: 2 malam). Aturan W3.
        /// </summary>
        public int MaxConsecutiveNightShifts { get; set; } = 2;

        /// <summary>
        /// Toleransi batas hari koreksi mundur mandiri oleh unit kerja (default: 3 hari). HRD-DEC-102.
        /// </summary>
        public int RetroactiveCorrectionWindowDays { get; set; } = 3;

        /// <summary>
        /// Apakah pelanggaran jeda istirahat antar shift dieskalasi menjadi aturan memblokir (Blocking).
        /// </summary>
        public bool RestHoursEscalatedToBlock { get; set; } = false;

        /// <summary>
        /// Apakah pelanggaran hari kerja berturut-turut dieskalasi menjadi aturan memblokir (Blocking).
        /// </summary>
        public bool ConsecutiveDaysEscalatedToBlock { get; set; } = false;

        /// <summary>
        /// Apakah pelanggaran shift malam berurutan dieskalasi menjadi aturan memblokir (Blocking).
        /// </summary>
        public bool NightShiftsEscalatedToBlock { get; set; } = false;

        /// <summary>
        /// Token konkurensi optimistik.
        /// </summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public MstHospitalSite? HospitalSite { get; set; }
    }
}
