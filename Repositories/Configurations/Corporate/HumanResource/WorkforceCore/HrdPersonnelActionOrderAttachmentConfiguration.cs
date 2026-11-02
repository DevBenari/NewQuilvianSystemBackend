using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class HrdPersonnelActionOrderAttachmentConfiguration : IEntityTypeConfiguration<HrdPersonnelActionOrderAttachment>
    {
        public void Configure(EntityTypeBuilder<HrdPersonnelActionOrderAttachment> builder)
        {
            builder.ToTable("HrdPersonnelActionOrderAttachment", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            builder.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
            builder.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
            builder.Property(x => x.StoragePath).HasMaxLength(500).IsRequired();
            builder.Property(x => x.UploadedAt).HasColumnType("timestamp with time zone");

            builder.HasOne(x => x.PersonnelActionOrder)
                .WithMany(x => x.Attachments)
                .HasForeignKey(x => x.PersonnelActionOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.PersonnelActionOrderId);
        }
    }
}
