namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Kontrak penerap keputusan workflow domain untuk satu jenis rujukan Workforce Planning.
    /// Diimplementasikan oleh service domain spesifik (misal: batch inisialisasi formasi awal,
    /// standar kebutuhan tenaga, manpower plan, headcount request, atau pembatalan kursi).
    /// </summary>
    public interface IWorkforcePlanningDecisionApplier
    {
        /// <summary>
        /// Jenis rujukan workflow yang ditangani (harus cocok dengan WorkforcePlanningValueConstants.WorkflowReferenceTypes).
        /// </summary>
        string ReferenceType { get; }

        /// <summary>
        /// Menerapkan keputusan akhir workflow (Approved, Rejected, Returned, dsb.) ke entitas domain terkait.
        /// </summary>
        Task<WorkforcePlanningApplyResult> ApplyDecisionAsync(
            WorkforcePlanningDecisionContext context,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Konteks keputusan workflow yang diteruskan ke penerap domain.
    /// </summary>
    public class WorkforcePlanningDecisionContext
    {
        public Guid WorkflowInstanceId { get; set; }
        public Guid ReferenceId { get; set; }
        public string ReferenceType { get; set; } = string.Empty;
        public string WorkflowStatus { get; set; } = string.Empty;
        public Guid ActorUserId { get; set; }
        public bool AllowAutoApply { get; set; }
    }

    /// <summary>
    /// Hasil penerapan keputusan workflow pada entitas domain.
    /// </summary>
    public class WorkforcePlanningApplyResult
    {
        public bool IsHandled { get; set; }
        public string PreviousStatus { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public bool StatusChanged { get; set; }
        public bool AutoApplyAttempted { get; set; }
        public bool AutoApplySucceeded { get; set; }
        public string? WarningMessage { get; set; }
        public bool IsAlreadyFinal { get; set; }

        public static WorkforcePlanningApplyResult Unhandled(string message) => new()
        {
            IsHandled = false,
            WarningMessage = message
        };

        public static WorkforcePlanningApplyResult Success(
            string previousStatus,
            string currentStatus,
            bool statusChanged = true,
            bool autoApplyAttempted = false,
            bool autoApplySucceeded = false) => new()
        {
            IsHandled = true,
            PreviousStatus = previousStatus,
            CurrentStatus = currentStatus,
            StatusChanged = statusChanged,
            AutoApplyAttempted = autoApplyAttempted,
            AutoApplySucceeded = autoApplySucceeded
        };

        public static WorkforcePlanningApplyResult AlreadyFinal(string currentStatus) => new()
        {
            IsHandled = true,
            StatusChanged = false,
            CurrentStatus = currentStatus,
            PreviousStatus = currentStatus,
            IsAlreadyFinal = true
        };
    }
}
