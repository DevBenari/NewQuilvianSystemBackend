using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.BillingManagement;

// BE-BKC-038 — capstone hardening lintas-slice Petty Cash. Menutup BIL-AT-072 (tidak ada
// PUT/PATCH/resubmit pada voucher), BIL-AT-077 (kas shift kasir tidak bergerak akibat aktivitas
// kas kecil - uji regresi paling penting seluruh rumpun ini), BIL-AT-078 (pasangan
// [AccessAction]/[AccessPermission] sama persis pada seluruh action ketiga controller baru), dan
// BIL-AT-079 (RecipientName/Purpose/RejectionReason/ResponseJson tidak pernah tercetak ke log
// aplikasi, sementara VoucherId/VoucherNumber/nominal/status/ActorUserId tercetak).
public sealed class PettyCashHardeningTests
{
    private static readonly Guid Actor = Guid.NewGuid();
    private const string ActorRole = "Kepala Kasir";

    private static readonly Type[] PettyCashControllers =
    {
        typeof(PettyCashCategoriesController),
        typeof(PettyCashBudgetController),
        typeof(PettyCashVouchersController)
    };

    // ---------------------------------------------------------------------------------------
    // BIL-AT-078 (bagian 1) — setiap action pada ketiga controller: [AccessAction] argumen
    // pertama sama persis dengan [AccessPermission] argumen kedua, dan AccessType valid. Karena
    // AccessMenuSeeder membentuk baris Akses Role langsung dari kedua atribut ini lewat refleksi
    // (Seeders/AccessMenuSeeder.cs), pasangan yang benar di sini SUDAH cukup untuk memastikan
    // barisnya benar-benar muncul dan bisa dicentang admin - tidak ada daftar seeder terpisah
    // yang bisa menyimpang seperti kasus BE-RWI-034 pada modul lain.
    // ---------------------------------------------------------------------------------------

    [Fact]
    public void SetiapActionPadaKetigaController_AccessActionDanAccessPermissionSamaPersis()
    {
        var kesalahan = new List<string>();

        foreach (var controllerType in PettyCashControllers)
        {
            var controllerAttribute = controllerType.GetCustomAttribute<AccessControllerAttribute>();
            Assert.NotNull(controllerAttribute);
            var controllerName = controllerAttribute!.ControllerName;
            Assert.False(string.IsNullOrWhiteSpace(controllerName));

            foreach (var endpoint in EndpointsOf(controllerType))
            {
                var actionAttribute = endpoint.GetCustomAttribute<AccessActionAttribute>();
                var permissionAttribute = endpoint.GetCustomAttribute<AccessPermissionAttribute>();

                if (actionAttribute is null)
                {
                    kesalahan.Add($"{controllerType.Name}.{endpoint.Name}: tidak ada [AccessAction]");
                    continue;
                }
                if (permissionAttribute is null)
                {
                    kesalahan.Add($"{controllerType.Name}.{endpoint.Name}: tidak ada [AccessPermission]");
                    continue;
                }

                var permissionResource = (string)permissionAttribute.Arguments![0]!;
                var permissionAction = (string)permissionAttribute.Arguments![1]!;

                if (permissionResource != controllerName)
                    kesalahan.Add(
                        $"{controllerType.Name}.{endpoint.Name}: [AccessPermission] argumen pertama " +
                        $"\"{permissionResource}\" != ControllerName \"{controllerName}\"");
                if (permissionAction != actionAttribute.ActionName)
                    kesalahan.Add(
                        $"{controllerType.Name}.{endpoint.Name}: [AccessPermission] argumen kedua " +
                        $"\"{permissionAction}\" != [AccessAction] argumen pertama \"{actionAttribute.ActionName}\"");
                if (!AccessTypes.AllowedForRoleAccess.Contains(actionAttribute.AccessType))
                    kesalahan.Add(
                        $"{controllerType.Name}.{endpoint.Name}: AccessType \"{actionAttribute.AccessType}\" " +
                        "bukan salah satu dari Read/Create/Update/Delete");
            }
        }

        Assert.True(kesalahan.Count == 0, string.Join(Environment.NewLine, kesalahan));
    }

    // ---------------------------------------------------------------------------------------
    // BIL-AT-072 — tidak ada satu pun endpoint PUT, PATCH, atau resubmit pada voucher. Budget
    // juga transaksi (bukan master data) sehingga diperiksa hal yang sama; Category adalah
    // master data dan MEMANG berhak punya PUT/PATCH/DELETE (master-data-endpoint-standard).
    // ---------------------------------------------------------------------------------------

    [Fact]
    public void VoucherDanBudgetController_TidakPunyaEndpointPutPatchAtauDelete()
    {
        foreach (var controllerType in new[] { typeof(PettyCashVouchersController), typeof(PettyCashBudgetController) })
        {
            foreach (var endpoint in EndpointsOf(controllerType))
            {
                var methods = endpoint.GetCustomAttributes<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>()
                    .SelectMany(x => x.HttpMethods);
                Assert.DoesNotContain(methods, m =>
                    string.Equals(m, "PUT", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(m, "PATCH", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(m, "DELETE", StringComparison.OrdinalIgnoreCase));
            }
        }
    }

    [Fact]
    public void PettyCashCategoryController_MasterDataSehinggaBolehPunyaPutPatchDelete()
    {
        var methods = EndpointsOf(typeof(PettyCashCategoriesController))
            .SelectMany(x => x.GetCustomAttributes<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>())
            .SelectMany(x => x.HttpMethods)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("PUT", methods);
        Assert.Contains("PATCH", methods);
        Assert.Contains("DELETE", methods);
    }

    // ---------------------------------------------------------------------------------------
    // BIL-AT-077 — uji regresi paling penting seluruh rumpun ini: kas shift kasir tidak bergerak
    // satu rupiah pun akibat aktivitas kas kecil.
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task PettyCashVoucherDisbursement_DoesNotMoveCashierShiftTotals()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (voucherService, _) = CreatePettyCashServices(db);
        var shiftService = CreateCashierShiftService(db);
        await SeedBudgetAsync(db, 5_000_000m);
        var category = await SeedActiveCategoryAsync(db);

        var opened = await shiftService.OpenAsync(
            new OpenShiftRequest { RegisterId = Guid.NewGuid(), OpeningCash = 100_000m, CorrelationId = Guid.NewGuid(), CausationId = Guid.NewGuid() },
            Guid.NewGuid(), Actor, "Cashier", CancellationToken.None);
        var shift = db.BilCashierShifts.Single();

        Assert.True(await shiftService.ApplyCashReceiptAsync(
            shift, "TENDER", Guid.NewGuid(), 250_000m, Actor, Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, CancellationToken.None));
        await db.SaveChangesAsync(CancellationToken.None);
        var systemCashAfterPatientPayment = shift.SystemCash;
        Assert.Equal(250_000m, systemCashAfterPatientPayment);

        // Aktivitas kas kecil penuh: dibuat, disetujui, uangnya diserahkan - PettyCashVoucherService.
        // DisburseAsync sengaja TIDAK memanggil CashierShiftService/menyentuh BilCashierShift sama
        // sekali (PC-DEC-001, lihat laporan BE-BKC-037 § 6).
        var voucher = await voucherService.CreateAsync(
            CreateRequest(category.Id, 300_000m), Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        var approved = await voucherService.ApproveAsync(
            voucher.Id, new ApprovePettyCashVoucherRequest { ExpectedRowVersion = voucher.RowVersion },
            Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        await voucherService.DisburseAsync(
            voucher.Id, new DisbursePettyCashVoucherRequest { ExpectedRowVersion = approved.RowVersion },
            Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);

        var shiftAfterDisbursement = await db.BilCashierShifts.SingleAsync(x => x.Id == shift.Id);
        Assert.Equal(systemCashAfterPatientPayment, shiftAfterDisbursement.SystemCash);
        Assert.Equal(100_000m, shiftAfterDisbursement.OpeningCash);

        var closed = await shiftService.CloseAsync(
            shift.Id,
            new CloseShiftRequest
            {
                PhysicalCash = 350_000m,
                ExpectedRowVersion = shiftAfterDisbursement.RowVersion,
                CorrelationId = Guid.NewGuid(),
                CausationId = Guid.NewGuid()
            },
            Guid.NewGuid(), Actor, "Cashier", CancellationToken.None);

        // 350.000 = OpeningCash 100.000 + SystemCash 250.000, PERSIS seperti bila voucher kas
        // kecil tidak pernah dicairkan sama sekali - Variance nol membuktikannya berangka.
        Assert.Equal(CashierShiftStatuses.Closed, closed.Status);
        Assert.Equal(250_000m, closed.SystemCash);
        Assert.Equal(350_000m, closed.ExpectedClosingCash);
        Assert.Equal(0m, closed.Variance);
    }

    // ---------------------------------------------------------------------------------------
    // BIL-AT-079 — RecipientName/Purpose/RejectionReason/ResponseJson tidak pernah tercetak ke
    // log aplikasi (pencarian teks pada SELURUH keluaran log, bukan field per field), sementara
    // VoucherId/VoucherNumber/nominal/status/ActorUserId tercetak.
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task FullLifecycleRejectAndCancel_NeverLogSensitiveFields_ButLogIdentifyingFields()
    {
        const string secretRecipient = "PENERIMA-RAHASIA-8f2c";
        const string secretPurpose = "TUJUAN-RAHASIA-91ab";
        const string secretRejectionReason = "ALASAN-TOLAK-RAHASIA-77dd";
        const string secretCancelReason = "ALASAN-BATAL-RAHASIA-22ee";

        await using var db = IsolatedBillingDbContextFactory.Create();
        var capturingLogger = new CapturingLogger();
        var (voucherService, _) = CreatePettyCashServices(db, capturingLogger);
        await SeedBudgetAsync(db, 5_000_000m);
        var category = await SeedActiveCategoryAsync(db);

        // Voucher 1: alur penuh sampai Selesai (COMPLETED) - membawa RecipientName/Purpose.
        var completedRequest = new CreatePettyCashVoucherRequest
        {
            RecipientName = secretRecipient,
            CategoryId = category.Id,
            Amount = 300_000m,
            Purpose = secretPurpose
        };
        var created = await voucherService.CreateAsync(completedRequest, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        var approved = await voucherService.ApproveAsync(
            created.Id, new ApprovePettyCashVoucherRequest { ExpectedRowVersion = created.RowVersion }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        var disbursed = await voucherService.DisburseAsync(
            created.Id, new DisbursePettyCashVoucherRequest { ExpectedRowVersion = approved.RowVersion }, Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        var completed = await voucherService.AttachProofAsync(
            created.Id, new AttachPettyCashProofRequest { ExpectedRowVersion = disbursed.RowVersion, ProofReferenceNumber = "NT-0001" },
            Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);

        // Voucher 2: ditolak - membawa RejectionReason.
        var rejectedSource = await voucherService.CreateAsync(
            CreateRequest(category.Id, 150_000m), Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        await voucherService.RejectAsync(
            rejectedSource.Id,
            new RejectPettyCashVoucherRequest { ExpectedRowVersion = rejectedSource.RowVersion, RejectionReason = secretRejectionReason },
            Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);

        // Voucher 3: dibatalkan pemohon sendiri - membawa Reason (kolom Reason yang sama dengan REJECT).
        var cancelledSource = await voucherService.CreateAsync(
            CreateRequest(category.Id, 120_000m), Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);
        await voucherService.CancelAsync(
            cancelledSource.Id,
            new CancelPettyCashVoucherRequest { ExpectedRowVersion = cancelledSource.RowVersion, Reason = secretCancelReason },
            Guid.NewGuid(), Actor, ActorRole, CancellationToken.None);

        var fullLogText = string.Join('\n', capturingLogger.CapturedText);
        Assert.NotEmpty(fullLogText);

        Assert.DoesNotContain(secretRecipient, fullLogText, StringComparison.Ordinal);
        Assert.DoesNotContain(secretPurpose, fullLogText, StringComparison.Ordinal);
        Assert.DoesNotContain(secretRejectionReason, fullLogText, StringComparison.Ordinal);
        Assert.DoesNotContain(secretCancelReason, fullLogText, StringComparison.Ordinal);
        // ResponseJson (yang memuat RecipientName/Purpose di atas) sendiri tidak pernah dilewatkan
        // ke logger sama sekali (lihat PettyCashVoucherService.AuditCommandAsync) - dibuktikan
        // transitif oleh dua assersi di atas, karena keduanya adalah isi ResponseJson.

        Assert.Contains(created.Id.ToString(), fullLogText, StringComparison.Ordinal);
        Assert.Contains(created.VoucherNumber, fullLogText, StringComparison.Ordinal);
        Assert.Contains("300000", fullLogText, StringComparison.Ordinal);
        Assert.Contains(PettyCashVoucherStatuses.WaitingApproval, fullLogText, StringComparison.Ordinal);
        Assert.Contains(PettyCashVoucherStatuses.Approved, fullLogText, StringComparison.Ordinal);
        Assert.Contains(PettyCashVoucherStatuses.CashReceived, fullLogText, StringComparison.Ordinal);
        Assert.Contains(PettyCashVoucherStatuses.Completed, fullLogText, StringComparison.Ordinal);
        Assert.Contains(PettyCashVoucherStatuses.Rejected, fullLogText, StringComparison.Ordinal);
        Assert.Contains(Actor.ToString(), fullLogText, StringComparison.Ordinal);
        Assert.NotNull(completed.CompletedAt);
    }

    // ---------------------------------------------------------------------------------------
    // Perkakas
    // ---------------------------------------------------------------------------------------

    private static List<MethodInfo> EndpointsOf(Type controllerType) =>
        controllerType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => !x.IsSpecialName)
            .Where(x => x.GetCustomAttributes<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>().Any())
            .ToList();

    private static (PettyCashVoucherService Service, PettyCashBudgetService Budget) CreatePettyCashServices(
        ApplicationDbContext db, ILogger<LoggerService>? logger = null)
    {
        var loggerService = new LoggerService(logger ?? NullLogger<LoggerService>.Instance, new HttpContextAccessor());
        var numberSeries = new BillingNumberSeriesService(db, Options.Create(new BillingInvoiceNumberOptions()));
        var categoryService = new PettyCashCategoryService(db, loggerService);
        var budgetService = new PettyCashBudgetService(db, loggerService);
        var voucherService = new PettyCashVoucherService(db, numberSeries, categoryService, budgetService, loggerService);
        return (voucherService, budgetService);
    }

    private static CashierShiftService CreateCashierShiftService(ApplicationDbContext db)
    {
        var loggerService = new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor());
        var numberSeries = new BillingNumberSeriesService(
            db,
            Options.Create(new BillingInvoiceNumberOptions()),
            Options.Create(new BillingDepositAccountNumberOptions()),
            Options.Create(new BillingCashierShiftNumberOptions()));
        return new CashierShiftService(db, numberSeries, loggerService);
    }

    private static async Task<BilPettyCashBudget> SeedBudgetAsync(ApplicationDbContext db, decimal currentBalance)
    {
        var budget = new BilPettyCashBudget
        {
            PoolCode = "HOSPITAL_MAIN",
            PoolName = "Kas Kecil Rumah Sakit",
            CurrentBalance = currentBalance,
            Status = PettyCashBudgetStatuses.Active,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = Actor
        };
        db.BilPettyCashBudgets.Add(budget);
        await db.SaveChangesAsync(CancellationToken.None);
        return budget;
    }

    private static async Task<MstPettyCashCategory> SeedActiveCategoryAsync(ApplicationDbContext db)
    {
        var category = new MstPettyCashCategory
        {
            CategoryCode = "TRANSPORT",
            CategoryName = "Transport",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = Actor
        };
        db.MstPettyCashCategories.Add(category);
        await db.SaveChangesAsync(CancellationToken.None);
        return category;
    }

    private static CreatePettyCashVoucherRequest CreateRequest(Guid categoryId, decimal amount = 300_000m) => new()
    {
        RecipientName = "Budi Santoso",
        CategoryId = categoryId,
        Amount = amount,
        Purpose = "Ongkos kirim dokumen"
    };

    // Menangkap SELURUH teks yang benar-benar tercetak ILogger - baik lewat pemanggilan Log(...)
    // (formatter(state, exception), persis argumen "{DisplayMessage}" yang dikirim LoggerService)
    // maupun lewat BeginScope(...) (dictionary scopeProperties milik LoggerService.WriteAsync).
    // Inilah satu-satunya cara membuktikan BIL-AT-079 secara jujur: memeriksa apa yang BENAR-BENAR
    // menjadi output logging, bukan menduga dari isi parameter "data" yang dikirim ke AuditAsync.
    private sealed class CapturingLogger : ILogger<LoggerService>
    {
        public List<string> CapturedText { get; } = new();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            if (state is IEnumerable<KeyValuePair<string, object?>> properties)
            {
                foreach (var property in properties)
                    CapturedText.Add($"{property.Key}={property.Value}");
            }
            return NoopScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            CapturedText.Add(formatter(state, exception));

        private sealed class NoopScope : IDisposable
        {
            public static readonly NoopScope Instance = new();
            public void Dispose() { }
        }
    }
}
