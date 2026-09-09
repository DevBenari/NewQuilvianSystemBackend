using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.BillingManagement;

public sealed class BillingDepositServiceTests
{
    [Fact]
    public async Task FirstNonCashTopUpCreatesOneInpatientAccountAndAppendOnlyMovement()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, paymentMethodId) = await SeedAsync(db);
        var service = CreateService(db);

        var result = await service.TopUpAsync(
            encounterId,
            TopUpRequest(paymentMethodId, 8_000_000m),
            Guid.NewGuid(),
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.Equal(BillingSettlementPurposes.DepositTopUp, result.Purpose);
        Assert.Equal(BillingSettlementStatuses.Settled, result.Status);
        Assert.Equal(8_000_000m, result.SuccessfulAmount);
        Assert.Equal(0, result.AllocatedAmount);
        Assert.False(result.IsReplay);
        var deposit = Assert.IsType<DepositResponse>(result.Deposit);
        Assert.StartsWith("DEP-", deposit.AccountNumber);
        Assert.Equal(8_000_000m, deposit.AvailableBalance);
        Assert.Equal(BillingDepositAccountStatuses.Active, deposit.Status);
        var movement = Assert.Single(deposit.Movements);
        Assert.Equal(BillingDepositMovementTypes.TopUp, movement.MovementType);
        Assert.Equal(8_000_000m, movement.BalanceEffect);
        Assert.Equal(8_000_000m, movement.BalanceAfter);
        Assert.Equal(paymentMethodId, movement.PaymentMethodId);
        Assert.Single(db.BilDepositAccounts);
        Assert.Single(db.BilDepositMovements);
    }

    [Fact]
    public async Task AdditionalTopUpReusesAccountAndRequiresCurrentVersion()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, paymentMethodId) = await SeedAsync(db);
        var service = CreateService(db);
        var first = await service.TopUpAsync(
            encounterId, TopUpRequest(paymentMethodId, 5_000_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var firstDeposit = Assert.IsType<DepositResponse>(first.Deposit);

        var second = await service.TopUpAsync(
            encounterId,
            TopUpRequest(paymentMethodId, 3_000_000m, firstDeposit.RowVersion),
            Guid.NewGuid(),
            Guid.NewGuid(),
            CancellationToken.None);
        var secondDeposit = Assert.IsType<DepositResponse>(second.Deposit);

        Assert.Equal(firstDeposit.Id, secondDeposit.Id);
        Assert.Equal(8_000_000m, secondDeposit.AvailableBalance);
        Assert.Equal(2, secondDeposit.Movements.Count);
        Assert.Single(db.BilDepositAccounts);
        Assert.Equal(2, db.BilDepositMovements.Count());
    }

    [Fact]
    public async Task IdenticalIdempotencyReplayDoesNotCreateAnotherMovement()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, paymentMethodId) = await SeedAsync(db);
        var service = CreateService(db);
        var request = TopUpRequest(paymentMethodId, 750_000m);
        var key = Guid.NewGuid();

        var first = await service.TopUpAsync(
            encounterId, request, key, Guid.NewGuid(), CancellationToken.None);
        var replay = await service.TopUpAsync(
            encounterId, request, key, Guid.NewGuid(), CancellationToken.None);

        Assert.False(first.IsReplay);
        Assert.True(replay.IsReplay);
        Assert.Equal(first.DepositMovementId, replay.DepositMovementId);
        Assert.Equal(
            750_000m,
            Assert.IsType<DepositResponse>(replay.Deposit).AvailableBalance);
        Assert.Single(db.BilDepositMovements);
    }

    [Fact]
    public async Task ReusedIdempotencyKeyWithDifferentPayloadIsRejected()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, paymentMethodId) = await SeedAsync(db);
        var service = CreateService(db);
        var request = TopUpRequest(paymentMethodId, 500_000m);
        var key = Guid.NewGuid();
        await service.TopUpAsync(
            encounterId, request, key, Guid.NewGuid(), CancellationToken.None);
        request.Amount = 600_000m;

        var exception = await Assert.ThrowsAsync<BillingDepositConflictException>(() =>
            service.TopUpAsync(
                encounterId, request, key, Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("isi berbeda", exception.Message);
        Assert.Single(db.BilDepositMovements);
    }

    [Fact]
    public async Task StaleVersionRejectsConcurrentTopUpWithoutChangingBalance()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, paymentMethodId) = await SeedAsync(db);
        var service = CreateService(db);
        var first = await service.TopUpAsync(
            encounterId, TopUpRequest(paymentMethodId, 1_000_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var firstDeposit = Assert.IsType<DepositResponse>(first.Deposit);
        await service.TopUpAsync(
            encounterId, TopUpRequest(paymentMethodId, 250_000m, firstDeposit.RowVersion),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BillingDepositConflictException>(() =>
            service.TopUpAsync(
                encounterId, TopUpRequest(paymentMethodId, 100_000m, firstDeposit.RowVersion),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("Muat ulang", exception.Message);
        Assert.Equal(1_250_000m, db.BilDepositAccounts.Single().AvailableBalance);
        Assert.Equal(2, db.BilDepositMovements.Count());
    }

    [Fact]
    public async Task CashTopUpIsRejectedUntilCashierShiftExists()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, _) = await SeedAsync(db);
        var cashMethod = PaymentMethod(isCash: true);
        db.MstPaymentMethods.Add(cashMethod);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var exception = await Assert.ThrowsAsync<BillingDepositValidationException>(() =>
            service.TopUpAsync(
                encounterId, TopUpRequest(cashMethod.Id, 100_000m),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Equal("Buka shift kasir sebelum menerima uang tunai.", exception.Message);
        Assert.Empty(db.BilDepositAccounts);
        Assert.Empty(db.BilDepositMovements);
    }

    [Fact]
    public async Task CashTopUpUsesActiveShiftAndIncreasesSystemCashExactlyOnce()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, _) = await SeedAsync(db);
        var cashMethod = PaymentMethod(isCash: true);
        var actorId = Guid.NewGuid();
        var shift = new BilCashierShift
        {
            ShiftNumber = $"CSH-TEST-{Guid.NewGuid():N}",
            CashierId = actorId,
            RegisterId = Guid.NewGuid(),
            OpeningCash = 25_000m,
            Status = CashierShiftStatuses.Open,
            OpenedAt = DateTimeOffset.UtcNow,
            RowVersion = Guid.NewGuid()
        };
        db.MstPaymentMethods.Add(cashMethod);
        db.BilCashierShifts.Add(shift);
        await db.SaveChangesAsync();
        var service = CreateService(db);
        var request = TopUpRequest(cashMethod.Id, 100_000m);
        var key = Guid.NewGuid();

        var result = await service.TopUpAsync(
            encounterId, request, key, actorId, CancellationToken.None);
        var replay = await service.TopUpAsync(
            encounterId, request, key, actorId, CancellationToken.None);

        Assert.False(result.IsReplay);
        Assert.True(replay.IsReplay);
        Assert.Equal(100_000m, shift.SystemCash);
        var movement = Assert.Single(db.BilDepositMovements);
        Assert.Equal(shift.Id, movement.CashierShiftId);
        Assert.Single(db.BilCashierShiftCommands.Where(
            x => x.CommandType == CashierShiftCommandTypes.CashReceipt));
    }

    [Fact]
    public async Task TenderDependentNonCashMethodIsRejectedAtCurrentBoundary()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, _) = await SeedAsync(db);
        var qrisMethod = PaymentMethod();
        qrisMethod.IsQris = true;
        qrisMethod.IsNeedReferenceNumber = true;
        db.MstPaymentMethods.Add(qrisMethod);
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var exception = await Assert.ThrowsAsync<BillingDepositValidationException>(() =>
            service.TopUpAsync(
                encounterId, TopUpRequest(qrisMethod.Id, 100_000m),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("settlement/tender", exception.Message);
        Assert.Empty(db.BilDepositMovements);
    }

    [Fact]
    public async Task DepositIsRejectedForNonInpatientEncounter()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, paymentMethodId) = await SeedAsync(db, EncounterType.Outpatient);
        var service = CreateService(db);

        var exception = await Assert.ThrowsAsync<BillingDepositValidationException>(() =>
            service.TopUpAsync(
                encounterId, TopUpRequest(paymentMethodId, 100_000m),
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("rawat inap", exception.Message);
        Assert.Empty(db.BilDepositAccounts);
    }

    [Fact]
    public async Task TopUpDoesNotAllocateOrLockRunningInvoice()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, paymentMethodId) = await SeedAsync(db);
        var invoice = new BilInvoice
        {
            EncounterId = encounterId,
            InvoiceNumber = "BIL-TEST-0001",
            ServiceType = "RANAP",
            Status = BillingInvoiceStatuses.Open,
            CurrentCalculationVersion = 0,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow
        };
        db.BilInvoices.Add(invoice);
        await db.SaveChangesAsync();
        var originalInvoiceVersion = invoice.RowVersion;
        var service = CreateService(db);

        var result = await service.TopUpAsync(
            encounterId, TopUpRequest(paymentMethodId, 8_000_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var deposit = Assert.IsType<DepositResponse>(result.Deposit);

        Assert.Equal(8_000_000m, deposit.AvailableBalance);
        Assert.Equal(0, result.AllocatedAmount);
        Assert.Null(deposit.Movements.Single().SettlementId);
        Assert.Equal(BillingInvoiceStatuses.Open, invoice.Status);
        Assert.Equal(0, invoice.CurrentCalculationVersion);
        Assert.Equal(originalInvoiceVersion, invoice.RowVersion);
        Assert.Empty(db.BilCalculationVersions);
    }

    [Fact]
    public async Task ReversalAppendsCompensatingMovementAndIsIdempotent()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, paymentMethodId) = await SeedAsync(db);
        var service = CreateService(db);
        var topUp = await service.TopUpAsync(
            encounterId, TopUpRequest(paymentMethodId, 900_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var deposit = Assert.IsType<DepositResponse>(topUp.Deposit);
        var original = Assert.Single(deposit.Movements);
        var request = ReversalRequest(deposit.RowVersion);
        var key = Guid.NewGuid();

        var reversed = await service.ReverseTopUpAsync(
            encounterId, original.Id, request, key, Guid.NewGuid(), CancellationToken.None);
        var replay = await service.ReverseTopUpAsync(
            encounterId, original.Id, request, key, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(0, reversed.AvailableBalance);
        Assert.Equal(2, reversed.Movements.Count);
        Assert.Equal(2, replay.Movements.Count);
        var storedOriginal = db.BilDepositMovements.Single(x => x.Id == original.Id);
        Assert.Equal(BillingDepositMovementTypes.TopUp, storedOriginal.MovementType);
        Assert.Equal(900_000m, storedOriginal.Amount);
        Assert.False(storedOriginal.IsDelete);
        var compensating = db.BilDepositMovements.Single(x => x.ReversesMovementId == original.Id);
        Assert.Equal(BillingDepositMovementTypes.Reversal, compensating.MovementType);
        Assert.Equal(900_000m, compensating.Amount);
    }

    [Fact]
    public async Task ReversalCannotMakeDepositBalanceNegative()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, paymentMethodId) = await SeedAsync(db);
        var service = CreateService(db);
        var topUp = await service.TopUpAsync(
            encounterId, TopUpRequest(paymentMethodId, 500_000m),
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        var account = db.BilDepositAccounts.Single();
        account.AvailableBalance = 250_000m;
        account.RowVersion = Guid.NewGuid();
        await db.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BillingDepositValidationException>(() =>
            service.ReverseTopUpAsync(
                encounterId,
                topUp.DepositMovementId!.Value,
                ReversalRequest(account.RowVersion),
                Guid.NewGuid(),
                Guid.NewGuid(),
                CancellationToken.None));

        Assert.Equal("Dana deposit atau saldo tagihan tidak mencukupi.", exception.Message);
        Assert.Equal(250_000m, account.AvailableBalance);
        Assert.Single(db.BilDepositMovements);
    }

    [Fact]
    public void ControllerContractUsesLockedRoutesAndPermissions()
    {
        var route = typeof(BillingPatientFundsController).GetCustomAttribute<RouteAttribute>();
        Assert.Equal(
            "api/v1/health-services/billing-management/billing/patient-funds",
            route?.Template);
        AssertPermission(nameof(BillingPatientFundsController.GetDeposit), "Read");
        AssertPermission(nameof(BillingPatientFundsController.GetDepositPolicy), "Read");
        AssertPermission(nameof(BillingPatientFundsController.GetEpisodeDepositSummary), "Read");
        AssertPermission(nameof(BillingPatientFundsController.TopUp), "Create");
    }

    [Fact]
    public async Task GetDepositPolicyAsync_WhenPolicyExists_ReturnsPolicyDetails()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        var guarantorId = Guid.NewGuid();
        var patientClassId = Guid.NewGuid();

        var policy = new MstDepositPolicy
        {
            Code = "POL-BPJS-VIP",
            Name = "Kebijakan Deposit BPJS Kelas VIP",
            GuarantorId = guarantorId,
            PatientClassId = patientClassId,
            IsRequired = true,
            MinimumAmount = 5_000_000m,
            FollowUpIntervalDays = 3,
            EffectiveFrom = DateTimeOffset.UtcNow.AddDays(-1),
            EffectiveTo = DateTimeOffset.UtcNow.AddYears(1),
            IsActive = true
        };
        db.MstDepositPolicies.Add(policy);
        await db.SaveChangesAsync();

        var result = await service.GetDepositPolicyAsync(guarantorId, patientClassId, CancellationToken.None);

        Assert.True(result.IsRequired);
        Assert.Equal(5_000_000m, result.MinimumAmount);
        Assert.Equal(3, result.FollowUpIntervalDays);
        Assert.Equal(policy.Id, result.PolicyId);
        Assert.Equal("POL-BPJS-VIP", result.PolicyCode);
        Assert.Equal(guarantorId, result.GuarantorId);
        Assert.Equal(patientClassId, result.PatientClassId);
    }

    [Fact]
    public async Task GetDepositPolicyAsync_WhenGuarantorFullyCovers_ReturnsIsRequiredFalse()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        var guarantorId = Guid.NewGuid();
        var patientClassId = Guid.NewGuid();

        var policy = new MstDepositPolicy
        {
            Code = "POL-CORP-FULL",
            Name = "Penjamin Menanggung Penuh Tanpa Deposit",
            GuarantorId = guarantorId,
            PatientClassId = patientClassId,
            IsRequired = false,
            MinimumAmount = 0m,
            FollowUpIntervalDays = 0,
            EffectiveFrom = DateTimeOffset.UtcNow.AddDays(-1),
            EffectiveTo = null,
            IsActive = true
        };
        db.MstDepositPolicies.Add(policy);
        await db.SaveChangesAsync();

        var result = await service.GetDepositPolicyAsync(guarantorId, patientClassId, CancellationToken.None);

        Assert.False(result.IsRequired);
        Assert.Equal(0m, result.MinimumAmount);
        Assert.Equal(policy.Id, result.PolicyId);
    }

    [Fact]
    public async Task GetDepositPolicyAsync_WhenNoPolicyConfigured_ReturnsIsRequiredFalseWithoutError()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        var guarantorId = Guid.NewGuid();
        var patientClassId = Guid.NewGuid();

        var result = await service.GetDepositPolicyAsync(guarantorId, patientClassId, CancellationToken.None);

        Assert.False(result.IsRequired);
        Assert.Equal(0m, result.MinimumAmount);
        Assert.Equal(0, result.FollowUpIntervalDays);
        Assert.Null(result.PolicyId);
        Assert.Null(result.PolicyCode);
        Assert.Equal(guarantorId, result.GuarantorId);
        Assert.Equal(patientClassId, result.PatientClassId);
    }

    [Fact]
    public async Task GetDepositPolicyAsync_WhenPolicyUpdated_ReflectsImmediately()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var service = CreateService(db);
        var guarantorId = Guid.NewGuid();
        var patientClassId = Guid.NewGuid();

        var policy = new MstDepositPolicy
        {
            Code = "POL-UPDATE-TEST",
            Name = "Kebijakan Awal",
            GuarantorId = guarantorId,
            PatientClassId = patientClassId,
            IsRequired = true,
            MinimumAmount = 1_000_000m,
            FollowUpIntervalDays = 2,
            EffectiveFrom = DateTimeOffset.UtcNow.AddDays(-1),
            IsActive = true
        };
        db.MstDepositPolicies.Add(policy);
        await db.SaveChangesAsync();

        var firstRead = await service.GetDepositPolicyAsync(guarantorId, patientClassId, CancellationToken.None);
        Assert.Equal(1_000_000m, firstRead.MinimumAmount);

        policy.MinimumAmount = 2_500_000m;
        policy.FollowUpIntervalDays = 5;
        await db.SaveChangesAsync();

        var secondRead = await service.GetDepositPolicyAsync(guarantorId, patientClassId, CancellationToken.None);
        Assert.Equal(2_500_000m, secondRead.MinimumAmount);
        Assert.Equal(5, secondRead.FollowUpIntervalDays);
    }

    [Fact]
    public async Task GetEpisodeDepositSummaryAsync_WhenNoDeposit_ReturnsZeroedSummaryWithout404()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, _) = await SeedAsync(db);
        var service = CreateService(db);
        var guarantorId = Guid.NewGuid();
        var patientClassId = Guid.NewGuid();

        var episode = new InpEpisode
        {
            Id = Guid.NewGuid(),
            EpisodeNumber = $"EP-{Guid.NewGuid():N}"[..20],
            EncounterId = encounterId,
            PatientId = Guid.NewGuid(),
            ServiceUnitId = Guid.NewGuid(),
            PatientClassId = patientClassId,
            EpisodeStatus = InpEpisodeStatus.Admitted,
            IsActive = true
        };
        db.Set<InpEpisode>().Add(episode);

        var guarantor = new RegPatientEncounterGuarantor
        {
            Id = Guid.NewGuid(),
            EncounterId = encounterId,
            PatientId = episode.PatientId,
            InsuranceProviderId = guarantorId,
            IsPrimary = true,
            Priority = 1,
            IsActive = true
        };
        db.RegPatientEncounterGuarantors.Add(guarantor);

        var policy = new MstDepositPolicy
        {
            Code = "POL-MIN-3M",
            Name = "Kebijakan Deposit Minimum 3 Juta",
            GuarantorId = guarantorId,
            PatientClassId = patientClassId,
            IsRequired = true,
            MinimumAmount = 3_000_000m,
            FollowUpIntervalDays = 3,
            EffectiveFrom = DateTimeOffset.UtcNow.AddDays(-1),
            IsActive = true
        };
        db.MstDepositPolicies.Add(policy);
        await db.SaveChangesAsync();

        var summary = await service.GetEpisodeDepositSummaryAsync(episode.Id, CancellationToken.None);

        Assert.NotNull(summary);
        Assert.Equal(episode.Id, summary.EpisodeId);
        Assert.Equal(encounterId, summary.EncounterId);
        Assert.False(summary.HasDepositAccount);
        Assert.Null(summary.DepositAccountId);
        Assert.Empty(summary.AccountNumber);
        Assert.Empty(summary.DepositStatus);
        Assert.True(summary.IsPolicyRequired);
        Assert.Equal(3_000_000m, summary.MinimumPolicyAmount);
        Assert.Equal(0m, summary.TotalReceived);
        Assert.Equal(0m, summary.TotalAllocated);
        Assert.Equal(0m, summary.TotalRefunded);
        Assert.Equal(0m, summary.AvailableBalance);
        Assert.Equal(3_000_000m, summary.PolicyShortfallAmount);
        Assert.Equal(0m, summary.FinalBillAmount);
        Assert.Equal(0m, summary.FinalBillShortfallAmount);
        Assert.Equal(3_000_000m, summary.OutstandingTopUp);
        Assert.Equal(3, summary.FollowUpIntervalDays);
    }

    [Fact]
    public async Task GetEpisodeDepositSummaryAsync_SeparatesPolicyShortfallAndFinalBillShortfall()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, paymentMethodId) = await SeedAsync(db);
        var service = CreateService(db);
        var guarantorId = Guid.NewGuid();
        var patientClassId = Guid.NewGuid();

        var episode = new InpEpisode
        {
            Id = Guid.NewGuid(),
            EpisodeNumber = $"EP-{Guid.NewGuid():N}"[..20],
            EncounterId = encounterId,
            PatientId = Guid.NewGuid(),
            ServiceUnitId = Guid.NewGuid(),
            PatientClassId = patientClassId,
            EpisodeStatus = InpEpisodeStatus.Admitted,
            IsActive = true
        };
        db.Set<InpEpisode>().Add(episode);

        var guarantor = new RegPatientEncounterGuarantor
        {
            Id = Guid.NewGuid(),
            EncounterId = encounterId,
            PatientId = episode.PatientId,
            InsuranceProviderId = guarantorId,
            IsPrimary = true,
            Priority = 1,
            IsActive = true
        };
        db.RegPatientEncounterGuarantors.Add(guarantor);

        var policy = new MstDepositPolicy
        {
            Code = "POL-MIN-5M",
            Name = "Kebijakan Deposit Minimum 5 Juta",
            GuarantorId = guarantorId,
            PatientClassId = patientClassId,
            IsRequired = true,
            MinimumAmount = 5_000_000m,
            FollowUpIntervalDays = 3,
            EffectiveFrom = DateTimeOffset.UtcNow.AddDays(-1),
            IsActive = true
        };
        db.MstDepositPolicies.Add(policy);

        var account = new BilDepositAccount
        {
            Id = Guid.NewGuid(),
            EncounterId = encounterId,
            AccountNumber = "DEP-SEP-001",
            AvailableBalance = 3_000_000m,
            Status = BillingDepositAccountStatuses.Active,
            IsActive = true
        };
        db.BilDepositAccounts.Add(account);

        var movement = new BilDepositMovement
        {
            Id = Guid.NewGuid(),
            DepositAccountId = account.Id,
            MovementType = BillingDepositMovementTypes.TopUp,
            Amount = 3_000_000m,
            PaymentMethodId = paymentMethodId,
            OccurredAt = DateTimeOffset.UtcNow,
            Reason = "Top-up parsial",
            PayloadHash = "hash-sep",
            IsActive = true
        };
        db.BilDepositMovements.Add(movement);

        var invoice = new BilInvoice
        {
            Id = Guid.NewGuid(),
            EncounterId = encounterId,
            InvoiceNumber = "INV-SEP-001",
            ServiceType = "Inpatient",
            Status = BillingInvoiceStatuses.Open,
            CurrentCalculationVersion = 1,
            IsActive = true
        };
        db.BilInvoices.Add(invoice);

        var calc = new BilCalculationVersion
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            VersionNo = 1,
            PatientAmount = 10_000_000m,
            Reason = "Kalkulasi sementara",
            IsActive = true
        };
        db.BilCalculationVersions.Add(calc);

        await db.SaveChangesAsync();

        var summary = await service.GetEpisodeDepositSummaryAsync(episode.Id, CancellationToken.None);

        Assert.NotNull(summary);
        Assert.True(summary.HasDepositAccount);
        Assert.Equal(3_000_000m, summary.TotalReceived);
        Assert.Equal(3_000_000m, summary.AvailableBalance);
        Assert.Equal(0m, summary.TotalAllocated);
        Assert.Equal(5_000_000m, summary.MinimumPolicyAmount);

        // Dua angka kekurangan terpisah sesuai RWI-DEC-095:
        // PolicyShortfallAmount: 5.000.000 (minimum kebijakan) - 3.000.000 (diterima) = 2.000.000
        Assert.Equal(2_000_000m, summary.PolicyShortfallAmount);
        Assert.Equal(2_000_000m, summary.OutstandingTopUp);

        // FinalBillShortfallAmount: 10.000.000 (tagihan final) - 0 (dialokasikan) - 3.000.000 (saldo tersedia) = 7.000.000
        Assert.Equal(10_000_000m, summary.FinalBillAmount);
        Assert.Equal(7_000_000m, summary.FinalBillShortfallAmount);

        // Kedua kekurangan tidak pernah disatukan
        Assert.NotEqual(summary.PolicyShortfallAmount, summary.FinalBillShortfallAmount);
    }

    [Fact]
    public async Task GetEpisodeDepositSummaryAsync_CalculatesCorrectlyFromMovementsAndInvoice()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var (encounterId, paymentMethodId) = await SeedAsync(db);
        var service = CreateService(db);
        var guarantorId = Guid.NewGuid();
        var patientClassId = Guid.NewGuid();

        var episode = new InpEpisode
        {
            Id = Guid.NewGuid(),
            EpisodeNumber = $"EP-{Guid.NewGuid():N}"[..20],
            EncounterId = encounterId,
            PatientId = Guid.NewGuid(),
            ServiceUnitId = Guid.NewGuid(),
            PatientClassId = patientClassId,
            EpisodeStatus = InpEpisodeStatus.Admitted,
            IsActive = true
        };
        db.Set<InpEpisode>().Add(episode);

        var guarantor = new RegPatientEncounterGuarantor
        {
            Id = Guid.NewGuid(),
            EncounterId = encounterId,
            PatientId = episode.PatientId,
            InsuranceProviderId = guarantorId,
            IsPrimary = true,
            Priority = 1,
            IsActive = true
        };
        db.RegPatientEncounterGuarantors.Add(guarantor);

        var policy = new MstDepositPolicy
        {
            Code = "POL-MIN-5M-FULL",
            Name = "Kebijakan Deposit 5 Juta",
            GuarantorId = guarantorId,
            PatientClassId = patientClassId,
            IsRequired = true,
            MinimumAmount = 5_000_000m,
            FollowUpIntervalDays = 2,
            EffectiveFrom = DateTimeOffset.UtcNow.AddDays(-1),
            IsActive = true
        };
        db.MstDepositPolicies.Add(policy);

        var account = new BilDepositAccount
        {
            Id = Guid.NewGuid(),
            EncounterId = encounterId,
            AccountNumber = "DEP-CALC-001",
            AvailableBalance = 2_000_000m,
            Status = BillingDepositAccountStatuses.Active,
            IsActive = true
        };
        db.BilDepositAccounts.Add(account);

        var topUp1 = new BilDepositMovement
        {
            Id = Guid.NewGuid(),
            DepositAccountId = account.Id,
            MovementType = BillingDepositMovementTypes.TopUp,
            Amount = 10_000_000m,
            PaymentMethodId = paymentMethodId,
            OccurredAt = DateTimeOffset.UtcNow.AddHours(-3),
            Reason = "Top-up awal 10 juta",
            PayloadHash = "hash1",
            IsActive = true
        };
        var reversal = new BilDepositMovement
        {
            Id = Guid.NewGuid(),
            DepositAccountId = account.Id,
            MovementType = BillingDepositMovementTypes.Reversal,
            Amount = 2_000_000m,
            PaymentMethodId = paymentMethodId,
            OccurredAt = DateTimeOffset.UtcNow.AddHours(-2),
            Reason = "Koreksi kelebihan setor 2 juta",
            PayloadHash = "hash2",
            IsActive = true
        };
        var allocation = new BilDepositMovement
        {
            Id = Guid.NewGuid(),
            DepositAccountId = account.Id,
            MovementType = BillingDepositMovementTypes.Allocation,
            Amount = 5_000_000m,
            OccurredAt = DateTimeOffset.UtcNow.AddHours(-1),
            Reason = "Alokasi tagihan running 5 juta",
            PayloadHash = "hash3",
            IsActive = true
        };
        var refund = new BilDepositMovement
        {
            Id = Guid.NewGuid(),
            DepositAccountId = account.Id,
            MovementType = BillingDepositMovementTypes.Release,
            Amount = 1_000_000m,
            OccurredAt = DateTimeOffset.UtcNow,
            Reason = "Refund pengembalian 1 juta",
            PayloadHash = "hash4",
            IsActive = true
        };
        db.BilDepositMovements.AddRange(topUp1, reversal, allocation, refund);

        var invoice = new BilInvoice
        {
            Id = Guid.NewGuid(),
            EncounterId = encounterId,
            InvoiceNumber = "INV-CALC-001",
            ServiceType = "Inpatient",
            Status = BillingInvoiceStatuses.Open,
            CurrentCalculationVersion = 1,
            IsActive = true
        };
        db.BilInvoices.Add(invoice);

        var calc = new BilCalculationVersion
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            VersionNo = 1,
            PatientAmount = 8_000_000m,
            Reason = "Tagihan kamar dan tindakan",
            IsActive = true
        };
        db.BilCalculationVersions.Add(calc);

        await db.SaveChangesAsync();

        var summary = await service.GetEpisodeDepositSummaryAsync(episode.Id, CancellationToken.None);

        Assert.NotNull(summary);
        Assert.True(summary.HasDepositAccount);
        Assert.Equal(account.Id, summary.DepositAccountId);
        Assert.Equal("DEP-CALC-001", summary.AccountNumber);
        Assert.Equal(BillingDepositAccountStatuses.Active, summary.DepositStatus);

        // TotalReceived = 10.000.000 (TopUp) - 2.000.000 (Reversal) = 8.000.000
        Assert.Equal(8_000_000m, summary.TotalReceived);
        // TotalAllocated = 5.000.000
        Assert.Equal(5_000_000m, summary.TotalAllocated);
        // TotalRefunded = 1.000.000
        Assert.Equal(1_000_000m, summary.TotalRefunded);
        // AvailableBalance = 2.000.000
        Assert.Equal(2_000_000m, summary.AvailableBalance);

        // Policy minimum = 5.000.000, TotalReceived = 8.000.000 >= 5.000.000 -> PolicyShortfall = 0
        Assert.Equal(0m, summary.PolicyShortfallAmount);
        Assert.Equal(0m, summary.OutstandingTopUp);

        // FinalBill = 8.000.000. Shortfall = 8.000.000 - 5.000.000 (allocated) - 2.000.000 (available) = 1.000.000
        Assert.Equal(8_000_000m, summary.FinalBillAmount);
        Assert.Equal(1_000_000m, summary.FinalBillShortfallAmount);
    }

    private static async Task<(Guid EncounterId, Guid PaymentMethodId)> SeedAsync(
        Repositories.ApplicationDbContext db,
        EncounterType encounterType = EncounterType.Inpatient)
    {
        var encounter = new TrxPatientEncounter
        {
            EncounterNumber = $"ENC-{Guid.NewGuid():N}",
            PatientId = Guid.NewGuid(),
            ServiceUnitId = Guid.NewGuid(),
            EncounterType = encounterType,
            EncounterStatus = EncounterStatus.Registered,
            IsActive = true
        };
        var paymentMethod = PaymentMethod();
        db.TrxPatientEncounters.Add(encounter);
        db.MstPaymentMethods.Add(paymentMethod);
        await db.SaveChangesAsync();
        return (encounter.Id, paymentMethod.Id);
    }

    private static MstPaymentMethod PaymentMethod(bool isCash = false) => new()
    {
        PaymentMethodCode = $"PM-{Guid.NewGuid():N}"[..20],
        PaymentMethodName = isCash ? "Tunai Uji" : "Non Tunai Manual Uji",
        PaymentMethodType = isCash ? "Cash" : "Transfer",
        IsCash = isCash,
        IsBankTransfer = !isCash,
        IsActive = true,
        IsAvailableForBilling = true
    };

    private static DepositTopUpRequest TopUpRequest(
        Guid paymentMethodId,
        decimal amount,
        Guid? expectedRowVersion = null) => new()
    {
        PaymentMethodId = paymentMethodId,
        Amount = amount,
        ExpectedRowVersion = expectedRowVersion,
        Reason = "Top-up deposit rawat inap untuk pengujian",
        CorrelationId = Guid.NewGuid(),
        CausationId = Guid.NewGuid()
    };

    private static ReverseDepositMovementRequest ReversalRequest(Guid rowVersion) => new()
    {
        ExpectedRowVersion = rowVersion,
        Reason = "Koreksi penerimaan top-up untuk pengujian",
        CorrelationId = Guid.NewGuid(),
        CausationId = Guid.NewGuid()
    };

    private static BillingDepositService CreateService(
        Repositories.ApplicationDbContext db)
    {
        var logger = new LoggerService(
            NullLogger<LoggerService>.Instance,
            new HttpContextAccessor());
        var numberSeries = new BillingNumberSeriesService(
            db,
            Options.Create(new BillingInvoiceNumberOptions()),
            Options.Create(new BillingDepositAccountNumberOptions()),
            Options.Create(new BillingCashierShiftNumberOptions()));
        var cashierShiftService = new CashierShiftService(db, numberSeries, logger);
        return new BillingDepositService(db, numberSeries, cashierShiftService, logger);
    }

    private static void AssertPermission(string methodName, string action)
    {
        var attribute = typeof(BillingPatientFundsController)
            .GetMethod(methodName)?
            .GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(attribute);
        var arguments = Assert.IsType<object[]>(attribute!.Arguments);
        Assert.Equal("BillingDeposit", arguments[0]);
        Assert.Equal(action, arguments[1]);
    }
}
