using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalRecordManagement
{
    public class MstMedicalRecordCompletenessProfileItemConfiguration
        : IEntityTypeConfiguration<MstMedicalRecordCompletenessProfileItem>
    {
        public void Configure(EntityTypeBuilder<MstMedicalRecordCompletenessProfileItem> builder)
        {
            builder.ToTable("MstMedicalRecordCompletenessProfileItem", "public");

            builder.HasKey(x => x.Id);

            // VAL-RM-A07: kriteria yang sama tidak boleh dua kali dalam satu profil.
            builder.HasIndex(x => new { x.ProfileId, x.Criterion })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
        }
    }
}
