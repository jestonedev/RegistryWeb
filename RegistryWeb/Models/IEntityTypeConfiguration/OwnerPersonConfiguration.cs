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
            builder.HasKey(e => e.IdOwnerPersons);

            builder.ToTable("owner_persons", nameDatebase);

            builder.HasIndex(e => e.IdOwnerProcess)
                .HasName("FK_owner_persons_owner_processes_id_process");

            builder.Property(e => e.IdOwnerPersons)
                .HasColumnName("id_owner_persons")
                .HasColumnType("int(11)");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.IdOwnerProcess)
                .HasColumnName("id_owner_process")
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

            builder.Property(e => e.NumeratorShare)
                .HasColumnName("numerator_share")
                .HasColumnType("int(11)");

            builder.Property(e => e.DenominatorShare)
                .HasColumnName("denominator_share")
                .HasColumnType("int(11)");

            builder.HasOne(d => d.IdOwnerProcessNavigation)
                .WithMany(p => p.OwnerPersons)
                .HasForeignKey(d => d.IdOwnerProcess)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_owner_persons_owner_processes_id_process");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
