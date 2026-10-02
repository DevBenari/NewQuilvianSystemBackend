using Microsoft.Extensions.Hosting;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Options;

/// <summary>
/// Melepas aturan pelaku klinis modul Operasi selama pengembangan, sehingga satu akun mana
/// pun dapat menjalankan seluruh alur tanpa perlu menyiapkan dokter, tim, dan tiga orang
/// pemberi sign-off lebih dulu.
/// </summary>
/// <remarks>
/// <para>
/// Yang dilepas ketika <c>OperatingRoom:RelaxClinicalRules</c> bernilai <c>true</c>:
/// </para>
/// <list type="bullet">
/// <item>dokter pemohon tidak harus sama dengan pengguna yang sedang login;</item>
/// <item>akun tidak harus tertaut ke data dokter maupun data tenaga;</item>
/// <item>tim tidak harus lengkap empat peran;</item>
/// <item>sign-off boleh diberikan siapa saja, tidak harus pemegang peran itu di tim;</item>
/// <item>operasi boleh dimulai siapa saja, tidak harus dokter bedah utama;</item>
/// <item>kesiapan tidak lagi menunggu consent, checklist, dan ketiga sign-off.</item>
/// </list>
/// <para>
/// Aturan-aturan itu ada untuk mencegah operasi salah pasien dan salah sisi, dan untuk
/// memastikan tiga profesi memeriksa kesiapan secara terpisah sebagaimana checklist
/// keselamatan bedah. Karena itu saklar ini <b>diabaikan di produksi</b> — nilainya tidak
/// sekadar diperingatkan, melainkan tidak berlaku sama sekali begitu lingkungannya
/// produksi.
/// </para>
/// <para>
/// Yang TIDAK dilepas: keharusan login, dan keutuhan data seperti kecocokan pasien dengan
/// kunjungan serta keberadaan tindakan bedahnya. Melepas itu bukan melonggarkan aturan,
/// melainkan membuat data yang tersimpan menjadi tidak masuk akal.
/// </para>
/// </remarks>
public sealed class OperatingRoomRuleRelaxation
{
    public OperatingRoomRuleRelaxation(IConfiguration configuration, IHostEnvironment environment)
    {
        IsRelaxed =
            configuration.GetValue("OperatingRoom:RelaxClinicalRules", false) &&
            !environment.IsProduction();
    }

    /// <summary>Benar bila aturan pelaku klinis sedang dilepas.</summary>
    public bool IsRelaxed { get; }
}
