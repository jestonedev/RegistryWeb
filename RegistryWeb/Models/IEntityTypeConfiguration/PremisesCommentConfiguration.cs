using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class PremisesCommentConfiguration : IEntityTypeConfiguration<PremisesComment>
    {
        private string nameDatebase;

        public PremisesCommentConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<PremisesComment> builder)
        {
            builder.HasKey(e => e.IdPremisesComment);

            builder.ToTable("premises_comments", nameDatebase);

            builder.Property(e => e.IdPremisesComment)
                .HasColumnName("id_premises_comment")
                .HasColumnType("int(11)");

            builder.Property(e => e.PremisesCommentText)
                .IsRequired()
                .HasColumnName("premises_comment_text")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
