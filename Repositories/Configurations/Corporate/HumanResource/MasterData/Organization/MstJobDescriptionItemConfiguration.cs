using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Organization
{
    public class MstJobDescriptionItemConfiguration : IEntityTypeConfiguration<MstJobDescriptionItem>
    {
        public void Configure(EntityTypeBuilder<MstJobDescriptionItem> entity)
        {
            entity.ToTable("MstJobDescriptionItem", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.JobDescriptionId)
                .IsRequired();

            entity.Property(x => x.SectionType)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Sequence)
                .IsRequired();

            entity.Property(x => x.Content)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasIndex(x => new { x.JobDescriptionId, x.SectionType, x.Sequence });
        }
    }
}
