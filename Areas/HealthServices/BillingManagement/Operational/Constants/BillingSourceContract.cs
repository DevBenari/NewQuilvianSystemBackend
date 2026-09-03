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
    /// Laboratory terdaftar sejak <c>RJ-BIL-BE-003</c>. Radiology terdaftar sejak
    /// <c>RJ-BIL-BE-004</c>, setelah <c>RJ-BIL-DEC-014</c> menunjuk pemilik modulnya dan
    /// menaikkan prefix <c>Rad</c> pada registry ke <c>ACTIVE</c>.
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

        /// <summary>
        /// Sumber Radiologi. Satu fakta diterbitkan per study, bukan per pesanan: pesanan yang
        /// sama dapat melahirkan beberapa study ketika terjadi pengulangan, dan masing-masing
        /// punya sebab serta kelayakan tagihnya sendiri.
        ///
        /// <c>RJ-BIL-GATE-DEC-004</c> menyatakan `Requested`, `Accepted`, `Scheduled`, dan
        /// pelepasan laporan **bukan** pemicu tagihan. Yang menerbitkan fakta ini hanyalah
        /// acquisition yang benar-benar dikerjakan dan menghasilkan citra yang dapat dipakai.
        /// </summary>
        public const string RadiologySourceContext = "Radiology";
        public const string RadiologyChargeEffectType = "RadiologyCharge";

        private static readonly IReadOnlyDictionary<string, string[]> AllowedEffectTypes =
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                [InternalTestSourceContext] = new[] { InternalTestEffectType },
                [PrescriptionSourceContext] = new[] { PrescriptionChargeEffectType },
                [ProcedureSourceContext] = new[] { ProcedureChargeEffectType },
                [LaboratorySourceContext] = new[] { LaboratoryChargeEffectType },
                [RadiologySourceContext] = new[] { RadiologyChargeEffectType }
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
