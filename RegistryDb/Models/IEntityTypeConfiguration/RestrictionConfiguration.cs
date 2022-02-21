using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class RestrictionConfiguration: IEntityTypeConfiguration<Restriction>
    {
        private string nameDatebase;

        public RestrictionConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<Restriction> builder)
        {
            builder.HasKey(e => e.IdRestriction);

            builder.ToTable("restrictions", nameDatebase);

            builder.HasIndex(e => e.IdRestrictionType)
                .HasName("FK_restrictions_restriction_types_id_restriction_type");

            builder.Property(e => e.IdRestriction)
                .HasColumnName("id_restriction")
                .HasColumnType("int(11)");

            builder.Property(e => e.Date)
                .HasColumnName("date")
                .HasColumnType("date");

            builder.Property(e => e.DateStateReg)
                .HasColumnName("date_state_reg")
                .HasColumnType("date");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.IdRestrictionType)
                .HasColumnName("id_restriction_type")
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

            builder.HasOne(d => d.RestrictionTypeNavigation)
                .WithMany(p => p.Restrictions)
                .HasForeignKey(d => d.IdRestrictionType)
                .OnDelete(DeleteBehavior.ClientSetNull);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
