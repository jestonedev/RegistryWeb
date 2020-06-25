using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class ActTypeDocumentConfiguration: IEntityTypeConfiguration<ActTypeDocument>
    {
        private string nameDatebase;

        public ActTypeDocumentConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ActTypeDocument> builder)
        {
            builder.HasKey(e => e.Id);

            builder.ToTable("act_type_document", "registry_test");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int(11)");

            builder.Property(e => e.ActFileType)
                .IsRequired()
                .HasColumnName("act_file_type")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsUnicode(false);


        }
    }
}
