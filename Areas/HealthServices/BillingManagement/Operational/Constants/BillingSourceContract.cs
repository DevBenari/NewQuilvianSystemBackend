namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants
{
    /// <summary>
    /// Daftar sumber klinis yang boleh mengirim milestone fact ke Billing, beserta effect type
    /// yang sah untuk masing-masing sumber.
    ///
    /// Daftar ini adalah gerbang kontrak, bukan sekadar konstanta. Sumber yang belum terdaftar
    /// ditolak dengan <c>BIL_SOURCE_INVALID</c> sebelum satu pun baris finansial ditulis,
    /// sehingga modul klinis tidak dapat membuka jalur charge dengan menebak nama context.
    ///
    /// Laboratory terdaftar sejak <c>RJ-BIL-BE-003</c>. Radiology sengaja belum terdaftar dan
    /// menjadi cakupan <c>RJ-BIL-BE-004</c>.
    /// </summary>
    public static class BillingSourceContract
    {
        public const string InternalTestSourceContext = "InternalTest";
        public const string InternalTestEffectType = "InternalTestCharge";

        public const string PrescriptionSourceContext = "Prescription";
        public const string PrescriptionChargeEffectType = "PrescriptionCharge";

        public const string ProcedureSourceContext = "Procedure";
        public const string ProcedureChargeEffectType = "ProcedureCharge";

        /// <summary>
        /// Sumber Laboratorium. Satu fakta diterbitkan per specimen/komponen pemeriksaan sesuai
        /// keputusan author <c>RJ-BIL-OQ-008</c>, bukan satu fakta untuk seluruh pesanan.
        /// </summary>
        public const string LaboratorySourceContext = "Laboratory";
        public const string LaboratoryChargeEffectType = "LaboratoryCharge";

        private static readonly IReadOnlyDictionary<string, string[]> AllowedEffectTypes =
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                [InternalTestSourceContext] = new[] { InternalTestEffectType },
                [PrescriptionSourceContext] = new[] { PrescriptionChargeEffectType },
                [ProcedureSourceContext] = new[] { ProcedureChargeEffectType },
                [LaboratorySourceContext] = new[] { LaboratoryChargeEffectType }
            };

        public static bool IsKnownSourceContext(string? sourceContext)
        {
            return !string.IsNullOrWhiteSpace(sourceContext) &&
                   AllowedEffectTypes.ContainsKey(sourceContext);
        }

        public static bool IsAllowedEffectType(string? sourceContext, string? effectType)
        {
            if (string.IsNullOrWhiteSpace(sourceContext) || string.IsNullOrWhiteSpace(effectType))
                return false;

            return AllowedEffectTypes.TryGetValue(sourceContext, out var allowed) &&
                   Array.IndexOf(allowed, effectType) >= 0;
        }
    }
}
