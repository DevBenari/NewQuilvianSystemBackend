using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.EmployeeRelationManagement
{
    public class TrxBipartiteNegotiationForumConfiguration : IEntityTypeConfiguration<TrxBipartiteNegotiationForum>
    {
        public void Configure(EntityTypeBuilder<TrxBipartiteNegotiationForum> entity)
        {
            entity.ToTable("trx_bipartite_negotiation_forums", "public");
            entity.HasKey(x => x.ForumId);

            entity.Property(x => x.ForumId).HasColumnName("forum_id");
            entity.Property(x => x.RegisterNumber).HasColumnName("register_number").HasMaxLength(64).IsRequired();
            entity.Property(x => x.MeetingDate).HasColumnName("meeting_date").HasColumnType("timestamp with time zone");
            entity.Property(x => x.DisputeType).HasColumnName("dispute_type").HasMaxLength(64).IsRequired();
            entity.Property(x => x.ManagementRepresentatives).HasColumnName("management_representatives").IsRequired();
            entity.Property(x => x.UnionRepresentatives).HasColumnName("union_representatives").IsRequired();
            entity.Property(x => x.AgendaDescription).HasColumnName("agenda_description").IsRequired();
            entity.Property(x => x.MinutesOfMeeting).HasColumnName("minutes_of_meeting").IsRequired();
            entity.Property(x => x.JointAgreementDocumentUrl).HasColumnName("joint_agreement_document_url").HasMaxLength(512);
            entity.Property(x => x.RegistrationDisnakerNumber).HasColumnName("registration_disnaker_number").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32).HasDefaultValue("Scheduled").IsRequired();

            entity.HasIndex(x => x.RegisterNumber).IsUnique();
            entity.HasIndex(x => x.DisputeType);
            entity.HasIndex(x => x.Status);
        }
    }
}
