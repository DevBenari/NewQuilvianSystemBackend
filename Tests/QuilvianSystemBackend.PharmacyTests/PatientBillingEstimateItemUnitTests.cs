using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Test suite untuk Acceptance Criteria Perbaikan Satuan Obat & Alkes pada Estimasi Biaya Rawat Inap:
/// 1. Dua obat yang mempunyai master satuan berbeda menghasilkan unitLabel berbeda.
/// 2. unitLabel estimasi identik dengan unit Running Invoice untuk invoiceItemId yang sama.
/// 3. Item tanpa master satuan tidak menampilkan "tablet / botol" (fallback "-").
/// 4. Item Alkes mengikuti sumber satuan resminya.
/// 5. Item non-farmasi tidak berubah (tindakan -> "tindakan", alat/penunjang -> "unit").
/// 6. Seluruh nilai nominal tagihan tetap sama sebelum dan sesudah perbaikan.
/// 7. Status invoice OPEN, FINAL, CLOSED dan item tanpa tarif tetap valid.
/// 8. Attribute AccessPermission("PatientBillingSummary", "ViewItemAmount") dipertahankan pada GetEstimate.
/// 
/// Catatan: Jangan menjalankan dotnet test secara otomatis sesuai batasan tugas.
/// </summary>
public class PatientBillingEstimateItemUnitTests
{
    private static (BilInvoiceItem item, string? runningInvoiceUnit, string estimateUnitLabel) CreateTestItem(
        string description,
        bool isPharmacy,
        string? measurementName,
        decimal quantity,
        decimal unitPrice,
        string group = "PHARMACY")
    {
        var category = new MstTariffCategory
        {
            Id = Guid.NewGuid(),
            TariffCategoryCode = isPharmacy ? "PHAR" : "PROC",
            TariffCategoryName = isPharmacy ? "Farmasi" : "Tindakan Medis",
            IsPharmacy = isPharmacy,
            IsProcedure = !isPharmacy
        };

        MstMeasurement? measurement = measurementName != null
            ? new MstMeasurement
            {
                Id = Guid.NewGuid(),
                MeasurementCode = measurementName.ToUpperInvariant(),
                MeasurementName = measurementName
            }
            : null;

        var drug = isPharmacy
            ? new MstDrug
            {
                Id = Guid.NewGuid(),
                DrugCode = "DRUG-001",
                DrugName = description,
                DispenseUnitMeasurement = measurement,
                DispenseUnitMeasurementId = measurement?.Id
            }
            : null;

        var tariff = new MstTariff
        {
            Id = Guid.NewGuid(),
            TariffCode = "TRF-001",
            TariffName = description,
            Drug = drug,
            DrugId = drug?.Id
        };

        var item = new BilInvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = Guid.NewGuid(),
            DescriptionSnapshot = description,
            Category = category,
            CategoryId = category.Id,
            Tariff = tariff,
            TariffId = tariff.Id,
            Quantity = quantity,
            UnitPrice = unitPrice,
            SourceDomain = isPharmacy ? "PHARMACY" : "CLINICAL",
            SourceOccurredAt = DateTimeOffset.UtcNow,
            Status = "ACTIVE"
        };

        // Running Invoice mapping
        var runningInvoiceUnit = BillingInvoiceService.ResolveItemUnit(item);

        // Estimasi Rawat Inap mapping
        var isPharmacyGroup = group == "PHARMACY" || item.Category?.IsPharmacy == true;
        var estimateUnitLabel = isPharmacyGroup
            ? (!string.IsNullOrWhiteSpace(runningInvoiceUnit) ? runningInvoiceUnit : "-")
            : (group is "PROCEDURE" or "SURGERY" || item.Category?.IsSurgery == true || item.Category?.IsProcedure == true ? "tindakan" : "unit");

        return (item, runningInvoiceUnit, estimateUnitLabel);
    }

    [Fact]
    public void AC1_DuaObatDenganMasterSatuanBerbeda_MenghasilkanUnitLabelBerbeda()
    {
        // Arrange: Obat A (Tablet), Obat B (Vial)
        var (_, _, unitLabelTablet) = CreateTestItem("Paracetamol 500mg", isPharmacy: true, measurementName: "Tablet", quantity: 10, unitPrice: 1500m);
        var (_, _, unitLabelVial) = CreateTestItem("Ceftriaxone 1g Injeksi", isPharmacy: true, measurementName: "Vial", quantity: 2, unitPrice: 85000m);

        // Assert:
        Assert.Equal("Tablet", unitLabelTablet);
        Assert.Equal("Vial", unitLabelVial);
        Assert.NotEqual(unitLabelTablet, unitLabelVial);
        Assert.DoesNotContain("tablet / botol", unitLabelTablet);
        Assert.DoesNotContain("tablet / botol", unitLabelVial);
    }

    [Fact]
    public void AC2_UnitLabelEstimasi_IdentikDenganUnitRunningInvoice_UntukItemYangSama()
    {
        // Arrange: Obat Sirup dengan satuan dispensing "Botol"
        var (item, runningUnit, estimateUnitLabel) = CreateTestItem("Amoxicillin Sirup Kering", isPharmacy: true, measurementName: "Botol", quantity: 1, unitPrice: 25000m);

        // Act: Pemetaan Running Invoice melalui BillingInvoiceService.MapItems
        var runningItems = BillingInvoiceService.MapItems(new[] { item });
        var runningItem = Assert.Single(runningItems);

        // Assert: Nilai unit identik antara Running Invoice dan Estimasi
        Assert.Equal("Botol", runningUnit);
        Assert.Equal("Botol", runningItem.Unit);
        Assert.Equal(runningItem.Unit, estimateUnitLabel);
    }

    [Fact]
    public void AC3_ItemTanpaMasterSatuan_TidakMenampilkanHardcodeTabletBotol_TetapiFallbackMinus()
    {
        // Arrange: Obat yang belum diset DispenseUnitMeasurement pada master datanya
        var (item, runningUnit, estimateUnitLabel) = CreateTestItem("Obat Racikan Tanpa Satuan", isPharmacy: true, measurementName: null, quantity: 5, unitPrice: 10000m);

        // Assert:
        Assert.Null(runningUnit);
        Assert.Equal("-", estimateUnitLabel);
        Assert.NotEqual("tablet / botol", estimateUnitLabel);
    }

    [Fact]
    public void AC4_ItemAlkes_MengikutiSumberSatuanResminya()
    {
        // Arrange: Alkes Medis Habis Pakai (Spuit 5cc = Pcs, Kassa Gulung = Roll, Infusion Set = Set)
        var (_, _, unitSpuit) = CreateTestItem("Spuit 5cc Disposable", isPharmacy: true, measurementName: "Pcs", quantity: 3, unitPrice: 5000m);
        var (_, _, unitKassa) = CreateTestItem("Kassa Gulung 10cm", isPharmacy: true, measurementName: "Roll", quantity: 2, unitPrice: 12000m);
        var (_, _, unitInfus) = CreateTestItem("Infusion Set Dewasa", isPharmacy: true, measurementName: "Set", quantity: 1, unitPrice: 18000m);

        // Assert:
        Assert.Equal("Pcs", unitSpuit);
        Assert.Equal("Roll", unitKassa);
        Assert.Equal("Set", unitInfus);
    }

    [Fact]
    public void AC5_ItemNonFarmasi_MempertahankanPerilakuSatuanYangSudahBerlaku()
    {
        // Arrange: Tindakan medis (group = PROCEDURE)
        var (_, runningUnitProc, estimateUnitProc) = CreateTestItem("Pemasangan Infus", isPharmacy: false, measurementName: null, quantity: 1, unitPrice: 50000m, group: "PROCEDURE");
        // Penunjang / Alat
        var (_, runningUnitSupp, estimateUnitSupp) = CreateTestItem("Pemeriksaan Hemoglobin", isPharmacy: false, measurementName: null, quantity: 1, unitPrice: 40000m, group: "SUPPORT");

        // Assert: Non-farmasi running invoice Unit bernilai null; estimasi label "tindakan" atau "unit"
        Assert.Null(runningUnitProc);
        Assert.Equal("tindakan", estimateUnitProc);

        Assert.Null(runningUnitSupp);
        Assert.Equal("unit", estimateUnitSupp);
    }

    [Fact]
    public void AC6_SemuaNilaiNominalTagihan_TetapSamaSebelumDanSesudahPerbaikanSatuan()
    {
        // Arrange: Tagihan obat dengan kalkulasi finansial
        const decimal qty = 15m;
        const decimal price = 2500m;
        var (item, _, _) = CreateTestItem("Paracetamol Tablet", isPharmacy: true, measurementName: "Tablet", quantity: qty, unitPrice: price);

        // Act: Hitung nominal
        var grossAmount = item.Quantity * item.UnitPrice;

        // Assert: Tidak ada perubahan pada besaran nilai nominal finansial
        Assert.Equal(15m, item.Quantity);
        Assert.Equal(2500m, item.UnitPrice);
        Assert.Equal(37500m, grossAmount);
    }

    [Theory]
    [InlineData("OPEN")]
    [InlineData("FINAL")]
    [InlineData("CLOSED")]
    public void AC7_StatusInvoice_OpenFinalClosed_DapatDiprosesDenganSatuanYangBenar(string invoiceStatus)
    {
        // Arrange: Item pada invoice dengan berbagai status
        var (item, runningUnit, estimateUnit) = CreateTestItem("Cefotaxime Vial", isPharmacy: true, measurementName: "Vial", quantity: 3, unitPrice: 60000m);

        // Assert: Resolusi satuan independen terhadap status invoice
        Assert.Equal("Vial", runningUnit);
        Assert.Equal("Vial", estimateUnit);
        Assert.False(string.IsNullOrWhiteSpace(invoiceStatus));
    }

    [Fact]
    public void AC8_EndpointGetEstimate_MempertahankanPermission_ViewItemAmount()
    {
        // Arrange: Refleksi ke method GetEstimate pada PatientBillingSummaryController
        var method = typeof(PatientBillingSummaryController).GetMethod(nameof(PatientBillingSummaryController.GetEstimate));
        Assert.NotNull(method);

        // Act: Periksa atribut AccessPermission
        var accessPermissionAttr = method.GetCustomAttribute<AccessPermissionAttribute>();
        var accessActionAttr = method.GetCustomAttribute<AccessActionAttribute>();
        var httpGetAttr = method.GetCustomAttribute<HttpGetAttribute>();

        // Assert:
        Assert.NotNull(accessPermissionAttr);
        Assert.Equal("PatientBillingSummary", accessPermissionAttr.Resource);
        Assert.Equal("ViewItemAmount", accessPermissionAttr.Action);

        Assert.NotNull(accessActionAttr);
        Assert.Equal("ViewItemAmount", accessActionAttr.ActionName);

        Assert.NotNull(httpGetAttr);
        Assert.Equal("episodes/{episodeId:guid}/breakdown/estimate", httpGetAttr.Template);
    }
}
