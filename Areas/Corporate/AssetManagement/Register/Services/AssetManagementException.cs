using Microsoft.AspNetCore.Http;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed class AssetManagementException : Exception
{
    public AssetManagementException(int statusCode, string code, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Code = code;
    }

    public int StatusCode { get; }

    public string Code { get; }

    public static AssetManagementException Invalid(string code, string message) =>
        new(StatusCodes.Status400BadRequest, code, message);

    public static AssetManagementException Forbidden(string code, string message) =>
        new(StatusCodes.Status403Forbidden, code, message);

    public static AssetManagementException NotFound(string code, string message) =>
        new(StatusCodes.Status404NotFound, code, message);

    public static AssetManagementException Conflict(string code, string message, Exception? innerException = null) =>
        new(StatusCodes.Status409Conflict, code, message, innerException);

    public static AssetManagementException TooLarge(string code, string message) =>
        new(StatusCodes.Status413PayloadTooLarge, code, message);

    public static AssetManagementException Unavailable(string code, string message) =>
        new(StatusCodes.Status503ServiceUnavailable, code, message);
}

public static class AssetRegisterErrorCodes
{
    public const string NumberAllocationFailed = "AST-VAL-REG-013";
    public const string VersionMismatch = "AST-VAL-REG-015";
    public const string OutsideUnit = "AST-VAL-REG-016";
    public const string ProcessLocked = "AST-VAL-REG-024";
    public const string AttachmentType = "AST-VAL-REG-070";
    public const string AttachmentTooLarge = "AST-VAL-REG-071";
    public const string AttachmentNotConfigured = "AST-VAL-REG-072";
    public const string AttachmentEmpty = "AST-ATT-EMPTY";
    public const string AttachmentNotFound = "AST-ATT-NOT-FOUND";
    public const string ProcessLockNotOwned = "AST-LOCK-NOT-OWNED";
}

public static class AssetRegisterMessages
{
    public const string NumberAllocationFailed = "Nomor inventaris belum dapat diterbitkan. Coba lagi beberapa saat.";
    public const string DocumentNumberAllocationFailed = "Nomor dokumen belum dapat diterbitkan. Coba lagi beberapa saat.";
    public const string VersionMismatch = "Data ini baru saja diubah orang lain. Muat ulang lalu coba lagi.";
    public const string OutsideUnit = "Aset ini bukan milik unit Anda.";
    public const string ProcessLockedFormat = "Aset {0} sedang dalam proses {1}. Selesaikan proses itu terlebih dahulu.";
    public const string ProcessLockNotOwnedFormat = "Aset {0} sedang dikunci oleh proses {1} yang lain.";
    public const string AttachmentType = "Jenis berkas tidak diizinkan.";
    public const string AttachmentTooLarge = "Berkas terlalu besar.";
    public const string AttachmentNotConfigured = "Unggahan belum dapat dipakai karena pengaturan lampiran belum diisi administrator.";
    public const string AttachmentEmpty = "Berkas wajib dilampirkan dan tidak boleh kosong.";
    public const string AttachmentNotFound = "Lampiran tidak ditemukan.";
}
