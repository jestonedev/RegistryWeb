using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class TenancyPersonConfiguration : IEntityTypeConfiguration<TenancyPerson>
    {
        private string nameDatebase;

        public TenancyPersonConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<TenancyPerson> builder)
        {
            builder.HasKey(e => e.IdPerson);

            builder.ToTable("tenancy_persons", nameDatebase);

            builder.HasIndex(e => e.IdDocumentIssuedBy)
                .HasName("FK_persons_document_issued_by_id_document_issued_by");

            builder.HasIndex(e => e.IdDocumentType)
                .HasName("FK_persons_document_types_id_document_type");

            builder.HasIndex(e => e.IdKinship)
                .HasName("FK_persons_kinships_id_kinship");

            builder.HasIndex(e => e.IdProcess)
                .HasName("IDX_tenancy_persons_id_process");

            builder.Property(e => e.IdPerson)
                .HasColumnName("id_person")
                .HasColumnType("int(11)");

            builder.Property(e => e.DateOfBirth)
                .HasColumnName("date_of_birth")
                .HasColumnType("date");

            builder.Property(e => e.DateOfDocumentIssue)
                .HasColumnName("date_of_document_issue")
                .HasColumnType("date");

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.Property(e => e.DocumentNum)
                .HasColumnName("document_num")
                .HasMaxLength(8)
                .IsUnicode(false);

            builder.Property(e => e.DocumentSeria)
                .HasColumnName("document_seria")
                .HasMaxLength(8)
                .IsUnicode(false);

            builder.Property(e => e.ExcludeDate)
                .HasColumnName("exclude_date")
                .HasColumnType("date");

            builder.Property(e => e.IdDocumentIssuedBy)
                .HasColumnName("id_document_issued_by")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdDocumentType)
                .HasColumnName("id_document_type")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdKinship)
                .HasColumnName("id_kinship")
                .HasColumnType("int(11)");

            builder.Property(e => e.IdProcess)
                .HasColumnName("id_process")
                .HasColumnType("int(11)");

            builder.Property(e => e.IncludeDate)
                .HasColumnName("include_date")
                .HasColumnType("date");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.Patronymic)
                .HasColumnName("patronymic")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.PersonalAccount)
                .HasColumnName("personal_account")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Comment)
                .HasColumnName("comment")
                .HasMaxLength(512)
                .IsUnicode(false);

            builder.Property(e => e.RegistrationDate)
                .HasColumnName("registration_date")
                .HasColumnType("date");

            builder.Property(e => e.RegistrationFlat)
                .HasColumnName("registration_flat")
                .HasMaxLength(15)
                .IsUnicode(false);

            builder.Property(e => e.RegistrationHouse)
                .HasColumnName("registration_house")
                .HasMaxLength(10)
                .IsUnicode(false);

            builder.Property(e => e.RegistrationIdStreet)
                .HasColumnName("registration_id_street")
                .HasMaxLength(17)
                .IsUnicode(false);

            builder.Property(e => e.RegistrationRoom)
                .HasColumnName("registration_room")
                .HasMaxLength(15)
                .IsUnicode(false);

            builder.Property(e => e.ResidenceFlat)
                .HasColumnName("residence_flat")
                .HasMaxLength(15)
                .IsUnicode(false);

            builder.Property(e => e.ResidenceHouse)
                .HasColumnName("residence_house")
                .HasMaxLength(10)
                .IsUnicode(false);

            builder.Property(e => e.ResidenceIdStreet)
                .HasColumnName("residence_id_street")
                .HasMaxLength(17)
                .IsUnicode(false);

            builder.Property(e => e.ResidenceRoom)
                .HasColumnName("residence_room")
                .HasMaxLength(15)
                .IsUnicode(false);

            builder.Property(e => e.Snils)
                .HasColumnName("snils")
                .HasMaxLength(14)
                .IsUnicode(false);

            builder.Property(e => e.Surname)
                .IsRequired()
                .HasColumnName("surname")
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.HasOne(d => d.IdProcessNavigation)
                .WithMany(p => p.TenancyPersons)
                .HasForeignKey(d => d.IdProcess)
                .HasConstraintName("FK_tsta_id_agreement");

            //Фильтры по умолчанию
            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
