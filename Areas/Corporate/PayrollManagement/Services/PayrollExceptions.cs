namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>Aksi tidak sah untuk status saat ini (HTTP 400, standar transaksi bagian 5.2).</summary>
public sealed class PayrollInvalidTransitionException(string message) : Exception(message);

/// <summary>Aktor tidak berwenang atas aksi itu, mis. pemisahan kewenangan pengaju dan pengesah (HTTP 403).</summary>
public sealed class PayrollForbiddenActionException(string message) : Exception(message);

/// <summary>Data diubah pihak lain lebih dulu; token versi yang dikirim sudah usang (HTTP 409).</summary>
public sealed class PayrollVersionConflictException(string message) : Exception(message);

/// <summary>
/// Kebijakan terkait belum disahkan pemilik proses sehingga otomatisasi finansialnya sengaja belum diaktifkan (NEEDS CONFIRMATION).
/// Bukan kesalahan data: bagian lain yang independen tetap berjalan (HTTP 409 dengan pesan yang menjelaskan keputusan yang hilang).
/// </summary>
public sealed class PayrollPolicyNotConfirmedException(string message) : Exception(message);
