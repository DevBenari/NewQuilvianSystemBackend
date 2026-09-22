using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabDisciplineSettingConfiguration : IEntityTypeConfiguration<LabDisciplineSetting>
    {
        public void Configure(EntityTypeBuilder<LabDisciplineSetting> builder)
        {
            builder.ToTable("LabDisciplineSetting", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Discipline).HasConversion<int>().IsRequired();
            builder.Property(x => x.ConsultantLabel).IsRequired().HasMaxLength(150);
            builder.Property(x => x.ConsultantName).HasMaxLength(200);
            builder.Property(x => x.StandingNote).HasMaxLength(1000);
            builder.Property(x => x.ReportNumberPrefix).HasMaxLength(20);
            builder.Property(x => x.IsActive).IsRequired();

            // Satu disiplin punya paling banyak SATU pengaturan. Parsial atas IsDelete, sebab
            // penghapusan di aplikasi ini berupa penandaan — index unik penuh membuat disiplin
            // yang pengaturannya pernah dihapus nol dapat diatur ulang.
            builder.HasIndex(x => x.Discipline)
                .IsUnique()
                .HasDatabaseName("IX_LabDisciplineSetting_Discipline")
                .HasFilter("\"IsDelete\" = false");

            // Nol foreign key. Disiplin adalah enum, bukan baris tabel.
        }
    }
}
