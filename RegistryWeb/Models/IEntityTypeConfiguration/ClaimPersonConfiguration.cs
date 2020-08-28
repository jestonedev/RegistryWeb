using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Models.IEntityTypeConfiguration
{
    public class ClaimPersonConfiguration : IEntityTypeConfiguration<ClaimPerson>
    {
        private string nameDatebase;

        public ClaimPersonConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<ClaimPerson> builder)
        {
            builder.HasKey(e => e.IdPerson);

            builder.ToTable("claim_persons", nameDatebase);

            builder.Property(e => e.IdPerson)
                .HasColumnName("id_person")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdClaim)
                .HasColumnName("id_claim")
                .HasColumnType("int(11)");

            builder.Property(e => e.Surname)
                .HasColumnName("surname")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.Patronymic)
                .HasColumnName("patronymic")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.DateOfBirth)
                .HasColumnName("date_of_birth")
                .HasColumnType("date");

            builder.Property(e => e.PlaceOfBirth)
                .HasColumnName("place_of_birth")
                .HasMaxLength(1024)
                .IsUnicode(false);

            builder.Property(e => e.WorkPlace)
                .HasColumnName("work_place")
                .HasMaxLength(1024)
                .IsUnicode(false);

            builder.Property(e => e.IsClaimer)
                .HasColumnName("is_claimer")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasOne(e => e.IdClaimNavigation)
                .WithMany(e => e.ClaimPersons)
                .HasForeignKey(d => d.IdClaim)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_claims_persons_id_claim");

            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
