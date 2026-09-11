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
        PolicyNotConfigured = 6,

        /// <summary>
        /// Pelakunya memang tidak boleh melakukan tindakan itu — bukan karena isian yang salah
        /// dan bukan karena keadaan data.
        ///
        /// Dipisahkan dari <see cref="Validation"/> karena tidak ada satu pun perbaikan isian
        /// yang dapat menolong: yang salah adalah siapa yang menekan tombolnya. Contoh yang
        /// menjadi alasan keberadaannya, dari <c>RAD-DEC-005</c>: admin yang menyusun sebuah
        /// aturan keselamatan tidak boleh mengesahkan aturannya sendiri.
        /// </summary>
        Forbidden = 7,

        /// <summary>
        /// Bentuk permintaannya benar, tetapi aturan bisnis menolaknya — <c>422</c> pada
        /// <c>RAD-API-001</c> bagian 4.
        ///
        /// Contohnya menyusun aturan keselamatan yang menunjuk alat yang sudah dipensiunkan.
        /// Tidak ada yang salah pada isiannya; yang salah adalah keadaan data yang dirujuknya.
        /// </summary>
        BusinessRule = 8
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

        public static RadOperationResult<T> Forbidden(string code, string message) =>
            new(RadOperationResultKind.Forbidden) { ErrorCode = code, ErrorMessage = message };

        public static RadOperationResult<T> BusinessRule(string code, string message) =>
            new(RadOperationResultKind.BusinessRule) { ErrorCode = code, ErrorMessage = message };

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

        /* ---------------------------------------------------------------- *
         * Siklus pengesahan aturan keselamatan — RAD-DEC-005
         * ---------------------------------------------------------------- */

        public const string SafetyRuleNotFound = "RAD_SAFETY_RULE_NOT_FOUND";
        public const string SafetyRequirementNotFound = "RAD_SAFETY_REQUIREMENT_NOT_FOUND";
        public const string ProcedureNotFound = "RAD_PROCEDURE_NOT_FOUND";

        /// <summary>Isian permintaan belum lengkap atau bentuknya salah.</summary>
        public const string ValidationFailed = "RAD_VALIDATION_FAILED";

        /// <summary>Alat atau butir keselamatan yang dirujuk sudah tidak aktif.</summary>
        public const string MasterDataInactive = "RAD_MASTER_DATA_INACTIVE";

        /// <summary>Aturan berada pada keadaan yang tidak lagi boleh diubah.</summary>
        public const string SafetyRuleNotEditable = "RAD_SAFETY_RULE_NOT_EDITABLE";

        /// <summary>
        /// Penyusun aturan mencoba mengesahkan aturannya sendiri.
        ///
        /// Inilah pengaman yang tidak dapat ditegakkan sistem izin: memegang
        /// <c>RadSafetyRule : Approve</c> menjawab "boleh mengesahkan", bukan "boleh mengesahkan
        /// yang ini". Yang membedakan hanya siapa yang menyusun baris itu sebelumnya.
        /// </summary>
        public const string SelfApprovalNotAllowed = "RAD_SELF_APPROVAL_NOT_ALLOWED";

        /// <summary>Sudah ada aturan berlaku untuk kombinasi alat, pemeriksaan, dan butir yang sama.</summary>
        public const string ActiveSafetyRuleExists = "RAD_ACTIVE_SAFETY_RULE_EXISTS";
    }
}
