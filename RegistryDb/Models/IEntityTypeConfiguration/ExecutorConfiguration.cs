using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistryDb.Models.Entities;

namespace RegistryDb.Models.IEntityTypeConfiguration
{
    public class ExecutorConfiguration : IEntityTypeConfiguration<Executor>
    {
        private string nameDatebase;

        public ExecutorConfiguration(string nameDatebase)
        {
            this.nameDatebase = nameDatebase;
        }

        public void Configure(EntityTypeBuilder<Executor> builder)
        {
            builder.HasKey(e => e.IdExecutor);

            builder.ToTable("executors", nameDatebase);

            builder.Property(e => e.IdExecutor)
                .HasColumnName("id_executor")
                .HasColumnType("int(11)");

            builder.Property(e => e.ExecutorName)
                .IsRequired()
                .HasColumnName("executor_name")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.ExecutorLogin)
                .HasColumnName("executor_login")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.Phone)
                .HasColumnName("phone")
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(e => e.IsInactive)
                .IsRequired()
                .HasColumnName("is_inactive")
                .HasColumnType("tinyint(1)");

            builder.Property(e => e.Deleted)
                .IsRequired()
                .HasColumnName("deleted")
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0");

            builder.HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
