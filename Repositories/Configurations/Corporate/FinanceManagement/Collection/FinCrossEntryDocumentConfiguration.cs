using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Collection;

public sealed class FinCrossEntryDocumentConfiguration : IEntityTypeConfiguration<FinCrossEntryDocument>
{
    public void Configure(EntityTypeBuilder<FinCrossEntryDocument> builder)
    {
        builder.ToTable("FinCrossEntryDocument", "public", t =>
        {
            t.HasCheckConstraint("CK_FinCrossEntryDocument_FileSize", "\"FileSize\" > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocumentName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.StoredFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.RelativePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.HasOne(x => x.CrossEntry)
            .WithMany(c => c.Documents)
            .HasForeignKey(x => x.CrossEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CrossEntryId, x.CreateDateTime })
            .HasDatabaseName("IX_FinCrossEntryDocument_CrossEntry_CreateDate");

        builder.HasIndex(x => x.StoredFileName)
            .IsUnique()
            .HasDatabaseName("IX_FinCrossEntryDocument_StoredFileName");
    }
}
