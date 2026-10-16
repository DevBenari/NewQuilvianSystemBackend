using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using System.Security.Claims;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Source Unit Tests untuk alur End-to-End:
/// Admisi Rawat Inap → Deposit Payment Order → Billing–Kasir / Pembayaran Deposit → Penerimaan Top-Up → Saldo Deposit → Riwayat dan Kwitansi.
/// Menutup seluruh skenario penerimaan, pencarian, validasi identitas, partial payment, overpayment rejection, dan receipt generator.
/// CATATAN: File test ini disiapkan sebagai sumber kode verifikasi dan TIDAK DIJALANKAN (sesuai larangan dotnet test).
/// </summary>
public class InpatientDepositPaymentOrderTests : IDisposable
{
    private readonly TestDatabase _database;
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingDepositService _depositService;
    private readonly BillingDepositOrderService _depositOrderService;
    private readonly BillingDepositOrdersController _depositOrdersController;
    private readonly Guid _actorUserId = Guid.NewGuid();
    private readonly Guid _patientId = Guid.NewGuid();
    private readonly Guid _cashPaymentMethodId = Guid.NewGuid();
    private readonly Guid _transferPaymentMethodId = Guid.NewGuid();
    private readonly Guid _bankAccountId = Guid.NewGuid();

    public InpatientDepositPaymentOrderTests()
    {
        _database = TestDatabase.Create();
        _dbContext = _database.CreateContext();

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _actorUserId.ToString()),
            new Claim("user_id", _actorUserId.ToString()),
            new Claim(ClaimTypes.Name, "Petugas Kasir Test")
        };
        httpContextAccessor.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var loggerService = new LoggerService(NullLogger<LoggerService>.Instance, httpContextAccessor);
        var numberSeries = new BillingNumberSeriesService(_dbContext);
        var cashierShiftService = new CashierShiftService(_dbContext, numberSeries, loggerService);

        _depositService = new BillingDepositService(_dbContext, numberSeries, cashierShiftService, loggerService);
        _depositOrderService = new BillingDepositOrderService(_dbContext, _depositService, numberSeries, loggerService);
        _depositOrdersController = new BillingDepositOrdersController(_depositOrderService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContextAccessor.HttpContext
            }
        };

        SeedMasterData();
    }

    private void SeedMasterData()
    {
        var patient = new MstPatient
        {
            Id = _patientId,
            MedicalRecordNumber = "RM-2026-0001",
            FullName = "Pasien Uji Ranap",
            IdentityNumber = "3174123456780001",
            BirthDate = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Gender = "L",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.MstPatients.Add(patient);

        var cashMethod = new MstPaymentMethod
        {
            Id = _cashPaymentMethodId,
            Code = "CASH",
            Name = "Tunai",
            Category = "CASH",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.MstPaymentMethods.Add(cashMethod);

        var trfMethod = new MstPaymentMethod
        {
            Id = _transferPaymentMethodId,
            Code = "BANK_TRF",
            Name = "Transfer Bank",
            Category = "TRANSFER",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _actorUserId
        };
        _dbContext.MstPaymentMethods.Add(trfMethod);

        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task Scenario01_AdmisiDeposit10Juta_MembuatOrderUNPAID_SaldoDepositMasihNol()
    {
        // Arrange
        var episodeId = Guid.NewGuid();
        var encounterId = Guid.NewGuid();

        // Act - Admisi mengajukan nominal deposit diminta Rp 10.000.000
        var order = await _depositOrderService.CreateOrderAsync(new CreateDepositPaymentOrderRequest
        {
            EpisodeId = episodeId,
            EncounterId = encounterId,
            PatientId = _patientId,
            RequestedAmount = 10_000_000m,
            OrderType = BilDepositOrderType.InitialDeposit,
            SourceType = BilDepositOrderSourceType.InpatientAdmission,
            Remarks = "Permintaan deposit pertama admisi ranap",
            IdempotencyKey = $"IDEMP-{episodeId}"
        }, _actorUserId);

        // Assert
        Assert.NotNull(order);
        Assert.Equal("UNPAID", order.Status);
        Assert.Equal(10_000_000m, order.RequestedAmount);
        Assert.Equal(0m, order.PaidAmount);
        Assert.Equal(10_000_000m, order.OutstandingAmount);

        // Uang belum diterima, saldo deposit akun harus Rp 0
        var account = await _dbContext.BilDepositAccounts
            .FirstOrDefaultAsync(x => x.EncounterId == encounterId && !x.IsDelete);
        Assert.True(account == null || account.AvailableBalance == 0m);
    }

    [Fact]
    public async Task Scenario02_PencarianOrder_BerdasarkanNoRM_NamaPasien_Dan_NIK()
    {
        // Arrange
        var episodeId = Guid.NewGuid();
        var encounterId = Guid.NewGuid();
        await _depositOrderService.CreateOrderAsync(new CreateDepositPaymentOrderRequest
        {
            EpisodeId = episodeId,
            EncounterId = encounterId,
            PatientId = _patientId,
            RequestedAmount = 5_000_000m
        }, _actorUserId);

        // Act 1: Cari berdasarkan No. RM
        var byRm = await _depositOrderService.SearchOrdersAsync(new DepositOrderSearchQuery
        {
            Search = "RM-2026-0001"
        });
        Assert.NotEmpty(byRm.Items);
        Assert.Equal("RM-2026-0001", byRm.Items.First().MedicalRecordNumber);

        // Act 2: Cari berdasarkan Nama Pasien (case-insensitive)
        var byName = await _depositOrderService.SearchOrdersAsync(new DepositOrderSearchQuery
        {
            Search = "uji ranap"
        });
        Assert.NotEmpty(byName.Items);

        // Act 3: Cari berdasarkan NIK
        var byNik = await _depositOrderService.SearchOrdersAsync(new DepositOrderSearchQuery
        {
            Search = "3174123456780001"
        });
        Assert.NotEmpty(byNik.Items);
        // NIK harus di-masking untuk keamanan tabel umum
        Assert.Contains("****", byNik.Items.First().IdentityNumberMasked);
    }

    [Fact]
    public async Task Scenario03_PembayaranBertahap_4Juta_Menjadi_PARTIALLY_PAID_Lalu_6Juta_Menjadi_PAID()
    {
        // Arrange
        var episodeId = Guid.NewGuid();
        var encounterId = Guid.NewGuid();
        var order = await _depositOrderService.CreateOrderAsync(new CreateDepositPaymentOrderRequest
        {
            EpisodeId = episodeId,
            EncounterId = encounterId,
            PatientId = _patientId,
            RequestedAmount = 10_000_000m
        }, _actorUserId);

        // Act 1: Kasir menerima pembayaran pertama Rp 4.000.000
        var pay1 = await _depositOrderService.PayOrderAsync(order.Id, new PayDepositOrderRequest
        {
            Amount = 4_000_000m,
            PaymentMethodId = _cashPaymentMethodId,
            Reason = "Pembayaran pertama uang muka",
            IdempotencyKey = "PAY-STEP-1"
        }, _actorUserId);

        // Assert 1: Status PARTIALLY_PAID, sisa Rp 6.000.000, saldo deposit bertambah Rp 4.000.000
        Assert.Equal("PARTIALLY_PAID", pay1.Status);
        Assert.Equal(4_000_000m, pay1.PaidAmount);
        Assert.Equal(6_000_000m, pay1.OutstandingAmount);

        var depositAccount = await _dbContext.BilDepositAccounts
            .FirstAsync(x => x.EncounterId == encounterId);
        Assert.Equal(4_000_000m, depositAccount.AvailableBalance);

        // Act 2: Kasir menerima pelunasan kedua Rp 6.000.000
        var pay2 = await _depositOrderService.PayOrderAsync(order.Id, new PayDepositOrderRequest
        {
            Amount = 6_000_000m,
            PaymentMethodId = _cashPaymentMethodId,
            Reason = "Pelunasan deposit ranap",
            IdempotencyKey = "PAY-STEP-2"
        }, _actorUserId);

        // Assert 2: Status PAID, sisa Rp 0, total saldo deposit Rp 10.000.000
        Assert.Equal("PAID", pay2.Status);
        Assert.Equal(10_000_000m, pay2.PaidAmount);
        Assert.Equal(0m, pay2.OutstandingAmount);

        var movements = await _dbContext.BilDepositMovements
            .Where(x => x.AccountId == depositAccount.Id && x.MovementType == "TOP_UP")
            .ToListAsync();
        Assert.Equal(2, movements.Count); // Dua mutasi TOP_UP terpisah dan sah
    }

    [Fact]
    public async Task Scenario04_OverpaymentDitolak()
    {
        // Arrange
        var episodeId = Guid.NewGuid();
        var encounterId = Guid.NewGuid();
        var order = await _depositOrderService.CreateOrderAsync(new CreateDepositPaymentOrderRequest
        {
            EpisodeId = episodeId,
            EncounterId = encounterId,
            PatientId = _patientId,
            RequestedAmount = 5_000_000m
        }, _actorUserId);

        // Act & Assert: Kasir membayar Rp 6.000.000 (melebihi outstanding 5jt) -> Wajib ditolak
        var ex = await Assert.ThrowsAsync<BillingDepositValidationException>(async () =>
        {
            await _depositOrderService.PayOrderAsync(order.Id, new PayDepositOrderRequest
            {
                Amount = 6_000_000m,
                PaymentMethodId = _cashPaymentMethodId
            }, _actorUserId);
        });

        Assert.Contains("melebihi sisa kewajiban", ex.Message);
    }

    [Fact]
    public async Task Scenario05_PembayaranTransferTanpaReferensi_WajibDitolak()
    {
        // Arrange
        var episodeId = Guid.NewGuid();
        var encounterId = Guid.NewGuid();
        var order = await _depositOrderService.CreateOrderAsync(new CreateDepositPaymentOrderRequest
        {
            EpisodeId = episodeId,
            EncounterId = encounterId,
            PatientId = _patientId,
            RequestedAmount = 5_000_000m
        }, _actorUserId);

        // Act & Assert: Metode transfer tanpa reference number -> Ditolak
        var ex = await Assert.ThrowsAsync<BillingDepositValidationException>(async () =>
        {
            await _depositOrderService.PayOrderAsync(order.Id, new PayDepositOrderRequest
            {
                Amount = 5_000_000m,
                PaymentMethodId = _transferPaymentMethodId,
                ReferenceNumber = "" // Kosong
            }, _actorUserId);
        });

        Assert.Contains("Nomor referensi / bukti transfer wajib diisi", ex.Message);
    }

    [Fact]
    public async Task Scenario06_OrderCancelled_TidakBisaDibayar()
    {
        // Arrange
        var episodeId = Guid.NewGuid();
        var encounterId = Guid.NewGuid();
        var order = await _depositOrderService.CreateOrderAsync(new CreateDepositPaymentOrderRequest
        {
            EpisodeId = episodeId,
            EncounterId = encounterId,
            PatientId = _patientId,
            RequestedAmount = 3_000_000m
        }, _actorUserId);

        // Batalkan order
        await _depositOrderService.CancelOrderAsync(order.Id, new CancelDepositPaymentOrderRequest
        {
            Reason = "Pasien beralih penjamin asuransi"
        }, _actorUserId);

        // Act & Assert: Mencoba membayar order yang sudah CANCELLED
        var ex = await Assert.ThrowsAsync<BillingDepositValidationException>(async () =>
        {
            await _depositOrderService.PayOrderAsync(order.Id, new PayDepositOrderRequest
            {
                Amount = 3_000_000m,
                PaymentMethodId = _cashPaymentMethodId
            }, _actorUserId);
        });

        Assert.Contains("sudah dibatalkan", ex.Message);
    }

    [Fact]
    public async Task Scenario07_KwitansiHanyaDapatDiterbitkanUntukPembayaranSah()
    {
        // Arrange
        var episodeId = Guid.NewGuid();
        var encounterId = Guid.NewGuid();
        var order = await _depositOrderService.CreateOrderAsync(new CreateDepositPaymentOrderRequest
        {
            EpisodeId = episodeId,
            EncounterId = encounterId,
            PatientId = _patientId,
            RequestedAmount = 2_500_000m
        }, _actorUserId);

        var pay = await _depositOrderService.PayOrderAsync(order.Id, new PayDepositOrderRequest
        {
            Amount = 2_500_000m,
            PaymentMethodId = _cashPaymentMethodId,
            Reason = "Uang muka kamar VIP"
        }, _actorUserId);

        // Act: Cetak kwitansi berdasarkan ID pembayaran yang sah
        var receipt = await _depositOrderService.GetReceiptByPaymentIdAsync(pay.PaymentId, CancellationToken.None);

        // Assert
        Assert.NotNull(receipt);
        Assert.Equal(order.OrderNumber, receipt.OrderNumber);
        Assert.Equal(2_500_000m, receipt.AmountPaid);
        Assert.Equal("RM-2026-0001", receipt.MedicalRecordNumber);
        Assert.Equal("Pasien Uji Ranap", receipt.PatientName);
        Assert.Contains("Dua Juta Lima Ratus Ribu Rupiah", receipt.AmountInWords);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _database.Dispose();
    }
}
