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
/// Struktur organisasinya memakai tabel yang sudah ada, persis seperti yang dibaca
/// <see cref="StockRequestApprovalAuthority"/>:
/// </para>
/// <list type="bullet">
/// <item>unit layanan <see cref="UnitId"/> → unit organisasi <see cref="OrgUnitId"/>;</item>
/// <item>unit layanan <see cref="UnitLainId"/> → <see cref="OrgUnitLainId"/>;</item>
/// <item>unit layanan <see cref="UnitTanpaTautanId"/> belum ditautkan;</item>
/// <item>atasan unit = penempatan manajerial HR (<c>WfpOrganizationAssignment</c>) yang aktif;</item>
/// <item>izin = penempatan jabatan pengguna (<c>ApplicationUserOrganization</c>) × kebijakan akses.</item>
/// </list>
/// <para>
/// Jabatan <see cref="JabatanPenyetujuId"/> diberi izin <c>StockRequest:Approve</c>; jabatan
/// <see cref="JabatanCitoId"/> diberi izin <c>StockRequest:AuthorizeEmergency</c>. Peran
/// SuperAdmin sengaja disiapkan tanpa penempatan apa pun.
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

    /// <summary>Pemohon; sekaligus atasan unit, berizin Approve dan CITO — untuk menguji larangan memutus sendiri.</summary>
    public Guid PemohonUserId { get; } = Guid.NewGuid();
    /// <summary>Atasan unit <see cref="UnitId"/> dengan izin Approve.</summary>
    public Guid AtasanUserId { get; } = Guid.NewGuid();
    /// <summary>Atasan unit <see cref="UnitId"/> dengan izin Approve dan CITO.</summary>
    public Guid AtasanCitoUserId { get; } = Guid.NewGuid();
    /// <summary>Atasan unit <see cref="UnitLainId"/> dengan izin Approve.</summary>
    public Guid AtasanLainUserId { get; } = Guid.NewGuid();
    /// <summary>Penempatan manajerialnya sudah berakhir.</summary>
    public Guid AtasanKedaluwarsaUserId { get; } = Guid.NewGuid();
    /// <summary>Atasan unit <see cref="UnitId"/> tanpa izin Approve.</summary>
    public Guid AtasanTanpaIzinUserId { get; } = Guid.NewGuid();
    /// <summary>Atasan unit <see cref="UnitId"/> yang profil tenaga kerjanya sudah dinonaktifkan.</summary>
    public Guid AtasanNonaktifUserId { get; } = Guid.NewGuid();
    /// <summary>Petugas berizin CITO, bukan atasan unit.</summary>
    public Guid PetugasCitoUserId { get; } = Guid.NewGuid();
    /// <summary>Pernah berizin CITO, penempatannya sudah berakhir.</summary>
    public Guid PetugasCitoKedaluwarsaUserId { get; } = Guid.NewGuid();
    /// <summary>Staf unit yang sama dengan izin Approve, tanpa penempatan manajerial.</summary>
    public Guid StafUserId { get; } = Guid.NewGuid();
    /// <summary>Berperan SuperAdmin, tanpa profil tenaga kerja dan tanpa penempatan apa pun.</summary>
    public Guid SuperAdminUserId { get; } = Guid.NewGuid();

    private readonly Guid _pemohonWfp = Guid.NewGuid();
    private readonly Guid _atasanWfp = Guid.NewGuid();
    private readonly Guid _atasanCitoWfp = Guid.NewGuid();
    private readonly Guid _atasanLainWfp = Guid.NewGuid();
    private readonly Guid _atasanKedaluwarsaWfp = Guid.NewGuid();
    private readonly Guid _atasanTanpaIzinWfp = Guid.NewGuid();
    private readonly Guid _atasanNonaktifWfp = Guid.NewGuid();
    private readonly Guid _petugasCitoWfp = Guid.NewGuid();
    private readonly Guid _stafWfp = Guid.NewGuid();

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
                     (_petugasCitoWfp, "UJI-WFP-CITO", true), (_stafWfp, "UJI-WFP-STAF", true)
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

        var peranSuperAdmin = new ApplicationRole
        {
            Id = Guid.NewGuid(), Name = "SuperAdmin", NormalizedName = "SUPERADMIN"
        };
        k.Roles.Add(peranSuperAdmin);
        k.UserRoles.Add(new IdentityUserRole<Guid> { UserId = SuperAdminUserId, RoleId = peranSuperAdmin.Id });

        // Atasan unit: penempatan manajerial HR pada unit organisasi yang ditautkan.
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
                     AtasanKedaluwarsaUserId, AtasanNonaktifUserId, StafUserId
                 })
            PenempatanJabatan(k, userId, JabatanPenyetujuId, akhir: null);

        PenempatanJabatan(k, PemohonUserId, JabatanCitoId, akhir: null);
        PenempatanJabatan(k, AtasanCitoUserId, JabatanCitoId, akhir: null);
        PenempatanJabatan(k, PetugasCitoUserId, JabatanCitoId, akhir: null);
        PenempatanJabatan(k, PetugasCitoKedaluwarsaUserId, JabatanCitoId, akhir: now.AddDays(-1));
        // Atasan tanpa izin hanya memegang jabatan staf yang tidak diberi izin apa pun.
        PenempatanJabatan(k, AtasanTanpaIzinUserId, JabatanStafId, akhir: null);

        k.SaveChanges();
    }

    public ApplicationDbContext CreateContext()
    {
        var konteks = _database.CreateContext();
        _contexts.Add(konteks);
        return konteks;
    }

    /// <summary>
    /// Service permintaan stok yang bertindak sebagai <paramref name="userId"/>.
    /// </summary>
    /// <param name="batasAktivasi">
    /// Nilai <see cref="StockRequestService.ApprovalEnforcedFromConfigKey"/>; kosong berarti tidak diatur.
    /// </param>
    /// <param name="denganStok">Bila benar, penyerahan benar-benar mengurangi stok lewat <see cref="DrugStockService"/>.</param>
    public StockRequestService Layanan(ApplicationDbContext k, Guid userId,
        string? batasAktivasi = null, bool denganStok = false)
    {
        var accessor = Accessor(userId);
        var pencatat = new LoggerService(NullLogger<LoggerService>.Instance, accessor);
        var konfigurasi = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [StockRequestService.ApprovalEnforcedFromConfigKey] = batasAktivasi
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
        bool tanpaWaktuPengajuan = false)
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
            RequestedByWorkforceId = _pemohonWfp,
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
    }
}
