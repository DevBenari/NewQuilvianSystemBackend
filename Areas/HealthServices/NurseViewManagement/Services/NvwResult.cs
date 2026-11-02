namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services
{
    public enum NvwResultKind
    {
        Success = 1,
        Created = 2,
        Validation = 3,
        Forbidden = 4,
        NotFound = 5,
        Conflict = 6,
        BusinessRule = 7
    }

    public sealed class NvwResult<T>
    {
        private NvwResult(NvwResultKind kind)
        {
            Kind = kind;
        }

        public NvwResultKind Kind { get; private init; }

        public T? Value { get; private init; }

        public string? Code { get; private init; }

        public string? Message { get; private init; }

        public bool IsSuccess => Kind is NvwResultKind.Success or NvwResultKind.Created;

        public static NvwResult<T> Ok(T value) => new(NvwResultKind.Success) { Value = value };

        public static NvwResult<T> Created(T value) => new(NvwResultKind.Created) { Value = value };

        public static NvwResult<T> Invalid(string code, string message) =>
            new(NvwResultKind.Validation) { Code = code, Message = message };

        public static NvwResult<T> Forbidden(string code, string message) =>
            new(NvwResultKind.Forbidden) { Code = code, Message = message };

        public static NvwResult<T> NotFound(string message) =>
            new(NvwResultKind.NotFound) { Code = NvwErrorCodes.NotFound, Message = message };

        public static NvwResult<T> Conflict(string code, string message) =>
            new(NvwResultKind.Conflict) { Code = code, Message = message };

        public static NvwResult<T> Rule(string code, string message) =>
            new(NvwResultKind.BusinessRule) { Code = code, Message = message };
    }

    public static class NvwErrorCodes
    {
        public const string NotFound = "NV-NOT-FOUND";
        public const string FloorUnknown = "VAL-NV-02";
        public const string VersionMismatch = "VAL-NV-04";
        public const string LoginAlreadyExists = "VAL-NV-05";
        public const string LoginNotCreated = "VAL-NV-06";
        public const string DeactivationReason = "VAL-NV-07";
        public const string AlreadyInactive = "VAL-NV-08";
        public const string CodeAllocationFailed = "NV-CODE-ALLOCATION";
        public const string LoginAccountConflict = "NV-LOGIN-ACCOUNT";
    }

    public static class NvwMessages
    {
        public const string DeviceNameRequired = "Nama perangkat wajib diisi, paling banyak 100 karakter.";
        public const string FloorUnknown = "Lantai tidak dikenal. Pilih lantai yang terdaftar pada data kamar.";
        public const string TooLong = "Isian terlalu panjang.";
        public const string VersionMismatch = "Data perangkat sudah diubah orang lain. Muat ulang lalu coba lagi.";
        public const string LoginAlreadyExists = "Perangkat ini sudah punya login. Gunakan Atur Ulang Login.";
        public const string LoginNotCreated = "Perangkat belum punya login. Gunakan Buat Login.";
        public const string DeactivationReason = "Alasan menonaktifkan wajib diisi (5–500 karakter).";
        public const string AlreadyInactive = "Perangkat sudah nonaktif.";
        public const string DeviceNotFound = "Perangkat Nurse View tidak ditemukan.";
        public const string CodeAllocationFailed = "Kode perangkat gagal diterbitkan. Coba lagi.";
        public const string LoginAccountConflict = "Akun login untuk kode perangkat ini sudah ada. Hubungi tim IT.";
    }
}
