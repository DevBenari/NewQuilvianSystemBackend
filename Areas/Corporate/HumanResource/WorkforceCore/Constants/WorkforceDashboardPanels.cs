using System;
using System.Collections.Generic;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Constants
{
    /// <summary>
    /// Kode panel, kelompok (<c>bucket</c>), dan jenis dokumen Dashboard SDM (state-transition v30 bagian 34.1,
    /// <c>VR-PKB-01</c>, <c>VR-PKB-02</c>). Klasifikasi ini dihitung saat dibaca dan tidak disimpan.
    /// </summary>
    public static class WorkforceDashboardPanels
    {
        public const string EmployeeOverview = "EMPLOYEE_OVERVIEW";
        public const string Profession = "PROFESSION";
        public const string ActiveStatus = "ACTIVE_STATUS";
        public const string ContractExpiry = "CONTRACT_EXPIRY";
        public const string DocumentExpiry = "DOCUMENT_EXPIRY";

        // EMPLOYEE_OVERVIEW / ACTIVE_STATUS
        public const string BucketActive = "ACTIVE";
        public const string BucketInactive = "INACTIVE";
        public const string BucketNewThisMonth = "NEW_THIS_MONTH";
        public const string BucketLeftThisMonth = "LEFT_THIS_MONTH";

        // PROFESSION: bucket = professionId (GUID) atau UNASSIGNED
        public const string BucketUnassigned = "UNASSIGNED";

        // DOCUMENT_EXPIRY
        public const string BucketExpired = "EXPIRED";
        public const string BucketWithin30 = "WITHIN_30";
        public const string BucketDays31To60 = "DAYS_31_60";
        public const string BucketDays61To90 = "DAYS_61_90";

        public const string DocumentSipStr = "SIP_STR";
        public const string DocumentSpkRkk = "SPK_RKK";
        public const string DocumentMandatoryCertificate = "MANDATORY_CERTIFICATE";

        public const string ProfessionUnassignedLabel = "Belum diisi";

        public static readonly IReadOnlyList<string> Panels = new[]
        {
            EmployeeOverview, Profession, ActiveStatus, ContractExpiry, DocumentExpiry
        };

        public static readonly IReadOnlyList<string> DocumentTypes = new[]
        {
            DocumentSipStr, DocumentSpkRkk, DocumentMandatoryCertificate
        };

        public static readonly IReadOnlyList<string> DocumentBuckets = new[]
        {
            BucketExpired, BucketWithin30, BucketDays31To60, BucketDays61To90
        };

        public static readonly IReadOnlyList<string> ContractBuckets = new[]
        {
            ContractRenewalBuckets.All,
            ContractRenewalBuckets.H60,
            ContractRenewalBuckets.H14,
            ContractRenewalBuckets.H7,
            ContractRenewalBuckets.PendingDecision,
            ContractRenewalBuckets.Overdue
        };

        /// <summary>
        /// Kelompok sah untuk sebuah panel. Untuk <see cref="Profession"/> kelompoknya adalah GUID profesi atau
        /// <see cref="BucketUnassigned"/>, sehingga diperiksa terpisah.
        /// </summary>
        public static bool IsValidBucket(string panel, string bucket) => panel switch
        {
            EmployeeOverview => bucket is BucketActive or BucketNewThisMonth or BucketLeftThisMonth,
            ActiveStatus => bucket is BucketActive or BucketInactive,
            ContractExpiry => Contains(ContractBuckets, bucket),
            DocumentExpiry => Contains(DocumentBuckets, bucket),
            Profession => bucket == BucketUnassigned || Guid.TryParse(bucket, out _),
            _ => false
        };

        /// <summary>
        /// Kelompok dokumen dari sisa hari terhadap tanggal acuan (kamus data v30 bagian 31.2): ≤ 0 = <c>EXPIRED</c>,
        /// 1–30, 31–60, 61–90; di luar 90 hari = <c>null</c>.
        /// </summary>
        public static string? DocumentBucketFromDays(int daysRemaining)
        {
            if (daysRemaining <= 0) return BucketExpired;
            if (daysRemaining <= 30) return BucketWithin30;
            if (daysRemaining <= 60) return BucketDays31To60;
            if (daysRemaining <= 90) return BucketDays61To90;
            return null;
        }

        private static bool Contains(IReadOnlyList<string> values, string value)
        {
            foreach (var v in values)
            {
                if (v == value) return true;
            }

            return false;
        }
    }
}
