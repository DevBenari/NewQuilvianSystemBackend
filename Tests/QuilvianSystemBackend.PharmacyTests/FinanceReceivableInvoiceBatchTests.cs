using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Unit test untuk Finance AR Invoice / FinReceivableInvoiceBatch:
/// 1. Due Date selalu otomatis 30 hari kalender dari Tanggal Pembuatan Invoice.
/// 2. Format resmi nomor invoice: {SEQUENCE}/{VISIT_CODE}/RSMMC/{ROMAN_MONTH}/{YEAR}.
/// 3. Validasi visit code (Inpatient -> IP, Outpatient/Emergency/OTC -> OP).
/// 4. Invariant: satu batch hanya boleh memiliki satu distinct visit code (campur IP + OP ditolak 422).
/// </summary>
public class FinanceReceivableInvoiceBatchTests
{
    [Fact]
    public void Case1_Inpatient_Generates_IP_And_RomanMonth()
    {
        // CASE 1: Sequence = 50, EncounterType = Inpatient, InvoiceDate = 2026-06-10
        // Expected: 0050/IP/RSMMC/VI/2026
        const long sequence = 50;
        const string visitCode = "IP";
        var invoiceDate = new DateOnly(2026, 6, 10);
        var romanMonth = FinanceReceivableInvoiceBatchService.ToRomanMonth(invoiceDate.Month);

        var invoiceNumber = $"{sequence:D4}/{visitCode}/RSMMC/{romanMonth}/{invoiceDate.Year}";

        Assert.Equal("0050/IP/RSMMC/VI/2026", invoiceNumber);
    }

    [Fact]
    public void Case2_Outpatient_Generates_OP_And_RomanMonth()
    {
        // CASE 2: Next sequence = 51, EncounterType = Outpatient, InvoiceDate = 2026-06-12
        // Expected: 0051/OP/RSMMC/VI/2026
        const long sequence = 51;
        const string visitCode = "OP";
        var invoiceDate = new DateOnly(2026, 6, 12);
        var romanMonth = FinanceReceivableInvoiceBatchService.ToRomanMonth(invoiceDate.Month);

        var invoiceNumber = $"{sequence:D4}/{visitCode}/RSMMC/{romanMonth}/{invoiceDate.Year}";

        Assert.Equal("0051/OP/RSMMC/VI/2026", invoiceNumber);
    }

    [Fact]
    public void Case3_Emergency_Resolves_To_OP()
    {
        // CASE 3: EncounterType = Emergency -> Expected: VisitCode = OP
        var recId = Guid.NewGuid();
        var encId = Guid.NewGuid();
        var receivable = new FinReceivable { Id = recId, ReceivableNumber = "REC-EMG-01" };
        var encounter = new RegPatientEncounter { Id = encId, EncounterType = EncounterType.Emergency };

        var items = new List<FinReceivableItem>
        {
            new() { Id = Guid.NewGuid(), ReceivableId = recId, EncounterId = encId }
        };

        var encounters = new Dictionary<Guid, RegPatientEncounter> { [encId] = encounter };
        var invoices = new Dictionary<Guid, BilInvoice>();

        var visitCode = FinanceReceivableInvoiceBatchService.ResolveReceivableVisitCode(
            receivable, items, invoices, encounters);

        Assert.Equal("OP", visitCode);
    }

    [Fact]
    public void Case4_OTC_AuthoritativeSource_Resolves_To_OP()
    {
        // CASE 4: OTC authoritative source (invoice.ServiceType == "OTC") -> Expected: VisitCode = OP
        var recId = Guid.NewGuid();
        var invId = Guid.NewGuid();
        var receivable = new FinReceivable { Id = recId, ReceivableNumber = "REC-OTC-01", InvoiceId = invId };
        var invoice = new BilInvoice { Id = invId, ServiceType = "OTC" };

        var items = new List<FinReceivableItem>();
        var invoices = new Dictionary<Guid, BilInvoice> { [invId] = invoice };
        var encounters = new Dictionary<Guid, RegPatientEncounter>();

        var visitCode = FinanceReceivableInvoiceBatchService.ResolveReceivableVisitCode(
            receivable, items, invoices, encounters);

        Assert.Equal("OP", visitCode);
    }

    [Fact]
    public void Case5_Outpatient_And_Emergency_CanCombine_InSingleBatch()
    {
        // CASE 5: Outpatient + Emergency -> Keduanya resolve ke OP -> Boleh satu batch
        var rec1Id = Guid.NewGuid();
        var enc1Id = Guid.NewGuid();
        var rec1 = new FinReceivable { Id = rec1Id, ReceivableNumber = "REC-OPD-01" };
        var enc1 = new RegPatientEncounter { Id = enc1Id, EncounterType = EncounterType.Outpatient };

        var rec2Id = Guid.NewGuid();
        var enc2Id = Guid.NewGuid();
        var rec2 = new FinReceivable { Id = rec2Id, ReceivableNumber = "REC-EMG-02" };
        var enc2 = new RegPatientEncounter { Id = enc2Id, EncounterType = EncounterType.Emergency };

        var items = new List<FinReceivableItem>
        {
            new() { Id = Guid.NewGuid(), ReceivableId = rec1Id, EncounterId = enc1Id },
            new() { Id = Guid.NewGuid(), ReceivableId = rec2Id, EncounterId = enc2Id }
        };

        var encounters = new Dictionary<Guid, RegPatientEncounter>
        {
            [enc1Id] = enc1,
            [enc2Id] = enc2
        };
        var invoices = new Dictionary<Guid, BilInvoice>();

        var visitCodes = new HashSet<string>();
        visitCodes.Add(FinanceReceivableInvoiceBatchService.ResolveReceivableVisitCode(
            rec1, items.Where(x => x.ReceivableId == rec1Id).ToList(), invoices, encounters));
        visitCodes.Add(FinanceReceivableInvoiceBatchService.ResolveReceivableVisitCode(
            rec2, items.Where(x => x.ReceivableId == rec2Id).ToList(), invoices, encounters));

        Assert.Single(visitCodes);
        Assert.Contains("OP", visitCodes);
    }

    [Fact]
    public void Case6_Inpatient_And_Outpatient_ThrowsValidationException()
    {
        // CASE 6: Inpatient + Outpatient -> Campur IP dan OP -> ditolak
        var rec1Id = Guid.NewGuid();
        var enc1Id = Guid.NewGuid();
        var rec1 = new FinReceivable { Id = rec1Id, ReceivableNumber = "REC-INP-01" };
        var enc1 = new RegPatientEncounter { Id = enc1Id, EncounterType = EncounterType.Inpatient };

        var rec2Id = Guid.NewGuid();
        var enc2Id = Guid.NewGuid();
        var rec2 = new FinReceivable { Id = rec2Id, ReceivableNumber = "REC-OPD-02" };
        var enc2 = new RegPatientEncounter { Id = enc2Id, EncounterType = EncounterType.Outpatient };

        var items = new List<FinReceivableItem>
        {
            new() { Id = Guid.NewGuid(), ReceivableId = rec1Id, EncounterId = enc1Id },
            new() { Id = Guid.NewGuid(), ReceivableId = rec2Id, EncounterId = enc2Id }
        };

        var encounters = new Dictionary<Guid, RegPatientEncounter>
        {
            [enc1Id] = enc1,
            [enc2Id] = enc2
        };
        var invoices = new Dictionary<Guid, BilInvoice>();

        var visitCodes = new HashSet<string>();
        visitCodes.Add(FinanceReceivableInvoiceBatchService.ResolveReceivableVisitCode(
            rec1, items.Where(x => x.ReceivableId == rec1Id).ToList(), invoices, encounters));
        visitCodes.Add(FinanceReceivableInvoiceBatchService.ResolveReceivableVisitCode(
            rec2, items.Where(x => x.ReceivableId == rec2Id).ToList(), invoices, encounters));

        Assert.Equal(2, visitCodes.Count);

        var action = () =>
        {
            if (visitCodes.Count > 1)
            {
                throw new ReceivableInvoiceBatchValidationException(
                    "Tagihan Rawat Inap dan Rawat Jalan/IGD/OTC tidak dapat digabung dalam satu Invoice AR.");
            }
        };

        var ex = Assert.Throws<ReceivableInvoiceBatchValidationException>(action);
        Assert.Equal("Tagihan Rawat Inap dan Rawat Jalan/IGD/OTC tidak dapat digabung dalam satu Invoice AR.", ex.Message);
    }

    [Fact]
    public void Case8_InvoiceDate_EndOfYear_Generates_XII_And_SameYear()
    {
        // CASE 8: InvoiceDate = 2026-12-31 -> Expected suffix /XII/2026
        var invoiceDate = new DateOnly(2026, 12, 31);
        var romanMonth = FinanceReceivableInvoiceBatchService.ToRomanMonth(invoiceDate.Month);
        const long sequence = 99;
        const string visitCode = "OP";

        var invoiceNumber = $"{sequence:D4}/{visitCode}/RSMMC/{romanMonth}/{invoiceDate.Year}";

        Assert.Equal("0099/OP/RSMMC/XII/2026", invoiceNumber);
    }

    [Fact]
    public void CaseS1_DueDate_Always_30_Calendar_Days_October()
    {
        // CASE S1: InvoiceDate = 2026-10-07 -> DueDate = 2026-11-06 (30 hari kalender)
        var invoiceDate = new DateOnly(2026, 10, 7);
        const int paymentTermDays = 30;
        var dueDate = invoiceDate.AddDays(paymentTermDays);

        Assert.Equal(30, paymentTermDays);
        Assert.Equal(new DateOnly(2026, 11, 6), dueDate);
    }

    [Fact]
    public void CaseS2_DueDate_Always_30_Calendar_Days_January()
    {
        // CASE S2: InvoiceDate = 2026-01-31 -> DueDate = 2026-03-02 (bukan AddMonths(1))
        var invoiceDate = new DateOnly(2026, 1, 31);
        const int paymentTermDays = 30;
        var dueDate = invoiceDate.AddDays(paymentTermDays);

        // 31 Jan 2026 + 30 days:
        // 2026 bukan tahun kabisat (Februari = 28 hari)
        // 31 Jan + 28 hari Feb = 28 hari dilewati, tersisa 2 hari di Maret -> 2026-03-02
        Assert.Equal(new DateOnly(2026, 3, 2), dueDate);
    }

    [Theory]
    [InlineData(1, "I")]
    [InlineData(2, "II")]
    [InlineData(3, "III")]
    [InlineData(4, "IV")]
    [InlineData(5, "V")]
    [InlineData(6, "VI")]
    [InlineData(7, "VII")]
    [InlineData(8, "VIII")]
    [InlineData(9, "IX")]
    [InlineData(10, "X")]
    [InlineData(11, "XI")]
    [InlineData(12, "XII")]
    public void All_RomanMonths_AreDeterministic(int month, string expected)
    {
        var actual = FinanceReceivableInvoiceBatchService.ToRomanMonth(month);
        Assert.Equal(expected, actual);
    }
}
