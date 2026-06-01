using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> entity)
        {
            entity.Property(x => x.Description)
                        .HasMaxLength(250);

            entity.Property(x => x.IsSystemRole)
                .HasDefaultValue(false);

            entity.Property(x => x.CreateDateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
