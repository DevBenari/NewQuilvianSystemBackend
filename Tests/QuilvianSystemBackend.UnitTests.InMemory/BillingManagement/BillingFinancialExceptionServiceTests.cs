using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.BillingManagement;

public sealed class BillingFinancialExceptionServiceTests
{
    [Fact]
    public async Task PartialWriteOffReducesOutstandingWithoutClosingInvoice()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceWithCalculationAsync(db, 500_000m);
        var makerId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var service = CreateService(db);

        var created = await service.CreateWriteOffAsync(
            WriteOffRequest(seeded.Invoice.Id, seeded.Invoice.RowVersion, 200_000m, "Sisa piutang kecil"),
            Guid.NewGuid(), makerId, CancellationToken.None);
        var approved = await service.ApproveWriteOffAsync(
            created.Id, WriteOffApproval(created.RowVersion), approverId, CancellationToken.None);

        Assert.Equal(BillingWriteOffCaseStatuses.Posted, approved.Status);
        Assert.False(approved.IsFullSettlement);
        Assert.Equal(500_000m, approved.OutstandingBefore);
        Assert.Equal(300_000m, approved.OutstandingAfter);
        Assert.Equal(BillingInvoiceStatuses.Open, (await db.BilInvoices.FindAsync(seeded.Invoice.Id))!.Status);
    }

    [Fact]
    public async Task FullWriteOffTransitionsInvoiceToSettledByWriteOff()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceWithCalculationAsync(db, 300_000m);
        var service = CreateService(db);

        var created = await service.CreateWriteOffAsync(
            WriteOffRequest(seeded.Invoice.Id, seeded.Invoice.RowVersion, 300_000m, "Piutang tidak tertagih"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var approved = await service.ApproveWriteOffAsync(
            created.Id, WriteOffApproval(created.RowVersion), Guid.NewGuid(), CancellationToken.None);

        Assert.True(approved.IsFullSettlement);
        Assert.Equal(0m, approved.OutstandingAfter);
        Assert.Equal(BillingInvoiceStatuses.SettledByWriteOff,
            (await db.BilInvoices.FindAsync(seeded.Invoice.Id))!.Status);
    }

    [Fact]
    public async Task RequesterCannotApproveOwnWriteOff()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceWithCalculationAsync(db, 200_000m);
        var makerId = Guid.NewGuid();
        var service = CreateService(db);

        var created = await service.CreateWriteOffAsync(
            WriteOffRequest(seeded.Invoice.Id, seeded.Invoice.RowVersion, 200_000m, "Coba self approve"),
            Guid.NewGuid(), makerId, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BillingFinancialExceptionForbiddenException>(() =>
            service.ApproveWriteOffAsync(created.Id, WriteOffApproval(created.RowVersion), makerId, CancellationToken.None));

        Assert.Contains("sendiri", exception.Message);
        Assert.Equal(BillingWriteOffCaseStatuses.Submitted,
            (await db.BilWriteOffCases.FindAsync(created.Id))!.Status);
    }

    [Fact]
    public async Task WriteOffAmountExceedingOutstandingIsRejected()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceWithCalculationAsync(db, 100_000m);
        var service = CreateService(db);

        var exception = await Assert.ThrowsAsync<BillingFinancialExceptionValidationException>(() =>
            service.CreateWriteOffAsync(
                WriteOffRequest(seeded.Invoice.Id, seeded.Invoice.RowVersion, 100_000.01m, "Melebihi outstanding"),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("melebihi saldo outstanding", exception.Message);
        Assert.Empty(db.BilWriteOffCases);
    }

    // BE-BKC-029 / BIL-AT-058: plafon residual non-billable adalah selisihnya sendiri, BUKAN
    // sisa tagihan pasien - keduanya sengaja berbeda jauh untuk membuktikan tidak tertukar.
    [Fact]
    public async Task WriteOffDefaultsToPatientArAndOutstandingIsIndependentOfResidualPlafon()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceWithCalculationAsync(
            db, patientAmount: 85_000m, nonBillableResidualAmount: 30_000m);
        var service = CreateService(db);

        var patientAr = await service.CreateWriteOffAsync(
            WriteOffRequest(seeded.Invoice.Id, seeded.Invoice.RowVersion, 1_000m, "Kategori bawaan"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        Assert.Equal(BillingWriteOffCategories.PatientAr, patientAr.Category);

        var exception = await Assert.ThrowsAsync<BillingFinancialExceptionValidationException>(async () =>
            await service.CreateWriteOffAsync(
                WriteOffRequest(
                    seeded.Invoice.Id, (await db.BilInvoices.FindAsync(seeded.Invoice.Id))!.RowVersion,
                    45_000m, "Melebihi sisa selisih", category: BillingWriteOffCategories.NonBillableResidual),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("selisih yang tidak dapat ditagihkan", exception.Message);
        Assert.Single(db.BilWriteOffCases); // hanya case PATIENT_AR di atas, pengajuan residual ditolak
    }

    // BE-BKC-029 / BIL-AT-059: penanggungan residual disetujui orang kedua TANPA menyentuh
    // outstanding pasien maupun status invoice - dibuktikan lewat probe PATIENT_AR bernominal
    // PERSIS outstanding awal yang harus tetap berhasil sesudahnya.
    [Fact]
    public async Task ApprovedResidualWriteOffDoesNotShiftPatientOutstandingOrInvoiceStatus()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceWithCalculationAsync(
            db, patientAmount: 85_000m, nonBillableResidualAmount: 30_000m);
        var makerId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var service = CreateService(db);

        var created = await service.CreateWriteOffAsync(
            WriteOffRequest(seeded.Invoice.Id, seeded.Invoice.RowVersion, 30_000m, "Selisih tidak dapat ditagihkan",
                category: BillingWriteOffCategories.NonBillableResidual),
            Guid.NewGuid(), makerId, CancellationToken.None);
        var approved = await service.ApproveWriteOffAsync(
            created.Id, WriteOffApproval(created.RowVersion), approverId, CancellationToken.None);

        Assert.Equal(BillingWriteOffCaseStatuses.Posted, approved.Status);
        Assert.Equal(BillingWriteOffCategories.NonBillableResidual, approved.Category);
        Assert.False(approved.IsFullSettlement);
        Assert.Equal(BillingInvoiceStatuses.Open, (await db.BilInvoices.FindAsync(seeded.Invoice.Id))!.Status);
        Assert.Equal(0m, await service.GetNonBillableResidualRemainingAsync(seeded.Invoice.Id, CancellationToken.None));

        // Probe: outstanding pasien harus TEPAT 85.000 (tidak berkurang oleh write-off residual
        // di atas) - pengajuan PATIENT_AR bernominal persis itu wajib berhasil.
        var probe = await service.CreateWriteOffAsync(
            WriteOffRequest(
                seeded.Invoice.Id, (await db.BilInvoices.FindAsync(seeded.Invoice.Id))!.RowVersion,
                85_000m, "Probe outstanding tidak bergeser"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        Assert.Equal(85_000m, probe.OutstandingBefore);
    }

    // BE-BKC-029 / BIL-AT-060 (butir 1/2): kategori berisi teks asing DITOLAK 422 - bukan diam-diam
    // diperlakukan sebagai PATIENT_AR (BIL-VAL-042); residual yang ditandai pelunasan penuh
    // DITOLAK 422 - selisih tidak dapat ditagihkan bukan piutang pasien (BIL-VAL-041).
    [Fact]
    public async Task WriteOffUnknownCategoryIsRejected()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceWithCalculationAsync(db, 100_000m);
        var service = CreateService(db);

        var exception = await Assert.ThrowsAsync<BillingFinancialExceptionValidationException>(() =>
            service.CreateWriteOffAsync(
                WriteOffRequest(seeded.Invoice.Id, seeded.Invoice.RowVersion, 10_000m, "Kategori tidak dikenal",
                    category: "BAD_LUCK"),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("tidak dikenali", exception.Message);
        Assert.Empty(db.BilWriteOffCases);
    }

    [Fact]
    public async Task WriteOffResidualCannotBeMarkedFullSettlement()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceWithCalculationAsync(
            db, patientAmount: 85_000m, nonBillableResidualAmount: 30_000m);
        var service = CreateService(db);

        var exception = await Assert.ThrowsAsync<BillingFinancialExceptionValidationException>(() =>
            service.CreateWriteOffAsync(
                WriteOffRequest(seeded.Invoice.Id, seeded.Invoice.RowVersion, 30_000m, "Ditandai pelunasan penuh",
                    category: BillingWriteOffCategories.NonBillableResidual, isFullSettlement: true),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("hapus tanda pelunasan penuh", exception.Message);
        Assert.Empty(db.BilWriteOffCases);
    }

    // BE-BKC-029 acceptance 5: Category MUST ikut payload hash - permintaan idempotency-key sama
    // dengan kategori berbeda WAJIB ditolak sebagai konflik, bukan diam-diam dianggap replay.
    [Fact]
    public async Task SameIdempotencyKeyWithDifferentCategoryIsRejectedAsConflict()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceWithCalculationAsync(
            db, patientAmount: 85_000m, nonBillableResidualAmount: 30_000m);
        var service = CreateService(db);
        var idempotencyKey = Guid.NewGuid();
        var request = WriteOffRequest(seeded.Invoice.Id, seeded.Invoice.RowVersion, 10_000m, "Sama semua kecuali kategori");

        await service.CreateWriteOffAsync(request, idempotencyKey, Guid.NewGuid(), CancellationToken.None);
        var residualRequest = new CreateWriteOffRequest
        {
            InvoiceId = request.InvoiceId,
            Amount = request.Amount,
            ExpectedInvoiceRowVersion = request.ExpectedInvoiceRowVersion,
            Reason = request.Reason,
            CorrelationId = request.CorrelationId,
            CausationId = request.CausationId,
            Category = BillingWriteOffCategories.NonBillableResidual
        };

        var exception = await Assert.ThrowsAsync<BillingFinancialExceptionConflictException>(() =>
            service.CreateWriteOffAsync(residualRequest, idempotencyKey, Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("isi berbeda", exception.Message);
    }

    // BE-BKC-029 / BIL-AT-061: pembatalan write-off residual membuka kembali plafon selisihnya,
    // tanpa menaikkan outstanding pasien atas uang yang tidak pernah ada di sana, dan tanpa
    // memaksa invoice yang memang belum pernah SETTLED_BY_WRITE_OFF kembali OPEN secara keliru.
    [Fact]
    public async Task ReversingPostedResidualWriteOffReopensResidualPlafonOnly()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceWithCalculationAsync(
            db, patientAmount: 85_000m, nonBillableResidualAmount: 30_000m);
        var service = CreateService(db);
        var created = await service.CreateWriteOffAsync(
            WriteOffRequest(seeded.Invoice.Id, seeded.Invoice.RowVersion, 30_000m, "Selisih tidak dapat ditagihkan",
                category: BillingWriteOffCategories.NonBillableResidual),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var approved = await service.ApproveWriteOffAsync(
            created.Id, WriteOffApproval(created.RowVersion), Guid.NewGuid(), CancellationToken.None);
        Assert.Equal(0m, await service.GetNonBillableResidualRemainingAsync(seeded.Invoice.Id, CancellationToken.None));

        var reversal = await service.ReverseAsync(
            "write-offs", created.Id, ReverseRequest(approved.RowVersion), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(BillingAdjustmentDirections.Debit, reversal.Direction);
        Assert.Equal(30_000m, reversal.Amount);
        Assert.Equal(created.Id, reversal.ReversesWriteOffCaseId);
        // Invoice tidak pernah SETTLED_BY_WRITE_OFF oleh write-off residual (BIL-VAL-018
        // dipertegas), jadi reversal-nya juga MUST NOT memaksanya - tetap OPEN sebagaimana adanya.
        Assert.Equal(BillingInvoiceStatuses.Open, (await db.BilInvoices.FindAsync(seeded.Invoice.Id))!.Status);
        Assert.Equal(30_000m, await service.GetNonBillableResidualRemainingAsync(seeded.Invoice.Id, CancellationToken.None));

        // Probe: outstanding pasien tetap 85.000 sepanjang siklus create->approve->reverse residual.
        var probe = await service.CreateWriteOffAsync(
            WriteOffRequest(
                seeded.Invoice.Id, (await db.BilInvoices.FindAsync(seeded.Invoice.Id))!.RowVersion,
                85_000m, "Probe outstanding pasca reversal residual"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        Assert.Equal(85_000m, probe.OutstandingBefore);

        // Plafon residual yang terbuka kembali dapat diajukan ulang penuh (30.000).
        var resubmitted = await service.CreateWriteOffAsync(
            WriteOffRequest(
                seeded.Invoice.Id, (await db.BilInvoices.FindAsync(seeded.Invoice.Id))!.RowVersion,
                30_000m, "Diajukan ulang setelah reversal", category: BillingWriteOffCategories.NonBillableResidual),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        Assert.Equal(30_000m, resubmitted.OutstandingBefore);
    }

    // BE-BKC-029/BKC-DES-018 dipertegas: regresi write-off piutang pasien murni tidak boleh
    // bergeser - ulang PartialWriteOffReducesOutstandingWithoutClosingInvoice/
    // FullWriteOffTransitionsInvoiceToSettledByWriteOff di atas, ditambah pemeriksaan Category
    // eksplisit bernilai PATIENT_AR pada keduanya.
    [Fact]
    public async Task PatientArFullWriteOffStillClosesInvoiceExactlyLikeBeforeAmendment()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceWithCalculationAsync(db, 300_000m);
        var service = CreateService(db);

        var created = await service.CreateWriteOffAsync(
            WriteOffRequest(seeded.Invoice.Id, seeded.Invoice.RowVersion, 300_000m, "Piutang tidak tertagih",
                category: BillingWriteOffCategories.PatientAr),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var approved = await service.ApproveWriteOffAsync(
            created.Id, WriteOffApproval(created.RowVersion), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(BillingWriteOffCategories.PatientAr, approved.Category);
        Assert.True(approved.IsFullSettlement);
        Assert.Equal(BillingInvoiceStatuses.SettledByWriteOff,
            (await db.BilInvoices.FindAsync(seeded.Invoice.Id))!.Status);
    }

    [Fact]
    public async Task ReversingFullWriteOffReopensInvoiceAndCreatesDebitAdjustmentIdempotently()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceWithCalculationAsync(db, 300_000m);
        var service = CreateService(db);
        var created = await service.CreateWriteOffAsync(
            WriteOffRequest(seeded.Invoice.Id, seeded.Invoice.RowVersion, 300_000m, "Piutang tidak tertagih"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var approved = await service.ApproveWriteOffAsync(
            created.Id, WriteOffApproval(created.RowVersion), Guid.NewGuid(), CancellationToken.None);
        Assert.Equal(BillingInvoiceStatuses.SettledByWriteOff,
            (await db.BilInvoices.FindAsync(seeded.Invoice.Id))!.Status);

        var reversal = await service.ReverseAsync(
            "write-offs", created.Id, ReverseRequest(approved.RowVersion), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(BillingAdjustmentDirections.Debit, reversal.Direction);
        Assert.Equal(300_000m, reversal.Amount);
        Assert.Equal(created.Id, reversal.ReversesWriteOffCaseId);
        Assert.Equal(BillingAdjustmentStatuses.Posted, reversal.Status);
        Assert.Equal(BillingInvoiceStatuses.Open, (await db.BilInvoices.FindAsync(seeded.Invoice.Id))!.Status);

        var replay = await service.ReverseAsync(
            "write-offs", created.Id, ReverseRequest(approved.RowVersion), Guid.NewGuid(), CancellationToken.None);
        Assert.True(replay.IsReplay);
        Assert.Equal(reversal.Id, replay.Id);
        Assert.Single(db.BilAdjustments.Where(x => x.ReversesWriteOffCaseId == created.Id));
    }

    [Fact]
    public async Task PostedCreditAndDebitAdjustmentsNetIntoOutstandingCorrectly()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceWithCalculationAsync(db, 500_000m);
        var service = CreateService(db);

        var credit = await service.CreateAdjustmentAsync(
            AdjustmentRequest(seeded.Invoice.Id, seeded.Invoice.RowVersion,
                BillingAdjustmentDirections.Credit, 50_000m, "Koreksi kelebihan tagih"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var creditApproved = await service.ApproveAdjustmentAsync(
            credit.Id, AdjustmentApproval(credit.RowVersion), Guid.NewGuid(), CancellationToken.None);

        var invoiceAfterCredit = await db.BilInvoices.FindAsync(seeded.Invoice.Id);
        var debit = await service.CreateAdjustmentAsync(
            AdjustmentRequest(seeded.Invoice.Id, invoiceAfterCredit!.RowVersion,
                BillingAdjustmentDirections.Debit, 80_000m, "Koreksi kurang tagih"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        await service.ApproveAdjustmentAsync(
            debit.Id, AdjustmentApproval(debit.RowVersion), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(BillingAdjustmentStatuses.Posted, creditApproved.Status);
        // Net effect: 500,000 - 50,000 (credit) + 80,000 (debit) = 530,000 outstanding.
        var writeOffProbe = await service.CreateWriteOffAsync(
            WriteOffRequest(seeded.Invoice.Id, (await db.BilInvoices.FindAsync(seeded.Invoice.Id))!.RowVersion,
                530_000m, "Verifikasi saldo net adjustment"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var writeOffApproved = await service.ApproveWriteOffAsync(
            writeOffProbe.Id, WriteOffApproval(writeOffProbe.RowVersion), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(530_000m, writeOffApproved.OutstandingBefore);
        Assert.True(writeOffApproved.IsFullSettlement);
    }

    [Fact]
    public async Task ApproveAdjustmentWithStaleRowVersionIsRejectedWithoutMutation()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceWithCalculationAsync(db, 250_000m);
        var service = CreateService(db);
        var created = await service.CreateAdjustmentAsync(
            AdjustmentRequest(seeded.Invoice.Id, seeded.Invoice.RowVersion,
                BillingAdjustmentDirections.Credit, 30_000m, "Koreksi kelebihan tagih"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BillingFinancialExceptionConflictException>(() =>
            service.ApproveAdjustmentAsync(
                created.Id, AdjustmentApproval(Guid.NewGuid()), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("berubah", exception.Message);
        Assert.Equal(BillingAdjustmentStatuses.Submitted,
            (await db.BilAdjustments.FindAsync(created.Id))!.Status);
    }

    [Fact]
    public async Task RequesterCannotApproveOwnAdjustment()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceWithCalculationAsync(db, 200_000m);
        var makerId = Guid.NewGuid();
        var service = CreateService(db);

        var created = await service.CreateAdjustmentAsync(
            AdjustmentRequest(seeded.Invoice.Id, seeded.Invoice.RowVersion,
                BillingAdjustmentDirections.Credit, 20_000m, "Koreksi self approve"),
            Guid.NewGuid(), makerId, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BillingFinancialExceptionForbiddenException>(() =>
            service.ApproveAdjustmentAsync(created.Id, AdjustmentApproval(created.RowVersion), makerId, CancellationToken.None));

        Assert.Contains("sendiri", exception.Message);
        Assert.Equal(BillingAdjustmentStatuses.Submitted,
            (await db.BilAdjustments.FindAsync(created.Id))!.Status);
    }

    [Fact]
    public async Task ReversingAdjustmentCreatesOppositeDirectionEntryAndIsIdempotentOnRetry()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var seeded = await SeedInvoiceWithCalculationAsync(db, 400_000m);
        var service = CreateService(db);
        var created = await service.CreateAdjustmentAsync(
            AdjustmentRequest(seeded.Invoice.Id, seeded.Invoice.RowVersion,
                BillingAdjustmentDirections.Credit, 40_000m, "Koreksi kelebihan tagih"),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var approved = await service.ApproveAdjustmentAsync(
            created.Id, AdjustmentApproval(created.RowVersion), Guid.NewGuid(), CancellationToken.None);

        var reversal = await service.ReverseAsync(
            "adjustments", created.Id, ReverseRequest(approved.RowVersion), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(BillingAdjustmentDirections.Debit, reversal.Direction);
        Assert.Equal(40_000m, reversal.Amount);
        Assert.Equal(created.Id, reversal.ReversesAdjustmentId);

        var replay = await service.ReverseAsync(
            "adjustments", created.Id, ReverseRequest(approved.RowVersion), Guid.NewGuid(), CancellationToken.None);
        Assert.True(replay.IsReplay);
        Assert.Single(db.BilAdjustments.Where(x => x.ReversesAdjustmentId == created.Id));
    }

    [Fact]
    public async Task UnsupportedReverseTypeIsRejected()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);

        var exception = await Assert.ThrowsAsync<BillingFinancialExceptionValidationException>(() =>
            service.ReverseAsync(
                "refunds", Guid.NewGuid(), ReverseRequest(Guid.NewGuid()), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("tidak didukung", exception.Message);
    }

    [Fact]
    public void FinancialExceptionEndpointsUseLockedPermissions()
    {
        AssertPermission(nameof(BillingFinancialExceptionsController.CreateAdjustment), "BillingAdjustment", "Create");
        AssertPermission(nameof(BillingFinancialExceptionsController.ApproveAdjustment), "BillingAdjustment", "Approve");
        AssertPermission(nameof(BillingFinancialExceptionsController.CreateWriteOff), "BillingWriteOff", "Create");
        AssertPermission(nameof(BillingFinancialExceptionsController.ApproveWriteOff), "BillingWriteOff", "Approve");
        AssertPermission(nameof(BillingFinancialExceptionsController.Reverse), "BillingFinancialException", "Reverse");
    }

    private static async Task<(BilInvoice Invoice, BilCalculationVersion Calculation)> SeedInvoiceWithCalculationAsync(
        Repositories.ApplicationDbContext db,
        decimal patientAmount,
        string serviceType = "RAJAL",
        decimal nonBillableResidualAmount = 0m)
    {
        var encounter = new RegPatientEncounter
        {
            Id = Guid.NewGuid(),
            EncounterNumber = $"ENC-{Guid.NewGuid():N}",
            PatientId = Guid.NewGuid(),
            ServiceUnitId = Guid.NewGuid(),
            EncounterType = EncounterType.Outpatient,
            EncounterStatus = EncounterStatus.Registered,
            EncounterDate = DateTime.UtcNow,
            IsActive = true
        };
        var invoice = new BilInvoice
        {
            EncounterId = encounter.Id,
            InvoiceNumber = $"BIL-{Guid.NewGuid():N}",
            ServiceType = serviceType,
            Status = BillingInvoiceStatuses.Open,
            CurrentCalculationVersion = 1,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow
        };
        var calculation = new BilCalculationVersion
        {
            InvoiceId = invoice.Id,
            Invoice = invoice,
            VersionNo = 1,
            GrossAmount = patientAmount,
            PatientAmount = patientAmount,
            NonBillableResidualAmount = nonBillableResidualAmount,
            IsLocked = true,
            CalculatedAt = DateTimeOffset.UtcNow,
            Reason = "Kalkulasi awal pengujian",
            BreakdownSnapshot = "{}",
            CreateDateTime = DateTime.UtcNow
        };
        db.RegPatientEncounters.Add(encounter);
        db.BilInvoices.Add(invoice);
        db.BilCalculationVersions.Add(calculation);
        await db.SaveChangesAsync();
        return (invoice, calculation);
    }

    private static CreateWriteOffRequest WriteOffRequest(
        Guid invoiceId, Guid rowVersion, decimal amount, string reason,
        string? category = null, bool isFullSettlement = false) => new()
    {
        InvoiceId = invoiceId,
        Amount = amount,
        ExpectedInvoiceRowVersion = rowVersion,
        Reason = reason,
        CorrelationId = Guid.NewGuid(),
        CausationId = Guid.NewGuid(),
        Category = category,
        IsFullSettlement = isFullSettlement
    };

    private static CreateAdjustmentRequest AdjustmentRequest(
        Guid invoiceId, Guid rowVersion, string direction, decimal amount, string reason) => new()
    {
        InvoiceId = invoiceId,
        Direction = direction,
        Amount = amount,
        ExpectedInvoiceRowVersion = rowVersion,
        Reason = reason,
        CorrelationId = Guid.NewGuid(),
        CausationId = Guid.NewGuid()
    };

    private static WriteOffApprovalRequest WriteOffApproval(Guid rowVersion) => new()
    {
        ExpectedRowVersion = rowVersion,
        Reason = "Finance menyetujui write-off"
    };

    private static AdjustmentApprovalRequest AdjustmentApproval(Guid rowVersion) => new()
    {
        ExpectedRowVersion = rowVersion,
        Reason = "Finance menyetujui adjustment"
    };

    private static ReverseExceptionRequest ReverseRequest(Guid rowVersion) => new()
    {
        ExpectedRowVersion = rowVersion,
        Reason = "Membatalkan entry yang keliru"
    };

    private static BillingFinancialExceptionService CreateService(Repositories.ApplicationDbContext db) =>
        new(db, new BillingArApHandoffService(db, Logger()), Logger());

    private static LoggerService Logger() =>
        new(NullLogger<LoggerService>.Instance, new HttpContextAccessor());

    private static void AssertPermission(string methodName, string controller, string action)
    {
        var attribute = typeof(BillingFinancialExceptionsController)
            .GetMethod(methodName)?.GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(attribute);
        var arguments = Assert.IsType<object[]>(attribute!.Arguments);
        Assert.Equal(controller, arguments[0]);
        Assert.Equal(action, arguments[1]);
    }
}
