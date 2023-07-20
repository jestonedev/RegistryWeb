using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class KumiMemorialOrderConfiguration : IEntityTypeConfiguration<KumiMemorialOrder>
    {
        private string nameDatebase;

        public KumiMemorialOrderConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<KumiMemorialOrder> builder)
        {
            builder.HasKey(e => e.IdOrder);

            builder.ToTable("kumi_memorial_orders", nameDatebase);

            builder.Property(e => e.IdOrder)
                .HasColumnName("id_order")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.IdGroup)
                .HasColumnName("id_group")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.Guid)
                .HasColumnName("guid")
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(e => e.NumDocument)
                .HasColumnName("num_d")
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(e => e.DateDocument)
                .HasColumnName("doc_d")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(e => e.SumIn)
                .HasColumnName("sum_in")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.SumZach)
                .HasColumnName("sum_zach")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(e => e.Kbk)
                .HasColumnName("kbk")
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(e => e.IdKbkType)
                .HasColumnName("id_kbk_type")
                .HasColumnType("int(11)")
                .IsRequired();

            builder.Property(e => e.TargetCode)
                .HasColumnName("target_code")
                .HasMaxLength(25)
                .IsUnicode(false);

            builder.Property(e => e.Okato)
                .HasColumnName("okato")
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(e => e.InnAdb)
               .HasColumnName("inn_adb")
               .HasMaxLength(12)
               .IsUnicode(false)
               .IsRequired();

            builder.Property(e => e.KppAdb)
               .HasColumnName("kpp_adb")
               .HasMaxLength(12)
               .IsUnicode(false)
               .IsRequired();

            builder.Property(e => e.DateEnrollUfk)
                .HasColumnName("date_enroll_ufk")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(e => e.Deleted)
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .IsRequired();

            builder.HasOne(e => e.KbkType)
                .WithMany(e => e.MemorialOrders)
                .HasForeignKey(e => e.IdKbkType);

            builder.HasOne(e => e.PaymentGroup)
                .WithMany(e => e.MemorialOrders)
                .HasForeignKey(e => e.IdGroup);

            builder.HasQueryFilter(r => r.Deleted == 0);
        }
    }
}
