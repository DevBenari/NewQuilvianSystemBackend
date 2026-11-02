using System;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using Xunit;

namespace QuilvianSystemBackend.Tests.OperatingRoom;

public class InpatientNewbornBirthEventTests
{
    [Fact]
    public void OprBirthRecordedIntegrationEvent_ShouldHaveDefaultValues()
    {
        var evt = new OprBirthRecordedIntegrationEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            CaseId = Guid.NewGuid(),
            BirthEventId = Guid.NewGuid(),
            MotherPatientId = Guid.NewGuid(),
            Gender = "Female",
            WeightGrams = 3150,
            LengthCentimeters = 49,
            ApgarScore1Min = 8,
            ApgarScore5Min = 9
        };

        Assert.Equal("operating-room.birth-event.recorded", evt.EventType);
        Assert.Equal("1.0", evt.Version);
        Assert.Equal("Female", evt.Gender);
        Assert.Equal(3150, evt.WeightGrams);
        Assert.Equal(8, evt.ApgarScore1Min);
        Assert.Equal(9, evt.ApgarScore5Min);
    }

    [Fact]
    public void NewbornBirthEventConsumerResult_ReportsSuccessAndIdempotency()
    {
        var resultSuccess = new NewbornBirthEventConsumerResult
        {
            Succeeded = true,
            AlreadyProcessed = false,
            BabyPatientId = Guid.NewGuid(),
            BabyMedicalRecordNumber = "BY-261024-1234",
            BabyEpisodeId = Guid.NewGuid(),
            BabyEpisodeNumber = "RWI-BY/20261024/001",
            Message = "Episode rawat inap bayi baru lahir berhasil dibuka otomatis."
        };

        Assert.True(resultSuccess.Succeeded);
        Assert.False(resultSuccess.AlreadyProcessed);
        Assert.NotNull(resultSuccess.BabyPatientId);
        Assert.StartsWith("BY-", resultSuccess.BabyMedicalRecordNumber);
        Assert.StartsWith("RWI-BY/", resultSuccess.BabyEpisodeNumber);

        var resultIdempotent = new NewbornBirthEventConsumerResult
        {
            Succeeded = true,
            AlreadyProcessed = true,
            BabyPatientId = resultSuccess.BabyPatientId,
            BabyMedicalRecordNumber = resultSuccess.BabyMedicalRecordNumber,
            Message = "Event kelahiran bayi ini sudah diproses sebelumnya (Idempotent)."
        };

        Assert.True(resultIdempotent.Succeeded);
        Assert.True(resultIdempotent.AlreadyProcessed);
        Assert.Equal(resultSuccess.BabyPatientId, resultIdempotent.BabyPatientId);
    }
}
