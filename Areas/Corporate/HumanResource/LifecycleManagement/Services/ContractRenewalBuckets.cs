using System;
using System.Linq.Expressions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    /// <summary>
    /// Satu-satunya definisi tahapan perpanjangan kontrak kerja (BE-HRD-457, <c>HRD-DEC-245</c>, state-transition v30
    /// bagian 34.1). Dipakai layar Kontrak (<c>GET employment-contracts/summary</c> dan <c>renewal-summary</c>) dan
    /// Dashboard SDM, sehingga kedua layar selalu menunjukkan angka yang sama.
    /// </summary>
    /// <remarks>
    /// <para>Contoh pada tanggal acuan 20 Desember 2026: kontrak berakhir 25 Desember masuk <c>H7</c>, berakhir
    /// 1 Januari 2027 masuk <c>H14</c>, berakhir 10 Februari 2027 masuk <c>H60</c>, berakhir 25 Februari 2027 belum
    /// dihitung (lebih dari 60 hari), dan kontrak aktif yang berakhir 15 Desember masuk <c>OVERDUE</c>.</para>
    /// <para>Tanggal acuan adalah tanggal UTC, sama persis dengan perhitungan layar Kontrak sebelum refactor.
    /// Mengubahnya ke tanggal operasional akan menggeser angka layar Kontrak, sehingga butuh keputusan terpisah.</para>
    /// </remarks>
    public static class ContractRenewalBuckets
    {
        public const string All = "ALL";
        public const string H60 = "H60";
        public const string H14 = "H14";
        public const string H7 = "H7";
        public const string PendingDecision = "PENDING_DECISION";
        public const string Overdue = "OVERDUE";

        /// <summary>Jendela "kontrak segera berakhir" dalam hari (<c>ExpiringSoonCount</c>).</summary>
        public const int ExpiringWindowDays = 60;
        public const int EscalationDays = 14;
        public const int NoticeDays = 7;

        /// <summary>Tanggal acuan perhitungan tahapan (tanggal UTC), sama dengan layar Kontrak.</summary>
        public static DateTime ReferenceDate() => DateTime.UtcNow.Date;

        /// <summary>Kontrak berjalan yang berakhir dalam 60 hari, termasuk hari ini (<c>ExpiringSoonCount</c>, kelompok <c>ALL</c>).</summary>
        public static Expression<Func<WfpContractHistory, bool>> ExpiringWithinWindow(DateTime today)
        {
            var windowEnd = today.AddDays(ExpiringWindowDays);
            return c => !c.IsDelete && c.IsCurrent && c.EndDate.HasValue && c.EndDate.Value >= today && c.EndDate.Value <= windowEnd;
        }

        /// <summary>
        /// Predikat satu tahapan berbasis tanggal akhir kontrak berjalan: <c>H60</c> (15–60 hari), <c>H14</c> (8–14 hari),
        /// <c>H7</c> (0–7 hari).
        /// </summary>
        public static Expression<Func<WfpContractHistory, bool>> Stage(string stage, DateTime today)
        {
            var h60Date = today.AddDays(ExpiringWindowDays);
            var h14Date = today.AddDays(EscalationDays);
            var h7Date = today.AddDays(NoticeDays);

            return stage switch
            {
                H60 => c => !c.IsDelete && c.IsCurrent && c.EndDate.HasValue && c.EndDate.Value >= today
                            && c.EndDate.Value <= h60Date && c.EndDate.Value > h14Date,
                H14 => c => !c.IsDelete && c.IsCurrent && c.EndDate.HasValue && c.EndDate.Value >= today
                            && c.EndDate.Value <= h14Date && c.EndDate.Value > h7Date,
                H7 => c => !c.IsDelete && c.IsCurrent && c.EndDate.HasValue && c.EndDate.Value >= today
                           && c.EndDate.Value <= h7Date,
                _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, "Tahapan kontrak berbasis tanggal hanya H60, H14, atau H7.")
            };
        }

        /// <summary>Kontrak aktif/ditandatangani yang tanggal akhirnya sudah lewat tanpa keputusan (<c>OVERDUE</c>).</summary>
        public static Expression<Func<WfpContractHistory, bool>> OverduePredicate(DateTime today)
        {
            var signed = EmploymentLifecycleValueConstants.ContractStatusSigned;
            return c => !c.IsDelete && c.EndDate.HasValue && c.EndDate.Value < today
                        && (c.ContractStatus == "Active" || c.ContractStatus == signed);
        }

        /// <summary>Usulan perpanjangan yang sudah diajukan dan menunggu keputusan (<c>PENDING_DECISION</c>).</summary>
        public static Expression<Func<HrdContractDecision, bool>> PendingDecisionPredicate()
        {
            return d => !d.IsDelete && d.DecisionType == ContractDecisionType.Extend && d.DecisionStatus == ContractDecisionStatus.Submitted;
        }

        /// <summary>
        /// Tahapan berbasis tanggal untuk satu kontrak yang sudah dimuat: <c>H60</c>, <c>H14</c>, <c>H7</c>, <c>OVERDUE</c>,
        /// atau <c>null</c> bila di luar seluruh tahapan. Aturannya sama dengan predikat di atas.
        /// </summary>
        public static string? Classify(WfpContractHistory contract, DateTime today)
        {
            if (contract.IsDelete || !contract.EndDate.HasValue)
            {
                return null;
            }

            var end = contract.EndDate.Value;
            if (end < today)
            {
                var isActive = contract.ContractStatus == "Active"
                               || contract.ContractStatus == EmploymentLifecycleValueConstants.ContractStatusSigned;
                return isActive ? Overdue : null;
            }

            if (!contract.IsCurrent)
            {
                return null;
            }

            if (end <= today.AddDays(NoticeDays)) return H7;
            if (end <= today.AddDays(EscalationDays)) return H14;
            if (end <= today.AddDays(ExpiringWindowDays)) return H60;
            return null;
        }
    }
}
