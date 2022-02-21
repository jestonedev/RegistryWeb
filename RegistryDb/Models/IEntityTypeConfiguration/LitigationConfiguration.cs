using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class LitigationConfiguration : IEntityTypeConfiguration<Litigation>
    {
        private string nameDatebase;

        public LitigationConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<Litigation> builder)
        {
            builder.HasKey(e => e.IdLitigation);

            builder.ToTable("litigation_info", nameDatebase);

            builder.HasIndex(e => e.IdLitigationType)
                .HasName("FK_litigation_info_id_litigation_type");

            builder.Property(e => e.IdLitigation)
                .HasColumnName("id_litigation")
                .HasColumnType("int(11)");

            builder.Property(e => e.Date)
                .HasColumnName("date")
                .HasColumnType("date");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.IdLitigationType)
                .HasColumnName("id_litigation_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.Number)
                .HasColumnName("number")
                .HasMaxLength(10)
                .IsUnicode(false);

            builder.Property(e => e.FileOriginName)
                .HasColumnName("file_origin_name")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.FileDisplayName)
                .HasColumnName("file_display_name")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.FileMimeType)
                .HasColumnName("file_mime_type")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.HasOne(d => d.LitigationTypeNavigation)
                .WithMany(p => p.Litigations)
                .HasForeignKey(d => d.IdLitigationType)
                .OnDelete(DeleteBehavior.ClientSetNull);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
