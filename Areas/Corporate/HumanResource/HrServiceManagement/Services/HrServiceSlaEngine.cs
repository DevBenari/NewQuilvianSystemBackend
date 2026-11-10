using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services
{
    public class HrServiceSlaEngine : IHrServiceSlaEngine
    {
        private readonly ILogger<HrServiceSlaEngine> _logger;

        // Jam kerja kantor operasional HRD Rumah Sakit Quilvian: 08:00 - 17:00 WIB (Senin - Jumat)
        private const int WorkHourStart = 8;
        private const int WorkHourEnd = 17;
        private const int DailyWorkHours = WorkHourEnd - WorkHourStart; // 9 jam per hari

        public HrServiceSlaEngine(ILogger<HrServiceSlaEngine> logger)
        {
            _logger = logger;
        }

        public DateTime CalculateSlaDeadline(DateTime startUtc, string priorityTier, int slaTargetHours)
        {
            // Aturan Bisnis HRS-B2:
            // Tier Cito dihitung secara linier 24/7 jam kalender demi mobilitas klinis darurat
            if (string.Equals(priorityTier, "Cito", StringComparison.OrdinalIgnoreCase))
            {
                return startUtc.AddHours(slaTargetHours);
            }

            // Tier Standar (24 jam) & Khusus (72 jam) dihitung berdasarkan jam kerja operasional RS (WIB = UTC+7)
            var currentWib = startUtc.AddHours(7);
            var remainingHours = (double)slaTargetHours;

            while (remainingHours > 0)
            {
                // Jika akhir pekan (Sabtu / Minggu), majukan ke hari Senin berikutnya jam 08:00 WIB
                if (currentWib.DayOfWeek == DayOfWeek.Saturday)
                {
                    currentWib = currentWib.Date.AddDays(2).AddHours(WorkHourStart);
                    continue;
                }
                if (currentWib.DayOfWeek == DayOfWeek.Sunday)
                {
                    currentWib = currentWib.Date.AddDays(1).AddHours(WorkHourStart);
                    continue;
                }

                // Jika di luar jam kerja (sebelum jam 08:00 WIB)
                if (currentWib.Hour < WorkHourStart)
                {
                    currentWib = currentWib.Date.AddHours(WorkHourStart);
                }
                // Jika setelah jam kerja (setelah jam 17:00 WIB)
                else if (currentWib.Hour >= WorkHourEnd)
                {
                    currentWib = currentWib.Date.AddDays(1).AddHours(WorkHourStart);
                    continue;
                }

                // Hitung sisa jam kerja yang tersedia pada hari berjalan
                var endOfWorkDay = currentWib.Date.AddHours(WorkHourEnd);
                var availableToday = (endOfWorkDay - currentWib).TotalHours;

                if (remainingHours <= availableToday)
                {
                    currentWib = currentWib.AddHours(remainingHours);
                    remainingHours = 0;
                }
                else
                {
                    remainingHours -= availableToday;
                    // Majukan ke jam 08:00 hari berikutnya
                    currentWib = currentWib.Date.AddDays(1).AddHours(WorkHourStart);
                }
            }

            // Konversi kembali dari WIB ke UTC
            var deadlineUtc = currentWib.AddHours(-7);

            _logger.LogInformation("[SLA-HRS-CALC] Start UTC: {StartUtc}, Tier: {Tier}, SLA: {TargetHours} Jam -> Deadline UTC: {DeadlineUtc}",
                startUtc, priorityTier, slaTargetHours, deadlineUtc);

            return deadlineUtc;
        }

        public (double ElapsedHours, double ElapsedPercentage, bool IsBreached) EvaluateSlaProgress(TrxHrServiceRequest ticket, DateTime currentUtc)
        {
            if (!ticket.SlaDeadlineUtc.HasValue)
            {
                return (0, 0, false);
            }

            // Jika status tiket sedang ditangguhkan (WaitingStaffResponse), evaluasi waktu membeku pada waktu dipause
            var effectiveEvaluationTime = (ticket.RequestStatus == "WaitingStaffResponse" && ticket.SlaPausedAtUtc.HasValue)
                ? ticket.SlaPausedAtUtc.Value
                : currentUtc;

            bool isBreached = effectiveEvaluationTime >= ticket.SlaDeadlineUtc.Value;

            double totalMinutes = (ticket.SlaDeadlineUtc.Value - ticket.RequestedAt).TotalMinutes;
            double elapsedMinutes = (effectiveEvaluationTime - ticket.RequestedAt).TotalMinutes - ticket.TotalPausedMinutes;

            if (elapsedMinutes < 0) elapsedMinutes = 0;

            double percentage = totalMinutes > 0 ? (elapsedMinutes / totalMinutes) * 100.0 : 0.0;
            if (isBreached) percentage = 100.0;

            percentage = Math.Min(100.0, Math.Max(0.0, percentage));

            return (elapsedMinutes / 60.0, percentage, isBreached);
        }

        public void PauseSlaTimer(TrxHrServiceRequest ticket, DateTime pauseTimeUtc)
        {
            if (!ticket.SlaPausedAtUtc.HasValue)
            {
                ticket.SlaPausedAtUtc = pauseTimeUtc;
                _logger.LogInformation("[SLA-HRS-PAUSE] Timer SLA untuk tiket {TicketNumber} ditangguhkan pada {PauseTimeUtc}",
                    ticket.RequestNumber, pauseTimeUtc);
            }
        }

        public void ResumeSlaTimer(TrxHrServiceRequest ticket, DateTime resumeTimeUtc)
        {
            if (ticket.SlaPausedAtUtc.HasValue)
            {
                var pausedSpan = resumeTimeUtc - ticket.SlaPausedAtUtc.Value;
                if (pausedSpan > TimeSpan.Zero)
                {
                    int pausedMinutes = (int)pausedSpan.TotalMinutes;
                    ticket.TotalPausedMinutes += pausedMinutes;
                    ticket.SlaDeadlineUtc = ticket.SlaDeadlineUtc?.Add(pausedSpan);
                    ticket.SlaDueAt = ticket.SlaDeadlineUtc;

                    _logger.LogInformation("[SLA-HRS-RESUME] Timer SLA tiket {TicketNumber} dilanjutkan. Perpanjangan: {Minutes} menit -> Deadline Baru: {NewDeadline}",
                        ticket.RequestNumber, pausedMinutes, ticket.SlaDeadlineUtc);
                }

                ticket.SlaPausedAtUtc = null;
            }
        }
    }
}
