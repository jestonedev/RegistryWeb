using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class ChangeLogConfiguration : IEntityTypeConfiguration<ChangeLog>
    {
        private string nameDatebase;

        public ChangeLogConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ChangeLog> builder)
        {
            builder.HasKey(e => e.IdRecord);

            builder.ToTable("log", nameDatebase);

            builder.HasIndex(e => e.OperationTime)
                .HasName("date_index");

            builder.Property(e => e.IdRecord)
                .HasColumnName("id_record")
                .HasColumnType("int(11)");

            builder.Property(e => e.FieldName)
                .IsRequired()
                .HasColumnName("field_name")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.FieldNewValue)
                .HasColumnName("field_new_value")
                .IsUnicode(false);

            builder.Property(e => e.FieldOldValue)
                .HasColumnName("field_old_value")
                .IsUnicode(false);

            builder.Property(e => e.IdKey)
                .HasColumnName("id_key")
                .HasColumnType("int(11)");

            builder.Property(e => e.OperationTime).HasColumnName("operation_time");

            builder.Property(e => e.OperationType)
                .IsRequired()
                .HasColumnName("operation_type")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.TableName)
                .IsRequired()
                .HasColumnName("table")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.UserName)
                .IsRequired()
                .HasColumnName("user_name")
                .HasMaxLength(255)
                .IsUnicode(false);
        }
    }
}
