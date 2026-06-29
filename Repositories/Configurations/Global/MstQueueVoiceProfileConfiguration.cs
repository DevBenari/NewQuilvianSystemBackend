using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class MstQueueVoiceProfileConfiguration : IEntityTypeConfiguration<MstQueueVoiceProfile>
    {
        public void Configure(EntityTypeBuilder<MstQueueVoiceProfile> entity)
        {
            entity.ToTable("MstQueueVoiceProfile", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.VoiceCode).HasMaxLength(80).IsRequired();
            entity.Property(x => x.VoiceName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Gender).HasMaxLength(20).HasDefaultValue("Female");
            entity.Property(x => x.Language).HasMaxLength(20).HasDefaultValue("id-ID");
            entity.Property(x => x.ModelPath).HasMaxLength(500).IsRequired();
            entity.Property(x => x.LengthScale).HasPrecision(5, 2).HasDefaultValue(1.08m);
            entity.Property(x => x.NoiseScale).HasPrecision(5, 2).HasDefaultValue(0.65m);
            entity.Property(x => x.NoiseW).HasPrecision(5, 2).HasDefaultValue(0.80m);
            entity.Property(x => x.Volume).HasPrecision(5, 2).HasDefaultValue(1.15m);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.Description).HasMaxLength(250);

            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CreateBy).HasDefaultValue(Guid.Empty);
            entity.Property(x => x.UpdateBy).HasDefaultValue(Guid.Empty);
            entity.Property(x => x.DeleteBy).HasDefaultValue(Guid.Empty);
            entity.Property(x => x.CancelBy).HasDefaultValue(Guid.Empty);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasIndex(x => x.VoiceCode).IsUnique();
            entity.HasIndex(x => new { x.IsActive, x.IsDelete, x.SortOrder });
            entity.HasIndex(x => new { x.IsDefault, x.IsActive, x.IsDelete });
        }
    }

}
