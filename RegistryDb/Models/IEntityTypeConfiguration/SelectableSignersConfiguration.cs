using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class SelectableSignersConfiguration : IEntityTypeConfiguration<SelectableSigner>
    {
        private string nameDatebase;

        public SelectableSignersConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<SelectableSigner> builder)
        {
            builder.HasKey(e => e.IdRecord);

            builder.ToTable("selectable_signers", nameDatebase);

            builder.Property(e => e.IdRecord)
                .HasColumnName("id_record")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdSignerGroup)
                .HasColumnName("id_signer_group")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdOwner)
                .HasColumnName("id_owner")
                .HasColumnType("int(11)");

            builder.Property(e => e.Surname)
                .IsRequired()
                .HasColumnName("surname")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.Patronymic)
                .HasColumnName("patronymic")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Post)
                .IsRequired()
                .HasColumnName("post")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Phone)
                .HasColumnName("phone")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(e => e.PrivEstateOwner).WithMany(e => e.SelectableSigners)
                .HasForeignKey(e => e.IdOwner)
                .OnDelete(DeleteBehavior.Restrict);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
