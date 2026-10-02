using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Hasil penilaian gerbang keselamatan sebuah study.
    /// </summary>
    public sealed record RadSafetyGateOutcome(
        bool PolicyConfigured,
        bool Cleared,
        IReadOnlyList<string> PendingMandatoryCodes,
        IReadOnlyList<string> FailedMandatoryCodes,
        int RuleVersion);

    /// <summary>
    /// Penilaian gerbang keselamatan radiologi.
    ///
    /// Seluruh isinya fungsi murni dan <c>static</c>: tidak menyentuh database, tidak menyentuh
    /// waktu sistem, dan tidak menyentuh identitas pemanggil. Itu disengaja. Aturan ini adalah
    /// satu-satunya hal yang berdiri antara sebuah permintaan dan penyinaran seorang pasien,
    /// sehingga ia harus dapat diuji secara langsung, tanpa database dan tanpa perancah.
    ///
    /// Tiga keadaan dibedakan, dan ketiganya berbeda maknanya:
    ///
    /// <list type="bullet">
    /// <item><b>Kebijakan belum ada.</b> Tidak satu pun aturan berstatus
    /// <c>RadSafetyRuleStatus.Active</c> untuk modalitas ini. Acquisition <b>ditolak</b>. Ini
    /// perilaku fail-closed yang dituntut <c>RJ-BIL-DEC-014</c>: tidak adanya aturan berarti
    /// belum ada yang menetapkan apa yang aman, bukan berarti semuanya aman.</item>
    /// <item><b>Kebijakan ada, butir wajib belum tuntas.</b> Acquisition ditolak sampai seluruh
    /// butir wajib berkeadaan <c>Passed</c> atau <c>NotApplicable</c>.</item>
    /// <item><b>Kebijakan ada dan seluruh butir wajib tuntas.</b> Acquisition boleh berjalan.</item>
    /// </list>
    ///
    /// Butir <b>tidak wajib</b> yang berkeadaan <c>Failed</c> sengaja <b>tidak</b> memblokir.
    /// Menjadikannya pemblokir akan menghapus perbedaan antara wajib dan tidak wajib, dan
    /// membuat admin memilih menandai semuanya tidak wajib supaya pekerjaan tetap berjalan —
    /// yang justru melemahkan gerbangnya secara keseluruhan.
    ///
    /// Sejak <c>RAD-DEC-005</c>, aturan yang belum disahkan tidak ikut dinilai sama sekali.
    /// Penyaringnya ditegakkan dua kali — pada query pemuat aturan dan sekali lagi di sini —
    /// karena kedua kekeliruan yang mungkin terjadi tidak setara: aturan sah yang tersaring
    /// keluar berakhir sebagai penolakan yang terlihat, sedangkan draf yang lolos masuk
    /// berakhir sebagai pasien yang disinari atas dasar aturan yang belum disetujui siapa pun.
    /// </summary>
    public static class RadSafetyGateEvaluator
    {
        /// <summary>
        /// Keadaan yang dianggap menuntaskan sebuah butir keselamatan.
        ///
        /// <c>NotApplicable</c> ikut menuntaskan karena butir yang memang tidak berlaku tidak
        /// dapat dijawab. Jejaknya tetap dibedakan dari <c>Passed</c> pada baris pemeriksaannya.
        /// </summary>
        public static bool IsSettled(RadSafetyCheckState state) =>
            state == RadSafetyCheckState.Passed || state == RadSafetyCheckState.NotApplicable;

        /// <summary>
        /// Aturan yang boleh ikut menentukan boleh-tidaknya seorang pasien disinari.
        ///
        /// Hanya <c>Active</c> yang boleh. <c>Draft</c> dan <c>PendingApproval</c> belum
        /// disahkan penanggung jawab klinis, sedangkan <c>Inactive</c> sudah dihentikan;
        /// ketiganya sama-sama bukan kebijakan yang berlaku hari ini.
        /// </summary>
        public static bool IsEnforceable(RadSafetyRuleStatus status) =>
            status == RadSafetyRuleStatus.Active;

        /// <summary>
        /// Menilai apakah sebuah study boleh melanjutkan ke acquisition.
        /// </summary>
        /// <param name="applicableRules">
        /// Aturan yang berlaku untuk modalitas dan pemeriksaan study ini. Aturan yang belum
        /// disahkan boleh ikut terkirim: penyaringnya ditegakkan lagi di sini, sehingga
        /// pemanggil yang lupa menyaring berakhir pada penolakan, bukan pada kelolosan.
        /// </param>
        /// <param name="checks">Baris pemeriksaan keselamatan milik study tersebut.</param>
        public static RadSafetyGateOutcome Evaluate(
            IReadOnlyCollection<MstRadModalitySafetyRule> applicableRules,
            IReadOnlyCollection<RadStudySafetyCheck> checks)
        {
            var safeChecks = checks ?? Array.Empty<RadStudySafetyCheck>();

            // Aturan yang belum disahkan disingkirkan lebih dulu, sebelum apa pun dihitung.
            // Akibatnya modalitas yang hanya punya draf terbaca sebagai "kebijakan belum
            // ditetapkan" — persis keadaan modalitas yang memang belum punya aturan sama sekali,
            // dan memang begitulah seharusnya: draf bukan kebijakan.
            var rules = (applicableRules ?? Array.Empty<MstRadModalitySafetyRule>())
                .Where(x => IsEnforceable(x.RuleStatus))
                .ToList();

            if (rules.Count == 0)
            {
                return new RadSafetyGateOutcome(
                    PolicyConfigured: false,
                    Cleared: false,
                    PendingMandatoryCodes: Array.Empty<string>(),
                    FailedMandatoryCodes: Array.Empty<string>(),
                    RuleVersion: 0);
            }

            var checkByRequirement = safeChecks
                .Where(x => !x.IsDelete)
                .GroupBy(x => x.SafetyRequirementId)
                .ToDictionary(g => g.Key, g => g.First());

            var pending = new List<string>();
            var failed = new List<string>();

            foreach (var rule in rules.Where(x => x.IsMandatory))
            {
                var code = rule.SafetyRequirement?.RequirementCode ?? rule.SafetyRequirementId.ToString();

                if (!checkByRequirement.TryGetValue(rule.SafetyRequirementId, out var check))
                {
                    // Butir wajib yang tidak punya baris pemeriksaan sama sekali diperlakukan
                    // sebagai belum dijawab, bukan sebagai lolos. Ketiadaan jawaban bukan jawaban.
                    pending.Add(code);
                    continue;
                }

                if (check.CheckState == RadSafetyCheckState.Failed)
                {
                    failed.Add(code);
                    continue;
                }

                if (!IsSettled(check.CheckState))
                {
                    pending.Add(code);
                }
            }

            // Versi aturan tertinggi yang berlaku dibekukan pada study ketika ia dinyatakan
            // lolos, sehingga perubahan aturan berikutnya tidak menulis ulang penilaian ini.
            var ruleVersion = rules.Max(x => x.RuleVersion);

            return new RadSafetyGateOutcome(
                PolicyConfigured: true,
                Cleared: pending.Count == 0 && failed.Count == 0,
                PendingMandatoryCodes: pending,
                FailedMandatoryCodes: failed,
                RuleVersion: ruleVersion);
        }

        /// <summary>
        /// Menyusun pesan yang menyebut butir mana yang menahan, bukan sekadar "gagal".
        ///
        /// Petugas yang tahu butir mana yang kurang dapat menyelesaikannya. Petugas yang hanya
        /// diberi tahu "tidak boleh" akan mencari jalan lain.
        /// </summary>
        public static string DescribeBlockage(RadSafetyGateOutcome outcome)
        {
            if (!outcome.PolicyConfigured)
            {
                return "Aturan keselamatan untuk modalitas ini belum ditetapkan, sehingga " +
                       "acquisition tidak dapat dijalankan. Hubungi admin Radiologi untuk " +
                       "menetapkan aturannya lebih dulu.";
            }

            var bagian = new List<string>();

            if (outcome.PendingMandatoryCodes.Count > 0)
            {
                bagian.Add(
                    $"belum dijawab: {string.Join(", ", outcome.PendingMandatoryCodes)}");
            }

            if (outcome.FailedMandatoryCodes.Count > 0)
            {
                bagian.Add(
                    $"dinyatakan tidak aman: {string.Join(", ", outcome.FailedMandatoryCodes)}");
            }

            return bagian.Count == 0
                ? "Gerbang keselamatan belum tuntas."
                : $"Gerbang keselamatan wajib {string.Join("; ", bagian)}.";
        }
    }
}
