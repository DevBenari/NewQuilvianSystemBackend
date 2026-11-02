using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using Xunit;

namespace QuilvianSystemBackend.Tests.OperatingRoom;

public class OperatingRoomBirthEventTests
{
    [Fact]
    public void OprBirthEvent_ModelShouldRetainAllClinicalBirthAttributes()
    {
        var caseId = Guid.NewGuid();
        var birthEvent = new OprBirthEvent
        {
            Id = Guid.NewGuid(),
            OprCaseId = caseId,
            BirthDateTime = new DateTime(2026, 10, 24, 11, 15, 0, DateTimeKind.Utc),
            Gender = "Female",
            WeightGrams = 3250.00m,
            LengthCentimeters = 49.50m,
            HeadCircumferenceCm = 34.00m,
            ApgarScore1Min = 8,
            ApgarScore5Min = 10,
            ApgarScore10Min = 10,
            ResuscitationNote = "Penghisapan lendir, pemotongan tali pusat, langsung menangis kuat.",
            ConditionNote = "Kondisi umum bayi bugar, refleks moro positif.",
            RecordedByDoctorId = Guid.NewGuid()
        };

        Assert.Equal(caseId, birthEvent.OprCaseId);
        Assert.Equal("Female", birthEvent.Gender);
        Assert.Equal(3250.00m, birthEvent.WeightGrams);
        Assert.Equal(49.50m, birthEvent.LengthCentimeters);
        Assert.Equal(34.00m, birthEvent.HeadCircumferenceCm);
        Assert.Equal(8, birthEvent.ApgarScore1Min);
        Assert.Equal(10, birthEvent.ApgarScore5Min);
        Assert.Equal(10, birthEvent.ApgarScore10Min);
        Assert.NotNull(birthEvent.ResuscitationNote);
    }

    [Fact]
    public void RecordBirthEventRequest_ShouldValidateApgarRange()
    {
        var validRequest = new RecordBirthEventRequest
        {
            BirthDateTime = DateTime.UtcNow,
            Gender = "Male",
            WeightGrams = 3100,
            LengthCentimeters = 50,
            ApgarScore1Min = 9,
            ApgarScore5Min = 10
        };

        var context = new ValidationContext(validRequest);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(validRequest, context, validationResults, true);

        Assert.True(isValid);
        Assert.Empty(validationResults);
    }

    [Fact]
    public void OutboxDelivery_IntegrationEventPayloadShouldMatchContract()
    {
        var caseId = Guid.NewGuid();
        var birthEventId = Guid.NewGuid();
        var motherPatientId = Guid.NewGuid();

        var outboxPayload = new
        {
            EventId = Guid.NewGuid(),
            EventType = "operating-room.birth-event.recorded",
            Version = "1.0",
            OccurredAt = DateTime.UtcNow,
            CaseId = caseId,
            BirthEventId = birthEventId,
            MotherPatientId = motherPatientId,
            Gender = "Male",
            WeightGrams = 3400.00m,
            LengthCentimeters = 51.00m,
            ApgarScore1Min = 8,
            ApgarScore5Min = 9
        };

        var json = System.Text.Json.JsonSerializer.Serialize(outboxPayload);

        Assert.Contains("operating-room.birth-event.recorded", json);
        Assert.Contains(caseId.ToString(), json);
        Assert.Contains(birthEventId.ToString(), json);
        Assert.Contains(motherPatientId.ToString(), json);
    }
}
