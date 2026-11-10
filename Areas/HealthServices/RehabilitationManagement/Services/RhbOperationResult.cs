namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services
{
    /// <summary>
    /// Jenis hasil tindakan service Rehab Medik. Dipetakan ke kode HTTP oleh <c>RhbHttp</c>
    /// sesuai <c>contracts/api-contract.md</c> bagian Kode status.
    /// </summary>
    public enum RhbResultKind
    {
        Success = 1,
        Created = 2,
        Validation = 3,
        Forbidden = 4,
        NotFound = 5,
        Conflict = 6,
        BusinessRule = 7
    }

    /// <summary>
    /// Hasil satu tindakan beserta kode aturan dan pesannya — pola <c>HmdResult</c>.
    /// </summary>
    /// <remarks>
    /// <see cref="Code"/> berisi kode <c>validation-matrix.md</c>, misalnya <c>RHB-VAL-070</c>.
    /// <see cref="Fields"/> berisi nama field request yang ditolak (camelCase), supaya layar dapat
    /// menandai field itu — bentuk <c>{ Code, Fields }</c> pada <c>validation-matrix.md</c>.
    /// <see cref="Details"/> hanya untuk rincian yang dibutuhkan layar (misalnya daftar kandidat
    /// duplikasi) dan tidak pernah memuat isi klinis.
    /// </remarks>
    public sealed class RhbResult<T>
    {
        private RhbResult(RhbResultKind kind)
        {
            Kind = kind;
        }

        public RhbResultKind Kind { get; private init; }

        public T? Value { get; private init; }

        public string? Code { get; private init; }

        public string? Message { get; private init; }

        public object? Details { get; private init; }

        public IReadOnlyList<string> Fields { get; private init; } = Array.Empty<string>();

        public bool IsSuccess => Kind is RhbResultKind.Success or RhbResultKind.Created;

        public static RhbResult<T> Ok(T value) => new(RhbResultKind.Success) { Value = value };

        public static RhbResult<T> Created(T value) => new(RhbResultKind.Created) { Value = value };

        public static RhbResult<T> Invalid(string code, string message, object? details = null) =>
            new(RhbResultKind.Validation) { Code = code, Message = message, Details = details };

        public static RhbResult<T> Forbidden(string code, string message) =>
            new(RhbResultKind.Forbidden) { Code = code, Message = message };

        public static RhbResult<T> NotFound(string message) =>
            new(RhbResultKind.NotFound) { Message = message };

        public static RhbResult<T> Conflict(string code, string message, object? details = null) =>
            new(RhbResultKind.Conflict) { Code = code, Message = message, Details = details };

        public static RhbResult<T> Rule(string code, string message, object? details = null) =>
            new(RhbResultKind.BusinessRule) { Code = code, Message = message, Details = details };

        /// <summary>Salinan hasil penolakan ini yang menyebut field request penyebabnya.</summary>
        public RhbResult<T> WithFields(params string[] fields) =>
            new(Kind) { Value = Value, Code = Code, Message = Message, Details = Details, Fields = fields };
    }
}
