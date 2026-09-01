namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Jenis hasil satu tindakan radiologi.
    ///
    /// <see cref="SafetyBlocked"/> dan <see cref="PolicyNotConfigured"/> sengaja dipisah dari
    /// <see cref="Validation"/>. Ketiganya sama-sama menolak permintaan, tetapi menuntut
    /// tindakan yang sama sekali berbeda: yang pertama menuntut petugas menyelesaikan gerbang
    /// keselamatannya, yang kedua menuntut admin menetapkan aturannya lebih dulu, dan yang
    /// ketiga menuntut permintaannya diperbaiki. Meleburnya menjadi satu <c>400</c> akan
    /// membuat petugas mencari kesalahan pada tempat yang salah — dan pada radiologi, mencari
    /// di tempat yang salah biasanya berakhir dengan mencari jalan pintas.
    /// </summary>
    public enum RadOperationResultKind
    {
        Success = 1,
        Validation = 2,
        NotFound = 3,
        Conflict = 4,
        SafetyBlocked = 5,
        PolicyNotConfigured = 6
    }

    /// <summary>
    /// Hasil satu tindakan radiologi beserta kode dan pesannya.
    ///
    /// Kode galat dipakai frontend untuk membedakan keadaan; pesannya untuk dibaca manusia.
    /// Keduanya wajib ada — kode tanpa pesan tidak dapat ditampilkan, pesan tanpa kode tidak
    /// dapat diperiksa oleh test.
    /// </summary>
    public sealed class RadOperationResult<T>
    {
        private RadOperationResult(RadOperationResultKind kind)
        {
            Kind = kind;
        }

        public RadOperationResultKind Kind { get; private init; }

        public T? Value { get; private init; }

        public string? ErrorCode { get; private init; }

        public string? ErrorMessage { get; private init; }

        public bool IsSuccess => Kind == RadOperationResultKind.Success;

        public static RadOperationResult<T> Success(T value) =>
            new(RadOperationResultKind.Success) { Value = value };

        public static RadOperationResult<T> Validation(string code, string message) =>
            new(RadOperationResultKind.Validation) { ErrorCode = code, ErrorMessage = message };

        public static RadOperationResult<T> NotFound(string code, string message) =>
            new(RadOperationResultKind.NotFound) { ErrorCode = code, ErrorMessage = message };

        public static RadOperationResult<T> Conflict(string code, string message) =>
            new(RadOperationResultKind.Conflict) { ErrorCode = code, ErrorMessage = message };

        public static RadOperationResult<T> SafetyBlocked(string code, string message) =>
            new(RadOperationResultKind.SafetyBlocked) { ErrorCode = code, ErrorMessage = message };

        public static RadOperationResult<T> PolicyNotConfigured(string code, string message) =>
            new(RadOperationResultKind.PolicyNotConfigured)
            {
                ErrorCode = code,
                ErrorMessage = message
            };
    }

    /// <summary>Kode galat radiologi yang dipakai lintas lapisan.</summary>
    public static class RadErrorCodes
    {
        public const string OrderNotFound = "RAD_ORDER_NOT_FOUND";
        public const string StudyNotFound = "RAD_STUDY_NOT_FOUND";
        public const string ModalityNotFound = "RAD_MODALITY_NOT_FOUND";
        public const string InvalidTransition = "RAD_INVALID_TRANSITION";
        public const string ConcurrencyConflict = "RAD_CONCURRENCY_CONFLICT";

        /// <summary>Identitas pasien, kunjungan, pemeriksaan, atau modalitas belum diverifikasi.</summary>
        public const string IdentityNotVerified = "RAD_IDENTITY_NOT_VERIFIED";

        /// <summary>Ada butir keselamatan wajib yang belum dijawab atau dijawab gagal.</summary>
        public const string SafetyGateNotCleared = "RAD_SAFETY_GATE_NOT_CLEARED";

        /// <summary>
        /// Belum ada satu pun aturan keselamatan aktif untuk modalitas ini.
        ///
        /// Ini **bukan** kesalahan petugas dan bukan kesalahan data pasien. Ia berarti kebijakan
        /// keselamatannya memang belum ditetapkan, dan sistem menolak melanjutkan karena
        /// meloloskan acquisition tanpa kebijakan adalah risiko yang tidak boleh diambil diam-diam.
        /// </summary>
        public const string SafetyPolicyNotConfigured = "RAD_SAFETY_POLICY_NOT_CONFIGURED";

        public const string StudyNotUsable = "RAD_STUDY_NOT_USABLE";
        public const string RepeatSourceInvalid = "RAD_REPEAT_SOURCE_INVALID";
        public const string RepeatAuthorizationRequired = "RAD_REPEAT_AUTHORIZATION_REQUIRED";
        public const string ReasonRequired = "RAD_REASON_REQUIRED";
    }
}
