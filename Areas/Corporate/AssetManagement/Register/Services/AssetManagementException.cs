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
    public const string HandoverNotRegistered = "AST-VAL-REG-020";
    public const string HandoverAssetLost = "AST-VAL-REG-021";
    public const string HandoverPurposeMismatch = "AST-VAL-REG-022";
    public const string HandoverTargetInvalid = "AST-VAL-REG-023";
    public const string ProcessLocked = "AST-VAL-REG-024";
    public const string HandoverRecipient = "AST-VAL-REG-025";
    public const string UnitInStockTake = "AST-VAL-REG-026";
    public const string HandoverNotFound = "AST-PLC-NOT-FOUND";
    public const string HandoverStatus = "AST-PLC-STATUS";
    public const string ConditionPhotoRequired = "AST-VAL-REG-030";
    public const string ConditionReportDecided = "AST-VAL-REG-031";
    public const string LossDocumentRequired = "AST-VAL-REG-032";
    public const string LossSelfApproval = "AST-VAL-REG-033";
    public const string ConditionAssetLost = "AST-VAL-REG-034";
    public const string ConditionReportOpen = "AST-VAL-REG-035";
    public const string ConditionReportNotFound = "AST-CND-NOT-FOUND";
    public const string ConditionRoute = "AST-CND-ROUTE";
    public const string ConditionSource = "AST-CND-SOURCE";
    public const string ConditionDocumentStatus = "AST-CND-STATUS";
    public const string AttachmentType = "AST-VAL-REG-070";
    public const string AttachmentTooLarge = "AST-VAL-REG-071";
    public const string AttachmentNotConfigured = "AST-VAL-REG-072";
    public const string AttachmentEmpty = "AST-ATT-EMPTY";
    public const string AttachmentNotFound = "AST-ATT-NOT-FOUND";
    public const string ProcessLockNotOwned = "AST-LOCK-NOT-OWNED";
    public const string UtilizationAssetIneligible = "AST-VAL-REG-040";
    public const string UtilizationDates = "AST-VAL-REG-041";
    public const string UtilizationAgreement = "AST-VAL-REG-042";
    public const string UtilizationType = "AST-VAL-REG-043";
    public const string UtilizationNotHospitalOwned = "AST-VAL-REG-044";
    public const string UtilizationApprover = "AST-VAL-REG-045";
    public const string UtilizationNotFound = "AST-UTL-NOT-FOUND";
    public const string UtilizationStatus = "AST-UTL-STATUS";
    public const string UtilizationRoute = "AST-UTL-ROUTE";
    public const string UtilizationOwner = "AST-UTL-OWNER";
    public const string DocumentFields = "AST-VAL-REG-080";
    public const string DocumentNotFound = "AST-DOC-NOT-FOUND";
    public const string DocumentTypeInactive = "AST-DOC-TYPE";
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
    public const string HandoverNotRegistered = "Hanya aset yang sudah terdaftar yang dapat diserahkan.";
    public const string HandoverAssetLost = "Aset yang berstatus hilang tidak dapat diserahkan.";
    public const string HandoverPurposeMismatch = "Aset ini sudah ditempatkan. Untuk memindahkan ke unit lain gunakan mutasi.";
    public const string HandoverInitialOnly = "Aset ini belum ditempatkan. Gunakan penempatan awal.";
    public const string HandoverTargetInvalid = "Ruangan atau penanggung jawab tidak sesuai dengan unit tujuan.";
    public const string HandoverRecipient = "Hanya penerima yang dituju yang dapat mengonfirmasi penyerahan ini.";
    public const string UnitInStockTake = "Unit ini sedang dalam inventarisasi. Penyerahan dapat dilakukan setelah inventarisasi selesai.";
    public const string HandoverNotFound = "Penyerahan tidak ditemukan.";
    public const string HandoverStatusFormat = "Penyerahan berstatus {0}, sehingga tidak dapat {1}.";
    public const string HandoverBatchSize = "Pilih 1 sampai 200 aset untuk diserahkan sekaligus.";
    public const string HandoverPurposeManual = "Pilih tujuan penempatan awal atau pindah dalam unit. Koreksi opname dan pascapemulihan dibuat otomatis oleh prosesnya.";
    public const string HandoverTargetRequired = "Unit, ruangan, dan penanggung jawab tujuan wajib diisi.";
    public const string HandoverNotes = "Catatan paling panjang 1000 karakter.";
    public const string HandoverOutsideScope = "Penyerahan ini bukan untuk unit Anda.";
    public const string ConditionPhotoRequired = "Lampirkan foto kondisi aset.";
    public const string ConditionReportDecided = "Laporan ini sudah diputuskan.";
    public const string LossDocumentRequired = "Status hilang wajib disertai berita acara kehilangan.";
    public const string LossSelfApproval = "Anda tidak dapat menyetujui laporan kehilangan yang Anda buat sendiri.";
    public const string ConditionAssetLost = "Aset ini berstatus hilang. Laporkan sebagai ditemukan kembali.";
    public const string ConditionReportOpen = "Aset ini masih punya laporan kondisi yang belum diputuskan.";
    public const string ConditionReportNotFound = "Laporan kondisi tidak ditemukan.";
    public const string ConditionReportOutsideScope = "Laporan kondisi ini bukan untuk unit Anda.";
    public const string ConditionDecideLossRoute = "Laporan hilang dan ditemukan kembali diputuskan lewat persetujuan Kepala Bagian Aset.";
    public const string ConditionApproveLossRoute = "Hanya laporan hilang atau ditemukan kembali yang disetujui lewat persetujuan kehilangan.";
    public const string ConditionDecideNotLost = "Kondisi hilang hanya dapat ditetapkan lewat persetujuan kehilangan.";
    public const string ConditionSource = "Pemeriksaan Bagian Aset hanya dapat dicatat oleh petugas yang berhak menetapkan kondisi.";
    public const string ConditionSourceManual = "Pilih sumber laporan unit atau pemeriksaan Bagian Aset. Pengembalian pinjaman dan tindak lanjut opname dibuat otomatis oleh prosesnya.";
    public const string ConditionRequired = "Pilih aset dan kondisi yang dilaporkan.";
    public const string ConditionFoundAgainOnlyLost = "Hanya aset berstatus hilang yang dapat dilaporkan ditemukan kembali.";
    public const string ConditionFoundAgainNotLost = "Aset yang ditemukan kembali dilaporkan dengan kondisi selain hilang.";
    public const string ConditionNotes = "Catatan paling panjang 1000 karakter.";
    public const string ConditionLossReportOnlyLost = "Berita acara kehilangan hanya dilampirkan pada laporan hilang.";
    public const string ConditionDocumentStatus = "Berita acara kehilangan tersedia setelah status hilang disetujui.";
    public const string ConditionAssetStatusFormat = "Aset berstatus {0}, sehingga kondisinya tidak dapat dilaporkan.";
    public const string UtilizationAssetIneligibleFormat = "Aset {0} tidak dapat dimanfaatkan karena {1}.";
    public const string UtilizationDates = "Tanggal berakhir harus sesudah tanggal mulai.";
    public const string UtilizationDatesRequired = "Tanggal mulai dan tanggal berakhir wajib diisi.";
    public const string UtilizationAgreement = "Nama mitra, nomor perjanjian, dan nilai perjanjian wajib diisi.";
    public const string UtilizationAgreementLength = "Nama mitra paling panjang 200 karakter, nomor perjanjian 100 karakter, dan tujuan 1000 karakter.";
    public const string UtilizationAgreementValue = "Nilai perjanjian tidak boleh negatif dan paling banyak 2 angka desimal.";
    public const string UtilizationType = "Pilih jenis pemanfaatan: sewa, kerja sama operasional, atau pinjam pakai.";
    public const string UtilizationNotHospitalOwnedFormat = "Aset {0} bukan milik rumah sakit, sehingga tidak dapat dimanfaatkan ke pihak lain.";
    public const string UtilizationApproverFormat = "Pemanfaatan ini harus disetujui {0}.";
    public const string UtilizationSelfDecision = "Anda tidak dapat memutuskan pengajuan pemanfaatan Anda sendiri.";
    public const string UtilizationForwardNotNeeded = "Pemanfaatan ini cukup disetujui Kepala Bagian Aset, sehingga tidak perlu diteruskan ke Direktur.";
    public const string UtilizationRejectAtDirector = "Pemanfaatan ini menunggu Direktur. Penolakan dilakukan lewat keputusan Direktur.";
    public const string UtilizationAssetCount = "Pilih 1 sampai 200 aset yang akan dimanfaatkan.";
    public const string UtilizationNotFound = "Pemanfaatan tidak ditemukan.";
    public const string UtilizationStatusFormat = "Pemanfaatan berstatus {0}, sehingga tidak dapat {1}.";
    public const string UtilizationOwner = "Hanya pembuat pengajuan yang dapat mengubah atau membatalkannya.";
    public const string UtilizationEndedBySchedule = "Masa pemanfaatan berakhir sesuai tanggal perjanjian.";
    public const string UtilizationAssetNotFound ="Sebagian aset yang dipilih tidak ditemukan atau sudah dihapus.";
    public const string DocumentFields = "Nomor dokumen wajib diisi; dokumen jenis ini wajib punya tanggal berakhir.";
    public const string DocumentFieldLength = "Nomor dokumen paling panjang 100 karakter, atas nama 200 karakter, dan catatan 500 karakter.";
    public const string DocumentDates = "Tanggal terbit tidak boleh sesudah tanggal berakhir.";
    public const string DocumentRequired = "Pilih aset dan jenis dokumen.";
    public const string DocumentNotFound = "Dokumen kepemilikan tidak ditemukan atau sudah dihapus.";
    public const string DocumentTypeInactive = "Pilih jenis dokumen yang masih aktif.";
    public const string DocumentAssetMismatch = "Aset pada dokumen tidak dapat diganti. Hapus dokumen ini lalu tambahkan pada aset yang benar.";
    public const string DocumentAssetStatusFormat = "Aset berstatus {0}, sehingga dokumen kepemilikannya tidak dapat diubah.";
}
