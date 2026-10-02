using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Seeders;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.OperatingRoom.Infrastructure;

/// <summary>
/// Menyiapkan data dasar modul Operasi di basis data uji dengan memanggil seeder demo milik
/// modul itu sendiri.
/// </summary>
/// <remarks>
/// Memakai seeder yang sama dengan aplikasi, bukan menyusun ulang datanya di sini, punya dua
/// akibat yang dikehendaki. Pertama, uji tidak pernah mengarang bentuk data yang berbeda dari
/// yang dipakai sungguhan. Kedua, seeder itu sendiri ikut teruji: bila susunan tenaga, dokter,
/// atau tautan akunnya berubah dan menjadi tidak konsisten, uji di sini yang gagal lebih dulu.
/// </remarks>
public sealed class OperatingRoomSeed
{
    public static readonly Guid PemilikLingkungan = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid AkunBedah = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid AkunAnestesi = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid AkunPerawat = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid AkunLuarTim = Guid.Parse("55555555-5555-5555-5555-555555555555");

    public OperatingRoomDemoSeedResult Hasil { get; private set; } = new();

    public Guid PasienId => Hasil.PatientId;
    public Guid KunjunganId => Hasil.EncounterId;
    public Guid RuangId => Hasil.RoomId;
    public Guid UnitTujuanId => Hasil.DestinationUnitId;
    public Guid DokterBedahId => Hasil.DemoSurgeonDoctorId;
    public Guid TenagaBedahId => Hasil.DemoSurgeonWorkforceId;
    public Guid TenagaAnestesiId => Hasil.TeamWorkforceIds[0];
    public Guid TenagaPerawatInstrumenId => Hasil.TeamWorkforceIds[1];
    public Guid TenagaPerawatSirkulerId => Hasil.TeamWorkforceIds[2];
    public IReadOnlyList<Guid> TindakanIds => Hasil.PatientProcedureIds;

    public static async Task<OperatingRoomSeed> BuatAsync(OperatingRoomHarness harness)
    {
        var seed = new OperatingRoomSeed();
        await using var db = harness.NewContext();

        db.Users.Add(new ApplicationUser
        {
            Id = PemilikLingkungan,
            UserName = "pemilik.lingkungan",
            NormalizedUserName = "PEMILIK.LINGKUNGAN",
            UserCode = "UJI-OWNER",
            DisplayName = "Pemilik Lingkungan Uji",
            Email = "pemilik@contoh.invalid",
            NormalizedEmail = "PEMILIK@CONTOH.INVALID",
            UserType = UserType.SuperAdmin,
            IsActive = true,
            MustChangePassword = false
        });
        await db.SaveChangesAsync();

        seed.Hasil = await OperatingRoomDemoSeeder.SeedAsync(db, "Development", "pemilik.lingkungan");

        if (!string.IsNullOrWhiteSpace(seed.Hasil.RefusedReason))
            throw new InvalidOperationException("Seeder demo Operasi menolak berjalan: " + seed.Hasil.RefusedReason);

        // Tiga akun peran dibuat langsung, bukan lewat UserManager, karena yang diuji di sini
        // adalah kewenangan klinis dan bukan pendaftaran identitas. Tautan tenaga dan dokternya
        // mengikuti apa yang dihasilkan seeder, sehingga tetap satu sumber kebenaran.
        db.Users.AddRange(
            Akun(AkunBedah, "uji.bedah", seed.Hasil.DemoSurgeonWorkforceId, seed.Hasil.DemoSurgeonDoctorId),
            Akun(AkunAnestesi, "uji.anestesi", seed.Hasil.TeamWorkforceIds[0], null),
            Akun(AkunPerawat, "uji.perawat", seed.Hasil.TeamWorkforceIds[1], null),
            Akun(AkunLuarTim, "uji.luar", null, null));
        await db.SaveChangesAsync();

        return seed;
    }

    private static ApplicationUser Akun(Guid id, string nama, Guid? tenagaId, Guid? dokterId) => new()
    {
        Id = id,
        UserName = nama,
        NormalizedUserName = nama.ToUpperInvariant(),
        UserCode = "UJI-" + nama,
        DisplayName = nama,
        Email = nama + "@contoh.invalid",
        NormalizedEmail = (nama + "@contoh.invalid").ToUpperInvariant(),
        UserType = UserType.SuperAdmin,
        WorkforceProfileId = tenagaId,
        DoctorId = dokterId,
        IsActive = true,
        MustChangePassword = false
    };

    /// <summary>Tindakan bedah yang belum terpakai kasus mana pun.</summary>
    public async Task<Guid> TindakanBelumTerpakaiAsync(ApplicationDbContext db)
    {
        var terpakai = await db.OprCaseProcedures.AsNoTracking()
            .Where(x => !x.IsDelete).Select(x => x.PatientProcedureId).ToListAsync();
        var bebas = TindakanIds.FirstOrDefault(x => !terpakai.Contains(x));
        if (bebas == Guid.Empty)
            throw new InvalidOperationException(
                "Seeder demo tidak menyediakan tindakan bedah yang belum terpakai.");
        return bebas;
    }
}
