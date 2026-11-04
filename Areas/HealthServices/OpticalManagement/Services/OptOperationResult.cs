namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services
{
    /// <summary>Jenis hasil operasi service Optik; dipetakan ke kode HTTP oleh <c>OptHttp</c>.</summary>
    public enum OptResultKind
    {
        Success = 1,
        Validation = 2,
        Forbidden = 3,
        NotFound = 4,
        Conflict = 5,
        Unavailable = 6
    }

    /// <summary>
    /// Hasil operasi service Optik. Penolakan membawa kode stabil (<c>OPT-VAL-nnn</c>, <c>contracts/validation-matrix.md</c>)
    /// dan kalimat yang langsung ditampilkan ke pengguna. Polanya sama dengan <c>McuResult</c>.
    /// </summary>
    public sealed class OptResult<T>
    {
        private OptResult(OptResultKind kind)
        {
            Kind = kind;
        }

        public OptResultKind Kind { get; private init; }

        public T? Value { get; private init; }

        public string? Code { get; private init; }

        public string? Message { get; private init; }

        public bool IsSuccess => Kind == OptResultKind.Success;

        public static OptResult<T> Ok(T value) => new(OptResultKind.Success) { Value = value };

        public static OptResult<T> Invalid(string code, string message) =>
            new(OptResultKind.Validation) { Code = code, Message = message };

        public static OptResult<T> Forbidden(string code, string message) =>
            new(OptResultKind.Forbidden) { Code = code, Message = message };

        public static OptResult<T> NotFound(string message) =>
            new(OptResultKind.NotFound) { Code = OptErrorCodes.NotFound, Message = message };

        public static OptResult<T> Conflict(string code, string message) =>
            new(OptResultKind.Conflict) { Code = code, Message = message };

        /// <summary>Modul pendukung (HR/Billing/Farmasi) tidak menjawab; tindakan ditolak tanpa mengubah data (<c>503</c>).</summary>
        public static OptResult<T> Unavailable(string code, string message) =>
            new(OptResultKind.Unavailable) { Code = code, Message = message };
    }

    /// <summary>Kode galat Optik — <c>contracts/validation-matrix.md</c> (<c>OPT-VAL-v1</c>).</summary>
    public static class OptErrorCodes
    {
        public const string NotFound = "OPT-NOT-FOUND";
        public const string InvalidRequest = "OPT-INVALID-REQUEST";
        public const string ConcurrencyConflict = "OPT-CONCURRENCY";

        public const string OpticalUnitMissing = "OPT-UNIT-MISSING";

        // BE-OPT-033: jalur stok ke Farmasi.
        public const string OpticalDepotMissing = "OPT-DEPOT-MISSING";
        public const string StockRejected = "OPT-STOCK-REJECTED";
        public const string StockUnavailable = "OPT-STOCK-UNAVAILABLE";
        public const string StockForbidden = "OPT-STOCK-FORBIDDEN";
        public const string RegistrationRejected = "OPT-REG-REJECTED";
        public const string RegistrationForbidden = "OPT-REG-FORBIDDEN";
        public const string RegistrationConflict = "OPT-REG-CONFLICT";
        public const string RegistrationUnavailable = "OPT-REG-UNAVAILABLE";

        public const string InvalidState = "OPT-INVALID-STATE";
        public const string SettingMissing = "OPT-SETTING-MISSING";

        /// <summary>Dokumen sudah dikunci Rekam Medis (kunjungan ditutup) — <c>OPT-DEC-050</c>.</summary>
        public const string MedicalRecordLocked = "OPT-RM-LOCKED";

        /// <summary>Pendaftaran keutuhan Rekam Medis ditolak; tidak ada perubahan yang tersimpan.</summary>
        public const string MedicalRecordRegistrationFailed = "OPT-RM-REGISTRATION";

        public const string Val001 = "OPT-VAL-001";
        public const string Val002 = "OPT-VAL-002";
        public const string Val003 = "OPT-VAL-003";

        public const string Val004 = "OPT-VAL-004";
        public const string Val005 = "OPT-VAL-005";
        public const string Val010 = "OPT-VAL-010";
        public const string Val011 = "OPT-VAL-011";
        public const string Val012 = "OPT-VAL-012";
        public const string Val013 = "OPT-VAL-013";
        public const string Val014 = "OPT-VAL-014";
        public const string Val015 = "OPT-VAL-015";
        public const string Val016 = "OPT-VAL-016";
        public const string Val017 = "OPT-VAL-017";
        public const string Val018 = "OPT-VAL-018";
        public const string Val019 = "OPT-VAL-019";
        public const string Val020 = "OPT-VAL-020";
        public const string Val021 = "OPT-VAL-021";
        public const string Val022 = "OPT-VAL-022";
        public const string Val023 = "OPT-VAL-023";

        public const string Val030 = "OPT-VAL-030";
        public const string Val031 = "OPT-VAL-031";
        public const string Val032 = "OPT-VAL-032";
        public const string Val033 = "OPT-VAL-033";
        public const string Val034 = "OPT-VAL-034";

        // BE-OPT-032: pesanan (OPT-VAL-v1 bagian 4).
        public const string Val040 = "OPT-VAL-040";
        public const string Val041 = "OPT-VAL-041";
        public const string Val042 = "OPT-VAL-042";
        public const string Val043 = "OPT-VAL-043";
        public const string Val044 = "OPT-VAL-044";
        public const string Val045 = "OPT-VAL-045";
        // BE-OPT-034: kirim ke pengerjaan.
        public const string Val046 = "OPT-VAL-046";
        public const string Val047 = "OPT-VAL-047";
        public const string Val048 = "OPT-VAL-048";
        // BE-OPT-040: penyerahan.
        public const string Val050 = "OPT-VAL-050";
        public const string Val051 = "OPT-VAL-051";
        public const string Val052 = "OPT-VAL-052";
        // BE-OPT-041: batal sesudah pengerjaan dan tidak diambil.
        public const string Val054 = "OPT-VAL-054";
        public const string Val055 = "OPT-VAL-055";
        public const string Val056 = "OPT-VAL-056";
        // BE-OPT-050: keluhan garansi.
        public const string Val060 = "OPT-VAL-060";
        public const string Val061 = "OPT-VAL-061";
        public const string Val062 = "OPT-VAL-062";
        public const string Val063 = "OPT-VAL-063";
        public const string Val064 = "OPT-VAL-064";
        public const string Val053 = "OPT-VAL-053";
        public const string Val057 = "OPT-VAL-057";
        public const string Val058 = "OPT-VAL-058";

        /// <summary>Billing tidak menjawab saat menagihkan atau membaca status bayar pesanan (<c>503</c>).</summary>
        public const string BillingUnavailable = "OPT-BIL-UNAVAILABLE";

        /// <summary>Stok depo Optik kurang saat kirim/serah (<c>OPT-VAL-049</c>, <c>409</c>).</summary>
        public const string Val049 = "OPT-VAL-049";

        public const string Val070 = "OPT-VAL-070";
        public const string Val071 = "OPT-VAL-071";
        public const string Val072 = "OPT-VAL-072";
        public const string Val073 = "OPT-VAL-073";
    }
}
