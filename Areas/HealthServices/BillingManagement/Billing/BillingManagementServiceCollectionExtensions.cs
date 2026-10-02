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
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;
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
        services.AddScoped<BillingSourceTariffResolver>();
        services.AddScoped<BillingClinicalChargeBridgeService>();
        services.AddScoped<EncounterBillingSummaryService>();
        services.AddScoped<BillingChargeReconciliationService>();
        services.AddScoped<BillingSyncPolicyService>();
        services.AddScoped<IBillingCoverageAdapter, RegistrationBillingCoverageAdapter>();
        // BE-RWI-147 / RWI-DEC-192 butir (e): InpatientRoomChargeCalculationService tidak lagi
        // didaftarkan. Tarif kamar hanya dihitung BillingCalculationService (satu jalur).
        services.AddScoped<IAdministrationFeeCalculationService, AdministrationFeeCalculationService>();
        services.AddScoped<AdministrationFeeCalculationService>();
        services.AddScoped<IInpatientClearanceService, InpatientClearanceService>();
        services.AddScoped<InpatientClearanceService>();
        // BE-RWI-150 / INT-RWF-01: penerima ketukan pintu Rawat Inap di dalam aplikasi.
        services.AddScoped<BillingInpatientEventReceiver>();
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
        // BE-FIN-049: kalkulasi snapshot saldo subledger bulanan untuk 4 control account dan penerbitan ke outbox.
        services.AddScoped<FinanceSubledgerSnapshotService>();
        // BE-FIN-008: satu-satunya penulis OutstandingAmount piutang — aging, koreksi, write-off.
        services.AddScoped<FinanceReceivableService>();
        // BE-FIN-039, FIN-DEC-048/054: Batch Tagihan AR — hanya membaca FinReceivable, memanggil
        // BillingCompanyGuarantorInvoiceDocumentService untuk dokumen gabungan, tidak menyalinnya.
        services.AddScoped<FinanceReceivableInvoiceBatchService>();
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
        // BE-FIN-032, FIN-VAL-102: pengecekan jenjang persetujuan (role Identity) — dipakai
        // bersama Purchase Order/Purchasing Invoice/Pembayaran, bukan hanya Purchasing.
        services.AddScoped<FinanceApprovalAuthorizationService>();
        // BE-FIN-032, 02-backend-architecture.md §C.1: Purchase Order dan Tanda Terima Barang.
        services.AddScoped<FinancePurchaseOrderService>();
        services.AddScoped<FinanceGoodsReceiptService>();
        // BE-FIN-033, FIN-DEC-051: Tukar Faktur (checkpoint dokumen faktur supplier).
        services.AddScoped<FinanceInvoiceExchangeService>();
        // BE-FIN-034, FIN-DEC-045/046/053: Purchasing Invoice — pengakuan utang + PPN Masukan.
        services.AddScoped<FinancePurchasingInvoiceService>();
        // BE-FIN-035, FIN-DEC-047/061: Retur Pembelian dan Deposit Retur — satu-satunya penulis
        // AvailableAmount (FIN-DES-046, ditegakkan penuh oleh BE-FIN-036).
        services.AddScoped<FinanceSupplierReturnService>();
        // BE-FIN-037, FIN-API-1.1 §B.6, FIN-DEC-059: Empat laporan Purchasing/AP read-only
        // (/summary, /invoice-exchanges, /due-dates, /reconciliation). Nol tabel baru.
        // /aging SENGAJA tidak ada — FIN-DEC-059 mencabut endpoint itu; layar AP memakai
        // GET api/finance/payable/aging (FinanceApController) yang sudah berjalan.
        services.AddScoped<FinancePurchasingReportService>();
        // BE-BKC-036 / PC-DES-004: kolam anggaran dan saldo berjalan kas kecil.
        services.AddScoped<PettyCashBudgetService>();
        // BE-BKC-037 / PC-DES-001: siklus hidup voucher kas kecil penuh.
        services.AddScoped<PettyCashVoucherService>();

        return services;
    }
}
