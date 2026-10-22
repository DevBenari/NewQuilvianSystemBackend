using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Tahap serah terima handoff AR ke Finance diturunkan dari data sungguhan (intake dan piutang), bukan diasumsikan dari
/// status handoff Billing. Handoff CREATED BUKAN piutang yang sudah diakui.
/// Fungsi yang diuji murni (tanpa database): ArHandoffFinanceStages.Derive.
/// </summary>
public class ArHandoffFinanceStageTests
{
    [Fact]
    public void Derive_NoIntakeAndNoReceivable_IsHandoffCreated()
    {
        Assert.Equal(ArHandoffFinanceStages.HandoffCreated, ArHandoffFinanceStages.Derive(null, false));
        Assert.Equal(ArHandoffFinanceStages.HandoffCreated, ArHandoffFinanceStages.Derive("  ", false));
    }

    [Theory]
    [InlineData("NEW")]
    [InlineData("new")]
    public void Derive_NewIntake_IsIntakePending(string intakeStatus)
    {
        Assert.Equal(ArHandoffFinanceStages.IntakePending, ArHandoffFinanceStages.Derive(intakeStatus, false));
    }

    [Fact]
    public void Derive_ErrorIntake_IsIntakeError()
    {
        Assert.Equal(ArHandoffFinanceStages.IntakeError, ArHandoffFinanceStages.Derive("ERROR", false));
    }

    [Theory]
    [InlineData("CONSUMED")]
    [InlineData("ACKNOWLEDGED")]
    public void Derive_IntakeDoneButNoReceivable_RequiresReconciliation(string intakeStatus)
    {
        // Intake tercatat selesai tetapi piutang tidak ditemukan: jangan dilaporkan sebagai diakui.
        Assert.Equal(ArHandoffFinanceStages.ReconciliationRequired, ArHandoffFinanceStages.Derive(intakeStatus, false));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("NEW")]
    [InlineData("ERROR")]
    [InlineData("CONSUMED")]
    [InlineData("ACKNOWLEDGED")]
    public void Derive_ExistingReceivable_IsAlwaysRecognized(string? intakeStatus)
    {
        // Piutang yang ada adalah bukti pengakuan dan menang atas status intake apa pun.
        Assert.Equal(ArHandoffFinanceStages.ReceivableRecognized, ArHandoffFinanceStages.Derive(intakeStatus, true));
    }

    [Fact]
    public void Derive_UnknownIntakeStatus_DoesNotClaimRecognition()
    {
        Assert.Equal(ArHandoffFinanceStages.IntakePending, ArHandoffFinanceStages.Derive("SOMETHING_NEW", false));
    }
}
