using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Options;

namespace QuilvianSystemBackend.Services.Features;

/// <summary>
/// Satu-satunya pintu status fitur BPJS (BPJS Kesehatan dan BPJS Ketenagakerjaan: iuran, kebijakan, kepesertaan, klaim COB berbasis SEP,
/// dan klaim JKK). Sumbernya satu saklar yang sudah ada, <c>Payroll:Policy:EnableBpjsInPayroll</c>, sehingga Payroll, HRD, Kesehatan
/// dan Keselamatan Kerja, serta Frontend (lewat <c>GET api/v1/system/features</c>) tidak pernah berbeda perilaku. Mengaktifkan kembali
/// fitur cukup dengan mengubah satu nilai konfigurasi; tidak ada data atau struktur database yang dihapus saat dinonaktifkan.
///
/// Saat nonaktif: tidak ada kalkulasi, kebijakan, kepesertaan, nomor klaim, atau formulir BPJS baru. Riwayat tetap dapat dibaca.
/// Pendaftaran pasien dengan penjamin BPJS dan antrean BPJS tidak termasuk (aturan dan konfigurasinya milik modul masing-masing).
/// </summary>
public interface IBpjsFeatureGate
{
    /// <summary>True bila fitur BPJS aktif. Bawaan false: BPJS tidak aktif kecuali dinyalakan secara eksplisit.</summary>
    bool IsEnabled { get; }

    /// <summary>Pesan baku untuk operasi BPJS baru yang ditolak karena fitur nonaktif.</summary>
    string DisabledMessage { get; }
}

/// <summary>
/// Dilempar saat operasi BPJS baru dicoba ketika fitur BPJS nonaktif. Turunan <see cref="InvalidOperationException"/> agar penangkap lama
/// tetap menganggapnya penolakan aturan (bukan galat sistem); controller yang menangkapnya lebih dulu mengubahnya menjadi HTTP 409.
/// </summary>
public sealed class BpjsFeatureDisabledException : InvalidOperationException
{
    public BpjsFeatureDisabledException(string message) : base(message)
    {
    }
}

public static class BpjsFeatureGateExtensions
{
    /// <summary>Menolak operasi BPJS baru dengan <see cref="BpjsFeatureDisabledException"/> bila fitur BPJS nonaktif.</summary>
    public static void EnsureEnabled(this IBpjsFeatureGate gate)
    {
        if (!gate.IsEnabled)
        {
            throw new BpjsFeatureDisabledException(gate.DisabledMessage);
        }
    }
}

public sealed class BpjsFeatureGate : IBpjsFeatureGate
{
    public const string Message =
        "Fitur BPJS dinonaktifkan sementara. Operasi baru yang berkaitan dengan BPJS Kesehatan dan BPJS Ketenagakerjaan tidak dapat dijalankan; "
        + "riwayat tetap tersimpan dan dapat dibaca.";

    private readonly PayrollPolicyOptions _options;

    public BpjsFeatureGate(IOptions<PayrollPolicyOptions> options)
    {
        _options = options?.Value ?? new PayrollPolicyOptions();
    }

    public bool IsEnabled => _options.EnableBpjsInPayroll;

    public string DisabledMessage => Message;
}
