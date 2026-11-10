using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Constants
{
    /// <summary>
    /// Definisi deret nomor dokumen Hubungan Industrial (Sub-modul 18).
    /// </summary>
    /// <remarks>
    /// Nomor dokumen resmi — laporan insiden, BAP, Surat Peringatan, skorsing, tiket keluhan,
    /// tiket whistleblower, register bipartit, dan nomor terminasi — sebelumnya dibentuk dari
    /// tanggal ditambah angka acak 3–4 digit (<c>Random.Shared.Next</c>), sehingga dua dokumen
    /// bisa bernomor sama dan urutannya tidak berurutan. Sejak remediasi 10 November 2026 seluruh
    /// bagian acak diganti nomor urut dari provider bersama <c>NumberSeriesAllocator</c>
    /// (<c>QBE-CODE-003</c>, <c>QBE-CODE-006</c>). Awalan dan penanda periode yang sudah dipakai
    /// dipertahankan; yang berubah hanya bagian akhirnya menjadi urut 4 digit.
    ///
    /// Contoh: <c>INC-202611-4821</c> (acak) menjadi <c>INC-202611-0001</c>, <c>INC-202611-0002</c>,
    /// dan seterusnya; surat peringatan <c>SP1/HRD/11/2026/482</c> menjadi
    /// <c>SP1/HRD/11/2026/0001</c>.
    /// </remarks>
    public static class EmployeeRelationNumberSeries
    {
        /// <summary>Jumlah digit nomor urut pada seluruh dokumen Hubungan Industrial.</summary>
        public const int SequenceDigits = 4;

        /// <summary>
        /// Aktor cadangan untuk stempel audit deret nomor bila identitas petugas tidak dapat
        /// dipetakan ke akun pengguna. Nilainya sama dengan cadangan yang sudah dipakai
        /// <c>EmployeeBenefitEnrollmentService</c>.
        /// </summary>
        public static readonly Guid SystemFallbackUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public static class Keys
        {
            public const string Incident = "HRD_ER_INCIDENT";
            public const string InvestigationBap = "HRD_ER_INVESTIGATION_BAP";
            public const string Sanction = "HRD_ER_SANCTION";
            public const string Suspension = "HRD_ER_SUSPENSION";
            public const string Grievance = "HRD_ER_GRIEVANCE";
            public const string Whistleblower = "HRD_ER_WHISTLEBLOWER";
            public const string Bipartite = "HRD_ER_BIPARTITE";
            public const string Separation = "HRD_ER_SEPARATION";
        }

        public static class Prefixes
        {
            public const string Incident = "INC";
            public const string InvestigationBap = "BAP-HRD";
            public const string Sanction = "SP";
            public const string Suspension = "SKORS";
            public const string Grievance = "GRV";
            public const string Whistleblower = "WB";
            public const string Bipartite = "BIP";
            public const string Separation = "SEP";
        }

        public static class ResetPolicies
        {
            public const string Incident = NumberSeriesResetPolicies.Monthly;
            public const string InvestigationBap = NumberSeriesResetPolicies.Yearly;
            public const string Sanction = NumberSeriesResetPolicies.Monthly;
            public const string Suspension = NumberSeriesResetPolicies.Monthly;
            public const string Grievance = NumberSeriesResetPolicies.Monthly;
            public const string Whistleblower = NumberSeriesResetPolicies.Monthly;
            public const string Bipartite = NumberSeriesResetPolicies.Yearly;
            public const string Separation = NumberSeriesResetPolicies.Monthly;
        }

        /// <summary>
        /// Mengambil nomor urut (bagian sesudah tanda hubung terakhir) dari nomor yang diterbitkan
        /// provider, untuk disusun ulang ke format surat resmi bergaris miring.
        /// </summary>
        public static string TrailingSequence(string allocatedNumber)
        {
            var index = allocatedNumber.LastIndexOf('-');
            return index >= 0 ? allocatedNumber[(index + 1)..] : allocatedNumber;
        }
    }
}
