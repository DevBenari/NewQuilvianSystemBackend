using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.RegistrationManagement;

/// <summary>
/// Bukti untuk <c>BE-EXT-03</c> — penunjuk perujuk pada kunjungan pasien
/// (<c>LAB-DEC-035</c>, <c>LAB-COORD-004</c>).
///
/// Yang dibuktikan di sini:
///   1. `TrxPatientEncounter` memiliki `ReferralInstitutionId` dan `ReferralDoctorId`, keduanya
///      boleh kosong;
///   2. keduanya bertaut ke data induk perujuk dengan `Restrict`, sehingga instansi atau dokter
///      yang masih ditunjuk kunjungan tidak dapat terhapus;
///   3. kunjungan rujukan dapat menyimpan keduanya dan membacanya kembali, sementara kunjungan
///      biasa tetap sah tanpa keduanya.
/// </summary>
/// <remarks>
/// Yang <b>tidak</b> ada di sini: pembuatan kunjungan lewat permintaan `INT-05`. Endpoint dan
/// perilaku idempotensinya adalah pekerjaan pemilik `registration-management` bersama
/// <c>BE-LAB-08</c>; task ini hanya menyediakan tempat penyimpanan penunjuknya.
/// </remarks>
public class EncounterReferralPointerTests
{
    [Fact]
    public void Kunjungan_MemilikiDuaPenunjukPerujukYangBolehKosong()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(TrxPatientEncounter));

        Assert.NotNull(entityType);

        foreach (var nama in new[]
                 {
                     nameof(TrxPatientEncounter.ReferralInstitutionId),
                     nameof(TrxPatientEncounter.ReferralDoctorId)
                 })
        {
            var kolom = entityType!.FindProperty(nama);

            Assert.NotNull(kolom);

            // Kunjungan yang bukan rujukan memang tidak punya perujuk, dan kunjungan lama tidak
            // pernah menyimpannya. Mewajibkannya akan membatalkan seluruh data yang sudah ada.
            Assert.True(kolom!.IsNullable);
        }
    }

    [Fact]
    public void KeduaPenunjuk_BertautKeDataIndukPerujukDenganRestrict()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(TrxPatientEncounter));

        var keInstansi = entityType!.GetForeignKeys().Single(x =>
            x.PrincipalEntityType.ClrType == typeof(MstReferralInstitution));

        var keDokter = entityType.GetForeignKeys().Single(x =>
            x.PrincipalEntityType.ClrType == typeof(MstReferralDoctor));

        Assert.Equal(
            nameof(TrxPatientEncounter.ReferralInstitutionId),
            keInstansi.Properties.Single().Name);

        Assert.Equal(
            nameof(TrxPatientEncounter.ReferralDoctorId),
            keDokter.Properties.Single().Name);

        Assert.Equal(DeleteBehavior.Restrict, keInstansi.DeleteBehavior);
        Assert.Equal(DeleteBehavior.Restrict, keDokter.DeleteBehavior);
    }

    [Fact]
    public void KeduaPenunjuk_BerindexHanyaUntukBarisYangBenarBenarMerujuk()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(TrxPatientEncounter));

        foreach (var nama in new[]
                 {
                     nameof(TrxPatientEncounter.ReferralInstitutionId),
                     nameof(TrxPatientEncounter.ReferralDoctorId)
                 })
        {
            var index = entityType!.GetIndexes().SingleOrDefault(x =>
                x.Properties.Count == 1 && x.Properties[0].Name == nama);

            Assert.NotNull(index);
            Assert.Equal($"\"{nama}\" IS NOT NULL", index!.GetFilter());
        }
    }

    [Fact]
    public async Task KunjunganRujukan_MenyimpanPenunjukInstansiDanDokternya()
    {
        await using var context = CreateInMemoryContext();

        var instansi = new MstReferralInstitution
        {
            Id = Guid.NewGuid(),
            InstitutionCode = "KLN-001",
            InstitutionName = "Klinik Sehat Sentosa"
        };

        var dokter = new MstReferralDoctor
        {
            Id = Guid.NewGuid(),
            ReferralInstitutionId = instansi.Id,
            DoctorName = "dr. Rina Wijaya"
        };

        var rujukan = new TrxPatientEncounter
        {
            Id = Guid.NewGuid(),
            EncounterNumber = "ENC-0001",
            PatientId = Guid.NewGuid(),
            ServiceUnitId = Guid.NewGuid(),
            IsReferral = true,
            ReferralNumber = "SR-2026-0001",
            ReferralInstitutionId = instansi.Id,
            ReferralDoctorId = dokter.Id
        };

        // Kunjungan biasa tetap sah tanpa satu pun penunjuk perujuk.
        var biasa = new TrxPatientEncounter
        {
            Id = Guid.NewGuid(),
            EncounterNumber = "ENC-0002",
            PatientId = Guid.NewGuid(),
            ServiceUnitId = Guid.NewGuid()
        };

        context.MstReferralInstitutions.Add(instansi);
        context.MstReferralDoctors.Add(dokter);
        context.TrxPatientEncounters.AddRange(rujukan, biasa);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var tersimpan = await context.TrxPatientEncounters
            .AsNoTracking()
            .Include(x => x.ReferralInstitution)
            .Include(x => x.ReferralDoctor)
            .SingleAsync(x => x.EncounterNumber == "ENC-0001");

        Assert.True(tersimpan.IsReferral);
        Assert.Equal("SR-2026-0001", tersimpan.ReferralNumber);
        Assert.Equal("Klinik Sehat Sentosa", tersimpan.ReferralInstitution!.InstitutionName);
        Assert.Equal("dr. Rina Wijaya", tersimpan.ReferralDoctor!.DoctorName);

        var tanpaRujukan = await context.TrxPatientEncounters
            .AsNoTracking()
            .SingleAsync(x => x.EncounterNumber == "ENC-0002");

        Assert.Null(tanpaRujukan.ReferralInstitutionId);
        Assert.Null(tanpaRujukan.ReferralDoctorId);
    }

    private static ApplicationDbContext CreateRelationalModelContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=encounter_referral_model_only")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"encounter-referral-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }
}
