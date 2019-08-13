using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;


namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class OwnerPersonConfiguration : IEntityTypeConfiguration<OwnerPerson>
    {
        private string nameDatebase;

        public OwnerPersonConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<OwnerPerson> builder)
        {
            builder.HasKey(e => e.IdPerson);

            builder.ToTable("owner_persons", nameDatebase);

            builder.HasIndex(e => e.IdProcess)
                .HasName("FK_owner_persons_owner_processes_id_process");

            builder.Property(e => e.IdPerson)
                .HasColumnName("id_person")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Patronymic)
                .IsRequired()
                .HasColumnName("patronymic")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Surname)
                .IsRequired()
                .HasColumnName("surname")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.HasOne(d => d.IdOwnerProcessNavigation)
                .WithMany(p => p.OwnerPersons)
                .HasForeignKey(d => d.IdProcess)
                .OnDelete(DeleteBehavior.ClientSetNull);

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
