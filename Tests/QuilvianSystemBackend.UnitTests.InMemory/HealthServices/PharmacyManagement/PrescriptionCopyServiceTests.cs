using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.HealthServices.NutritionManagement;

namespace QuilvianSystemBackend.Tests.HealthServices.PharmacyManagement;

/// <summary>
/// Pengujian copy resep.
/// </summary>
/// <remarks>
/// Yang diuji adalah janji yang menentukan bagi dokumen yang dibawa pasien: angkanya berasal
/// dari penyerahan yang benar-benar terjadi, penyiapan dan pembatalan tidak ikut terhitung,
/// identitas penerbit berasal dari master, kekosongan kredensial dilaporkan alih-alih
/// dikarang, dan penerbitan tidak menyentuh stok maupun histori penyerahan.
/// </remarks>
public sealed class PrescriptionCopyServiceTests
{
    private sealed class Fixture : IAsyncDisposable
    {
        public required ApplicationDbContext Context { get; init; }
        public required PrescriptionCopyService Service { get; init; }
        public required PrescriptionDispensingService Dispensing { get; init; }
        public required DrugStockService StockService { get; init; }
        public required Guid DrugId { get; init; }
        public required Guid DepoId { get; init; }
        public required Guid PrescriptionId { get; init; }
        public required Guid ItemId { get; init; }
        public required Guid WorkforceId { get; init; }

        public async ValueTask DisposeAsync() => await Context.DisposeAsync();
    }

    private static async Task<Fixture> CreateAsync(decimal prescribed = 20m,
        bool withSite = true, bool withLicense = false, string licenseType = "SIPA Apoteker")
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"prescription-copy-{Guid.NewGuid():N}").Options;
        var context = new ApplicationDbContext(options);

        var drugId = Guid.NewGuid();
        var measurementId = Guid.NewGuid();
        var depoId = Guid.NewGuid();
        var encounterId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var serviceUnitId = Guid.NewGuid();
        var workforceId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var prescriptionId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        context.Set<MstDrugCategory>().Add(new MstDrugCategory
        {
            Id = categoryId, DrugCategoryCode = "K1", DrugCategoryName = "Umum",
            DrugCategoryType = "General", IsActive = true
        });
        context.Set<MstDrug>().Add(new MstDrug
        {
            Id = drugId, DrugCategoryId = categoryId, DrugCode = "OBT-001",
            DrugName = "Amoxicillin 500 mg", IsActive = true
        });
        context.Set<MstMeasurement>().Add(new MstMeasurement
        {
            Id = measurementId, MeasurementCode = "TAB", MeasurementName = "Tablet",
            MeasurementType = "Unit", IsActive = true
        });
        context.Set<MstServiceUnit>().Add(new MstServiceUnit
        {
            Id = serviceUnitId, ServiceUnitCode = "RJ", ServiceUnitName = "Rawat Jalan",
            IsActive = true
        });
        context.Set<MstPatient>().Add(new MstPatient
        {
            Id = patientId, PatientCode = "P-001", MedicalRecordNumber = "RM-001",
            FullName = "Pasien Uji", IsActive = true
        });
        context.Set<MstDoctor>().Add(new MstDoctor
        {
            Id = doctorId, DoctorCode = "DR-001", FullName = "dr. Uji Coba", IsActive = true
        });
        context.Set<TrxPatientEncounter>().Add(new TrxPatientEncounter
        {
            Id = encounterId, EncounterNumber = "ENC-001",
            PatientId = patientId, ServiceUnitId = serviceUnitId
        });
        context.Set<MstWorkforceProfile>().Add(new MstWorkforceProfile
        {
            Id = workforceId, ProfileCode = "WF1", DisplayName = "Inez Fatiha, S.Farm., Apt.",
            IsActive = true
        });
        context.Set<MstDrugStorageLocation>().Add(new MstDrugStorageLocation
        {
            Id = depoId, StorageLocationCode = "DP1", StorageLocationName = "Depo Rawat Jalan",
            IsActive = true, IsAllowDispensing = true
        });

        if (withSite)
        {
            // Dua lokasi, supaya terbukti bahwa yang dipilih adalah lokasi utama bertipe
            // rumah sakit — bukan sekadar baris pertama yang ditemukan.
            context.Set<MstLegalEntity>().Add(new MstLegalEntity
            {
                Id = Guid.NewGuid(), LegalEntityCode = "LE1",
                LegalEntityName = "PT Uji", IsActive = true
            });
            context.Set<MstHospitalSite>().AddRange(
                new MstHospitalSite
                {
                    Id = Guid.NewGuid(), LegalEntityId = Guid.NewGuid(), SiteCode = "LAB-01",
                    SiteName = "Aneka Laboratorium", SiteType = "DiagnosticCenter",
                    IsMainSite = false, IsActive = true
                },
                new MstHospitalSite
                {
                    Id = Guid.NewGuid(), LegalEntityId = Guid.NewGuid(), SiteCode = "RS-01",
                    SiteName = "RS Metropolitan Medical Centre", SiteType = "Hospital",
                    Address = "Jl. Uji Coba No. 1", PhoneNumber = "021-1234567",
                    IsMainSite = true, IsActive = true
                });
        }

        if (withLicense)
        {
            context.Set<WfpCredentialLicense>().Add(new WfpCredentialLicense
            {
                Id = Guid.NewGuid(), WorkforceProfileId = workforceId,
                LicenseType = licenseType, LicenseNumber = "SIPA-503/2026/001",
                Issuer = "Dinas Kesehatan", IssueDate = DateTime.UtcNow.AddYears(-1),
                ExpiredDate = DateTime.UtcNow.AddYears(2),
                IsPrimary = true, IsVerified = true, IsRevoked = false, IsActive = true
            });
        }

        context.Set<TrxPrescription>().Add(new TrxPrescription
        {
            Id = prescriptionId, PrescriptionNumber = "RX-001", EncounterId = encounterId,
            PatientId = patientId, DoctorId = doctorId,
            PrescriptionDateTime = DateTime.UtcNow,
            PrescriptionStatus = PrescriptionStatus.Submitted,
            FulfillmentStatus = PrescriptionFulfillmentStatus.ReadyToDispense,
            PaymentStatus = PrescriptionPaymentStatus.Paid
        });
        context.Set<TrxPrescriptionItem>().Add(new TrxPrescriptionItem
        {
            Id = itemId, PrescriptionId = prescriptionId, DrugId = drugId,
            DrugCodeSnapshot = "OBT-001", DrugNameSnapshot = "Amoxicillin 500 mg",
            GenericNameSnapshot = "Amoxicillin", StrengthSnapshot = "500 mg",
            DispenseUnitMeasurementId = measurementId, DispenseUnitNameSnapshot = "Tablet",
            Signa = "3 x 1 tablet sesudah makan",
            Quantity = prescribed, SortOrder = 1
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var accessor = new MutableHttpContextAccessor();
        accessor.SetUser(Guid.NewGuid());
        var logger = new LoggerService(NullLogger<LoggerService>.Instance, accessor);
        var stockService = new DrugStockService(context, accessor, logger);

        return new Fixture
        {
            Context = context,
            StockService = stockService,
            Service = new PrescriptionCopyService(context, accessor, logger),
            Dispensing = new PrescriptionDispensingService(context, accessor, logger, stockService),
            DrugId = drugId,
            DepoId = depoId,
            PrescriptionId = prescriptionId,
            ItemId = itemId,
            WorkforceId = workforceId
        };
    }

    private static Task<DrugStockBalanceResponse> IsiAsync(Fixture f, decimal quantity) =>
        f.StockService.RecordOpeningBalanceAsync(new RecordOpeningBalanceRequest
        {
            Batch = new DrugBatchInput
            {
                DrugId = f.DrugId, BatchNumber = "B-001",
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(180)
            },
            StorageLocationId = f.DepoId,
            Quantity = quantity,
            IdempotencyKey = "open-b1"
        });

    /// <summary>Menyiapkan lalu menyerahkan sejumlah obat, mengembalikan id penyerahannya.</summary>
    private static async Task<Guid> SerahkanAsync(Fixture f, decimal quantity, string key)
    {
        await f.Dispensing.PrepareAsync(f.PrescriptionId, new PreparePrescriptionDispensingRequest
        {
            StorageLocationId = f.DepoId,
            PreparedByWorkforceId = f.WorkforceId,
            Items = [new PrescriptionDispensingItemInput
            {
                PrescriptionItemId = f.ItemId, Quantity = quantity
            }],
            IdempotencyKey = $"p-{key}"
        });

        var usageId = await f.Context.PhmDrugUsages.AsNoTracking()
            .Where(x => x.Status == DrugUsageStatus.Draft)
            .OrderByDescending(x => x.CreateDateTime).Select(x => x.Id).FirstAsync();

        await f.Dispensing.DispenseAsync(f.PrescriptionId, usageId,
            new PrescriptionDispensingCommandRequest
            {
                ExpectedVersion = 0, IdempotencyKey = $"d-{key}"
            });

        return usageId;
    }

    private static IssuePrescriptionCopyRequest Terbitkan(Fixture f, string key = "c1") => new()
    {
        PharmacistWorkforceId = f.WorkforceId,
        IdempotencyKey = key
    };

    // ------------------------------------------------------- 1. belum pernah diserahkan

    [Fact]
    public async Task BelumPernahDiserahkan_SeluruhnyaNedet()
    {
        await using var f = await CreateAsync(prescribed: 20m);

        var hasil = await f.Service.GetPreviewAsync(f.PrescriptionId);

        Assert.NotNull(hasil);
        var baris = Assert.Single(hasil!.Items);
        Assert.Equal(20m, baris.QuantityPrescribed);
        Assert.Equal(0m, baris.QuantityDispensed);
        Assert.Equal(20m, baris.QuantityRemaining);
        Assert.Equal(PrescriptionCopyMark.Nedet, baris.Mark);
        Assert.False(hasil.IsFullyDispensed);
    }

    // ------------------------------------------------------------ 2. diserahkan sebagian

    [Fact]
    public async Task DiserahkanSebagian_SisaBenarDanNedet()
    {
        await using var f = await CreateAsync(prescribed: 20m);
        await IsiAsync(f, 100m);
        await SerahkanAsync(f, 15m, "a");

        var hasil = await f.Service.GetPreviewAsync(f.PrescriptionId);

        var baris = Assert.Single(hasil!.Items);
        Assert.Equal(20m, baris.QuantityPrescribed);
        Assert.Equal(15m, baris.QuantityDispensed);
        Assert.Equal(5m, baris.QuantityRemaining);
        Assert.Equal(PrescriptionCopyMark.Nedet, baris.Mark);
        Assert.False(hasil.IsFullyDispensed);
    }

    // --------------------------------------------------------- 3. diserahkan seluruhnya

    [Fact]
    public async Task DiserahkanSeluruhnya_SisaNolDanDet()
    {
        await using var f = await CreateAsync(prescribed: 20m);
        await IsiAsync(f, 100m);
        await SerahkanAsync(f, 20m, "a");

        var hasil = await f.Service.GetPreviewAsync(f.PrescriptionId);

        var baris = Assert.Single(hasil!.Items);
        Assert.Equal(20m, baris.QuantityDispensed);
        Assert.Equal(0m, baris.QuantityRemaining);
        Assert.Equal(PrescriptionCopyMark.Det, baris.Mark);
        Assert.True(hasil.IsFullyDispensed);
    }

    // ------------------------------------------- 4 & 5. beberapa penyerahan, sisa benar

    [Fact]
    public async Task SatuItemBeberapaPenyerahan_JumlahnyaDiakumulasi()
    {
        await using var f = await CreateAsync(prescribed: 20m);
        await IsiAsync(f, 100m);
        await SerahkanAsync(f, 5m, "a");
        await SerahkanAsync(f, 7m, "b");
        await SerahkanAsync(f, 3m, "c");

        var hasil = await f.Service.GetPreviewAsync(f.PrescriptionId);

        var baris = Assert.Single(hasil!.Items);
        Assert.Equal(15m, baris.QuantityDispensed);
        Assert.Equal(5m, baris.QuantityRemaining);
        Assert.Equal(PrescriptionCopyMark.Nedet, baris.Mark);

        // Tiga penyerahan terpisah, bukan satu yang ditimpa berulang.
        Assert.Equal(3, await f.Context.PhmDrugUsages
            .CountAsync(x => x.PrescriptionId == f.PrescriptionId));
    }

    // ------------------------------------------------- 7. yang sudah det tidak bersisa

    [Fact]
    public async Task SudahDet_TidakMenghasilkanSisaMeskiDiserahkanBerlebih()
    {
        await using var f = await CreateAsync(prescribed: 20m);
        await IsiAsync(f, 100m);
        await SerahkanAsync(f, 20m, "a");

        // Baris pemakaian tambahan yang melampaui resep, ditulis langsung untuk meniru data
        // lama yang mungkin tidak melewati penjagaan service.
        var usage = await f.Context.PhmDrugUsages.AsNoTracking()
            .FirstAsync(x => x.PrescriptionId == f.PrescriptionId);
        f.Context.PhmDrugUsageItems.Add(new PhmDrugUsageItem
        {
            DrugUsageId = usage.Id, DrugId = f.DrugId, PrescriptionItemId = f.ItemId,
            MeasurementId = Guid.NewGuid(), DrugCodeSnapshot = "OBT-001",
            DrugNameSnapshot = "Amoxicillin 500 mg", Quantity = 5m, LineNumber = 99
        });
        await f.Context.SaveChangesAsync();
        f.Context.ChangeTracker.Clear();

        var hasil = await f.Service.GetPreviewAsync(f.PrescriptionId);

        var baris = Assert.Single(hasil!.Items);
        Assert.Equal(25m, baris.QuantityDispensed);
        Assert.Equal(0m, baris.QuantityRemaining);
        Assert.Equal(PrescriptionCopyMark.Det, baris.Mark);
    }

    // --------------------------------------------------- 8 & 9. retur dan pembatalan

    /// <summary>
    /// Retur tidak mengurangi jumlah yang sudah diserahkan. Obat yang sudah berpindah ke
    /// pasien tetap tercatat diserahkan; pengembaliannya adalah peristiwa stok tersendiri.
    /// </summary>
    [Fact]
    public async Task Retur_TidakMengurangiJumlahYangSudahDiserahkan()
    {
        await using var f = await CreateAsync(prescribed: 20m);
        await IsiAsync(f, 100m);
        var usageId = await SerahkanAsync(f, 20m, "a");

        var sebelum = await f.Service.GetPreviewAsync(f.PrescriptionId);
        Assert.Equal(20m, Assert.Single(sebelum!.Items).QuantityDispensed);

        // Retur yang menunjuk penyerahan tadi.
        f.Context.PhmDrugReturns.Add(new PhmDrugReturn
        {
            Id = Guid.NewGuid(), ReturnNumber = "RTN-001",
            EncounterId = (await f.Context.PhmDrugUsages.AsNoTracking()
                .Where(x => x.Id == usageId).Select(x => x.EncounterId).FirstAsync()),
            StorageLocationId = f.DepoId,
            ReturnedByWorkforceId = f.WorkforceId,
            SourceDrugUsageId = usageId,
            Status = DrugReturnStatus.Verified,
            ReturnedAt = DateTime.UtcNow, ItemCount = 1
        });
        await f.Context.SaveChangesAsync();
        f.Context.ChangeTracker.Clear();

        var sesudah = await f.Service.GetPreviewAsync(f.PrescriptionId);

        var baris = Assert.Single(sesudah!.Items);
        Assert.Equal(20m, baris.QuantityDispensed);
        Assert.Equal(0m, baris.QuantityRemaining);
        Assert.Equal(PrescriptionCopyMark.Det, baris.Mark);
    }

    /// <summary>
    /// Penyiapan yang dibatalkan tidak pernah sampai ke pasien, jadi ia tidak boleh terhitung
    /// sebagai penyerahan.
    /// </summary>
    [Fact]
    public async Task PenyiapanDibatalkan_TidakTerhitungSebagaiPenyerahan()
    {
        await using var f = await CreateAsync(prescribed: 20m);
        await IsiAsync(f, 100m);

        await f.Dispensing.PrepareAsync(f.PrescriptionId, new PreparePrescriptionDispensingRequest
        {
            StorageLocationId = f.DepoId, PreparedByWorkforceId = f.WorkforceId,
            Items = [new PrescriptionDispensingItemInput
            {
                PrescriptionItemId = f.ItemId, Quantity = 8m
            }],
            IdempotencyKey = "p-batal"
        });
        var usageId = await f.Context.PhmDrugUsages.AsNoTracking()
            .Where(x => x.Status == DrugUsageStatus.Draft).Select(x => x.Id).FirstAsync();

        await f.Dispensing.CancelAsync(f.PrescriptionId, usageId,
            new CancelPrescriptionDispensingRequest
            {
                Reason = "Pasien membatalkan.", ExpectedVersion = 0, IdempotencyKey = "c-batal"
            });

        var hasil = await f.Service.GetPreviewAsync(f.PrescriptionId);

        var baris = Assert.Single(hasil!.Items);
        Assert.Equal(0m, baris.QuantityDispensed);
        Assert.Equal(20m, baris.QuantityRemaining);
        Assert.Equal(PrescriptionCopyMark.Nedet, baris.Mark);
    }

    /// <summary>Penyiapan yang masih draft juga belum sampai ke pasien.</summary>
    [Fact]
    public async Task PenyiapanMasihDraft_TidakTerhitungSebagaiPenyerahan()
    {
        await using var f = await CreateAsync(prescribed: 20m);
        await IsiAsync(f, 100m);

        await f.Dispensing.PrepareAsync(f.PrescriptionId, new PreparePrescriptionDispensingRequest
        {
            StorageLocationId = f.DepoId, PreparedByWorkforceId = f.WorkforceId,
            Items = [new PrescriptionDispensingItemInput
            {
                PrescriptionItemId = f.ItemId, Quantity = 9m
            }],
            IdempotencyKey = "p-draft"
        });

        var hasil = await f.Service.GetPreviewAsync(f.PrescriptionId);

        var baris = Assert.Single(hasil!.Items);
        Assert.Equal(0m, baris.QuantityDispensed);
        Assert.Equal(20m, baris.QuantityRemaining);
    }

    // --------------------------------------------------- 10 & 11. identitas penerbit

    /// <summary>
    /// Identitas fasilitas berasal dari master, dan yang dipilih adalah lokasi utama bertipe
    /// rumah sakit — bukan baris pertama yang kebetulan ditemukan.
    /// </summary>
    [Fact]
    public async Task IdentitasFasilitas_DiambilDariMasterLokasiUtama()
    {
        await using var f = await CreateAsync(withSite: true);

        var hasil = await f.Service.GetPreviewAsync(f.PrescriptionId, f.WorkforceId);

        Assert.Equal("RS Metropolitan Medical Centre", hasil!.Issuer.SiteName);
        Assert.Equal("Jl. Uji Coba No. 1", hasil.Issuer.SiteAddress);
        Assert.Equal("021-1234567", hasil.Issuer.SitePhoneNumber);
    }

    /// <summary>
    /// Nomor izin praktik dibaca dari master kredensial, bukan diketik.
    /// </summary>
    [Fact]
    public async Task NomorIzinApoteker_DibacaDariMasterKredensial()
    {
        await using var f = await CreateAsync(withLicense: true);

        var hasil = await f.Service.GetPreviewAsync(f.PrescriptionId, f.WorkforceId);

        Assert.Equal("Inez Fatiha, S.Farm., Apt.", hasil!.Issuer.PharmacistName);
        Assert.Equal("SIPA-503/2026/001", hasil.Issuer.PharmacistLicenseNumber);
        Assert.DoesNotContain(hasil.MissingDocumentData,
            x => x.Contains("izin praktik", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Kredensial yang kosong dilaporkan apa adanya. Sistem tidak boleh mengarang nomor izin
    /// pada dokumen yang berlaku sebagai keterangan resmi.
    /// </summary>
    [Fact]
    public async Task IzinApotekerKosong_DilaporkanBukanDikarang()
    {
        await using var f = await CreateAsync(withLicense: false);

        var hasil = await f.Service.GetPreviewAsync(f.PrescriptionId, f.WorkforceId);

        Assert.Null(hasil!.Issuer.PharmacistLicenseNumber);
        Assert.Null(hasil.Issuer.PharmacistLicenseType);
        Assert.Contains(hasil.MissingDocumentData,
            x => x.Contains("izin praktik", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Izin yang bukan izin praktik apoteker — misalnya STR keperawatan — tidak dipungut.
    /// Lebih baik kosong daripada mencantumkan nomor izin yang keliru.
    /// </summary>
    [Fact]
    public async Task IzinBukanApoteker_TidakDipakai()
    {
        await using var f = await CreateAsync(withLicense: true, licenseType: "STR Keperawatan");

        var hasil = await f.Service.GetPreviewAsync(f.PrescriptionId, f.WorkforceId);

        Assert.Null(hasil!.Issuer.PharmacistLicenseNumber);
        Assert.Contains(hasil.MissingDocumentData,
            x => x.Contains("izin praktik", StringComparison.OrdinalIgnoreCase));
    }

    // ------------------------------------------------------------------ 12. penerbitan

    /// <summary>
    /// Menerbitkan copy resep tidak mengubah stok maupun histori penyerahan.
    /// </summary>
    [Fact]
    public async Task Penerbitan_TidakMengubahStokMaupunHistoriPenyerahan()
    {
        await using var f = await CreateAsync(prescribed: 20m, withLicense: true);
        await IsiAsync(f, 100m);
        await SerahkanAsync(f, 15m, "a");

        var stokSebelum = await f.Context.PhmDrugStockBalances.AsNoTracking()
            .SumAsync(x => x.QuantityOnHand);
        var mutasiSebelum = await f.Context.PhmDrugStockMutations.CountAsync();
        var pemakaianSebelum = await f.Context.PhmDrugUsages.CountAsync();

        var copy = await f.Service.IssueAsync(f.PrescriptionId, Terbitkan(f));

        Assert.Equal(stokSebelum, await f.Context.PhmDrugStockBalances.AsNoTracking()
            .SumAsync(x => x.QuantityOnHand));
        Assert.Equal(mutasiSebelum, await f.Context.PhmDrugStockMutations.CountAsync());
        Assert.Equal(pemakaianSebelum, await f.Context.PhmDrugUsages.CountAsync());

        Assert.StartsWith("CR-", copy.CopyNumber);
        Assert.Equal(PrescriptionCopyStatus.Issued, copy.Status);
    }

    /// <summary>
    /// Angka pada lembar dibekukan saat terbit: penyerahan berikutnya tidak mengubah lembar
    /// yang sudah dipegang pasien.
    /// </summary>
    [Fact]
    public async Task AngkaPadaLembar_DibekukanSaatTerbit()
    {
        await using var f = await CreateAsync(prescribed: 20m);
        await IsiAsync(f, 100m);
        await SerahkanAsync(f, 15m, "a");

        var copy = await f.Service.IssueAsync(f.PrescriptionId, Terbitkan(f));
        Assert.Equal(5m, Assert.Single(copy.Items).QuantityRemaining);

        // Sisa obat diserahkan sesudah lembar tercetak.
        await SerahkanAsync(f, 5m, "b");

        var lembar = await f.Service.GetDetailAsync(copy.Id);
        var pratinjau = await f.Service.GetPreviewAsync(f.PrescriptionId);

        // Lembar tetap menyatakan keadaan saat ia terbit.
        Assert.Equal(5m, Assert.Single(lembar!.Items).QuantityRemaining);
        Assert.Equal(PrescriptionCopyMark.Nedet, Assert.Single(lembar.Items).Mark);

        // Sedangkan keadaan sekarang sudah lunas.
        Assert.Equal(0m, Assert.Single(pratinjau!.Items).QuantityRemaining);
        Assert.Equal(PrescriptionCopyMark.Det, Assert.Single(pratinjau.Items).Mark);
    }

    /// <summary>Penerbitan dengan kunci yang sama tidak menghasilkan dua lembar.</summary>
    [Fact]
    public async Task PenerbitanDiulang_TidakMenghasilkanDuaLembar()
    {
        await using var f = await CreateAsync(prescribed: 20m);

        var pertama = await f.Service.IssueAsync(f.PrescriptionId, Terbitkan(f, "sama"));
        var kedua = await f.Service.IssueAsync(f.PrescriptionId, Terbitkan(f, "sama"));

        Assert.Equal(pertama.Id, kedua.Id);
        Assert.Equal(pertama.CopyNumber, kedua.CopyNumber);
        Assert.Equal(1, await f.Context.PhmPrescriptionCopies.CountAsync());
    }

    /// <summary>Lembar yang dicabut tetap ada beserta alasannya; ia mungkin sudah beredar.</summary>
    [Fact]
    public async Task Pencabutan_MenyimpanLembarBesertaAlasannya()
    {
        await using var f = await CreateAsync(prescribed: 20m);
        var copy = await f.Service.IssueAsync(f.PrescriptionId, Terbitkan(f));

        var dicabut = await f.Service.RevokeAsync(copy.Id, new RevokePrescriptionCopyRequest
        {
            Reason = "Salah cetak nama pasien.",
            ExpectedVersion = copy.Version,
            IdempotencyKey = "rev-1"
        });

        Assert.Equal(PrescriptionCopyStatus.Revoked, dicabut.Status);
        Assert.Equal("Salah cetak nama pasien.", dicabut.RevokeReason);
        Assert.NotNull(dicabut.RevokedAt);
        Assert.Equal(1, await f.Context.PhmPrescriptionCopies.CountAsync());
        Assert.Single(dicabut.Items);
    }

    /// <summary>Resep tanpa satu pun baris obat tidak dapat diterbitkan copy resepnya.</summary>
    [Fact]
    public async Task ResepTanpaItem_TidakDapatDiterbitkan()
    {
        await using var f = await CreateAsync(prescribed: 20m);
        var item = await f.Context.TrxPrescriptionItems.FirstAsync();
        item.IsDelete = true;
        await f.Context.SaveChangesAsync();
        f.Context.ChangeTracker.Clear();

        var exception = await Assert.ThrowsAsync<PrescriptionCopyUnprocessableException>(
            () => f.Service.IssueAsync(f.PrescriptionId, Terbitkan(f)));

        Assert.Equal("PHM120", exception.Code);
    }
}
