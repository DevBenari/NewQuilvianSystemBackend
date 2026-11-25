using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public interface IHrdFixedTermPolicyService
    {
        /// <summary>
        /// Evaluasi kepatuhan hukum PKWT berdasarkan UU No. 27 Tahun 2026 (atau aturan transisi PP No. 35/2021).
        /// </summary>
        FixedTermCheckResponse EvaluateCompliance(WfpContractHistory contract);

        /// <summary>
        /// Validasi usulan perpanjangan kontrak (ProposedStartDate, ProposedEndDate, ExtensionMonths)
        /// terhadap batasan maksimal 2x perpanjangan dan maksimal 12 bulan per perpanjangan (UU 27/2026 Pasal 82).
        /// </summary>
        (bool IsValid, string? ErrorMessage) ValidateRenewalProposal(
            WfpContractHistory contract,
            DateOnly proposedStartDate,
            DateOnly? proposedEndDate,
            int? extensionMonths);
    }
}
