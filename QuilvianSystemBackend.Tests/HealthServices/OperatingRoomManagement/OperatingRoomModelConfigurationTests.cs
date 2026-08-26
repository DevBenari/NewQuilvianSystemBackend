using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.OperatingRoomManagement;

public class OperatingRoomModelConfigurationTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"operating-room-model-{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public void AllOperationalEntities_AreMappedWithCanonicalNamesAndIdentityAudit()
    {
        using var context = CreateContext();
        var expected = new[]
        {
            typeof(OprCase), typeof(OprCaseProcedure), typeof(OprSchedule),
            typeof(OprTeamMember), typeof(OprSafetyChecklist), typeof(OprExecutionRecord),
            typeof(OprExecutionAddendum), typeof(OprAnesthesiaRecord), typeof(OprMaterialUsage),
            typeof(OprRecovery), typeof(OprHandover), typeof(OprStatusHistory),
            typeof(OprIntegrationDelivery)
        };

        foreach (var type in expected)
        {
            var entity = context.Model.FindEntityType(type);
            Assert.NotNull(entity);
            Assert.Equal(type.Name, entity!.GetTableName());
            Assert.Equal("public", entity.GetSchema());
            Assert.True(typeof(IdentityModel).IsAssignableFrom(type));
        }
    }

    [Fact]
    public void ClinicalRelationships_UseRestrictDeleteBehavior()
    {
        using var context = CreateContext();
        var operationalForeignKeys = context.Model.GetEntityTypes()
            .Where(x => x.ClrType.Namespace?.Contains("OperatingRoomManagement") == true)
            .SelectMany(x => x.GetForeignKeys())
            .ToList();

        Assert.NotEmpty(operationalForeignKeys);
        Assert.All(operationalForeignKeys, foreignKey =>
            Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior));
    }

    [Fact]
    public void CaseAndEditableClinicalRecords_HaveConcurrencyTokens()
    {
        using var context = CreateContext();
        var expected = new[]
        {
            typeof(OprCase), typeof(OprExecutionRecord), typeof(OprAnesthesiaRecord),
            typeof(OprRecovery)
        };

        foreach (var type in expected)
        {
            var version = context.Model.FindEntityType(type)!.FindProperty("Version");
            Assert.NotNull(version);
            Assert.True(version!.IsConcurrencyToken);
        }
    }

    [Fact]
    public void BusinessKeys_HaveUniqueIndexes()
    {
        using var context = CreateContext();

        Assert.Contains(context.Model.FindEntityType(typeof(OprCase))!.GetIndexes(),
            index => index.IsUnique && index.Properties.Select(x => x.Name).SequenceEqual(["CaseNumber"]));
        Assert.Contains(context.Model.FindEntityType(typeof(OprCaseProcedure))!.GetIndexes(),
            index => index.IsUnique && index.Properties.Select(x => x.Name).SequenceEqual(["PatientProcedureId"]));
        Assert.Contains(context.Model.FindEntityType(typeof(OprIntegrationDelivery))!.GetIndexes(),
            index => index.IsUnique && index.Properties.Select(x => x.Name)
                .SequenceEqual(["Destination", "IdempotencyKey"]));
    }
}
