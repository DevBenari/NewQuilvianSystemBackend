using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

namespace QuilvianSystemBackend.Tests.BillingManagement;

public sealed class BillingChargeSourceAdapterTests
{
    private const string ContractVersion = ContractBillingChargeSourceAdapter.ContractVersion;

    [Fact]
    public void AdhocCatalogIsAcceptedAsBillableSourceDomain()
    {
        var adapter = new ContractBillingChargeSourceAdapter();

        var snapshot = adapter.ValidateAndNormalize(new UpsertChargeRequest
        {
            SourceDomain = "ADHOC_CATALOG",
            SourceDetailId = "TARIFF-1",
            SourceVersion = 1,
            SourceStatus = "ADDED",
            ContractVersion = ContractVersion
        });

        Assert.Equal("ADHOC_CATALOG", snapshot.SourceDomain);
        Assert.Equal("ADDED", snapshot.SourceStatus);
    }

    [Fact]
    public void AdhocCatalogOrderIsCompleteImmediatelyOnEntry()
    {
        // BKC-DEC-059: siklus hidupnya identik ADHOC - begitu dicatat, layanan (tarif) sudah
        // terjadi. Tanpa completeOnEntry, "ADDED" yang juga anggota NormalVoidFromStatuses akan
        // membuat IsOrderComplete permanen false dan memblokir finalisasi invoice, persis bug yang
        // pernah terjadi pada policy ADHOC sebelum completeOnEntry ditambahkan.
        var adapter = new ContractBillingChargeSourceAdapter();
        var item = new BilInvoiceItem { SourceDomain = "ADHOC_CATALOG", SourceStatus = "ADDED" };

        Assert.True(adapter.IsOrderComplete(item));
    }

    [Fact]
    public void AdhocCatalogItemCanStillBeVoidedFromAddedStatus()
    {
        // completeOnEntry hanya memengaruhi IsOrderComplete; jalur pembatalan tetap mengikuti
        // NormalVoidFromStatuses seperti ADHOC, supaya kasir masih bisa membatalkan salah ketik
        // sebelum invoice final.
        var adapter = new ContractBillingChargeSourceAdapter();
        var item = new BilInvoiceItem { SourceDomain = "ADHOC_CATALOG", SourceStatus = "ADDED", SourceVersion = 1 };

        var snapshot = adapter.ValidateVoid(item, new VoidInvoiceItemRequest
        {
            SourceVersion = 2,
            SourceStatus = "VOIDED",
            ContractVersion = ContractVersion,
            Reason = "Salah entri tarif"
        });

        Assert.Equal("VOIDED", snapshot.SourceStatus);
    }
}
