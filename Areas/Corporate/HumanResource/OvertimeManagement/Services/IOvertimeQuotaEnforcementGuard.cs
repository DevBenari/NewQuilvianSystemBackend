namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimeQuotaEvaluationResult
    {
        public bool IsAllowed { get; set; } = true;
        public bool IsMedicalDirectorEscalation { get; set; } = false;
        public decimal AccumulatedDailyHours { get; set; } = 0;
        public decimal AccumulatedWeeklyHours { get; set; } = 0;
        public string? DenialReason { get; set; }
    }

    public interface IOvertimeQuotaEnforcementGuard
    {
        Task<OvertimeQuotaEvaluationResult> EvaluateQuotaAsync(
            Guid workforceProfileId,
            DateOnly overtimeDate,
            decimal requestedHours,
            bool isEmergencyCito = false,
            CancellationToken cancellationToken = default);
    }
}
