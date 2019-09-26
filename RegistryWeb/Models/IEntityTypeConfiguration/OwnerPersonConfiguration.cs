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
            builder.HasKey(e => e.IdOwner);

            builder.ToTable("owner_persons", nameDatebase);

            builder.Property(e => e.IdOwner)
                .HasColumnName("id_owner")
                .HasColumnType("int(11)")
                .ValueGeneratedNever();

            builder.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Patronymic)
                .HasColumnName("patronymic")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Surname)
                .IsRequired()
                .HasColumnName("surname")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.HasOne(d => d.IdOwnerNavigation)
                .WithOne(p => p.OwnerPerson)
                .HasForeignKey<OwnerPerson>(d => d.IdOwner)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
