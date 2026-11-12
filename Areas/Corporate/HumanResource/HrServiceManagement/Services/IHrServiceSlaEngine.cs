using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services
{
    public interface IHrServiceSlaEngine
    {
        /// <summary>
        /// Menghitung tenggat waktu penyelesaian tiket berdasarkan jam operasional resmi rumah sakit (HRS-B2)
        /// Standar/Khusus: Senin-Jumat 08:00-17:00 WIB (9 jam kerja/hari).
        /// Cito: 24/7 jam kalender linier darurat medis.
        /// </summary>
        DateTime CalculateSlaDeadline(DateTime startUtc, string priorityTier, int slaTargetHours);

        /// <summary>
        /// Mengevaluasi durasi waktu yang telah berjalan dan persentase SLA tiket aktif
        /// </summary>
        (double ElapsedHours, double ElapsedPercentage, bool IsBreached) EvaluateSlaProgress(TrxHrServiceRequest ticket, DateTime currentUtc);

        /// <summary>
        /// Menangguhkan timer SLA saat tiket beralih ke status WaitingStaffResponse (HRS-W2)
        /// </summary>
        void PauseSlaTimer(TrxHrServiceRequest ticket, DateTime pauseTimeUtc);

        /// <summary>
        /// Melanjutkan timer SLA saat staf pemohon merespons klarifikasi (HRS-W2)
        /// </summary>
        void ResumeSlaTimer(TrxHrServiceRequest ticket, DateTime resumeTimeUtc);
    }
}
