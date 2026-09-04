using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class MstLabRejectionReasonConfiguration : IEntityTypeConfiguration<MstLabRejectionReason>
    {
        public void Configure(EntityTypeBuilder<MstLabRejectionReason> builder)
        {
            builder.ToTable("MstLabRejectionReason", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReasonCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ReasonName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);

            builder.HasIndex(x => x.ReasonCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
        }
    }
}
