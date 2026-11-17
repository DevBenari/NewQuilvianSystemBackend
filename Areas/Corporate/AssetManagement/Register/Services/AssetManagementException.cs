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
    public const string AssetName = "AST-VAL-REG-001";
    public const string GroupInactive = "AST-VAL-REG-002";
    public const string OwnershipInactive = "AST-VAL-REG-003";
    public const string DonationReference = "AST-VAL-REG-004";
    public const string Dates = "AST-VAL-REG-005";
    public const string AcquisitionCost = "AST-VAL-REG-006";
    public const string MedicalAttributes = "AST-VAL-REG-007";
    public const string DraftQuantity = "AST-VAL-REG-008";
    public const string GoodsReceipt = "AST-VAL-REG-009";
    public const string ReasonRequired = "AST-VAL-REG-010";
    public const string GroupInactiveAtApproval = "AST-VAL-REG-011";
    public const string SelfApproval = "AST-VAL-REG-012";
    public const string DraftOnly = "AST-VAL-REG-014";
    public const string Field = "AST-REG-FIELD";
    public const string Status = "AST-REG-STATUS";
    public const string NotFound = "AST-REG-NOT-FOUND";
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
    public const string AssetName = "Nama aset wajib diisi, paling panjang 250 karakter.";
    public const string GroupInactive = "Pilih kelompok aset yang masih aktif.";
    public const string OwnershipInactive = "Pilih jenis kepemilikan yang masih aktif.";
    public const string DonationReference = "Aset hibah wajib mencantumkan nomor berita acara hibah.";
    public const string Dates = "Tanggal siap pakai tidak boleh sebelum tanggal perolehan dan tidak boleh di masa depan.";
    public const string AcquisitionCost = "Harga perolehan wajib lebih dari Rp 0 dengan paling banyak 2 angka desimal.";
    public const string MedicalAttributes = "Alat medis wajib diisi merek, tipe, nomor seri, produsen, dan nomor izin edar.";
    public const string DraftQuantity = "Jumlah unit yang dibuat sekaligus 1 sampai 200.";
    public const string GoodsReceipt = "Penerimaan barang yang dipilih tidak ditemukan atau sudah dibatalkan.";
    public const string ReasonRequired = "Alasan wajib diisi, paling panjang 500 karakter.";
    public const string GroupInactiveAtApproval = "Kelompok aset sudah tidak aktif. Kembalikan ke petugas untuk diganti.";
    public const string SelfApproval = "Anda tidak dapat menyetujui pengajuan Anda sendiri.";
    public const string DraftOnly = "Aset ini sudah diajukan atau terdaftar, sehingga tidak dapat diubah dari formulir draft.";
    public const string AcquisitionTypeRequired = "Pilih jenis perolehan: pembelian atau hibah masuk. Saldo awal hanya lewat impor data awal.";
    public const string StatusFormat = "Aset berstatus {0}, sehingga tidak dapat {1}.";
    public const string NotFound = "Aset tidak ditemukan atau sudah dihapus.";
    public const string BatchSize = "Pilih 1 sampai 200 aset untuk disetujui sekaligus.";
    public const string LabelBatchSize = "Pilih 1 sampai 200 aset untuk dicetak labelnya.";
    public const string InventoryNumberRequired = "Nomor inventaris wajib diisi, paling panjang 30 karakter.";
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
