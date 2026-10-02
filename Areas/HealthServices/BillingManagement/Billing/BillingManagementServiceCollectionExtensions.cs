using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PettyCash.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing;

public static class BillingManagementServiceCollectionExtensions
{
    public static IServiceCollection AddBillingManagement(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<BillingModuleService>();
        services.AddScoped<BillingInvoiceService>();
        services.AddScoped<BillingCalculationService>();
        services.AddScoped<BillingPayerEditService>();
        services.AddScoped<BillingInsuranceInvoiceDocumentService>();
        services.AddScoped<BillingCompanyGuarantorInvoiceDocumentService>();
        services.AddScoped<BillingDiscountService>();
        services.AddScoped<BillingDepositService>();
        services.AddScoped<BillingAllocationService>();
        services.AddScoped<BillingSettlementService>();
        services.AddScoped<BillingRefundService>();
        services.AddScoped<BillingInvoiceClosureService>();
        services.AddScoped<BilConsumerHandoffService>();
        services.AddScoped<BilPrescriptionClearanceRecoveryService>();
        services.AddScoped<BillingFinancialExceptionService>();
        services.AddScoped<BillingArApHandoffService>();
        services.AddScoped<BillingFinalizationService>();
        services.AddScoped<BillingNumberSeriesService>();
        services.AddScoped<BillingReminderService>();
        services.AddScoped<CashierShiftService>();
        services.AddScoped<IBillingChargeSourceAdapter, ContractBillingChargeSourceAdapter>();
        services.AddScoped<IBillingCoverageAdapter, RegistrationBillingCoverageAdapter>();
        services.AddScoped<IInpatientRoomChargeCalculationService, InpatientRoomChargeCalculationService>();
        services.AddScoped<InpatientRoomChargeCalculationService>();
        services.AddScoped<IAdministrationFeeCalculationService, AdministrationFeeCalculationService>();
        services.AddScoped<AdministrationFeeCalculationService>();
        services.AddScoped<IInpatientClearanceService, InpatientClearanceService>();
        services.AddScoped<InpatientClearanceService>();
        services.AddOptions<BillingPaymentProviderOptions>()
            .BindConfiguration(BillingPaymentProviderOptions.SectionName);

        // Selama belum ada integrasi mesin pembayaran/tender bank, tender non-tunai diterima
        // langsung supaya kasir tidak menunggu konfirmasi yang tidak akan datang. Setel
        // Billing:PaymentProvider:AutoAcceptWithoutProvider = false untuk kembali ke perilaku
        // aman (tender non-tunai berstatus Pending) begitu provider tersambung.
        services.AddScoped<IBillingPaymentProviderAdapter>(provider =>
            provider.GetRequiredService<IOptions<BillingPaymentProviderOptions>>()
                .Value.AutoAcceptWithoutProvider
                ? new AutoAcceptBillingPaymentProviderAdapter()
                : new DeferredBillingPaymentProviderAdapter());
        services.AddOptions<BillingInvoiceNumberOptions>()
            .BindConfiguration(BillingInvoiceNumberOptions.SectionName)
            .ValidateOnStart();
        services.AddOptions<BillingDepositAccountNumberOptions>()
            .BindConfiguration(BillingDepositAccountNumberOptions.SectionName)
            .ValidateOnStart();
        services.AddOptions<BillingCashierShiftNumberOptions>()
            .BindConfiguration(BillingCashierShiftNumberOptions.SectionName)
            .ValidateOnStart();
        // BE-BKC-034 / PC-DES-008: nomor voucher Petty Cash (PTC-YYYYMMDD-NNNN).
        services.AddOptions<PettyCashVoucherNumberOptions>()
            .BindConfiguration(PettyCashVoucherNumberOptions.SectionName)
            .ValidateOnStart();
        services.AddScoped<AdministrationFeePolicyService>();
        services.AddScoped<DiscountPolicyService>();
        services.AddScoped<TaxRuleService>();
        services.AddScoped<RoomChargePolicyService>();
        services.AddScoped<RegisterService>();
        // BE-BKC-035 / PC-DES-002: master data kategori pengeluaran kas kecil.
        services.AddScoped<PettyCashCategoryService>();
        // BE-FIN-004: rekening bank dan mata uang/kurs milik Finance.
        services.AddScoped<BankAccountService>();
        services.AddScoped<CurrencyService>();
        // BE-FIN-011, FIN-DES-017: satu-satunya penulis FinAccountingEventOutbox. Dipakai
        // FinanceReceivableService dan FinanceBillingIntakeService lewat DI (scoped, DbContext sama).
        services.AddScoped<FinanceAccountingOutboxService>();
        // BE-FIN-012: pantauan kotak keluar kejadian, baca saja — tidak ada penulisan data.
        services.AddScoped<FinanceAccountingEventService>();
        // BE-FIN-008: satu-satunya penulis OutstandingAmount piutang — aging, koreksi, write-off.
        services.AddScoped<FinanceReceivableService>();
        // BE-FIN-017, 02-backend-architecture.md §4.22: pemilik logika penerimaan (pembuatan dari
        // tender + pembuktian FR-FIN-035). Dipakai FinanceBillingIntakeService lewat DI.
        services.AddScoped<FinanceReceiptService>();
        // BE-FIN-009: konsumen fakta AR dari Billing (gap FinanceBillingIntakeService ditutup di sini).
        services.AddScoped<FinanceBillingIntakeService>();
        // BE-FIN-014 / MVP-4: layanan perhitungan kas tersedia, setoran bank, dan penutupan harian.
        services.AddScoped<FinanceCashManagementService>();
        // BE-FIN-019, 02-backend-architecture.md §4.22: input manual utang supplier dan koreksinya.
        services.AddScoped<FinanceSupplierPayableService>();
        // BE-FIN-020, 02-backend-architecture.md §4.22: layanan pembayaran keluar dan potongan.
        services.AddScoped<FinancePaymentService>();
        // BE-BKC-036 / PC-DES-004: kolam anggaran dan saldo berjalan kas kecil.
        services.AddScoped<PettyCashBudgetService>();
        // BE-BKC-037 / PC-DES-001: siklus hidup voucher kas kecil penuh.
        services.AddScoped<PettyCashVoucherService>();

        return services;
    }
}
