using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Controllers;
using QuilvianSystemBackend.Attributes;

namespace QuilvianSystemBackend.Tests.OperatingRoom;

/// <summary>
/// Kontrak izin khusus endpoint laporan Operasi (`BE-OPR-010`).
/// </summary>
/// <remarks>
/// <para>
/// `PermissionMatrixTests` sudah menyapu seluruh controller Operasi dan memastikan setiap
/// endpoint menuntut login dan menuntut izin. Yang <b>tidak</b> dijamin sapuan itu adalah bahwa
/// tiap laporan menuntut izin atas sumber daya yang <b>benar</b> — dan di laporan itu persoalan
/// nyata: laporan material membawa nomor serial implant, keterangan yang tidak otomatis boleh
/// dibaca siapa pun yang berhak membaca daftar kasus operasi.
/// </para>
/// <para>
/// <b>Yang tidak diuji di sini, dan sebabnya.</b> Penolakan `403` runtime untuk peran tertentu
/// tidak dapat dibuktikan dari dalam proyek uji ini. Keputusannya diambil filter otorisasi atas
/// pemetaan peran-ke-izin milik lingkungan, dan pada lingkungan pengembangan pemeriksaan itu
/// justru dimatikan (`Security:Authorization:Enabled`). Memalsukan pemetaannya di dalam uji
/// hanya akan membuktikan bahwa tiruannya bekerja, bukan bahwa otorisasinya bekerja. Celah itu
/// dicatat sebagai gap pada `docs/module-blueprints/operations/MODULE-STATUS.md`, bukan ditutup
/// dengan uji yang terlihat hijau.
/// </para>
/// </remarks>
public class ReportPermissionTests
{
    private static readonly Type Controller = typeof(OperatingRoomReportController);

    private static MethodInfo Aksi(string nama) =>
        Controller.GetMethod(nama, BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Aksi {nama} tidak ditemukan.");

    private static (string Sumber, string Aksi) Izin(string namaAksi)
    {
        var izin = Aksi(namaAksi).GetCustomAttributes<AccessPermissionAttribute>().Single();

        // Argumen filternya [controllerName, actionName, deniedCode, deniedMessage].
        return ((string)izin.Arguments[0]!, (string)izin.Arguments[1]!);
    }

    [Fact]
    public void Controller_laporan_menuntut_login()
    {
        Assert.NotNull(Controller.GetCustomAttribute<AuthorizeAttribute>());
    }

    [Fact]
    public void Ketiga_laporan_terdeteksi_sebagai_endpoint()
    {
        // Menjaga uji di bawah dari kemungkinan lulus karena tidak menemukan apa pun.
        foreach (var nama in new[] { "GetOperations", "GetUtilization", "GetMaterials" })
        {
            Assert.NotEmpty(Aksi(nama).GetCustomAttributes<HttpMethodAttribute>());
        }
    }

    [Theory]
    [InlineData("GetOperations")]
    [InlineData("GetUtilization")]
    [InlineData("GetMaterials")]
    public void Setiap_laporan_menuntut_izin_baca_saja(string namaAksi)
    {
        // Laporan hanya membaca. Menuntut izin `Update` atau `Create` akan menutup laporan dari
        // peran yang seharusnya boleh membacanya; sebaliknya izin tulis yang ikut terpasang
        // memberi kesan laporan dapat mengubah data.
        var (_, aksi) = Izin(namaAksi);
        Assert.Equal("Read", aksi);
    }

    [Theory]
    [InlineData("GetOperations")]
    [InlineData("GetUtilization")]
    public void Laporan_kasus_dan_pemakaian_ruang_memakai_izin_kasus_operasi(string namaAksi)
    {
        var (sumber, _) = Izin(namaAksi);
        Assert.Equal("OperatingRoomCase", sumber);
    }

    [Fact]
    public void Laporan_material_memakai_izin_material_bukan_izin_kasus()
    {
        // Inilah pembedaan yang paling menentukan di berkas ini. Laporan material membawa nomor
        // bets dan nomor serial implant; menyamakan izinnya dengan izin daftar kasus akan
        // membuka jejak implant kepada setiap peran yang boleh membaca jadwal operasi.
        var (sumber, aksi) = Izin("GetMaterials");

        Assert.Equal("OperatingRoomMaterial", sumber);
        Assert.Equal("Read", aksi);
        Assert.NotEqual("OperatingRoomCase", sumber);
    }

    [Fact]
    public void Setiap_laporan_hanya_menuntut_satu_izin()
    {
        // Dua izin pada satu endpoint membuat pembacanya harus menebak apakah keduanya wajib
        // atau salah satu cukup — dan jawabannya tidak terlihat dari atributnya.
        foreach (var nama in new[] { "GetOperations", "GetUtilization", "GetMaterials" })
        {
            Assert.Single(Aksi(nama).GetCustomAttributes<AccessPermissionAttribute>());
        }
    }

    [Theory]
    [InlineData("GetOperations")]
    [InlineData("GetUtilization")]
    [InlineData("GetMaterials")]
    public void Setiap_laporan_mendaftarkan_aksi_izinnya_untuk_pemetaan_peran(string namaAksi)
    {
        // `AccessAction` itulah yang membuat izinnya dapat dipetakan ke peran oleh pengelola
        // lingkungan. Endpoint tanpa pendaftaran ini tidak akan pernah muncul pada layar
        // pengaturan hak akses, sehingga tidak dapat diberikan maupun dicabut.
        var daftar = Aksi(namaAksi).GetCustomAttributes<AccessActionAttribute>().ToList();

        Assert.NotEmpty(daftar);
        Assert.All(daftar, x => Assert.Equal("Read", x.AccessType));
    }

    [Fact]
    public void Tidak_ada_laporan_yang_dibebaskan_dari_pemeriksaan_izin()
    {
        // `AllowAnonymous` pada laporan berarti seluruh jadwal operasi dan jejak implant dapat
        // dibaca tanpa login.
        Assert.Null(Controller.GetCustomAttribute<AllowAnonymousAttribute>());

        foreach (var aksi in Controller.GetMethods(BindingFlags.Public | BindingFlags.Instance |
                                                   BindingFlags.DeclaredOnly)
                     .Where(x => x.GetCustomAttributes<HttpMethodAttribute>().Any()))
        {
            Assert.Null(aksi.GetCustomAttribute<AllowAnonymousAttribute>());
        }
    }
}
