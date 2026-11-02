using System;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using Xunit;

namespace QuilvianSystemBackend.Tests.OperatingRoom;

public class InpatientBedHoldTests
{
    [Fact]
    public void InpEpisode_ShouldStoreSurgicalBookingAndBedHoldFields()
    {
        var episode = new InpEpisode
        {
            Id = Guid.NewGuid(),
            EpisodeNumber = "RWI/2026/10/0001",
            SurgicalBookingNumber = "BKG-20261024-001",
            ClinicalSubCategory = "ObstetricSurgery",
            BedHoldStatus = "InSurgery",
            IsRoomingInBabyPlanned = true,
            MaternityPackageId = Guid.NewGuid(),
        };

        Assert.Equal("BKG-20261024-001", episode.SurgicalBookingNumber);
        Assert.Equal("ObstetricSurgery", episode.ClinicalSubCategory);
        Assert.Equal("InSurgery", episode.BedHoldStatus);
        Assert.True(episode.IsRoomingInBabyPlanned);
        Assert.NotNull(episode.MaternityPackageId);
    }

    [Fact]
    public void UpdateBedHoldStatusRequest_DefaultsToInSurgery()
    {
        var req = new UpdateBedHoldStatusRequest();
        Assert.Equal("InSurgery", req.BedHoldStatus);
    }

    [Fact]
    public void TriDoctorAssignments_ShouldAssignSpAnAndSpAAsConsultants()
    {
        var dpjp = new InpDoctorAssignment
        {
            Id = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            AssignmentRole = InpDoctorAssignmentRole.Dpjp,
            SequenceNumber = 1
        };

        var spAn = new InpDoctorAssignment
        {
            Id = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            AssignmentRole = InpDoctorAssignmentRole.Consultant,
            SequenceNumber = 2
        };

        var spA = new InpDoctorAssignment
        {
            Id = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            AssignmentRole = InpDoctorAssignmentRole.Consultant,
            SequenceNumber = 3
        };

        Assert.Equal(InpDoctorAssignmentRole.Dpjp, dpjp.AssignmentRole);
        Assert.Equal(InpDoctorAssignmentRole.Consultant, spAn.AssignmentRole);
        Assert.Equal(InpDoctorAssignmentRole.Consultant, spA.AssignmentRole);
        Assert.True(dpjp.SequenceNumber < spAn.SequenceNumber);
        Assert.True(spAn.SequenceNumber < spA.SequenceNumber);
    }
}
