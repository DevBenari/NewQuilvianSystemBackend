using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

/// <summary>
/// Penyedia data awal untuk uji Finance AP (Purchasing dan Payable). Semua metode menyimpan
/// datanya langsung ke basis data uji supaya uji tinggal memanggil service yang diuji.
/// </summary>
internal static class FinanceApSeed
{
    public static MstSupplier AddSupplier(ApplicationDbContext db, string code, int paymentTermDays = 0)
    {
        var supplier = new MstSupplier
        {
            Id = Guid.NewGuid(),
            SupplierCode = code,
            SupplierName = $"Supplier {code}",
            PaymentTermDays = paymentTermDays,
            IsActive = true
        };
        db.MstSuppliers.Add(supplier);
        db.SaveChanges();
        return supplier;
    }

    public static MstBankAccount AddBankAccount(ApplicationDbContext db)
    {
        var bankAccount = new MstBankAccount
        {
            Id = Guid.NewGuid(),
            BankId = Guid.NewGuid(),
            AccountNumber = "1234567890",
            AccountName = "Rekening Pembayaran RS",
            AccountType = "PAYMENT",
            CurrencyCode = "IDR",
            IsActive = true
        };
        db.MstBankAccounts.Add(bankAccount);
        db.SaveChanges();
        return bankAccount;
    }

    public static FinSupplierPayable AddPayable(ApplicationDbContext db, MstSupplier supplier, string invoiceNumber, decimal amount)
    {
        var payable = new FinSupplierPayable
        {
            Id = Guid.NewGuid(),
            PayableNumber = $"AP-TEST-{Guid.NewGuid():N}",
            SupplierId = supplier.Id,
            SupplierInvoiceNumber = invoiceNumber,
            SupplierInvoiceDate = new DateOnly(2026, 10, 1),
            OriginalAmount = amount,
            OutstandingAmount = amount,
            DueDate = new DateOnly(2026, 10, 31),
            PaymentTermDays = 30,
            Status = FinSupplierPayableStatuses.Outstanding
        };
        db.FinSupplierPayables.Add(payable);
        db.SaveChanges();
        return payable;
    }

    public static FinPurchaseOrder AddPurchaseOrder(ApplicationDbContext db, MstSupplier supplier)
    {
        var purchaseOrder = new FinPurchaseOrder
        {
            Id = Guid.NewGuid(),
            PONumber = $"PO-TEST-{Guid.NewGuid():N}",
            SupplierId = supplier.Id,
            Status = FinPurchaseOrderStatuses.Approved,
            TotalAmount = 1_000_000m,
            ApprovalTier = ApprovalTiers.Tier1,
            RequestedByUserId = Guid.NewGuid(),
            RequestedAt = DateTimeOffset.UtcNow
        };
        db.FinPurchaseOrders.Add(purchaseOrder);
        db.SaveChanges();
        return purchaseOrder;
    }

    public static FinGoodsReceipt AddGoodsReceipt(ApplicationDbContext db, FinPurchaseOrder purchaseOrder)
    {
        var goodsReceipt = new FinGoodsReceipt
        {
            Id = Guid.NewGuid(),
            GRNumber = $"GR-TEST-{Guid.NewGuid():N}",
            PurchaseOrderId = purchaseOrder.Id,
            ReceivedDate = new DateOnly(2026, 10, 5),
            Status = FinGoodsReceiptStatuses.Received
        };
        db.FinGoodsReceipts.Add(goodsReceipt);
        db.SaveChanges();
        return goodsReceipt;
    }

    /// <summary>
    /// Membuat pengguna uji. <paramref name="roleName"/> kosong berarti pengguna tanpa role Finance.
    /// </summary>
    public static Guid AddUserWithRole(ApplicationDbContext db, string? roleName)
    {
        var userId = Guid.NewGuid();
        var userName = $"user-{userId:N}";
        db.Users.Add(new ApplicationUser
        {
            Id = userId,
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            UserCode = userId.ToString("N")[..12],
            DisplayName = userName
        });

        if (roleName is not null)
        {
            var role = db.Roles.FirstOrDefault(x => x.Name == roleName);
            if (role is null)
            {
                role = new ApplicationRole
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                };
                db.Roles.Add(role);
            }

            db.UserRoles.Add(new IdentityUserRole<Guid> { UserId = userId, RoleId = role.Id });
        }

        db.SaveChanges();
        return userId;
    }

    /// <summary>
    /// Membentuk <see cref="FinanceApprovalAuthorizationService"/> yang membaca role dari basis data uji,
    /// persis seperti di aplikasi (UserManager di atas UserStore EF Core).
    /// </summary>
    public static FinanceApprovalAuthorizationService BuildApprovalAuthorization(ApplicationDbContext db)
    {
        var store = new UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>(db);
        var userManager = new UserManager<ApplicationUser>(
            store,
            null!,
            new PasswordHasher<ApplicationUser>(),
            null!,
            null!,
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            NullLogger<UserManager<ApplicationUser>>.Instance);

        return new FinanceApprovalAuthorizationService(userManager);
    }
}
