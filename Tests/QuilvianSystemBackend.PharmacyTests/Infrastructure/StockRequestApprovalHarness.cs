using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

/// <summary>
/// Data acuan paling kecil untuk menguji persetujuan permintaan stok (<c>LGS-DEC-002</c> sampai
/// <c>LGS-DEC-006</c>) pada basis data SQLite dalam memori yang dibuang setelah uji.
/// </summary>
/// <remarks>
/// <para>
/// Seluruh data di sini hidup hanya di basis data uji (<see cref="TestDatabase"/>) dan tidak
/// pernah menyentuh basis data tim, lokal, maupun produksi. Identitasnya sengaja sintetis dan
/// berawalan "Uji" supaya tidak pernah tertukar dengan data organisasi sungguhan.
/// </para>
/// <para>
/// Sejak FIN-LGS-070 kewenangan hanya dibaca dari izin aktif:
/// penempatan akses pengguna (<c>ApplicationUserOrganization</c>) × kebijakan akses. Jabatan
/// <see cref="JabatanPenyetujuId"/> diberi izin <c>StockRequest:Approve</c>; jabatan
/// <see cref="JabatanCitoId"/> diberi izin <c>StockRequest:AuthorizeEmergency</c>; jabatan
/// <see cref="JabatanStafId"/> tidak diberi izin apa pun. Peran SuperAdmin sengaja disiapkan tanpa
/// penempatan apa pun.
/// </para>
/// <para>
/// Data organisasi HRD tetap disemai dengan sengaja — unit layanan <see cref="UnitId"/> →
/// <see cref="OrgUnitId"/>, <see cref="UnitLainId"/> → <see cref="OrgUnitLainId"/>,
/// <see cref="UnitTanpaTautanId"/> tanpa tautan, dan penempatan manajerial
/// (<c>WfpOrganizationAssignment</c>) yang aktif, berakhir, atau tidak ada — supaya uji dapat
/// membuktikan bahwa data itu tidak lagi menentukan siapa yang boleh memutus. Nama pengguna
/// "Atasan…" dipertahankan dari rancangan lama; perannya kini ditentukan izinnya saja.
/// </para>
/// </remarks>
public sealed class StockRequestApprovalHarness : IDisposable
{
    public const int VersiAwal = 2;
    public const decimal JumlahDiminta = 10m;
    public const decimal StokAwal = 100m;

    private readonly TestDatabase _database = TestDatabase.Create();
    private readonly List<ApplicationDbContext> _contexts = [];
    private int _nomor;

    public Guid UnitId { get; } = Guid.NewGuid();
    public Guid UnitLainId { get; } = Guid.NewGuid();
    public Guid UnitTanpaTautanId { get; } = Guid.NewGuid();
    public Guid OrgUnitId { get; } = Guid.NewGuid();
    public Guid OrgUnitLainId { get; } = Guid.NewGuid();
    public Guid GudangId { get; } = Guid.NewGuid();
    public Guid ObatId { get; } = Guid.NewGuid();
    public Guid SatuanId { get; } = Guid.NewGuid();

    public Guid DepartemenId { get; } = Guid.NewGuid();
    public Guid JabatanPenyetujuId { get; } = Guid.NewGuid();
    public Guid JabatanCitoId { get; } = Guid.NewGuid();
    public Guid JabatanStafId { get; } = Guid.NewGuid();

    /// <summary>Pemohon; berizin Approve dan CITO — untuk menguji larangan memutus sendiri.</summary>
    public Guid PemohonUserId { get; } = Guid.NewGuid();
    /// <summary>Penyetuju berizin Approve (penempatan manajerial HRD pada <see cref="OrgUnitId"/>).</summary>
    public Guid AtasanUserId { get; } = Guid.NewGuid();
    /// <summary>Berizin Approve dan CITO.</summary>
    public Guid AtasanCitoUserId { get; } = Guid.NewGuid();
    /// <summary>Berizin Approve; penempatan manajerial HRD-nya pada unit lain (<see cref="OrgUnitLainId"/>).</summary>
    public Guid AtasanLainUserId { get; } = Guid.NewGuid();
    /// <summary>Berizin Approve; penempatan manajerial HRD-nya sudah berakhir.</summary>
    public Guid AtasanKedaluwarsaUserId { get; } = Guid.NewGuid();
    /// <summary>Penempatan manajerial HRD aktif, tetapi tanpa izin Approve.</summary>
    public Guid AtasanTanpaIzinUserId { get; } = Guid.NewGuid();
    /// <summary>Berizin Approve; profil tenaga kerja HRD-nya sudah dinonaktifkan, akunnya tetap aktif.</summary>
    public Guid AtasanNonaktifUserId { get; } = Guid.NewGuid();
    /// <summary>Petugas berizin CITO, tanpa izin Approve.</summary>
    public Guid PetugasCitoUserId { get; } = Guid.NewGuid();
    /// <summary>Pernah berizin CITO, penempatan aksesnya sudah berakhir.</summary>
    public Guid PetugasCitoKedaluwarsaUserId { get; } = Guid.NewGuid();
    /// <summary>Staf gudang/unit tanpa izin Approve maupun CITO; dipakai sebagai petugas penyerah barang.</summary>
    public Guid StafUserId { get; } = Guid.NewGuid();
    /// <summary>Berperan SuperAdmin, tanpa profil tenaga kerja dan tanpa penempatan apa pun.</summary>
    public Guid SuperAdminUserId { get; } = Guid.NewGuid();
    /// <summary>Berizin Approve; penempatan manajerial HRD pada dua unit organisasi.</summary>
    public Guid AtasanGandaUserId { get; } = Guid.NewGuid();

    /// <summary>Kode jabatan CITO pada <c>MstPosition</c> uji.</summary>
    public const string KodeJabatanCito = "UJI-POS-CITO";

    private readonly Guid _pemohonWfp = Guid.NewGuid();
    private readonly Guid _atasanWfp = Guid.NewGuid();
    private readonly Guid _atasanCitoWfp = Guid.NewGuid();
    private readonly Guid _atasanLainWfp = Guid.NewGuid();
    private readonly Guid _atasanKedaluwarsaWfp = Guid.NewGuid();
    private readonly Guid _atasanTanpaIzinWfp = Guid.NewGuid();
    private readonly Guid _atasanNonaktifWfp = Guid.NewGuid();
    private readonly Guid _petugasCitoWfp = Guid.NewGuid();
    private readonly Guid _stafWfp = Guid.NewGuid();
    private readonly Guid _atasanGandaWfp = Guid.NewGuid();

    public StockRequestApprovalHarness()
    {
        using var k = _database.CreateContext();
        var now = DateTime.UtcNow;

        k.MstOrganizationUnits.AddRange(
            new MstOrganizationUnit
            {
                Id = OrgUnitId, LegalEntityId = Guid.NewGuid(), UnitCode = "UJI-ORG-1",
                UnitName = "Instalasi Rawat Inap (Uji)", IsActive = true, CreateDateTime = now
            },
            new MstOrganizationUnit
            {
                Id = OrgUnitLainId, LegalEntityId = Guid.NewGuid(), UnitCode = "UJI-ORG-2",
                UnitName = "Instalasi Gawat Darurat (Uji)", IsActive = true, CreateDateTime = now
            });

        k.MstServiceUnits.AddRange(
            Unit(UnitId, "UJI-SU-1", "Ruang Melati (Uji)", OrgUnitId),
            Unit(UnitLainId, "UJI-SU-2", "IGD (Uji)", OrgUnitLainId),
            Unit(UnitTanpaTautanId, "UJI-SU-3", "Poli Tanpa Tautan (Uji)", null));

        k.MstDrugStorageLocations.Add(new MstDrugStorageLocation
        {
            Id = GudangId, StorageLocationCode = "UJI-GUD-SRQ", StorageLocationName = "Gudang Induk (Uji)",
            IsActive = true, CreateDateTime = now
        });

        foreach (var (id, kode, aktif) in new[]
                 {
                     (_pemohonWfp, "UJI-WFP-PEMOHON", true), (_atasanWfp, "UJI-WFP-ATASAN", true),
                     (_atasanCitoWfp, "UJI-WFP-ATASAN-CITO", true), (_atasanLainWfp, "UJI-WFP-ATASAN-LAIN", true),
                     (_atasanKedaluwarsaWfp, "UJI-WFP-ATASAN-LAMA", true),
                     (_atasanTanpaIzinWfp, "UJI-WFP-ATASAN-TANPA-IZIN", true),
                     (_atasanNonaktifWfp, "UJI-WFP-ATASAN-NONAKTIF", false),
                     (_petugasCitoWfp, "UJI-WFP-CITO", true), (_stafWfp, "UJI-WFP-STAF", true),
                     (_atasanGandaWfp, "UJI-WFP-ATASAN-GANDA", true)
                 })
        {
            k.MstWorkforceProfiles.Add(new MstWorkforceProfile
            {
                Id = id, ProfileCode = kode, DisplayName = kode, IsActive = aktif, CreateDateTime = now
            });
        }

        Pengguna(k, PemohonUserId, _pemohonWfp);
        Pengguna(k, AtasanUserId, _atasanWfp);
        Pengguna(k, AtasanCitoUserId, _atasanCitoWfp);
        Pengguna(k, AtasanLainUserId, _atasanLainWfp);
        Pengguna(k, AtasanKedaluwarsaUserId, _atasanKedaluwarsaWfp);
        Pengguna(k, AtasanTanpaIzinUserId, _atasanTanpaIzinWfp);
        Pengguna(k, AtasanNonaktifUserId, _atasanNonaktifWfp);
        Pengguna(k, PetugasCitoUserId, _petugasCitoWfp);
        Pengguna(k, PetugasCitoKedaluwarsaUserId, null);
        Pengguna(k, StafUserId, _stafWfp);
        Pengguna(k, SuperAdminUserId, null);
        Pengguna(k, AtasanGandaUserId, _atasanGandaWfp);

        // Jabatan sungguhan pada master jabatan uji, supaya penunjukan berdasarkan kode dapat diuji.
        k.MstPositions.AddRange(
            new MstPosition
            {
                Id = JabatanCitoId, DepartmentId = DepartemenId, PositionCode = KodeJabatanCito,
                PositionName = "Penanggung Jawab CITO (Uji)", IsActive = true, CreateDateTime = now
            },
            new MstPosition
            {
                Id = JabatanPenyetujuId, DepartmentId = DepartemenId, PositionCode = "UJI-POS-PENYETUJU",
                PositionName = "Kepala Unit (Uji)", IsActive = true, CreateDateTime = now
            },
            new MstPosition
            {
                Id = JabatanStafId, DepartmentId = DepartemenId, PositionCode = "UJI-POS-STAF",
                PositionName = "Staf Unit (Uji)", IsActive = true, CreateDateTime = now
            });

        var peranSuperAdmin = new ApplicationRole
        {
            Id = Guid.NewGuid(), Name = "SuperAdmin", NormalizedName = "SUPERADMIN"
        };
        k.Roles.Add(peranSuperAdmin);
        k.UserRoles.Add(new IdentityUserRole<Guid> { UserId = SuperAdminUserId, RoleId = peranSuperAdmin.Id });

        // Penempatan manajerial HRD: tidak lagi dibaca persetujuan (FIN-LGS-070), disemai supaya
        // uji dapat membuktikan bahwa ada atau tidaknya penempatan ini tidak mengubah kewenangan.
        var setahunLalu = now.AddYears(-1);
        Penempatan(k, _atasanWfp, OrgUnitId, manajerial: true, mulai: setahunLalu, akhir: null);
        Penempatan(k, _atasanCitoWfp, OrgUnitId, manajerial: true, mulai: setahunLalu, akhir: null);
        Penempatan(k, _pemohonWfp, OrgUnitId, manajerial: true, mulai: setahunLalu, akhir: null);
        Penempatan(k, _atasanLainWfp, OrgUnitLainId, manajerial: true, mulai: setahunLalu, akhir: null);
        Penempatan(k, _atasanKedaluwarsaWfp, OrgUnitId, manajerial: true,
            mulai: now.AddYears(-2), akhir: now.AddDays(-1));
        Penempatan(k, _atasanTanpaIzinWfp, OrgUnitId, manajerial: true, mulai: setahunLalu, akhir: null);
        Penempatan(k, _atasanNonaktifWfp, OrgUnitId, manajerial: true, mulai: setahunLalu, akhir: null);
        Penempatan(k, _stafWfp, OrgUnitId, manajerial: false, mulai: setahunLalu, akhir: null);
        Penempatan(k, _atasanGandaWfp, OrgUnitId, manajerial: true, mulai: setahunLalu, akhir: null);
        Penempatan(k, _atasanGandaWfp, OrgUnitLainId, manajerial: true, mulai: setahunLalu, akhir: null);

        // Izin: Approve pada JabatanPenyetujuId, AuthorizeEmergency pada JabatanCitoId.
        var controllerId = Guid.NewGuid();
        var aksiApproveId = Guid.NewGuid();
        var aksiCitoId = Guid.NewGuid();
        k.SysControllerAccesses.Add(new SysControllerAccess
        {
            Id = controllerId, ModuleId = Guid.NewGuid(),
            ControllerName = StockRequestApprovalAuthority.PermissionResource,
            DisplayName = "Stock Request", IsActive = true, CreateDateTime = now
        });
        k.SysActionAccesses.AddRange(
            new SysActionAccess
            {
                Id = aksiApproveId, ControllerAccessId = controllerId,
                ActionName = StockRequestApprovalAuthority.ApprovePermissionAction,
                DisplayName = "Approve Stock Request", IsActive = true, CreateDateTime = now
            },
            new SysActionAccess
            {
                Id = aksiCitoId, ControllerAccessId = controllerId,
                ActionName = StockRequestApprovalAuthority.EmergencyPermissionAction,
                DisplayName = "Authorize Emergency Stock Request", IsActive = true, CreateDateTime = now
            });
        k.SysAccessPolicies.AddRange(
            new SysAccessPolicy
            {
                DepartmentId = DepartemenId, PositionId = JabatanPenyetujuId, ControllerAccessId = controllerId,
                ActionAccessId = aksiApproveId, IsAllowed = true, IsActive = true, CreateDateTime = now
            },
            new SysAccessPolicy
            {
                DepartmentId = DepartemenId, PositionId = JabatanCitoId, ControllerAccessId = controllerId,
                ActionAccessId = aksiCitoId, IsAllowed = true, IsActive = true, CreateDateTime = now
            });

        foreach (var userId in new[]
                 {
                     PemohonUserId, AtasanUserId, AtasanCitoUserId, AtasanLainUserId,
                     AtasanKedaluwarsaUserId, AtasanNonaktifUserId
                 })
            PenempatanJabatan(k, userId, JabatanPenyetujuId, akhir: null);

        PenempatanJabatan(k, PemohonUserId, JabatanCitoId, akhir: null);
        PenempatanJabatan(k, AtasanCitoUserId, JabatanCitoId, akhir: null);
        PenempatanJabatan(k, PetugasCitoUserId, JabatanCitoId, akhir: null);
        PenempatanJabatan(k, PetugasCitoKedaluwarsaUserId, JabatanCitoId, akhir: now.AddDays(-1));
        // Atasan tanpa izin dan staf hanya memegang jabatan staf yang tidak diberi izin apa pun.
        PenempatanJabatan(k, AtasanTanpaIzinUserId, JabatanStafId, akhir: null);
        PenempatanJabatan(k, StafUserId, JabatanStafId, akhir: null);
        PenempatanJabatan(k, AtasanGandaUserId, JabatanPenyetujuId, akhir: null);

        k.SaveChanges();
    }

    public ApplicationDbContext CreateContext()
    {
        var konteks = _database.CreateContext();
        _contexts.Add(konteks);
        return konteks;
    }

    // ------------------------------------------------------------- Stock Opname (FIN-LGS-091)

    private TestDatabase? _databaseNomor;

    /// <summary>Aksesor yang bertindak sebagai <paramref name="userId"/>, untuk service selain permintaan stok.</summary>
    public IHttpContextAccessor AksesorPengguna(Guid userId) => Accessor(userId);

    /// <summary>
    /// Alokator nomor bersama di atas basis data SQLite tersendiri, seperti harness keanggotaan pasien.
    /// Alokator membuka transaksinya sendiri; pada satu koneksi SQLite transaksi itu akan bertabrakan
    /// dengan transaksi bisnis pemanggil.
    /// </summary>
    public NumberSeriesAllocator AlokatorNomor()
    {
        _databaseNomor ??= TestDatabase.Create();
        return new NumberSeriesAllocator(new PabrikKonteks(_databaseNomor));
    }

    /// <summary>
    /// Izin <c>StockOpname:Approve</c> pada jabatan penyetuju, sehingga Pemohon, Atasan, AtasanCito, AtasanLain,
    /// dan AtasanGanda memegangnya; Staf dan AtasanTanpaIzin tidak. Hanya dipanggil uji Stock Opname.
    /// </summary>
    public async Task SediakanIzinStockOpnameAsync()
    {
        using var k = _database.CreateContext();
        var now = DateTime.UtcNow;
        var controllerId = Guid.NewGuid();
        var aksiId = Guid.NewGuid();

        k.SysControllerAccesses.Add(new SysControllerAccess
        {
            Id = controllerId, ModuleId = Guid.NewGuid(), ControllerName = StockOpnamePermissions.Resource,
            DisplayName = "Stock Opname", IsActive = true, CreateDateTime = now
        });
        k.SysActionAccesses.Add(new SysActionAccess
        {
            Id = aksiId, ControllerAccessId = controllerId, ActionName = StockOpnamePermissions.Approve,
            DisplayName = "Approve Stock Opname", IsActive = true, CreateDateTime = now
        });
        k.SysAccessPolicies.Add(new SysAccessPolicy
        {
            DepartmentId = DepartemenId, PositionId = JabatanPenyetujuId, ControllerAccessId = controllerId,
            ActionAccessId = aksiId, IsAllowed = true, IsActive = true, CreateDateTime = now
        });
        await k.SaveChangesAsync();
    }

    private sealed class PabrikKonteks(TestDatabase db) : IDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext() => db.CreateContext();
    }

    /// <summary>
    /// Service permintaan stok yang bertindak sebagai <paramref name="userId"/>.
    /// </summary>
    /// <param name="batasAktivasi">
    /// Nilai <see cref="StockRequestService.ApprovalEnforcedFromConfigKey"/>; kosong berarti tidak diatur.
    /// </param>
    /// <param name="denganStok">Bila benar, penyerahan benar-benar mengurangi stok lewat <see cref="DrugStockService"/>.</param>
    /// <param name="tenggatJam">Nilai tenggat verifikasi susulan; null berarti belum dikonfigurasi.</param>
    /// <param name="peringatanJam">Nilai batas peringatan tenggat.</param>
    /// <param name="log">Logger penangkap untuk membuktikan jejak audit; null berarti dibuang.</param>
    public StockRequestService Layanan(ApplicationDbContext k, Guid userId,
        string? batasAktivasi = null, bool denganStok = false,
        string? tenggatJam = null, string? peringatanJam = null, CatatanLogUji? log = null)
    {
        var accessor = Accessor(userId);
        var pencatat = new LoggerService(
            log ?? (Microsoft.Extensions.Logging.ILogger<LoggerService>)NullLogger<LoggerService>.Instance,
            accessor);
        var konfigurasi = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [StockRequestService.ApprovalEnforcedFromConfigKey] = batasAktivasi,
                [StockRequestEmergencyReviewDeadline.DueHoursConfigKey] = tenggatJam,
                [StockRequestEmergencyReviewDeadline.WarningHoursConfigKey] = peringatanJam
            })
            .Build();

        return new StockRequestService(k, accessor, pencatat,
            drugStockService: denganStok ? new DrugStockService(k, accessor, pencatat) : null,
            configuration: konfigurasi);
    }

    /// <summary>Menyediakan satu batch obat uji di gudang uji dengan saldo siap pakai.</summary>
    public async Task SediakanStokAsync(decimal jumlah = StokAwal)
    {
        using var k = _database.CreateContext();
        var betsId = Guid.NewGuid();

        k.Set<PhmDrugBatch>().Add(new PhmDrugBatch
        {
            Id = betsId,
            DrugId = ObatId,
            BatchNumber = "UJI-SRQ-BATCH-1",
            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            CreateDateTime = DateTime.UtcNow
        });

        k.PhmDrugStockBalances.Add(new PhmDrugStockBalance
        {
            Id = Guid.NewGuid(),
            DrugId = ObatId,
            DrugBatchId = betsId,
            StorageLocationId = GudangId,
            Status = DrugStockStatus.Available,
            QuantityOnHand = jumlah,
            QuantityReserved = 0,
            CreateDateTime = DateTime.UtcNow
        });

        await k.SaveChangesAsync();
    }

    /// <summary>Jumlah saldo fisik dan jumlah baris mutasi stok, untuk membuktikan stok tidak berubah.</summary>
    public async Task<(decimal Saldo, int Mutasi)> PotretStokAsync()
    {
        using var k = _database.CreateContext();
        // Dijumlah di memori: SQLite tidak dapat menjumlah kolom decimal di sisi basis data.
        var saldo = (await k.PhmDrugStockBalances.AsNoTracking()
            .Where(x => x.DrugId == ObatId)
            .Select(x => x.QuantityOnHand)
            .ToListAsync()).Sum();
        var mutasi = await k.PhmDrugStockMutations.AsNoTracking().CountAsync();
        return (saldo, mutasi);
    }

    /// <summary>
    /// Menyisipkan satu permintaan dengan satu baris, dibuat dan diajukan oleh pemohon.
    /// </summary>
    public async Task<(Guid Id, Guid ItemId)> PermintaanAsync(
        StockRequestStatus status = StockRequestStatus.Submitted,
        StockRequestPriority prioritas = StockRequestPriority.Routine,
        Guid? unitId = null,
        DateTime? diajukanPada = null,
        Guid? dibuatOleh = null,
        bool tanpaWaktuPengajuan = false,
        Guid? pemintaWorkforceId = null)
    {
        using var k = _database.CreateContext();
        var now = DateTime.UtcNow;
        var id = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var pembuat = dibuatOleh ?? PemohonUserId;
        var diajukan = status == StockRequestStatus.Draft || tanpaWaktuPengajuan
            ? (DateTime?)null
            : diajukanPada ?? now.AddHours(-1);

        k.PhmStockRequests.Add(new PhmStockRequest
        {
            Id = id,
            RequestNumber = $"UJI-SRQ-{++_nomor:0000}",
            RequestingServiceUnitId = unitId ?? UnitId,
            StorageLocationId = GudangId,
            RequestedByWorkforceId = pemintaWorkforceId ?? _pemohonWfp,
            Status = status,
            Priority = prioritas,
            RequestedAt = now.AddHours(-2),
            SubmittedAt = diajukan,
            ItemCount = 1,
            Version = VersiAwal,
            CreateBy = pembuat,
            CreateDateTime = now.AddHours(-2)
        });

        k.PhmStockRequestItems.Add(new PhmStockRequestItem
        {
            Id = itemId,
            StockRequestId = id,
            DrugId = ObatId,
            MeasurementId = SatuanId,
            DrugCodeSnapshot = "UJI-OBT-1",
            DrugNameSnapshot = "Obat Uji Satu",
            RequestedQuantity = JumlahDiminta,
            LineNumber = 1,
            CreateBy = pembuat,
            CreateDateTime = now.AddHours(-2)
        });

        k.PhmStockRequestHistories.Add(Riwayat(id, null, StockRequestStatus.Draft,
            "CreateStockRequest", pembuat, now.AddHours(-2)));

        if (status != StockRequestStatus.Draft)
            k.PhmStockRequestHistories.Add(Riwayat(id, StockRequestStatus.Draft,
                StockRequestStatus.Submitted, "SubmitStockRequest", PemohonUserId,
                diajukan ?? now.AddHours(-1)));

        await k.SaveChangesAsync();
        return (id, itemId);
    }

    /// <summary>Mengubah keadaan unit organisasi pada basis data uji.</summary>
    public async Task UbahUnitOrganisasiAsync(Guid organizationUnitId, bool aktif, DateTime? berakhir = null)
    {
        using var k = _database.CreateContext();
        var unit = await k.MstOrganizationUnits.SingleAsync(x => x.Id == organizationUnitId);
        unit.IsActive = aktif;
        unit.EffectiveEndDate = berakhir;
        await k.SaveChangesAsync();
    }

    /// <summary>Menonaktifkan akun pengguna pada basis data uji.</summary>
    public async Task NonaktifkanPenggunaAsync(Guid userId)
    {
        using var k = _database.CreateContext();
        var user = await k.Users.SingleAsync(x => x.Id == userId);
        user.IsActive = false;
        await k.SaveChangesAsync();
    }

    /// <summary>
    /// Mencabut izin <c>StockRequest:<paramref name="aksi"/></c> dengan menonaktifkan kebijakan
    /// aksesnya pada basis data uji, seperti administrator mencabutnya di Akses Role.
    /// </summary>
    public async Task CabutIzinAsync(string aksi)
    {
        using var k = _database.CreateContext();
        var aksiId = await k.SysActionAccesses
            .Where(x => x.ActionName == aksi)
            .Select(x => x.Id)
            .SingleAsync();
        var kebijakan = await k.SysAccessPolicies.Where(x => x.ActionAccessId == aksiId).ToListAsync();
        foreach (var baris in kebijakan) baris.IsActive = false;
        await k.SaveChangesAsync();
    }

    /// <summary>Profil tenaga kerja pemohon, untuk saringan petugas peminta dan pembuatan permintaan.</summary>
    public Guid PemohonWorkforceId => _pemohonWfp;

    /// <summary>Profil tenaga kerja staf unit, sebagai petugas peminta kedua pada uji saringan.</summary>
    public Guid StafWorkforceId => _stafWfp;

    /// <summary>
    /// Baris master obat dan satuan uji untuk <see cref="ObatId"/> dan <see cref="SatuanId"/>,
    /// supaya perintah buat dan ubah permintaan — yang memeriksa master farmasi — dapat diuji.
    /// </summary>
    public async Task SediakanMasterObatAsync()
    {
        using var k = _database.CreateContext();
        k.MstDrugs.Add(new MstDrug
        {
            Id = ObatId, DrugCategoryId = Guid.NewGuid(), DrugCode = "UJI-OBT-1",
            DrugName = "Obat Uji Satu", IsActive = true, CreateDateTime = DateTime.UtcNow
        });
        k.MstMeasurements.Add(new MstMeasurement
        {
            Id = SatuanId, MeasurementCode = "UJI-SAT-1", MeasurementName = "Tablet (Uji)",
            IsActive = true, CreateDateTime = DateTime.UtcNow
        });
        await k.SaveChangesAsync();
    }

    /// <summary>Menonaktifkan controller izin <c>StockRequest</c> pada basis data uji.</summary>
    public async Task NonaktifkanControllerIzinAsync()
    {
        using var k = _database.CreateContext();
        var controller = await k.SysControllerAccesses
            .SingleAsync(x => x.ControllerName == StockRequestApprovalAuthority.PermissionResource);
        controller.IsActive = false;
        await k.SaveChangesAsync();
    }

    /// <summary>
    /// Memundurkan waktu otorisasi CITO pada riwayat, hanya di basis data uji, supaya keadaan
    /// tenggat dapat diuji tanpa menunggu jam berjalan.
    /// </summary>
    public async Task MundurkanOtorisasiAsync(Guid stockRequestId, TimeSpan mundur)
    {
        using var k = _database.CreateContext();
        var riwayat = await k.PhmStockRequestHistories
            .SingleAsync(x => x.StockRequestId == stockRequestId && x.Action == "AuthorizeEmergencyStockRequest");
        riwayat.OccurredAt = riwayat.OccurredAt - mundur;
        await k.SaveChangesAsync();
    }

    /// <summary>Permintaan CITO yang sudah diotorisasi lewat jalur darurat oleh <paramref name="pengotorisasi"/>.</summary>
    public async Task<(Guid Id, Guid ItemId, int Versi)> CitoTerotorisasiAsync(Guid? pengotorisasi = null)
    {
        var (id, itemId) = await PermintaanAsync(prioritas: StockRequestPriority.Emergency);
        var hasil = await Layanan(CreateContext(), pengotorisasi ?? PetugasCitoUserId)
            .AuthorizeEmergencyAsync(id, new()
            {
                Reason = "Pasien IGD, stok depo habis",
                ExpectedVersion = VersiAwal,
                IdempotencyKey = $"uji-cito-{id:N}"
            });
        return (id, itemId, hasil.Version);
    }

    private static PhmStockRequestHistory Riwayat(Guid requestId, StockRequestStatus? dari,
        StockRequestStatus ke, string aksi, Guid pelaku, DateTime pada) => new()
        {
            StockRequestId = requestId,
            FromStatus = dari,
            ToStatus = ke,
            Action = aksi,
            ActorUserId = pelaku,
            OccurredAt = pada,
            Source = "UJI:seed",
            CorrelationId = $"uji-{aksi}-{requestId:N}",
            CreateBy = pelaku,
            CreateDateTime = pada
        };

    private static MstServiceUnit Unit(Guid id, string kode, string nama, Guid? orgUnitId) => new()
    {
        Id = id,
        ServiceUnitCode = kode,
        ServiceUnitName = nama,
        OrganizationUnitId = orgUnitId,
        IsActive = true,
        CreateDateTime = DateTime.UtcNow
    };

    private static void Pengguna(ApplicationDbContext k, Guid userId, Guid? workforceProfileId)
    {
        var nama = $"uji-{userId:N}";
        k.Users.Add(new ApplicationUser
        {
            Id = userId,
            UserName = nama,
            NormalizedUserName = nama.ToUpperInvariant(),
            UserCode = userId.ToString("N")[..12],
            DisplayName = nama,
            WorkforceProfileId = workforceProfileId,
            IsActive = true
        });
    }

    private void Penempatan(ApplicationDbContext k, Guid workforceProfileId, Guid orgUnitId,
        bool manajerial, DateTime mulai, DateTime? akhir)
    {
        k.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
        {
            WorkforceProfileId = workforceProfileId,
            OrganizationUnitId = orgUnitId,
            DepartmentId = DepartemenId,
            PositionId = JabatanPenyetujuId,
            IsManagerialAssignment = manajerial,
            IsPrimary = true,
            EffectiveStartDate = mulai,
            EffectiveEndDate = akhir,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });
    }

    private void PenempatanJabatan(ApplicationDbContext k, Guid userId, Guid jabatanId, DateTime? akhir)
    {
        k.ApplicationUserOrganizations.Add(new ApplicationUserOrganization
        {
            UserId = userId,
            DepartmentId = DepartemenId,
            PositionId = jabatanId,
            IsActive = true,
            EffectiveStartDate = DateTime.UtcNow.AddYears(-1),
            EffectiveEndDate = akhir,
            CreateDateTime = DateTime.UtcNow
        });
    }

    private static IHttpContextAccessor Accessor(Guid userId)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Uji"))
        };

        // Bukan HttpContextAccessor bawaan, alasannya sama dengan PharmacyHarness: yang bawaan
        // menyimpan konteksnya pada AsyncLocal statis sehingga pengguna dua service saling timpa.
        return new AccessorTetap(context);
    }

    private sealed class AccessorTetap(HttpContext context) : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; } = context;
    }

    public void Dispose()
    {
        foreach (var konteks in _contexts) konteks.Dispose();
        _database.Dispose();
        _databaseNomor?.Dispose();
    }
}
