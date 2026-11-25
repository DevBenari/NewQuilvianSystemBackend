using System;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Constants
{
    /// <summary>
    /// Kode definisi dan jenis rujukan workflow persetujuan sub-modul 05 (T15–T18, <c>INT-HR-30</c>;
    /// planning <c>PLANNING_PERSETUJUAN_T15_T18_KONTRAK_DAN_MASA_KERJA.md</c>).
    /// </summary>
    /// <remarks>
    /// Kode definisi sama persis dengan baris <c>MstWorkflowDefinition.WorkflowCode</c> yang ditanam
    /// <c>OnboardingRuntimeDevSeeder</c> (<c>T15</c>–<c>T18</c>). Langkah dan penyetujunya diatur admin
    /// lewat layar Langkah Workflow, bukan di kode.
    /// </remarks>
    public static class EmploymentLifecycleWorkflowCodes
    {
        public const string ProbationAssessmentDefinitionCode = "T15";
        public const string ContractDecisionDefinitionCode = "T16";
        public const string ContractCorrectionDefinitionCode = "T17";
        public const string InitialContractBatchDefinitionCode = "T18";

        public const string ProbationAssessmentReferenceType = "WorkforceProbationAssessment";
        public const string ContractDecisionReferenceType = "WorkforceContractDecision";
        public const string ContractCorrectionReferenceType = "WorkforceContractCorrection";
        public const string InitialContractBatchReferenceType = "WorkforceInitialContractBatch";

        public static bool IsLifecycleReference(string? referenceType) =>
            Eq(referenceType, ProbationAssessmentReferenceType) ||
            Eq(referenceType, ContractDecisionReferenceType) ||
            Eq(referenceType, ContractCorrectionReferenceType) ||
            Eq(referenceType, InitialContractBatchReferenceType);

        /// <summary>Kunci idempoten pembuatan instance: satu instance per dokumen.</summary>
        public static string IdempotencyKey(string definitionCode, Guid referenceId) =>
            $"hrd-lifecycle:{definitionCode.ToLowerInvariant()}:{referenceId:N}";

        /// <summary>Hasil workflow yang relevan bagi dokumen lifecycle.</summary>
        public enum Outcome
        {
            Unknown = 0,
            InReview = 1,
            Returned = 2,
            Approved = 3,
            Rejected = 4,
            Withdrawn = 5
        }

        /// <summary>
        /// Memetakan status instance ke hasil dokumen. Mesin menutup instance yang disetujui dengan
        /// <c>Completed</c> (bukan <c>Approved</c>); keduanya diperlakukan sama.
        /// </summary>
        public static Outcome MapOutcome(string? workflowStatus)
        {
            if (Eq(workflowStatus, WorkflowValueConstants.WorkflowStatus.Submitted) ||
                Eq(workflowStatus, WorkflowValueConstants.WorkflowStatus.InProgress))
            {
                return Outcome.InReview;
            }

            if (Eq(workflowStatus, WorkflowValueConstants.WorkflowStatus.RevisionRequested) ||
                Eq(workflowStatus, WorkflowValueConstants.WorkflowStatus.Returned))
            {
                return Outcome.Returned;
            }

            if (Eq(workflowStatus, WorkflowValueConstants.WorkflowStatus.Completed) ||
                Eq(workflowStatus, WorkflowValueConstants.WorkflowStatus.Approved))
            {
                return Outcome.Approved;
            }

            if (Eq(workflowStatus, WorkflowValueConstants.WorkflowStatus.Rejected))
            {
                return Outcome.Rejected;
            }

            if (Eq(workflowStatus, WorkflowValueConstants.WorkflowStatus.Withdrawn) ||
                Eq(workflowStatus, WorkflowValueConstants.WorkflowStatus.Cancelled))
            {
                return Outcome.Withdrawn;
            }

            return Outcome.Unknown;
        }

        private static bool Eq(string? left, string right) =>
            string.Equals(left?.Trim(), right, StringComparison.OrdinalIgnoreCase);
    }
}
